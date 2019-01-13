using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTowards2DKinematic : MoveTowards2DBase
{
    [SerializeField]
    float _maxSpeed = 20;

    [SerializeField]
    float _minAngleOfMovementConsideration = 1;

    public override void MovementBehaviour(Vector2 inTargetPos)
    {
        Vector2 normalisedVectorToTarget = (inTargetPos - _myRigidBody.position).normalized;
        bool directionCheck = (_minAngleOfMovementConsideration >= 180 || Vector2.Angle(transform.up, normalisedVectorToTarget) < _minAngleOfMovementConsideration);

        if (directionCheck)
        {
            _myRigidBody.MovePosition(Vector2.MoveTowards(Position, inTargetPos, _maxSpeed * Time.fixedDeltaTime));
        }
    }

}
