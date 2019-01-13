using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;

public class DragFire : MonoBehaviour{

    // This class will store an association between a Finger and a LineRenderer instance
    [System.Serializable]
    public class Link
    {
        // The finger associated with this link
        public LeanFinger Finger;

        // The LineRenderer instance associated with this link
        public LineRenderer ControlLine;

        // The LineRenderer instance associated with this link
        public LineRenderer AimingLine;

        // The LineRenderer instance associated with this link
        public LineRenderer TrajectoryLine;

        public ScrollTiledTexture ScrollingBehaviour;
    }
    
    [InspectorReadOnly]
    public Rigidbody2D LaunchedObject2D;

    [InspectorReadOnly]
    float timeSpentAiming;
    [InspectorReadOnly]
    bool _isPlayerSlowMoAim;
    public bool isPlayerSlowMoAim
    {
        get { return _isPlayerSlowMoAim; }
    }
    [Tooltip("The line prefab")]
    public LineRenderer AimingLinePrefab;
    public LineRenderer TrajectoryLinePrefab;
    public LineRenderer ControlLinePrefab;

    [Header("The distance from the camera the line points will be spawned in world space")]
    public float DistanceFromCam = 1.0f;


    [Header("Aim settings")]
    [SerializeField]
    float _dragAimheightMultiplier = 1.2f;
    [SerializeField]
    float _dragAimWidthMultiplier = 0.8f;
    [SerializeField]
    float firingPowerVectorCoarseness = 1f / 25f;
    [SerializeField]
    float firingAngleVectorPosCoarseness = 1f / 20f;
    
    // This stores all the links between Fingers and LineRenderer instances
    protected List<Link> links = new List<Link>();
    
    Vector3 _lastKnownFingerStartPos;
    Vector3 _lastKnownFingerEndPos;
    //float _lastKnownProportionOfMaxSpeed;
    Link _lastKnownLink;

    public UnityEvent OnDragStart;
    public UnityEvent OnDragStop;

    [Header("callbacks")]

    public System.Action OnAimStart;
    public System.Action OnAimEnd;
    public System.Action<float> OnFireOff;

    Vector2 FiringSwipeVector
    {
        get
        {
            Vector2 DragVector;

            if (GameManager.Instance.isDragOppositeToFire)
            {
                DragVector = _lastKnownFingerStartPos - _lastKnownFingerEndPos;
            }
            else
            {
                DragVector = _lastKnownFingerEndPos - _lastKnownFingerStartPos;
            }

            DragVector = new Vector2(DragVector.x * _dragAimWidthMultiplier, DragVector.y * _dragAimheightMultiplier);
            return DragVector;
        }
    }


    Vector3 WantedLaunchVelocity(Vector3 FiringSwipeVector, out float inLastKnownProportionOfMaxSpeedUsed)
    {
        if (GameManager.Instance.DragDistanceForMaxSpeed > 0)
            inLastKnownProportionOfMaxSpeedUsed = Mathf.Clamp(FiringSwipeVector.magnitude / GameManager.Instance.DragDistanceForMaxSpeed, 0f, 1f);    
        else
            inLastKnownProportionOfMaxSpeedUsed = 1;

        //one unit of erergy is the attempt to travel at max speed
        inLastKnownProportionOfMaxSpeedUsed = Mathf.Clamp(inLastKnownProportionOfMaxSpeedUsed, 0, GameManager.Instance.Energy);
            

        inLastKnownProportionOfMaxSpeedUsed = GetModifiedFiringPowerForCoarseness(inLastKnownProportionOfMaxSpeedUsed, firingPowerVectorCoarseness);

        Vector3 firingVector = GetModifiedFiringAngleForCoarseness(FiringSwipeVector, firingAngleVectorPosCoarseness);
            
        float Speed = GameManager.Instance.PlayerBaseLaunchSpeed * inLastKnownProportionOfMaxSpeedUsed;
        return firingVector.normalized * Speed;

    }

    Vector3 GetModifiedFiringAngleForCoarseness(Vector3 inFiringVector, float inFiringAngleVectorPosCoarseness)
    {
        if (inFiringAngleVectorPosCoarseness > 0)
        {
            float newX = Mathf.Round(FiringSwipeVector.x / inFiringAngleVectorPosCoarseness) * inFiringAngleVectorPosCoarseness;
            float newY = Mathf.Round(FiringSwipeVector.y / inFiringAngleVectorPosCoarseness) * inFiringAngleVectorPosCoarseness;

            inFiringVector = new Vector3(newX, newY, 0);
        }

        return inFiringVector;
    }

    float GetModifiedFiringPowerForCoarseness(float inLastKnownProportionOfMaxSpeed, float inFiringPowerVectorCoarseness)
    {
        if (inFiringPowerVectorCoarseness > 0)
            inLastKnownProportionOfMaxSpeed = Mathf.Round(inLastKnownProportionOfMaxSpeed / inFiringPowerVectorCoarseness) * inFiringPowerVectorCoarseness;

        return inLastKnownProportionOfMaxSpeed;
    }

    // Update is called once per frame
    //   void Update () {

    //       //Do we force the player to fire off?
    //       if (GameManager.Instance.MaxTimeAllowedForAiming > 0 
    //           &&
    //           GameManager.Instance.IsPlayerAiming)
    //       {
    //           if (timeSpentAiming > GameManager.Instance.MaxTimeAllowedForAiming)
    //           {
    //               if (OnAimEnd != null)
    //                   OnAimEnd();

    //               _isPlayerSlowMoAim = false;

    //               timeSpentAiming = 0;

    //               if (GameManager.Instance.isAutoLaunchOnTimeout)
    //               {
    //                   OnDragStop.Invoke();

    //                   float proportionOfMaxSpeedUsed;
    //                   FireOff(WantedLaunchVelocity(FiringSwipeVector, out proportionOfMaxSpeedUsed));

    //                   GameManager.Instance.DrainEnergy(proportionOfMaxSpeedUsed);
    //               }
    //           }
    //           else
    //           {
    //               timeSpentAiming += Time.unscaledDeltaTime;
    //           }
    //       }
    //}
    [SerializeField]
    float _maxAllowedY = -10;
    [SerializeField]
    float _maxAllowedX = -1000;
    [SerializeField]
    float _minAllowedX = 1000;

    protected virtual void OnFingerDown(LeanFinger finger)
    {
        _lastKnownFingerStartPos = finger.GetStartWorldPosition(DistanceFromCam);
        _lastKnownFingerEndPos = finger.GetWorldPosition(DistanceFromCam);

        if (_lastKnownFingerStartPos.y <= _maxAllowedY &&
            _lastKnownFingerStartPos.x >= _minAllowedX &&
            _lastKnownFingerStartPos.x <= _maxAllowedX)
        {
            if (OnAimStart != null)
                OnAimStart();
        }
    }

    /// <summary>
    /// Can potentially optimise by reusing the aiming prefabs
    /// </summary>
    /// <param name="finger"></param>
    protected virtual void OnFingerSet(LeanFinger finger)
    {
        // Make sure the prefab exists
        if (GameManager.Instance.IsPlayerAiming)
        {
            if (_lastKnownLink == null)
            {
                // Make new link
                _lastKnownLink = new Link();

                // Assign this finger to this link
                //_lastKnownLink.Finger = finger;

                // Create LineRenderer instance for this link
                if (AimingLinePrefab)
                {
                    _lastKnownLink.AimingLine = Instantiate(AimingLinePrefab,transform);
                }

                // Create LineRenderer instance for this link
                if (TrajectoryLinePrefab)
                {
                    _lastKnownLink.TrajectoryLine = Instantiate(TrajectoryLinePrefab, transform);
                    _lastKnownLink.ScrollingBehaviour = _lastKnownLink.TrajectoryLine.GetComponent<ScrollTiledTexture>();
                }

                // Create LineRenderer instance for this link
                if (ControlLinePrefab)
                {
                    _lastKnownLink.ControlLine = Instantiate(ControlLinePrefab, transform);
                }

                // Add new link to list
                links.Add(_lastKnownLink);
            }

            ToggleAimingUI(true);

            _lastKnownFingerStartPos = finger.GetStartWorldPosition(DistanceFromCam);
            _lastKnownFingerEndPos = finger.GetWorldPosition(DistanceFromCam);

            //lastKnownLink = links.Find(l => l.Finger == finger);

            DrawPlayerAimingGuides(_lastKnownLink.AimingLine, _lastKnownLink.TrajectoryLine, _lastKnownLink.ControlLine, _lastKnownLink.ScrollingBehaviour, finger);
        }
    }

    protected virtual void OnFingerUp(LeanFinger finger)
    {
        if (GameManager.Instance.IsPlayerAiming)
        {
            _lastKnownFingerStartPos = finger.GetStartWorldPosition(DistanceFromCam);
            _lastKnownFingerEndPos = finger.GetWorldPosition(DistanceFromCam);

            //lastKnownLink = links.Find(l => l.Finger == finger);

            // Try and find the link for this finger
            //var link = links.Find(l => l.Finger == finger);

            // Link doesn't exist?
            //if (link != null)
            //{
            //    DrawPlayerAimingGuides(link.AimingLine, link.CorridorLine, link.ControlLine, link.Finger);
            //}

            OnDragStop.Invoke();
            
            OnAimEnd();
            timeSpentAiming = 0;

            float proportionOfMaxSpeedUsed;
            LaunchedObject2D.velocity = WantedLaunchVelocity(FiringSwipeVector, out proportionOfMaxSpeedUsed);

            ToggleAimingUI(false);

            if(OnFireOff !=null)
                OnFireOff(proportionOfMaxSpeedUsed);
            
        }
    }

    public void ToggleAimingUI(bool inIsActive)
    {
        // Link exists?
        if (_lastKnownLink != null)
        {
            if (_lastKnownLink.AimingLine)
                _lastKnownLink.AimingLine.gameObject.SetActive(inIsActive);

            if (_lastKnownLink.TrajectoryLine)
                _lastKnownLink.TrajectoryLine.gameObject.SetActive(inIsActive);

            if (_lastKnownLink.ControlLine)
                _lastKnownLink.ControlLine.gameObject.SetActive(inIsActive);
        }
    }

    [Header("Control Line")]
    [SerializeField]
    float controlLineWidth = 0.25f;

    [Header("Aim Line")]
    [SerializeField]
    float aimLineWidth = 0.4f;
    [SerializeField]
    float aimLineTextureScaling = 0.2f;


    [Header("Trajectory Line")]
    [SerializeField]
    float trajectoryLineTextureScaling = 0.3f;
    [SerializeField]
    float trajectoryLineWidth = 1f;
    [SerializeField]
    int _calculatedTrajectoryPoints = 500;
    [SerializeField]
    float _totalCalculatedTrajectoryDistance = 4;
    [SerializeField]
    int _skipDrawingInitialTrajectoryLineNum = 5;
    [SerializeField]
    bool _changeScrollSpeedAccordingToPower = false;
    [SerializeField]
    float _lineRendererMaxTextureScrollSpeed = 2.5f;


    [SerializeField]
    float _unityPhysicsWonkiness = 1.043f;

    protected virtual void DrawPlayerAimingGuides(
        LineRenderer inStraightTravelLine,
        LineRenderer inTrajectoryLine,
        LineRenderer inControlLine,
        ScrollTiledTexture inScrollTiledTexture,
        LeanFinger finger)
    {
        // Get start and current world position of finger
        Vector3 fingerStartPos = finger.GetStartWorldPosition(DistanceFromCam);
        Vector3 fingerCurrentPos = finger.GetWorldPosition(DistanceFromCam);

        //==============================================

        float multiplierToPlayerBaseLaunchSpeed;
        if (GameManager.Instance.DragDistanceForMaxSpeed > 0)
            multiplierToPlayerBaseLaunchSpeed = Mathf.Clamp(FiringSwipeVector.magnitude / GameManager.Instance.DragDistanceForMaxSpeed, 0f, 1f);
        else
            multiplierToPlayerBaseLaunchSpeed = 1;

        Vector2 BallTravelDirVector = FiringSwipeVector.normalized;

        if (inStraightTravelLine)
        {
            inStraightTravelLine.positionCount = 2;
            inStraightTravelLine.SetPosition(0, LaunchedObject2D.position);
            inStraightTravelLine.SetPosition(1, FiringSwipeVector.normalized + LaunchedObject2D.position);
            inStraightTravelLine.material.mainTextureScale = new Vector2(aimLineTextureScaling, 1);

            if (GameManager.Instance.MaxTimeAllowedForAiming > 0)
            {
                float lineWidth = (timeSpentAiming) / GameManager.Instance.MaxTimeAllowedForAiming * aimLineWidth;
                inStraightTravelLine.startWidth = lineWidth;
                inStraightTravelLine.endWidth = lineWidth;
            }
            else
            {
                inStraightTravelLine.startWidth = aimLineWidth;
                inStraightTravelLine.endWidth = aimLineWidth;
            }
        }

        if (inTrajectoryLine)
        {
            inTrajectoryLine.positionCount = 2;
            inTrajectoryLine.SetPosition(0, LaunchedObject2D.position);
            inTrajectoryLine.SetPosition(1, FiringSwipeVector.normalized + LaunchedObject2D.position);
            inTrajectoryLine.material.mainTextureScale = new Vector2(trajectoryLineTextureScaling, 1);

            inTrajectoryLine.startWidth = trajectoryLineWidth;
            inTrajectoryLine.endWidth = trajectoryLineWidth;

            float ignorethis;
            Vector2[] trajectoryPoints = TrajectoryCalculation(
                LaunchedObject2D.position,
                WantedLaunchVelocity(FiringSwipeVector, out ignorethis),
                Physics2D.gravity * LaunchedObject2D.gravityScale * _unityPhysicsWonkiness,
                _calculatedTrajectoryPoints, _totalCalculatedTrajectoryDistance
                );

            //DrawingTrajectoryWithSpeed.Invoke(WantedLaunchVelocity.sqrMagnitude);

            //Collider2D g;
            //Vector2[] trajectoryPoints = simulatePath(
            //    LaunchedObject2D.position,
            //    WantedLaunchVelocity,
            //    Physics.gravity,
            //     _calculatedTrajectoryPoints, _calculatedTrajectoryDistanceBetweenPoints, out g
            //    );

            int finalDrawPointCount = trajectoryPoints.Length - 1 - _skipDrawingInitialTrajectoryLineNum;
            inTrajectoryLine.positionCount = finalDrawPointCount;
            for (int i = 0; i < finalDrawPointCount; i++)
            {
                inTrajectoryLine.SetPosition(i, trajectoryPoints[i + _skipDrawingInitialTrajectoryLineNum]);
            }

            //Max scroll == 
            if (inScrollTiledTexture != null)
            {
                if (_changeScrollSpeedAccordingToPower)
                    inScrollTiledTexture.ScrollUnitsPerSecond = Vector2.left * Mathf.Lerp(0, (1 / Time.timeScale) * _lineRendererMaxTextureScrollSpeed, multiplierToPlayerBaseLaunchSpeed);
                else
                    inScrollTiledTexture.ScrollUnitsPerSecond = Vector2.left * (1 / Time.timeScale) * _lineRendererMaxTextureScrollSpeed;

            }
        }

        if (inControlLine)
        {
            inControlLine.positionCount = 2;
            inControlLine.SetPosition(0, fingerStartPos);
            inControlLine.SetPosition(1, fingerCurrentPos);

            inControlLine.startWidth = controlLineWidth;
            inControlLine.endWidth = controlLineWidth;
        }

        //inAimingLine.material.mainTextureScale = new Vector2(BallTravelDist * aimLineTextureScaling, 1);
        //inCorridorLine.material.mainTextureScale = new Vector2(BallTravelDist * corridorLineTextureScaling, 1);

        //==============================================

    }

    protected virtual void OnEnable()
    {
        LeanTouch.OnFingerUp += OnFingerUp;
        LeanTouch.OnFingerSet += OnFingerSet;
        LeanTouch.OnFingerDown += OnFingerDown;
    }

    protected virtual void OnDisable()
    {
        LeanTouch.OnFingerUp -= OnFingerUp;
        LeanTouch.OnFingerSet -= OnFingerSet;
        LeanTouch.OnFingerDown -= OnFingerDown;
    }


    /// <summary>
    /// Calculate the trajectory Of A ball
    /// </summary>
    /// <param name="inStartPos"></param>
    /// <param name="inStartVel"></param>
    /// <param name="inGravity"></param>
    /// <param name="inCalculatedPoints"></param>
    /// <param name="inDistance"></param>
    /// <returns></returns>
    public static Vector2[] TrajectoryCalculation(Vector2 inStartPos, Vector2 inStartVel, Vector2 inGravity, int inCalculatedPoints, float inDistance)
    {       
        Vector2[] curvePoints = new Vector2[inCalculatedPoints];

        curvePoints[0] = inStartPos;
        Vector2 tmpVel = inStartVel;
        float h = inDistance / inCalculatedPoints;

        for (int i = 1; i < inCalculatedPoints; ++i)
        {
            curvePoints[i] = curvePoints[i - 1] + h * tmpVel;
            tmpVel += h * inGravity;
        }

        return curvePoints;
    }

    /// <summary>
    /// Simulate the path of a launched ball.
    /// Slight errors are inherent in the numerical method used.
    /// </summary>
    public static Vector2[] SimulatePath(
        Vector2 inInitialLoc, 
        Vector2 inInitialVeloc, 
        Vector2 inGravity, 
        int inSegmentCount, 
        float inSegmentScale, 
        LayerMask inCollisionLayerMask, 
        out Collider2D _hitObject)
    {
        Vector2[] segments = new Vector2[inSegmentCount];

        // The first line point is wherever the player's cannon, etc is
        segments[0] = inInitialLoc;

        // The initial velocity
        Vector2 segVelocity = inInitialVeloc;

        // reset our hit object
        _hitObject = null;

        for (int i = 1; i < inSegmentCount; i++)
        {
            // Time it takes to traverse one segment of length segScale (careful if velocity is zero)
            float segTime = (segVelocity.sqrMagnitude != 0) ? inSegmentScale / segVelocity.magnitude : 0;

            // Add velocity from gravity for this segment's timestep
            segVelocity = segVelocity + inGravity * segTime;

            // Check to see if we're going to hit a physics object
            //RaycastHit2D hit;
            //bool t = Physics.Raycast(segments[i - 1], segVelocity, out hit, segmentScale);
            RaycastHit2D hit = Physics2D.Raycast(segments[i - 1], segVelocity, inSegmentScale,inCollisionLayerMask );

            if (hit.collider != null)
            {
                // remember who we hit
                _hitObject = hit.collider;

                // set next position to the position where we hit the physics object
                segments[i] = segments[i - 1] + segVelocity.normalized * hit.distance;
                // correct ending velocity, since we didn't actually travel an entire segment
                segVelocity = segVelocity - inGravity * (inSegmentScale - hit.distance) / segVelocity.magnitude;
                // flip the velocity to simulate a bounce
                segVelocity = Vector3.Reflect(segVelocity, hit.normal);

                /*
				 * Here you could check if the object hit by the Raycast had some property - was 
				 * sticky, would cause the ball to explode, or was another ball in the air for 
				 * instance. You could then end the simulation by setting all further points to 
				 * this last point and then breaking this for loop.
				 */
            }
            // If our raycast hit no objects, then set the next position to the last one plus v*t
            else
            {
                segments[i] = segments[i - 1] + segVelocity * segTime;
            }
        }

        return segments;
    }

    //IEnumerator makePlayerVulnerable()
    //{
    //    yield return new WaitForSeconds(1);
    //    player.isInvulnerable = false;
    //}

}
