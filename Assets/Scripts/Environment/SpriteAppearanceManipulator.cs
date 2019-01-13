using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpriteAppearanceManipulator : MonoBehaviour {

    [SerializeField]
    Color _initColor;

    [SerializeField]
    Color _finalColor;

    [SerializeField]
    Color _snapFinalColor;

    [SerializeField]
    SpriteRenderer _spriteR;

    [SerializeField]
    SpriteMask _spriteM;
    
    [SerializeField]
    UnityEvent ShowAction;
        
    public void StartLerpToFinalColourAcrossTime (float totalTime) {

        StartCoroutine(LerpToColour(_initColor, _finalColor, _spriteR, totalTime, () => { ShowAction.Invoke();}));
        StartCoroutine(LerpSpriteMask(0, 1, _spriteM, totalTime, null));
    }

    public void SnapToFinalColour()
    {
        _spriteR.color = _snapFinalColor;
    }

    IEnumerator LerpToColour(Color initColor, Color finalColor, SpriteRenderer spriteR, float inTotalTime, System.Action OnLerpComplete)
    {
        float timeLeft = inTotalTime;

        while (timeLeft > 0)
        {
            yield return new WaitForSeconds(Time.deltaTime);

            timeLeft -= Time.deltaTime;

            spriteR.color = Color.Lerp(finalColor, initColor, timeLeft / inTotalTime);
        }

        if (OnLerpComplete != null)
            OnLerpComplete.Invoke();

        yield break;
    }

    IEnumerator LerpSpriteMask(float initAlpha, float finalAlpha, SpriteMask spriteM, float inTotalTime, System.Action OnLerpComplete)
    {
        float timeLeft = inTotalTime;

        while (timeLeft > 0)
        {
            yield return new WaitForSeconds(Time.deltaTime);

            timeLeft -= Time.deltaTime;

            spriteM.alphaCutoff = Mathf.Lerp(finalAlpha, initAlpha, timeLeft / inTotalTime);
        }

        if (OnLerpComplete != null)
            OnLerpComplete.Invoke();

        yield break;
    }
}
