using UnityEngine;
using System;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class StartOptions : MonoBehaviour {
  
    [SerializeField]
    private string _startLevelName;

    [Header("UI references")]
    [SerializeField]
    private CanvasGroup _allAvailableOptions;
    [SerializeField]
    GameObject _googlePlayButtons;
    [SerializeField]
    private GameObject _quitButton;
    [SerializeField]
    private GameObject _resumeButton;
    [SerializeField]
    private TextMeshProUGUI _resumeTextMesh;

    [SerializeField]
    private GameObject _upgradeButton;
    [SerializeField]
    private TextMeshProUGUI _upgradeTextMesh;
    [SerializeField]
    private Animator _upgradeOutlineFlasher;

    [SerializeField]
    private GameObject _newGameButton;
    [SerializeField]
    private Image _newGameButtonImage;
    [SerializeField]
    private TextMeshProUGUI _newGameTextMesh;
    
    [SerializeField]
    private CanvasGroup _chooseStageMenu;
    [SerializeField]
    private Button[] _chooseStageButtonsAscending;
    [SerializeField]
    private Button _unlockNextStage;
    [SerializeField]
    TextMeshProUGUI _chooseMenuCurrentShards;
    [SerializeField]
    private TextMeshProUGUI _unlockNextStageText;

    [Header("Options Menu")]
    [SerializeField]
    CanvasGroup _optionsMenu;
    [SerializeField]
    Button _iapButton;
    [SerializeField]
    GameObject _thankYouAfterIAPBought;
    [SerializeField]
    Toggle _SFXToggle;
    [SerializeField]
    Toggle _BGMToggle;

    [Header("Upgrades")]
    [SerializeField]
    CanvasGroup _upgradesMenu;
    [SerializeField]
    TextMeshProUGUI _moneyCount;
    [SerializeField]
    TextMeshProUGUI _highestLvl;
    [SerializeField]
    TextMeshProUGUI _hiScore;
    [SerializeField]
    [FormerlySerializedAs("barParent")]
    RectTransform _barParent;
    [SerializeField]
    Button _watchAdsButton;
    [SerializeField]
    TextMeshProUGUI _watchAdsText;

    [Space]
    [SerializeField]
    RawImage _healthUpgrade;
    [SerializeField]
    RawImage _healthUpgradeParent;
    [SerializeField]
    TextMeshProUGUI _healthUpgradeCost;
    [SerializeField]
    TextMeshProUGUI _healthUpgradeDescription;
    [SerializeField]
    Image _healthUpgradeIndicator;
    
    [Space]
    [SerializeField]
    RawImage _energyUpgrade;
    [SerializeField]
    RawImage _energyUpgradeParent;
    [SerializeField]
    TextMeshProUGUI _energyUpgradeCost;
    [SerializeField]
    TextMeshProUGUI _energyUpgradeDescription;
    [SerializeField]
    Image _energyUpgradeIndicator;

    [Space]
    [SerializeField]
    RawImage _damageUpgrade;
    [SerializeField]
    RawImage _damageUpgradeParent;
    [SerializeField]
    TextMeshProUGUI _damageUpgradeCost;
    [SerializeField]
    TextMeshProUGUI _damageUpgradeDescription;
    [SerializeField]
    Image _damageUpgradeIndicator;

    [Space]
    [SerializeField]
    RawImage _orbitalUpgrade;
    [SerializeField]
    RawImage _orbitalUpgradeParent;
    [SerializeField]
    TextMeshProUGUI _orbitalUpgradeCost;
    [SerializeField]
    TextMeshProUGUI _orbitalUpgradeDescription;
    [SerializeField]
    Image _orbitalUpgradeIndicator;
        
    [Space]
    [SerializeField]
    Color _newButtonHighlightColor = new Color(31, 146, 219);
    [SerializeField]
    Color _newButtonDullColor = Color.red;

    [Space]
    [SerializeField]
    TextMeshProUGUI _versionName;

    public Action<GameManager.StartGameMode,int> CallToStartGame;
    public Action<bool> OnOptionsMenuToggled;
    public Action<bool> OnUpgradesMenuToggled;
    public Action<bool> OnChooseStageMenuToggled;
    public Action BuyHealthUpgrade;
    public Action BuyEnergyUpgrade;
    public Action BuyDMGUpgrade;
    public Action BuyOrbitalUpgrade;
    public Action BuyUnlockForNextStage;

    public Action WatchAnAd;

    public Action AccessAchievements;
    public Action AccessLeaderBoards;

    bool _waitingForUpgradeRefreshAfterPurchaseAttempt = true;

    public void RefreshAvailableMainMenuButtons(
        bool inIsContinueDataAvailable, 
        bool inUpgradesCanBeBought)
    {
        _allAvailableOptions.gameObject.SetActive(true);
        _googlePlayButtons.SetActive(true);
        _chooseStageMenu.SetActive(false);

        if (!inIsContinueDataAvailable)
        {
            //continueTextMesh.text = "New Game";
            _resumeButton.SetActive(false);
            _newGameButtonImage.color = _newButtonHighlightColor;
        }
        else
        {
            _resumeButton.SetActive(true);
            _newGameButtonImage.color = _newButtonDullColor;
        }

        if (inUpgradesCanBeBought)
            _upgradeOutlineFlasher.SetTrigger("startflashing");
        else
            _upgradeOutlineFlasher.SetTrigger("stopflashing");
    }

    public void TransitionToGameplay(int inStartModeValue)
    {
        TransitionToGameplay(inStartModeValue, 1);
    }

    public void TransitionToGameplay(int inStartModeValue, int inStageChosen)
    {
        if(CallToStartGame != null)
            CallToStartGame((GameManager.StartGameMode)inStartModeValue, inStageChosen);

        _allAvailableOptions.gameObject.SetActive(false);
        _googlePlayButtons.SetActive(false);
        _chooseStageMenu.SetActive(false);
    }
    
    public void OnStageChoose(int inStageChosen)
    {
        TransitionToGameplay(1, inStageChosen);
    }

    public void OnToggleChooseStageMenu(bool inIsActive)
    {
        OnChooseStageMenuToggled(inIsActive);
    }

    public void OnUnlockNextStageAttempted()
    {
        if (BuyUnlockForNextStage != null)
            BuyUnlockForNextStage();
    }

    public void RefreshUpgradesMenu(bool inIsActive)
    {
        _upgradesMenu.SetActive(inIsActive);
        
        if (inIsActive)
            _waitingForUpgradeRefreshAfterPurchaseAttempt = false;
    }

    public void RefreshOptionsMenu(bool inIsActive, bool isIAPActive,bool inIsSFXEnabled, bool inIsBGMEnabled, string inGameVerName)
    {
        _optionsMenu.SetActive(inIsActive);

        if (inIsActive)
        {
            _iapButton.gameObject.SetActive(isIAPActive);
            _thankYouAfterIAPBought.SetActive(!isIAPActive);    //show thank you if IAP is bought

            _SFXToggle.isOn = inIsSFXEnabled;
            _BGMToggle.isOn = inIsBGMEnabled;
            
            _versionName.SetText("ver."+inGameVerName);
        }
    }

    public void RefreshChooseStageMenu(
        bool inIsActive, 
        int inMaxReachedStage, 
        int inMaxUnlockedStage, 
        int inUnlockStageCost, 
        int inCurrentMoney, 
        int inLevelToReach,
        bool inAllStagesAvailableAndUnlocked)
    {
        _chooseStageMenu.SetActive(inIsActive);

        if (inIsActive)
        {
            if(inMaxReachedStage <= 0)
                OnStageChoose(inMaxReachedStage);

            for (int i = 0; i < _chooseStageButtonsAscending.Length; ++i)
            {
                _chooseStageButtonsAscending[i].gameObject.SetActive(i < inMaxReachedStage && i < inMaxUnlockedStage);
            }

            if (inAllStagesAvailableAndUnlocked)
            {
                _unlockNextStage.gameObject.SetActive(false);
                _chooseMenuCurrentShards.SetText("All Stages unlocked");
            }
            else
            {

                //We have unlocked the stage by playing that far but not paid for it yet
                if (inMaxReachedStage > inMaxUnlockedStage)
                {
                    _unlockNextStage.gameObject.SetActive(true);
                    _unlockNextStageText.SetText(string.Format("Unlock Next Stage {0} Shards", inUnlockStageCost));
                    _chooseMenuCurrentShards.SetText(string.Format("Current Shards: {0}", inCurrentMoney));
                    
                }
                else
                {
                    _unlockNextStage.gameObject.SetActive(false);
                    _chooseMenuCurrentShards.SetText(string.Format("Reach Level {0} for next level unlock", inLevelToReach));
                    
                }
            }
        }
    }

    public void RefreshWatchAdsButton(bool inIsActive, int inRewardAmount)
    {
        _watchAdsButton.gameObject.SetActive(inIsActive);

        if(inIsActive)
        {
            _watchAdsText.SetText("Watch an Ad for " + inRewardAmount.ToString());
        }
    }

    public void OnAdWatchCalled()
    {
        if(WatchAnAd != null)
            WatchAnAd();
    }

    public void OnToggleUpgradesMenu(bool inIsToggled)
    {
        OnUpgradesMenuToggled(inIsToggled);
    }

    public void OnToggleOptionsMenu(bool inIsToggle)
    {
        OnOptionsMenuToggled(inIsToggle);
    }
    
    public void RefreshMoneyCount(int inCount)
    {
        _moneyCount.text = inCount.ToString();
    }

    public void RefreshHighestLvl(int inHighestLvl)
    {
        _highestLvl.text = "Highest Lv.\n" + inHighestLvl;
    }
    
    public void RefreshHiScore(int inHiScore)
    {
        _hiScore.text = "High Score\n" + inHiScore;
    }

    public void RefreshUpgradesCount(
        UpgradeManager.UpgradeType inUpgradeType, 
        int inCurrentAmount, 
        int inMaxAmount, 
        int inCostOfUpgrade, 
        int inCurrentMoney, 
        int inMinUnlockLevel,
        int inCurrentHighestLevel,
        string inUpgradeDescription)
    {
        float parentSize = _barParent.rect.width;
        Color indicatorColor;
        bool haveWeReachedSufficientLevelToUnlock = inCurrentHighestLevel >= inMinUnlockLevel;
        bool haveWeEnoughShards = inCurrentMoney >= inCostOfUpgrade;

        if (haveWeEnoughShards && haveWeReachedSufficientLevelToUnlock)
            indicatorColor = Color.green;
        else
            indicatorColor = Color.red;

        switch (inUpgradeType)
        {
            case UpgradeManager.UpgradeType.ENERGY:
                
                _energyUpgradeIndicator.color = indicatorColor;

                _energyUpgradeParent.SetUVRect(inMaxAmount);
                _energyUpgrade.SetScalingUVAsAMeter(parentSize, inCurrentAmount, inMaxAmount);
                _energyUpgradeCost.text = inCostOfUpgrade.ToString();

                if (haveWeReachedSufficientLevelToUnlock)
                    _energyUpgradeDescription.SetText(inUpgradeDescription);
                else
                    _energyUpgradeDescription.SetText(inUpgradeDescription+"\n(Reach lv. " + inMinUnlockLevel +" to unlock)");

                break;
            case UpgradeManager.UpgradeType.HEALTH:
                
                _healthUpgradeIndicator.color = indicatorColor;

                _healthUpgradeParent.SetUVRect(inMaxAmount);
                _healthUpgrade.SetScalingUVAsAMeter(parentSize, inCurrentAmount, inMaxAmount);
                _healthUpgradeCost.text = inCostOfUpgrade.ToString();

                if (haveWeReachedSufficientLevelToUnlock)
                    _healthUpgradeDescription.SetText(inUpgradeDescription);
                else
                    _healthUpgradeDescription.SetText(inUpgradeDescription + "\n(Reach level " + inMinUnlockLevel + " to unlock)");

                break;
            case UpgradeManager.UpgradeType.DMG:
                
                _damageUpgradeIndicator.color = indicatorColor;

                _damageUpgradeParent.SetUVRect(inMaxAmount);
                _damageUpgrade.SetScalingUVAsAMeter(parentSize, inCurrentAmount, inMaxAmount);
                _damageUpgradeCost.text = inCostOfUpgrade.ToString();

                if (haveWeReachedSufficientLevelToUnlock)
                    _damageUpgradeDescription.SetText(inUpgradeDescription);
                else
                    _damageUpgradeDescription.SetText(inUpgradeDescription + "\n(Reach level " + inMinUnlockLevel + " to unlock)");

                break;
            
            case UpgradeManager.UpgradeType.ORBITAL:
                
                _orbitalUpgradeIndicator.color = indicatorColor;

                _orbitalUpgradeParent.SetUVRect(inMaxAmount);
                _orbitalUpgrade.SetScalingUVAsAMeter(parentSize, inCurrentAmount, inMaxAmount);
                _orbitalUpgradeCost.text = inCostOfUpgrade.ToString();
                
                if (haveWeReachedSufficientLevelToUnlock)
                    _orbitalUpgradeDescription.SetText(inUpgradeDescription);
                else
                    _orbitalUpgradeDescription.SetText(inUpgradeDescription + "\n(Reach level " + inMinUnlockLevel + " to unlock)");

                break;

            case UpgradeManager.UpgradeType.SEGMENT_SKIPS:
                break;
            default:
                break;
        }
    }

    public void OnHealthUpgradeBought()
    {
        if (BuyHealthUpgrade != null && !_waitingForUpgradeRefreshAfterPurchaseAttempt)
        {
            _waitingForUpgradeRefreshAfterPurchaseAttempt = true;
            BuyHealthUpgrade();
        }
    }

    public void OnyEnergyUpgradeBought()
    {
        if (BuyEnergyUpgrade != null && !_waitingForUpgradeRefreshAfterPurchaseAttempt)
        {
            _waitingForUpgradeRefreshAfterPurchaseAttempt = true;
            BuyEnergyUpgrade();
        }
    }

    public void OnDMGUpgradeBought()
    {
        if (BuyDMGUpgrade != null && !_waitingForUpgradeRefreshAfterPurchaseAttempt)
        {
            _waitingForUpgradeRefreshAfterPurchaseAttempt = true;
            BuyDMGUpgrade();
        }
    }

    public void OnOrbitalUpgradeBought()
    {
        if (BuyOrbitalUpgrade != null && !_waitingForUpgradeRefreshAfterPurchaseAttempt)
        {
            _waitingForUpgradeRefreshAfterPurchaseAttempt = true;
            BuyOrbitalUpgrade();
        }
    }

    public void OnAccessAchievements()
    {
        if (AccessAchievements != null) AccessAchievements();
    }

    public void OnAccessLeaderboards()
    {
        if (AccessLeaderBoards != null) AccessLeaderBoards();
    }

}
