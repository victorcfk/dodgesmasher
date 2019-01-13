using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using System;

public class GameManager : MonoBehaviour {

    public enum StartGameMode
    {
        NEW_GAME_WITH_TUTORIAL,
        NEW_GAME_WITHOUT_TUTORIAL,
        CONTINUE_GAME_WITHOUT_TUTORIAL
    }

    enum EndLevelMode
    {
        NEXT_LEVEL,
        GAME_OVER,
        QUIT_GAME,
    }

    enum UIMode
    {
        TUTORIAL_STAGE,
        TRANSITION_STAGE,
        FULL_STAGE,
        YOU_WIN_STAGE,
    }

    public static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            return _instance;
        }
    }
    
#if UNITY_EDITOR
    [SerializeField]
    bool _skipNormalLoading = false;
#endif

    [SerializeField]
    private Player _playerAvatarPrefab;
    
    [NonSerialized]
    private Player _playerAvatar;
    [NonSerialized]
    private Damager _playerDamager;
    [NonSerialized]
    private Rigidbody2D _playerAvatarRigidBody2D;
    [NonSerialized]
    private Collider2D _playerAvatarCollider;
    [NonSerialized]
    private SpriteRenderer _playerAvatarSpriteRenderer;

    [Space]
    [SerializeField]
    Camera _gameCamera;
    [Space]
    [SerializeField]
    GameplayUIManager _gameplayUIManager;
    [SerializeField]
    SoundManager _soundManager;
    [SerializeField]
    MyIAPManager _myIAPManager;
    [SerializeField]
    Canvas _gameplayUIManagerCanvas;
    [Space]
    [SerializeField]
    StartOptions _startOptions;
    [SerializeField]
    Canvas _startOptionsCanvas;
    [Space]
    [SerializeField]
    Animator _fadeTransition;
    [SerializeField]
    Image _fadeImage;
    int _fadeOutHash = Animator.StringToHash("fadeout");
    int _fadeInHash = Animator.StringToHash("fadein");

    [Space]
    [SerializeField]
    private UpgradeManager _upgradeManager;
    [SerializeField]
    private GooglePlayGamesManager _googlePlayGamesManager;

    [Header("Scene info")]
    [SerializeField]
    int _mainMenuSceneIndex;
    [SerializeField]
    int _gameplaySceneIndex;
    bool _isInGameplay = false;

    [Space]
    [Header("Gameplay Settings & Time behaviours")]
    
    public bool allowDMGSlowDown = true;
    public bool allowDamageInvulnerability = true;
    public bool isDragOppositeToFire = true;
    public float playerOnDamagedInvulnerabilityTimerGain = 1;
    bool _allowPlayerControl = true;
    public bool IsPlayerAiming { get; private set; }
    [SerializeField]
    int _playerMaxHealthBase = 3;
    [SerializeField]
    int _playerMaxEnergyBase = 3;
    [SerializeField]
    float _playerEnergyRegenBase = 0.5f;
    [SerializeField]
    int _playerHealthRegainAfterEachLevelBase = 0;
    [SerializeField]
    int _playerDMGBase = 1;
    [SerializeField]
    float _playerEnergyOnHitBase = 0;
    [SerializeField]
    int _playerOrbitalCountBase = 0;
    [SerializeField]
    int _playerHealthRegainAfterEachLevel;

    [SerializeField]
    float _defaultFixedTimeStep = 0.02f;
    [SerializeField]
    float _defaultTimeScale = 1;
    [SerializeField]
    float _aimingSlowDownRate = 0.2f;

    [SerializeField]
    float _freezeFrameTimeScale = 0.1f;
    [SerializeField]
    int _freezeFrameFrameSkips = 5;
    float _timeTillNextFire;
    public float MaxTimeAllowedForAiming;
    public float TimeBetweenFire = 0;
    bool _isPaused;
    
    [Space]

    [Header("Movement Behaviours")]
    [Tooltip("How far does the player need to drag to get to max speed")]
    public float DragDistanceForMaxSpeed = 5;
    public float PlayerBaseLaunchSpeed = 15f;   //The max speed at which the player can be launched
    public float DefaultPlayerGravityScale = 1;
    
    public float Energy
    {
        get
        {
            return _playerCurrentEnergy;
        }
    }
    [Space]
    float _playerCurrentEnergy = 3;  //We assume 1 energy gives 1 velocity
    float _playerMaxEnergy = 3;
    float _playerEnergyRegenPerSec = 0.5f;
    public float MinEnergyRequiredToFire = 0.1f;
    [Tooltip("If more than 0, this drains fixed energy on jump, rather than based on the distance used")]
    public float FixedEnergyDrain = 1;
    [Tooltip("If more than 0, each jump will take at least this much energy")]
    public float MinEnergyDrain = 0.1f;
    public bool AllowEnergyRegenWhileAiming = false;

    [Space]

    [Header("Level creation and Object Spawning")]
    [SerializeField]
    EnemySpawner _enemySpawner;
    [SerializeField]
    ObjectSpawner _environmentSpawner;
    [SerializeField]
    List<LevelTemplate> _enemyTemplates;
    [SerializeField]
    List<LevelTemplate> _environmentTemplates;
    [SerializeField]
    private DragFire _dragFire;
    [SerializeField]
    private TutorialManager _tutorialPrefab;
    TutorialManager _tutorialObject;

    //private GameObject _wallsObject;
    int _currentRunSeed;

    //These are not remembered if the game is restarted. We assume the player will not remember
    //==============================
    int _lastUsedTopEnvTemplate = -1;
    int _currentTopEnvTemplate = -1;

    int _lastUsedBtmEnvTemplate = -1;
    int _currentBtmEnvTemplate = -1;

    int _lastUsedTopEnemyTemplate = -1; 
    int _currentTopEnemyTemplate = -1;

    int _lastUsedBtmEnemyTemplate = -1;
    int _currentBtmEnemyTemplate = -1;
    //==============================

    [Serializable]
    struct BGAndLevelRange
    {
        public string BGResourceName;
        public Color AimAreaColor;
    }

    [SerializeField]
    private List<BGAndLevelRange> _BGsForLevels;
    [SerializeField]
    private SpriteRenderer BGImage;

    [Header("Saving")]
    List<string> _runProgressionKeys =
        new List<string>(
            new string[] {
            "run_difficulty",           //0
            "run_level",                //1
            "run_health",               //2
            "run_score",                //3
            "run_score_multiplier",     //4
            "run_last_known_seed"});    //5

    List<string> _permProgressionKeys =
        new List<string>(
            new string[] {
            "hi_score",                     //0
            "money",                        //1
            "perm_upgrade_1_heath",         //2
            "perm_upgrade_2_energy",        //3
            "perm_upgrade_3_damage",        //4
            "perm_upgrade_4_orbital",       //5
            "perm_upgrade_5_stun_on_hit",   //6
            "perm_upgrade_6_segment_skips", //7
            "purchased",                    //8
            "play_duration",                //9
            "highest_achieved_level",       //10
            "rated",                        //11
            "sfx_on",                       //12
            "bgm_on",                       //13
            "tutorial_status",               //14
            "achievement1", //15
            "achievement2", //16
            "achievement3", //17
            "achievement4", //18
            "achievement5", //19
            "stage_unlocked"    //20
            });

    [Header("Misc")]

    [SerializeField]
    int _currentLevel;
    const int FINAL_STAGE = 5;
    const int FINAL_LEVEL = 29;
    int AvailableBoughtStages
    {
        get
        {
            if (IsPayingPlayer)
                return int.MaxValue;
            else
                return ZPlayerPrefs.GetInt(_permProgressionKeys[20], 1);
        }

        set
        {
            ZPlayerPrefs.SetInt(_permProgressionKeys[20], value);
        }
    }

    int UnlockStagePrice
    {
        get
        {
            if (IsPayingPlayer)
                return 0;
            else
            {
                int boughtStages = AvailableBoughtStages;

                switch (boughtStages)
                {
                    case 1:
                        return 500;
                    case 2:
                        return 1000;
                    case 3:
                        return 2000;
                    case 4:
                        return 4000;
                    case 5:
                    default:
                        return 0;
                }
            }
        }
    }


    /// <summary>
    /// The stage number also functions as the current difficulty of the game
    /// </summary>
    /// <param name="inCurrentLevel"></param>
    /// <returns></returns>
    int GetStageForLevel(int inCurrentLevel)
    {
        bool ignorethis;

        return GetStageForLevel(inCurrentLevel, out ignorethis);
    }

    int GetStageForLevel(int inCurrentLevel, out bool isTransitionStage)
    {
        isTransitionStage = false;

        int[] StageTransitionPoints = { 6, 12, 18, 24, 30 };
        
        for (int i = 0; i<StageTransitionPoints.Length; i++)
        {
            if (inCurrentLevel == StageTransitionPoints[i]) isTransitionStage = true;
            if (inCurrentLevel < StageTransitionPoints[i]) return (i+1);
        }

        return StageTransitionPoints.Length;
    }

    int GetLevelForStage(int inStageNum)
    {
        int[] StageStartLevels = { 0, 6, 12, 18, 24, 30 };
        Mathf.Clamp(inStageNum, 0, StageStartLevels.Length-1);

        return StageStartLevels[inStageNum-1] + 1;    //Skip past the transition scenes if any
    }

    bool IsTutorialStage
    {
        get
        {
            return _currentLevel == 0;
        }
    }

    bool IsTransitionLevel
    {
        get
        {
            bool isTransitionStage;
            GetStageForLevel(_currentLevel, out isTransitionStage);

            return isTransitionStage;
        }
    }

    bool HasWonFinalLevel
    {
        get
        {
            return _currentLevel > FINAL_LEVEL;
        }
    }

    bool IsFirstStage
    {
        get
        {
            return _currentLevel == 1;
        }
    }

    [SerializeField]
    int _currentMoney;
    int GetCurrentMoney()
    {
        return _currentMoney;
    }
    [SerializeField]
    int _currentScore = 0;
    [SerializeField]
    float _currentScoreMultiplier = 1;
    [SerializeField]
    float _scoreMultiplierGainOnDMG = 1;
    [SerializeField]
    float _scoreMultiplierLossOnFire = 0.5f;
    int _hiScore = 0;
    int _highestLevelReached = 0;
    int GetHighestLevelReached()
    {
        return _highestLevelReached;
    }
    int _totalPlayDurationSeconds = 0;

    [Space]
    [SerializeField]
    float _minScoreMultiplier = 1;
    [SerializeField]
    float _maxScoreMultiplier = 5;
    [Space]
    [SerializeField]
    float _minScreenHeightProportionBeforeStart = 0.6f;
    [SerializeField]
    float _playerAvatarHeightFromPivotToEdge = 0.5f;

    [Space]
    [SerializeField]
    float _maxTimeToEndScene = 5;
    [SerializeField]
    float _maxTimeToStartScene = 5;

    IEnumerator _sceneEndingIEnumerator;
    IEnumerator _sceneStartingIEnumerator;
    IEnumerator _restoreSpeedCoroutine;
    bool _isGameTransitioningBetweenLevels;
    bool _isGameOverAccepted;
    bool _isInStartMenuPurchaseArea = false;
    bool _isInGameOverMenuPurchaseArea = false;
    bool _isInUpgradesMenuPurchaseArea = false;

    DateTime _lastPlayTimeRecorded;

    AsyncOperation _sceneLoadingOperation;
    
    string _myGameURL = "market://details?id=com.ludojam.impulse";
    string _stage1AchievementComplete = "CgkI4tHEodgeEAIQAg";//achievement_industry
    string _stage2AchievementComplete = "CgkI4tHEodgeEAIQAw";//achievement_endless_sea
    string _stage3AchievementComplete = "CgkI4tHEodgeEAIQBA";//achievement_city
    string _stage4AchievementComplete = "CgkI4tHEodgeEAIQAQ";//achievement_purgatory
    string _stage5AchievementComplete = "CgkI4tHEodgeEAIQBQ";//achievement_submission
    string _leaderBoardID = "CgkI4tHEodgeEAIQBg";

    bool _tutorialCompleted = false;
    bool HasTutorialBeenCompleted
    {
        get
        {
            return _tutorialCompleted;   
        }
    }

    bool IsPayingPlayer
    {
        get
        {
            if (!ZPlayerPrefs.HasKey(_permProgressionKeys[8]))
            {
                return false;
            }
            else
            {
                return ZPlayerPrefs.GetInt(_permProgressionKeys[8]) == 1;
            }
        }
    }

    bool DoWeHaveSavedGameplayRunData
    {
        get
        {
            if (!ZPlayerPrefs.HasKey(_runProgressionKeys[1]))
            {
                return false;
            }
            else
            {

#if UNITY_EDITOR
                Debug.Log("run progression: " + ZPlayerPrefs.GetInt(_runProgressionKeys[1], 1));
#endif

                return ZPlayerPrefs.GetInt(_runProgressionKeys[1], 1) > 1;
            }
        }
    }
    
    bool IsRatedTheGameBefore
    {
        get
        {
            if (!ZPlayerPrefs.HasKey(_permProgressionKeys[11]))
                return false;
            else
                return ZPlayerPrefs.GetInt(_permProgressionKeys[11], 0) == 1;
        }
    }

    bool IsPlayedASufficientAmount
    {
        get
        {
            return
                //got past first segment
                _highestLevelReached > 6 ||
               //play duration > 1 hours
               _totalPlayDurationSeconds > 900;  //15 minutes worth of play = 15*60 = 750
        }
    }

    [NonSerialized]
    int _lastKnownPlayerHealth = 3;
    [NonSerialized]
    bool _lastKnownPlayerAiming = false;

    int _gamesPlayed = 0;
    int _gamesPlayedWithoutSeeingAds = 0;
    bool _hasAttemptedGooglePlayLoginOnce = false;

    System.Random _unknownSeededRandom = new System.Random();
    
    //======================================

    void AskPlayerToRateTheGame()
    {
        Debug.Log("ask to rate");

        ShowDialogBox("Support Us!", "If you are enjoying the game so far, help us by leaving a review and get some Shards!", RewardForReview, null, false);

        //No matter the outcome, we've asked the player
        ZPlayerPrefs.SetInt(_permProgressionKeys[11], 1);
    }

    void RewardForReview()
    {
        Application.OpenURL(_myGameURL);

        ShowInfoBox("Thank you!", "Here's 500 Shards!",null);

        _currentMoney = Mathf.Clamp(_currentMoney + 500,0,int.MaxValue);
    }

    void TransitionLevelComplete()
    {
        ReportGooglePlayAchievements(_highestLevelReached);
        GoToNextLevel();
    }

    /// <summary>
    /// Reports achievements up to a level. records failures
    /// </summary>
    /// <param name="inHighestGameplayLevel"></param>
    void ReportGooglePlayAchievements(int inHighestGameplayLevel)
    {
        string achievementName = string.Empty;
        string failureRecordKey = string.Empty;

        if (inHighestGameplayLevel >= 6)
        {
            achievementName = _stage1AchievementComplete;
            failureRecordKey = _permProgressionKeys[15];
        }
        else
        if (inHighestGameplayLevel >= 12)
        {
            achievementName = _stage2AchievementComplete;
            failureRecordKey = _permProgressionKeys[16];
        }
        else
        if (inHighestGameplayLevel >= 18)
        {
            achievementName = _stage3AchievementComplete;
            failureRecordKey = _permProgressionKeys[17];
        }
        else
        if (inHighestGameplayLevel >= 24)
        {
            achievementName = _stage4AchievementComplete;
            failureRecordKey = _permProgressionKeys[18];
        }
        else
            if (inHighestGameplayLevel >= 30)
        {
            achievementName = _stage5AchievementComplete;
            failureRecordKey = _permProgressionKeys[19];
        }

        Social.ReportProgress(achievementName, 100.0f, (bool success) => {
                    if (success)
                        ZPlayerPrefs.SetInt(failureRecordKey, 1);   //reported
                    else
                        ZPlayerPrefs.SetInt(failureRecordKey, 2);   //achieved but failed to report
        });
    }

    public void TutorialComplete()
    {
        Debug.Log("set tutorial status");
        _tutorialCompleted = true;
        ZPlayerPrefs.SetInt(_permProgressionKeys[14], 1);
        GoToNextLevel();
    }

    public Player Player
    {
        get
        {
            return _playerAvatar;
        }
    }

    public Vector2 PlayerAvatarPosition
    {
        get { return _playerAvatarRigidBody2D.position; }
    }

    public Transform PlayerAvatarTransform
    {
        get { return _playerAvatar.transform; }
    }

    public Camera GameCamera
    {
        get
        {
            return _gameCamera;
        }
    }
    
    void Awake () {

        //We prefer the existing reference
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            Initialise();
        }
    }

    void Initialise()
    {
        Input.multiTouchEnabled = false;

        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        Screen.orientation = ScreenOrientation.Portrait;

        PortraitCheck();

        SceneManager.sceneLoaded += (x,y) => { FadeIn(); _sceneLoadingOperation = null; };

        //Set the initial appearance to be faded out
        _fadeImage.color = Color.black;

        InitialiseUpgradeManager();
        InitialiseGameplayUI();
        InitialiseStartOptions();
        InitialiseIAP();

        //Initialise enemy spawner
        _enemySpawner.OnAllObjectsDestroyed += GoToNextLevel;
        _enemySpawner.OnEnemyDeathGainScore += AddScore;
        LoadPermanentProgression();

        if (!IsPayingPlayer)
            Advertisement.Initialize("1655397");

        _lastPlayTimeRecorded = DateTime.Now;

        DontDestroyOnLoad(gameObject);

        _googlePlayGamesManager.Initialise();

        //level loading behaviours
        //==========================
#if UNITY_EDITOR
        if (_skipNormalLoading)
        {
            savedLevel = _currentLevel;
        }
#endif
        //First time you start the game
        if (!HasTutorialBeenCompleted)
        {
            TransitionToGameplay(_gameplaySceneIndex, StartGameMode.NEW_GAME_WITH_TUTORIAL);
        }
        else
        {
            TransitionToMainMenu();
        }

        //==========================
    }

    void InitialiseGameplayUI()
    {
        _dragFire.OnAimStart += AimStart;
        _dragFire.OnAimStart += RemoveScoreMultiplier;
        _dragFire.OnAimEnd += AimEnd;

        _dragFire.OnFireOff += DrainEnergy;
        _gameplayUIManager.OnGameOver += GameOverAccepted;
        _gameplayUIManager.OnPauseModeToggled += PauseGameToggle;
        _gameplayUIManager.OnQuit += QuitGameplayRun;
    }
        
    void InitialiseStartOptions()
    {
        _startOptions.CallToStartGame += CallToStartGame;

        _startOptions.BuyDMGUpgrade     += () => { TryToPurchase(UpgradeManager.UpgradeType.DMG);       };
        _startOptions.BuyEnergyUpgrade  += () => { TryToPurchase(UpgradeManager.UpgradeType.ENERGY);    };
        _startOptions.BuyHealthUpgrade  += () => { TryToPurchase(UpgradeManager.UpgradeType.HEALTH);    };
        _startOptions.BuyOrbitalUpgrade += () => { TryToPurchase(UpgradeManager.UpgradeType.ORBITAL);   };
        _startOptions.BuyUnlockForNextStage += TryToBuyStageUnlock;

        _startOptions.AccessAchievements += ShowAchievements;
        _startOptions.AccessLeaderBoards += ShowLeaderBoards;

        _startOptions.OnUpgradesMenuToggled += UpgradesRefresh;
        _startOptions.OnOptionsMenuToggled += OptionsRefresh;
        _startOptions.OnChooseStageMenuToggled += ChooseStageRefresh;

        _startOptions.WatchAnAd += ShowRewardedAd;
    }
    
    void InitialiseUpgradeManager()
    {
        _upgradeManager.Initialise(DeductMoney,GetCurrentMoney, GetHighestLevelReached, ()=> { return IsPayingPlayer; });
    }
    
    void InitialiseIAP()
    {
        _myIAPManager.Initialise(
            PurchaseUnlockFullGameSuccess,
            PurchaseShardsSuccess,
            PurchaseUnlockFullGameFailed,
            PurchaseShardsFailed);
    }

    /// <summary>
    /// If Backup time is 0 or less, we do not use it. The IEnumerator will just keep checking till it is true.
    /// </summary>
    /// <param name="inIsSaveRunProgress"></param>
    /// <param name="inBoolFuncToCheckAsTrigger"></param>
    /// <param name="inBackupTimeToRunAction"></param>
    void BeginEndOfGameplayLevel(bool inIsSaveRunProgress, Func<bool> inBoolFuncToCheckAsTrigger, float inBackupTimeToRunAction = 2)
    {
        _sceneLoadingOperation = SceneManager.LoadSceneAsync(_gameplaySceneIndex);
        _sceneLoadingOperation.allowSceneActivation = false;
        _sceneEndingIEnumerator = RunActionWhenDelegateTrue(
                            () => TriggerEndGameplayLevel(inIsSaveRunProgress, true),
                            inBoolFuncToCheckAsTrigger,
                            0.1f,
                            inBackupTimeToRunAction);

        if(!IsScreenCompletedFadedOut())
            FadeOut();
    }

    void TriggerEndGameplayLevel(bool inIsSaveRunProgress, bool inIsPerformGC)
    {
        _sceneLoadingOperation.allowSceneActivation = true;

        if (inIsSaveRunProgress)
            SaveRunProgression();

        if (inIsPerformGC)
            System.GC.Collect();
    }

    #region ======================MAIN MENU======================

    public void TransitionToMainMenu()
    {
        SceneManager.sceneLoaded += SwapUIToMainMenuMode;
        SceneManager.sceneLoaded -= SwapUIToGameMode;

        //Prep to start main menu when the scene has reloaded.
        _isInGameplay = false;
        Time.timeScale = 1;
    }

    void UpgradesRefresh(bool inIsTryingToOpenUpgradesMenu)
    {
        if (DoWeHaveSavedGameplayRunData && inIsTryingToOpenUpgradesMenu)
        {
            ShowDialogBox("Warning","You have an Ongoing Game.\n\n Accessing Upgrades will clear your progress and you will lose all Shards earned.\n\n\n Do you want to Continue?",
                () =>
                {
                    ClearRunProgression();
                    UpgradesRefresh(inIsTryingToOpenUpgradesMenu);
                    _startOptions.RefreshAvailableMainMenuButtons(false,_upgradeManager.DoWeHaveAnyUpgradesToBuy);
                },
                null,true,true,null,null,null,null,null,null,false);

            return;
        }

        if (inIsTryingToOpenUpgradesMenu)
        {
            _isInUpgradesMenuPurchaseArea = true;
            _startOptions.RefreshMoneyCount(_currentMoney);
            _startOptions.RefreshHighestLvl(_highestLevelReached);
            _startOptions.RefreshHiScore(_hiScore);

            Array a = Enum.GetValues(typeof(UpgradeManager.UpgradeType));
            foreach (UpgradeManager.UpgradeType upgradeType in a)
            {
                int numOfUpgrades = _upgradeManager.GetOwnedNumberOf(upgradeType);

                //Debug.Log(upgradeType.ToString() + " " + numOfUpgrades);

                _startOptions.RefreshUpgradesCount(
                    upgradeType,
                    numOfUpgrades,
                    _upgradeManager.GetMaxNumberOf(upgradeType),
                    _upgradeManager.GetCostOf(upgradeType),
                    _currentMoney,
                    _upgradeManager.GetUnlockLevel(upgradeType),
                    _highestLevelReached,
                    _upgradeManager.GetPurchaseText(upgradeType));
            }

            if(isAdAvailable() && !IsPayingPlayer)
            {
                int amountOfShardsToReward = Mathf.Clamp(_highestLevelReached * 25 ,25, 1000);  //highest level being 29

                AdRewardResult = () => { _currentMoney = Mathf.Clamp(_currentMoney + amountOfShardsToReward, 0, int.MaxValue); UpgradesRefresh(true); };
                _startOptions.RefreshWatchAdsButton(true, amountOfShardsToReward);
            }
            else
            {
                _startOptions.RefreshWatchAdsButton(false,0);
            }


            _startOptions.RefreshUpgradesMenu(true);
        }
        else
        {
            _isInUpgradesMenuPurchaseArea = false;

            _startOptions.RefreshUpgradesMenu(false);
            _startOptions.RefreshAvailableMainMenuButtons(DoWeHaveSavedGameplayRunData, _upgradeManager.DoWeHaveAnyUpgradesToBuy);
        }
    }

    void OptionsRefresh(bool inIsTryingToOpenOptionsMenu)
    {
        _isInStartMenuPurchaseArea = inIsTryingToOpenOptionsMenu;

        _startOptions.RefreshOptionsMenu(
            inIsTryingToOpenOptionsMenu, 
            !IsPayingPlayer, 
            _soundManager.IsSFXEnabled, 
            _soundManager.IsBGMEnabled,
            Application.version.ToString());
    }

    void ChooseStageRefresh(bool inIsTryingToOpenChooseStageMenu)
    {
        int highestStageReached = GetStageForLevel(_highestLevelReached);
        int highestStageUnlocked = AvailableBoughtStages;
        bool hasReachedMaxStage = highestStageReached >= FINAL_STAGE;
        bool hasUnlockedMaxStage = highestStageUnlocked >= FINAL_STAGE;

        _startOptions.RefreshChooseStageMenu(
            inIsTryingToOpenChooseStageMenu,
            highestStageReached,
            highestStageUnlocked,
            UnlockStagePrice, _currentMoney,
            GetLevelForStage(highestStageReached+1), hasReachedMaxStage && hasUnlockedMaxStage);
    }

    void DeductMoney(int inCostOfPurchase)
    {
        _currentMoney -= inCostOfPurchase;
    }

    void TryToBuyStageUnlock()
    {
        int price = UnlockStagePrice;
        if (_currentMoney >= price)
        {
            _currentMoney -= price;
            AvailableBoughtStages++;

            ChooseStageRefresh(true);
        }
        else
        {
            ShowToast("Insufficient Shards");
        }
    }

    void TryToPurchase(UpgradeManager.UpgradeType inUpgradeType)
    {
        bool flag = _upgradeManager.AttemptToBuy(inUpgradeType);

        if (flag)
        {
            ShowToast("You bought an upgrade!");

            SavePermanentProgression();
        }
        else
        {
            Debug.Log("unable to buy : " + inUpgradeType);
        }

        UpgradesRefresh(true);
    }
    
    void ShowAchievements()
    {
        // show achievements UI
        Debug.Log("ShowAchievements");
        if ( !Social.localUser.authenticated)
        {
            _googlePlayGamesManager.CallForAuthenticate(
                () => Social.ShowAchievementsUI(),
                () => ShowToast("Failed to log into Google Play")
                );
        }
        else
        {
            Social.ShowAchievementsUI();
        }
    }

    void ShowLeaderBoards()
    {
        // show achievements UI
        Debug.Log("ShowLeaderboard");
        if (!Social.localUser.authenticated)
        {
            _googlePlayGamesManager.CallForAuthenticate(
                () => Social.ShowLeaderboardUI(),
                () => ShowToast("Failed to log into Google Play")
                );
        }
        else
        {
            Social.ShowLeaderboardUI();
        }
    }

    #endregion

    #region ======================GAMEPLAY HANDLING======================

    void CallToStartGame(StartGameMode inStartMode, int inStageChosen)
    {
        if (_sceneLoadingOperation == null)
        {
            TransitionToGameplay(_gameplaySceneIndex, inStartMode, inStageChosen);
            BeginEndOfGameplayLevel(false, IsScreenCompletedFadedOut);
        }
    }

    void TransitionToGameplay(int inGameplayLevelNumber, StartGameMode inStartMode, int inStageNumChosen = 1)
    {
        SceneManager.sceneLoaded -= SwapUIToMainMenuMode;
        SceneManager.sceneLoaded += SwapUIToGameMode;

        Time.timeScale = 1;
        _isInGameplay = true;
        _allowPlayerControl = false;
        _isGameTransitioningBetweenLevels = true;
        _isPaused = false;

        SceneManager.sceneLoaded += PerformStartGameplayLevelSequence;

        switch (inStartMode)
        {
            case StartGameMode.CONTINUE_GAME_WITHOUT_TUTORIAL:
                LoadRunProgression();
                
                break;

            case StartGameMode.NEW_GAME_WITH_TUTORIAL:
                ClearRunProgression();
                _currentRunSeed = _unknownSeededRandom.Next();
                UnityEngine.Random.InitState(_currentRunSeed);

                _currentLevel = 0;
                break;
            case StartGameMode.NEW_GAME_WITHOUT_TUTORIAL:
            default:
                ClearRunProgression();
                _currentRunSeed = _unknownSeededRandom.Next();
                UnityEngine.Random.InitState(_currentRunSeed);

                _currentLevel = GetLevelForStage(inStageNumChosen);
                break;
        }

#if UNITY_EDITOR
        if (_skipNormalLoading)
        {
            _currentLevel = savedLevel;
            return;
        }
#endif
    }

    void SwapUIToGameMode(Scene from, LoadSceneMode mode)
    {
        if (_isInGameplay)
        {
            _startOptionsCanvas.enabled = false;
            _gameplayUIManagerCanvas.enabled = true;
        }
    }
    
    void SwapUIToMainMenuMode(Scene from, LoadSceneMode mode)
    {
        if (!_isInGameplay)
        {
            if (!Social.localUser.authenticated && !_hasAttemptedGooglePlayLoginOnce)
            {
                _hasAttemptedGooglePlayLoginOnce = true;

                _googlePlayGamesManager.CallForAuthenticate
                       (ReportUnsentAchievements);
            }
            else
            {
                ReportUnsentAchievements();
            }

            _startOptionsCanvas.enabled = true;
            _gameplayUIManagerCanvas.enabled = false;
            _startOptions.RefreshAvailableMainMenuButtons(DoWeHaveSavedGameplayRunData, _upgradeManager.DoWeHaveAnyUpgradesToBuy);
            
            if(_gamesPlayed > 0 && IsPlayedASufficientAmount && !IsRatedTheGameBefore)
            {
                AskPlayerToRateTheGame();
            }
        }
    }

    void ReportUnsentAchievements()
    {
        if (ZPlayerPrefs.GetInt(_permProgressionKeys[15], 0) == 2)
            ReportGooglePlayAchievements(6);

        if (ZPlayerPrefs.GetInt(_permProgressionKeys[16], 0) == 2)
            ReportGooglePlayAchievements(12);

        if (ZPlayerPrefs.GetInt(_permProgressionKeys[17], 0) == 2)
            ReportGooglePlayAchievements(18);

        if (ZPlayerPrefs.GetInt(_permProgressionKeys[18], 0) == 2)
            ReportGooglePlayAchievements(24);

        if (ZPlayerPrefs.GetInt(_permProgressionKeys[19], 0) == 2)
            ReportGooglePlayAchievements(30);
    }
    #endregion

    #region ======================START LEVEL CODE======================
    //Starting the level
    //=========================================
    void PerformStartGameplayLevelSequence(Scene from, LoadSceneMode mode)
    {
        Debug.Log("1. Loaded Scene: " + from.name);
        Debug.Log("2. Current Scene: "+SceneManager.GetActiveScene().name);
        if (_sceneStartingIEnumerator != null) return;
        
        InitialiseStartOfLevel();

        //We can insert a transtion feeling later on 
        //=========================================

        _sceneStartingIEnumerator = RunActionWhenDelegateTrue(
           TriggerStartGameplayLevel,
           IsPlayerReadyToStartLevel,
           0.1f,
           _maxTimeToStartScene);
    }

    Action _onTriggerOfLevelStart;
    /// <summary>
    /// Return control to the player and officially start the level
    /// </summary>
    void TriggerStartGameplayLevel()
    {
        if (_onTriggerOfLevelStart != null)
            _onTriggerOfLevelStart();

        _sceneStartingIEnumerator = null;
        _isGameTransitioningBetweenLevels = false;
        _isGameOverAccepted = false;

        if (IsTutorialStage)
        {
            _gameplayUIManager.ToggleMinimalGameplayUI(true);
        }
        else if(IsTransitionLevel)
        {
            if (!HasWonFinalLevel)
                TransitionLevelComplete();  //skip stopping
            else
                _playerAvatar.TakeDamage(_playerAvatar.MaxHealth);  //Kill the player to trigger gameover

            return;
        }
        else
        {
            _gameplayUIManager.ToggleFullGameplayUI(true);
        }
        
        _playerAvatarRigidBody2D.gravityScale = DefaultPlayerGravityScale;
        _playerAvatarRigidBody2D.velocity = Vector2.zero;
        _playerAvatarCollider.enabled = true;
        _allowPlayerControl = true;
    }

    void InitialiseStartOfLevel()
    {
        _sceneEndingIEnumerator = null;

        InitialiseBackGround(true,_currentLevel);
        InitialisePlayer();

        if (IsTutorialStage)
        {
            SetUpTutorial();

            RefreshUI(UIMode.TUTORIAL_STAGE);
        }
        else if(IsTransitionLevel)
        {
            //Restore to full life
            Player.Health = Player.MaxHealth;
            if (HasWonFinalLevel)
            {
                RefreshUI(UIMode.YOU_WIN_STAGE);
            }
            else
            {
                RefreshUI(UIMode.TRANSITION_STAGE);
            }
        }
        else
        { 
            if(IsFirstStage)
                Player.Health = Player.MaxHealth;
            
            //Spawn env
            //===================================
            CreateEnvironment(
                _environmentTemplates,
                _environmentSpawner,
                GetStageForLevel(_currentLevel),
                _currentLevel, 
                _currentRunSeed * _currentLevel, 
                _lastUsedTopEnvTemplate, _lastUsedBtmEnvTemplate, 
                out _currentTopEnvTemplate, out _currentBtmEnvTemplate);

            //===================================

            //Spawn Enemies
            //===================================
            CreateEnemies(
                _enemyTemplates, 
                _enemySpawner,
                GetStageForLevel(_currentLevel),
                _currentLevel,
                _currentRunSeed * _currentLevel,
                _lastUsedTopEnemyTemplate, _lastUsedBtmEnemyTemplate,
                out _currentTopEnemyTemplate, out _currentBtmEnemyTemplate);

            //===================================

            RefreshUI(UIMode.FULL_STAGE);
        }
    }

    void SetUpTutorial()
    {
        _tutorialObject = Instantiate(_tutorialPrefab);


        _onTriggerOfLevelStart += _tutorialObject.DisplayDraggingGuide;

        _dragFire.OnAimStart += _tutorialObject.DisplayAimAndSlowGuide;
        _dragFire.OnAimEnd += _tutorialObject.DisplayEnergyGuide;
    }

    void CleanUpTutorial()
    {
        _onTriggerOfLevelStart -= _tutorialObject.DisplayDraggingGuide;

        _dragFire.OnAimStart -= _tutorialObject.DisplayAimAndSlowGuide;
        _dragFire.OnAimEnd -= _tutorialObject.DisplayEnergyGuide;
    }

    void InitialiseBackGround(bool inIsBGActive, int inCurrentLevel)
    {
        if (inIsBGActive)
        {
            int wantedlevel = Mathf.Clamp(GetStageForLevel(inCurrentLevel) - 1, 0, _BGsForLevels.Count-1);

            BGAndLevelRange wantedBGSet = _BGsForLevels[wantedlevel];
            
            BGImage.sprite = Resources.Load<Sprite>(wantedBGSet.BGResourceName);

            _gameplayUIManager.SetAimAreaColor(wantedBGSet.AimAreaColor);
        }

        BGImage.enabled = inIsBGActive;
    }

    [SerializeField]
    GameObject _orbitalGuardianPrefab;
    void InitialisePlayer()
    {
        //===========

        if (_playerAvatar == null)
            _playerAvatar = Instantiate(_playerAvatarPrefab, GetTheClosestOffScreenPositionFromBottom(), _playerAvatarPrefab.transform.rotation);
        if (_playerAvatarRigidBody2D == null)
            _playerAvatarRigidBody2D = _playerAvatar.GetComponent<Rigidbody2D>();
        if (_playerDamager == null)
            _playerDamager = _playerAvatar.GetComponent<Damager>();
        if (_playerAvatarCollider == null)
            _playerAvatarCollider = _playerAvatar.GetComponent<Collider2D>();
        if(_playerAvatarSpriteRenderer == null)
            _playerAvatarSpriteRenderer = _playerAvatar.GetComponent<SpriteRenderer>();

        _dragFire.LaunchedObject2D = _playerAvatarRigidBody2D;

        _playerAvatar.MaxHealth             =   _upgradeManager.GetUpgradedPlayerHealth(_playerMaxHealthBase);
        _playerMaxEnergy                    =   _upgradeManager.GetUpgradedPlayerMaxEnergy(_playerMaxEnergyBase);
        _playerEnergyRegenPerSec            =   _upgradeManager.GetUpgradedPlayerEnergyRegen(_playerEnergyRegenBase);
        _playerHealthRegainAfterEachLevel   =   _upgradeManager.GetUpgradedPlayerHealthRegainAfterEachLevel(_playerHealthRegainAfterEachLevelBase);
        _playerDamager.DmgValue             =   _upgradeManager.GetUpgradePlayerDMG(_playerDMGBase);
        int totalNumOfOrbitals              = _upgradeManager.GetUpgradedPlayerOrbitals(_playerOrbitalCountBase);
        for (int i=0; i< totalNumOfOrbitals; i++)
        {
            GameObject orbitalInstance = Instantiate(_orbitalGuardianPrefab,_playerAvatar.transform.position, Quaternion.Euler(0, 0, 360 / totalNumOfOrbitals * i));

            orbitalInstance.transform.SetParent(_playerAvatar.transform);
        }

        _playerDamager.OnDamageOtherObject += (x, y) => { _playerCurrentEnergy += _upgradeManager.GetUpgradedPlayerEnergyOnHit(_playerEnergyOnHitBase); };
        _playerDamager.OnDamageOtherObject += (x, y) =>  AddScoreMultiplier();
        _playerDamager.OnDamageOtherObject += (x, y) => PlayPlayerHitSomethingSound();
        if (allowDMGSlowDown)
            _playerDamager.OnDamageOtherObject += (x, y) => FrameSlowDown();

        _playerAvatar.OnDeath += InitialiseGameOver;
        _playerAvatar.OnTakeDamage += () => { _gameplayUIManager.RefreshHealthAmount(_playerAvatar.Health, _playerAvatar.MaxHealth); };
        _playerAvatar.OnTakeDamage += PlayPlayerGotHurtSound;

        if (IsTutorialStage)
            _playerAvatar.OnTakeDamage += () => { _playerAvatar.Health++; };    //We dont allow the player to die during the tutorial

        if (allowDamageInvulnerability)
            _playerAvatar.OnTakeDamage += () => _playerAvatar.invulnerabletimer += playerOnDamagedInvulnerabilityTimerGain;

        //Set up for player
        //===========

        _playerAvatarCollider.enabled = false;
        _playerAvatarRigidBody2D.gravityScale = -0.5f;
        _playerAvatarRigidBody2D.velocity = Vector2.zero;
        _playerAvatarRigidBody2D.position = GetTheClosestOffScreenPositionFromBottom();
        _allowPlayerControl = false;
        _playerCurrentEnergy = _playerMaxEnergy;
        _playerAvatar.Health = _lastKnownPlayerHealth;

        //===========
    }
   
    void RefreshUI(UIMode inUIInitialiseMode)
    {
        _gameplayUIManager.SetEnergyMeterFollowTarget(PlayerAvatarTransform);

        switch (inUIInitialiseMode)
        {
            case UIMode.TUTORIAL_STAGE:
                break;
            case UIMode.TRANSITION_STAGE:
                _gameplayUIManager.RefreshStageName(_currentLevel.ToString());
                _gameplayUIManager.RefreshExtraInfo("Next Stage\n\nHealth\nRefreshed");
                break;
            case UIMode.YOU_WIN_STAGE:
                _gameplayUIManager.ShowYouWin();
                _gameplayUIManager.RefreshPauseUI(false);
                break;
            case UIMode.FULL_STAGE:
            default:
                _gameplayUIManager.RefreshStageName(_currentLevel.ToString());
                _gameplayUIManager.RefreshScore(_currentScore);
                _gameplayUIManager.RefreshScoreMultiplier(_currentScoreMultiplier);
                _gameplayUIManager.RefreshHealthAmount(_playerAvatar.Health, _playerAvatar.MaxHealth);
                _gameplayUIManager.RefreshPauseUI(false);
                break;
        }
        
        _isGameOverAccepted = false;
    }

    void CreateEnvironment(
        List<LevelTemplate> inEnvironmentLevelTemplates, 
        ObjectSpawner inEnvironmentSpawner,
        int inCurrentDifficulty,
        int inCurrentLevel,
        int inRandomSeed,
        int inTopPosToExclude,
        int inBtmPosToExclude,
        out int outTopPosUsed,
        out int outBtmPosUsed
        )
    {
        //What is unique/constant to the level? What is unique/constant to the player? We want player to be able to resume... or not?
        System.Random inRandomObj = new System.Random(inRandomSeed);

        Debug.Log("environ: " + inRandomSeed + " ==> " + inRandomObj.Next());

        List<Transform> locationsToSpawn;
        List<GameObject> prefabsToSpawn;
        
        LevelTemplate wantedLevelTemplate = inEnvironmentLevelTemplates.Find((x) => x.BaseDifficulty == inCurrentDifficulty);
        if (wantedLevelTemplate == null)
        {
            wantedLevelTemplate = inEnvironmentLevelTemplates[inEnvironmentLevelTemplates.Count - 1];
        }

        wantedLevelTemplate.GetGameObjectGroupsExcluding(
            out locationsToSpawn, 
            out prefabsToSpawn, 
            inTopPosToExclude, 
            inBtmPosToExclude,
            out outTopPosUsed, 
            out outBtmPosUsed, 
            inRandomObj);

        List<Transform> listOfTransforms = prefabsToSpawn.ConvertAll((x) => x.transform);

        inEnvironmentSpawner.SpawnObjects(locationsToSpawn, listOfTransforms, true);
    }

    void CreateEnemies(
        List<LevelTemplate> inEnemyLevelTemplates, 
        EnemySpawner inEnemySpawner, 
        int inCurrentDifficulty,
        int inCurrentLevel,
        int inRandomSeed,
        int inTopPosToExclude,
        int inBtmPosToExclude,
        out int outTopPosUsed,
        out int outBtmPosUsed)
    {
        //What is unique/constant to the level? What is unique/constant to the player? We want player to be able to resume... or not?
        System.Random inRandomObj = new System.Random(inRandomSeed);

        Debug.Log("enemies: " + inRandomSeed + " ==> " + inRandomObj.Next());

        List<Transform> locationsToSpawn;
        List<GameObject> prefabsToSpawn;
        
        LevelTemplate wantedLevelTemplate = inEnemyLevelTemplates.Find((x) => x.BaseDifficulty == inCurrentDifficulty);
        if (wantedLevelTemplate == null)
        {
            wantedLevelTemplate = inEnemyLevelTemplates[inEnemyLevelTemplates.Count - 1];
        }

        wantedLevelTemplate.GetGameObjectGroupsExcluding(
            out locationsToSpawn,
            out prefabsToSpawn,
            inTopPosToExclude,
            inBtmPosToExclude,
            out outTopPosUsed,
            out outBtmPosUsed,
            inRandomObj);

        List <Enemy> listOfEnemies = prefabsToSpawn.ConvertAll((x) => x.GetComponent<Enemy>());

        inEnemySpawner.SpawnObjects(locationsToSpawn, listOfEnemies, false);
    }
    
    Vector2 GetTheClosestOffScreenPositionFromBottom()
    {        
        return (Vector2)Camera.main.ViewportToWorldPoint(new Vector2(0.5f,0.0f)) - new Vector2(0, _playerAvatarHeightFromPivotToEdge);
    }

    bool IsPlayerReadyToStartLevel()
    {
        if (_playerAvatar == null || _playerAvatarRigidBody2D == null)
            return false;

        Vector2 screenPos = _gameCamera.WorldToViewportPoint(_playerAvatarRigidBody2D.position);

        //the player needs to be high enough
        return screenPos.y >= _minScreenHeightProportionBeforeStart;
    }

    #endregion

    #region ======================END LEVEL CODE======================
    
    void PerformEndGameplayLevelSequence(int inGameplaySceneIndex, EndLevelMode inEndLevelMode)
    {
        if (_sceneEndingIEnumerator != null) return;
        
        InitialiseEndOfLevel();

        _totalPlayDurationSeconds = RecordPlayDuration(_totalPlayDurationSeconds, ref _lastPlayTimeRecorded);

        switch (inEndLevelMode)
        {
            case EndLevelMode.NEXT_LEVEL:
                BeginEndOfGameplayLevel(true, () => { return IsPlayerAboveScreen() && IsScreenCompletedFadedOut(); }, _maxTimeToEndScene);
                break;
            case EndLevelMode.GAME_OVER:
                BeginEndOfGameplayLevel(false, () => { return _isGameOverAccepted && IsScreenCompletedFadedOut(); }, 0);
                break;
            case EndLevelMode.QUIT_GAME:
                BeginEndOfGameplayLevel(true, () => { return IsScreenCompletedFadedOut(); });
                break;
            default:
                break;
        }
    }

    void CleanupGameplayRun()
    {
        SceneManager.sceneLoaded -= PerformStartGameplayLevelSequence;
        _isGameTransitioningBetweenLevels = true;
        _isPaused = false;

        //====

        if (_sceneStartingIEnumerator != null)
        {
            StopCoroutine(_sceneStartingIEnumerator);
            _sceneStartingIEnumerator = null;
        }

        if (_sceneEndingIEnumerator != null)
        {
            StopCoroutine(_sceneEndingIEnumerator);
            _sceneEndingIEnumerator = null;
        }

        if (_restoreSpeedCoroutine != null)
        {
            StopCoroutine(_restoreSpeedCoroutine);
            _restoreSpeedCoroutine = null;
        }

        _gameplayUIManager.ToggleFullGameplayUI(false);
        //====
    }

    void InitialiseEndOfLevel()
    {
        if (IsTutorialStage)
            CleanUpTutorial();

        _playerAvatarRigidBody2D.gravityScale = -0.5f;
        _playerAvatarRigidBody2D.velocity = Vector2.zero;
        _playerAvatarCollider.enabled = false;
        _allowPlayerControl = false;
        _dragFire.ToggleAimingUI(false);
        _gameplayUIManager.ToggleFullGameplayUI(false);
        
        AimEnd();        
    }
    
    bool IsPlayerAboveScreen()
    {
        Vector2 screenPos = _gameCamera.WorldToViewportPoint(_playerAvatarRigidBody2D.position + new Vector2(0, _playerAvatarHeightFromPivotToEdge));

        //Player higher than the screen
        bool isPlayerAboveCamera = screenPos.y > 1;

        return isPlayerAboveCamera;
    }
    
    void GameOverAccepted()
    {
        Debug.Log("gameover accepted");
        _isGameOverAccepted = true;
        _isInGameOverMenuPurchaseArea = false;
    }

    void GoToNextLevel()
    {
        //A safety check in case the player dies while winning.
        if (_playerAvatar.Health <= 0)
        {
            InitialiseGameOver();
            return;
        }

        if (_isGameTransitioningBetweenLevels) return;
        _isGameTransitioningBetweenLevels = true;
                
        //Set up for next level
        //===============================================
        //We record down the used positions so we do not repeat them
        _lastUsedTopEnvTemplate = _currentTopEnvTemplate;
        _lastUsedBtmEnvTemplate = _currentBtmEnvTemplate;

        _lastUsedTopEnemyTemplate = _currentTopEnemyTemplate;
        _lastUsedBtmEnemyTemplate = _currentBtmEnemyTemplate;

        _currentLevel++;

        if (_highestLevelReached < _currentLevel)
            _highestLevelReached = _currentLevel;

        _lastKnownPlayerHealth = _playerAvatar.Health + _playerHealthRegainAfterEachLevel;
        //===============================================

        PerformEndGameplayLevelSequence(_gameplaySceneIndex, EndLevelMode.NEXT_LEVEL);
    }
    
    void InitialiseGameOver()
    {
        if (_isGameTransitioningBetweenLevels) return;
        _isGameTransitioningBetweenLevels = true;

        if (_currentScore > _hiScore)
        {
            Social.ReportScore(_hiScore, _leaderBoardID, (bool success) =>
            {
                Debug.Log("Hi score post: " + success);
            });
        }

        _gamesPlayed++;
        _gamesPlayedWithoutSeeingAds++;

        _isGameOverAccepted = false;

        _playerAvatarRigidBody2D.gravityScale = 0;
        _playerAvatarRigidBody2D.velocity = Vector2.zero;
        _playerAvatarCollider.enabled = false;
        _playerAvatarSpriteRenderer.enabled = false;
        _playerAvatar.enabled = false;
        _allowPlayerControl = false;
                
        TransitionToMainMenu();
        CleanupGameplayRun();

        RunActionAfterDelay(this, () => PerformEndGameplayLevelSequence(_mainMenuSceneIndex, EndLevelMode.GAME_OVER), 1);
        RunActionAfterDelay(this, DisplayGameOverOptions, 1.5f);
    }

    void DisplayGameOverOptions()
    {
        if(IsPayingPlayer)
        {
            ShowDoubleShardsEarnedInfoBox();
        }
        else if (isAdAvailable())
        {
            AdRewardResult = ShowDoubleShardsEarnedInfoBox;

            //Is it time to show the player a forced Ad?
            if (IsPlayedASufficientAmount && _gamesPlayedWithoutSeeingAds > 2)
            {
                ShowRewardedAd();
            }
            else
            {
                _isInGameOverMenuPurchaseArea = true;
                
                ShowDialogBox(
                    "Game Over",
                    null,
                    ShowRewardedAd,
                    ShowSingleShardsEarnedInfoBox, 
                    true, false, 
                    "Watch Ad x2 Shards", 
                    "No Thanks",
                    GetScoreText(_currentScore, _hiScore),
                    "Shards Earned",
                    _currentScore.ToString()+"\n\n",
                    (_currentScore/2).ToString()+"\n\n",true);
            }
        }
        else
        {
            ShowSingleShardsEarnedInfoBox();
        }
    }

    void ShowDoubleShardsEarnedInfoBox()
    {
        ShowInfoBox(
                "\nGame Over\n\n",
                null,
                () => {

                    _currentMoney = Mathf.Clamp(_currentMoney + _currentScore, 0, int.MaxValue);

                    if (_currentScore > _hiScore)
                        _hiScore = _currentScore;

                    _isGameOverAccepted = true;
                    ClearRunProgression();
                    SavePermanentProgression();

                    _gamesPlayedWithoutSeeingAds = 0;
                },
                true,
                GetScoreText(_currentScore, _hiScore),
                    "x2 Shards Earned",
                    _currentScore.ToString() + "\n\n",
                    _currentScore.ToString() + "\n\n");
    }

    void ShowSingleShardsEarnedInfoBox()
    {
        ShowInfoBox(
                "\nGame Over\n\n",
               null,
                () => {
                    _currentMoney = Mathf.Clamp(_currentMoney + _currentScore / 2, 0, int.MaxValue);

                    if (_currentScore > _hiScore)
                        _hiScore = _currentScore;

                    _isGameOverAccepted = true;
                    ClearRunProgression();
                    SavePermanentProgression();
                },
                true,
                GetScoreText(_currentScore,_hiScore),
                    "Shards Earned",
                    _currentScore.ToString() + "\n\n",
                    (_currentScore / 2).ToString() + "\n\n");
    }

    string GetScoreText(int inCurrentScore, int inHiScore)
    {
        string scoreText;
        if (inCurrentScore > inHiScore)
        {
            scoreText = "New High Score";
        }
        else
        {
            scoreText = "Score";
        }
        return scoreText;
    }

    #endregion

    private void Update()
    {
        if(TimeBetweenFire > 0)
            _timeTillNextFire = Mathf.Clamp(_timeTillNextFire - Time.deltaTime, 0 , TimeBetweenFire);

        //Regen the energy amount
        if (AllowEnergyRegenWhileAiming || !IsPlayerAiming)
        {
            _playerCurrentEnergy = Mathf.Clamp(_playerCurrentEnergy + _playerEnergyRegenPerSec * Time.deltaTime, 0, _playerMaxEnergy);
        }
        //We show the energymeter is the energy is less than max or the player is aiming
        bool shouldEnergyMeterBeActive = !_isGameTransitioningBetweenLevels && ((_playerCurrentEnergy < _playerMaxEnergy) || IsPlayerAiming);

        _gameplayUIManager.ToggleEnergyMeter(shouldEnergyMeterBeActive);
        if (shouldEnergyMeterBeActive)
            _gameplayUIManager.RefreshEnergyAmount(_playerCurrentEnergy, _playerMaxEnergy);

        if (_lastKnownPlayerAiming != IsPlayerAiming)
        {
            _lastKnownPlayerAiming = IsPlayerAiming;
            _gameplayUIManager.RefreshSlowMoUI(IsPlayerAiming);
        }
    }

    bool IsPlayerAllowedToAim
    {
        get
        {
            return 
                _timeTillNextFire <= 0 && 
                _playerCurrentEnergy >= MinEnergyRequiredToFire && 
                _allowPlayerControl && 
                _isInGameplay;
        }
    }
    
    void DrainEnergy(float inProportionOfMaxEnergyUsed)
    {
        float energyUsed;

        if (FixedEnergyDrain > 0)
            energyUsed = Instance.FixedEnergyDrain;
        else
            if (MinEnergyDrain > 0)
        {
            energyUsed = Mathf.Clamp(inProportionOfMaxEnergyUsed, MinEnergyDrain, _playerMaxEnergy);
        }
        else
            energyUsed = inProportionOfMaxEnergyUsed;

        _playerCurrentEnergy -= energyUsed;
    }

    void FrameSlowDown()
    {
        if (!IsPlayerAiming)
        {
            Time.timeScale = _freezeFrameTimeScale;
            Time.fixedDeltaTime = _freezeFrameTimeScale * _defaultFixedTimeStep;

            if (_restoreSpeedCoroutine != null)
                StopCoroutine(_restoreSpeedCoroutine);

            _restoreSpeedCoroutine = CallForTimeScaleChange(_defaultTimeScale, _defaultFixedTimeStep, _freezeFrameFrameSkips);
        }
    }
    
    void SavePermanentProgression()
    {
        ZPlayerPrefs.SetInt(_permProgressionKeys[0], _hiScore);
        ZPlayerPrefs.SetInt(_permProgressionKeys[1], _currentMoney);
        ZPlayerPrefs.SetInt(_permProgressionKeys[10], _highestLevelReached);
        ZPlayerPrefs.SetInt(_permProgressionKeys[9], _totalPlayDurationSeconds);

        ZPlayerPrefs.SetInt(_permProgressionKeys[12], _soundManager.IsSFXEnabled ? 1 : 0);
        ZPlayerPrefs.SetInt(_permProgressionKeys[13], _soundManager.IsBGMEnabled ? 1 : 0);

        int perm_upgrade_1_heath            = _upgradeManager.GetOwnedNumberOf(UpgradeManager.UpgradeType.HEALTH);
        int perm_upgrade_2_energy           = _upgradeManager.GetOwnedNumberOf(UpgradeManager.UpgradeType.ENERGY);
        int perm_upgrade_3_damage           = _upgradeManager.GetOwnedNumberOf(UpgradeManager.UpgradeType.DMG);
        int perm_upgrade_4_orbital           = _upgradeManager.GetOwnedNumberOf(UpgradeManager.UpgradeType.ORBITAL);

        ZPlayerPrefs.SetInt(_permProgressionKeys[2], perm_upgrade_1_heath);
        ZPlayerPrefs.SetInt(_permProgressionKeys[3], perm_upgrade_2_energy);
        ZPlayerPrefs.SetInt(_permProgressionKeys[4], perm_upgrade_3_damage);
        ZPlayerPrefs.SetInt(_permProgressionKeys[5], perm_upgrade_4_orbital);

        ZPlayerPrefs.Save();
    }
    
    void LoadPermanentProgression()
    {
        _hiScore = ZPlayerPrefs.GetInt(_permProgressionKeys[0], 0);
        _currentMoney = ZPlayerPrefs.GetInt(_permProgressionKeys[1], 0);
        _highestLevelReached = ZPlayerPrefs.GetInt(_permProgressionKeys[10], 0);
        _totalPlayDurationSeconds = ZPlayerPrefs.GetInt(_permProgressionKeys[9], 0);
        _tutorialCompleted = ZPlayerPrefs.GetInt(_permProgressionKeys[14], 0) == 1;
            
        _soundManager.ToggleSFX(ZPlayerPrefs.GetInt(_permProgressionKeys[12], 1) == 1);
        _soundManager.ToggleBGM(ZPlayerPrefs.GetInt(_permProgressionKeys[13], 1) == 1);
        
        int perm_upgrade_1_heath            = ZPlayerPrefs.GetInt(_permProgressionKeys[2], 0);
        int perm_upgrade_2_energy           = ZPlayerPrefs.GetInt(_permProgressionKeys[3], 0);
        int perm_upgrade_3_damage           = ZPlayerPrefs.GetInt(_permProgressionKeys[4], 0);
        int perm_upgrade_4_orbital          = ZPlayerPrefs.GetInt(_permProgressionKeys[5], 0);

        _upgradeManager.SetNumberOfOwned(UpgradeManager.UpgradeType.HEALTH, perm_upgrade_1_heath);
        _upgradeManager.SetNumberOfOwned(UpgradeManager.UpgradeType.ENERGY, perm_upgrade_2_energy);
        _upgradeManager.SetNumberOfOwned(UpgradeManager.UpgradeType.DMG,    perm_upgrade_3_damage);
        _upgradeManager.SetNumberOfOwned(UpgradeManager.UpgradeType.ORBITAL, perm_upgrade_4_orbital);
    }
    
    void ClearRunProgression()
    {
        _currentLevel = 1;
        _currentScore = 0;
        _currentScoreMultiplier = 1;

        SaveRunProgression();
    }

    void SaveRunProgression()
    {
        ZPlayerPrefs.SetInt(_runProgressionKeys[1], _currentLevel);
        ZPlayerPrefs.SetInt(_runProgressionKeys[2], _lastKnownPlayerHealth);
        ZPlayerPrefs.SetInt(_runProgressionKeys[3], _currentScore);
        ZPlayerPrefs.SetFloat(_runProgressionKeys[4], _currentScoreMultiplier);
        ZPlayerPrefs.SetInt(_runProgressionKeys[5], _currentRunSeed);

        ZPlayerPrefs.Save();
    }
    
    void LoadRunProgression()
    {
        _currentLevel               = ZPlayerPrefs.GetInt(_runProgressionKeys[1], _currentLevel);
        _lastKnownPlayerHealth      = ZPlayerPrefs.GetInt(_runProgressionKeys[2], _lastKnownPlayerHealth);
        _currentScore               = ZPlayerPrefs.GetInt(_runProgressionKeys[3], _currentScore);
        _currentScoreMultiplier     = ZPlayerPrefs.GetFloat(_runProgressionKeys[4], _currentScoreMultiplier);

        //Check for the presence of a random seed. If it exists, we use it, otherwise we create a new seed.
        _currentRunSeed = ZPlayerPrefs.GetInt(_runProgressionKeys[5], _unknownSeededRandom.Next());
        UnityEngine.Random.InitState(_currentRunSeed);
    }

    void QuitGameplayRun()
    {
        Debug.Log("quit gameplay");
        _fadeImage.color = Color.black;
        CleanupGameplayRun();
        TransitionToMainMenu();

        PerformEndGameplayLevelSequence(_mainMenuSceneIndex, EndLevelMode.QUIT_GAME);
    }

    void PauseGameToggle()
    {
        _isPaused = !_isPaused;
        _gameplayUIManager.RefreshPauseUI(_isPaused);
        _gameplayUIManager.RefreshTogglePauseMenu(_isPaused,_soundManager.IsSFXEnabled,_soundManager.IsBGMEnabled);

        if (_isPaused)
        {
            _allowPlayerControl = false;

            _fadeTransition.enabled = false;
            _fadeImage.color = default(Color);
            foreach (Enemy e in _enemySpawner.ObjectsAlreadySpawned)
            {
                e.gameObject.SetActive(false);
            }

            Time.timeScale = 0;
        }
        else
        {
            _allowPlayerControl = true;

            _fadeTransition.enabled = true;
            _fadeImage.color = default(Color);
            foreach (Enemy e in _enemySpawner.ObjectsAlreadySpawned)
            {
                e.gameObject.SetActive(true);
            }

            Time.timeScale = 1;
        }
    }

    void AimStart()
    {
        if (IsPlayerAllowedToAim)
        {
            IsPlayerAiming = true;

            Time.timeScale = _aimingSlowDownRate;
            Time.fixedDeltaTime = _aimingSlowDownRate * _defaultFixedTimeStep;

            if (_restoreSpeedCoroutine != null)
                StopCoroutine(_restoreSpeedCoroutine);
        }
    }

    void AimEnd()
    {
        IsPlayerAiming = false;
        Time.timeScale = _defaultTimeScale;
        Time.fixedDeltaTime = _defaultFixedTimeStep;

        _timeTillNextFire = TimeBetweenFire;

        if (_restoreSpeedCoroutine != null)
            StopCoroutine(_restoreSpeedCoroutine);
    }
    
    void AddScore(int inScore)
    {
        _currentScore += Mathf.RoundToInt(inScore * _currentScoreMultiplier);
        _gameplayUIManager.RefreshScore(_currentScore);
    }

    void AddScoreMultiplier()
    {
        _currentScoreMultiplier += _scoreMultiplierGainOnDMG;
        if (_currentScoreMultiplier > _maxScoreMultiplier)
            _currentScoreMultiplier = _maxScoreMultiplier;

        _gameplayUIManager.RefreshScoreMultiplier(_currentScoreMultiplier);
    }

    void RemoveScoreMultiplier()
    {
        _currentScoreMultiplier -= _scoreMultiplierLossOnFire;
        if (_currentScoreMultiplier < _minScoreMultiplier)
            _currentScoreMultiplier = _minScoreMultiplier;

        _gameplayUIManager.RefreshScoreMultiplier(_currentScoreMultiplier);
    }

    void OnDestroy()
    {
        OnApplicationClosing();
    }
    
    void OnApplicationClosing()
    {
        _totalPlayDurationSeconds = RecordPlayDuration(_totalPlayDurationSeconds, ref _lastPlayTimeRecorded);

        SaveRunProgression();
        SavePermanentProgression();
    }

    public void QuitApplication()
    {
        OnApplicationClosing();

        //If we are running in a standalone build of the game
#if UNITY_STANDALONE
		//Quit the application
		Application.Quit();
#endif

        //If we are running in the editor
#if UNITY_EDITOR
        //Stop playing the scene
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Debug.Log("Quttin'");
        Application.Quit();
#endif

    }

    int RecordPlayDuration(int inCurrentPlayDurationSeconds, ref DateTime inLastPlayTimeRecorded)
    {
        inCurrentPlayDurationSeconds = Mathf.Clamp(inCurrentPlayDurationSeconds + (int)(DateTime.Now - inLastPlayTimeRecorded).TotalSeconds, 0, int.MaxValue);

        inLastPlayTimeRecorded = DateTime.Now;

        return inCurrentPlayDurationSeconds;
    }

    IEnumerator RunActionAfterTime(System.Action inAction, float inTime)
    {
        if (inAction == null) return null;

        if (inTime <= 0)
        {
            inAction.Invoke();
            return null;
        }
        else
        {
            IEnumerator ie = RunAfterTime(inAction, inTime);

            StartCoroutine(ie);

            return ie;
        }
    }

    IEnumerator RunAfterTime(System.Action inAction, float t)
    {
        yield return new WaitForSeconds(t);
        inAction.Invoke();
    }

    /// <summary>
    /// If Backup time is 0 or less, we do not use it. The IEnumerator will just keep checking till it is true.
    /// </summary>
    /// <param name="inAction"></param>
    /// <param name="inDelegateCheck"></param>
    /// <param name="inDelegateCheckIntervals"></param>
    /// <param name="inBackupTimeToRunDelegate"></param>
    /// <returns></returns>
    IEnumerator RunActionWhenDelegateTrue(Action inAction, Func<bool> inDelegateCheck, float inDelegateCheckIntervals, float inBackupTimeToRunDelegate)
    {
        IEnumerator ie = RunWhenDelegateTrue(inAction, inDelegateCheck, inDelegateCheckIntervals, inBackupTimeToRunDelegate);

        StartCoroutine(ie);

        return ie;
    }

    /// <summary>
    /// If Backup time is 0 or less, we do not use it. The IEnumerator will just keep checking till it is true.
    /// </summary>
    /// <param name="inAction"></param>
    /// <param name="inDelegateCheck"></param>
    /// <param name="inDelegateCheckIntervals"></param>
    /// <param name="inBackupTimeToRunDelegate"></param>
    /// <returns></returns>
    IEnumerator RunWhenDelegateTrue(Action inAction, Func<bool> inDelegateCheck, float inDelegateCheckIntervals, float inBackupTimeToRunDelegate)
    {
        if (inAction == null) yield break;

        float timer = 0;
        //While we :
        //1. have not reached the backup time or there is no backup time and 
        //2. the delegate check is false, keep yielding 
        while ((inBackupTimeToRunDelegate <= 0 || inBackupTimeToRunDelegate > timer) && !inDelegateCheck())
        {
            timer += inDelegateCheckIntervals;
            yield return new WaitForSeconds(inDelegateCheckIntervals);
        }

        if (inAction != null)
            inAction.Invoke();
    }

    public IEnumerator CallForTimeScaleChange(float inTimeScale, float inDefaultFixedTimeStep, float inDuration)
    {
        IEnumerator IE = SetSpeed(inTimeScale, inDefaultFixedTimeStep, inDuration); ;
        StartCoroutine(IE);

        return IE;
    }

    public IEnumerator CallForTimeScaleChange(float inTimeScale, float defaultFixedTimeStep, int inFrames)
    {
        IEnumerator IE = SetSpeed(inTimeScale, defaultFixedTimeStep, inFrames);
        StartCoroutine(IE);

        return IE;
    }

    IEnumerator SetSpeed(float inTimeScale, float inDefaultFixedTimeStep, float inDelay)
    {
        yield return new WaitForSeconds(inDelay);
        Time.timeScale = inTimeScale;
        Time.fixedDeltaTime = inTimeScale * inDefaultFixedTimeStep;
    }

    IEnumerator SetSpeed(float inTimeScale, float inDefaultFixedTimeStep, int inFrameDelay)
    {
        for (int i = 0; i < inFrameDelay; i++)
            yield return null;

        Time.timeScale = inTimeScale;
        Time.fixedDeltaTime = inTimeScale * inDefaultFixedTimeStep;
    }

    //Make this into a coroutineutility
    public static IEnumerator RunActionAfterDelay(MonoBehaviour inRunningBehaviour, Action inAction, float inDelaySecs)
    {
        IEnumerator ie = RunActionAfterDelayIE(inAction, inDelaySecs);

        inRunningBehaviour.StartCoroutine(ie);

        return ie;
    }

    //Make this into a coroutineutility
    public static IEnumerator RunActionAfterFuncCheckAndDelay(MonoBehaviour inRunningBehaviour, Action inAction, Func<bool> inBoolDelegateCheck, float inTotalDelaySecs)
    {
        IEnumerator ie = RunActionAfterFuncCheckAndDelayIE(inAction, inBoolDelegateCheck, inTotalDelaySecs);

        inRunningBehaviour.StartCoroutine(ie);

        return ie;
    }

    static IEnumerator RunActionAfterDelayIE(Action inAction, float inTotalTime)
    {
        while (inTotalTime >= 0)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            inTotalTime -= Time.deltaTime;
        }
        inAction.Invoke();
    }

    static IEnumerator RunActionAfterFuncCheckAndDelayIE(Action inAction, Func<bool> inBoolDelegateCheck, float inTotalDelaySecs)
    {
        while (inTotalDelaySecs > 0)
        {
            yield return new WaitForSeconds(Time.deltaTime);

            inTotalDelaySecs -= Time.deltaTime;
        }

        while (!inBoolDelegateCheck())
        {
            yield return new WaitForSeconds(Time.deltaTime);
        }

        inAction.Invoke();
    }

    #region PURCHASING

    void PurchaseUnlockFullGameSuccess()
    {
        Debug.Log("full game bought");

        //If we do not already have a purchase saved or somehow we set it to non zero
        if (!ZPlayerPrefs.HasKey(_permProgressionKeys[8]) || ZPlayerPrefs.GetInt(_permProgressionKeys[8]) != 1)
        {
            ZPlayerPrefs.SetInt(_permProgressionKeys[8], 1);
            SavePermanentProgression();

            //Waiting to end the game.
            if (_isInGameOverMenuPurchaseArea)
            {
                ShowInfoBox("\nThank you!\n\n", "No Ads, Free purchases unlocked!\nThanks for the support!\n\n", ShowDoubleShardsEarnedInfoBox, false);
            }
            else
            if (_isInStartMenuPurchaseArea)
            {
                ShowInfoBox("\nThank you!\n\n", "No Ads, Free purchases unlocked!\nThanks for the support!\n\n", () => OptionsRefresh(true));
            }
        }

        ShowToast("Full game unlocked!");
        //Do nothing if that is the case.
    }

    void PurchaseUnlockFullGameFailed()
    {
        //Waiting to end the game.
        if (_isInGameOverMenuPurchaseArea || _isInStartMenuPurchaseArea)
        {
            ShowToast("Purchase failed");
        }
    }

    void PurchaseShardsSuccess()
    {
        Debug.Log("shards bought");

        int boughtShardsAmount = 9000;
        _currentMoney = Mathf.Clamp(_currentMoney + boughtShardsAmount, 0, int.MaxValue);

        if (_isInUpgradesMenuPurchaseArea)
            ShowInfoBox("\nThank you!\n\n", "You got "+ boughtShardsAmount + " Shards!\n\n",()=>UpgradesRefresh(true));
        
        SavePermanentProgression();

        ShowToast("Shards purchased!");
    }

    void PurchaseShardsFailed()
    {
        ShowToast("Purchase Shards failed");
    }
    
    public void WaitingForPurchase()
    {
        ShowToast("Waiting for purchase");
    }

    #endregion

    #region ADBehaviour

    Action AdRewardResult;

    bool isAdAvailable()
    {
        return Advertisement.IsReady("rewardedVideo");
    }

    void ShowRewardedAd()
    {
        if (Advertisement.IsReady("rewardedVideo"))
        {
            var options = new ShowOptions { resultCallback = HandleShowResult };
            Advertisement.Show("rewardedVideo", options);
        }
        else
        {
            Debug.Log("The ad was not ready.");
        }
    }

    private void HandleShowResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                Debug.Log("The ad was successfully shown.");
                if (AdRewardResult != null)
                {
                    AdRewardResult();
                    AdRewardResult = null;
                }
                
                break;
            case ShowResult.Skipped:
                Debug.Log("The ad was skipped before reaching the end.");
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                break;
        }
    }
    #endregion

    #region GLOBALCANVAS
    
    bool IsScreenCompletedFadedOut()
    {
        return _fadeImage.color.a == 1;
    }

    bool IsScreenCompletedFadedIn()
    {
        return _fadeImage.color.a == 0;
    }

    void FadeOut()
    {
        if (!_fadeTransition.enabled)
            _fadeTransition.enabled = true;

        _fadeTransition.Rebind();

        _fadeTransition.SetTrigger(_fadeOutHash);
    }

    void FadeIn()
    {
        if(!_fadeTransition.enabled)
            _fadeTransition.enabled = true;

        _fadeTransition.Rebind();

        _fadeTransition.SetTrigger(_fadeInHash);
    }

    [Header("Dialog boxes")]
    [SerializeField]
    CanvasGroup _dialogBox;
    [SerializeField]
    TMPro.TextMeshProUGUI _dialogText;
    [SerializeField]
    TMPro.TextMeshProUGUI _dialogHeader;

    [SerializeField]
    GameObject _iapFullGameButton;

    [SerializeField]
    Button _dialogAccept;
    [SerializeField]
    CanvasGroup _dialogAcceptCanvasGroup;
    [SerializeField]
    TMPro.TextMeshProUGUI _dialogAcceptText;

    [SerializeField]
    Button _dialogCancel;
    [SerializeField]
    CanvasGroup _dialogCancelCanvasGroup;
    [SerializeField]
    TMPro.TextMeshProUGUI _dialogCancelText;

    [SerializeField]
    Button _dialogAcknowledgeAndClose;
    [SerializeField]
    CanvasGroup _dialogAcknowledgeAndCloseCanvasGroup;

    [SerializeField]
    TMPro.TextMeshProUGUI _specialScoreHeader1;
    [SerializeField]
    TMPro.TextMeshProUGUI _specialScore1;
    [SerializeField]
    TMPro.TextMeshProUGUI _specialScoreHeader2;
    [SerializeField]
    TMPro.TextMeshProUGUI _specialScore2;

    public void ShowDialogBox(
        string inHeader,
        string inText,
        Action inAccept,
        Action inCancel,
        bool inCloseOnAccept = true,
        bool inCloseOnCancel = true,
        string inAlternativeAcceptText = null,
        string inAlternativeCancelText = null,
        string inSpecialScoreHeader1 = null,
        string inSpecialScoreHeader2 = null,
        string inSpecialScore1 = null,
        string inSpecialScore2 = null,
        bool inSetIAPButtonActive = false)
    {
        _dialogHeader.SetText(inHeader);
        _iapFullGameButton.SetActive(inSetIAPButtonActive);

        if (String.IsNullOrEmpty(inText))
        {
            _dialogText.gameObject.SetActive(false);
            _specialScoreHeader1.gameObject.SetActive(true);
            _specialScoreHeader2.gameObject.SetActive(true);
            _specialScore1.gameObject.SetActive(true);
            _specialScore2.gameObject.SetActive(true);

            _specialScoreHeader1.SetText(inSpecialScoreHeader1);
            _specialScoreHeader2.SetText(inSpecialScoreHeader2);

            _specialScore1.SetText(inSpecialScore1);
            _specialScore2.SetText(inSpecialScore2);

        }
        else
        {
            _specialScoreHeader1.gameObject.SetActive(false);
            _specialScoreHeader2.gameObject.SetActive(false);
            _specialScore1.gameObject.SetActive(false);
            _specialScore2.gameObject.SetActive(false);

            _dialogText.gameObject.SetActive(true);

            _dialogText.SetText(inText);
        }
        
        _dialogBox.SetActive(true);
        _dialogAcknowledgeAndCloseCanvasGroup.SetActive(false);

        //====================================

        _dialogAcceptCanvasGroup.SetActive(true);
        _dialogAccept.onClick.RemoveAllListeners();
        if (inAlternativeAcceptText != null)
            _dialogAcceptText.SetText(inAlternativeAcceptText);
        else
            _dialogAcceptText.SetText("Ok");

        if (inAccept != null)
            _dialogAccept.onClick.AddListener(() => inAccept());
        if (inCloseOnAccept)
            _dialogAccept.onClick.AddListener(() => _dialogBox.SetActive(false));

        //====================================

        _dialogCancelCanvasGroup.SetActive(true);
        _dialogCancel.onClick.RemoveAllListeners();
        if (inAlternativeCancelText != null)
            _dialogCancelText.SetText(inAlternativeCancelText);
        else
            _dialogCancelText.SetText("Cancel");

        if (inCancel != null)
            _dialogCancel.onClick.AddListener(() => inCancel());
        if (inCloseOnCancel)
            _dialogCancel.onClick.AddListener(() => _dialogBox.SetActive(false));
    }

    public void ShowInfoBox(
        string inHeader, 
        string inText, 
        Action inAccept = null, 
        bool inCloseOnAcknowledge = true,
        string inSpecialScoreHeader1 = null,
        string inSpecialScoreHeader2 = null,
        string inSpecialScore1 = null,
        string inSpecialScore2 = null,
        bool inHasNoButton = false)
    {
        _iapFullGameButton.SetActive(false);
        _dialogHeader.SetText(inHeader);
        
        if (string.IsNullOrEmpty(inText))
        {
            _dialogText.gameObject.SetActive(false);
            _specialScoreHeader1.gameObject.SetActive(true);
            _specialScoreHeader2.gameObject.SetActive(true);
            _specialScore1.gameObject.SetActive(true);
            _specialScore2.gameObject.SetActive(true);

            _specialScoreHeader1.SetText(inSpecialScoreHeader1);
            _specialScoreHeader2.SetText(inSpecialScoreHeader2);

            _specialScore1.SetText(inSpecialScore1);
            _specialScore2.SetText(inSpecialScore2);

        }
        else
        {
            _specialScoreHeader1.gameObject.SetActive(false);
            _specialScoreHeader2.gameObject.SetActive(false);
            _specialScore1.gameObject.SetActive(false);
            _specialScore2.gameObject.SetActive(false);

            _dialogText.gameObject.SetActive(true);

            _dialogText.SetText(inText);
        }

        _dialogBox.SetActive(true);

        _dialogAcknowledgeAndCloseCanvasGroup.SetActive(true);
        _dialogAcceptCanvasGroup.SetActive(false);
        _dialogCancelCanvasGroup.SetActive(false);

        if (inHasNoButton)
        {
            _dialogAcknowledgeAndClose.gameObject.SetActive(false);
        }
        else
        {
            _dialogAcknowledgeAndClose.gameObject.SetActive(true);
            _dialogAcknowledgeAndClose.onClick.RemoveAllListeners();
            if (inAccept != null)
                _dialogAcknowledgeAndClose.onClick.AddListener(() => inAccept());
            if (inCloseOnAcknowledge)
                _dialogAcknowledgeAndClose.onClick.AddListener(() => _dialogBox.SetActive(false));
        }
    }

    [SerializeField]
    Animator _toastAnimator;
    [SerializeField]
    TMPro.TextMeshProUGUI _toastText;
    public void ShowToast(string inText)
    {
        _toastText.SetText(inText);
        _toastAnimator.SetTrigger("fadeinfadeout");
    }

    #endregion

    #region Sounds
    
    public void ToggleSFX(bool inIsOn)
    {
        _soundManager.ToggleSFX(inIsOn);
    }

    public void ToggleBGM(bool inIsOn)
    {
        _soundManager.ToggleBGM(inIsOn);
    }
    
    void PlayPlayerHitSomethingSound()
    {
        _soundManager.PlayGotHitSound();
    }

    void PlayPlayerGotHurtSound()
    {
        _soundManager.PlayGotHitSound();
    }

    void PlayPlayerDestroySound()
    {
        _soundManager.PlayDestroyedEnemySound();
    }

    void PortraitCheck()
    {
        ZPlayerPrefs.Initialize("4aBNMTOwo@@%2", "IvYH12$%16");
    }

    #endregion

    #region ======================EDITOR HACKS======================
#if UNITY_EDITOR

    int savedDiff;
    int savedLevel;

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.M) && !_isGameTransitioningBetweenLevels)
        {
            _playerAvatar.TakeDamage(_playerAvatar.MaxHealth);
        }

        if (Input.GetKeyDown(KeyCode.N) && !_isGameTransitioningBetweenLevels)
        {
            if (IsTutorialStage)
            {
                TutorialComplete();
            }
            else
                if (IsTransitionLevel )
            {
                TransitionLevelComplete();
            }
            else
            {
                _enemySpawner.DestroyAllSpawnedObjects();
            }
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            this._playerAvatar.MaxHealth = 1000;
            this._playerAvatar.Health = 1000;
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            _currentMoney = Mathf.Clamp(_currentMoney + 5000, 0, int.MaxValue);
        }
    }

    [ContextMenu("clearalldata")]
    public void ClearAllSave()
    {
        ZPlayerPrefs.DeleteAll();
    }

#endif
    #endregion
}
