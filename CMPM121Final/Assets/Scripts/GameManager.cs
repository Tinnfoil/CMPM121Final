using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Transform startPosition;
    public string LevelPrompt = "Level 1";
    public string NextSceneName = "RocketScene";

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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        UI.instance.SetNotification(LevelPrompt);
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
        FindObjectOfType<FirstPersonController>().isDead = true;
        FindObjectOfType<PlayerInput>().SwitchCurrentActionMap("Dead");
        Invoke("ResetPlayer", 1);
    }

    private void ResetPlayer()
    {
        UI.instance.SetDeathScreen(false);
        CharacterController controller = FindObjectOfType<CharacterController>();
        controller.enabled = false;
        controller.transform.position = startPosition.position;
        controller.enabled = true;
        controller.transform.rotation = startPosition.rotation;
        if (FindObjectOfType<FirstPersonController>())
        {
            FindObjectOfType<FirstPersonController>().CinemachineCameraTarget.transform.rotation = startPosition.rotation;
            FindObjectOfType<FirstPersonController>().isDead = false;
        }
        else if (FindObjectOfType<FirstPersonControllerGrapple>())
        {
            FindObjectOfType<FirstPersonControllerGrapple>().CinemachineCameraTarget.transform.rotation = startPosition.rotation;
        }

        controller.GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");

        foreach(Triggerable t in FindObjectsOfType<Triggerable>())
        {
            t.Reset();
        }
        foreach (Trigger t in FindObjectsOfType<Trigger>())
        {
            t.Reset();
        }
    }

    public void GoToNextLevel(float delay)
    {
        Invoke("LoadNextScene", delay);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(NextSceneName, LoadSceneMode.Single);
    }
}
