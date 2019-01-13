using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ColliderEnabling : MonoBehaviour {

    [SerializeField]
    UnityEvent _restorationAction;

    [SerializeField]
    BoxCollider2D _boxColl;

    [SerializeField]
    LayerMask _wantedLayers;

    Collider2D[] _results = new Collider2D[5];

    // Use this for initialization
    public void AttemptToEnableAfterTimeAndColliderCheck(float inTimeDelayBeforeCheck){
        
        GameManager.RunActionAfterFuncCheckAndDelay(
            this,
            () => { _restorationAction.Invoke(); },
            () => { return Physics2D.OverlapBoxNonAlloc(transform.position, _boxColl.size*transform.localScale.x, transform.rotation.eulerAngles.z, _results, _wantedLayers) <= 0; }, inTimeDelayBeforeCheck);
    }
}
