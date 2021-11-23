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
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hitTargets.Contains(other.gameObject))
        {
            hitTargets.Add(other.gameObject);
            // Add force to this target
        }
    }
}
