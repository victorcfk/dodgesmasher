using UnityEngine;

public class RigidBody2DFollowTargetTransform : FollowTargetTransform
{
    public Rigidbody2D MyRigidbody2D;

    protected void Awake()
    {
        if (MyRigidbody2D == null)
            MyRigidbody2D = GetComponent<Rigidbody2D>();
    }

    protected override void PerformFollow()
    {
        if (Target != null)
        {
            if (FollowPosition)
                MyRigidbody2D.position = Target.position + PositionOffset;

            if (FollowRotation)
                MyRigidbody2D.rotation = Target.rotation.eulerAngles.z;
        }
        else
        {
            switch (_targetLostBehaviour)
            {
                case TargetLostBehaviour.DESTROY:
                    Destroy(gameObject);
                    break;
                case TargetLostBehaviour.DISABLE:
                    gameObject.SetActive(false);
                    break;
                case TargetLostBehaviour.DO_NOTHING:
                default:
                    break;
            }
        }
    }
}