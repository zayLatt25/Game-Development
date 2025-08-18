using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SmallScaleInc.TopDownPixelCharactersPack1
{
    public class ZombieAI : MonoBehaviour
    {
        [Header("References")]
        public Transform player;        // The player to chase
        private CapsuleCollider2D capsuleCollider;
        public PlayerController playerController;
        public SpriteRenderer spriteRenderer;
        private Color originalColor;

        [Header("Settings")]
        public float detectionRadius = 5f;  // If the player is this close, zombie chases
        public float moveSpeed = 2f;        // How fast the zombie moves
        public bool isRunner;

        [Header("Alert Settings")]
        public float alertRange = 5f;              // Radius within which other zombies will be alerted

        public float alertedDetectionRadius = 15f; // New detection radius after taking damage
        private float baseDetectionRadius;
        public float alertDuration = 15f; // Time (in seconds) the zombie stays alert

        // --- New: List of animator controllers for random appearance ---
        [Header("Appearance Settings")]
        public RuntimeAnimatorController[] zombieAnimatorControllers;
        private Animator animator;

        [Header("Health")]
        public int maxHealth = 100;
        private int currentHealth;

        public bool isChasing = false;
        private bool isDead = false;

        [Header("Attack Settings")]
        public float attackRange = 0.7f;     // If within this distance, can attack
        public float attackDelay = 1f;       // Minimum time between consecutive attacks
        public float windUpTime = 0.3f;      // Time before damage is actually dealt in the attack
        public float totalAttackTime = 1f;   // Zombie remains “locked” in attack for 1s
        public int zombieDamage = 1;         // Damage per attack
        public bool stopMovementWhileAttacking = true;

        // Attack timing
        public bool isAttacking = false;   // Are we in the middle of an attack animation?
        private float nextAttackTime = 0f;  // Next time we can start another attack

        // Lists for prefabs
        [SerializeField] private List<GameObject> bloodPrefabs = new List<GameObject>();
        [SerializeField] private List<GameObject> radiatedPrefabs = new List<GameObject>();

        public bool isRadiated = false; // Determines whether to use radiated effects

        // --- Animation / Direction Variables ---
        private Vector3 previousPosition;
        private string currentDirection = "isSouth"; // default direction?
        public float directionUpdateInterval = 0.2f; // update direction 5x per second
        private float nextDirectionUpdateTime = 0f;

        void Start()
        {
            animator = GetComponent<Animator>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();
            playerController = FindObjectOfType<PlayerController>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            previousPosition = transform.position;
            currentHealth = maxHealth;
            baseDetectionRadius = detectionRadius;  // Store the original detection radius
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
            if (isRunner)
            {
                moveSpeed = 1f;
            }
            // --- Randomize Appearance ---
            if (zombieAnimatorControllers != null && zombieAnimatorControllers.Length > 0)
            {
                int randomIndex = Random.Range(0, zombieAnimatorControllers.Length);
                animator.runtimeAnimatorController = zombieAnimatorControllers[randomIndex];
            }
        }


    void Update()
    {
        if (isDead) return;
        if (player == null || playerController == null) return; // safety check

        // 1) Distance check => chase or idle
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if(playerController.isDead == true)
        {
            isChasing = false; 
        }
        else
        {
            isChasing = (distanceToPlayer <= detectionRadius);
        }

        // 2) Move (unless we’re currently locked in an attack and are configured to stop)
        if (isChasing && (!stopMovementWhileAttacking || !isAttacking))
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
        }

        if (isChasing == false)
        {
            ResetAllMovementBools();
            animator.SetBool(currentDirection, true); //Sets the default direction to east.
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            animator.SetBool("isCrouchRunning", false);
            animator.SetBool("isCrouchIdling", false);
            return;
        }

        // 3) Animate movement
        HandleMovementAnimation();

        // 4) If close enough, try to attack
        if (distanceToPlayer <= attackRange)
        {
            TryAttackPlayer();
        }
    }


        // ---------------------------------------------------------------------
            // Attempt to start an attack if enough time has passed and we’re not already attacking
            // ---------------------------------------------------------------------
            private void TryAttackPlayer()
            {
                if (!isAttacking && Time.time >= nextAttackTime)
                {
                    nextAttackTime = Time.time + attackDelay;
                    StartCoroutine(AttackRoutine());
                }
            }

            // ---------------------------------------------------------------------
            // Attack routine: freeze movement (if configured) for totalAttackTime,
            // deal damage after windUpTime.
            // ---------------------------------------------------------------------
            private System.Collections.IEnumerator AttackRoutine()
            {
                isAttacking = true;
                ResetAllMovementBools();

                    animator.SetBool("isRunning", false);
 
                    animator.SetBool("isWalking", false);
                
                HandleAttackAttackAnimation();
                // Wait for windUpTime (zombie stands still if stopMovementWhileAttacking = true)
                yield return new WaitForSeconds(windUpTime);

                // After 0.3s, we apply damage (assuming the player is still around).
                if (!isDead && playerController != null && !playerController.isDead) 
                {
                    // Debug.Log("Zombie deals damage to player!");
                    playerController.TakeDamage(zombieDamage);
                }

                // Then wait the remainder of the totalAttackTime 
                // (1s total minus the 0.3s windUp = 0.7s more)
                float remainder = totalAttackTime - windUpTime;
                if (remainder > 0f)
                {
                    yield return new WaitForSeconds(remainder);
                }

                // Done attacking, can move again
                isAttacking = false;
                ResetAttackAttackParameters();
                if(isRunner)
                {
                    animator.SetBool("isRunning", true);
                }
                else
                {
                    animator.SetBool("isWalking", true);
                }
            }

        /// <summary>
        /// Reduces the zombies's health and checks for death.
        /// </summary>
        /// <param name="damageAmount">Amount of damage to apply.</param>


        public void TakeDamage(int damageAmount)
        {
            if (isDead) return; // If already dead, ignore further damage

            currentHealth -= damageAmount;
            // Debug.Log($"Zombie took {damageAmount} damage. Current Health: {currentHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // Increase this zombie's detection radius so it starts chasing immediately
                detectionRadius = alertedDetectionRadius;
                
                // Alert nearby zombies
                AlertNearbyZombies();
                
                // Restart/reset alert timer if you are using a temporary alert
                StopCoroutine("ResetDetectionRadius");
                StartCoroutine("ResetDetectionRadius");

                // Trigger visual feedback
                StartCoroutine(FlashRed());
                TriggerTakeDamageAnimation();
            }
        }


        public void Alert()
        {
            if (isDead) return; // Do nothing if the zombie is dead

            // Set the detection radius to the alerted value and start chasing the player.
            detectionRadius = alertedDetectionRadius;
            isChasing = true;
            
            // Optionally, restart the alert timer so that after alertDuration, the zombie can revert.
            StopCoroutine("ResetDetectionRadius");
            StartCoroutine("ResetDetectionRadius");
            
            // (Optional) You can also trigger an animation or sound effect here to indicate the alert.
            // Debug.Log($"{gameObject.name} is alerted!");
        }

        private void AlertNearbyZombies()
        {
            // Find all colliders within the alertRange around this zombie
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, alertRange);
            foreach (Collider2D col in colliders)
            {
                // Skip self
                if (col.gameObject == this.gameObject) 
                    continue;
                
                // Check if the collider belongs to a zombie
                ZombieAI otherZombie = col.GetComponent<ZombieAI>();
                if (otherZombie != null)
                {
                    // Optionally, you might want to check if the other zombie isn't already chasing
                    if (!otherZombie.isChasing)
                    {
                        otherZombie.Alert();
                    }
                }
            }
        }

        private IEnumerator ResetDetectionRadius()
        {
            yield return new WaitForSeconds(alertDuration);
            detectionRadius = baseDetectionRadius;
        }

        private IEnumerator FlashRed()
        {
            // If for some reason spriteRenderer is null, just skip
            if (spriteRenderer == null) yield break;

            // Change to red
            spriteRenderer.color = Color.red;

            // Wait a tiny fraction of a second (tweak as desired)
            yield return new WaitForSeconds(0.3f);

            // Restore original color
            spriteRenderer.color = originalColor;
        }


        /// <summary>
        /// Called when the zombie's health <= 0. Stops movement, can play a death animation, etc.
        /// </summary>
        private void Die()
        {
            // Debug.Log("Zombie died!");
            isDead = true;
            spriteRenderer.sortingOrder = 2;
            if (capsuleCollider != null)
            {
                capsuleCollider.enabled = false;
            }
            TriggerDie();

            // Inform the player controller that a zombie was killed
            if (playerController != null)
            {
                playerController.IncrementZombieKillCount();
            }
        }



private void HandleMovementAnimation()
{
    // Current velocity since last frame
    Vector3 currentPos = transform.position;
    Vector2 velocity = (currentPos - previousPosition) / Time.deltaTime;
    previousPosition = currentPos;

    float speed = velocity.magnitude;
    bool isMoving = (speed > 0.01f);

    // Always reset all directional movement bools
    ResetAllMovementBools();

    if (isMoving)
    {
        // Only recalc angle & set direction if time >= nextDirectionUpdateTime
        if (Time.time >= nextDirectionUpdateTime)
        {
            nextDirectionUpdateTime = Time.time + directionUpdateInterval;

            // 1) Determine the 8-direction angle
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            // 2) Get the "isNorth", "isSouthWest", etc.
            string newDirection = DetermineDirectionFromAngle(angle);

            // 3) If direction changed, update direction booleans
            if (newDirection != currentDirection)
            {
                UpdateDirection(newDirection);
                currentDirection = newDirection;
            }
        }

        // We still set Move + direction, but using the last known currentDirection 
        // (not necessarily newly computed if we haven’t hit the interval yet).
        string movementDirection = currentDirection.Substring(2);
        SetMovementAnimation(true, "Move", movementDirection);
    }
    else
    {
        // If not moving, we do idle => no movement bool set => animator goes idle
    }
}

        // ---------------------------------------------------------------------
        // EXACT same approach as your player code’s "UpdateDirection" function.
        // ---------------------------------------------------------------------
        private void UpdateDirection(string newDirection)
        {
            // Turn off all 8 directions
            string[] directions = {
                "isWest","isEast","isSouth","isSouthWest","isNorthEast",
                "isSouthEast","isNorth","isNorthWest"
            };
            foreach (string d in directions)
            {
                animator.SetBool(d, false);
            }

            // Turn on the new direction
            animator.SetBool(newDirection, true);
            if(isRunner)
            {
                animator.SetBool("isRunning", true);
            }
            else
            {
                animator.SetBool("isWalking", true);
            }
        }

        // ---------------------------------------------------------------------
        // EXACT same approach as your "SetMovementAnimation" in the player code
        // ---------------------------------------------------------------------
        private void SetMovementAnimation(bool isActive, string baseKey, string direction)
        {
            if (isActive)
            {
                string animationKey = $"{baseKey}{direction}";
                animator.SetBool(animationKey, true);
            }
        }

        // ---------------------------------------------------------------------
        // EXACT same approach as your "ResetAllMovementBools"
        // ---------------------------------------------------------------------
        private void ResetAllMovementBools()
        {
            string[] directions = { "North", "South", "East", "West", "NorthEast", "NorthWest", "SouthEast", "SouthWest" };
            foreach (string baseKey in new string[] { "Move", "RunBackwards", "StrafeLeft", "StrafeRight" })
            {
                foreach (string direction in directions)
                {
                    animator.SetBool($"{baseKey}{direction}", false);
                }
            }

            // If you also have "CrouchRunNorth", etc. in the animator, reset them as well
            animator.SetBool("CrouchRunNorth", false);
            animator.SetBool("CrouchRunSouth", false);
            animator.SetBool("CrouchRunEast", false);
            animator.SetBool("CrouchRunWest", false);
            animator.SetBool("CrouchRunNorthEast", false);
            animator.SetBool("CrouchRunNorthWest", false);
            animator.SetBool("CrouchRunSouthEast", false);
            animator.SetBool("CrouchRunSouthWest", false);
        }

        // ---------------------------------------------------------------------
        // EXACT same as your player's "DetermineDirectionFromAngle"
        // ---------------------------------------------------------------------
        private string DetermineDirectionFromAngle(float angle)
        {
            if ((angle >= 330 || angle < 15))
                return "isEast";
            else if ((angle >= 15 && angle < 60))
                return "isNorthEast";
            else if ((angle >= 60 && angle < 120))
                return "isNorth";
            else if ((angle >= 120 && angle < 165))
                return "isNorthWest";
            else if ((angle >= 165 && angle < 195))
                return "isWest";
            else if ((angle >= 195 && angle < 240))
                return "isSouthWest";
            else if ((angle >= 240 && angle < 300))
                return "isSouth";
            else if ((angle >= 300 && angle < 345))
                return "isSouthEast";

            return "isEast"; // fallback
        }


    //Dying:
    public void TriggerDie()
            {
                if (!gameObject.activeInHierarchy)
                {
                    return;
                }
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
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
                animator.SetTrigger(deathDirectionTrigger);
            
            }

    //Take damage:

            public void TriggerTakeDamageAnimation()
            {
                // Set 'isTakeDamage' to true to initiate the take damage animation
                animator.SetBool("isTakeDamage", true);

                // Determine the current direction and trigger the appropriate take damage animation
                if (animator.GetBool("isNorth")) animator.SetBool("TakeDamageNorth", true);
                else if (animator.GetBool("isSouth")) animator.SetBool("TakeDamageSouth", true);
                else if (animator.GetBool("isEast")) animator.SetBool("TakeDamageEast", true);
                else if (animator.GetBool("isWest")) animator.SetBool("TakeDamageWest", true);
                else if (animator.GetBool("isNorthEast")) animator.SetBool("TakeDamageNorthEast", true);
                else if (animator.GetBool("isNorthWest")) animator.SetBool("TakeDamageNorthWest", true);
                else if (animator.GetBool("isSouthEast")) animator.SetBool("TakeDamageSouthEast", true);
                else if (animator.GetBool("isSouthWest")) animator.SetBool("TakeDamageSouthWest", true);

                // Spawn the appropriate effect at the character's position
                SpawnEffect();

                // Optionally, reset the take damage parameters after a delay or at the end of the animation
                StartCoroutine(ResetTakeDamageParameters());
            }

            private void SpawnEffect()
            {
                // Determine the list of prefabs to use based on the isRadiated flag
                List<GameObject> prefabsToUse = isRadiated ? radiatedPrefabs : bloodPrefabs;

                if (prefabsToUse == null || prefabsToUse.Count == 0)
                {
                    // Debug.LogWarning("No prefabs available in the selected list!");
                    return;
                }

                // Pick a random prefab from the selected list
                GameObject selectedPrefab = prefabsToUse[Random.Range(0, prefabsToUse.Count)];

                if (selectedPrefab == null)
                {
                    // Debug.LogWarning("Selected prefab is null!");
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
                    spriteRenderer.sortingOrder = 2;
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
                // RestoreDirectionAfterAttack();
            }


        void HandleAttackAttackAnimation()
        {
            // Also do any AttackAttack parameters for main animator
            TriggerAttackAnimation(currentDirection.Substring(2));

        }



        // This is your normal method, but we added the direction as a param
        // and simplified the random attack part. (You can adapt as you wish.)
        void TriggerAttackAnimation(string direction)
        {
            // Example: “AttackAttackEast” / “AttackAttackSouth” etc.
            string attackParam = "AttackAttack" + direction;
            animator.SetBool(attackParam, true);

            // isRunning? set “isAttackRunning”, else “isAttackAttacking”
            animator.SetBool("isAttackAttacking", true);
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
            }
            animator.SetBool("isAttackAttacking", false);
        }
    }

    
}