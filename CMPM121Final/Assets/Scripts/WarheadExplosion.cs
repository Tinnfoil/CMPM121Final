using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarheadExplosion : MonoBehaviour
{
    public List<GameObject> hitTargets;

    private void Awake()
    {
        hitTargets = new List<GameObject>();        
    }

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
        if (!hitTargets.Contains(other.gameObject))
        {
            hitTargets.Add(other.gameObject);
            // Add force to this target
        }
    }
}
