using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnCollision : MonoBehaviour {

    public LayerMask DestroyingLayers;

    private void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;
        
        if (DestroyingLayers.Contains(other.gameObject.layer))
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (DestroyingLayers.Contains(other.gameObject.layer))
        {
            Destroy(this.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D other = collision.collider;

        if (DestroyingLayers.Contains(other.gameObject.layer))
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (DestroyingLayers.Contains(other.gameObject.layer))
        {
            Destroy(this.gameObject);
        }
    }
}
