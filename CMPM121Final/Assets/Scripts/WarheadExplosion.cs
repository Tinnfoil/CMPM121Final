using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarheadExplosion : MonoBehaviour
{
    public List<GameObject> hitTargets;
    public GameObject explosionSphere;

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
    }

    public void DisableSelf()
    {
        enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hitTargets.Contains(other.gameObject.transform.root.gameObject))
        {
            hitTargets.Add(other.gameObject.transform.root.gameObject);
            if (other.GetComponentInParent<FirstPersonController>())
            {
                other.GetComponentInParent<FirstPersonController>().AddImpact((other.transform.position - transform.position).normalized, 30f);
                other.GetComponentInParent<FirstPersonController>().Grounded = false;
                other.GetComponentInParent<FirstPersonController>()._verticalVelocity = 2;
            }
            // Add force to this target
        }
    }
}
