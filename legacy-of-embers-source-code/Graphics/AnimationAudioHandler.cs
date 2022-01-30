using UnityEngine;

namespace Game.Graphics
{
    /// <summary>
    /// Handles audio played during animation events.
    /// </summary>
    public class AnimationAudioHandler : MonoBehaviour
    {
        [Header("AudioSources")]

        [Tooltip("AudioSource to play footstep audio through.")]
        public AudioSource FootstepAudio;

        [Tooltip("AudioSource to play vault audio through.")]
        public AudioSource VaultAudio;

        [Tooltip("AudioClip to be played on footstep events.")]
        public AudioClip FootstepClip;

        [Tooltip("AudioClip to be played on vault events.")]
        public AudioClip VaultSwooshClip;

        /// <summary>
        /// Plays a footstep noise in response to an animation event.
        /// </summary>
        public void OnFootstep()
        {
            FootstepAudio.pitch = Random.Range(0.8f, 1.2f);
            FootstepAudio.PlayOneShot(FootstepClip, 1.0f);
        }

        /// <summary>
        /// Plays a vault noise in response to an animation event.
        /// </summary>
        public void OnVault()
        {
            VaultAudio.pitch = Random.Range(0.8f, 1.2f);
            VaultAudio.PlayOneShot(VaultSwooshClip, 1.0f);
        }
    }
}

