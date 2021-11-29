using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warhead : MonoBehaviour
{
    public GameObject model;
    public GameObject ExplosionVFX;
    public GameObject WarheadVFX;
    private GameObject warheadVFXref;
    Rigidbody rb;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void InitializedWarhead(Vector3 position, Quaternion rotation, Vector3 force)
    {
        warheadVFXref = Instantiate(WarheadVFX, position, rotation);
        warheadVFXref.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //model.transform.rotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
    }

    private void OnCollisionEnter(Collision collision)
    {
        transform.position = collision.GetContact(0).point;
        Destroy(Instantiate(ExplosionVFX, transform.position, Quaternion.identity), 3);
        Destroy(gameObject);
        warheadVFXref.GetComponent<WarheadVFX>().DestroySelf();
        Destroy(warheadVFXref);
    }

}
