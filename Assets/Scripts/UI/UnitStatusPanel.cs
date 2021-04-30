using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Capstone
{
    public class UnitStatusPanel : MonoBehaviour
    {
        [SerializeField] private UICircle unitIcon;
        [SerializeField] private Text nameTxt;
        [SerializeField] private Text healthTxt;
        [SerializeField] private Text attackTxt;
        [SerializeField] private GameObject healObj;
        [SerializeField] private Text healTxt;
        [SerializeField] private Text movingRangeTxt;
        [SerializeField] private Text coolDownTxt;
        [SerializeField] private Text promoPointsTxt;
        [SerializeField] private UICircle rankStar;
        [SerializeField] private Slider promoProgress;
        [SerializeField] private List<Toggle> promoToggles;

        private StatusData tmpData;

        // Start is called before the first frame update
        void Start() { }
        // Update is called once per frame
        void Update() { }
  
        //Please add additonal information via modifying StatusData class
        public void SetPanel(StatusData data)
        {
            tmpData = data;
            unitIcon.texSelector(Global.LookUpUnitIdx(data.Name));
            nameTxt.text = data.Name;
            //currently check via if
            showSpecializationTxt();
            healthTxt.text = data.CurrentHealth.ToString() + " / " + data.TotalHealth;
            attackTxt.text = data.Attack.ToString();
            movingRangeTxt.text = data.MovingRange.ToString();
            coolDownTxt.text = data.CoolDown.ToString();

            int promotionPoints = data.TotalPromos * data.RequiredPromoPoint + data.CurPromoPoint;
            promoProgress.maxValue = 3 * data.RequiredPromoPoint;
            promoProgress.value = promotionPoints <= promoProgress.maxValue ? promotionPoints : promoProgress.maxValue;
            promoPointsTxt.text = (promoProgress.value <= promoProgress.maxValue ? promoProgress.value : promoProgress.maxValue).ToString();

            foreach (Toggle t in promoToggles)
            {
                t.isOn = false;
                t.interactable = false;             
            }
            
            if (data.TotalPromos < 3)
                ActivePromoToggles(data.TotalPromos * data.RequiredPromoPoint + data.CurPromoPoint, data.RequiredPromoPoint);

            RestorePromoHistory();
        }

        private void showSpecializationTxt()
        {
            healObj.SetActive(tmpData.Name.Contains("Cleric") || tmpData.Heal > 0);       
            healTxt.text = tmpData.Heal.ToString();
        }

        public void AddRankStar(int idx)
        {
            rankStar.texSelector(idx);
            //UnitController forced promotion points to 0 after one promotion, can not promote consecutively for now 
        }

        //lazy code but no need to generalize for now
        public void ActivePromoToggles(int curVal, int reqVal)
        { 
            if (curVal >= 3 * reqVal && tmpData.TotalPromos == 2)
            {
                promoToggles[5].interactable = true;
                promoToggles[4].interactable = true;
            }
            else if (curVal >= 2 * reqVal && tmpData.TotalPromos == 1)
            {
                promoToggles[3].interactable = true;
                promoToggles[2].interactable = true;
            }
            else if (curVal >= reqVal && tmpData.TotalPromos == 0)
            {
                promoToggles[1].interactable = true;
                promoToggles[0].interactable = true;
            }

        }

        public void RestorePromoHistory()
        {
            foreach(int x in tmpData.PromoHistory)
            {
                promoToggles[x].interactable = true;
                promoToggles[x].isOn = true;
                promoToggles[x].Select();
            }
        }

        public void ConfirmPromotion(int x)
        {
            bool shorter = CombatUIController.instance.selectedUnitController != null && CombatUIController.instance.selectedUnitController.allegiance == GameLogicManager.Instance.myID;
            if (promoToggles[x].isOn && !tmpData.PromoHistory.Contains(x) && shorter)
            {
                tmpData.StorePromoHistory(x);
                CombatUIController.instance.ManagePromotion(x);
            }
        }

    }
}