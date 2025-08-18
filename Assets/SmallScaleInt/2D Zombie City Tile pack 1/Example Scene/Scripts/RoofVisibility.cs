using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

namespace SmallScaleInc.TopDownPixelCharactersPack1
{
    public class RoofVisibility : MonoBehaviour
    {
        public TilemapRenderer roofRenderer; // Assign the Roof TilemapRenderer in the Inspector
        public TilemapRenderer roofDetailsRenderer; // Assign the RoofDetails TilemapRenderer in the Inspector
        private int roofCounter = 0; // Tracks how many roof tiles the player is under

        [Header("Fade Settings")]
        public float fadeDuration = 0.5f;   // Duration for fading in/out
        private Coroutine currentFadeCoroutine = null;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<TilemapCollider2D>() != null)
            {
                TilemapRenderer tilemapRenderer = other.GetComponent<TilemapRenderer>();
                if (tilemapRenderer == roofRenderer || tilemapRenderer == roofDetailsRenderer)
                {
                    roofCounter++;
                    if (roofCounter == 1) // Only trigger fade when first entering a roof area
                    {
                        FadeRoof(false); // Fade out (make transparent)
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<TilemapCollider2D>() != null)
            {
                TilemapRenderer tilemapRenderer = other.GetComponent<TilemapRenderer>();
                if (tilemapRenderer == roofRenderer || tilemapRenderer == roofDetailsRenderer)
                {
                    roofCounter--;
                    if (roofCounter <= 0) // Only fade back in when completely outside a roof area
                    {
                        roofCounter = 0;
                        FadeRoof(true); // Fade in (make opaque)
                    }
                }
            }
        }

        /// <summary>
        /// Initiates a fade coroutine to smoothly transition the roof and roof details alpha.
        /// </summary>
        private void FadeRoof(bool fadeIn)
        {
            float startAlpha = fadeIn ? 0f : 1f;
            float endAlpha = fadeIn ? 1f : 0f;

            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
            }
            currentFadeCoroutine = StartCoroutine(FadeRoofCoroutine(startAlpha, endAlpha));
        }

        /// <summary>
        /// Gradually interpolates the roof and roof details tilemap's alpha from startAlpha to endAlpha over fadeDuration.
        /// </summary>
        private IEnumerator FadeRoofCoroutine(float startAlpha, float endAlpha)
        {
            float elapsedTime = 0f;

            // Get the Tilemap components
            Tilemap roofTilemap = roofRenderer.GetComponent<Tilemap>();
            Tilemap roofDetailsTilemap = roofDetailsRenderer.GetComponent<Tilemap>();

            // Ensure both tilemaps start at the specified startAlpha
            Color initialRoofColor = roofTilemap.color;
            initialRoofColor.a = startAlpha;
            roofTilemap.color = initialRoofColor;

            Color initialRoofDetailsColor = roofDetailsTilemap.color;
            initialRoofDetailsColor.a = startAlpha;
            roofDetailsTilemap.color = initialRoofDetailsColor;

            while (elapsedTime < fadeDuration)
            {
                float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);

                Color newRoofColor = roofTilemap.color;
                newRoofColor.a = newAlpha;
                roofTilemap.color = newRoofColor;

                Color newRoofDetailsColor = roofDetailsTilemap.color;
                newRoofDetailsColor.a = newAlpha;
                roofDetailsTilemap.color = newRoofDetailsColor;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Guarantee final alpha is set
            Color finalRoofColor = roofTilemap.color;
            finalRoofColor.a = endAlpha;
            roofTilemap.color = finalRoofColor;

            Color finalRoofDetailsColor = roofDetailsTilemap.color;
            finalRoofDetailsColor.a = endAlpha;
            roofDetailsTilemap.color = finalRoofDetailsColor;
        }
    }
}