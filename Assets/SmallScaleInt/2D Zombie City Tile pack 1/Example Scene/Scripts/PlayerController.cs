using UnityEngine;
using UnityEngine.UI;             // For UI elements
using UnityEngine.SceneManagement; // For reloading the scene
using System.Collections;
using TMPro;
using System.Collections.Generic;

namespace SmallScaleInc.TopDownPixelCharactersPack1
{
    public class PlayerController : MonoBehaviour
    {
        public AnimationController animationController;
        private CircleCollider2D circleCollider;
        public float speed = 1.0f; // the movement speed of the player
        private Rigidbody2D rb;
        private Vector2 movementDirection;
        private bool isOnStairs = false; // when on stairs, the player moves in a different angle.
        public bool isCrouching = false; // when crouching, the player moves slower
        private SpriteRenderer spriteRenderer;
        private float lastAngle;  // Store the last calculated angle
        private bool isRunning = false;
        private Color originalColor;

        // Add this field at the top where other variables are declared
        private AudioSource gunfireAudioSource;



        // Archer specifics
        public bool isActive; // If the character is active
        public bool isRanged; // If the character is an archer OR caster character
        public bool isStealth; // If true, Makes the player transparent when crouched
        public bool isShapeShifter;
        public bool isSummoner;
        public GameObject projectilePrefab;
        public GameObject AoEPrefab;
        public GameObject Special1Prefab;
        public GameObject HookPrefab;
        public GameObject ShapeShiftPrefab;
        public float projectileSpeed = 10.0f;
        public float shootDelay = 0.5f;

        // Melee specifics
        public bool isMelee;
        public GameObject meleePrefab;

        // Damage and fire rate settings
        [Header("Damage Settings")]
        public float bulletDamage = 1f;
        public float bulletsPerSecond = 3f;
        private float nextFireTime = 0f;

        [Header("Line Renderer / Bullet Trace")]
        public GameObject bulletLinePrefab;
        public float lineDisplayTime = 0.05f;

        [Header("Shot Origin Offsets")]
        public float muzzleForwardOffset = 0.5f;
        public float muzzleUpOffset = 0.2f;

        // -------------------- Health & UI --------------------
        public int maxHealth = 100;
        public int currentHealth;
        public bool isDead = false;
        public Slider healthSlider;     // Assign your UI Slider in the Inspector
        public GameObject gameOver;     // Assign your GAMEOVER UI GameObject in the Inspector

        // --- Score / Kill Count ---
        public int zombieKillCount = 0;
        public TextMeshProUGUI killCountText; // Assign this in the Inspector with your score UI element


        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animationController = GetComponent<AnimationController>();
            circleCollider = GetComponent<CircleCollider2D>();
            originalColor = spriteRenderer.color;
            // Initialize health
            currentHealth = maxHealth;
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }
            // Setup AudioSource for gunfire
            gunfireAudioSource = GetComponent<AudioSource>();
            if (killCountText != null)
            {
                originalScale = killCountText.transform.localScale; // Save the original size
            }
        }

        void Update()
        {
            // Existing movement and input code...
            if(isDead)
            {
                return;
            }
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 directionToMouse = (mousePosition - (Vector2)transform.position).normalized;

            float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
            lastAngle = SnapAngleToEightDirections(angle);

            movementDirection = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad));

            HandleMovement();
            HandleZombieDamage();
            HandleShooting(); // for playing sound

            bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                             Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

            if (isMoving && !isRunning)
            {
                isRunning = true;
            }
            else if (!isMoving && isRunning)
            {
                isRunning = false;
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                if (isShapeShifter && isActive)
                {
                    StartCoroutine(ShapeShiftDelayed());
                }
                HandleCrouching();
            }

            if (isActive)
            {
                // Check for missing prefabs (projectile, AoE, etc.)
                if (projectilePrefab == null || AoEPrefab == null ||
                    Special1Prefab == null || HookPrefab == null)
                {
                    return;
                }

                if (isRanged)
                {
                    if (Input.GetMouseButtonDown(1))
                    {
                        Invoke(nameof(DelayedShoot), shootDelay);
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        StartCoroutine(DeploySpecial1Delayed());
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha3))
                    {
                        StartCoroutine(DeployAoEDelayed());
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha5))
                    {
                        if (isSummoner)
                        {
                            StartCoroutine(DeployHookDelayed());
                        }
                        else
                        {
                            StartCoroutine(Quickshot());
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha6))
                    {
                        StartCoroutine(CircleShot());
                    }
                }

                if (isMelee)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        StartCoroutine(DeployAoEDelayed());
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha5))
                    {
                        StartCoroutine(DeployHookDelayed());
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha6))
                    {
                        Invoke(nameof(DelayedShoot), shootDelay);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.LeftControl) && isRunning)
                {
                    if (isShapeShifter && isActive)
                    {
                        StartCoroutine(ShapeShiftDelayed());
                    }
                }
            }
        }

        void FixedUpdate()
        {
            if (movementDirection != Vector2.zero)
            {
                rb.MovePosition(rb.position + movementDirection * speed * Time.fixedDeltaTime);
            }
        }

        private void HandleShooting()
        {
            if (Input.GetMouseButtonDown(1)) // Right mouse button pressed
            {
                PlayGunfireSound(); // Play gunfire sound allowing overlapping
            }
        }

        private void PlayGunfireSound()
        {
            // // Create a new GameObject for the sound and add an AudioSource
            // GameObject gunfireSoundObject = new GameObject("GunfireSound");
            // AudioSource newGunfireSource = gunfireSoundObject.AddComponent<AudioSource>();

            // // Set the AudioSource properties
            // newGunfireSource.clip = gunfireAudioSource.clip; // Use the existing gunfire sound clip
            // newGunfireSource.outputAudioMixerGroup = gunfireAudioSource.outputAudioMixerGroup; // Maintain mixer settings
            // newGunfireSource.volume = gunfireAudioSource.volume;
            // newGunfireSource.spatialBlend = 0; // Set to 0 for 2D sound (if using 3D sounds, adjust accordingly)

            // // Play the sound
            // newGunfireSource.Play();

            // // Destroy the sound object after the clip finishes playing
            // Destroy(gunfireSoundObject, newGunfireSource.clip.length);
        }

                /// <summary>
        /// Allows the gunfire sound to finish naturally if a single shot was fired.
        /// If the button is held, it loops.
        /// </summary>
        private IEnumerator StopGunfireSoundAfterDelay()
        {
            yield return new WaitForSeconds(0.25f); // Small delay to allow natural fade-out
            if (!Input.GetMouseButton(1)) // Check if the button is still being held
            {
                gunfireAudioSource.Stop();
            }
        }

        /// <summary>
        /// Reduces the player's health, updates the UI slider, and checks for death.
        /// </summary>
        public void TakeDamage(int damageAmount)
        {
            if (isDead) return; // Ignore damage if already dead

            currentHealth -= damageAmount;
            // Debug.Log($"Player took {damageAmount} damage. Current Health: {currentHealth}");

            if (healthSlider != null)
            {
                healthSlider.value = currentHealth;
            }

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                animationController.TriggerTakeDamageAnimation();
            }
        }

        /// <summary>
        /// Called when the player's health falls to 0. Plays the death animation,
        /// disables movement, shows the GAMEOVER screen, and restarts the scene after a delay.
        /// </summary>
        private void Die()
        {
            // Debug.Log("Player died!");
            isDead = true;

            if (circleCollider != null)
            {
                circleCollider.enabled = false;
            }

            animationController.TriggerDie();

            // Prevent the player body from being pushed
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Static; // Makes the body completely immovable
            }

            // Show the GAMEOVER UI for 3 seconds
            if (gameOver != null)
            {
                gameOver.SetActive(true);
            }
            StartCoroutine(RestartSceneAfterDelay(3f));
        }


        /// <summary>
        /// Waits for the specified delay and then restarts the current scene.
        /// </summary>
        /// <param name="delay">Delay in seconds before restarting.</param>
        private IEnumerator RestartSceneAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }


     // ---------------------------------------------------------------
        // NEW: Damaging the zombie under the mouse if right mouse held
        // ---------------------------------------------------------------
        private Coroutine pulseCoroutine; 
        private Vector3 originalScale; // Stores the original size at game start


public void IncrementZombieKillCount()
{
    zombieKillCount++;
    if (killCountText != null)
    {
        killCountText.text = zombieKillCount.ToString();

        // Stop any ongoing pulse effect before starting a new one
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }

        // Start a new pulse effect
        pulseCoroutine = StartCoroutine(PulseTextEffect(killCountText));
    }
}

private IEnumerator PulseTextEffect(TextMeshProUGUI text)
{
    float duration = 0.2f; // Total pulse duration
    float maxScaleFactor = 1.5f; // How much larger it grows
    float time = 0f;

    Vector3 maxScale = originalScale * maxScaleFactor; // Calculate target size

    // Enlarge the text
    while (time < duration / 2)
    {
        text.transform.localScale = Vector3.Lerp(text.transform.localScale, maxScale, time / (duration / 2));
        time += Time.deltaTime;
        yield return null;
    }
    text.transform.localScale = maxScale;
    time = 0f;

    // Shrink back to original size
    while (time < duration / 2)
    {
        text.transform.localScale = Vector3.Lerp(text.transform.localScale, originalScale, time / (duration / 2));
        time += Time.deltaTime;
        yield return null;
    }

    text.transform.localScale = originalScale; // Ensure final reset
    pulseCoroutine = null;
}
private void HandleZombieDamage()
{
    if (Input.GetMouseButton(1))
    {
        float timeBetweenShots = 1f / bulletsPerSecond; // Fire rate control
        if (Time.time >= nextFireTime)
        {
            speed = 0.5f;
            nextFireTime = Time.time + timeBetweenShots;

            Vector2 playerPos = transform.position;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePos - playerPos).normalized;
            Vector2 muzzleOrigin = playerPos;
            float maxDistance = 10f; 

            // Continue raycasting as long as we are hitting zombies
            Vector2 rayOrigin = muzzleOrigin;
            bool shouldContinue = true;
            List<Vector2> hitPoints = new List<Vector2> { muzzleOrigin }; // Store hit points for the tracer

            while (shouldContinue)
            {
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, maxDistance);

                if (hit.collider != null)
                {
                    hitPoints.Add(hit.point);

                    // If it's a zombie, apply damage
                    ZombieAI zombie = hit.collider.GetComponent<ZombieAI>();
                    if (zombie != null)
                    {
                        zombie.TakeDamage((int)bulletDamage);
                        // Debug.Log("Hit zombie! Dealt " + bulletDamage + " damage.");

                        // 50% chance to pass through
                        if (Random.value > 0.5f)
                        {
                            // Debug.Log("Bullet passed through the zombie!");
                            rayOrigin = hit.point + direction * 0.1f; // Move slightly forward to avoid hitting the same zombie
                        }
                        else
                        {
                            // Debug.Log("Bullet stopped!");
                            shouldContinue = false;
                        }
                    }
                    else
                    {
                        shouldContinue = false; // Stop if we hit something else
                    }
                }
                else
                {
                    hitPoints.Add(rayOrigin + direction * maxDistance);
                    shouldContinue = false; // Stop if we hit nothing
                }
            }

            // Show tracer line for the full bullet path
            StartCoroutine(ShowShotLine(hitPoints));
        }
    }
    else
    {
        speed = 1.0f;
        nextFireTime = 0f;
    }
}

// -------------------------------------------------------------
// COROUTINE: Instantiates a line prefab and shows the bullet path
// -------------------------------------------------------------
private IEnumerator ShowShotLine(List<Vector2> hitPoints)
{
    GameObject lineObj = Instantiate(bulletLinePrefab, Vector3.zero, Quaternion.identity);
    LineRenderer lr = lineObj.GetComponent<LineRenderer>();

    lr.positionCount = hitPoints.Count;
    for (int i = 0; i < hitPoints.Count; i++)
    {
        lr.SetPosition(i, hitPoints[i]);
    }

    yield return new WaitForSeconds(lineDisplayTime);
    Destroy(lineObj);
}

float SnapAngleToEightDirections(float angle)
{
    angle = (angle + 360) % 360; // Normalize angle to [0..360)

    if (isOnStairs)
    {
        // -- If you have special "stairs" angles, adjust them likewise.
        //    (Below is just an example of how you might do it.)
        if (angle < 30 || angle >= 330)
            return 0;
        else if (angle >= 30 && angle < 75)
            return 60;
        else if (angle >= 75 && angle < 105)
            return 90;
        else if (angle >= 105 && angle < 150)
            return 120;
        else if (angle >= 150 && angle < 210)
            return 180;
        else if (angle >= 210 && angle < 255)
            return 240;
        else if (angle >= 255 && angle < 285)
            return 270;
        else if (angle >= 285 && angle < 330)
            return 300;
    }
    else
    {
        // -- Normal isometric 8 directions
        //    Adjusted so diagonals fall at 30°, 150°, 210°, and 330°.
        if (angle < 15 || angle >= 345)
            return 0;    // East
        else if (angle >= 15 && angle < 75)
            return 30;   // NE
        else if (angle >= 75 && angle < 105)
            return 90;   // North
        else if (angle >= 105 && angle < 165)
            return 150;  // NW
        else if (angle >= 165 && angle < 195)
            return 180;  // West
        else if (angle >= 195 && angle < 255)
            return 210;  // SW
        else if (angle >= 255 && angle < 285)
            return 270;  // South
        else if (angle >= 285 && angle < 345)
            return 330;  // SE
    }

    return 0;
}


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Stairs")
            {
                isOnStairs = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.tag == "Stairs")
            {
                isOnStairs = false;
            }
        }

        float GetPerpendicularAngle(float angle, bool isLeft)
        {
            // Calculate the base perpendicular angle (90 degrees offset)
            float perpendicularAngle = isLeft ? angle - 90 : angle + 90;
            perpendicularAngle = (perpendicularAngle + 360) % 360; // Normalize the angle

            // Use your SnapAngleToEightDirections function to snap to the nearest valid angle
            return SnapAngleToEightDirections(perpendicularAngle);
        }

        void HandleMovement()
        {
            if (Input.GetKey(KeyCode.W))
            {
                return;
            }
            else if (!isCrouching) // Allow strafing only when not crouching, if desired
            {
                if (Input.GetKey(KeyCode.S))
                {
                    movementDirection = -movementDirection; // Move backwards
                }

                else if (Input.GetKey(KeyCode.A))
                {
                    float leftAngle = GetPerpendicularAngle(Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg, true);
                    movementDirection = new Vector2(Mathf.Cos(leftAngle * Mathf.Deg2Rad), Mathf.Sin(leftAngle * Mathf.Deg2Rad));

                }
                else if (Input.GetKey(KeyCode.D))
                {

                    float rightAngle = GetPerpendicularAngle(Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg, false);
                    movementDirection = new Vector2(Mathf.Cos(rightAngle * Mathf.Deg2Rad), Mathf.Sin(rightAngle * Mathf.Deg2Rad));
                }
                else
                {
                    movementDirection = Vector2.zero; // No movement input
                }
            }
            else
            {
                movementDirection = Vector2.zero; // No movement input
            }
        }

        void HandleCrouching()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                isCrouching = !isCrouching; // Toggle crouching
                // speed = isCrouching ? 1.0f : 2.0f; // Adjust speed based on crouch state if needed

                if (isCrouching && isStealth)
                {
                    // Set the color to dark gray and reduce opacity to 50%
                    spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
                else
                {
                    // Reset the color to white and opacity to 100%
                    spriteRenderer.color = Color.white;
                }
            }
        }

        //Ranged character specific methods:

        public void SetArcherStatus(bool status)
        {
            isRanged = status;
        }

        public void SetActiveStatus(bool status)
        {
            isActive = status;
        }

        void DelayedShoot()
        {
            Vector2 fireDirection = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad));
            ShootProjectile(fireDirection);
        }

        void ShootProjectile(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GameObject projectileInstance = Instantiate(projectilePrefab, transform.position, Quaternion.Euler(0, 0, angle));
            Rigidbody2D rbProjectile = projectileInstance.GetComponent<Rigidbody2D>();
            if (rbProjectile != null)
            {
                rbProjectile.linearVelocity = direction * projectileSpeed;
            }
            // Destroy the instantiated prefab after another 1.5 seconds
            Destroy(projectileInstance, 1.5f);
        }

        IEnumerator Quickshot()
        {
            // Initial small delay before starting the quickshot sequence
            yield return new WaitForSeconds(0.1f);

            // Loop to fire five projectiles in the facing direction
            for (int i = 0; i < 5; i++)
            {
                Vector2 fireDirection = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad));
                ShootProjectile(fireDirection);

                // Wait for 0.18 seconds before firing the next projectile
                yield return new WaitForSeconds(0.18f);
            }
        }

        IEnumerator CircleShot()
        {
            float initialDelay = 0.1f;
            float timeBetweenShots = 0.9f / 8;  // Total time divided by the number of shots

            yield return new WaitForSeconds(initialDelay);

            // Use the lastAngle as the start angle and generate projectiles in 8 directions
            for (int i = 0; i < 8; i++)
            {
                float angle = lastAngle + i * 45;  // Increment by 45 degrees for each direction
                angle = Mathf.Deg2Rad * angle;  // Convert to radians for direction calculation
                Vector2 fireDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                ShootProjectile(fireDirection);

                yield return new WaitForSeconds(timeBetweenShots);
            }
        }

        IEnumerator DeployAoEDelayed()
        {
            if (AoEPrefab != null)
            {
                GameObject aoeInstance; // Declare outside to ensure visibility for later destruction

                if (isSummoner)
                {
                    // Get mouse position and convert it to world coordinates
                    Vector3 mouseScreenPosition = Input.mousePosition;
                    mouseScreenPosition.z = Camera.main.nearClipPlane; // Set this to your camera's near clip plane
                    Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

                    yield return new WaitForSeconds(0.3f); // Wait before instantiating (adjust time as needed)
                    // Instantiate the AoE prefab at the mouse's world position
                    aoeInstance = Instantiate(AoEPrefab, new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, 0), Quaternion.identity);

                    Destroy(aoeInstance, 8.7f);
                }
                else
                {
                    if(isMelee)
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                    else if(isShapeShifter)
                    {
                        yield return new WaitForSeconds(0.2f);
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.3f);
                    }
                    // Instantiate the AoE prefab at the player's position
                    aoeInstance = Instantiate(AoEPrefab, transform.position, Quaternion.identity);
                    Destroy(aoeInstance, 0.9f);
                }

                // Destroy the AoE instance after 0.9 seconds
                
            }
        }


        IEnumerator ShapeShiftDelayed()
        {
            if (ShapeShiftPrefab != null)
            {

                yield return new WaitForSeconds(0.001f);
                
                // Instantiate the AoE prefab at the player's position
                GameObject shapeShiftInstance = Instantiate(ShapeShiftPrefab, transform.position, Quaternion.identity);

                
                // Destroy the instantiated prefab after another 0.5 seconds
                Destroy(shapeShiftInstance, 0.9f);
            }
        }
        IEnumerator DeploySpecial1Delayed()
        {
            if (Special1Prefab != null)
            {
                GameObject Special1PrefabInstance; // Declare outside to ensure visibility for later destruction

                if (isSummoner)
                {
                    // Get mouse position and convert it to world coordinates
                    Vector3 mouseScreenPosition = Input.mousePosition;
                    mouseScreenPosition.z = Camera.main.nearClipPlane; // Set this to your camera's near clip plane
                    Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

                    yield return new WaitForSeconds(0.6f); // Wait before instantiating (adjust time as needed)
                    // Instantiate the Special1 prefab at the mouse's world position
                    Special1PrefabInstance = Instantiate(Special1Prefab, new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, 0), Quaternion.identity);
                }
                else
                {
                    if(isMelee)
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.6f);
                    }
                    // Instantiate the Special1 prefab at the player's position
                    Special1PrefabInstance = Instantiate(Special1Prefab, transform.position, Quaternion.identity);
                }

                // Destroy the Special1 instance after 1.0 seconds
                Destroy(Special1PrefabInstance, 1.0f);
            }
        }

        IEnumerator DeployHookDelayed()
        {
            GameObject hookInstance;
            if (isSummoner)
                {
                    // Get mouse position and convert it to world coordinates
                    Vector3 mouseScreenPosition = Input.mousePosition;
                    mouseScreenPosition.z = Camera.main.nearClipPlane; // Set this to your camera's near clip plane
                    Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

                    yield return new WaitForSeconds(0.6f); // Wait before instantiating (adjust time as needed)
                    // Instantiate the Special1 prefab at the mouse's world position
                    hookInstance = Instantiate(HookPrefab, new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, 0), Quaternion.identity);

                    Destroy(hookInstance, 5.2f);
                }
                else
                {
                    if (HookPrefab != null)
                    {
                        Vector2 direction = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad));
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        hookInstance = Instantiate(HookPrefab, transform.position, Quaternion.Euler(0, 0, angle));
                        // Destroy the instantiated prefab after another 1.0 seconds
                        Destroy(hookInstance, 1.0f);
                    }
                    yield return null; // Ensures the method correctly implements IEnumerator
                }
        }

        public void FlashGreen()
        {
            StartCoroutine(FlashEffect());
        }

        private IEnumerator FlashEffect()
        {
            spriteRenderer.color = Color.green; // Change to green
            yield return new WaitForSeconds(0.7f); // Wait for 0.2 seconds
            spriteRenderer.color = originalColor; // Restore original color
        }


        // Melee attack method
        // void MeleeAttack()
        // {
        //     if (meleePrefab != null)
        //     {
        //         StartCoroutine(DelayedMeleeAttack());
        //     }
        // }

        // IEnumerator DelayedMeleeAttack()
        // {
        //     // Wait for 0.5 seconds before initiating the melee attack
        //     yield return new WaitForSeconds(0.5f);

        //     Vector2 direction = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad));
        //     // Calculate the rotation angle for the melee attack
        //     float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        //     // Instantiate the melee attack prefab at the player's position
        //     GameObject meleeInstance = Instantiate(meleePrefab, transform.position, Quaternion.Euler(0, 0, angle));

        //     // Set the instantiated melee attack prefab as a child of the player
        //     meleeInstance.transform.SetParent(transform);

        //     // Optionally, destroy the melee attack prefab after a short duration
        //     Destroy(meleeInstance, 0.1f); // Adjust the duration as needed
        // }

    }
}
