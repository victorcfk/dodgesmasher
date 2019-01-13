using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTowardsDirectionOnCreation : MoveTowards2DBase {
    
    [SerializeField]
    float _launchSpeed = 5;

    bool onlyOnce = true;
    public override void MovementBehaviour(Vector2 inTargetPos)
    {
        if (!onlyOnce) return;
        onlyOnce = false;

        Vector2 _direction = inTargetPos - _myRigidBody.position;

        _myRigidBody.AddForce(_direction.normalized * _launchSpeed, ForceMode2D.Impulse);
    }
}
