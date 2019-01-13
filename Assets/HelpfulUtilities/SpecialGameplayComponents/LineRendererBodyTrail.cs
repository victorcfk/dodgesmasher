using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererBodyTrail : MonoBehaviour
{
    [SerializeField]
    Rigidbody FollowingObj;

    [SerializeField]
    Transform OriginOfTrail;

    [SerializeField]
    LineRenderer MyLineRenderer;
    [SerializeField]
    float DepthOfLineRenderer = 500;
    [SerializeField]
    int bonesToSkip = 0;

    Vector3[] _bonePositions;
    Vector3[] _targetPositionForBone;
    float[] _boneLengths;

    void Awake()
    {
        SetUpBody();
    }

    void OnEnable()
    {
        if (OriginOfTrail == null)	OriginOfTrail = FollowingObj.transform;

        Vector3 objForward = new Vector2(FollowingObj.velocity.x, FollowingObj.velocity.y).normalized;
        Vector3 objPos = new Vector3(FollowingObj.position.x, FollowingObj.position.y, DepthOfLineRenderer);

        CalculateSnapBodyMotion(objForward, objPos, _bonePositions, _boneLengths);

        MyLineRenderer.SetPositions(_bonePositions);

        for(int i=0; i<_targetPositionForBone.Length; i++)
        {
            _targetPositionForBone[i] = _bonePositions[i];
        }

    }

    // Use this for initialization
    void SetUpBody ()
    {

        //Set up body flow motion
        //===============================================================

        _bonePositions = new Vector3[MyLineRenderer.positionCount];
        _targetPositionForBone = new Vector3[MyLineRenderer.positionCount];
        _boneLengths = new float[MyLineRenderer.positionCount - 1];

        MyLineRenderer.GetPositions(_bonePositions);

        for (int i = 0; i < _bonePositions.Length - 1; i++)
        {
            _boneLengths[i] = Vector3.Distance(_bonePositions[i], _bonePositions[i + 1]);
        }
        //===============================================================

        bonesToSkip = Mathf.Clamp(bonesToSkip, 0, MyLineRenderer.positionCount);
    }

    // Use this for initialization
    void CalculateSnapBodyMotion(Vector3 inCurrentTravelDir, Vector3 inCurrentPosition, Vector3[] inBonePositions, float[] boneLengths)
    {
        //The intial bone Follows the origin
        inBonePositions[0] = inCurrentPosition;
        for (int i = 1; i < inBonePositions.Length; i++)
        {
            inBonePositions[i] = inBonePositions[i - 1] - inCurrentTravelDir * boneLengths[i-1];
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //Hack to ensure worm body only appears once it is moving
        if (!MyLineRenderer.enabled)
            MyLineRenderer.enabled = true;

        Vector3 objForward = new Vector2(FollowingObj.velocity.x, FollowingObj.velocity.y).normalized;
        Vector3 objPos = new Vector3(OriginOfTrail.position.x, OriginOfTrail.position.y, DepthOfLineRenderer);

        //Flow motion on the body
        //===============================================================
        CalculateFlowMotion(objPos, objForward, _bonePositions, _boneLengths, bonesToSkip);
        //CalculateFlowMotionW(_bonePositions, _boneLengths, MovingObj, 0);
        MyLineRenderer.SetPositions(_bonePositions);
        //===============================================================
    }

    void CalculateFlowMotion(Vector3 inCurrentPosition, Vector3 inCurrentForward,Vector3[] inBonePositions, float[] boneLengths, int bonesToSkip)
    {
        //The intial bone Follows the position of the object
        inBonePositions[0] = inCurrentPosition;

        //Skip any bones if necessary, making them follow the prey forward
        for (int i = 1; i < bonesToSkip; i++)
        {
            inBonePositions[i] = -1 * boneLengths[i-1] * inCurrentForward + inBonePositions[i - 1];
        }

        //bones to skip should at least be one
        if (bonesToSkip < 1)
            bonesToSkip = 1;

        //The rest of the bones always move towards a target position
        //The distance between the (parent and target) plus the distance between (child and target)
        //should always be the length of the bone, to preserve the shape of the line renderer
        for (int i = bonesToSkip; i < inBonePositions.Length; i++)
        {
            float distanceOfParentBoneToTargetPos = Vector3.Distance(inBonePositions[i-1], _targetPositionForBone[i]);
            float distanceOfCurrentBoneToTargetPos = boneLengths[i - 1] - distanceOfParentBoneToTargetPos;

            //Does the distance from the parent bone to the target exceed the length of the bone? (Might occur due to frame update gaps)
            if (distanceOfParentBoneToTargetPos > boneLengths[i - 1])
            {
                //Yes, move this bone directly towards its parent bone
                Vector3 directionFromParentToCurrentBone = (inBonePositions[i] - inBonePositions[i - 1]).normalized;

                inBonePositions[i] = inBonePositions[i - 1] + directionFromParentToCurrentBone * boneLengths[i - 1];

                _targetPositionForBone[i] = inBonePositions[i - 1];
            }
            else
            {
                //No, move this bone towards its target
                Vector3 directionToIntersection = (_targetPositionForBone[i] - inBonePositions[i]).normalized;

                inBonePositions[i] = _targetPositionForBone[i] - directionToIntersection * distanceOfCurrentBoneToTargetPos;
            }
        }
    }

    private void OnDisable()
    {
        MyLineRenderer.enabled = false;
    }

}
