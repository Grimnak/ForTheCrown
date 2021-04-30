using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Capstone
{
public class ButtonInstruction : MonoBehaviour
{
    public Image textImg;

    void OnMouseEnter()
    {
        textImg.gameObject.SetActive(true);
    }

    void OnMouseExit()
    {
        textImg.gameObject.SetActive(false);
    }
}
}