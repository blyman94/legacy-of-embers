using UnityEngine;

namespace Game.Graphics
{
    public class WeaponGraphics : MonoBehaviour
    {
        /// <summary>
        /// The particle system that mimics a muzzle flash when a gun is fired.
        /// </summary>
        [SerializeField]
        private ParticleSystem muzzleFlash;

        /// <summary>
        /// Plays the muzzle flash animation of the weapon.
        /// </summary>
        public void Fire()
        {
            muzzleFlash.Play();
        }
    }
}

