using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Capstone
{
    //data structure for status to show in the status UI panel
    //never to be a component of a gameobject
    public class StatusData
    {
        private float curHealth;
        public float CurrentHealth
        {
            get { return curHealth; }
            set
            {
                if (value > 0)
                    curHealth = value;
                else
                    curHealth = 0;
            }
        }

        public float TotalHealth { get; set; }
        public float Attack { get; private set; }
        public int AttackRange { get; set; }
        public int MovingRange { get; set;  }
        public int PopulationValue { get; private set; }
        public string Name { get; }
        //special ability cool down
        public int CoolDown { get; private set; }
        //only for a cleric unit
        public float Heal { get; private set; }
        public int CurPromoPoint { get; private set; }
        public int RequiredPromoPoint { get; }
        public int TotalPromos { get; private set; }
        public List<int> PromoHistory { get; private set; }

        /// <summary>
        /// This method constructs data structure for status to show in the status UI panel.
        /// </summary>
        public StatusData(string name, float curHealth, float totHealth, float attack, int attackRange, int movingRange, int cooldown, int curPromoPoint, int requiredPromPoint, int totPromos, float heal = -1)
        {
            this.Name = name;
            this.CurrentHealth = curHealth;
            this.TotalHealth = totHealth;
            this.Attack = attack;
            this.AttackRange = attackRange;
            this.Heal = heal;
            this.MovingRange = movingRange;
            this.CoolDown = cooldown;
            this.Heal = heal;
            this.CurPromoPoint = curPromoPoint;
            this.RequiredPromoPoint = requiredPromPoint;
            this.TotalPromos = totPromos;
            this.PopulationValue = Global.LookUpPopulationCost(name);
            this.PromoHistory = new List<int>();
        }

        /// <summary>
        /// This method tracks status for each unit.
        /// </summary>
        public void UpdataStatus(float curHealth, int totHealth, int movingRange, int cooldown, int curPromoPoint, int totPromos, int attack, int heal)
        {
            this.CurrentHealth = curHealth;
            this.TotalHealth = totHealth;
            this.MovingRange = movingRange;
            this.CoolDown = cooldown;
            this.CurPromoPoint = curPromoPoint;
            this.TotalPromos = totPromos;
            this.Attack = attack;
            this.Heal = heal;
        }

        public void StorePromoHistory(int selection)
        {
            PromoHistory.Add(selection);
        }

    }
}