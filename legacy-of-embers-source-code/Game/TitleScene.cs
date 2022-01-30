using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    /// <summary>
    /// Class to control the behaviours present in the Title Scene.
    /// </summary>
    public class TitleScene : MonoBehaviour
    {
        /// <summary>
        /// Audio source to play the ambient audio.
        /// </summary>
        [SerializeField]
        private AudioSource ambience;

        /// <summary>
        /// Audio source of the bonfire.
        /// </summary>
        [SerializeField]
        private AudioSource bonfire;

        /// <summary>
        /// Button to start the game.
        /// </summary>
        [SerializeField]
        private Button playButton;

        /// <summary>
        /// Button to quit the game.
        /// </summary>
        [SerializeField]
        private Button quitButton;

        private IEnumerator Start()
        {
            yield return FadeInAudio();
        }

        /// <summary>
        /// Hooks into the game manager to move game to the main scene.
        /// </summary>
        public void MoveToNextScene()
        {
            DisableAllButtons();
            StartCoroutine(FadeOutAudio());
            GameManager.Instance.LoadScene("Main");
        }

        /// <summary>
        /// Quits the application.
        /// </summary>
        public void QuitGame()
        {
            GameManager.Instance.QuitGame();
        }

        /// <summary>
        /// Prevents the player from clicking on buttons.
        /// </summary>
        private void DisableAllButtons()
        {
            playButton.interactable = false;
            quitButton.interactable = false;
        }

        /// <summary>
        /// Smoothly fades in all audio in the scene.
        /// </summary>
        /// <returns></returns>
        public IEnumerator FadeInAudio()
        {
            float endVolAmbience = ambience.volume;
            float endVolBonfire = bonfire.volume;

            ambience.volume = 0;
            bonfire.volume = 0;

            float elapsedTime = 0.0f;
            float fadeInTime = GameManager.Instance.FadeInTime;

            while (elapsedTime < fadeInTime)
            {
                ambience.volume =
                    Mathf.Lerp(0.0f, endVolAmbience, elapsedTime / fadeInTime);
                bonfire.volume =
                    Mathf.Lerp(0.0f, endVolBonfire, elapsedTime / fadeInTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Smoothly fades out all audio in the scene.
        /// </summary>
        public IEnumerator FadeOutAudio()
        {
            float startVolAmbience = ambience.volume;
            float startVolBonfire = bonfire.volume;

            float elapsedTime = 0.0f;
            float fadeOutTime = GameManager.Instance.FadeOutTime;

            while (elapsedTime < fadeOutTime)
            {
                ambience.volume =
                    Mathf.Lerp(startVolAmbience, 0.0f, elapsedTime / fadeOutTime);
                bonfire.volume =
                    Mathf.Lerp(startVolBonfire, 0.0f, elapsedTime / fadeOutTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}

