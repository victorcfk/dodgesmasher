///Derived from: http://wiki.unity3d.com/index.php?title=MouseOrbitImproved
using UnityEngine;

public class MouseOrbitImproved : MonoBehaviour
{
    [SerializeField]
    Transform OrbitTarget;

    [SerializeField]
    Camera OrbitingCamera;

    [SerializeField]
    LayerMask LayersThatBlockThisCamera;
    
    [SerializeField]
    float xSpeed = 120.0f;
    [SerializeField]
    float ySpeed = 120.0f;

    [SerializeField]
    float yAngleMinLimit = -20f;
    [SerializeField]
    float yAngleMaxLimit = 80f;

    [SerializeField]
    float distanceMin = 2f;
    [SerializeField]
    float distanceMax = 15f;

    public bool IsControlledByMouse = true;

    float _camXAngle;
    float _camYAngle;
    float _camDistanceToTarget;

    float _initialCamXAngle;
    float _initialCamYAngle;
    float _initialCamDistToTarget;

    RaycastHit hit;

    private void Start()
    {
        if(IsControlledByMouse)
            InitialiseCamera();
    }

    private void LateUpdate()
    {
        if (IsControlledByMouse)
                UpdateCameraPosition(
                    Input.GetAxis("Mouse X") * xSpeed * _camDistanceToTarget * 0.02f,
                    Input.GetAxis("Mouse Y") * ySpeed * 0.02f,
                    -Input.GetAxis("Mouse ScrollWheel") * 5,
                    yAngleMinLimit,
                    yAngleMaxLimit,
                    distanceMin,
                    distanceMax,
                    ref _camDistanceToTarget,
                    ref _camXAngle,
                    ref _camYAngle,
                    out hit,
                    OrbitTarget,
                    OrbitingCamera,
                    LayersThatBlockThisCamera);
    }

    public void InitialiseCamera()
    {
        InitialiseCamera(this.OrbitingCamera, this.OrbitTarget);
    }

    public void InitialiseCamera(
        Camera OrbitingCamera, 
        Transform OrbitTarget)
    {
        Vector3 angles = OrbitingCamera.transform.eulerAngles;
        _camXAngle = angles.y;
        _camYAngle = angles.x;
        _camDistanceToTarget = Vector3.Distance(OrbitingCamera.transform.position, OrbitTarget.position);

        _initialCamXAngle = _camXAngle;
        _initialCamYAngle = _camYAngle;
        _initialCamDistToTarget = _camDistanceToTarget;
    }

    [ContextMenu("dasdas")]
    public void ResetCamera()
    {
        _camXAngle = _initialCamXAngle;
        _camYAngle = _initialCamYAngle;
        _camDistanceToTarget = _initialCamDistToTarget;
    }

    public void UpdateCameraPosition(
        float inDeltaXAngle, 
        float inDeltaYAngle, 
        float inDeltaDist,
        float inYAngleMinLimit,
        float inYAngleMaxLimit,
        float inDistanceMin,
        float inDistanceMax,
        ref float distance,
        ref float x,
        ref float y,
        out RaycastHit rayCastHit,
        Transform inOrbitTarget,
        Camera inOrbitingCamera,
        LayerMask inLayersThatBlockThisCamera)
    {
        rayCastHit = default(RaycastHit);

        if (inOrbitTarget == null || inOrbitingCamera == null) return;
        
        distance = Mathf.Clamp(distance + inDeltaDist, inDistanceMin, inDistanceMax);

        if (inLayersThatBlockThisCamera.value != 0 &&
            Physics.Linecast(inOrbitTarget.position, inOrbitingCamera.transform.position, out rayCastHit, inLayersThatBlockThisCamera))
        {
            distance -= rayCastHit.distance;
        }

        //=============================

        x += inDeltaXAngle;
        y = ClampAngle(y + inDeltaYAngle, inYAngleMinLimit, inYAngleMaxLimit);
        
        //=============================

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 position = rotation * (distance * Vector3.back) + inOrbitTarget.position;

        inOrbitingCamera.transform.rotation = rotation;
        inOrbitingCamera.transform.position = position;
    }

    private void OnValidate()
    {
        if (yAngleMinLimit < -90)
            yAngleMinLimit = -90;

        if (yAngleMaxLimit > 90)
            yAngleMaxLimit = 90;

        if (yAngleMaxLimit < yAngleMinLimit)
            yAngleMaxLimit = yAngleMinLimit;
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}