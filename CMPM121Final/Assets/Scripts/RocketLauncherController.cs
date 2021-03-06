using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif
using StarterAssets;
using System;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
[RequireComponent(typeof(PlayerInput))]
#endif
public class RocketLauncherController : MonoBehaviour
{
    public GameObject WarheadPrefab;
    public Transform WarheadTransform;
    public float cooldown = .25f;
    bool reloading = false;
    public Action OnFire;
    // Start is called before the first frame update
    void Start()
    {
        //Time.timeScale = .05f;
        GetComponent<StarterAssetsInputs>().OnRocketButton += FireRocket;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FireRocket()
    {
        if (!reloading)
        {
            Vector3 direction = CalculateFireDirection();
            //GameObject warhead = Instantiate(WarheadPrefab, WarheadTransform.position - direction, Quaternion.LookRotation(direction, Vector3.up));
            GameObject warhead = Instantiate(WarheadPrefab, Camera.main.transform.position, Quaternion.LookRotation(direction, Vector3.up));
            warhead.GetComponent<Warhead>().InitializedWarhead(WarheadTransform.position - direction, Quaternion.LookRotation(direction, Vector3.up), direction * 60);
            warhead.GetComponent<Rigidbody>().AddForce(direction * 60, ForceMode.Impulse);
            WarheadTransform.gameObject.SetActive(false);
            Invoke("ReloadRocket", cooldown);
            OnFire?.Invoke();
        }
        reloading = true;
    }

    public void ReloadRocket()
    {
        WarheadTransform.gameObject.SetActive(true);
        reloading = false;
    }


    public Vector3 CalculateFireDirection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool detectedObject = Physics.Raycast(ray, out hit, 100, ~0, QueryTriggerInteraction.Ignore);
        Vector3 direction = Camera.main.transform.forward;
        if (detectedObject)
        {
            direction = (hit.point - Camera.main.transform.position).normalized;
        }
        else
        {
            direction = ((Camera.main.transform.position + Camera.main.transform.forward * 15) - WarheadTransform.position).normalized;
        }
        return direction;
    }
}
