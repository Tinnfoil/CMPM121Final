using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarheadExplosion : MonoBehaviour
{
    public List<GameObject> hitTargets;
    public GameObject explosionSphere;
    private bool effecting = true;

    private void Awake()
    {
        hitTargets = new List<GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        explosionSphere.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, .5f);
        explosionSphere.transform.localScale = new Vector3(0, 0, 0);
        LeanTween.scale(explosionSphere, new Vector3(4, 4, 4), .1f);
        LeanTween.alpha(explosionSphere, 0, .2f);
        Invoke("DisableSelf", Time.deltaTime);

        Collider[] collisions = Physics.OverlapSphere(transform.position, 4, ~0, QueryTriggerInteraction.Ignore);
        foreach(Collider col in collisions)
        {
            if (col.GetComponentInParent<Trigger>())
            {
                col.GetComponentInParent<Trigger>().TriggerObject();
            }

            if (col.GetComponent<Rigidbody>())
            {

                col.GetComponent<Rigidbody>().AddExplosionForce(10, transform.position, 4, 1.02f, ForceMode.Impulse);
            }
        }
    }

    public void DisableSelf()
    {
        enabled = false;
        effecting = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hitTargets.Contains(other.gameObject.transform.root.gameObject) && effecting)
        {
            hitTargets.Add(other.gameObject.transform.root.gameObject);
            if (other.GetComponentInParent<FirstPersonController>())
            {
                FirstPersonController controller = other.GetComponentInParent<FirstPersonController>();
                if (controller.Grounded)
                {
                    controller._verticalVelocity = Mathf.Sqrt(controller.JumpHeight * -2f * controller.Gravity);
                    controller._fallTimeoutDelta = controller.FallTimeout;
                    controller.Grounded = false;
                    controller.launchTime = .1f;
                }
                else
                {
                    controller._verticalVelocity = 2;
                }
                controller.AddImpact((other.transform.position - transform.position).normalized, 30f);
                controller.Grounded = false;
            }
        }
    }
}
