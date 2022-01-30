using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Actor identifier.
    /// </summary>
    [CreateAssetMenu(fileName = "new ActorIDObject",
        menuName = "Data.../ActorIDObject")]
    public class ActorIDObject : ScriptableObject
    {
        [Tooltip("Unique ID number for the actor.")]
        public int IdNumber;

        [Tooltip("In game name of the actor.")]
        public string ActorName;

        [Tooltip("In game description of the actor.")]
        public string ActorDescription;

        [Tooltip("Type of actor.")]
        public ActorType ActorType;

        [Tooltip("Subtype of Actor")]
        public ActorSubtype ActorSubtype;

        [Tooltip("Alignment of actor. Passed to the actor's fighter " +
        "component and builder component (if applicable). Fighters of the " +
        "same alignment cannot attack one another.")]
        public Alignment Alignment;

        [Tooltip("Sprite to represent this actor in the Turn Display.")]
        public Sprite Thumbnail;

        /// <summary>
        /// Returns a formatted string representing the summary of the actor, 
        /// crafted from its various stats.
        /// </summary>
        public string GetActorSummary()
        {
            string actorSummary = "";

            actorSummary += "<b>" + ActorName + "</b> \n";

            actorSummary += "<i>" + ActorDescription + "</i> \n\n";

            string alignment = "";
            switch (Alignment)
            {
                case Alignment.Player:
                    alignment = "Alignment: Player";
                    break;
                case Alignment.Enemy:
                    alignment = "Alignment: Enemy";
                    break;
                default:
                    alignment = "Alignment: None";
                    break;
            }

            actorSummary += alignment;

            return actorSummary;
        }
    }
}
