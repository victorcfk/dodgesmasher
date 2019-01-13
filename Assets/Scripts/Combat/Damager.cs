using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Damager : MonoBehaviour {

    [InspectorReadOnly]
    public Transform target;
    [InspectorReadOnly]
    public Transform firer;
    
    public List<string> TargetTags;
    public int DmgValue = 1;

    [SerializeField]
    float _forceAppliedOnHit;
    [SerializeField]
    [FormerlySerializedAs("ImpactPsys")]
    public ParticleSystem _impactPsys;

    ContactPoint2D[] _contacts = new ContactPoint2D[5];

    public System.Action<GameObject, GameObject> OnDamageOtherObject;   //First gameobject is the caller, 2nd is the damaged item
    //=================================
    private void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;

        if (TargetTags.Contains(other.tag))
            AffectOtherObj(other.gameObject, other.ClosestPoint(transform.position), other.transform.position - transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (TargetTags.Contains(other.tag))
            AffectOtherObj(other.gameObject, other.ClosestPoint(transform.position), other.transform.position - transform.position);
    }
    
    //=================================
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D other = collision.collider;
        if (TargetTags.Contains(other.tag))
        {
            if (collision.GetContacts(_contacts) > 0)
                AffectOtherObj(other.gameObject, _contacts[0].point, -_contacts[0].normal);
            else
                AffectOtherObj(other.gameObject, other.bounds.ClosestPoint(transform.position), other.transform.position - transform.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (TargetTags.Contains(other.tag))
            AffectOtherObj(other.gameObject, other.bounds.ClosestPoint(transform.position), other.transform.position - transform.position);
    }
    //=================================

    protected virtual void AffectOtherObj(GameObject inOtherObj, Vector2 inImpactPosition, Vector2 inImpactVector)
    {
        if (_forceAppliedOnHit != 0)
        {
            Rigidbody2D rgb = inOtherObj.GetComponent<Rigidbody2D>();

            if (rgb)
                rgb.AddForce(inImpactVector.normalized * _forceAppliedOnHit, ForceMode2D.Impulse);
        }

        if (_impactPsys != null)
        {
            Instantiate(_impactPsys, inImpactPosition, Quaternion.LookRotation(Vector3.forward, inImpactVector));
        }

        //=========================

        Damageable dmg = inOtherObj.GetComponent<Damageable>();
        if (dmg != null) DamageOtherObject(dmg);
    }

    protected virtual void DamageOtherObject(Damageable inDamageableObj)
    {
        if (OnDamageOtherObject != null)
            OnDamageOtherObject(gameObject, inDamageableObj.DamageableObject);

        inDamageableObj.TakeDamage(DmgValue);
    }
}
