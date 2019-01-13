using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTowards2D : MoveTowards2DBase
{
    [SerializeField]
    float Acceleration = 2;
    [SerializeField]
    float MaxSpeed = 20;

    [SerializeField]
    bool ignoreDistanceChecks = false;

    [SerializeField]
    float _minAngleOfMovementConsideration = 1;
    //enum AccelerationMode
    //{
    //    ALWAYS_ACCELERATE_TOWARDS_TARGET,
    //    ONLY_ACCELERATE_WHEN_FACING_TARGET,
    //    ALWAYS_ACCELERATE_FORWARD
    //}

    enum MoveType
    {
        MOVE_AS_POINT,
        MOVE_AS_DIR,
    }
    [SerializeField]
    MoveType _moveType;

    float brakingDistance
    {
        get
        {
            return MaxSpeed / Acceleration;
        }
    }

    public override void MovementBehaviour(Vector2 inTargetPos)
    {
        switch (_moveType)
        {
            case MoveType.MOVE_AS_DIR:
                moveAsDirection(inTargetPos);
                break;
            case MoveType.MOVE_AS_POINT:
                moveAsPosition(inTargetPos);
                break;
            default:
                break;
        }


    }

    void moveAsDirection(Vector2 inTargetVector)
    {
        bool directionCheck = (_minAngleOfMovementConsideration >= 180 || Vector2.Angle(transform.up, inTargetVector) < _minAngleOfMovementConsideration);

        if (directionCheck)
        {
            _myRigidBody.AddForce(inTargetVector * Acceleration, ForceMode2D.Force);
        }
    }


    private void Start()
    {
        
        if (_turningBehaviour == null)
            _turningBehaviour = GetComponent<TurnTowardsBase>();
        
    }

    [SerializeField]
    bool _disableTurningDuringMovement;
    [SerializeField]
    TurnTowardsBase _turningBehaviour;

    void moveAsPosition(Vector2 inTargetPos)
    {
        Vector2 vectorToTarget = inTargetPos - _myRigidBody.position;
        Vector2 normalisedVectorToTarget = vectorToTarget.normalized;

        //Are we close enough
        if (inTargetPos.IsWithinDistanceOf(_myRigidBody.position, brakingDistance) && !ignoreDistanceChecks)
        {
            _myRigidBody.AddForce(vectorToTarget, ForceMode2D.Force);

            //if (IsWithinDistance(TargetPos, transform.position, 0.2f))
            //    return;
            //else
            //    MyRigidbody.AddForce(-vectorToTarget, ForceMode.Acceleration);
        }
        else
        {
            bool directionCheck = (_minAngleOfMovementConsideration >= 180 || Vector2.Angle(transform.up, normalisedVectorToTarget) < _minAngleOfMovementConsideration);

            if (directionCheck)
            {
                //Debug.DrawRay(MyRigidbody.position, normalisedVectorToTarget, Color.green, 1);

                _myRigidBody.AddForce(normalisedVectorToTarget * Acceleration, ForceMode2D.Force);

                if (_disableTurningDuringMovement)
                    _turningBehaviour.setTarget(null);

            }
            //_myRigidBody.velocity = Vector2.ClampMagnitude(_myRigidBody.velocity, MaxSpeed);
        }
    }
}
