using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : LivingEntity
{
    private static readonly Color InfectedTint = new(1, 0.5f, 0.5f);
    private const KeyCode PickupKey = KeyCode.C;
    private const KeyCode WeaponSwitchKey = KeyCode.Q;

    [Header("Renderers")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private AudioSource _audioSource;
    
    [Header("Audio Clips (Optional - will use AudioManager if not assigned)")]
    [SerializeField] private AudioClip _infectionSound;
    [SerializeField] private AudioClip _vaccinePickupSound;
    [SerializeField] private AudioClip _weaponPickupSound;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Status")]
    [SerializeField] private bool _isInfected = false;
    public bool IsInfected => _isInfected;
    public event Action<bool> IsInfectedChanged;

    [Header("Weapons")]
    [SerializeField] private int _currentWeaponIndex = 0;
    [SerializeField] private WeaponData[] _weapons;

    private Camera _mainCamera;
    private Vector2 _moveInput;
    private float _nextFireTime;
    private Coroutine _infectedCoroutine;
    private Coroutine _meleeSwingCoroutine;
    private WeaponData CurrentWeapon => _weapons[_currentWeaponIndex];
    private GameController _gameController; // Reference to check pause state
    private TutorialManager _tutorialManager; // Reference to check tutorial state

    protected override void Start()
    {
        base.Start();
        _mainCamera = Camera.main;

        _gameController = FindObjectOfType<GameController>();
        _tutorialManager = FindObjectOfType<TutorialManager>();

        // Initialize weapons
        if (_weapons != null && _weapons.Length > 0)
        {
            InitializeWeapons();
            SwitchToWeapon(0);
        }
    }

    private void InitializeWeapons()
    {
        // Disable all weapon renderers initially and set weapon types
        for (int i = 0; i < _weapons.Length; i++)
        {
            var weapon = _weapons[i];
            
            // Set weapon types based on index (you can customize this in the inspector)
            if (i == 0) // SubMachineGun
            {
                weapon.Type = WeaponType.Firearm;
                weapon.IsFirearm = true;
            }
            else if (i == 1) // AK47
            {
                weapon.Type = WeaponType.Firearm;
                weapon.IsFirearm = true;
            }
            else if (i == 2) // Knife
            {
                weapon.Type = WeaponType.Melee;
                weapon.IsFirearm = false;
            }
            else if (i == 3) // Axe
            {
                weapon.Type = WeaponType.Melee;
                weapon.IsFirearm = false;
            }
            else if (i == 4) // Flamethrower
            {
                weapon.Type = WeaponType.Flamethrower;
                weapon.IsFirearm = false;
            }
            
            if (weapon.GunRenderer != null)
                weapon.GunRenderer.gameObject.SetActive(false);
            if (weapon.MuzzleRenderer != null)
                weapon.MuzzleRenderer.enabled = false;
        }
    }

    private void Update()
    {
        // Check if game is paused - if so, don't process input
        if (_gameController != null && _gameController.IsPaused)
            return;

        HandleMovementInput();
        HandleAiming();
        HandleFiring();
        HandlePickup();
        HandleWeaponSwitch();
    }

    private void HandleMovementInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        _moveInput = new Vector2(moveX, moveY).normalized;

        // Apply movement speed modifier during tutorial
        float speedMultiplier = 1f;
        if (_tutorialManager != null)
        {
            speedMultiplier = _tutorialManager.GetTutorialMovementSpeedMultiplier();
        }

        transform.position += (Vector3)_moveInput * (_moveSpeed * speedMultiplier * Time.deltaTime);
    }
    private void HandleAiming()
    {
        if (_weapons == null || _weapons.Length == 0 || CurrentWeapon.GunRenderer == null)
            return;

        // Don't override aiming during melee swing
        if (!CurrentWeapon.IsFirearm && _meleeSwingCoroutine != null)
            return;

        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = (mouseWorldPos - transform.position);

        // Rotate gun towards mouse
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        CurrentWeapon.GunRenderer.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Flip player sprite
        _spriteRenderer.flipX = aimDirection.x < 0f;

        // Flip gun with scale
        Vector3 gunScale = CurrentWeapon.GunRenderer.transform.localScale;
        gunScale.y = Mathf.Abs(gunScale.y);
        if (aimDirection.x < 0f)
            gunScale.y = -Mathf.Abs(gunScale.y);
        CurrentWeapon.GunRenderer.transform.localScale = gunScale;
    }

    private void HandleFiring()
    {
        if (_weapons == null || _weapons.Length == 0) return;

        bool shouldFire = false;
        
        // Handle different weapon types
        switch (CurrentWeapon.Type)
        {
            case WeaponType.Firearm:
                shouldFire = Input.GetMouseButton(0);
                break;
            case WeaponType.Melee:
                shouldFire = Input.GetMouseButtonDown(0);
                break;
            case WeaponType.Flamethrower:
                shouldFire = Input.GetMouseButton(0); // Continuous fire
                break;
        }

        if (shouldFire && Time.time >= _nextFireTime)
        {
            switch (CurrentWeapon.Type)
            {
                case WeaponType.Firearm:
                    Fire();
                    break;
                case WeaponType.Melee:
                    MeleeAttack();
                    break;
                case WeaponType.Flamethrower:
                    HandleFlamethrower();
                    break;
            }
        }
        else if (CurrentWeapon.Type == WeaponType.Flamethrower && !shouldFire)
        {
            // Stop flamethrower when not firing
            StopFlamethrower();
        }
    }

    private void Fire()
    {
        WeaponData weapon = CurrentWeapon;

        // Set next fire time
        _nextFireTime = Time.time + (1f / weapon.FireRate);

        // Spawn bullet/projectile
        if (weapon.BulletPrefab != null && weapon.GunRenderer != null)
        {
            Vector2 dir = weapon.GunRenderer.transform.right; // gun faces right by default
            Vector3 spawnPos = weapon.MuzzleRenderer != null ?
                weapon.MuzzleRenderer.transform.position :
                weapon.GunRenderer.transform.position;

            Bullet bullet = Instantiate(weapon.BulletPrefab, spawnPos, Quaternion.identity);
            bullet.Initialize(dir);
        }

        // Muzzle flash (only for firearms)
        if (weapon.IsFirearm && weapon.MuzzleRenderer != null)
        {
            StartCoroutine(ShowMuzzleFlash(weapon.MuzzleRenderer));
        }

        // Play sound effect
        if (weapon.Sfx != null)
        {
            _audioSource.PlayOneShot(weapon.Sfx, Random.Range(0.08f, 0.12f));
        }

        Debug.Log($"Fired {(_currentWeaponIndex < _weapons.Length ? $"weapon {_currentWeaponIndex}" : "unknown weapon")}!");
    }

    private void MeleeAttack()
    {
        WeaponData weapon = CurrentWeapon;

        // Set next attack time
        _nextFireTime = Time.time + (1f / weapon.FireRate);

        // Start melee swing animation
        if (_meleeSwingCoroutine != null)
            StopCoroutine(_meleeSwingCoroutine);

        _meleeSwingCoroutine = StartCoroutine(MeleeSwingCoroutine());

        // Play sound effect
        if (weapon.Sfx != null)
        {
            var volume = weapon.IsFirearm ? Random.Range(0.08f, 0.12f) : 1f;
            _audioSource.PlayOneShot(weapon.Sfx, volume);
        }

        Debug.Log($"Melee attack with weapon {_currentWeaponIndex}!");
    }
    
    private void HandleFlamethrower()
    {
        WeaponData weapon = CurrentWeapon;
        
        // Set next fire time for continuous fire
        _nextFireTime = Time.time + (1f / weapon.FireRate);
        
        // Get the flamethrower component and start firing
        if (weapon.GunRenderer != null)
        {
            var flamethrower = weapon.GunRenderer.GetComponent<FlamethrowerWeapon>();
            if (flamethrower != null)
            {
                flamethrower.StartFlamethrower();
            }
        }
        
        Debug.Log($"Flamethrower active with weapon {_currentWeaponIndex}!");
    }
    
    private void StopFlamethrower()
    {
        WeaponData weapon = CurrentWeapon;
        
        if (weapon.GunRenderer != null)
        {
            var flamethrower = weapon.GunRenderer.GetComponent<FlamethrowerWeapon>();
            if (flamethrower != null)
            {
                flamethrower.StopFlamethrower();
            }
        }
    }

    private IEnumerator MeleeSwingCoroutine()
    {
        WeaponData weapon = CurrentWeapon;
        if (weapon.GunRenderer == null) yield break;

        var melee = weapon.GunRenderer.GetComponent<MeleeWeapon>();
        if (melee == null)
        {
            Debug.LogError($"MeleeWeapon component not found on {weapon.GunRenderer.name}!");
            yield break;
        }

        // Start with damage disabled
        melee.CanDealDamage = false;

        // Get mouse direction for swing center
        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseDirection = (mouseWorldPos - transform.position).normalized;
        float centerAngle = Mathf.Atan2(mouseDirection.y, mouseDirection.x) * Mathf.Rad2Deg;

        // Calculate swing arc based on facing direction
        float startAngle, endAngle;
        const float SwingDegree = 70f;
        if (mouseDirection.x >= 0f) // Facing right
        {
            startAngle = centerAngle + SwingDegree;
            endAngle = centerAngle - SwingDegree;
        }
        else // Facing left - swing from top to bottom
        {
            startAngle = centerAngle - SwingDegree;
            endAngle = centerAngle + SwingDegree;
        }

        float swingDuration = 0.3f;
        float elapsedTime = 0f;

        // Store original rotation for smooth transition
        Quaternion originalRotation = weapon.GunRenderer.transform.rotation;
        float originalAngle = originalRotation.eulerAngles.z;
        if (originalAngle > 180f) originalAngle -= 360f; // Normalize to -180 to 180

        // Phase 1: Quick lerp to start position (10% of duration)
        float prepDuration = swingDuration * 0.1f;
        while (elapsedTime < prepDuration)
        {
            float prepProgress = elapsedTime / prepDuration;
            float currentAngle = Mathf.LerpAngle(originalAngle, startAngle, prepProgress);
            weapon.GunRenderer.transform.rotation = Quaternion.Euler(0, 0, currentAngle);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Phase 2: Main swing (80% of duration) - ENABLE DAMAGE HERE
        melee.CanDealDamage = true;
        float swingStartTime = elapsedTime;
        float mainDuration = swingDuration * 0.8f;

        while (elapsedTime < swingStartTime + mainDuration)
        {
            float swingProgress = (elapsedTime - swingStartTime) / mainDuration;
            float currentAngle = Mathf.LerpAngle(startAngle, endAngle, swingProgress);
            weapon.GunRenderer.transform.rotation = Quaternion.Euler(0, 0, currentAngle);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Phase 3: Return to resting position (10% of duration) - DISABLE DAMAGE HERE
        melee.CanDealDamage = false;
        float finalDuration = swingDuration * 0.1f;
        float finalStartTime = elapsedTime;
        float restingAngle = centerAngle; // Rest at mouse direction

        while (elapsedTime < finalStartTime + finalDuration)
        {
            mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseDirection = (mouseWorldPos - transform.position).normalized;
            restingAngle = Mathf.Atan2(mouseDirection.y, mouseDirection.x) * Mathf.Rad2Deg;
            float finalProgress = (elapsedTime - finalStartTime) / finalDuration;
            float currentAngle = Mathf.LerpAngle(endAngle, restingAngle, finalProgress);
            weapon.GunRenderer.transform.rotation = Quaternion.Euler(0, 0, currentAngle);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final rotation is set to resting position
        weapon.GunRenderer.transform.rotation = Quaternion.Euler(0, 0, restingAngle);

        melee.CanDealDamage = false;
        _meleeSwingCoroutine = null;
    }

    private IEnumerator ShowMuzzleFlash(SpriteRenderer muzzleRenderer)
    {
        muzzleRenderer.enabled = true;
        yield return new WaitForSeconds(0.05f);
        muzzleRenderer.enabled = false;
    }

    private void HandleWeaponSwitch()
    {
        // Number key weapon switching (1-9)
        for (int i = 0; i < Mathf.Min(9, _weapons?.Length ?? 0); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SwitchToWeapon(i);
                break;
            }
        }
    }

    private void SwitchToWeapon(int weaponIndex)
    {
        if (_weapons == null || weaponIndex < 0 || weaponIndex >= _weapons.Length)
            return;

        // Disable current weapon renderers
        if (_currentWeaponIndex >= 0 && _currentWeaponIndex < _weapons.Length)
        {
            var currentWeapon = _weapons[_currentWeaponIndex];
            if (currentWeapon.GunRenderer != null)
                currentWeapon.GunRenderer.gameObject.SetActive(false);
            if (currentWeapon.MuzzleRenderer != null)
                currentWeapon.MuzzleRenderer.enabled = false;
        }

        // Switch to new weapon
        _currentWeaponIndex = weaponIndex;
        var newWeapon = _weapons[_currentWeaponIndex];

        // Enable new weapon renderers
        if (newWeapon.GunRenderer != null)
            newWeapon.GunRenderer.gameObject.SetActive(true);

        _audioSource.pitch = 1 + newWeapon.PitchOffset;

        Debug.Log($"Switched to weapon {_currentWeaponIndex}");
    }

    public void Infect()
    {
        _isInfected = true;
        TintSprite();
        IsInfectedChanged?.Invoke(_isInfected);

        // Play infection sound
        AudioClip clipToPlay = _infectionSound != null ? _infectionSound : 
            (AudioManager.Instance != null ? AudioManager.Instance.GetInfectionSound() : null);
        
        if (clipToPlay != null && _audioSource != null)
        {
            float volume = AudioManager.Instance != null ? AudioManager.Instance.GetAdjustedSFXVolume() : 0.8f;
            _audioSource.PlayOneShot(clipToPlay, volume);
        }

        if (_infectedCoroutine != null) StopCoroutine(_infectedCoroutine);
        _infectedCoroutine = StartCoroutine(InfectedCoroutine());
    }

    private IEnumerator InfectedCoroutine()
    {
        while (_isInfected)
        {
            yield return new WaitForSeconds(1f);
            if (_isInfected) TakeDamage(1);
        }
    }

    private void HandlePickup()
    {
        if (Input.GetKeyDown(PickupKey))
        {
            DroppableItem nearest = FindNearestItem();
            if (nearest != null)
            {

                switch (nearest.ItemType)
                {
                    case DroppableItem.Type.Vaccine:
                        // Play vaccine pickup sound
                        AudioClip vaccineClip = _vaccinePickupSound != null ? _vaccinePickupSound : 
                            (AudioManager.Instance != null ? AudioManager.Instance.GetVaccinePickupSound() : null);
                        
                        if (vaccineClip != null && _audioSource != null)
                        {
                            float volume = AudioManager.Instance != null ? AudioManager.Instance.GetAdjustedSFXVolume() : 0.7f;
                            _audioSource.PlayOneShot(vaccineClip, volume);
                        }
                        
                        CureInfection();
                        TakeDamage(-10); // heal
                        break;

                    case DroppableItem.Type.SubMachineGun:
                    case DroppableItem.Type.Ak47:
                    case DroppableItem.Type.Knife:
                    case DroppableItem.Type.Axe:
                    case DroppableItem.Type.Flamethrower:
                        // Play weapon pickup sound
                        AudioClip weaponClip = _weaponPickupSound != null ? _weaponPickupSound : 
                            (AudioManager.Instance != null ? AudioManager.Instance.GetWeaponPickupSound() : null);
                        
                        if (weaponClip != null && _audioSource != null)
                        {
                            float volume = AudioManager.Instance != null ? AudioManager.Instance.GetAdjustedSFXVolume() : 0.6f;
                            _audioSource.PlayOneShot(weaponClip, volume);
                        }
                        
                        SwitchToWeapon(nearest.ItemType switch
                        {
                            DroppableItem.Type.SubMachineGun => 0,
                            DroppableItem.Type.Ak47 => 1,
                            DroppableItem.Type.Knife => 2,
                            DroppableItem.Type.Axe => 3,
                            DroppableItem.Type.Flamethrower => 4,
                            _ => throw new ArgumentOutOfRangeException($"Unknown ItemType {nearest.ItemType}")
                        });
                        break;
                }

                Destroy(nearest.gameObject);
            }
        }
    }

    private DroppableItem FindNearestItem()
    {
        DroppableItem[] items = FindObjectsOfType<DroppableItem>();
        DroppableItem nearest = null;
        float minDist = float.MaxValue;

        foreach (var item in items)
        {
            float dist = Vector2.Distance(transform.position, item.transform.position);
            if (dist < item.PickupRange && dist < minDist)
            {
                minDist = dist;
                nearest = item;
            }
        }

        return nearest;
    }

    private void CureInfection()
    {
        if (_isInfected)
        {
            _isInfected = false;
            TintSprite();
            IsInfectedChanged?.Invoke(_isInfected);
        }
    }

    public void Knockback(Vector2 direction, float force)
    {
        Rigidbody2D.AddForce(direction * force, ForceMode2D.Impulse);
    }

    private void TintSprite()
    {
        _spriteRenderer.color = _isInfected ? InfectedTint : Color.white;
    }

    private void HandleShooting()
    {
        if (Input.GetMouseButton(0) && Time.time >= _nextFireTime)
        {
            Fire();

            // ADD SCREEN SHAKE
            StartCoroutine(ScreenShake(0.1f, 0.1f));

            // ADD MUZZLE FLASH
            if (CurrentWeapon.MuzzleRenderer != null)
            {
                StartCoroutine(MuzzleFlash());
            }
        }
    }

    private IEnumerator ScreenShake(float duration, float magnitude)
    {
        Vector3 originalPos = _mainCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            _mainCamera.transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _mainCamera.transform.localPosition = originalPos;
    }

    private IEnumerator MuzzleFlash()
    {
        CurrentWeapon.MuzzleRenderer.enabled = true;
        yield return new WaitForSeconds(0.05f);
        CurrentWeapon.MuzzleRenderer.enabled = false;
    }

    [Serializable]
    public struct WeaponData
    {
        public bool IsFirearm;
        public WeaponType Type;
        public SpriteRenderer GunRenderer;
        public SpriteRenderer MuzzleRenderer;
        public Bullet BulletPrefab;
        public int FireRate;
        public AudioClip Sfx;
        public float PitchOffset;
    }
    
    public enum WeaponType
    {
        Firearm,
        Melee,
        Flamethrower
    }
}