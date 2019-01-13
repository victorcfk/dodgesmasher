using System;
using UnityEngine;

public class FollowTargetTransform : MonoBehaviour
{
    public Transform Target;
    public Vector3 PositionOffset;
    
    public bool FollowOnUpdate = true;
    public bool FollowOnLateUpdate = false;
    
    public bool FollowPosition = true;
    public bool FollowRotation = false;

    protected enum ParentingBehaviour
    {
        DO_NOTHING,
        DEPARENT_ON_START,
        SET_PARENT_AS_TARGET_ON_START,
    }
    protected enum TargetLostBehaviour
    {
        DO_NOTHING,
        DESTROY,
        DISABLE,
    }

    [SerializeField]
    protected ParentingBehaviour _parentBehaviour;
    [SerializeField]
    protected TargetLostBehaviour _targetLostBehaviour;

    protected virtual void Start()
    {
        switch (_parentBehaviour)
        {
            case ParentingBehaviour.DEPARENT_ON_START:
                transform.SetParent(null);
                break;
            case ParentingBehaviour.SET_PARENT_AS_TARGET_ON_START:
                Target = transform.parent;
                transform.SetParent(null);
                
                break;

            case ParentingBehaviour.DO_NOTHING:
            default:
                break;
        }
    }

    private void LateUpdate()
    {
        if (FollowOnLateUpdate) PerformFollow();
    }

    private void Update()
    {
        if (FollowOnUpdate) PerformFollow();
    }

    protected virtual void PerformFollow()
    {
        if (Target != null)
        {
            if (FollowPosition)
                transform.position = Target.position + PositionOffset;

            if (FollowRotation)
                transform.rotation = Target.rotation;
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
