using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class crosshairTween : MonoBehaviour
{

    FirstPersonController player;

    // Start is called before the first frame update
    void Start()
    {

        player = FindObjectOfType<FirstPersonController>();
        player.GetComponent<RocketLauncherController>().OnFire += CrosshairAnimation;
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void CrosshairAnimation()
    {

        gameObject.transform.localScale = new Vector3(2, 2, 2);
        LeanTween.scale(gameObject, new Vector3(1, 1, 1), 0.25f);

    }

}
