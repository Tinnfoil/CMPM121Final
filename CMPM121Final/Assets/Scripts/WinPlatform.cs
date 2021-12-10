using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinPlatform : MonoBehaviour
{
    bool won = false;
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
        if (!other.GetComponentInParent<FirstPersonController>() || won) return;
        won = true;
        UI.instance.SetNotification("Level Completed!");
        GameManager.instance.GoToNextLevel(1);
    }
}
