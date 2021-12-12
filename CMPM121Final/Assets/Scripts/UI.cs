using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI : MonoBehaviour
{
    public static UI instance;

    public Image DeathBG;
    public TextMeshProUGUI Notification;

    private void Awake()
    {
        if(instance == null)
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
        SetNotification("");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDeathScreen(bool enabled)
    {
        DeathBG.gameObject.SetActive(enabled);
    }

    public void SetNotification(string text)
    {
        StopAllCoroutines();
        Notification.GetComponent<CanvasGroup>().alpha = 0;
        LeanTween.alphaCanvas(Notification.GetComponent<CanvasGroup>(), 1, 1).setEaseOutExpo();
        Notification.text = text;
        Invoke("FadeAwayText", 2);
    }

    public void FadeAwayText()
    {
        LeanTween.alphaCanvas(Notification.GetComponent<CanvasGroup>(), 0, 1).setEaseOutExpo();
    }

}
