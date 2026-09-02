using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoloSessionLifecycleTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [Test]
    public void RecoverInterruptedSaveTest_RestoresDurableBackupBeforeNextMutation()
    {
        byte[] originalBytes = { 0x10, 0x20, 0x30, 0x40 };
        byte[] interruptedBytes = { 0x50, 0x60, 0x70 };
        string temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            $"RiichiNya-SaveRecovery-{Guid.NewGuid():N}");
        string livePath = Path.Combine(temporaryDirectory, "yaml.json");
        string backupPath = livePath + ".phase1-test-backup";
        string absentPath = livePath + ".phase1-test-absent";

        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            File.WriteAllBytes(backupPath, originalBytes);
            File.WriteAllBytes(livePath, interruptedBytes);

            RecoverSaveFile(livePath, backupPath, absentPath);

            Assert.That(File.ReadAllBytes(livePath), Is.EqualTo(originalBytes));
            Assert.That(File.Exists(backupPath), Is.False);
            Assert.That(File.Exists(absentPath), Is.False);

            GuardSaveFile(livePath, backupPath, absentPath);

            Assert.That(File.Exists(livePath), Is.False);
            Assert.That(File.ReadAllBytes(backupPath), Is.EqualTo(originalBytes));
            Assert.That(File.Exists(absentPath), Is.False);

            File.WriteAllBytes(livePath, interruptedBytes);
            RecoverSaveFile(livePath, backupPath, absentPath);

            Assert.That(File.ReadAllBytes(livePath), Is.EqualTo(originalBytes));
            Assert.That(File.Exists(backupPath), Is.False);
            Assert.That(File.Exists(absentPath), Is.False);

            File.Delete(livePath);
            GuardSaveFile(livePath, backupPath, absentPath);

            Assert.That(File.Exists(livePath), Is.False);
            Assert.That(File.Exists(backupPath), Is.False);
            Assert.That(File.Exists(absentPath), Is.True);

            File.WriteAllBytes(livePath, interruptedBytes);
            RecoverSaveFile(livePath, backupPath, absentPath);

            Assert.That(File.Exists(livePath), Is.False);
            Assert.That(File.Exists(backupPath), Is.False);
            Assert.That(File.Exists(absentPath), Is.False);
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, true);
            }
        }
    }

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

    [Test]
    public void ForfeitRequested_IsSeparateAndReturnsBeforeDiscard()
    {
        Type controllerType = AssertPlayerHandInputBoundary();
        MethodInfo handleEscape = GetMethod(controllerType, "HandleEscape");

        Assert.That(handleEscape, Is.Not.Null,
            "The production Escape branch needs a callable seam for same-frame regression coverage.");

        Component controller = CreateInactiveObject("PlayerHandController").AddComponent(controllerType);
        int forfeitCount = 0;
        int discardCount = 0;
        int callCount = 0;
        AddEventHandler(controller, "ForfeitRequested", (Action)(() => forfeitCount++));
        AddEventHandler(controller, "OnPlayerDiscard", (Action<int>)(_ => discardCount++));
        AddEventHandler(controller, "OnPlayerCall", (Action<PlayerCallType>)(_ => callCount++));

        bool consumed = (bool)handleEscape.Invoke(controller, new object[] { true });
        if (!consumed)
        {
            Invoke(controller, "DiscardSelectedTile");
            RaiseEvent(controller, "OnPlayerCall", PlayerCallType.Tsumo);
        }

        Assert.That(consumed, Is.True);
        Assert.That(forfeitCount, Is.EqualTo(1));
        Assert.That(discardCount, Is.Zero);
        Assert.That(callCount, Is.Zero);
        Assert.That(Enum.GetNames(typeof(PlayerCallType)), Does.Not.Contain("Forfeit"));
    }

    [Test]
    public void OpenConfirmation_SynchronouslyBlocksGameplay()
    {
        Type controllerType = AssertPlayerHandInputBoundary();
        Component controller = CreateInactiveObject("PlayerHandController").AddComponent(controllerType);
        SoloScoringGameManager manager = CreateManager(controller);

        manager.currentState = GameState.PlayerTurn;
        RaiseEvent(controller, "ForfeitRequested");

        Component soloUiController = GetFieldValue<Component>(manager, "soloUIController");
        GameObject overlay = GetFieldValue<GameObject>(soloUiController, "forfeitConfirmation");
        Button cancelButton = GetFieldValue<Button>(soloUiController, "cancelButton");
        Assert.That(manager.currentState, Is.EqualTo(GameState.Processing));
        Assert.That(GetFieldValue<bool>(manager, "pendingForfeit"), Is.True);
        Assert.That(GetFieldValue<bool>(controller, "gameplayInputEnabled"), Is.False);
        Assert.That(overlay.activeSelf, Is.True);
        Assert.That(cancelButton, Is.Not.Null);
    }

    [Test]
    public void SecondEscape_CancelsAndRestoresGameplay()
    {
        Type controllerType = AssertPlayerHandInputBoundary();
        Component controller = CreateInactiveObject("PlayerHandController").AddComponent(controllerType);
        SoloScoringGameManager manager = CreateManager(controller);

        manager.currentState = GameState.PlayerTurn;
        RaiseEvent(controller, "ForfeitRequested");
        RaiseEvent(controller, "ForfeitRequested");

        Component soloUiController = GetFieldValue<Component>(manager, "soloUIController");
        Assert.That(manager.currentState, Is.EqualTo(GameState.PlayerTurn));
        Assert.That(GetFieldValue<bool>(manager, "pendingForfeit"), Is.False);
        Assert.That(GetFieldValue<bool>(controller, "gameplayInputEnabled"), Is.True);
        Assert.That(GetFieldValue<GameObject>(soloUiController, "forfeitConfirmation").activeSelf, Is.False);
    }

    [Test]
    public void Confirmation_DoesNotPauseTimer()
    {
        Type controllerType = AssertPlayerHandInputBoundary();
        Component controller = CreateInactiveObject("PlayerHandController").AddComponent(controllerType);
        SoloScoringGameManager manager = CreateManager(controller);
        Timer timer = GetFieldValue<Timer>(manager, "redstoneClock");
        timer.StartTimer(180f);

        manager.currentState = GameState.PlayerTurn;
        RaiseEvent(controller, "ForfeitRequested");
        Invoke(timer, "CheckTimerTick", 1f);

        Assert.That(GetFieldValue<bool>(timer, "_paused"), Is.False);
        Assert.That(timer.RemainingTime, Is.EqualTo(179f));
        Assert.That(manager.currentState, Is.EqualTo(GameState.Processing));
    }

    [Test]
    public void TimeoutDuringConfirmation_WinsAndRejectsLateActions()
    {
        Type controllerType = AssertPlayerHandInputBoundary();
        Component controller = CreateInactiveObject("PlayerHandController").AddComponent(controllerType);
        SoloScoringGameManager manager = CreateManager(controller);
        int gameOverCount = 0;
        manager.OnGameOver += () => gameOverCount++;
        byte[] originalSave = PreserveSaveFile();

        try
        {
            manager.currentState = GameState.PlayerTurn;
            RaiseEvent(controller, "ForfeitRequested");
            Invoke(manager, "HandleTimerFinished");
            manager.ConfirmForfeit();
            manager.CancelForfeit();
            RaiseEvent(controller, "ForfeitRequested");

            Assert.That(gameOverCount, Is.EqualTo(1));
            Assert.That(manager.currentState, Is.EqualTo(GameState.GameOver));
            Assert.That(GetFieldValue<GameEndReason>(manager, "lastEndReason"), Is.EqualTo(GameEndReason.TimeExpired));
            Assert.That(GetFieldValue<bool>(manager, "sessionFinalized"), Is.True);
            Assert.That(GetFieldValue<bool>(manager, "pendingForfeit"), Is.False);
        }
        finally
        {
            RestoreSaveFile(originalSave);
        }
    }

    [Test]
    public void TimeoutNewRecord_RendersAndPersistsUpdatedHighScore()
    {
        byte[] originalSave = PreserveSaveFile();

        try
        {
            SettingsManager.Save(new PetitGameSaveData { highScore = 10f });
            SoloScoringGameManager manager = CreateManager();
            Component soloUiController = GetFieldValue<Component>(manager, "soloUIController");
            UiGameOver gameOver = CreateInactiveObject("UiGameOver").AddComponent<UiGameOver>();
            TextMeshProUGUI total = CreateUiObject("Total", typeof(CanvasRenderer), typeof(TextMeshProUGUI))
                .GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI record = CreateUiObject("Record", typeof(CanvasRenderer), typeof(TextMeshProUGUI))
                .GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI reason = CreateUiObject("Reason", typeof(CanvasRenderer), typeof(TextMeshProUGUI))
                .GetComponent<TextMeshProUGUI>();
            SetField(gameOver, "uiTotalScore", total);
            SetField(gameOver, "uiRecordScore", record);
            SetField(gameOver, "uiReason", reason);
            SetField(soloUiController, "uiGameOver", gameOver);

            ScoreManagerDistance score = GetFieldValue<ScoreManagerDistance>(manager, "scoreManagerDistance");
            score.GetInstantDistance(50f);
            float finalScore = score.DistanceWithAccumulated;
            Invoke(manager, "HandleTimerFinished");

            Assert.That(record.text, Is.EqualTo(finalScore.ToString()));
            Assert.That(SettingsManager.Load().highScore, Is.EqualTo(finalScore));
        }
        finally
        {
            RestoreSaveFile(originalSave);
        }
    }

    [Test]
    public void PlayerController_Subscriptions_AreSymmetricAcrossRootCycles()
    {
        Type controllerType = AssertPlayerHandInputBoundary();
        Component controller = CreateInactiveObject("PlayerHandController").AddComponent(controllerType);
        SoloScoringGameManager manager = CreateManager(controller);

        Assert.That(CountTargetHandlers(controller, manager), Is.EqualTo(3));
        Invoke(manager, "OnDisable");
        Assert.That(CountTargetHandlers(controller, manager), Is.Zero);
        Invoke(manager, "OnEnable");
        Invoke(manager, "OnEnable");
        Assert.That(CountTargetHandlers(controller, manager), Is.EqualTo(3));
    }

    [Test]
    public void UiController_Subscriptions_AreSymmetricAcrossRootCycles()
    {
        SoloScoringGameManager manager = CreateManager();
        Component soloUiController = GetFieldValue<Component>(manager, "soloUIController");
        Button confirmButton = GetFieldValue<Button>(soloUiController, "confirmButton");
        Button cancelButton = GetFieldValue<Button>(soloUiController, "cancelButton");
        int confirmCount = 0;
        int cancelCount = 0;
        AddEventHandler(soloUiController, "ConfirmRequested", (Action)(() => confirmCount++));
        AddEventHandler(soloUiController, "CancelRequested", (Action)(() => cancelCount++));

        Invoke(soloUiController, "OnEnable");
        Invoke(soloUiController, "OnEnable");
        confirmButton.onClick.Invoke();
        Assert.That(confirmCount, Is.EqualTo(1));

        Invoke(soloUiController, "OnDisable");
        cancelButton.onClick.Invoke();
        Assert.That(cancelCount, Is.Zero);

        Invoke(soloUiController, "OnEnable");
        cancelButton.onClick.Invoke();
        Assert.That(cancelCount, Is.EqualTo(1));
    }

    [Test]
    public void ForfeitOverlay_IsOutsidePanelMapAndSelectsCancel()
    {
        AssertSoloModeRootSceneContract();
        AssertForfeitOverlaySceneContract();
        AssertSceneOverlaySelectsCancel();
        SoloScoringGameManager manager = CreateManager();
        Component soloUiController = GetFieldValue<Component>(manager, "soloUIController");

        Invoke(soloUiController, "ShowForfeitConfirmation");

        GameObject overlay = GetFieldValue<GameObject>(soloUiController, "forfeitConfirmation");
        Button cancelButton = GetFieldValue<Button>(soloUiController, "cancelButton");
        Assert.That(overlay.activeSelf, Is.True);
        Assert.That(cancelButton, Is.Not.Null);
        Assert.That(Enum.GetNames(typeof(GameUIState)), Does.Not.Contain("ForfeitConfirmation"));
    }

    [Test]
    public void GameOver_Forfeit_RendersReasonAndDistance()
    {
        UiGameOver gameOver = CreateInactiveObject("UiGameOver").AddComponent<UiGameOver>();
        TextMeshProUGUI total = CreateUiObject("Total", typeof(CanvasRenderer), typeof(TextMeshProUGUI))
            .GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI record = CreateUiObject("Record", typeof(CanvasRenderer), typeof(TextMeshProUGUI))
            .GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI reason = CreateUiObject("Reason", typeof(CanvasRenderer), typeof(TextMeshProUGUI))
            .GetComponent<TextMeshProUGUI>();
        SetField(gameOver, "uiTotalScore", total);
        SetField(gameOver, "uiRecordScore", record);
        SetField(gameOver, "uiReason", reason);

        gameOver.Initialize(123.5f, 456f, GameEndReason.Forfeit);

        Assert.That(reason.text, Is.EqualTo("포기"));
        Assert.That(total.text, Is.EqualTo(123.5f.ToString()));
        Assert.That(record.text, Is.EqualTo(456f.ToString()));
    }

    [Test]
    public void ReturnToLobby_DisablesSoloModeRoot()
    {
        CreateManager();
        GameObject modeRoot = CreateObject("SoloScoringModeRoot");
        GameObject uiManagerObject = CreateInactiveObject("UiManager");
        Component uiManager = uiManagerObject.AddComponent(typeof(SoloScoringGameManager).Assembly.GetType("UiManager"));
        FieldInfo panels = GetField(uiManager.GetType(), "panels");
        panels.SetValue(uiManager, Activator.CreateInstance(panels.FieldType));
        FieldInfo panelMap = GetField(uiManager.GetType(), "panelMap");
        panelMap.SetValue(uiManager, Activator.CreateInstance(panelMap.FieldType));
        object history = Activator.CreateInstance(GetField(uiManager.GetType(), "historyStack").FieldType);
        history.GetType().GetMethod("Push").Invoke(history, new object[] { UIState.MainMenu });
        SetField(uiManager, "historyStack", history);
        SetField(uiManager, "currentState", UIState.InGame);
        SetField(uiManager, "soloScoringModeRoot", modeRoot);

        Invoke(uiManager, "OnBBagguButton");

        Assert.That(modeRoot.activeSelf, Is.False);
        Assert.That(GetFieldValue<UIState>(uiManager, "currentState"), Is.EqualTo(UIState.MainMenu));
    }

    [Test]
    public void RestartAfterLobby_UsesFreshStateAndSingleHandlers()
    {
        Scene loadedScene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Additive);
        try
        {
            UiManager uiManager = FindComponents<UiManager>(loadedScene)[0];
            Transform soloRoot = FindTransform(loadedScene, "SoloScoringModeRoot");
            SoloScoringGameManager manager = FindComponents<SoloScoringGameManager>(loadedScene)[0];
            PlayerHandController controller = FindComponents<PlayerHandController>(loadedScene)[0];
            PlayerHandView view = FindComponents<PlayerHandView>(loadedScene)[0];
            SoloScoringUIController soloUiController = FindComponents<SoloScoringUIController>(loadedScene)[0];

            Invoke(uiManager, "Awake");
            Invoke(manager, "Awake");
            Invoke(soloUiController, "Awake");
            Invoke(view, "Awake");
            Invoke(controller, "Awake");
            Invoke(manager, "OnEnable");
            Invoke(soloUiController, "OnEnable");
            Invoke(controller, "OnEnable");
            uiManager.OnGameStartButton();

            MahjongRound firstRound = GetFieldValue<MahjongRound>(manager, "currentRound");
            Timer timer = GetFieldValue<Timer>(manager, "redstoneClock");
            Invoke(timer, "CheckTimerTick", 10f);
            GetFieldValue<ScoreManagerDistance>(manager, "scoreManagerDistance").GetInstantDistance(50f);
            manager.currentState = GameState.PlayerTurn;
            RaiseEvent(controller, "ForfeitRequested");
            Invoke(controller, "MoveHand", 2);

            MahjongTileGameObject[] tiles = GetFieldValue<MahjongTileGameObject[]>(view, "tilesInHand");
            Assert.That(GetFieldValue<bool>(tiles[2], "isSelected"), Is.True);

            uiManager.OnBBagguButton();
            Invoke(controller, "OnDisable");
            Invoke(soloUiController, "OnDisable");
            Invoke(manager, "OnDisable");
            Assert.That(soloRoot.gameObject.activeSelf, Is.False);

            Invoke(manager, "OnEnable");
            Invoke(soloUiController, "OnEnable");
            Invoke(controller, "OnEnable");
            uiManager.OnGameStartButton();

            MahjongRound secondRound = GetFieldValue<MahjongRound>(manager, "currentRound");
            Assert.That(secondRound, Is.Not.SameAs(firstRound));
            Assert.That(timer.RemainingTime, Is.EqualTo(180f));
            Assert.That(GetFieldValue<ScoreManagerDistance>(manager, "scoreManagerDistance").DistanceWithAccumulated, Is.Zero);
            Assert.That(manager.currentState, Is.EqualTo(GameState.PlayerTurn));
            Assert.That(GetFieldValue<bool>(manager, "pendingForfeit"), Is.False);
            Assert.That(GetFieldValue<GameObject>(soloUiController, "forfeitConfirmation").activeSelf, Is.False);
            Assert.That(controller.currentIndex, Is.EqualTo(6));
            Assert.That(Array.FindAll(tiles, tile => GetFieldValue<bool>(tile, "isSelected")).Length, Is.EqualTo(1));
            Assert.That(GetFieldValue<bool>(tiles[6], "isSelected"), Is.True);
            Assert.That(CountTargetHandlers(controller, manager), Is.EqualTo(3));
            Assert.That(CountTargetHandlers(soloUiController, manager), Is.EqualTo(2));
            Assert.That(CountTargetHandlers(secondRound, manager), Is.EqualTo(6));
            Assert.That(CountTargetHandlers(timer, manager), Is.EqualTo(1));
        }
        finally
        {
            EditorSceneManager.CloseScene(loadedScene, true);
        }
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
        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/SampleScene.unity");
        MonoScript managerScript = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/SoloScoringGameManager.cs");

        Assert.That(sceneAsset, Is.Not.Null, "The edited scene YAML must remain importable.");
        Assert.That(managerScript, Is.Not.Null);
        Assert.That(managerScript.GetClass(), Is.EqualTo(typeof(SoloScoringGameManager)),
            "The preserved manager GUID must resolve to the renamed MonoBehaviour.");
        Assert.That(GetField(uiManagerType, "soloScoringModeRoot"), Is.Not.Null,
            "UiManager must own the serialized solo mode root activation boundary.");
        Assert.That(CountOccurrences(scene, "m_Name: SoloScoringModeRoot"), Is.EqualTo(1));
        Assert.That(scene, Does.Match("m_Name: SoloScoringModeRoot[\\s\\S]*?m_IsActive: 0"),
            "The solo mode root must be inactive at lobby startup.");
        Assert.That(scene, Does.Contain("soloScoringModeRoot: {fileID: 1987654321}"));
        Assert.That(scene, Does.Contain("m_Name: SoloScoringGameManager"));
        Assert.That(CountOccurrences(scene, "m_Name: EventSystem"), Is.EqualTo(1));

        Scene loadedScene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Additive);
        try
        {
            Transform soloRoot = FindTransform(loadedScene, "SoloScoringModeRoot");
            EventSystem[] eventSystems = FindComponents<EventSystem>(loadedScene);
            Assert.That(soloRoot, Is.Not.Null);
            Assert.That(soloRoot.gameObject.activeSelf, Is.False);
            Assert.That(eventSystems, Has.Length.EqualTo(1));
            Assert.That(eventSystems[0].transform.IsChildOf(soloRoot), Is.False,
                "The shared EventSystem must stay outside the inactive solo root.");
        }
        finally
        {
            EditorSceneManager.CloseScene(loadedScene, true);
        }

        Assert.That(ReadGuid("Assets/Scripts/SoloScoringGameManager.cs.meta"),
            Is.EqualTo("83be086a716bef149853d38249179bd7"));
        Assert.That(ReadGuid("Assets/Scripts/UI-Kozeki/PlayerHandController.cs.meta"),
            Is.EqualTo("f741381e994254649afad56bd8fdc47a"));
        Assert.That(ReadGuid("Assets/Scripts/UI-Kozeki/SoloScoringUIController.cs.meta"),
            Is.EqualTo("9b978f8eb5c74984b8f91d51ff046652"));
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

    private static void RaiseEvent(Component source, string eventName, params object[] arguments)
    {
        Delegate handlers = GetField(source.GetType(), eventName).GetValue(source) as Delegate;
        Assert.That(handlers, Is.Not.Null, $"{eventName} must have an invokable backing delegate.");
        handlers.DynamicInvoke(arguments);
    }

    private static void AddEventHandler(Component source, string eventName, Delegate handler)
    {
        source.GetType().GetEvent(eventName).AddEventHandler(source, handler);
    }

    private static void GuardSaveFile(string livePath, string backupPath, string absentPath)
    {
    }

    private static void RecoverSaveFile(string livePath, string backupPath, string absentPath)
    {
    }

    private static byte[] PreserveSaveFile()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "yaml.json");
        return File.Exists(savePath) ? File.ReadAllBytes(savePath) : null;
    }

    private static void RestoreSaveFile(byte[] originalSave)
    {
        string savePath = Path.Combine(Application.persistentDataPath, "yaml.json");
        if (originalSave == null)
        {
            File.Delete(savePath);
            return;
        }

        File.WriteAllBytes(savePath, originalSave);
    }

    private static string ReadGuid(string assetMetaPath)
    {
        foreach (string line in File.ReadAllLines(assetMetaPath))
        {
            if (line.StartsWith("guid: ", StringComparison.Ordinal))
            {
                return line.Substring("guid: ".Length);
            }
        }

        return string.Empty;
    }

    private static Transform FindTransform(Scene scene, string objectName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform candidate in transforms)
            {
                if (candidate.name == objectName)
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static T[] FindComponents<T>(Scene scene) where T : Component
    {
        var components = new List<T>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            components.AddRange(root.GetComponentsInChildren<T>(true));
        }

        return components.ToArray();
    }

    private static void AssertSceneOverlaySelectsCancel()
    {
        Scene loadedScene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Additive);
        try
        {
            SoloScoringUIController[] controllers = FindComponents<SoloScoringUIController>(loadedScene);
            EventSystem[] eventSystems = FindComponents<EventSystem>(loadedScene);
            Assert.That(controllers, Has.Length.EqualTo(1));
            Assert.That(eventSystems, Has.Length.EqualTo(1));

            EventSystem eventSystem = eventSystems[0];
            Invoke(eventSystem, "OnEnable");
            Invoke(controllers[0], "Awake");
            controllers[0].ShowForfeitConfirmation();

            Button cancelButton = GetFieldValue<Button>(controllers[0], "cancelButton");
            Assert.That(EventSystem.current, Is.EqualTo(eventSystem));
            Assert.That(eventSystem.currentSelectedGameObject, Is.EqualTo(cancelButton.gameObject));
        }
        finally
        {
            EditorSceneManager.CloseScene(loadedScene, true);
        }
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
        foreach (FieldInfo field in source.GetType().GetFields(
                     BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
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
