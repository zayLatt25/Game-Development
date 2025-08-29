using UnityEngine;

// Helper script to guide users through setting up the audio system
// This script will automatically configure audio clips if they're not assigned
public class AudioSetupHelper : MonoBehaviour
{
    [Header("Audio Setup Instructions")]
    [TextArea(5, 10)]
    [SerializeField] private string setupInstructions = 
        "AUDIO SETUP INSTRUCTIONS:\n\n" +
        "1. Make sure your Player GameObject has an AudioSource component\n" +
        "2. Assign the audio clips in the Player script inspector:\n" +
        "   - Infection Sound: infection.wav\n" +
        "   - Vaccine Pickup Sound: vaccine.wav\n" +
        "   - Weapon Pickup Sound: weapon_pickup.wav\n" +
        "3. Or use the AudioManager for centralized audio management\n" +
        "4. Ensure the audio files are in the Audio folder\n\n" +
        "The audio system is already implemented in the code!";

    [Header("Auto-Setup (Optional)")]
    [SerializeField] private bool autoSetupAudioClips = false;
    
    private void Start()
    {
        if (autoSetupAudioClips)
        {
            SetupAudioClips();
        }
        
        // Log setup instructions
        Debug.Log(setupInstructions);
    }
    
    private void SetupAudioClips()
    {
        // Find Player and try to auto-assign audio clips
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            // Try to find audio clips by name
            AudioClip[] allClips = Resources.FindObjectsOfTypeAll<AudioClip>();
            
            foreach (AudioClip clip in allClips)
            {
                if (clip.name.ToLower().Contains("infection"))
                {
                    Debug.Log($"Found infection sound: {clip.name}");
                }
                else if (clip.name.ToLower().Contains("vaccine"))
                {
                    Debug.Log($"Found vaccine sound: {clip.name}");
                }
                else if (clip.name.ToLower().Contains("weapon") || clip.name.ToLower().Contains("pickup"))
                {
                    Debug.Log($"Found weapon pickup sound: {clip.name}");
                }
            }
        }
    }
    
    [ContextMenu("Check Audio Setup")]
    private void CheckAudioSetup()
    {
        Player player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("No Player found in scene!");
            return;
        }
        
        // Check if Player has AudioSource
        AudioSource playerAudio = player.GetComponent<AudioSource>();
        if (playerAudio == null)
        {
            Debug.LogError("Player is missing AudioSource component!");
        }
        else
        {
            Debug.Log("✓ Player has AudioSource component");
        }
        
        // Check if AudioManager exists
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogWarning("No AudioManager found. Consider creating one for centralized audio management.");
        }
        else
        {
            Debug.Log("✓ AudioManager found");
        }
        
        // Check audio files
        CheckAudioFiles();
    }
    
    private void CheckAudioFiles()
    {
        string[] requiredFiles = { "infection", "vaccine", "weapon_pickup" };
        
        foreach (string fileName in requiredFiles)
        {
            AudioClip[] clips = Resources.FindObjectsOfTypeAll<AudioClip>();
            bool found = false;
            
            foreach (AudioClip clip in clips)
            {
                if (clip.name.ToLower().Contains(fileName.ToLower()))
                {
                    Debug.Log($"✓ Found audio file: {clip.name}");
                    found = true;
                    break;
                }
            }
            
            if (!found)
            {
                Debug.LogWarning($"⚠ Missing audio file: {fileName}");
            }
        }
    }
}
