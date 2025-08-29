using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Player Audio Clips")]
    [SerializeField] private AudioClip infectionSound;
    [SerializeField] private AudioClip vaccinePickupSound;
    [SerializeField] private AudioClip weaponPickupSound;
    
    [Header("Zombie Audio Clips")]
    [SerializeField] private AudioClip hitFleshSound;
    [SerializeField] private AudioClip deathSound;
    
    [Header("Weapon Audio Clips")]
    [SerializeField] private AudioClip knifeSlashSound;
    [SerializeField] private AudioClip axeSlashSound;
    [SerializeField] private AudioClip submachineGunSound;
    
    [Header("Audio Settings")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float sfxVolume = 0.8f;
    [SerializeField] private float musicVolume = 0.6f;
    
    private static AudioManager _instance;
    public static AudioManager Instance => _instance;
    
    private void Awake()
    {
        // Singleton pattern
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Player audio methods
    public AudioClip GetInfectionSound() => infectionSound;
    public AudioClip GetVaccinePickupSound() => vaccinePickupSound;
    public AudioClip GetWeaponPickupSound() => weaponPickupSound;
    
    // Zombie audio methods
    public AudioClip GetHitFleshSound() => hitFleshSound;
    public AudioClip GetDeathSound() => deathSound;
    
    // Weapon audio methods
    public AudioClip GetKnifeSlashSound() => knifeSlashSound;
    public AudioClip GetAxeSlashSound() => axeSlashSound;
    public AudioClip GetSubmachineGunSound() => submachineGunSound;
    
    // Volume control methods
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        AudioListener.volume = masterVolume;
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
    }
    
    public float GetMasterVolume() => masterVolume;
    public float GetSFXVolume() => sfxVolume;
    public float GetMusicVolume() => musicVolume;
    
    // Helper method to get adjusted volume for SFX
    public float GetAdjustedSFXVolume()
    {
        return masterVolume * sfxVolume;
    }
}
