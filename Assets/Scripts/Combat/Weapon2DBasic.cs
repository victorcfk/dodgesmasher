using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine;
using UnityEngine.Events;
using System;

public class Weapon2DBasic : WeaponBase
{
    [SerializeField]
    [FormerlySerializedAs("projectileBasic")]
    protected GameObject _projectileBasic;

    [SerializeField]
    [FormerlySerializedAs("projectileSpeed")]
    protected float _projectileSpeed;

    [SerializeField]
    [FormerlySerializedAs("OnFire")]
    protected UnityEvent _onFire;

    [SerializeField]
    protected bool _startOnCoolDown = true;

    [InspectorReadOnly]
    [SerializeField]
    protected float _timer;

    [SerializeField]
    protected GameObject _firingWarningFX;
    [SerializeField]
    protected float _firingWindUpDuration = 0.5f;
    
    public override bool IsReadyToFire{ get { return _timer <= 0 && isActiveAndEnabled; } }
    public override bool IsFiring
    {
        get
        {

            if (_firingCoroutines == null || _firingCoroutines.Length ==0) return false;
            else
            {
                //If any firing numerator has not finished, return true;
                for(int i=0; i<_firingCoroutines.Length; i++)
                {
                    if (_firingCoroutines[i] != null) return true;
                }

                return false;
            }
        }
    }

    [SerializeField]
    protected float[] _firingTime;
    [SerializeField]
    protected Transform[] _firingLocation;
    protected IEnumerator[] _firingCoroutines;

    void Awake()
    {
        if(_startOnCoolDown)
            _timer = _firingCooldown;

        if (_firingTime == null || _firingTime.Length == 0) _firingTime = new float[] { 0 };
        if (_firingLocation == null || _firingLocation.Length == 0) _firingLocation = new Transform[] { this.transform };

        _firingCoroutines = new IEnumerator[_firingTime.Length];
    }

    // Update is called once per frame
    void Update () {

        if (_timer > 0)
            _timer -= Time.deltaTime;
    }

    public override void StartFiring()
    {
        if (IsReadyToFire)
        {
            //Do we have a firing warning FX in place?
            if(_firingWarningFX != null)
                Instantiate(_firingWarningFX, transform.position,transform.rotation,transform);

            _timer = _firingCooldown;

            //we assign the firing ienumerators
            for (int i = 0; i < _firingTime.Length; i++)
            {
                IEnumerator fireIE = FiringRoutine(
                        _projectileBasic,
                        _projectileSpeed,
                        _firingWindUpDuration + _firingTime[i],
                        _firingLocation[i],
                        
                        (x) => {
                            _onFire.Invoke();
                            _firingCoroutines[x] = null;
                        },i);

                StartCoroutine(fireIE);
                _firingCoroutines[i] = fireIE;
            }
        }
    }

    IEnumerator FiringRoutine(
        GameObject inProjectileBasic,
        float inProjectileSpeed,
        float inFiringWindUpDuration,
        Transform inFiringPosition,
        Action inOnFireAction
        )
    {
        yield return new WaitForSeconds(inFiringWindUpDuration);
        
        _lastFiredProjectile = FireOffProjectile(inProjectileBasic, inFiringPosition, inProjectileSpeed);
        
        if(inOnFireAction != null)
            inOnFireAction();
    }

    IEnumerator FiringRoutine(
        GameObject inProjectileBasic,
        float inProjectileSpeed,
        float inFiringWindUpDuration,
        Transform inFiringPosition,
        Action<int> inOnFireAction,
        int inOnFireActionParam
        )
    {
        yield return new WaitForSeconds(inFiringWindUpDuration);

        _lastFiredProjectile = FireOffProjectile(inProjectileBasic, inFiringPosition, inProjectileSpeed);

        if (inOnFireAction != null)
            inOnFireAction(inOnFireActionParam);
    }

    protected virtual GameObject FireOffProjectile(
        GameObject inProjectileBasic,
        Transform inFiringPosition,
        float inProjectileSpeed)
    {
        GameObject projectile = Instantiate(inProjectileBasic, inFiringPosition.position, inFiringPosition.rotation);
        Rigidbody2D rgb = projectile.GetComponent<Rigidbody2D>();
        rgb.velocity = inFiringPosition.up * inProjectileSpeed;

        return projectile;
    }
}
