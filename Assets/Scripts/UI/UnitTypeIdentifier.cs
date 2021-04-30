using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Capstone {
    public class UnitTypeIdentifier : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] public string unitType = "Undefined";

        public UnityEvent<bool> DisplayModelEvent = new UnityEvent<bool>();

        public void OnPointerDown(PointerEventData eventData)
        {
            DisplayModelEvent.Invoke(true);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            DisplayModelEvent.Invoke(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            DisplayModelEvent.Invoke(false);
        }
    }
}
