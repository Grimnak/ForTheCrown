/* 
 * Author:  Dylan Klingensmith
 * Last Edited By:  Eric Henderson
 * Date Created:  2-9-2021
 * Description:  This script is dynamically attached to each cleric unit in the game so it can handle and execute player commands.
 * Filename:  ClericController.cs
 */

using System.Collections;
using UnityEngine;
using Photon.Pun;

namespace Capstone
{
    public class ClericController : UnitController
    {
        public override void Start()
        {
            base.Start();

            unitType = "Cleric";
            attackDamage = 20;
            defaultAttackDamage = 20;
            defaultMovementRange = 2;
            currentMovementRange = 2;
            totalHealth = 60;
            currentHealth = 60;
            isAlive = true;
            attackRange = 1;
            defaultAttackRange = 1;
            movementSpeed = 8;
            chargingSpeed = 7;
            hasActed = false;
            healAmount = 40;
            remainingSpecialAbilityCooldown = 0;
            totalSpecialAbilityCooldown = 2;
            currentPromotionPoints = 0;
            requiredPromotionPoints = 7;
            maxPromotions = 3;
            totalPromotions = 0;
            attackAudio = "StaffNoise";

            //Request and set healthBarController
            if (CombatUIController.instance)
            {
                healthBarController = CombatUIController.instance.onRequestHealthBarController(healthBarDisplayPos, Global.UnitIdx.Cleric, totalHealth);
                healthBarController.gameObject.SetActive(false);
            }

            statusData = new StatusData(unitType, currentHealth, totalHealth, attackDamage, attackRange,currentMovementRange, remainingSpecialAbilityCooldown, currentPromotionPoints, requiredPromotionPoints, totalPromotions, heal: healAmount);

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

        public override void Attack()
        {
            Select();

            hasActed = true;
            currentPromotionPoints++;
            
                if(currentPromotionPoints >= requiredPromotionPoints && totalPromotions <= maxPromotions){
                    ParticleSystem pPS = transform.Find("Promotion_Effect").GetComponent<ParticleSystem>();
                    pPS.Play();
                }
            transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z), Vector3.up);
            currentDirection = target.transform.position - transform.position;
            currentDirection = Vector3.Normalize(currentDirection);
            StartCoroutine("StationaryAttack");
        }

        public override void Heal()
        {
            bool actuallyHealed = false;

            //select the unit and its target
            Select();
            UnitController tUnitController = target?.GetComponent<UnitController>();
            // Special ability activated.
            if (remainingSpecialAbilityCooldown == totalSpecialAbilityCooldown)
            {
                for (int candidateTileIndex = 0; candidateTileIndex < LevelGenerator.map.transform.childCount; candidateTileIndex++)
                {
                    GameObject candidateTile = LevelGenerator.map.transform.GetChild(candidateTileIndex).gameObject;

                    if (TileManager.CombatDistanceBetweenTiles(gameObject.transform.parent.gameObject, candidateTile) == 1)
                    {
                        GameObject unit = TileManager.FindSpecificChildByTag(candidateTile, "Unit");
                        UnitController unitController = unit?.GetComponent<UnitController>();

                        if (unit != null && unitController.allegiance == allegiance && unitController.currentHealth < unitController.totalHealth)
                        {
                            unitController.currentHealth = unitController.currentHealth + healAmount;

                            actuallyHealed = true;

                            //if you healed the unit for more than their max health, bring their health to their max health
                            if (unitController.currentHealth > unitController.totalHealth)
                            {
                                unitController.currentHealth = unitController.totalHealth;
                            }

                            //Update UI with new health values
                            unitController.healthBarController.SetCurrentHealth(unitController.currentHealth);
                            unitController.statusData.CurrentHealth = unitController.currentHealth;
                        }
                    }
                }
            }
            // Special ability not activated.
            else
            {
                if (tUnitController.currentHealth < tUnitController.totalHealth)
                {
                    tUnitController.currentHealth += healAmount;

                    actuallyHealed = true;

                    //if you healed the target for more than their max health, bring their health to their max health
                    if (tUnitController.currentHealth > tUnitController.totalHealth)
                    {
                        tUnitController.currentHealth = tUnitController.totalHealth;
                    }

                    //Update UI with new health values
                    tUnitController.healthBarController.SetCurrentHealth(tUnitController.currentHealth);
                    tUnitController.statusData.CurrentHealth = tUnitController.currentHealth;

                    //look at the target
                    transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z), Vector3.up);
                    currentDirection = target.transform.position - transform.position;
                    currentDirection = Vector3.Normalize(currentDirection);
                }
            }

            //State that the unit has done its action for this turn
            hasActed = true;

            //start the healing coroutine
            StartCoroutine("Healing");

            // Provide promotion points if healing was done.
            if (actuallyHealed)
            {
                currentPromotionPoints += 2;
                
                if(currentPromotionPoints >= requiredPromotionPoints && totalPromotions <= maxPromotions){
                    ParticleSystem pPS = transform.Find("Promotion_Effect").GetComponent<ParticleSystem>();
                    pPS.Play();
                }
            }
        }

        public override IEnumerator Healing()
        {
            //activate animation lock
            GameLogicManager.animationLock = true;

            //start the healing animiation
            animator.SetTrigger("IsHealing");
            yield return new WaitForSeconds(.5f);

            //change particle location based on whether special ability is being used or not
            if (remainingSpecialAbilityCooldown == totalSpecialAbilityCooldown)
            {
                ParticleSystem selfPS = transform.Find("AOE_Healing_Particles")?.GetComponent<ParticleSystem>();
                //start the healing particle system on self
                selfPS.Play();
                FindObjectOfType<AudioManager>()?.Play("HealNoise");
                yield return new WaitForSeconds(1f);

                //end the healing particle system on self
                selfPS.Stop();
            }
            else
            {
                ParticleSystem targetPS = target.transform.Find("Healing_Particles")?.GetComponent<ParticleSystem>();
                //start the healing particle system on the target
                targetPS.Play();
                FindObjectOfType<AudioManager>()?.Play("HealNoise");
                yield return new WaitForSeconds(1f);

                //end the healing particle system on the target
                targetPS.Stop();
            }

            //remove animation lock and reset trigger
            GameLogicManager.animationLock = false;
            animator.ResetTrigger("IsHealing");
            
        }

        /// <summary>
        /// When a cleric is fully promoted, it gains more attack damage.
        /// </summary>
        public override void PromoteFinalTime()
        {
            defaultAttackDamage += 15;
            totalPromotions++;
        }

        /// <summary>
        /// When a cleric uses its special ability, it heals all friendly adjacent units.  This ability counts as the cleric's action for the turn.
        /// </summary>
        public override void UseSpecialAbility()
        {
            if (!hasActed)
            {
                base.UseSpecialAbility();
                Heal();
                TileManager.DeselectCurrentSelection();
            }
        }
    }
}
