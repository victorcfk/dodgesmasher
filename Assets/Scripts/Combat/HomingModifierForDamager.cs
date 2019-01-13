using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HomingModifierForDamager : MonoBehaviour {

    [SerializeField]
    Damager _damager;

    [SerializeField]
    Rigidbody2D _damagerRGB;

    [SerializeField]
    MoveTowards2DBase _move;

    [SerializeField]
    TurnTowardsBase _turn;

    [SerializeField]
    [FormerlySerializedAs("Acceleration")]
    float _acceleration = 2;

    [SerializeField]
    [FormerlySerializedAs("MaxSpeed")]
    float _maxSpeed = 20;
    [Tooltip("negative turn speed is instant")]
    public float TurnDegreesPerSec = 180;

    [SerializeField]
    float _delayToAccelerate;
    [SerializeField]
    float _delayToTurn;

    // Use this for initialization
    void Start () {

        GameManager.RunActionAfterDelay(this, () => _delayToAccelerate = 0, _delayToAccelerate);
        GameManager.RunActionAfterDelay(this, () => _delayToTurn = 0, _delayToTurn);
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        if (_delayToAccelerate <= 0)
        {
            //_damagerRGB.AddForce(transform.up * Acceleration, ForceMode2D.Force);
            float newMag = Mathf.Clamp(_damagerRGB.velocity.magnitude + _acceleration * Time.fixedDeltaTime, 0, _maxSpeed);
            _damagerRGB.velocity = transform.up * newMag;
        }

        if (_delayToTurn <= 0 )
        {
            Vector2 vectorToTarget = (Vector2)_damager.target.position - _damagerRGB.position;
            Vector2 normalisedVectorToTarget = vectorToTarget.normalized;

            TurningBehaviour(normalisedVectorToTarget, TurnDegreesPerSec);
        }
    }

    void TurningBehaviour(Vector2 inDesiredDir, float inTurnDegreesPerSec)
    {
        float wantedAngle = Vector2.SignedAngle(Vector2.up, inDesiredDir);
        float currentAngle = Vector2.SignedAngle(Vector2.up, transform.up);

        if (inTurnDegreesPerSec >= 0)
        {
            _damagerRGB.MoveRotation(
                Mathf.MoveTowardsAngle(currentAngle, wantedAngle, inTurnDegreesPerSec * Time.fixedDeltaTime));
        }
        else
            _damagerRGB.MoveRotation(wantedAngle);
    }
}
