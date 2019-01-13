using UnityEngine;
using UnityEngine.Events;

public class AIBounceBasic : AIShootBasic {
    [InspectorReadOnly]
    [SerializeField]
    Vector2 _lastKnownDirectionOfMovement;
    [SerializeField]
    LayerMask reverseDirectionOnCollisionWithLayers;

    Rigidbody2D _myRigidBody;

    protected override void Awake()
    {
        base.Awake();
        if (_myRigidBody == null)
            _myRigidBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    protected override Vector2 GetPositionToMoveTo()
    {
        if (_lastKnownDirectionOfMovement == Vector2.zero)
        {
            _lastKnownDirectionOfMovement = GetDirectionOfMovement(_directionOfMovementType);
        }

        Vector2 wantedPosition = _lastKnownDirectionOfMovement.normalized * _movementDistanceAway + _moveTowardsBehaviour.Position;
        _lastKnownPos = wantedPosition;

        //wantedPosition.DebugDrawCross();
        //Debug.DrawLine(transform.position, wantedPosition, Color.green, 0.1f);
        return wantedPosition;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (reverseDirectionOnCollisionWithLayers.Contains(collision.gameObject.layer))
        {
            _lastKnownDirectionOfMovement = -1 * _lastKnownDirectionOfMovement;

            _myRigidBody.velocity = -_myRigidBody.velocity;
        }
    }
}
