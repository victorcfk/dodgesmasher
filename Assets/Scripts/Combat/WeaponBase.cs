using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class WeaponBase : MonoBehaviour {

    [SerializeField]
    [FormerlySerializedAs("cooldown")]
    protected float _firingCooldown;
    public float FiringCoolDown
    {
        get
        {
            return _firingCooldown;
        }
    }

    [InspectorReadOnly]
    [SerializeField]
    protected GameObject _lastFiredProjectile;

    public abstract bool IsReadyToFire{ get; }
    public abstract bool IsFiring { get; }
    public abstract void StartFiring();
}
