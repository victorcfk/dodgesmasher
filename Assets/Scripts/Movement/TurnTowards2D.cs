using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TurnTowards2D : TurnTowardsBase
{
    [SerializeField]
    Vector2 TargetPos;

    [SerializeField]
    bool OrientFacing = true;

    [SerializeField]
    Rigidbody2D myRigidbody;

    [Tooltip("negative turn speed is instant")]
    public float TurnDegreesPerSec = 180;

    void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate () {

        if (OrientFacing && TargetPos != myRigidbody.position)
        {
            Vector2 desiredDir = (TargetPos - myRigidbody.position).normalized;
            TurningBehaviour(desiredDir, TurnDegreesPerSec);
        }
	}

    void TurningBehaviour(Vector2 inDesiredDir, float inTurnDegreesPerSec)
    {
        float wantedAngle = Vector2.SignedAngle(Vector2.up, inDesiredDir);
        float currentAngle = Vector2.SignedAngle(Vector2.up, transform.up);

        if (inTurnDegreesPerSec >= 0)
        {
            myRigidbody.MoveRotation(
                Mathf.MoveTowardsAngle(currentAngle, wantedAngle, inTurnDegreesPerSec * Time.fixedDeltaTime));
        }
        else
            myRigidbody.MoveRotation(wantedAngle);
    }

    public override void setTarget(Transform inTarget)
    {
        if (inTarget == null || inTarget == transform)
            OrientFacing = false;
        else
        {
            OrientFacing = true;
            TargetPos = inTarget.position;
        }
    }
}
