using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Dictates which components are required to build a specific robot.
    /// </summary>
    [CreateAssetMenu(fileName = "new BotRecipe",
        menuName = "Inventory.../BotRecipe")]
    public class BotRecipeObject : ScriptableObject
    {
        [Tooltip("Type of bot this recipe makes.")]
        public ActorSubtype BotType;

        [Header("Requirements")]

        [Tooltip("Number of Cyan Crystals required to make the bot.")]
        public int CyanCrystals;

        [Tooltip("Number of Magenta Crystals required to make the bot.")]
        public int MagentaCrystals;

        [Tooltip("Number of Yellow Crystals required to make the bot.")]
        public int YellowCrystals;

        [Tooltip("Number of Steels required to make the bot.")]
        public int Steels;

        [Tooltip("Number of Woods required to make the bot.")]
        public int Woods;

        [HideInInspector]
        public int[] resourceRequirements;

        public void OnEnable()
        {
            resourceRequirements = new int[5] { CyanCrystals, MagentaCrystals,
            YellowCrystals, Steels, Woods };
        }

        /// <summary>
        /// Compares components available to components required and returns a
        /// boolean signaling whether the robot can be built with current
        /// component counts.
        /// </summary>
        /// <param name="inventory">Inventory of the entity trying to build a 
        /// robot.</param>
        /// <returns>True if robot can be built, false otherwise.</returns>
        public bool CanBuildBot(int[] resourceCounts)
        {
            for (int i = 0; i < resourceRequirements.Length; i++)
            {
                if (resourceRequirements[i] > resourceCounts[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
