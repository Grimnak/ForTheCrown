using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEffectController : MonoBehaviour
{
    private Canvas canvas;
    [SerializeField] private Text textEffect;
    public static UIEffectController instance;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        canvas = gameObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 10;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowTurnBeginEffect(bool isYourTurn)
    {
        //Camera.main.gameObject.GetComponent<Cinemachine.CinemachineBrain>().enabled = false;
        if (isYourTurn)
            textEffect.text = "Your Turn";
        else
            textEffect.text = "Opponent's Turn";
        textEffect.gameObject.transform.localPosition = new Vector3(0, 0, -880);
        textEffect.gameObject.SetActive(true);
        StartCoroutine("Wait2Sec");
    }

    IEnumerator Wait2Sec()
    {
        yield return new WaitForSeconds(2);
        textEffect.gameObject.SetActive(false);
        Camera.main.gameObject.GetComponent<Cinemachine.CinemachineBrain>().enabled = true;
    }

}
