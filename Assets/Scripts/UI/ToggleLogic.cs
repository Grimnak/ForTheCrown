using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Capstone
{
    public class ToggleLogic : MonoBehaviour
    {
        [SerializeField] private GameObject modelToDisplay;
        [SerializeField] public string unitType = "Undefined";
        [SerializeField] private int depth;
        [SerializeField] private int rotateFactor;
        public UnityEvent<StatusData> clickEvent = new UnityEvent<StatusData>();
        public UnityAction<bool> DisplayModelAction;
        void Start()
        {
            if (PopulationUI.instance.selectionPhase)
            {
                clickEvent.AddListener(PopulationUI.instance.textAction);
                DisplayModelAction = new UnityAction<bool>(OnValueChanged);
            }
            depth = -10;
            rotateFactor = 30;
        }

        void Update()
        {
            if (PopulationUI.instance.selectionPhase)
            {
                if (modelToDisplay.activeSelf)
                {
                    StatusData data = modelToDisplay.GetComponent<UnitController>().statusData;
                    modelToDisplay.transform.Rotate(rotateFactor * Vector3.up * Time.deltaTime);
                    clickEvent.Invoke(data);
                    modelToDisplay.transform.Rotate(new Vector3(0, 20, 0) * Time.deltaTime);
                }
            }
        }

        private void OnDisable()
        {
            if (modelToDisplay != null)
                modelToDisplay.SetActive(false);
        }

        public void OnValueChanged(bool value)
        { 
            if (modelToDisplay != null)
                modelToDisplay.SetActive(value);
        }

        public void SpawnNewPopuItem(Vector3 scale, Vector3 pos, float width, float height)
        {
            GameObject instance = (GameObject) Instantiate(Resources.Load("UnitBody"+unitType), new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation) as GameObject;
            instance.transform.SetParent(transform);   
            instance.transform.localScale = scale;
            instance.transform.localPosition = pos;
            instance.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
            instance.GetComponent<Image>().raycastTarget = true;
            if (PopulationUI.instance.selectionPhase)
                instance.GetComponent<UnitTypeIdentifier>().DisplayModelEvent.AddListener(DisplayModelAction);
        }
    }
}