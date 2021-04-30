/* 
 * Author:  Dylan Klingensmith
 * Last Edited By:  Eric Henderson
 * Date Created:  2-9-2021
 * Description:  This script is dynamically attached to each siege unit in the game so it can handle and execute player commands.
 * Filename:  SiegeController.cs
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Capstone
{
    public class SiegeController : UnitController
    {
        public override void Start()
        {
            base.Start();

            unitType = "Siege";
            attackDamage = 180;
            defaultAttackDamage = 180;
            healAmount = 0;
            defaultMovementRange = 1;
            currentMovementRange = 1;
            totalHealth = 60;
            currentHealth = 60;
            isAlive = true;
            attackRange = 6;
            defaultAttackRange = 6;
            movementSpeed = 4;
            chargingSpeed = 10;
            hasActed = false;
            remainingSpecialAbilityCooldown = 2;
            totalSpecialAbilityCooldown = 2;
            currentPromotionPoints = 0;
            requiredPromotionPoints = 2;
            maxPromotions = 3;
            totalPromotions = 0;

            //Request and set healthBarController
            if (CombatUIController.instance)
            {
                healthBarController = CombatUIController.instance.onRequestHealthBarController(healthBarDisplayPos, Global.UnitIdx.Siege, totalHealth);
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
                if (!GameLogicManager.animationLock && target != null)
                {
                    //look at your current target; 
                    transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z), Vector3.up);
                }
                SetHealthBarActive(true);
            }
            else if (!GameLogicManager.animationLock)
            {
                SetHealthBarActive(false);
            }
        }

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
                transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z), Vector3.up);
                currentDirection = target.transform.position - transform.position;
                currentDirection = Vector3.Normalize(currentDirection);
                /*
                PlayerController pc = GameLogicManager.Instance.controllers[allegiance] as PlayerController;
                if (AIController.Instance != null)               //if AI exists, we will only have one player controller, for the player
                {
                    if(!AIController.Instance.gameArmy.Contains(gameObject) && allegiance == 0)            //AI doesn't control this, must mean its a player unit
                    {
                        pc.AttacksThrown++;
                    }
                }
                else
                {
                    if(PhotonNetwork.IsConnected)           //otherwise if we are connected, just add attack to whichever player through it
                    {
                        pc.AttacksThrown++;
                    }
                }*/
                StartCoroutine("StationaryAttack");
            }
        }

        public override IEnumerator StationaryAttack()
        {
            //activate animation lock
            GameLogicManager.animationLock = true;
          
            animator.SetTrigger("Attack");
            yield return new WaitForSeconds(1f);
            animator.SetTrigger("AttackReady");
            yield return new WaitForSeconds(.5f);
            FindObjectOfType<AudioManager>()?.Play("CatapultNoise");

            target.transform.parent.gameObject.GetComponent<TileHelper>().activeShotSiegeList.Add((gameObject, attackDamage, allegiance));

            for (int candidateTileIndex = 0; candidateTileIndex < LevelGenerator.map.transform.childCount; candidateTileIndex++)
            {
                GameObject candidateTile = LevelGenerator.map.transform.GetChild(candidateTileIndex).gameObject;

                if (TileManager.CombatDistanceBetweenTiles(target.transform.parent.gameObject, candidateTile) <= 1)
                {
                    candidateTile.GetComponent<TileHelper>().pendingRockBlastCount++;
                    candidateTile.transform.Find("Incoming_Effect").GetComponent<ParticleSystem>().Play();
                }
            }
            
            GameLogicManager.animationLock = false;
            
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("AttackReady");
        }

        /// <summary>
        /// When a siege weapon is fully promoted, it gains one extra movement per turn.
        /// </summary>
        public override void PromoteFinalTime()
        {
            defaultMovementRange++;
            totalPromotions++;
        }

        /// <summary>
        /// When a siege weapon uses its special ability, it may fire anywhere on the map for this turn.
        /// </summary>
        public override void UseSpecialAbility()
        {
            if (!hasActed)
            {
                base.UseSpecialAbility();
                attackRange = Mathf.Max(LevelGenerator.gridLength, LevelGenerator.gridWidth);

                if (allegiance == GameLogicManager.Instance.myID)
                {
                    TileManager.ShowCombatOptions(gameObject.transform.parent.gameObject, attackRange);
                }
            }
        }

        public override IEnumerator IsAttacked()
        {
            //performs the attacked animation
            yield return new WaitForSeconds(1f);
            animator.SetTrigger("IsAttacked");
            healthBarController.SetCurrentHealth(currentHealth);
            statusData.CurrentHealth = currentHealth;
            yield return new WaitForSeconds(1.5f);
            animator.ResetTrigger("IsAttacked");
        }
    }
}
