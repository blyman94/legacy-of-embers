using Game.Entity;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Indicator to represent when an actor is covered relative to another 
    /// actor.
    /// </summary>
    public class CoverIndicator : MonoBehaviour
    {
        [Tooltip("Actor to which this indicator belongs.")]
        public Actor Actor;

        [Header("Image Parameters")]

        [Tooltip("Image that represents the cover indicator.")]
        public Image IndicatorImage;

        [Tooltip("Indicator will start to fade after this time elapses.")]
        public float fadeAfterTime;

        [Tooltip("Indicator will take this long to fade to clear.")]
        public float fadeTime;

        /// <summary>
        /// Camera through which the meter will be viewed.
        /// </summary>
        private Camera MainCamera;

        /// <summary>
        /// Determines if the indicator is currently showing above the actor.
        /// </summary>
        private bool indicatorShown;

        /// <summary>
        /// Used to store the current color of the image, so it can be restored
        /// after the image is faded to the clear color.
        /// </summary>
        private Color imageColor;

        private void OnEnable()
        {
            Actor.ShowCoverIndicator += ShowCoverIndicator;
        }

        private void Start()
        {
            MainCamera = Camera.main;
            imageColor = IndicatorImage.color;
            IndicatorImage.color = Color.clear;
        }

        private void LateUpdate()
        {
            if (indicatorShown)
            {
                transform.LookAt(MainCamera.transform);
                transform.Rotate(0, 180, 0);
            }
        }

        /// <summary>
        /// Reveal the cover indicator above the actor.
        /// </summary>
        private void ShowCoverIndicator()
        {
            StopAllCoroutines();
            indicatorShown = true;
            IndicatorImage.color = imageColor;
            StartCoroutine(HideCoverIndicatorRoutine());
        }

        /// <summary>
        /// Conceal the cover indicator above the actor.
        /// </summary>
        private IEnumerator HideCoverIndicatorRoutine()
        {
            yield return new WaitForSeconds(fadeAfterTime);
            float elapsedTime = 0.0f;
            while (elapsedTime < fadeTime)
            {
                IndicatorImage.color = Color.Lerp(imageColor, Color.clear, (elapsedTime / fadeTime));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            indicatorShown = false;
        }
    }
}

