using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Set of stats that govern the actor's influence on the game.
    /// </summary>
    [CreateAssetMenu(fileName = "new ActorStatsObject",
        menuName = "Data.../ActorStatsObject")]
    public class ActorStatsObject : ScriptableObject
    {
        [Header("Turn Limits")]

        [Tooltip("Amount of time the actor has to move on a given turn. Time " +
        "only decreases when the actor is in motion.")]
        public float MaxMoveTime;

        [Tooltip("The maximum amount of times per turn an actor can attack.")]
        public int MaxAttacksPerTurn;

        [Tooltip("The maximum number of bots the actor can build at a time.")]
        public int MaxBuilds;

        [Header("Health")]
        [Tooltip("How much health the actor has.")]
        public int MaxHealth;

        [Header("Movement")]
        [Tooltip("How quickly the actor moves.")]
        public float MaxSpeedStanding;
    }
}

