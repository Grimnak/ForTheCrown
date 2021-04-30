using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TogglePair : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Toggle theOther;
    [SerializeField] private string textToDisplay;
    
    private Toggle thisToggle;

    private bool startTimer;
    private float delayTime;
    private float timer;
    private Text thisText;
    void Start()
    {
        ToggleGroup tg = gameObject.GetComponent<ToggleGroup>();
        thisToggle = gameObject.GetComponent<Toggle>();
        if (tg != null)
        {
            theOther.group = tg;
            thisToggle.group = tg;
        }
        timer = 0f;
        delayTime = 0.5f;
        thisText = GameObject.Find("ToggleDescription").GetComponent<Text>();
    }

    public void LockTogglePair(bool val)
    {
        theOther.interactable = false;
        /*
        thisToggle.group.allowSwitchOff = false;
        ColorBlock cb = new ColorBlock();
        cb = thisToggle.colors;
        cb.normalColor = thisToggle.colors.normalColor;
        cb.selectedColor = thisToggle.colors.selectedColor;
        cb.highlightedColor = thisToggle.colors.selectedColor;
        thisToggle.colors = cb;
        //theOther.group = null;
        */
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        timer = 0f;
        startTimer = true;
        thisText.text = textToDisplay;
        // Debug.Log("Enter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        startTimer = false;
        thisText.enabled = false;
    }

    private void Update()
    {
        DelayTimeShowText();
    }

    void DelayTimeShowText()
    {
        if (startTimer)
        {
            thisText.rectTransform.position = Input.mousePosition;
            timer += Time.deltaTime;
            if (timer > delayTime)
            {
                thisText.enabled = true;
                timer = 0f;
            }
        }
    }
}
