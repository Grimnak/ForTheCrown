/* 
 * Author:  Dylan Klingensmith
 * Last Edited By: Eric Henderson
 * Date Created:  4-7-2021
 * Description:  This script is dynamically attached to each horseman unit in the game so it can handle and execute player commands.
 * Filename:  HorsemanController.cs
 */

using UnityEngine;
using System.Collections;
using Photon.Pun;

namespace Capstone
{
    public class HorsemanController : UnitController
    {

        public override void Start()
        {
            base.Start();

            unitType = "Horseman";
            healAmount = 0;
            attackDamage = 30;
            defaultAttackDamage = 30;
            defaultMovementRange = 4;
            currentMovementRange = 4;
            totalHealth = 85;
            currentHealth = 85;
            isAlive = true;
            attackRange = 1;
            defaultAttackRange = 1;
            movementSpeed = 10;
            chargingSpeed = 10;
            hasActed = false;
            remainingSpecialAbilityCooldown = 0;
            totalSpecialAbilityCooldown = 2;
            currentPromotionPoints = 0;
            requiredPromotionPoints = 7;
            maxPromotions = 3;
            totalPromotions = 0;
            attackAudio = "StabNoise";

            //Request and set healthBarController
            if (CombatUIController.instance)
            {
                healthBarController = CombatUIController.instance.onRequestHealthBarController(healthBarDisplayPos, Global.UnitIdx.Horseman, totalHealth);
                healthBarController.gameObject.SetActive(false);
            }

            statusData = new StatusData(unitType, currentHealth, totalHealth, attackDamage, attackRange, currentMovementRange, remainingSpecialAbilityCooldown, currentPromotionPoints, requiredPromotionPoints, totalPromotions, heal: healAmount);

        }

        void Update()
        {
            Select();

            //Options for inputs
            if (selected)
            {
                animator.SetBool("IsSelected", true);

                if (!GameLogicManager.animationLock && target != null)
                {
                    //look at your current target; 
                    transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z), Vector3.up);
                }
                SetHealthBarActive(true);
            }
            else if (!GameLogicManager.animationLock)
            {
                animator.SetBool("IsSelected", false);
                SetHealthBarActive(false);
            }
        }

        /// <summary>
        /// When a horseman is fully promoted, it gains one extra movement per turn.
        /// </summary>
        public override void PromoteFinalTime()
        {
            defaultMovementRange++;
            totalPromotions++;
        }

        /// <summary>
        /// When a horseman uses its special ability, it readies itself for a brutal charge, dealing increased damage.
        /// </summary>
        public override void UseSpecialAbility()
        {
            if (!hasActed)
            {
                base.UseSpecialAbility();
                StartCoroutine("HorseSpecial");
                attackRange += 1;

                if (allegiance == GameLogicManager.Instance.myID)
                {
                    TileManager.ShowCombatOptions(gameObject.transform.parent.gameObject, attackRange);
                }
            }
        }

        /// <summary>
        /// This method is called when a horseman attacks.  Decides whether to use moving attack or stationary attack.
        /// </summary>
        public override void Attack()
        {
            if (currentHealth > 0)
            {
                if (allegiance == GameLogicManager.Instance.myID)
                {
                    Select();
                }
                else
                {
                    selected = true;
                    animator.SetBool("IsSelected", true);
                }

                hasActed = true;
                currentPromotionPoints += 2;
                
                if(currentPromotionPoints >= requiredPromotionPoints && totalPromotions <= maxPromotions){
                    ParticleSystem pPS = transform.Find("Promotion_Effect").GetComponent<ParticleSystem>();
                    pPS.Play();
                }
                transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z), Vector3.up);
                currentDirection = target.transform.position - transform.position;
                currentDirection = Vector3.Normalize(currentDirection);

                //determine if we are using the special attack coroutine, or the regular attack coroutine
                if(attackRange == 1){
                    StartCoroutine("StationaryAttack");
                }
                else
                {
                    StartCoroutine("MovingAttack");
                }
                
            }
        }
    
        /// <summary>
        /// This coroutine handles a horse's special ability animations.
        /// </summary>
        public virtual IEnumerator HorseSpecial()
        { 
            //activate animation lock
            GameLogicManager.animationLock = true;
            animator.SetTrigger("Special");
            yield return new WaitForSeconds(.3f);
            FindObjectOfType<AudioManager>()?.Play("NeighNoise");
            yield return new WaitForSeconds(.7f);
            animator.ResetTrigger("Special");
            GameLogicManager.animationLock = false;
        }
    }
}