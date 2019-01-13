using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Player : MonoBehaviour , Damageable {

    [SerializeField]
    int _health;

    public int MaxHealth = 5;
    public int Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;
            if (_health > MaxHealth) _health = MaxHealth;
            if (_health < 0) _health = 0;
        }
    }

    // If max speed is non zero, clamp the objects speed
    [SerializeField]
    public float _maxSpeed;

    public Rigidbody2D myRigidBody;
    public bool isInvulnerable
    {
        get
        {
            return invulnerabletimer > 0;
        }
    }

    public bool IsDamageable
    {
        get
        {
            return isActiveAndEnabled;
        }
    }

    public GameObject DamageableObject
    {
        get
        {
            return gameObject;
        }
    }

    //[SerializeField]
    //bool _orientFacing = true;
    
    [SerializeField]
    [FormerlySerializedAs("damagePsys")]
    ParticleSystem _damagePsys;
    [SerializeField]
    ParticleSystem _deathPsys;

    [InspectorReadOnly]
    public float invulnerabletimer;
    bool lastKnownIsInvulnerable;

    public System.Action OnTakeDamage;
    public System.Action OnDeath;
    public UnityEvent OnBecomeInvulnerableEvent;
    public UnityEvent OnBecomeVulnerableEvent;
    
	// Update is called once per frame
	void Update ()
    {
        if (Health <= 0)
        {
            if(OnDeath != null)
                OnDeath();
            
            _deathPsys.Play();
        }
        
        //if (_orientFacing && myRigidBody.velocity != Vector2.zero)
        //    transform.up = myRigidBody.velocity;

        //Invulnerability behaviour
        //==============================

        if (invulnerabletimer > 0)
            invulnerabletimer -= Time.deltaTime;
        else
            invulnerabletimer = 0;

        if(lastKnownIsInvulnerable != isInvulnerable)
        {
            if (isInvulnerable)
                OnBecomeInvulnerableEvent.Invoke();
            else
                OnBecomeVulnerableEvent.Invoke();
        }

        lastKnownIsInvulnerable = isInvulnerable;

        //==============================
        
        if (_maxSpeed > 0 && myRigidBody.velocity.sqrMagnitude > _maxSpeed * _maxSpeed)
        {
            myRigidBody.drag = 1;
        }
        else
        {
            myRigidBody.drag = 0;
        }

        //float angle = Vector2.SignedAngle(Vector2.up, myRigidBody.velocity);
        //myRigidBody.MoveRotation(angle);
    }
    
    public void TakeDamage(int inDmg)
    {
        if(inDmg > 0)
        {
            if (!isInvulnerable)
            {
                Health -= inDmg;
                _damagePsys.Play();

                if(OnTakeDamage != null)
                    OnTakeDamage();

                if (Health <= 0)
                {
                    if (OnDeath != null)
                        OnDeath();

                    _deathPsys.transform.SetParent(null);
                    _deathPsys.Play();
                }
            }
        }
        else
        {
            //Its a healing
            Health -= inDmg;
        }

        
    }

}
