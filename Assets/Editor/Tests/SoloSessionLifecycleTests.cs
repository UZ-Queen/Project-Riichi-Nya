using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class SoloSessionLifecycleTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject createdObject in createdObjects)
        {
            UnityEngine.Object.DestroyImmediate(createdObject);
        }

        createdObjects.Clear();
        SetStaticProperty(typeof(MahjongGameManager), "Instance", null);
        SetStaticProperty(typeof(GameUIManager), "Instance", null);
    }

    [Test]
    public void ConfirmForfeit_FinalizesOnceWithoutSavingHighScore()
    {
        AssertPlayerHandRenderingBoundary();
        Type controllerType = AssertPlayerHandInputBoundary();

        MethodInfo confirmForfeit = GetMethod(typeof(MahjongGameManager), "ConfirmForfeit");
        FieldInfo lastEndReason = GetField(typeof(MahjongGameManager), "lastEndReason");

        Assert.That(confirmForfeit, Is.Not.Null, "Forfeit must wait for an explicit confirmation.");
        Assert.That(lastEndReason, Is.Not.Null, "The finalizer must retain the observed end reason.");

        string savePath = Path.Combine(Application.persistentDataPath, "yaml.json");
        byte[] originalSave = File.Exists(savePath) ? File.ReadAllBytes(savePath) : null;

        try
        {
            SettingsManager.Save(new PetitGameSaveData { highScore = 4321f });
            byte[] expectedSave = File.ReadAllBytes(savePath);
            Component controller = CreateInactiveObject("PlayerHandController").AddComponent(controllerType);
            MahjongGameManager manager = CreateManager(controller);
            int gameOverCount = 0;
            manager.OnGameOver += () => gameOverCount++;

            manager.currentState = GameState.PlayerTurn;
            RaiseEvent(controller, "ForfeitRequested");
            Assert.That(manager.currentState, Is.EqualTo(GameState.Processing));
            Assert.That(GetFieldValue<bool>(manager, "pendingForfeit"), Is.True);

            RaiseEvent(controller, "ForfeitRequested");
            Assert.That(manager.currentState, Is.EqualTo(GameState.PlayerTurn));
            Assert.That(GetFieldValue<bool>(manager, "pendingForfeit"), Is.False);

            RaiseEvent(controller, "ForfeitRequested");
            confirmForfeit.Invoke(manager, null);
            confirmForfeit.Invoke(manager, null);

            Assert.That(lastEndReason.GetValue(manager).ToString(), Is.EqualTo("Forfeit"));
            Assert.That(gameOverCount, Is.EqualTo(1));
            Assert.That(File.ReadAllBytes(savePath), Is.EqualTo(expectedSave));
        }
        finally
        {
            if (originalSave == null)
            {
                File.Delete(savePath);
            }
            else
            {
                File.WriteAllBytes(savePath, originalSave);
            }
        }
    }

    [Test]
    public void StartNewGame_Twice_DetachesAndResetsSession()
    {
        AssertPlayerHandRenderingBoundary();
        AssertPlayerHandInputBoundary();

        Assert.That(GetField(typeof(MahjongGameManager), "pendingForfeit"), Is.Not.Null);
        Assert.That(GetField(typeof(MahjongGameManager), "sessionFinalized"), Is.Not.Null);

        MahjongGameManager manager = CreateManager();

        manager.StartNewGame();
        MahjongRound firstRound = GetFieldValue<MahjongRound>(manager, "currentRound");
        manager.StartNewGame();
        MahjongRound secondRound = GetFieldValue<MahjongRound>(manager, "currentRound");

        Assert.That(secondRound, Is.Not.SameAs(firstRound));
        Assert.That(CountTargetHandlers(firstRound, manager), Is.EqualTo(0));
        Assert.That(CountTargetHandlers(secondRound, manager), Is.EqualTo(6));
        Assert.That(CountTargetHandlers(GetFieldValue<Timer>(manager, "redstoneClock"), manager), Is.EqualTo(1));
        Assert.That(manager.currentState, Is.EqualTo(GameState.PlayerTurn));
        Assert.That(GetFieldValue<bool>(manager, "pendingForfeit"), Is.False);
        Assert.That(GetFieldValue<bool>(manager, "sessionFinalized"), Is.False);
    }

    private MahjongGameManager CreateManager(Component playerHandController = null)
    {
        GameObject uiObject = CreateInactiveObject("GameUIManager");
        GameUIManager gameUIManager = uiObject.AddComponent<GameUIManager>();
        FieldInfo panels = GetField(typeof(GameUIManager), "panels");
        panels.SetValue(gameUIManager, Activator.CreateInstance(panels.FieldType));
        SetField(gameUIManager, "gameCanvas", CreateObject("GameCanvas"));
        uiObject.SetActive(true);

        GameObject managerObject = CreateInactiveObject("MahjongGameManager");
        MahjongGameManager manager = managerObject.AddComponent<MahjongGameManager>();
        ScoreManagerDistance score = managerObject.AddComponent<ScoreManagerDistance>();
        Timer timer = managerObject.AddComponent<Timer>();
        SetField(manager, "scoreManagerDistance", score);
        SetField(manager, "redstoneClock", timer);
        if (playerHandController != null)
        {
            SetField(manager, "playerHand", playerHandController);
        }

        manager.Construct(score);
        score.Initialize();
        managerObject.SetActive(true);
        return manager;
    }

    private static void AssertPlayerHandRenderingBoundary()
    {
        Assembly runtimeAssembly = typeof(MahjongGameManager).Assembly;
        Type controllerType = runtimeAssembly.GetType("PlayerHandController") ?? runtimeAssembly.GetType("PlayerHand");
        Type viewType = runtimeAssembly.GetType("PlayerHandView");

        Assert.That(viewType, Is.Not.Null, "PlayerHandView must own hand rendering.");
        Assert.That(controllerType, Is.Not.Null, "The hand input owner must exist.");
        Assert.That(GetField(controllerType, "playerHandView"), Is.Not.Null,
            "PlayerHand must delegate presentation to one serialized PlayerHandView.");
        Assert.That(GetField(controllerType, "tilePrefap"), Is.Null,
            "PlayerHand must not retain the tile prefab after rendering extraction.");
        Assert.That(GetMethod(viewType, "FillHand"), Is.Not.Null);
        Assert.That(GetMethod(viewType, "TsumoTile"), Is.Not.Null);
        Assert.That(GetMethod(viewType, "UpdateSelectedIndex"), Is.Not.Null);
    }

    private static Type AssertPlayerHandInputBoundary()
    {
        Type controllerType = typeof(MahjongGameManager).Assembly.GetType("PlayerHandController");

        Assert.That(controllerType, Is.Not.Null, "The input owner must be named PlayerHandController.");
        Assert.That(GetField(typeof(MahjongGameManager), "playerHand").FieldType, Is.EqualTo(controllerType));
        Assert.That(controllerType.GetEvent("ForfeitRequested"), Is.Not.Null,
            "Forfeit must use a separate session-intent event.");
        Assert.That(Enum.GetNames(typeof(PlayerCallType)), Does.Not.Contain("Forfeit"),
            "PlayerCallType must contain mahjong actions only.");
        return controllerType;
    }

    private static void RaiseEvent(Component source, string eventName)
    {
        Delegate handlers = GetField(source.GetType(), eventName).GetValue(source) as Delegate;
        Assert.That(handlers, Is.Not.Null, $"{eventName} must have an invokable backing delegate.");
        handlers.DynamicInvoke();
    }

    private GameObject CreateInactiveObject(string name)
    {
        GameObject createdObject = CreateObject(name);
        createdObject.SetActive(false);
        return createdObject;
    }

    private GameObject CreateObject(string name)
    {
        GameObject createdObject = new GameObject(name);
        createdObjects.Add(createdObject);
        return createdObject;
    }

    private static int CountTargetHandlers(object source, object target)
    {
        int count = 0;
        foreach (FieldInfo field in source.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (!typeof(Delegate).IsAssignableFrom(field.FieldType))
            {
                continue;
            }

            Delegate value = field.GetValue(source) as Delegate;
            if (value == null)
            {
                continue;
            }

            foreach (Delegate handler in value.GetInvocationList())
            {
                if (ReferenceEquals(handler.Target, target))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static MethodInfo GetMethod(Type type, string methodName)
    {
        return type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    }

    private static FieldInfo GetField(Type type, string fieldName)
    {
        return type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    }

    private static void Invoke(object target, string methodName, params object[] arguments)
    {
        GetMethod(target.GetType(), methodName).Invoke(target, arguments);
    }

    private static T GetFieldValue<T>(object target, string fieldName)
    {
        return (T)GetField(target.GetType(), fieldName).GetValue(target);
    }

    private static void SetField(object target, string fieldName, object value)
    {
        GetField(target.GetType(), fieldName).SetValue(target, value);
    }

    private static void SetStaticProperty(Type type, string propertyName, object value)
    {
        type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public).SetValue(null, value);
    }
}
