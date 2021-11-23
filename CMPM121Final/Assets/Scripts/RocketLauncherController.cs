using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif
using StarterAssets;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
[RequireComponent(typeof(PlayerInput))]
#endif
public class RocketLauncherController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<StarterAssetsInputs>().OnRocketButton += FireRocket;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FireRocket()
    {

    }
}
