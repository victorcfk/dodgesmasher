using UnityEngine;

public class FlashSpriteRenderer : FlashRendererBase
{
    [SerializeField]
    SpriteRenderer _renderer;

    [SerializeField]
    Color _VisibleColor;

    [SerializeField]
    Color _HiddenColor;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();

        ToggleRenderer(false);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();

        ToggleRenderer(true);
    }

    protected override void ToggleRenderer(bool inIsRendererVisible)
    {
        _renderer.color = inIsRendererVisible? (_VisibleColor) : (_HiddenColor);
    }
}
