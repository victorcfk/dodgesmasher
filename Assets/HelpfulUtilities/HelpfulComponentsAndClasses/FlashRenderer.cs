using System;
using UnityEngine;

public class FlashRenderer : FlashRendererBase
{
    [SerializeField]
    Renderer _renderer;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (_renderer == null)
            _renderer = GetComponent<Renderer>();

        ToggleRenderer(true);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (_renderer == null)
            _renderer = GetComponent<Renderer>();

        ToggleRenderer(false);
    }

    protected override void ToggleRenderer(bool inIsRendererVisible)
    {
        _renderer.enabled = inIsRendererVisible;
    }
}
