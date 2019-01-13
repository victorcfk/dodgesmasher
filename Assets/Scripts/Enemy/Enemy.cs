using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, Damageable
{
    [SerializeField]
    int _health;

    [SerializeField]
    int _scoreOnKill = 10;
    public int ScoreOnKill
    {
        get
        {
            return _scoreOnKill;
        }
    }

    [SerializeField]
    GameObject _deathFX;

    public Action<Enemy> OnDeath;
    public Action<GameObject> OnTakeDamage;

    public int Health
    {
        get
        {
            return _health;
        }
        protected set
        {
            _health = value;
        }
    }

    public bool IsDamageable
    {
        get
        {
            return isActiveAndEnabled;
        }
    }

    [Header ("Code regarding pushing away objects that are too close to you, without actually colliding with them")]
    [SerializeField]
    CircleCollider2D _myCircleCollider;
    [SerializeField]
    BoxCollider2D _myBoxCollider;

    [SerializeField]
    LayerMask _pushLayers;
    
    [SerializeField]
    float _baseForceToPushAwayEnemies = 8;
    [SerializeField]
    bool _pushAwayFurtherIfCloser = true;

    Collider2D[] wantedResults = new Collider2D[5];
    public void Update()
    {
        if (_baseForceToPushAwayEnemies > 0)
        {
            if (_myBoxCollider != null)
                _myBoxCollider.PushRigidbodiesAway(_baseForceToPushAwayEnemies, wantedResults, _pushLayers, _pushAwayFurtherIfCloser);
            else
            if (_myCircleCollider != null)
                _myCircleCollider.PushRigidbodiesAway(_baseForceToPushAwayEnemies, wantedResults, _pushLayers, _pushAwayFurtherIfCloser);
        }
    }

    public GameObject DamageableObject
    {
        get
        {
            return gameObject;
        }
    }

    [ContextMenu("takedmg")]
    public void TakeDamage()
    {
        TakeDamage(1);
    }

    public void TakeDamage(int inDmg)
    {
        _health -= inDmg;

        if (OnTakeDamage != null)
            OnTakeDamage(gameObject);

        if (_health <= 0) OnDeathCallback();
    }

    private void OnDeathCallback()
    {
        if (OnDeath != null)
            OnDeath(this);
        else
            Destroy(this.gameObject);

        if (_deathFX != null)
            Instantiate(_deathFX, transform.position, Quaternion.identity);
    }
}