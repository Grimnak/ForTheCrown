using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Capstone
{
public class Loading : MonoBehaviour
{
    [SerializeField]
    private float fps = 5.0f;
    private float time;

    [SerializeField]
    private Sprite[] animations;
    private int curTextureIdx;

    [SerializeField]
    private Image LogoImg;
    [SerializeField]
    private Text LoadingText;

    AsyncOperation async;

    int progress = 0;

    void Start()
    {
        StartCoroutine(LoadScene());
        FindObjectOfType<AudioManager>().Play("LoadingBackground");
    }

    IEnumerator LoadScene()
    {
        while (Global.menuScene == "MainMenuScene" && !Global.oneLogoCycleDone)
        {
            yield return null;  //at least one cycle for the initial loading
        }
        async = SceneManager.LoadSceneAsync(Global.menuScene);
        //return when loading finished
        yield return async;
    }

    // Update is called once per frame
    void Update()
    {
        if (async != null)
            progress = (int)(async.progress * 100);
        DrawAnimation(animations);
    }

    void DrawAnimation(Sprite[] tex)
    {
        time += Time.deltaTime;

        if(time >= fps)
        {
            curTextureIdx++;
            time = 0;

            if (curTextureIdx >= tex.Length)
            {
                Global.oneLogoCycleDone = true;
                curTextureIdx = 0;
            }
        }

        LogoImg.sprite = tex[curTextureIdx];
        LoadingText.text = progress > 0 ? "LOADING..." + progress + "%" : "LOADING...";
    }
}
}