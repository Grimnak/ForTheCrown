using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace Capstone
{
public class MapSelection : MonoBehaviour
{
    [SerializeField] private Toggle emptyToggle;
    [SerializeField] private GameObject returnBtn;
    [SerializeField] private GameObject previewPanel;
    [SerializeField] private Image mapPreview;
    [SerializeField] private Text mapObjective;
    [SerializeField] private Sprite[] maps;
    [SerializeField] private string[] mapObjectives;
    public Animator mpAnimator;
    private int mapIndex;

    // Start is called before the first frame update
    void Start()
    {
        mpAnimator = previewPanel.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        returnBtn.SetActive(emptyToggle.isOn);
    }

    public void ShowMapPanel(int idx)
    {
        previewPanel.SetActive(true);
        SelectMap(idx);
        if (mpAnimator != null)
            mpAnimator.SetTrigger("isOn");
    }

    public void HideMap(bool b)
    {
        previewPanel.SetActive(false);
    }

    private void SelectMap(int idx)
    {
        mapPreview.sprite = maps[idx - 1]; // idx is 1-indexed, not 0-indexed. think level 1, 2, 3 etc.
        mapObjective.text = mapObjectives[idx - 1];
        mapIndex = idx;
    }

    public void StartEndlessLevel()
    {
        if (PhotonNetwork.IsConnected)
        {
            RoomManager.Instance.LoadGame(mapIndex);
        }
        else
        {
            Global.activeGameMode = Global.ActiveGameMode.endlessMode;
            UIController.instance.StartEndlessLevel(mapIndex);
        }
    }
}
}