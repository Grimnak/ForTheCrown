/* 
 * Author:  Dylan Klingensmith
 * Last Edited By:  Eric Henderson
 * Date Created:  2-9-2021
 * Description:  This script is dynamically attached to each archer unit in the game so it can handle and execute player commands.
 * Filename:  ArcherController.cs
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Capstone
{
    public class ArcherController : UnitController
    {
        public override void Start()
        {
            base.Start();

            unitType = "Archer";
            attackDamage = 25;
            defaultAttackDamage = 25;
            defaultMovementRange = 2;
            currentMovementRange = 2;
            totalHealth = 75;
            currentHealth = 75;
            isAlive = true;
            attackRange = 3;
            defaultAttackRange = 3;
            movementSpeed = 7;
            chargingSpeed = 10;
            hasActed = false;
            remainingSpecialAbilityCooldown = 0;
            totalSpecialAbilityCooldown = 2;
            currentPromotionPoints = 0;
            requiredPromotionPoints = 7;
            maxPromotions = 3;
            totalPromotions = 0;
            attackAudio = "BowNoise";

            //Request and set healthBarController
            if (CombatUIController.instance)
            {
                healthBarController = CombatUIController.instance.onRequestHealthBarController(healthBarDisplayPos, Global.UnitIdx.Archer, totalHealth);
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

        public override IEnumerator StationaryAttack()
        {
            //activate animation lock
            GameLogicManager.animationLock = true;

            animator.speed = 0.5f;

            if (remainingSpecialAbilityCooldown != totalSpecialAbilityCooldown)
            {
                animator.SetTrigger("Attack");
                yield return new WaitForSeconds(1f);
                animator.SetTrigger("AttackReady");

                
                target.GetComponent<UnitController>().Attacked(CalculateDamage(attackDamage));
                yield return new WaitForSeconds(.4f);
                FindObjectOfType<AudioManager>()?.Play("BowNoise");
                yield return new WaitForSeconds(.7f);
                FindObjectOfType<AudioManager>()?.Play("ArrowNoise");

            }
            else
            {
                //allows targettile to last throughout entire IEnumerator
                // GameObject temporaryTargetTile = TileManager.currentlyTargetedTile;

                //allows target to last throughout entire IENUMERATOR
                GameObject temporaryTarget = target;
                ParticleSystem enemyUPS = target.transform.Find("Explosive_Effect").GetComponent<ParticleSystem>();

                //list of units affected by the explosion
                List<GameObject> adjacentUnitList = new List<GameObject>();

                // If firing explosive shot, damage all enemies adjacent to primary target as well.
                if (remainingSpecialAbilityCooldown == totalSpecialAbilityCooldown)
                {
                    animator.SetTrigger("Special");
                    yield return new WaitForSeconds(1f);
                    animator.SetTrigger("AttackReady");

                    for (int candidateTileIndex = 0; candidateTileIndex < LevelGenerator.map.transform.childCount; candidateTileIndex++)
                    {
                        GameObject candidateTile = LevelGenerator.map.transform.GetChild(candidateTileIndex).gameObject;

                        if (TileManager.CombatDistanceBetweenTiles(temporaryTarget.transform.parent.gameObject, candidateTile) == 1)
                        {
                            GameObject unit = TileManager.FindSpecificChildByTag(candidateTile, "Unit");
                            if (unit != null)
                            {
                                adjacentUnitList.Add(unit); 
                            }
                        }
                    }

                    yield return new WaitForSeconds(1.5f);

                    // Primary target takes full damage.
                    temporaryTarget.GetComponent<UnitController>().Attacked(CalculateDamage(attackDamage));

                    // All units adjacent to primary target take half damage.
                    foreach (GameObject unit in adjacentUnitList)
                    {
                        unit.GetComponent<UnitController>().Attacked(CalculateDamage(attackDamage / 2));
                    }

                    //handles bow and fire explosion sounds
                    yield return new WaitForSeconds(.2f);
                    FindObjectOfType<AudioManager>()?.Play("BowNoise");
                    yield return new WaitForSeconds(.7f);
                    FindObjectOfType<AudioManager>()?.Play("ArrowNoise");
                    yield return new WaitForSeconds(.2f);
                    FindObjectOfType<AudioManager>()?.Play("FireNoise");
                    yield return new WaitForSeconds(.1f);

                    //play explosive effect
                    enemyUPS.Play();
                    yield return new WaitForSeconds(.1f);
                }
            }

            yield return new WaitForSeconds(.9f);

            GameLogicManager.animationLock = false;
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Special");
            animator.ResetTrigger("AttackReady");
            animator.speed = 1f;
        }

        /// <summary>
        /// When an archer is fully promoted, its attack range is increased by one.
        /// </summary>
        public override void PromoteFinalTime()
        {
            defaultAttackRange++;
            totalPromotions++;
        }

        /// <summary>
        /// When an archer uses its special ability, it fires an explosive shot that does half damage to all units adjacent to the primary target (including friendly units).
        /// </summary>
        public override void UseSpecialAbility()
        {
            if (!hasActed)
            {
                base.UseSpecialAbility();
            }
        }
    }
}

