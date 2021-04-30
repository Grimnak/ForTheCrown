using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Capstone
{
public class UnitHealthBarController : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private UICircle profile;
    private Coroutine relocate;
    
    public Vector3 posToFocus { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetProfileImg(int unitIdx)
    {
        profile.texSelector(unitIdx);
    }

    public void SetCurrentHealth(int val)
    {
        healthSlider.value = val;
    }
        
    public void SetMaxHealth(int val)
    {
        healthSlider.maxValue = val;
    }

    void OnEnable()
    {
        relocate = StartCoroutine("Relocating");
    }

    void OnDisable()
    {
        StopCoroutine(relocate);
    }

    IEnumerator Relocating()
    {
        while (true)
        {
            //Always lookat the mainCamera
            transform.parent.rotation = Quaternion.LookRotation(-Camera.main.transform.forward, Vector3.up);
            yield return null;
        }
    }
}
}