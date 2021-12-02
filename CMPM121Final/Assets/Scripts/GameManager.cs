using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Transform startPosition;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DieAndRestart()
    {
        UI.instance.SetDeathScreen(true);
        FindObjectOfType<FirstPersonController>().GetComponent<PlayerInput>().SwitchCurrentActionMap("Dead");
        Invoke("ResetPlayer", 1);
    }

    private void ResetPlayer()
    {
        UI.instance.SetDeathScreen(false);
        FirstPersonController controller = FindObjectOfType<FirstPersonController>();
        controller._controller.Move(startPosition.position - controller.transform.position);
        controller.transform.rotation = startPosition.rotation;
        controller.CinemachineCameraTarget.transform.rotation = startPosition.rotation;
        controller.GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");
    }
}
