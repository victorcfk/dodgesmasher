using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayUIManager : MonoBehaviour {
    
    [Header("Energy UI")]
    [SerializeField]
    RectTransform _energyMeterUI;
    [SerializeField]
    Canvas _energyMeterUICanvas;
    [SerializeField]
    FollowTargetTransform _energyMeterUIFollowTargetTransform;
    [SerializeField]
    RawImage _energyChargingPipsMeter;
    [SerializeField]
    RawImage _energyFullPipsMeter;
    [SerializeField]
    RawImage _energyMaxPipsMeter;
    [SerializeField]
    float _sizePerEnergyPip = 0.5f;

    int _lastKnownNumberOfEnergyPips = 0;
    float _lastKnownParentRectWidth = 0;

    [Space]

    [Header("Health UI")]
    [SerializeField]
    Transform _healthPipsParent;
    [SerializeField]
    CanvasGroup _healthPipsParentCanvasGroup;
    [SerializeField]
    GameObject _healthPipPrefab;
    [SerializeField]
    List<GameObject> _healthEmptyPips;
    [SerializeField]
    List<GameObject> _healthFullPips;
    
    [Header("Area Name UI")]
    [SerializeField]
    TextMeshProUGUI _areaName;
    [SerializeField]
    Animator _areaAnimator;
    [SerializeField]
    TextMeshProUGUI _areaExtraInfo;
    [SerializeField]
    Animator _areaExtraInfoAnimator;

    [Header("Score UI")]
    [SerializeField]
    TextMeshProUGUI _score;
    [SerializeField]
    Animator _scoreAnimator;
    [Space]
    [SerializeField]
    TextMeshProUGUI _scoreMultiplier;
    [SerializeField]
    Animator _scoreMultiplierAnimator;

    [Header("Pause UI")]
    [SerializeField]
    CanvasGroup _pauseMenu;
    [SerializeField]
    Toggle _sfxToggle;
    [SerializeField]
    Toggle _bgmToggle;
    [SerializeField]
    Image _pauseButtonImage;
    [SerializeField]
    Sprite _pauseImage;
    [SerializeField]
    Sprite _resumeImage;

    [Header("Misc.")]
    [SerializeField]
    Canvas SlowMo;
    [Space]

    [SerializeField]
    Image _aimingArea;
        
    public System.Action OnRestart;
    public System.Action OnGameOver;
    public System.Action OnQuit;
    public System.Action OnPauseModeToggled;
    
    bool _initialScoreRefresh = true;
    bool _initialScoreMultiplierRefresh = true;

    int _fadeinoutHash = Animator.StringToHash("fadeinout");
    int _pulseHash = Animator.StringToHash("pulse");
    
    public void SetAimAreaColor(Color inColor)
    {
        _aimingArea.color = inColor;
    }

    public void SetEnergyMeterFollowTarget(Transform inFollowTarget)
    {
        _energyMeterUIFollowTargetTransform.Target = inFollowTarget;
    }

    public void RefreshSlowMoUI(bool inIsSlowMo)
    {
        SlowMo.enabled = inIsSlowMo;
    }

    public void RefreshEnergyAmount(float inEnergy, float inMaxEnergy)
    {
        if (_lastKnownNumberOfEnergyPips != inMaxEnergy)
        {
            _lastKnownParentRectWidth = inMaxEnergy * _sizePerEnergyPip;
            _energyMeterUI.sizeDelta = new Vector2(_lastKnownParentRectWidth, _energyMeterUI.sizeDelta.y);

            _energyMaxPipsMeter.SetScalingUVAsAMeter(_lastKnownParentRectWidth, inMaxEnergy, inMaxEnergy);
        }
        
        _energyChargingPipsMeter.SetScalingUVAsAMeter(_lastKnownParentRectWidth, inEnergy, inMaxEnergy);
        _energyFullPipsMeter.SetScalingUVAsAMeter(_lastKnownParentRectWidth, (int)inEnergy, inMaxEnergy);   //round down as integer values of the bar.
    }

    public void ToggleEnergyMeter( bool inIsActive)
    {
        _energyMeterUICanvas.enabled = inIsActive;
    }

    public void RefreshHealthAmount(int inHealth, int inMaxHealth)
    {
#if UNITY_EDITOR
        if (inHealth > 10) return;
#endif

        if (_healthPipsParent.childCount < inMaxHealth)
        {
            for (int i = _healthPipsParent.childCount; i < inMaxHealth; i++)
            {
                GameObject HealthPip = Instantiate(_healthPipPrefab);

                HealthPip.transform.SetParent(_healthPipsParent, false);

                _healthEmptyPips.Add(HealthPip.transform.GetChild(0).gameObject);
                _healthFullPips.Add(HealthPip.transform.GetChild(1).gameObject);
            }
        }
        else if (_healthPipsParent.childCount > inMaxHealth)
        {
            for (int i = _healthPipsParent.childCount; i >= inMaxHealth; i--)
            {
                _healthEmptyPips.Remove(_healthPipsParent.GetChild(i - 1).transform.GetChild(0).gameObject);
                _healthFullPips.Remove(_healthPipsParent.GetChild(i - 1).transform.GetChild(1).gameObject);

                Destroy(_healthPipsParent.GetChild(i - 1).gameObject);
            }
        }

        for (int i = 0; i < _healthEmptyPips.Count; i++)
        {
            if (i < inHealth)
            {
                _healthFullPips[i].SetActive(true);
                _healthEmptyPips[i].SetActive(false);
            }
            else
            {
                _healthFullPips[i].SetActive(false);
                _healthEmptyPips[i].SetActive(true);
            }
        }
    }

    public void RefreshStageName(string inStageName)
    {
        _areaName.SetText(inStageName);
        if (_areaAnimator.isInitialized)
            _areaAnimator.SetTrigger(_fadeinoutHash);
    }

    public void RefreshExtraInfo(string inExtraInfo)
    {
        _areaExtraInfo.SetText(inExtraInfo);
        if (_areaExtraInfoAnimator.isInitialized)
            _areaExtraInfoAnimator.SetTrigger(_fadeinoutHash);
    }

    public void RefreshScore(int inScore)
    {
        _score.text = inScore.ToString();

        //Do not pulse on the initial score setting
        if (!_initialScoreRefresh)
        {
            if (_scoreAnimator.isInitialized)
                _scoreAnimator.SetTrigger(_pulseHash);
        }
        else
            _initialScoreRefresh = false;
    }

    public void RefreshScoreMultiplier(float inScoreMultiplier)
    {
        _scoreMultiplier.text = "x"+inScoreMultiplier.ToString("F1");

        //Do not pulse on the initial score setting
        if (!_initialScoreMultiplierRefresh)
        {
            if(_scoreMultiplierAnimator.isInitialized)
                _scoreMultiplierAnimator.SetTrigger(_pulseHash);
        }
        else
            _initialScoreMultiplierRefresh = false;
    }

    [SerializeField]
    GameObject _youWinPrefab;
    public void ShowYouWin()
    {
        Instantiate(_youWinPrefab);
    }
    
    public void ToggleMinimalGameplayUI(bool inIsActive)
    {
        _aimingArea.enabled = inIsActive;
    }

    public void ToggleFullGameplayUI(bool inIsActive)
    {
        _aimingArea.enabled = inIsActive;

        _scoreMultiplier.enabled = inIsActive;
        _score.enabled = inIsActive;
        _healthPipsParentCanvasGroup.SetActive(inIsActive);
        _pauseButtonImage.enabled = inIsActive;

        //hack to never set pause menu to full when toggling the UI
        if (!inIsActive)
            _pauseMenu.SetActive(false);
    }

    public void UserInitiatedGameOver()
    {
        if (OnGameOver != null)
            OnGameOver();
    }

    public void UserInitiatedRestart()
    {
        if (OnRestart != null)
            OnRestart();
    }

    public void UserInitiatedTogglePauseMode()
    {
        if (OnPauseModeToggled != null)
            OnPauseModeToggled();
    }

    public void UserInitiatedQuit()
    {
        if (OnQuit != null)
            OnQuit();
    }

    public void RefreshPauseUI(bool inIsPaused)
    {
        if (inIsPaused)
        {
            _pauseButtonImage.sprite = _resumeImage;
        }
        else
        {
            _pauseButtonImage.sprite = _pauseImage;
        }
    }

    public void RefreshTogglePauseMenu(bool inIsToggled, bool inIsSFXEnabled, bool inIsBGMEnabled)
    {
        _sfxToggle.isOn = inIsSFXEnabled;
        _bgmToggle.isOn = inIsBGMEnabled;

        _pauseMenu.SetActive(inIsToggled);
    }

}