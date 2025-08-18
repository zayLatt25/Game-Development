using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SmallScaleInc.TopDownPixelCharactersPack1
{
    public class AnimationController : MonoBehaviour
    {
        private Animator animator;
        public Animator muzzleAnimator; 
        public SpriteRenderer muzzleFlashRenderer;

        public string currentDirection = "isEast"; // Default direction
        public bool isCurrentlyRunning; //for debugging purposes
        public bool isCrouching = false;
        public bool isDying = false;
        private PlayerController playerController;
            // Lists for prefabs
        [SerializeField] private List<GameObject> bloodPrefabs = new List<GameObject>();
        [SerializeField] private List<GameObject> radiatedPrefabs = new List<GameObject>();

        public bool isRadiated = false; // Determines whether to use radiated effects


        public float rollTime = 0.5f; //the time it takes to peform a roll before swtiching back to default animation.

        void Start()
        {
            
            animator = GetComponent<Animator>();
            playerController = GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController script not found on the same GameObject!");
            }
            animator.SetBool("isEast", true); //Sets the default direction to east.
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            animator.SetBool("isCrouchRunning", false);
            animator.SetBool("isCrouchIdling", false);
        }

        void Update()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            if (isDying)
            {
                return;
            }
            //WORKS
            // if(isAttacking == false)
            // {
            //     HandleMovement();
            // }            
            
            HandleAttackAttack();
            HandleMovement();
            //Other input actions:
            if (Input.GetKeyDown(KeyCode.C))
            {

                if (isCrouching == false)
                {
                    TriggerCrouchIdleAnimation();
                    isCrouching = true;
                }
                else
                {
                    isCrouching = false;
                    // Reset the crouch idle parameters after a delay or at the end of the animation
                    ResetCrouchIdleParameters();
                }
            }
            else if (Input.GetKey(KeyCode.Alpha1))
            {
                TriggerTakeDamageAnimation();
            }
            else if (Input.GetKey(KeyCode.Alpha2))
            {
                TriggerSpecialAbility2Animation();
            }
            else if (Input.GetKey(KeyCode.Alpha3))
            {
                TriggerCastSpellAnimation();
            }
            else if (Input.GetKey(KeyCode.Alpha4))
            {
                TriggerKickAnimation();
            }
            else if (Input.GetKey(KeyCode.Alpha5))
            {
                TriggerPummelAnimation();
            }
            else if (Input.GetKey(KeyCode.Alpha6))
            {
                TriggerAttackSpinAnimation();
            }
            else if (Input.GetKey(KeyCode.Alpha7))
            {
                TriggerDie();
            }
            else if (Input.GetKey(KeyCode.LeftShift) && isCurrentlyRunning)
            {
                TriggerFlipAnimation();
            }
            else if (Input.GetKey(KeyCode.LeftControl) && isCurrentlyRunning)
            {
                TriggerRollAnimation();
            }
            else if (Input.GetKey(KeyCode.LeftAlt) && isCurrentlyRunning)
            {
                TriggerSlideAnimation();
            }

        }

        void UpdateDirection(string newDirection)
        {
            // Iterate through all possible direction names
            string[] directions = { "isWest", "isEast", "isSouth", "isSouthWest", "isNorthEast", "isSouthEast", "isNorth", "isNorthWest" };

            foreach (string direction in directions)
            {
                // Set all directions to false except the new direction
                animator.SetBool(direction, direction == newDirection);
            }

            if(currentDirection != newDirection)
            {
                isAttacking = false;
                ResetAttackAttackParameters();
            }
            // Update the current direction
            currentDirection = newDirection;
            // Reset the parameters to restart animations from new directions
        }

        public bool isRunning;
        public bool isRunningBackwards;
        public bool isStrafingLeft;
        public bool isStrafingRight;
        public bool isAttacking = false;


        void HandleMovement()
        {

            // Calculate direction based on mouse position
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition.z = Camera.main.transform.position.z - transform.position.z;
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            Vector3 directionToMouse = mouseWorldPosition - transform.position;
            directionToMouse.Normalize(); // Normalize the direction vector

            // Determine the closest cardinal or intercardinal direction
            float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;

            string newDirection = DetermineDirectionFromAngle(angle);

            if(newDirection != currentDirection)
            {
                UpdateDirection(newDirection);
            }
            string movementDirection = newDirection.Substring(2); // Remove "is" from the direction name

            // Capture movement input states
            isRunning = Input.GetKey(KeyCode.W);
            isRunningBackwards = Input.GetKey(KeyCode.S);
            isStrafingLeft = Input.GetKey(KeyCode.A);
            isStrafingRight = Input.GetKey(KeyCode.D);

            // Set general movement boolean
            isCurrentlyRunning = isRunning || isRunningBackwards || isStrafingLeft || isStrafingRight;
            
            // Reset all directional movement parameters
            ResetAllMovementBools();
            
            // Update animator with movement conditions
            animator.SetBool("isRunning", isRunning);
            animator.SetBool("isRunningBackwards", isRunningBackwards);
            animator.SetBool("isStrafingLeft", isStrafingLeft);
            animator.SetBool("isStrafingRight", isStrafingRight);
            if(isCrouching)
            {animator.SetBool("isCrouchRunning", isRunning);}

            // Set specific movement animations
            if (isCrouching)
            {
                SetMovementAnimation(isRunning, "CrouchRun", movementDirection);
            }
            else
            {
                SetMovementAnimation(isRunning, "Move", movementDirection);
                SetMovementAnimation(isRunningBackwards, "RunBackwards", movementDirection);
                SetMovementAnimation(isStrafingLeft, "StrafeLeft", movementDirection);
                SetMovementAnimation(isStrafingRight, "StrafeRight", movementDirection);
                SetMovementAnimation(isRunningBackwards, "Move", movementDirection);
                SetMovementAnimation(isStrafingLeft, "Move", movementDirection);
                SetMovementAnimation(isStrafingRight, "Move", movementDirection);
            }
        }

        void SetMovementAnimation(bool isActive, string baseKey, string direction)
        {
            if (isActive)
            {
                string animationKey = $"{baseKey}{direction}";
                animator.SetBool(animationKey, true);
            }
        }

        void ResetAllMovementBools()
        {
            string[] directions = new string[] { "North", "South", "East", "West", "NorthEast", "NorthWest", "SouthEast", "SouthWest" };
            foreach (string baseKey in new string[] { "Move", "RunBackwards", "StrafeLeft", "StrafeRight" })
            {
                foreach (string direction in directions)
                {
                    animator.SetBool($"{baseKey}{direction}", false);
                }
            }

            animator.SetBool("CrouchRunNorth", false);
            animator.SetBool("CrouchRunSouth", false);
            animator.SetBool("CrouchRunEast", false);
            animator.SetBool("CrouchRunWest", false);
            animator.SetBool("CrouchRunNorthEast", false);
            animator.SetBool("CrouchRunNorthWest", false);
            animator.SetBool("CrouchRunSouthEast", false);
            animator.SetBool("CrouchRunSouthWest", false);
        }


        string DetermineDirectionFromAngle(float angle)
        {
            // Normalize angle to [0..360)
            angle = (angle + 360) % 360;

            if (angle < 15f || angle >= 345f)
                return "isEast";        // corresponds to ~0°
            else if (angle >= 15f && angle < 75f)
                return "isNorthEast";   // corresponds to ~30°
            else if (angle >= 75f && angle < 105f)
                return "isNorth";       // corresponds to 90°
            else if (angle >= 105f && angle < 165f)
                return "isNorthWest";   // corresponds to 150°
            else if (angle >= 165f && angle < 195f)
                return "isWest";        // corresponds to 180°
            else if (angle >= 195f && angle < 255f)
                return "isSouthWest";   // corresponds to 210°
            else if (angle >= 255f && angle < 285f)
                return "isSouth";       // corresponds to 270°
            else if (angle >= 285f && angle < 345f)
                return "isSouthEast";   // corresponds to 330°

            // Fallback (should rarely reach here if the above covers 0..360)
            return "isEast";
        }



        void SetDirectionBools(bool isWest, bool isEast, bool isSouth, bool isSouthWest, bool isNorthEast, bool isSouthEast, bool isNorth, bool isNorthWest)
        {
            animator.SetBool("isWest", isWest);
            animator.SetBool("isEast", isEast);
            animator.SetBool("isSouth", isSouth);
            animator.SetBool("isSouthWest", isSouthWest);
            animator.SetBool("isNorthEast", isNorthEast);
            animator.SetBool("isSouthEast", isSouthEast);
            animator.SetBool("isNorth", isNorth);
            animator.SetBool("isNorthWest", isNorthWest);
        }


        //Default Attacks:

void HandleAttackAttack()
        {
            if (Input.GetMouseButton(1))
            {
                if (isCrouching)
                {
                    return;
                }
                isAttacking = true;

                // Move muzzle flash sprite to a high layer so it becomes visible
                muzzleFlashRenderer.sortingOrder = 150;

                // If you want to ensure a fresh muzzle animation each time we start firing,
                // you can also reset the animator state or just keep toggling booleans.

                // Figure out direction from mouse
                Vector3 mouseScreenPosition = Input.mousePosition;
                mouseScreenPosition.z = Camera.main.transform.position.z - transform.position.z;
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
                Vector3 directionToMouse = mouseWorldPosition - transform.position;
                directionToMouse.Normalize();

                float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
                if (angle < 0) angle += 360;
                string newDirection = DetermineDirectionFromAngle(angle);

                if (newDirection != currentDirection)
                {
                    ResetAllGunFireBools();
                    UpdateDirection(newDirection);
                }

                // Check if running
                bool isRunning = 
                    Input.GetKey(KeyCode.W) ||
                    Input.GetKey(KeyCode.S) ||
                    Input.GetKey(KeyCode.A) ||
                    Input.GetKey(KeyCode.D);

                // Run-attack vs stationary-attack in the main animator
                if (isRunning)
                {
                    animator.SetBool("isAttackRunning", false);
                    animator.SetBool("isAttackAttacking", false);

                    // Muzzle Animator logic
                    ResetAllGunFireBools();
                    string muzzleState = "Gunfire" + newDirection.Substring(2);
                    muzzleAnimator.SetBool(muzzleState, true);
                }
                else
                {
                    animator.SetBool("isAttackRunning", false);
                    animator.SetBool("isAttackAttacking", true);

                    // Muzzle Animator logic
                    ResetAllGunFireBools();
                    string muzzleState = "Gunfire" + newDirection.Substring(2);
                    muzzleAnimator.SetBool(muzzleState, true);
                }

                // Also do any AttackAttack parameters for main animator
                TriggerAttack(isRunning, newDirection.Substring(2));
            }
            else if (Input.GetMouseButtonUp(1))
            {
                // Mouse was released => Stop attacking
                isAttacking = false;

                // Reset main animator’s booleans
                ResetAttackAttackParameters();
                RestoreDirectionAfterAttack();

                // Move muzzle flash sprite to sorting order 0 so it’s effectively hidden
                muzzleFlashRenderer.sortingOrder = -1;

                // Reset muzzle animator booleans
                ResetAllGunFireBools();
            }
        }



        // This is your normal method, but we added the direction as a param
        // and simplified the random attack part. (You can adapt as you wish.)
        void TriggerAttack(bool isRunning, string direction)
        {
            if(isCurrentlyRunning)
            {return;}
            // Example: “AttackAttackEast” / “AttackAttackSouth” etc.
            string attackParam = "AttackAttack" + direction;
            animator.SetBool(attackParam, true);

            // isRunning? set “isAttackRunning”, else “isAttackAttacking”
            animator.SetBool("isAttackRunning",   isRunning);
            animator.SetBool("isAttackAttacking", !isRunning);
        }

        // -------------------------------------------------------------------
        // MUZZLE ANIMATOR METHODS
        // -------------------------------------------------------------------
        void ResetAllGunFireBools()
        {
            // Turn off all 8 directions in the muzzle animator
            // e.g. GunFireNorth, GunFireSouth, GunFireEast, ...
            string[] muzzleDirs = {
                "GunfireNorth","GunfireSouth","GunfireEast","GunfireWest",
                "GunfireNorthEast","GunfireNorthWest","GunfireSouthEast","GunfireSouthWest"
            };

            foreach (var dir in muzzleDirs)
            {
                muzzleAnimator.SetBool(dir, false);
            }
        }



        void ResetAttackAttackParameters()
        {
            string[] directions = {
                "North","South","East","West",
                "NorthEast","NorthWest","SouthEast","SouthWest"
            };
            foreach (string dir in directions)
            {
                animator.SetBool("AttackAttack" + dir, false);
                animator.SetBool("Attack2"      + dir, false);
                animator.SetBool("AttackRun"    + dir, false);
            }
        }

        void RestoreDirectionAfterAttack()
        {
            animator.SetBool("isAttackAttacking", false);
            animator.SetBool("isAttackRunning",   false);
            animator.SetBool("isRunning",         false);
        }

        //Take Damage:

        public void TriggerTakeDamageAnimation()
        {
            if (playerController != null && !playerController.isActive)
            {
                return;
            }
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            // // Set 'isTakeDamage' to true to initiate the take damage animation
            // animator.SetBool("isTakeDamage", true);

            // // Determine the current direction and trigger the appropriate take damage animation
            // if (animator.GetBool("isNorth")) animator.SetBool("TakeDamageNorth", true);
            // else if (animator.GetBool("isSouth")) animator.SetBool("TakeDamageSouth", true);
            // else if (animator.GetBool("isEast")) animator.SetBool("TakeDamageEast", true);
            // else if (animator.GetBool("isWest")) animator.SetBool("TakeDamageWest", true);
            // else if (animator.GetBool("isNorthEast")) animator.SetBool("TakeDamageNorthEast", true);
            // else if (animator.GetBool("isNorthWest")) animator.SetBool("TakeDamageNorthWest", true);
            // else if (animator.GetBool("isSouthEast")) animator.SetBool("TakeDamageSouthEast", true);
            // else if (animator.GetBool("isSouthWest")) animator.SetBool("TakeDamageSouthWest", true);

            // Spawn the appropriate effect at the character's position
            SpawnEffect();

            // // Optionally, reset the take damage parameters after a delay or at the end of the animation
            // StartCoroutine(ResetTakeDamageParameters());
        }

        private void SpawnEffect()
        {
            // Determine the list of prefabs to use based on the isRadiated flag
            List<GameObject> prefabsToUse = isRadiated ? radiatedPrefabs : bloodPrefabs;

            if (prefabsToUse == null || prefabsToUse.Count == 0)
            {
                Debug.LogWarning("No prefabs available in the selected list!");
                return;
            }

            // Pick a random prefab from the selected list
            GameObject selectedPrefab = prefabsToUse[Random.Range(0, prefabsToUse.Count)];

            if (selectedPrefab == null)
            {
                Debug.LogWarning("Selected prefab is null!");
                return;
            }

            // Instantiate the selected prefab at the character's position and orientation
            GameObject effectInstance = Instantiate(selectedPrefab, transform.position, Quaternion.identity);

            // Start a coroutine to modify the Order in Layer after 0.5 seconds
            StartCoroutine(UpdateSpriteOrder(effectInstance));
        }

        private IEnumerator UpdateSpriteOrder(GameObject effectInstance)
        {
            if (effectInstance == null)
            {
                yield break;
            }

            // Wait for 0.5 seconds
            yield return new WaitForSeconds(0.5f);

            // Get the SpriteRenderer component on the effect instance
            SpriteRenderer spriteRenderer = effectInstance.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                // Set the Order in Layer to 40
                spriteRenderer.sortingOrder = 40;
                // Debug.Log($"Updated sorting order to 40 for {effectInstance.name}");
            }
            else
            {
                Debug.LogWarning("SpriteRenderer not found on the effect instance!");
            }
        }



        IEnumerator ResetTakeDamageParameters()
        {
            // Wait for the length of the animation before resetting
            yield return new WaitForSeconds(0.5f); // Adjust the wait time based on your animation length

            // Reset all take damage parameters to false
            animator.SetBool("isTakeDamage", false);
            animator.SetBool("TakeDamageNorth", false);
            animator.SetBool("TakeDamageSouth", false);
            animator.SetBool("TakeDamageEast", false);
            animator.SetBool("TakeDamageWest", false);
            animator.SetBool("TakeDamageNorthEast", false);
            animator.SetBool("TakeDamageNorthWest", false);
            animator.SetBool("TakeDamageSouthEast", false);
            animator.SetBool("TakeDamageSouthWest", false);

            // Restore the direction to ensure the character returns to the correct idle state
            RestoreDirectionAfterAttack();
        }

        //Crouch:
        public void TriggerCrouchIdleAnimation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // Set 'isCrouchIdling' to true to initiate the crouch idle animation
            animator.SetBool("isCrouchIdling", true);

            // Determine the current direction and trigger the appropriate crouch idle animation
            if (animator.GetBool("isNorth")) animator.SetBool("CrouchIdleNorth", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("CrouchIdleSouth", true);
            else if (animator.GetBool("isEast")) animator.SetBool("CrouchIdleEast", true);
            else if (animator.GetBool("isWest")) animator.SetBool("CrouchIdleWest", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("CrouchIdleNorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("CrouchIdleNorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("CrouchIdleSouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("CrouchIdleSouthWest", true);

        }

        public void ResetCrouchIdleParameters()
        {
            // Reset all crouch idle parameters to false
            animator.SetBool("isCrouchIdling", false);
            animator.SetBool("CrouchIdleNorth", false);
            animator.SetBool("CrouchIdleSouth", false);
            animator.SetBool("CrouchIdleEast", false);
            animator.SetBool("CrouchIdleWest", false);
            animator.SetBool("CrouchIdleNorthEast", false);
            animator.SetBool("CrouchIdleNorthWest", false);
            animator.SetBool("CrouchIdleSouthEast", false);
            animator.SetBool("CrouchIdleSouthWest", false);

            // Optionally, restore the direction to ensure the character returns to the correct idle state
            RestoreDirectionAfterAttack();
        }



        //Die
        public void TriggerDie()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            isDying = true; 
            // Check the current direction and trigger the appropriate die animation
            if (currentDirection.Equals("isNorth")) TriggerDeathAnimation("dieNorth");
            else if (currentDirection.Equals("isSouth")) TriggerDeathAnimation("dieSouth");
            else if (currentDirection.Equals("isEast")) TriggerDeathAnimation("dieEast");
            else if (currentDirection.Equals("isWest")) TriggerDeathAnimation("dieWest");
            else if (currentDirection.Equals("isNorthEast")) TriggerDeathAnimation("dieNorthEast");
            else if (currentDirection.Equals("isNorthWest")) TriggerDeathAnimation("dieNorthWest");
            else if (currentDirection.Equals("isSouthEast")) TriggerDeathAnimation("dieSouthEast");
            else if (currentDirection.Equals("isSouthWest")) TriggerDeathAnimation("dieSouthWest");
        }

        private void TriggerDeathAnimation(string deathDirectionTrigger)
        {
            // Trigger the specific death direction
            animator.SetTrigger(deathDirectionTrigger);

            if(playerController.isDead == false)
            StartCoroutine(ResetDieParameters());
        }


        IEnumerator ResetDieParameters()
        {
            yield return new WaitForSeconds(2);
            animator.ResetTrigger("dieNorth");
            animator.ResetTrigger("dieSouth");
            animator.ResetTrigger("dieEast");
            animator.ResetTrigger("dieWest");
            animator.ResetTrigger("dieNorthEast");
            animator.ResetTrigger("dieNorthWest");
            animator.ResetTrigger("dieSouthEast");
            animator.ResetTrigger("dieSouthWest");

            animator.SetBool("isDie", false);

            // Force the Animator back to the default state
            animator.Play("IdleEast", 0);
            // Restore the direction to ensure the character returns to the correct idle state
            RestoreDirectionAfterAttack();
            isDying = false; 
        }

        // Special Ability 1:
        public void TriggerSpecialAbility1Animation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // Set 'isSpecialAbility1' to true to initiate the special ability animation
            animator.SetBool("isSpecialAbility1", true);

            // Determine the current direction and trigger the appropriate special ability animation
            if (animator.GetBool("isNorth")) animator.SetBool("SpecialAbility1North", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("SpecialAbility1South", true);
            else if (animator.GetBool("isEast")) animator.SetBool("SpecialAbility1East", true);
            else if (animator.GetBool("isWest")) animator.SetBool("SpecialAbility1West", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("SpecialAbility1NorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("SpecialAbility1NorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("SpecialAbility1SouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("SpecialAbility1SouthWest", true);

            // Reset the special ability parameters after a delay or at the end of the animation
            StartCoroutine(ResetSpecialAbility1Parameters());
        }

        IEnumerator ResetSpecialAbility1Parameters()
        {
            // Wait for the length of the animation before resetting
            yield return new WaitForSeconds(0.5f); // Adjust the wait time based on your animation length

            // Reset all special ability parameters to false
            animator.SetBool("isSpecialAbility1", false);
            animator.SetBool("SpecialAbility1North", false);
            animator.SetBool("SpecialAbility1South", false);
            animator.SetBool("SpecialAbility1East", false);
            animator.SetBool("SpecialAbility1West", false);
            animator.SetBool("SpecialAbility1NorthEast", false);
            animator.SetBool("SpecialAbility1NorthWest", false);
            animator.SetBool("SpecialAbility1SouthEast", false);
            animator.SetBool("SpecialAbility1SouthWest", false);

            // Optionally, restore the direction to ensure the character returns to the correct idle state
            RestoreDirectionAfterAttack();
        }

        // Special Ability 2:
        public void TriggerSpecialAbility2Animation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // Set 'isSpecialAbility1' to true to initiate the special ability animation
            animator.SetBool("isSpecialAbility2", true);

            // Determine the current direction and trigger the appropriate special ability animation
            if (animator.GetBool("isNorth")) animator.SetBool("SpecialAbility2North", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("SpecialAbility2South", true);
            else if (animator.GetBool("isEast")) animator.SetBool("SpecialAbility2East", true);
            else if (animator.GetBool("isWest")) animator.SetBool("SpecialAbility2West", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("SpecialAbility2NorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("SpecialAbility2NorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("SpecialAbility2SouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("SpecialAbility2SouthWest", true);

            // Reset the special ability parameters after a delay or at the end of the animation
            StartCoroutine(ResetSpecialAbility2Parameters());
        }

        IEnumerator ResetSpecialAbility2Parameters()
        {
            // Wait for the length of the animation before resetting
            yield return new WaitForSeconds(0.5f); // Adjust the wait time based on your animation length

            // Reset all special ability parameters to false
            animator.SetBool("isSpecialAbility2", false);
            animator.SetBool("SpecialAbility2North", false);
            animator.SetBool("SpecialAbility2South", false);
            animator.SetBool("SpecialAbility2East", false);
            animator.SetBool("SpecialAbility2West", false);
            animator.SetBool("SpecialAbility2NorthEast", false);
            animator.SetBool("SpecialAbility2NorthWest", false);
            animator.SetBool("SpecialAbility2SouthEast", false);
            animator.SetBool("SpecialAbility2SouthWest", false);

            // Optionally, restore the direction to ensure the character returns to the correct idle state
            RestoreDirectionAfterAttack();
        }


        //Cast spell
        public void TriggerCastSpellAnimation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // Set 'isCastingSpell' to true to initiate the spell casting animation
            animator.SetBool("isCastingSpell", true);

            // Determine the current direction and trigger the appropriate cast spell animation
            if (animator.GetBool("isNorth")) animator.SetBool("CastSpellNorth", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("CastSpellSouth", true);
            else if (animator.GetBool("isEast")) animator.SetBool("CastSpellEast", true);
            else if (animator.GetBool("isWest")) animator.SetBool("CastSpellWest", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("CastSpellNorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("CastSpellNorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("CastSpellSouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("CastSpellSouthWest", true);

            // Reset the cast spell parameters after a delay or at the end of the animation
            StartCoroutine(ResetCastSpellParameters());
        }

        IEnumerator ResetCastSpellParameters()
        {
            // Wait for the length of the animation before resetting
            yield return new WaitForSeconds(0.5f); // Adjust the wait time based on your animation length

            // Reset all cast spell parameters to false
            animator.SetBool("isCastingSpell", false);
            animator.SetBool("CastSpellNorth", false);
            animator.SetBool("CastSpellSouth", false);
            animator.SetBool("CastSpellEast", false);
            animator.SetBool("CastSpellWest", false);
            animator.SetBool("CastSpellNorthEast", false);
            animator.SetBool("CastSpellNorthWest", false);
            animator.SetBool("CastSpellSouthEast", false);
            animator.SetBool("CastSpellSouthWest", false);

            // Optionally, restore the direction to ensure the character returns to the correct idle state
            RestoreDirectionAfterAttack(); 
        }

        //Kick:
        public void TriggerKickAnimation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // Set 'isKicking' to true to initiate the kick animation
            animator.SetBool("isKicking", true);

            // Determine the current direction and trigger the appropriate kick animation
            if (animator.GetBool("isNorth")) animator.SetBool("KickNorth", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("KickSouth", true);
            else if (animator.GetBool("isEast")) animator.SetBool("KickEast", true);
            else if (animator.GetBool("isWest")) animator.SetBool("KickWest", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("KickNorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("KickNorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("KickSouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("KickSouthWest", true);

            // Reset the kick parameters after a delay or at the end of the animation
            StartCoroutine(ResetKickParameters());
        }

        IEnumerator ResetKickParameters()
        {
            // Wait for the length of the animation before resetting
            yield return new WaitForSeconds(0.5f); // Adjust the wait time based on your animation length

            // Reset all kick parameters to false
            animator.SetBool("isKicking", false);
            animator.SetBool("KickNorth", false);
            animator.SetBool("KickSouth", false);
            animator.SetBool("KickEast", false);
            animator.SetBool("KickWest", false);
            animator.SetBool("KickNorthEast", false);
            animator.SetBool("KickNorthWest", false);
            animator.SetBool("KickSouthEast", false);
            animator.SetBool("KickSouthWest", false);

            // Optionally, restore the direction to ensure the character returns to the correct idle state
            RestoreDirectionAfterAttack(); 
        }

        //Flip animation:
        public void TriggerFlipAnimation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // Set 'isFlipping' to true to initiate the flip animation
            animator.SetBool("isFlipping", true);

            // Determine the current direction and trigger the appropriate flip animation
            if (animator.GetBool("isNorth")) animator.SetBool("FrontFlipNorth", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("FrontFlipSouth", true);
            else if (animator.GetBool("isEast")) animator.SetBool("FrontFlipEast", true);
            else if (animator.GetBool("isWest")) animator.SetBool("FrontFlipWest", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("FrontFlipNorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("FrontFlipNorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("FrontFlipSouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("FrontFlipSouthWest", true);

            // Reset the flip parameters after a delay or at the end of the animation
            StartCoroutine(ResetFlipParameters());
        }

        IEnumerator ResetFlipParameters()
        {
            // Wait for the length of the animation before resetting
            yield return new WaitForSeconds(0.5f); // Adjust the wait time based on your animation length

            // Reset all flip parameters to false
            animator.SetBool("isFlipping", false);
            animator.SetBool("FrontFlipNorth", false);
            animator.SetBool("FrontFlipSouth", false);
            animator.SetBool("FrontFlipEast", false);
            animator.SetBool("FrontFlipWest", false);
            animator.SetBool("FrontFlipNorthEast", false);
            animator.SetBool("FrontFlipNorthWest", false);
            animator.SetBool("FrontFlipSouthEast", false);
            animator.SetBool("FrontFlipSouthWest", false);

            // Optionally, restore the direction to ensure the character returns to the correct idle state
            RestoreDirectionAfterAttack();  
        }


        //rolling

        public void TriggerRollAnimation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // Set 'isRolling' to true to initiate the roll animation
            animator.SetBool("isRolling", true);

            // Determine the current direction and trigger the appropriate roll animation
            if (animator.GetBool("isNorth")) animator.SetBool("RollingNorth", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("RollingSouth", true);
            else if (animator.GetBool("isEast")) animator.SetBool("RollingEast", true);
            else if (animator.GetBool("isWest")) animator.SetBool("RollingWest", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("RollingNorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("RollingNorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("RollingSouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("RollingSouthWest", true);

            // Reset the roll parameters after a delay or at the end of the animation
            StartCoroutine(ResetRollParameters());
        }

        IEnumerator ResetRollParameters()
        {
            // Wait for the length of the animation before resetting
            yield return new WaitForSeconds(rollTime); // Adjust the wait time based on your animation length

            // Reset all roll parameters to false
            animator.SetBool("isRolling", false);
            animator.SetBool("RollingNorth", false);
            animator.SetBool("RollingSouth", false);
            animator.SetBool("RollingEast", false);
            animator.SetBool("RollingWest", false);
            animator.SetBool("RollingNorthEast", false);
            animator.SetBool("RollingNorthWest", false);
            animator.SetBool("RollingSouthEast", false);
            animator.SetBool("RollingSouthWest", false);

            // Optionally, restore the direction to ensure the character returns to the correct idle state
            RestoreDirectionAfterAttack();  
        }

        //Slide
        public void TriggerSlideAnimation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // Set 'isSliding' to true to initiate the slide animation
            animator.SetBool("isSliding", true);

            // Determine the current direction and trigger the appropriate slide animation
            if (animator.GetBool("isNorth")) animator.SetBool("SlidingNorth", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("SlidingSouth", true);
            else if (animator.GetBool("isEast")) animator.SetBool("SlidingEast", true);
            else if (animator.GetBool("isWest")) animator.SetBool("SlidingWest", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("SlidingNorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("SlidingNorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("SlidingSouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("SlidingSouthWest", true);

            // Reset the slide parameters after a delay or at the end of the animation
            StartCoroutine(ResetSlideParameters());
        }

        IEnumerator ResetSlideParameters()
        {
            // Wait for the length of the animation before resetting
            yield return new WaitForSeconds(0.7f); // Adjust the wait time based on your animation length

            // Reset all slide parameters to false
            animator.SetBool("isSliding", false);
            animator.SetBool("SlidingNorth", false);
            animator.SetBool("SlidingSouth", false);
            animator.SetBool("SlidingEast", false);
            animator.SetBool("SlidingWest", false);
            animator.SetBool("SlidingNorthEast", false);
            animator.SetBool("SlidingNorthWest", false);
            animator.SetBool("SlidingSouthEast", false);
            animator.SetBool("SlidingSouthWest", false);

            // Optionally, restore the direction to ensure the character returns to the correct idle state
            RestoreDirectionAfterAttack(); 
        }

        //Pummel
        public void TriggerPummelAnimation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // Set 'isPummeling' to true to initiate the pummel animation
            animator.SetBool("isPummeling", true);

            // Determine the current direction and trigger the appropriate pummel animation
            if (animator.GetBool("isNorth")) animator.SetBool("PummelNorth", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("PummelSouth", true);
            else if (animator.GetBool("isEast")) animator.SetBool("PummelEast", true);
            else if (animator.GetBool("isWest")) animator.SetBool("PummelWest", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("PummelNorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("PummelNorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("PummelSouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("PummelSouthWest", true);

            // Reset the pummel parameters after a delay or at the end of the animation
            StartCoroutine(ResetPummelParameters());
        }

        IEnumerator ResetPummelParameters()
        {
            // Wait for the length of the animation before resetting
            yield return new WaitForSeconds(0.5f); // Adjust the wait time based on your animation length

            // Reset all pummel parameters to false
            animator.SetBool("isPummeling", false);
            animator.SetBool("PummelNorth", false);
            animator.SetBool("PummelSouth", false);
            animator.SetBool("PummelEast", false);
            animator.SetBool("PummelWest", false);
            animator.SetBool("PummelNorthEast", false);
            animator.SetBool("PummelNorthWest", false);
            animator.SetBool("PummelSouthEast", false);
            animator.SetBool("PummelSouthWest", false);

            // Optionally, restore the direction to ensure the character returns to the correct idle state
            RestoreDirectionAfterAttack();  
        }

        //Attack spin
        public void TriggerAttackSpinAnimation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // Set 'isAttackSpinning' to true to initiate the attack spin animation
            animator.SetBool("isAttackSpinning", true);

            // Determine the current direction and trigger the appropriate attack spin animation
            if (animator.GetBool("isNorth")) animator.SetBool("AttackSpinNorth", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("AttackSpinSouth", true);
            else if (animator.GetBool("isEast")) animator.SetBool("AttackSpinEast", true);
            else if (animator.GetBool("isWest")) animator.SetBool("AttackSpinWest", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("AttackSpinNorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("AttackSpinNorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("AttackSpinSouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("AttackSpinSouthWest", true);

            // Reset the attack spin parameters after a delay or at the end of the animation
            StartCoroutine(ResetAttackSpinParameters());
        }

        IEnumerator ResetAttackSpinParameters()
        {
            // Wait for the length of the animation before resetting
            yield return new WaitForSeconds(0.5f); // Adjust the wait time based on your animation length

            // Reset all attack spin parameters to false
            animator.SetBool("isAttackSpinning", false);
            animator.SetBool("AttackSpinNorth", false);
            animator.SetBool("AttackSpinSouth", false);
            animator.SetBool("AttackSpinEast", false);
            animator.SetBool("AttackSpinWest", false);
            animator.SetBool("AttackSpinNorthEast", false);
            animator.SetBool("AttackSpinNorthWest", false);
            animator.SetBool("AttackSpinSouthEast", false);
            animator.SetBool("AttackSpinSouthWest", false);

            // Optionally, restore the direction to ensure the character returns to the correct idle state
            RestoreDirectionAfterAttack(); 
        }


    }
}