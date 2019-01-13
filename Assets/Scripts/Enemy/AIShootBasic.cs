using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class AIShootBasic : MonoBehaviour {

    [InspectorReadOnly]
    protected Transform _shootingTarget;
    [SerializeField]
    float minShootingAngle = 1;

    [SerializeField]
    WeaponBase _weaponBehaviour;
    [SerializeField]
    TurnTowardsBase _turningBehaviour;
    [SerializeField]
    [FormerlySerializedAs("_move")]
    protected MoveTowards2DBase _moveTowardsBehaviour;

    public UnityEvent OnEnabled;

    protected enum DirectionType
    {
        RANDOM,
        TOWARDS_TARGET,
        AWAY_FROM_TARGET,
        RANDOM_HORIZONTAL,
        RANDOM_VERTICAL,
        RANDOM_ORTHOGONAL,
        RANDOM_EIGHT_AXIS
    }
    protected enum MovementBehaviour
    {
        MOVE_AND_TURN,
        ONLY_TURN,
        ONLY_MOVE,
        STATIC,
    }
    [SerializeField]
    [FormerlySerializedAs("_firingBehaviour")]
    protected MovementBehaviour _movementBehaviourDuringFiring;

    [SerializeField]
    [FormerlySerializedAs("_betweenFiringBehaviour")]
    protected MovementBehaviour _movementBehaviourBetweenFiring;

    [Space]
    [SerializeField]
    [FormerlySerializedAs("_movementType")]
    protected DirectionType _directionOfMovementType;

    [InspectorReadOnly]
    protected float _movementTimer;
    [SerializeField]
    protected float _timeBetweenMovement = 2;
    [SerializeField]
    protected float _movementDistanceAway = 4;

    
    protected Vector2 _lastKnownPos;

    Vector3 VectorToShootingTarget
    {
        get
        {
            if (_shootingTarget == null)
                return Vector3.zero;
            else
                return (_shootingTarget.position - transform.position).normalized;
        }
    }

    protected virtual void Awake()
    {
        if (_moveTowardsBehaviour == null)
            _moveTowardsBehaviour = GetComponent<MoveTowards2DBase>();
        if (_turningBehaviour ==null)
            _turningBehaviour = GetComponent<TurnTowardsBase>();
        if (_weaponBehaviour == null)
            _weaponBehaviour = GetComponentInChildren<WeaponBase>();
    }

    // Update is called once per frame
    void FixedUpdate () {

        if (_shootingTarget == null)
        {
            _shootingTarget = GameManager.Instance.PlayerAvatarTransform;

        }
        else
        {
            if (_weaponBehaviour.IsFiring)
            {
                PerformMovement(_movementBehaviourDuringFiring, _shootingTarget);
            }
            else
            {
                if (Vector3.Angle(_weaponBehaviour.transform.up, VectorToShootingTarget) < minShootingAngle && _weaponBehaviour.IsReadyToFire)
                    _weaponBehaviour.StartFiring();
                else
                    PerformMovement(_movementBehaviourBetweenFiring, _shootingTarget);
            }
        }
		
	}

    private void PerformMovement(MovementBehaviour inMoveBehave, Transform inTarget)
    {
        switch (inMoveBehave)
        {
            case MovementBehaviour.ONLY_TURN:
                _turningBehaviour.setTarget(inTarget);
                break;
            case MovementBehaviour.STATIC:
                _turningBehaviour.setTarget(null);

                break;
            case MovementBehaviour.ONLY_MOVE:

                _turningBehaviour.setTarget(null);               
                _moveTowardsBehaviour.MovementBehaviour(GetPositionToMoveTo());
                break;
            case MovementBehaviour.MOVE_AND_TURN:
            default:
                
                _turningBehaviour.setTarget(inTarget);
                _moveTowardsBehaviour.MovementBehaviour(GetPositionToMoveTo());
                break;
        }
    }

    // Update is called once per frame
    protected virtual Vector2 GetPositionToMoveTo()
    {
        //We have timed movement
        if (_timeBetweenMovement > 0)
        {
            if (_movementTimer > 0)
            {
                _movementTimer -= Time.deltaTime;

                _lastKnownPos.DebugDrawCross(Color.green,1,0.5f);

                return _lastKnownPos;
            }
            else
            {
                _movementTimer = _timeBetweenMovement;
            }
        }

        Vector2 wantedPosition;
        Vector2 wantedDir = GetDirectionOfMovement(_directionOfMovementType);
       
        wantedPosition = wantedDir.normalized * _movementDistanceAway + _moveTowardsBehaviour.Position;
        _lastKnownPos = wantedPosition;

        Debug.DrawLine(transform.position, wantedPosition, Color.green, 0.1f);
        return wantedPosition;
    }

    protected Vector2 GetDirectionOfMovement(DirectionType inDirectionType)
    {
        Vector2 wantedDir;
        switch (inDirectionType)
        {
            case DirectionType.RANDOM:
                wantedDir = (Random.insideUnitCircle);
                break;

            case DirectionType.TOWARDS_TARGET:
                wantedDir = ((Vector2)_shootingTarget.position - _moveTowardsBehaviour.Position);
                break;

            case DirectionType.AWAY_FROM_TARGET:
                wantedDir = (-(Vector2)_shootingTarget.position + _moveTowardsBehaviour.Position);
                break;

            case DirectionType.RANDOM_HORIZONTAL:
                wantedDir = (Random.Range(-1, 2) * Vector2.right);
                break;

            case DirectionType.RANDOM_VERTICAL:
                wantedDir = (Random.Range(-1, 2) * Vector2.up);
                break;

            case DirectionType.RANDOM_ORTHOGONAL:
                Vector2[] axes = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
                wantedDir = axes.GetRandElementInList().normalized;
                break;

            case DirectionType.RANDOM_EIGHT_AXIS:
                wantedDir = Vector2.zero;
                break;

            default:
                wantedDir = Vector2.zero;
                break;
        }

        return wantedDir;
    }

    private void OnEnable()
    {
        OnEnabled.Invoke();
    }
}
