using UnityEngine;

public abstract class FlashRendererBase : MonoBehaviour
{
    [SerializeField]
    float secondsDelayBetweenFlash;

    protected bool _rendererIsFullyVisible;
    float timeCounter;

    protected virtual void OnEnable()
    {
        timeCounter = secondsDelayBetweenFlash;
    }

    protected virtual void OnDisable()
    {
        timeCounter = secondsDelayBetweenFlash;
    }

    private void Update()
    {
        if (timeCounter > 0)
        {
            timeCounter -= Time.deltaTime;
        }
        else
        {
            _rendererIsFullyVisible = !_rendererIsFullyVisible;

            ToggleRenderer(_rendererIsFullyVisible);

            timeCounter = secondsDelayBetweenFlash;
        }
    }

    protected abstract void ToggleRenderer(bool inIsRendererVisible);
}
