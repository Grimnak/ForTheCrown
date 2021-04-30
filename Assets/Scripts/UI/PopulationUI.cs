using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;

namespace Capstone
{
    public class PopulationUI : MonoBehaviourPunCallbacks
    {
        public static PopulationUI instance;
        // Start is called before the first frame update
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private ScrollRect viewScroll;
        [SerializeField] private Text healthText;
        [SerializeField] private Text attackText;
        [SerializeField] private Text populationText;
        [SerializeField] private Text attackRangeText;
        [SerializeField] private Text moveRangeText;
        [SerializeField] private GameObject uiCanvas;
        [SerializeField] private Transform endTrans;
        [SerializeField] private Button toBattleBtn;
        [SerializeField] private Text populationPoints;
        [SerializeField] private ToggleLogic[] toggles;

        public bool selectionPhase = true;
        public UnityAction<StatusData> textAction;    //unit description

        //drag and drop event
        private PointerEventData pointerEventData;
        private GraphicRaycaster gr;

        private Transform currentGameObject;
        private Transform targetContainer;
        private Image currentItem;
        public string CurrentUnitType { private set; get; }

        private string targetTag = "Board"; //valid location tag
        private string itemTag = "PopuItem";

        private Vector3 startPos;
        private Vector3 offset;
        private float depth;

        public bool Stop = true;
        public int populationPointsToSpend;
        private bool fromItemPool = false;
    
        private List<RaycastResult> mousRayResults = new List<RaycastResult>();

        private List<Transform> armyList = new List<Transform>();

        public UnityEvent<string> RemoveUnitEvent;
        public UnityEvent<string> AddUnitEvent;

        private void Awake()
        {
            if (instance != null) Destroy(gameObject);
            instance = this;
            textAction = new UnityAction<StatusData>(ShowDescription);
            RemoveUnitEvent = new UnityEvent<string>();
            AddUnitEvent = new UnityEvent<string>();
        }

        void Start()
        {
            SetGraphicRaycaster(this.uiCanvas);
            ObjectManager.DontDestroyOnLoad(gameObject);
        }

        // Update is called once per frame
        void Update()
        {
            GetOverUI();
            if (selectionPhase)
            {
                if (armyList.Count > 0)
                {
                    for (int i = 0; i < armyList.Count; ++i)
                    {
                        armyList[i].position = new Vector3(endTrans.position.x + (i % 8) * 120, endTrans.position.y - (i / 8) * 120, -10 - i);
                        armyList[i].localScale = new Vector3(2f, 2f, 1f);
                    }
                    // if we're in multiplayer, only allow host to see the start battle btn
                    if (PhotonNetwork.IsConnected) { if (PhotonNetwork.IsMasterClient) toBattleBtn.gameObject.SetActive(true); }
                    else toBattleBtn.gameObject.SetActive(true);
                }
                if (!toggleGroup.AnyTogglesOn())
                {
                    healthText.text = "?";
                    attackText.text = "?";
                    populationText.text = "?";
                    attackRangeText.text = "?";
                    moveRangeText.text = "?";
                }
            }
        }

        private void GetOverUI()
        {
            if (Input.GetMouseButtonDown(0))
            {
                GetTransform(out currentGameObject, out startPos);
                if (currentGameObject != null && currentGameObject.tag.Contains(itemTag))
                {
                    depth = Camera.main.WorldToScreenPoint(currentGameObject.position).z;
                    offset = currentGameObject.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth));
                    Stop = false;
                    if (fromItemPool)
                    {
                        RectTransform rt = currentGameObject.GetComponent<RectTransform>();
                        currentGameObject.parent.GetComponent<ToggleLogic>().SpawnNewPopuItem(currentGameObject.localScale, currentGameObject.localPosition, rt.sizeDelta.x, rt.sizeDelta.y);                  
                    }
                    if (selectionPhase)
                    {
                        viewScroll.horizontal = false;
                        if (armyList.Contains(currentGameObject))
                        {
                            CurrentUnitType = currentGameObject.GetComponent<UnitTypeIdentifier>().unitType;
                            armyList.Remove(currentGameObject);
                            RemoveUnitEvent.Invoke(CurrentUnitType);
                        }
                    }
                    else
                    {
                        currentGameObject.SetParent(CombatUIController.instance.rootCanvas.transform);         
                    }
                    currentGameObject.transform.SetAsLastSibling();
                }
                else
                {
                    fromItemPool = false;
                    Stop = true;
                }
            }
            if (!Stop)
            {
                if (Input.GetMouseButton(0))
                {
                    DragProcess();
                }

                if (Input.GetMouseButtonUp(0))
                {
                    //mouseUp = true;
                    DropItem();
                    if (currentItem != null)
                        currentItem.raycastTarget = true;
                }
            }
        }

        private void DragProcess()
        {
            if (currentItem == null || currentItem.raycastTarget)
            {
                currentItem = currentGameObject.GetComponent<Image>();
                currentItem.raycastTarget = false;
                CurrentUnitType = currentGameObject.GetComponent<UnitTypeIdentifier>().unitType;
            }
            Vector3 curScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace) + offset;
            if (selectionPhase)
                currentGameObject.position = curPosition;
            else
                currentGameObject.position = curScreenSpace;
        }

        private void DropItem()
        {
            bool fromItemPool = this.fromItemPool;
            Vector3 outPos;
            GetTransform(out targetContainer, out outPos);
            if (targetContainer == null)
            {
                currentItem = null;
                Destroy(currentGameObject.gameObject);
            }
            else if (targetContainer.tag.Contains(targetTag))
            {
                armyList.Add(currentGameObject);
                currentGameObject.transform.SetParent(endTrans);
                AddUnitEvent.Invoke(CurrentUnitType);
            }
            else
            {
                currentItem = null;
                Destroy(currentGameObject.gameObject);
            }
            Stop = true;
            viewScroll.horizontal = true;
            this.fromItemPool = false;
        }

        private void GetTransform(out Transform trans, out Vector3 record)
        {
            mousRayResults.Clear();
            if (pointerEventData == null)
                pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;
            gr.Raycast(pointerEventData, mousRayResults);
            if (mousRayResults.Count != 0)
            {
                trans = mousRayResults[0].gameObject.transform;
                for (int i = 0; i < mousRayResults.Count; ++i)
                {
                    if (mousRayResults[i].gameObject.tag.Contains("ItemPool"))
                    {
                        this.fromItemPool = true;
                    }
                    if (mousRayResults[i].gameObject.tag.Contains(targetTag) && !Stop)
                    {
                        trans = mousRayResults[i].gameObject.transform;
                    }
                }           
                record = trans.position;
            }
            else
            {
                trans = null;
                record = Vector3.zero;
            }
        }

        private void ShowDescription(StatusData sd)
        {
            healthText.text = sd.TotalHealth.ToString();
            attackText.text = sd.Attack.ToString();
            populationText.text = sd.PopulationValue.ToString();
            attackRangeText.text = sd.AttackRange.ToString();
            moveRangeText.text = sd.MovingRange.ToString();
        }

        public void MoveToPlacementPhase()
        {
            if (PhotonNetwork.IsConnected) photonView.RPC("TransitionToGameScene", RpcTarget.All, Global.activeGameMap);
            else TransitionToGameScene(Global.activeGameMap);
        }

        [PunRPC]
        public void TransitionToGameScene(int map =-1)
        {
            selectionPhase = false;
            Stop = true;
            fromItemPool = false;
            pointerEventData = null;
            Debug.Log("active game map:" + Global.activeGameMap);
            if (map!=-1) Global.activeGameMap = map; // for communicating map to multiplayer client
            SceneManager.LoadScene("GameScene");
        }
        public void SetGraphicRaycaster(GameObject c)
        {
            gr = c.GetComponent<GraphicRaycaster>();
        }

        public void SetPopulationPoints(int value)
        {
            if (!selectionPhase) return;
            populationPoints.text = "Available Population Points: " + value.ToString();
            populationPointsToSpend = value;
            CheckTogglesActive();
        }

        private void CheckTogglesActive()
        {
            foreach(ToggleLogic unitToggle in toggles)
            {
                unitToggle.gameObject.SetActive(populationPointsToSpend >= Global.LookUpPopulationCost(unitToggle.unitType));
            }
        }
    }
}