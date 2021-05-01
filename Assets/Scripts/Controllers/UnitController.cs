/* 
 * Author:  Dylan Klingensmith
 * Last Edited By:  Eric Henderson
 * Date Created:  2-9-2021
 * Description:  This script behaves as a base class for every unit in the game.
 * Filename:  UnitController.cs
 */ 

using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

namespace Capstone
{
    public class UnitController : MonoBehaviour
    {
        /// <summary>
        /// This field references the transform for where the unit's health bar should be displayed.
        /// </summary>
        public Transform healthBarDisplayPos;

        /// <summary>
        /// This property references the unit's health bar manager.
        /// </summary>
        public UnitHealthBarController healthBarController { get; set; }

        /// <summary>
        /// This property stores the data to show in the status UI panel.
        /// </summary>
        public StatusData statusData { get; set; }

        /// <summary>
        /// This property references the animator component attached to the unit.
        /// </summary>
        public Animator animator { get; set; }

        /// <summary>
        /// This property references the rigidbody component attached to the unit.
        /// </summary>
        public Rigidbody rigidBody { get; set; }

        /// <summary>
        /// This property is the class or type of unit.
        /// </summary>
        public string unitType { get; set; }

        /// <summary>
        /// This property is the string of the name of the audio for this units attack
        /// </summary>
        public string attackAudio { get; set; }

        /// <summary>
        /// This property indicates the default amount of spaces that a unit can move per turn.
        /// </summary>
        public int defaultMovementRange { get; set; }

        /// <summary>
        /// This property indicates the amount of remaining spaces that a unit can move this turn.
        /// </summary>
        public int currentMovementRange { get; set; }

        /// <summary>
        /// This property indicates the speed at which the unit moves.
        /// </summary>
        public float movementSpeed { get; set; }

        /// <summary>
        /// This property indicates the speed at which the unit attacks.
        /// </summary>
        public float chargingSpeed { get; set; }

        /// <summary>
        /// This property indicates the amount a unit can heal for.
        /// </summary>
        public int healAmount { get; set; }

        /// <summary>
        /// This property indicates the amount of damage a unit deals when attacking.
        /// </summary>
        public int attackDamage { get; set; }

        /// <summary>
        /// This property indicates the default amount of damage a unit deals when attacking.
        /// </summary>
        public int defaultAttackDamage { get; set; }

        /// <summary>
        /// This property indicates the amount of tiles away from a target the unit can be in order to attack.
        /// </summary>
        public int attackRange { get; set; }

        /// <summary>
        /// This property indicates the default amount of tiles away from a target the unit can be in order to attack.
        /// </summary>
        public int defaultAttackRange { get; set; }

        /// <summary>
        /// This property indicates the current facing direction of the unit.
        /// </summary>
        public Vector3 currentDirection { get; set; }

        /// <summary>
        /// This property indicates the total amount of health a unit has.
        /// </summary>
        public int totalHealth { get; set; }

        /// <summary>
        /// This property indicates the amount of remaining health that a unit currently has.
        /// </summary>
        public int currentHealth { get; set; }

        /// <summary>
        /// This property indicates whether or not a unit's health is above zero.
        /// </summary>
        public bool isAlive { get; set; }

        /// <summary>
        /// This property indicates whether or not a unit has already acted (attacked or healed) this turn.
        /// </summary>
        public bool hasActed { get; set; }

        /// <summary>
        /// This property indicates whether or not the unit is selected.
        /// </summary>
        public bool selected { get; set; }

        /// <summary>
        /// This property represents the unit's target.
        /// </summary>
        public GameObject target { get; set; }

        /// <summary>
        /// This property indicates whether the unit is part of the master's team (0), the opponent's team (1).
        /// </summary>
        public int allegiance { get; set; }

        /// <summary>
        /// This property indicates the current numerical progress towards the unit's next promotion.
        /// </summary>
        public int currentPromotionPoints { get; set; }

        /// <summary>
        /// This property indicates the total required number of promotion points a unit must receive until it is promoted.
        /// </summary>
        public int requiredPromotionPoints { get; set; }

        /// <summary>
        /// This property indicates the maximum number of promotions a unit may receive.
        /// </summary>
        public int maxPromotions { get; set; }

        /// <summary>
        /// This property indicates the total number of promotions a unit currently has.
        /// </summary>
        public int totalPromotions { get; set; }

        /// <summary>
        /// This property indicates how many turns remain before a unit's special ability may be used again.
        /// </summary>
        public int remainingSpecialAbilityCooldown { get; set; }
        /// <summary>
        /// This property indicates how many total turns must pass before a unit's special ability may be used again.
        /// </summary>
        public int totalSpecialAbilityCooldown { get; set; }

        /// <summary>
        /// Determines how this unit's damage received is modified via blocking.  
        /// </summary>
        public float damageModifier;

        /// <summary>
        /// This method takes care of default value instantiation.
        /// </summary>
        public virtual void Start()
        {
            animator = GetComponent<Animator>();
            rigidBody = GetComponent<Rigidbody>();
            if (SceneManager.GetActiveScene().name.Equals("GameScene"))
                DetermineAllegiance(); // do not call if we're previewing the unit in selection phase
        }

        /// <summary>
        /// Correctly sets the allegiance for the unit.
        /// </summary>
        public virtual void DetermineAllegiance()
        {
            if (AIController.Instance != null && AIController.Instance.gameArmy != null && (AIController.Instance.gameArmy.Contains(gameObject) || AIController.Instance.stagingArmy.Contains(gameObject)))
            {
                allegiance = 1;
            }
            else
            {
                if (PhotonNetwork.IsConnected)
                {
                    int l = gameObject.name.Length;
                    string temp = gameObject.name.Remove(l-1);
                    allegiance = int.Parse(gameObject.name[l - 1].ToString());
                    gameObject.name = temp;
                }
                else allegiance = 0;
            }
        }

        /// <summary>
        /// This method checks if a unit is selected by the local player.
        /// </summary>
        public virtual void Select()
        {
            if (gameObject == TileManager.FindSpecificChildByTag(TileManager.currentlySelectedTile, "Unit") && isAlive)
            {
                selected = true;
                if (TileManager.currentlyTargetedTile != null)
                {
                    GameObject currentTarget = TileManager.FindSpecificChildByTag(TileManager.currentlyTargetedTile, "Unit");

                    // Set the unit's target.
                    if (currentTarget != null)
                    {
                        target = currentTarget;
                    }
                    else
                    {
                        target = TileManager.currentlyTargetedTile;
                    }
                }
            }
            else
            {
                selected = false;
            }
        }

        /// <summary>
        /// This method is called when a unit moves.
        /// </summary>
        public virtual void Move()
        {
            if (currentHealth > 0)
            {
                if (allegiance == 0)
                {
                    Select();
                }
                else
                {
                    selected = true;
                    animator.SetBool("IsSelected", true);
                }
                StartCoroutine(Moving());
                ParticleSystem uPS = target.transform.Find("Movement_Effect").GetComponent<ParticleSystem>();
                uPS.Play();
            }
        }

        /// <summary>
        /// Determine whether or not a unit may be promoted.
        /// </summary>
        public virtual bool PromotionAvailable()
        {
            int id = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient ? 0 : 1 : 0;
            PlayerController pc = GameLogicManager.Instance.controllers[id] as PlayerController;
                    
            if (GameLogicManager.IsMyTurn(pc.allegiance) && currentPromotionPoints >= requiredPromotionPoints && totalPromotions < maxPromotions)
            {
                
                return true;
                
            }
            else
            {
                return false;
                
            }
        }

        /// <summary>
        /// Upon receiving a promotion, the player may opt to provide their unit with a one-time heal.
        /// </summary>
        public virtual void PromoteSingleHealOption()
        {
            currentHealth += totalHealth / 2;
            if (currentHealth > totalHealth)
            {
                currentHealth = totalHealth;
            }
            healthBarController.SetCurrentHealth(currentHealth);
            StartCoroutine("SelfHealing");

            currentPromotionPoints = 0;
            totalPromotions++;

            if (totalPromotions == maxPromotions)
            {
                PromoteFinalTime();
            }

            UpdataStatus();
            ParticleSystem pPS = transform.Find("Promotion_Effect").GetComponent<ParticleSystem>();
            pPS.Stop();
           
        }

        /// <summary>
        /// Upon receiving a promotion, the player may opt to provide their unit with a stat increase.
        /// </summary>
        public virtual void PromoteStatsOption()
        {
            totalHealth += 10;
            healthBarController.SetMaxHealth(totalHealth);

            if (healAmount > 0)
            {
                healAmount += 10;
            }
            else
            {
                defaultAttackDamage += 5;
                attackDamage = defaultAttackDamage;
            }

            currentPromotionPoints = 0;
            totalPromotions++;

            if (totalPromotions == maxPromotions)
            {
                PromoteFinalTime();
            }
            
            UpdataStatus();
            ParticleSystem pPS = transform.Find("Promotion_Effect").GetComponent<ParticleSystem>();
            pPS.Stop();
        }

        /// <summary>
        /// Upon receiving its final promotion, a unit will receive a specific bonus stat increase.
        /// </summary>
        public virtual void PromoteFinalTime() { }

        /// <summary>
        /// When a unit is self-healed through a promotion, play the healing particle effect on itself.
        /// </summary>
        public virtual IEnumerator SelfHealing()
        {
            ParticleSystem selfPS = gameObject.transform.Find("Healing_Particles")?.GetComponent<ParticleSystem>();
            //start the healing particle system on self
            selfPS.Play();
            FindObjectOfType<AudioManager>()?.Play("HealNoise");
            yield return new WaitForSeconds(1f);

            //end the healing particle system on self
            selfPS.Stop();
        }

        /// <summary>
        /// This coroutine handles a unit's movement animations.
        /// </summary>
        public virtual IEnumerator Moving(List<(GameObject, GameObject)> pathList = null)
        {
            GameLogicManager.animationLock = true;

            // Create a list of the tiles we will be traveling to in this movement if it wasn't provided.
            if (pathList == null)
            {
                pathList = TileManager.FindPath(LevelGenerator.map, transform.parent.gameObject, target);
            }

            // Check if pathList is still null (i.e. wasn't provided and couldn't be found).
            if (pathList == null)
                yield return null;

            // Performs the running animation.
            animator.SetTrigger("IsMoving");
            yield return new WaitForSeconds(.1f);

            for (int tileNumber = 0; tileNumber < pathList.Count; tileNumber++)
            {
                // Figure out which tile we are heading to this round.
                (_, GameObject nextTile) = pathList[tileNumber];

                // Look at and make this tile our target.
                transform.LookAt(new Vector3(nextTile.transform.position.x, transform.position.y, nextTile.transform.position.z), Vector3.up);
                currentDirection = nextTile.transform.position - transform.position;
                currentDirection = Vector3.Normalize(currentDirection);

                // Moves towards the target.
                rigidBody.velocity = currentDirection * movementSpeed;
                yield return new WaitForSeconds(Vector3.Distance(nextTile.transform.position, transform.position) * (1 / movementSpeed));
            }

            // Stops movement and turns off animation.
            rigidBody.velocity = Vector3.zero;
            animator.SetTrigger("DoneMoving");
            GameLogicManager.animationLock = false;
        }

        /// <summary>
        /// This method is called when a unit's special ability is used.
        /// </summary>
        public virtual void UseSpecialAbility()
        {
            if (remainingSpecialAbilityCooldown <= 0)
            {
                remainingSpecialAbilityCooldown = totalSpecialAbilityCooldown;
            }
        }

        /// <summary>
        /// Calculate the actual damage output of the attack.
        /// </summary>
        /// <param name="baseAttackDamage">This is the amount of damage a unit does before applying any modifiers.</param>
        /// <returns>The actual amount of damage caused.</returns>
        public virtual int CalculateDamage(int baseAttackDamage)
        {
            return baseAttackDamage + (int)UnityEngine.Random.Range(-baseAttackDamage / 8f, baseAttackDamage / 8f);
        }

        /// <summary>
        /// This method is called when a unit attacks.
        /// </summary>
        public virtual void Attack()
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

        /// <summary>
        /// This coroutine handles a unit's melee attack animations.
        /// </summary>
        public virtual IEnumerator MovingAttack()
        {
            //activate animation lock
            GameLogicManager.animationLock = true;

            animator.SetTrigger("Attack");
            yield return new WaitForSeconds(1f);
            ParticleSystem horsePS = transform.Find("Horse_Special_Effect").GetComponent<ParticleSystem>();
            horsePS.Play();
            animator.speed = 0.5f;
            animator.SetTrigger("AttackReady");

            
            //yield return new WaitForSeconds(3f);
            target.GetComponent<UnitController>().Attacked(CalculateDamage(attackDamage));
       
            yield return new WaitForSeconds(.7f);
            FindObjectOfType<AudioManager>()?.Play(attackAudio);
            ParticleSystem strikePS = target.transform.Find("Horse_Strike").GetComponent<ParticleSystem>();
            strikePS.Play();
            yield return new WaitForSeconds(.3f);
            GameLogicManager.animationLock = false;
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("AttackReady");
            animator.speed = 1;
        }

        /// <summary>
        /// This coroutine handles a unit's attack animations.
        /// </summary>
        public virtual IEnumerator StationaryAttack()
        {
        
            //activate animation lock
            GameLogicManager.animationLock = true;

            animator.SetTrigger("Attack");
            yield return new WaitForSeconds(1f);
            

            animator.speed = 0.5f;
            animator.SetTrigger("AttackReady");

            
            //yield return new WaitForSeconds(3f);
            target.GetComponent<UnitController>().Attacked(CalculateDamage(attackDamage));
       
            yield return new WaitForSeconds(.7f);
            FindObjectOfType<AudioManager>()?.Play(attackAudio);
            yield return new WaitForSeconds(.3f);
            GameLogicManager.animationLock = false;
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("AttackReady");
            animator.speed = 1;
        }

        /// <summary>
        /// This method is called when a unit has been attacked.
        /// </summary>
        /// <param name="damageTaken">The amount of damage the unit received.</param>
        public virtual void Attacked(int damageTaken)
        {
            currentHealth -= damageTaken;
            currentPromotionPoints++;
            
                if(currentPromotionPoints >= requiredPromotionPoints && totalPromotions <= maxPromotions){
                    ParticleSystem pPS = transform.Find("Promotion_Effect").GetComponent<ParticleSystem>();
                    pPS.Play();
                }
            
            //choose IsAttacked if unit is still alive after attack.  Choose Death if unit should die with that attack.  
            if (currentHealth <= 0 && isAlive)
            {
                StartCoroutine("IEnumSetDamageText", damageTaken);
                StartCoroutine("Death");
            }
            else
            {
                StartCoroutine("IEnumSetDamageText", damageTaken);
                StartCoroutine("IsAttacked");
            }
            
        }

        /// <summary>
        /// This coroutine handles a unit's damage text when it has been attacked.
        /// </summary>
        /// <param name="damageTaken">The amount of damage the unit received.</param>
        public virtual IEnumerator IEnumSetDamageText(int damageTaken)
        {
            yield return new WaitForSeconds(1f);
            SetDamageText(damageTaken);

            while (true)
            {
                GameObject[] activeDamagePopups = GameObject.FindGameObjectsWithTag(Global.Tags.damage);
                foreach (GameObject damagePopup in activeDamagePopups)
                {
                    damagePopup.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
                }
                yield return null;
            }
        }

        /// <summary>
        /// This coroutine handles a unit's animations when it has been attacked.
        /// </summary>
        public virtual IEnumerator IsAttacked()
        {
            //performs the attacked animation
            //yield return new WaitForSeconds(.4f);
            yield return new WaitForSeconds(1f);
            animator.SetTrigger("IsAttacked");
            ParticleSystem uPS = transform.Find("Blood_Effect").GetComponent<ParticleSystem>();
            uPS.Play();

            healthBarController.SetCurrentHealth(currentHealth);
            statusData.CurrentHealth = currentHealth;
            yield return new WaitForSeconds(1.5f);
            uPS.Stop();
            animator.ResetTrigger("IsAttacked");
        }

        /// <summary>
        /// This coroutine handles a unit's death animations.
        /// </summary>
        public virtual IEnumerator Death()
        {
            //PlayerController pc = GameLogicManager.Instance.controllers[allegiance] as PlayerController;
            bool trainingWin = false;
            PlayerController pc;
            PlayerController pc2;
            /*
            if (AIController.Instance == null) 
            { 
                if(this.allegiance == 0)
                {
                    pc2 = GameLogicManager.Instance.controllers[1] as PlayerController;
                    pc = GameLogicManager.Instance.controllers[0] as PlayerController;
                }
                else
                {
                    pc2 = GameLogicManager.Instance.controllers[0] as PlayerController;
                }
            }*/
            
            //triggers the death animation
            isAlive = false;
            if (AIController.Instance != null && AIController.Instance.gameArmy.Contains(gameObject))
            {
                AIController.Instance.gameArmy.Remove(gameObject);
                //can set win condition here if AI army is empty
                if(AIController.Instance.gameArmy.Count == 0 && !GameLogicManager.Instance.inEndlessMode)
                {
                    trainingWin = true;
                }
                pc = GameLogicManager.Instance.controllers[0] as PlayerController;
                pc.enemiesKilledByPlayer++;
            }
            else
            {
                pc = GameLogicManager.Instance.controllers[allegiance] as PlayerController;
                if (this.allegiance == 0)
                {
                    pc2 = GameLogicManager.Instance.controllers[1] as PlayerController;
                }
                else
                {
                    pc2 = GameLogicManager.Instance.controllers[0] as PlayerController;
                }
                pc.gameArmy.Remove(gameObject);
                pc.unitsLostByPlayer++;
                if(AIController.Instance == null && pc2 != null)            //if AI is null, we should be in MP, check that there's a second PC to be sure, this was causing problems in SP earlier
                {
                    pc2.enemiesKilledByPlayer++;
                }
                if (pc.gameArmy.Count == 0 && GameLogicManager.Instance.inEndlessMode)
                {
                    GameLogicManager.Instance.isPlayerArmyEmpty = true;
                }
                
            }
            yield return new WaitForSeconds(1f);
            healthBarController.SetCurrentHealth(currentHealth);
            statusData.CurrentHealth = currentHealth;
            animator.SetTrigger("Death");
            yield return new WaitForSeconds(1f);
            animator.ResetTrigger("Death");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length + 0.3f);
            gameObject.transform.parent.gameObject.tag = Global.Tags.inactive;
            Destroy(gameObject);

            
            if(trainingWin)
            {
                pc.ShowTrainingWinPanel();          //show training WIN panel here
            }
            else
            {
                if (pc.gameArmy.Count == 0)         //if one of the players is out of units
                {
                    if (PhotonNetwork.IsConnected)           //if we are connected, so we're playing online
                    {
                        pc.ShowMPLoseResultsPanel();            //show loser panel to person who lost their last unit
                        if (this.allegiance == 0)
                        {  pc2 = GameLogicManager.Instance.controllers[1] as PlayerController;  }
                        else { pc2 = GameLogicManager.Instance.controllers[0] as PlayerController;  }

                        pc2.ShowMPWinResultsPanel();            //show winner panel to other player who beat them
                    }
                    else
                    {
                        if (GameLogicManager.Instance.inEndlessMode)
                        {
                            pc.ShowEndlessResultsPanel();                   //show endless panel if in endless
                        }
                        else
                        {
                            if (AIController.Instance.gameArmy.Count > 0)
                            {
                                pc.ShowTrainingLossPanel();                 //show training LOSS panel here      
                            }
                        }
                    }

                }
            }
            

        }

        /// <summary>
        /// This method is called when a unit heals another unit.
        /// </summary>
        public virtual void Heal() { }

         /// <summary>
        /// This coroutine handles a unit's healing animations.
        /// </summary>
        public virtual IEnumerator Healing()
        {
            //activate animation lock
            GameLogicManager.animationLock = true;

            //start the healing animiation
            animator.SetTrigger("IsHealing");   
            yield return new WaitForSeconds(.5f);

            ParticleSystem tPS = target.transform.Find("Healing_Particles").GetComponent<ParticleSystem>();
            //start the healing particle system on the target
            tPS.Play();
            yield return new WaitForSeconds(1f);

            //end the healing particle system on the target
            tPS.Stop();

            //remove animation lock and reset trigger
            GameLogicManager.animationLock = false;
            animator.ResetTrigger("IsHealing");
        }

        /// <summary>
        /// This method is called to determine whether the unit health bar should be shown.
        /// </summary>
        public virtual void SetHealthBarActive(bool isActive)
        {
            if (healthBarController)
                healthBarController.gameObject.SetActive(isActive);
        }

        /// <summary>
        /// This method is called to update the corresponding status panel.
        /// </summary>
        public virtual void UpdataStatus()
        {
            statusData.UpdataStatus(currentHealth, totalHealth, currentMovementRange, remainingSpecialAbilityCooldown, currentPromotionPoints,totalPromotions, attackDamage, healAmount);
        }

        /// <summary>
        /// Make sure status data always exist when enabled
        /// </summary>
        private void OnEnable()
        {
            statusData = new StatusData(unitType, currentHealth, totalHealth, attackDamage, attackRange, currentMovementRange, remainingSpecialAbilityCooldown, currentPromotionPoints, requiredPromotionPoints, totalPromotions, heal: healAmount);
        }

        /// <summary>
        /// This method is called to instantiate and set the popup damage text when a unit is attacked.
        /// </summary>
        /// <param name="damageTaken">The amount of damage the unit received.</param>
        public void SetDamageText(int damageTaken)
        {
            GameObject existingDamageText;

            existingDamageText = Instantiate(Resources.Load("Damage Container") as GameObject, new Vector3(transform.position.x, transform.position.y + 6, transform.position.z), Quaternion.identity);
            existingDamageText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText("-" + damageTaken.ToString());
        }

        /// <summary>
        /// Triggers unit events at the start of your next turn.  
        /// </summary>
        public void TurnEvents()
        {
            if (!GameLogicManager.IsMyTurn(allegiance))
            {
                if (unitType.Equals("Knight") && damageModifier != 1)
                {
                    damageModifier = 1;
                    animator.SetBool("Special", false);
                }

            }
        }
    }
}
