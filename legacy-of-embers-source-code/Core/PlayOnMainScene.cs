using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Plays all AudioSources in its AudioSources array when the game enters
    /// the main scene.
    /// </summary>
    public class PlayOnMainScene : MonoBehaviour
    {
        public AudioSource[] AudioSources;

        private void Start()
        {
            GameManager.Instance.GameStateChanged += PlayAudio;
        }

        private void PlayAudio(GameState previous, GameState current)
        {
            if (previous == GameState.Default && current == GameState.Pregame)
            {
                foreach (AudioSource source in AudioSources)
                {
                    source.Play();
                }
            }
        }
    }
}

