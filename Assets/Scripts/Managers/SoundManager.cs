using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    
    public bool IsSFXEnabled { get; private set; }
    public bool IsBGMEnabled { get; private set; }

    [SerializeField]
    AudioSource _sfxAudioSource;
    [SerializeField]
    AudioSource _bgmAudioSource;

    [SerializeField]
    AudioClip _hitEnemySound;
    public void PlayHitSound()
    {
        if (IsSFXEnabled)
            _sfxAudioSource.PlayOneShot(_hitEnemySound);
    }

    [SerializeField]
    AudioClip _playerGotHurt;
    public void PlayGotHitSound()
    {
        if (IsSFXEnabled)
            _sfxAudioSource.PlayOneShot(_playerGotHurt);
    }
    
    [SerializeField]
    AudioClip _destroyedEnemySound;
    public void PlayDestroyedEnemySound()
    {
        if(IsSFXEnabled)
            _sfxAudioSource.PlayOneShot(_destroyedEnemySound);
    }

    [SerializeField]
    AudioClip _bgm;
    public void PlayBGM(bool inPlayBGM)
    {
        if (inPlayBGM)
        {
            if (!_bgmAudioSource.isPlaying)
                _bgmAudioSource.Play();
        }
        else
        {
            if (_bgmAudioSource.isPlaying)
                _bgmAudioSource.Stop();
        }
    }

    public void ToggleBGM(bool inIsOn)
    {
        IsBGMEnabled = inIsOn;

        PlayBGM(inIsOn);
    }

    public void ToggleSFX(bool inIsOn)
    {
        IsSFXEnabled = inIsOn;
    }
}
