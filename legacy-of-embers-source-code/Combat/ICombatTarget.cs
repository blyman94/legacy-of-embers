using Game.Combat.Reserves;
using Game.Core;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Defines a contract for a class to be considered an attackable object. 
    /// All combat targets must have health, and be able to take damage and die.
    /// </summary>
    public interface ICombatTarget
    {
        /// <summary>
        /// IReserve representing the Combat Target's health.
        /// </summary>
        IReserve HealthReserve { get; }

        /// <summary>
        /// Alignment of the combat target. A combat target cannot be attacked 
        /// by a fighter it is aligned with.
        /// </summary>
        Alignment Alignment { get; }

        /// <summary>
        /// Property to retrieve the transform of the combat target.
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Transform used to determine whether the target is in cover or not.
        /// </summary>
        Transform TargetPoint { get; }

        /// <summary>
        /// Reduces the target's health by the specified damage amount.
        /// </summary>
        /// <param name="damage">Amount by which to reduce health.</param>
        /// <param name="critical">Whether or not damage was dealt by a critical
        /// hit.</param>
        void TakeDamage(int damage, bool critical);

        /// <summary>
        /// A method called when the targets's health is dropped to 0.
        /// </summary>
        void Die();
    }
}
