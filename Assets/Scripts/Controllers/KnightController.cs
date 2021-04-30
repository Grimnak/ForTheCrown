/* 
 * Author:  Dylan Klingensmith
 * Last Edited By:  Eric Henderson
 * Date Created:  1-22-2021
 * Description:  This script is dynamically attached to each knight unit in the game so it can handle and execute player commands.
 * Filename:  KnightController.cs
 */

using UnityEngine;
using System.Collections;
using Photon.Pun;

namespace Capstone
{
    public class KnightController : UnitController
    {
        public override void Start()
        {
            base.Start();

            unitType = "Knight";
            healAmount = 0;
            attackDamage = 33;
            defaultAttackDamage = 33;
            defaultMovementRange = 2;
            currentMovementRange = 2;
            totalHealth = 100;
            currentHealth = 100;
            isAlive = true;
            attackRange = 1;
            defaultAttackRange = 1;
            movementSpeed = 7;
            chargingSpeed = 10;
            hasActed = false;
            remainingSpecialAbilityCooldown = 0;
            totalSpecialAbilityCooldown = 4;
            currentPromotionPoints = 0;
            requiredPromotionPoints = 7;
            maxPromotions = 3;
            totalPromotions = 0;
            damageModifier = 1;
            attackAudio = "SwordNoise";

            //Request and set healthBarController
            if (CombatUIController.instance)
            {
                healthBarController = CombatUIController.instance.onRequestHealthBarController(healthBarDisplayPos, Global.UnitIdx.Knight, totalHealth);
                healthBarController.gameObject.SetActive(false);
            }
            statusData = new StatusData(unitType, currentHealth, totalHealth, 
            attackDamage, attackRange, currentMovementRange, remainingSpecialAbilityCooldown, 
            currentPromotionPoints, requiredPromotionPoints, totalPromotions, heal: healAmount);
        }

        void Update()
        {
            Select();

            //Options for inputs
            if (selected)
            {
                animator.SetBool("IsSelected", true);

                if (!GameLogicManager.animationLock && TileManager.currentlyTargetedTile != null)
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

        public override void Attacked(int damageTaken)
        {
            currentHealth -= (int)(damageTaken * damageModifier);
            currentPromotionPoints++;
            
                if(currentPromotionPoints >= requiredPromotionPoints && totalPromotions <= maxPromotions){
                    ParticleSystem pPS = transform.Find("Promotion_Effect").GetComponent<ParticleSystem>();
                    pPS.Play();
                }
            //choose IsAttacked if unit is still alive after attack.  Choose Death if unit should die with that attack.  
            if (currentHealth <= 0 && isAlive)
            {
                StartCoroutine("IEnumSetDamageText", (int)(damageTaken * damageModifier));
                StartCoroutine("Death");
            }
            else
            {
                StartCoroutine("IEnumSetDamageText", (int)(damageTaken * damageModifier));
                StartCoroutine("IsAttacked");
            }
        }

        /// <summary>
        /// When a knight is fully promoted, it gains one extra movement per turn.
        /// </summary>
        public override void PromoteFinalTime()
        {
            defaultMovementRange++;
            totalPromotions++;
        }

        /// <summary>
        /// When a knight uses its special ability, it raises its shield and takes half damage until its next turn.  This ability counts as the knight's action for the turn.
        /// </summary>
        public override void UseSpecialAbility()
        {
            if (!hasActed)
            {
                base.UseSpecialAbility();
                hasActed = true;
                damageModifier = 0.5f;
                StartCoroutine("DefensiveStance");
                TileManager.DeselectCurrentSelection();
            }
        }

        /// <summary>
        /// This coroutine handles a knight's special ability.
        /// </summary>
        public virtual IEnumerator DefensiveStance()
        {
        
             //activate animation lock
            GameLogicManager.animationLock = true;

            animator.SetBool("Special",true);

            yield return new WaitForSeconds(1f);
            FindObjectOfType<AudioManager>()?.Play("DefendNoise");
            transform.Find("White_Block").GetComponent<ParticleSystem>().Play();

            yield return new WaitForSeconds(1f);

            //remove animation lock and reset trigger
            GameLogicManager.animationLock = false;
        }

    }
}
