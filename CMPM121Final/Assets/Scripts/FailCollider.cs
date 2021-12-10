using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FailCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject);
        if (other.GetComponentInParent<FirstPersonController>() || other.GetComponentInParent<FirstPersonControllerGrapple>())
        {
            GameManager.instance.DieAndRestart();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("cool:"+ collision.collider.gameObject);
    }
}
