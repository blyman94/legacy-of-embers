using Game.Core;
using Game.Combat;
using System.Collections;
using UnityEngine;

namespace Game.Graphics
{
    /// <summary>
    /// Listens for events from various components of a Humanoid and updates the
    /// Humanoid's animator component accordingly.
    /// </summary>
    public class HumanoidAnimationHandler : MonoBehaviour
    {
        [Tooltip("Animator of this humanoid.")]
        public Animator Animator;

        /// <summary>
        /// Fighter component of this humanoid.
        /// </summary>
        public Fighter Fighter { get; set; }

        /// <summary>
        /// Determines if the humanoid is currently taking damage.
        /// </summary>
        private bool isTakingDamage = false;

        private void OnEnable()
        {
            Fighter.Fire += FireWeapon;
        }

        /// <summary>
        /// Triggers the build animation.
        /// </summary>
        public void Build()
        {
            Animator.SetTrigger("build_t");
        }

        /// <summary>
        /// Triggers the crouch animation.
        /// </summary>
        public void Crouch()
        {
            Animator.SetTrigger("crouch_t");
            Animator.SetBool("crouching_b", true);
        }

        /// <summary>
        /// Triggers the death animation.
        /// </summary>
        public void Die()
        {
            int deathAnimIndex = Random.Range(0, 4);
            Animator.SetTrigger("die_t" + deathAnimIndex);
        }

        /// <summary>
        /// Triggers the draw weapon animation.
        /// </summary>
        public void DrawWeapon()
        {
            Animator.SetTrigger("drawWeapon_t");
            Animator.SetBool("weaponDrawn_b", true);
        }

        /// <summary>
        /// Triggers the fire animation.
        /// </summary>
        public void FireWeapon()
        {
            Animator.SetTrigger("fire_t");
        }

        /// <summary>
        /// Triggers the holster weapon animation.
        /// </summary>
        public void HolsterWeapon()
        {
            Animator.SetTrigger("holsterWeapon_t");
            Animator.SetBool("weaponDrawn_b", false);
        }

        /// <summary>
        /// Triggers the melee attack animation.
        /// </summary>
        public void StartMeleeAttack()
        {
            Animator.SetTrigger("melee_t");
        }

        /// <summary>
        /// Triggers the vault animation.
        /// </summary>
        public void StartVault()
        {
            Animator.SetTrigger("vault_t");
        }

        /// <summary>
        /// Triggers the take damage routine.
        /// </summary>
        public void TakeDamage()
        {
            StartCoroutine(TakeDamageRoutine());
        }

        /// <summary>
        /// Triggers the take damage behaviour.
        /// </summary>
        public IEnumerator TakeDamageRoutine()
        {
            if (!isTakingDamage)
            {
                isTakingDamage = true;
                int damageAnimIndex = Random.Range(0, 4);
                Animator.SetTrigger("takeDamage_t" + damageAnimIndex);
                yield return new WaitForSeconds(0.1f);
                isTakingDamage = false;
            }

        }

        /// <summary>
        /// Updates the speed paramater of the animatior.
        /// </summary>
        public void UpdateSpeed(float newSpeed)
        {
            if (newSpeed > 0)
            {
                if (Animator.GetBool("crouching_b"))
                {
                    Animator.SetBool("crouching_b", false);
                }
            }
            Animator.SetFloat("speed_f", newSpeed);
        }

        /// <summary>
        /// Changes the booleans in the animator to reflect which type of 
        /// weapon is currently equipped.
        /// </summary>
        public void UpdateWeaponType(WeaponClass weaponClass)
        {
            if (weaponClass == WeaponClass.Rifle ||
                weaponClass == WeaponClass.LongRifle)
            {
                Animator.SetBool("handgunEquipped_b", false);
                Animator.SetBool("rifleEquipped_b", true);
            }
            if (weaponClass == WeaponClass.Handgun)
            {
                Animator.SetBool("rifleEquipped_b", false);
                Animator.SetBool("handgunEquipped_b", true);
            }
        }
    }
}
