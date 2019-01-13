using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamageableCallBack : MonoBehaviour, Damageable
{
    [SerializeField]
    int _health = 1;
    [SerializeField]
    float _timeBeforeRestoration = 1;

    public UnityEvent OnDeathCallBack;
    public UnityEvent OnRestorationCallBack;

    public bool IsDamageable
    {
        get
        {
            return isActiveAndEnabled;
        }
    }

    public GameObject DamageableObject
    {
        get
        {
            return gameObject;
        }
    }

    [ContextMenu("Take Damage")]
    public void d()
    {
        TakeDamage(1);
    }

    public void TakeDamage(int inDmg)
    {
        if (isActiveAndEnabled)
        {
            _health -= inDmg;

            if (_health <= 0)
            {
                OnDeathCallBack.Invoke();

                GameManager.RunActionAfterDelay(this, () => { OnRestorationCallBack.Invoke(); _health = 1; }, _timeBeforeRestoration);
            }
        }
    }
}
