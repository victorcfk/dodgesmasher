using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashEnemyOnDMG : MonoBehaviour {
    [SerializeField]
    SpriteRenderer _spriteToFlash;
    [SerializeField]
    Enemy _enemy;
    [SerializeField]
    Color _flashColor;
    [SerializeField]
    float _flashDuration = 0.1f;
    
    Color _originalColor;

    MEC.CoroutineHandle _ch;

    // Use this for initialization
    void Start () {
        _enemy.OnTakeDamage += FlashRenderer;
        _originalColor = _spriteToFlash.color;
    }
    
    // Update is called once per frame
    void FlashRenderer (GameObject inObj) {

        if (gameObject != null && gameObject.activeInHierarchy)
        {
            _spriteToFlash.color = _flashColor;

            if (_ch.IsValid)
                MEC.Timing.KillCoroutines(_ch);

            _ch = MEC.Timing.CallDelayed(_flashDuration, RestoreColor,gameObject);
        }
    }

    void RestoreColor()
    {
        if(gameObject != null && gameObject.activeInHierarchy)
        {
            _spriteToFlash.color = _originalColor;
        }

    }
}
