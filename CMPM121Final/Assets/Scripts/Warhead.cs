using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warhead : MonoBehaviour
{
    public GameObject model;
    public GameObject ExplosionVFX;
    public GameObject TrailVFX;
    Rigidbody rb;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //model.transform.rotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(Instantiate(ExplosionVFX, transform.position, Quaternion.identity), 3);
        TrailVFX.transform.parent = null;
        TrailVFX.GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
        Destroy(TrailVFX, 2f);
        Destroy(gameObject);
    }

}
