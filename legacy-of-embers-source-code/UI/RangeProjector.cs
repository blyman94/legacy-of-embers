using Game.Combat;
using Game.Core;
using Game.Entity;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Indicator that shows how far an actor's weapon reaches without an 
    /// accuracy penalty.
    /// </summary>
    public class RangeProjector : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Type of indicator (melee or ranged)")]
        private ProjectorType type;

        [SerializeField]
        [Tooltip("The fighter component whose range this projector is " +
            "indicating.")]
        private Fighter fighter;

        /// <summary>
        /// Projector component to indicate range.
        /// </summary>
        private Projector projector;

        private void Awake()
        {
            projector = GetComponent<Projector>();
        }

        private void OnEnable()
        {
            TurnManager.Instance.TurnSequenceUpdated += ToggleProjector;
            Fighter.WeaponSwitched += UpdateAttackRange;
        }

        private void OnDisable()
        {
            Fighter.WeaponSwitched -= UpdateAttackRange;
        }

        /// <summary>
        /// Enables/Disables the projector based on whether it is the actor's 
        /// current turn.
        /// </summary>
        private void ToggleProjector()
        {
            Actor currentActor = TurnManager.Instance.TurnQueue[0];
            if (currentActor.Fighter == fighter &&
                currentActor.ActorId.Alignment == Alignment.Player)
            {
                projector.enabled = true;
            }
            else
            {
                projector.enabled = false;
            }

            UpdateAttackRange();
        }

        /// <summary>
        /// Updates the orthographic size of the projector in response to 
        /// changes in attack range.
        /// </summary>
        private void UpdateAttackRange()
        {
            Actor currentActor = TurnManager.Instance.TurnQueue[0];
            if (type == ProjectorType.Melee)
            {
                projector.orthographicSize = fighter.MeleeWeapon.AttackRange;
            }
            else
            {
                if (fighter.CurrentWeapon != null)
                {
                    projector.orthographicSize = fighter.CurrentWeapon.AttackRange;
                }
                else
                {
                    projector.enabled = false;
                }
            }
        }
    }
}

