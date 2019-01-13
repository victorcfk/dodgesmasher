using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoveTowards2DBase : MonoBehaviour {
    
    [SerializeField]
    protected Rigidbody2D _myRigidBody;

    public Vector2 Position
    {
        get
        {
            if (_myRigidBody == null)
                _myRigidBody = GetComponent<Rigidbody2D>();

            return _myRigidBody.position;
        }
    }

    // Use this for initialization
    private void Awake () {
        
        if(_myRigidBody == null)
            _myRigidBody = GetComponent<Rigidbody2D>();
    }
    
    public abstract void MovementBehaviour(Vector2 inTargetPos);
}
