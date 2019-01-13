using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateRigidBody2DContinuously : MonoBehaviour {

    [SerializeField]
    Rigidbody2D _myRigidbody;

    [SerializeField]
    float _rotationSpeed;

	// Update is called once per frame
	void Update () {

        _myRigidbody.MoveRotation( _myRigidbody.rotation + _rotationSpeed * Time.deltaTime);
    }
}
