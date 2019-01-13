using UnityEngine;
using UnityEngine.Events;
using MEC;

public class DumbWeaponFirer : MonoBehaviour {

    [SerializeField]
    WeaponBase _weaponBehaviour;

    public UnityEvent OnEnabled;
    CoroutineHandle ch;

    private void OnDisable()
    {
        Timing.KillCoroutines(ch);
    }

    private void OnEnable()
    {
        if (_weaponBehaviour == null)
            _weaponBehaviour = GetComponentInChildren<WeaponBase>();

        _weaponBehaviour.StartFiring();
        ch = Timing.CallPeriodically(Mathf.Infinity, _weaponBehaviour.FiringCoolDown, _weaponBehaviour.StartFiring);

        OnEnabled.Invoke();
    }
}
