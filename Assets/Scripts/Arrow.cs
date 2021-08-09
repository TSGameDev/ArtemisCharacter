using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] float damage = 15f;
    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Attributes attributes = collision.gameObject.GetComponent<Attributes>();
        if (attributes != null) { attributes.TakeDamage(damage); }

        transform.parent = collision.gameObject.transform;
        DisableRagdoll();
        Debug.Log(collision.gameObject.name);
    }

    void DisableRagdoll()
    {
        rb.isKinematic = true;
        rb.detectCollisions = false;
        rb.useGravity = false;
    }

}
