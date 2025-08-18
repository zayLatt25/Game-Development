using UnityEngine;
using SmallScaleInc.TopDownPixelCharactersPack1; // Include the namespace

namespace SmallScaleInc.TopDownPixelCharactersPack1
{
    public class ChestInteraction : MonoBehaviour
    {
        private Animator animator;
        private bool isOpened = false;
        private PlayerController playerController;

        void Start()
        {
            playerController = FindObjectOfType<PlayerController>();
            animator = GetComponent<Animator>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && !isOpened)
            {
                OpenChest();
            }
        }

        private void OpenChest()
        {
            animator.SetTrigger("Open"); // Play animation when triggered
            isOpened = true;

            if (playerController != null)
            {
                playerController.currentHealth = playerController.maxHealth; // Restore player's health
                playerController.healthSlider.value = playerController.currentHealth;
                playerController.FlashGreen(); // Trigger the green flash effect
                // Debug.Log("Player health restored to max!");
            }
            else
            {
                Debug.LogWarning("PlayerController not found in the scene!");
            }
        }
    }
}