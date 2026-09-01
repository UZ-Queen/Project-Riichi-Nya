using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        SetStaticProperty(typeof(SoloScoringGameManager), "Instance", null);
        Type soloUiType = typeof(SoloScoringGameManager).Assembly.GetType("SoloScoringUIController");
        if (soloUiType != null)
        {
            SetStaticProperty(soloUiType, "Instance", null);
        }
    }

    [Test]
    public void ConfirmForfeit_FinalizesOnceWithoutSavingHighScore()
    {
        AssertSoloManagerRenameBoundary();
        AssertSoloModeRootSceneContract();
        AssertSoloUiRenameBoundary();
        AssertSoloUiOwnershipBoundary();
        AssertForfeitOverlaySceneContract();
        AssertPlayerHandRenderingBoundary();
        Type controllerType = AssertPlayerHandInputBoundary();

        MethodInfo confirmForfeit = GetMethod(typeof(SoloScoringGameManager), "ConfirmForfeit");
        FieldInfo lastEndReason = GetField(typeof(SoloScoringGameManager), "lastEndReason");

        Assert.That(confirmForfeit, Is.Not.Null, "Forfeit must wait for an explicit confirmation.");
        Assert.That(lastEndReason, Is.Not.Null, "The finalizer must retain the observed end reason.");

        string savePath = Path.Combine(Application.persistentDataPath, "yaml.json");
        byte[] originalSave = File.Exists(savePath) ? File.ReadAllBytes(savePath) : null;

        try
        {
            SettingsManager.Save(new PetitGameSaveData { highScore = 4321f });
            byte[] expectedSave = File.ReadAllBytes(savePath);
            Component controller = CreateInactiveObject("PlayerHandController").AddComponent(controllerType);
            SoloScoringGameManager manager = CreateManager(controller);
            int gameOverCount = 0;
            manager.OnGameOver += () => gameOverCount++;

            manager.currentState = GameState.PlayerTurn;
            Assert.That(CountTargetHandlers(controller, manager), Is.EqualTo(3),
                "The manager must subscribe once to discard, call, and forfeit intents.");
            RaiseEvent(controller, "ForfeitRequested");
            Assert.That(manager.currentState, Is.EqualTo(GameState.Processing));
            Assert.That(GetFieldValue<bool>(manager, "pendingForfeit"), Is.True);
            Assert.That(GetFieldValue<bool>(controller, "gameplayInputEnabled"), Is.False,
                "Gameplay input must be blocked synchronously before overlay animation can complete.");
            Component soloUiController = GetFieldValue<Component>(manager, "soloUIController");
            Assert.That(CountTargetHandlers(soloUiController, manager), Is.EqualTo(2),
                "The manager must consume one confirm and one cancel event route.");

            GetFieldValue<Button>(soloUiController, "cancelButton").onClick.Invoke();
            Assert.That(manager.currentState, Is.EqualTo(GameState.PlayerTurn));
            Assert.That(GetFieldValue<bool>(manager, "pendingForfeit"), Is.False);

            RaiseEvent(controller, "ForfeitRequested");
            GetFieldValue<Button>(soloUiController, "confirmButton").onClick.Invoke();
            GetFieldValue<Button>(soloUiController, "confirmButton").onClick.Invoke();

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
        AssertSoloManagerRenameBoundary();
        AssertSoloModeRootSceneContract();
        AssertSoloUiRenameBoundary();
        AssertSoloUiOwnershipBoundary();
        AssertForfeitOverlaySceneContract();
        AssertPlayerHandRenderingBoundary();
        Type controllerType = AssertPlayerHandInputBoundary();

        Assert.That(GetField(typeof(SoloScoringGameManager), "pendingForfeit"), Is.Not.Null);
        Assert.That(GetField(typeof(SoloScoringGameManager), "sessionFinalized"), Is.Not.Null);

        Component controller = CreateInactiveObject("PlayerHandController").AddComponent(controllerType);
        SoloScoringGameManager manager = CreateManager(controller);

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

        Component soloUiController = GetFieldValue<Component>(manager, "soloUIController");
        Invoke(manager, "OnDisable");
        Assert.That(CountTargetHandlers(controller, manager), Is.EqualTo(0));
        Assert.That(CountTargetHandlers(soloUiController, manager), Is.EqualTo(0));
        Assert.That(CountTargetHandlers(secondRound, manager), Is.EqualTo(0));
        Assert.That(CountTargetHandlers(GetFieldValue<Timer>(manager, "redstoneClock"), manager), Is.EqualTo(0));

        Invoke(manager, "OnEnable");
        manager.StartNewGame();
        Assert.That(CountTargetHandlers(controller, manager), Is.EqualTo(3));
        Assert.That(CountTargetHandlers(soloUiController, manager), Is.EqualTo(2));
        Assert.That(CountTargetHandlers(GetFieldValue<MahjongRound>(manager, "currentRound"), manager), Is.EqualTo(6));
        Assert.That(CountTargetHandlers(GetFieldValue<Timer>(manager, "redstoneClock"), manager), Is.EqualTo(1));
    }

    private SoloScoringGameManager CreateManager(Component playerHandController = null)
    {
        Type soloUiType = typeof(SoloScoringGameManager).Assembly.GetType("SoloScoringUIController");
        GameObject uiObject = CreateInactiveObject("SoloScoringUIController");
        Component gameUIManager = uiObject.AddComponent(soloUiType);
        FieldInfo panels = GetField(soloUiType, "panels");
        panels.SetValue(gameUIManager, Activator.CreateInstance(panels.FieldType));
        SetField(gameUIManager, "gameCanvas", CreateObject("GameCanvas"));
        GameObject overlay = CreateUiObject("ForfeitConfirmation", typeof(CanvasGroup));
        overlay.SetActive(false);
        Button confirmButton = CreateUiObject("Confirm", typeof(Image), typeof(Button)).GetComponent<Button>();
        Button cancelButton = CreateUiObject("Cancel", typeof(Image), typeof(Button)).GetComponent<Button>();
        CreateObject("EventSystem").AddComponent<EventSystem>();
        SetField(gameUIManager, "forfeitConfirmation", overlay);
        SetField(gameUIManager, "confirmButton", confirmButton);
        SetField(gameUIManager, "cancelButton", cancelButton);
        if (playerHandController != null)
        {
            SetField(gameUIManager, "playerHandController", playerHandController);
        }
        uiObject.SetActive(true);
        if (GetFieldValue<object>(gameUIManager, "panelMap") == null)
        {
            Invoke(gameUIManager, "Awake");
        }
        Invoke(gameUIManager, "OnEnable");

        GameObject managerObject = CreateInactiveObject("SoloScoringGameManager");
        SoloScoringGameManager manager = managerObject.AddComponent<SoloScoringGameManager>();
        ScoreManagerDistance score = managerObject.AddComponent<ScoreManagerDistance>();
        Timer timer = managerObject.AddComponent<Timer>();
        SetField(manager, "scoreManagerDistance", score);
        SetField(manager, "redstoneClock", timer);
        SetField(manager, "soloUIController", gameUIManager);
        if (playerHandController != null)
        {
            SetField(manager, "playerHand", playerHandController);
        }

        manager.Construct(score);
        score.Initialize();
        managerObject.SetActive(true);
        if (playerHandController != null)
        {
            Invoke(manager, "OnEnable");
        }

        return manager;
    }

    private static void AssertSoloUiRenameBoundary()
    {
        Assembly runtimeAssembly = typeof(SoloScoringGameManager).Assembly;
        Type soloUiType = runtimeAssembly.GetType("SoloScoringUIController");
        Type compatibilityType = runtimeAssembly.GetType("GameUIManager");

        Assert.That(soloUiType, Is.Not.Null, "The solo presentation owner must be named SoloScoringUIController.");
        Assert.That(typeof(MonoBehaviour).IsAssignableFrom(soloUiType), Is.True);
        Assert.That(compatibilityType, Is.Null, "The bounded Task 1 compatibility facade must be removed after caller migration.");
    }

    private static void AssertSoloManagerRenameBoundary()
    {
        Type soloManagerType = typeof(SoloScoringGameManager).Assembly.GetType("SoloScoringGameManager");
        Type compatibilityType = typeof(SoloScoringGameManager).Assembly.GetType("MahjongGameManager");

        Assert.That(soloManagerType, Is.Not.Null,
            "The solo lifecycle owner must be named SoloScoringGameManager.");
        Assert.That(typeof(MonoBehaviour).IsAssignableFrom(soloManagerType), Is.True);
        Assert.That(compatibilityType, Is.Null,
            "The temporary compatibility facade must be removed after caller migration.");
    }

    private static void AssertSoloModeRootSceneContract()
    {
        string scenePath = Path.Combine(Application.dataPath, "Scenes", "SampleScene.unity");
        string scene = File.ReadAllText(scenePath);
        Type uiManagerType = typeof(SoloScoringGameManager).Assembly.GetType("UiManager");

        Assert.That(GetField(uiManagerType, "soloScoringModeRoot"), Is.Not.Null,
            "UiManager must own the serialized solo mode root activation boundary.");
        Assert.That(CountOccurrences(scene, "m_Name: SoloScoringModeRoot"), Is.EqualTo(1));
        Assert.That(scene, Does.Contain("soloScoringModeRoot: {fileID: 1987654321}"));
        Assert.That(scene, Does.Contain("m_Name: SoloScoringGameManager"));
        Assert.That(CountOccurrences(scene, "m_Name: EventSystem"), Is.EqualTo(1));
    }

    private static void AssertSoloUiOwnershipBoundary()
    {
        Type soloUiType = typeof(SoloScoringGameManager).Assembly.GetType("SoloScoringUIController");
        string[] movedFields =
        {
            "playerHandView", "uiScoreDistanceInfo", "uiScoreInfo", "uiRoundInfo", "uiCallHolder",
            "uiWininfo", "uiRemainingTime", "uiGameOver", "forfeitConfirmation", "confirmButton", "cancelButton"
        };

        foreach (string fieldName in movedFields)
        {
            Assert.That(GetField(soloUiType, fieldName), Is.Not.Null, $"Solo UI must own {fieldName}.");
            Assert.That(GetField(typeof(SoloScoringGameManager), fieldName), Is.Null, $"Game manager must not own {fieldName} presentation state.");
        }

        Assert.That(soloUiType.GetEvent("ConfirmRequested"), Is.Not.Null);
        Assert.That(soloUiType.GetEvent("CancelRequested"), Is.Not.Null);
        Assert.That(GetMethod(soloUiType, "ShowForfeitConfirmation"), Is.Not.Null);
        Assert.That(GetMethod(soloUiType, "HideForfeitConfirmation"), Is.Not.Null);
        Assert.That(Enum.GetNames(typeof(GameUIState)), Does.Not.Contain("ForfeitConfirmation"));
    }

    private static void AssertForfeitOverlaySceneContract()
    {
        string scenePath = Path.Combine(Application.dataPath, "Scenes", "SampleScene.unity");
        string scene = File.ReadAllText(scenePath);

        Assert.That(CountOccurrences(scene, "m_Name: ForfeitConfirmation"), Is.EqualTo(1));
        Assert.That(scene, Does.Not.Contain("\n  - state: 9\n"),
            "The forfeit overlay must not remain in the mutually exclusive panel map.");
        Assert.That(scene, Does.Not.Contain("m_MethodName: ConfirmForfeit"));
        Assert.That(scene, Does.Not.Contain("m_MethodName: CancelForfeit"));
        Assert.That(scene, Does.Contain("forfeitConfirmation: {fileID: 1412270343}"));
        Assert.That(scene, Does.Contain("confirmButton: {fileID: 294217836}"));
        Assert.That(scene, Does.Contain("cancelButton: {fileID: 1444117286}"));
        Assert.That(scene, Does.Contain("m_SelectOnRight: {fileID: 1444117286}"));
        Assert.That(scene, Does.Contain("m_SelectOnLeft: {fileID: 294217836}"));
        Assert.That(scene, Does.Contain("m_SelectOnDown: {fileID: 1444117286}"));
        Assert.That(scene, Does.Contain("m_SelectOnUp: {fileID: 294217836}"));
        Assert.That(scene, Does.Contain("m_Name: EventSystem"));
    }

    private static int CountOccurrences(string source, string value)
    {
        int count = 0;
        int index = 0;
        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    private static void AssertPlayerHandRenderingBoundary()
    {
        Assembly runtimeAssembly = typeof(SoloScoringGameManager).Assembly;
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
        Type controllerType = typeof(SoloScoringGameManager).Assembly.GetType("PlayerHandController");

        Assert.That(controllerType, Is.Not.Null, "The input owner must be named PlayerHandController.");
        Assert.That(GetField(typeof(SoloScoringGameManager), "playerHand").FieldType, Is.EqualTo(controllerType));
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

    private GameObject CreateUiObject(string name, params Type[] componentTypes)
    {
        var types = new List<Type> { typeof(RectTransform) };
        types.AddRange(componentTypes);
        GameObject createdObject = new GameObject(name, types.ToArray());
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
