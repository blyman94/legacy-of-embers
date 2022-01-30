using Game.Core;
using Game.Entity;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Stores recipes and prefabs for robot building. Handles aligning robots
    /// to their builder's alignment before the instatiation method is called 
    /// on those prefabs.
    /// </summary>
    public class BuildingManager : Singleton<BuildingManager>
    {
        [Header("Robot Recipes")]

        [Tooltip("Recipe for the Collector Bot.")]
        public BotRecipeObject CollectorBotRecipe;

        [Tooltip("Recipe for the Defender Bot.")]
        public BotRecipeObject DefenderBotRecipe;

        [Tooltip("Recipe for the Warrior Bot.")]
        public BotRecipeObject WarriorBotRecipe;

        [Header("Robot Prefabs - Ally")]

        [Tooltip("Prefab representing the collector bot.")]
        public GameObject CollectorBotPrefabAlly;

        [Tooltip("Prefab representing the defender bot.")]
        public GameObject DefenderBotPrefabAlly;

        [Tooltip("Prefab representing the warrior bot.")]
        public GameObject WarriorBotPrefabAlly;

        [Header("Robot Prefabs - Enemy")]
        [Tooltip("Prefab representing the collector bot.")]
        public GameObject CollectorBotPrefabEnemy;

        [Tooltip("Prefab representing the defender bot.")]
        public GameObject DefenderBotPrefabEnemy;

        [Tooltip("Prefab representing the warrior bot.")]
        public GameObject WarriorBotPrefabEnemy;

        /// <summary>
        /// List of Bot recipes.
        /// </summary>
        public List<BotRecipeObject> Recipes { get; set; }

        protected override void Awake()
        {
            base.Awake();
            Recipes = new List<BotRecipeObject>()
            {
                CollectorBotRecipe, DefenderBotRecipe, WarriorBotRecipe
            };
        }

        /// <summary>
        /// Checks which bots the passed inventory can build with its current 
        /// resource counts
        /// </summary>
        /// <param name="inventory">The subject inventory against which to check
        /// resource requirements.</param>
        /// <returns>builAbility - An array with the same length as recipes in the
        /// manager that represents which robots can be built with current
        /// resource counts.</returns>
        public bool[] BuildAbilityCheck(int[] resourceCounts)
        {
            bool[] buildAbility = new bool[Recipes.Count];

            for (int i = 0; i < Recipes.Count; i++)
            {
                buildAbility[i] =
                    Recipes[i].CanBuildBot(resourceCounts);
            }

            return buildAbility;
        }

        /// <summary>
        /// Returns true if the passed resourceCounts are sufficient to build 
        /// the passed botType;
        /// </summary>
        /// <param name="botType">Subject robot whose recipe will be compared
        /// to the passed resourceCounts to determine build ability.</param>
        /// <param name="resourceCounts">The resource counts with which the 
        /// botType is to be potentially built with.</param>
        public bool BuildBotCheck(ActorSubtype botType, int[] resourceCounts)
        {
            switch (botType)
            {
                case ActorSubtype.Collector:
                    return CollectorBotRecipe.CanBuildBot(resourceCounts);
                case ActorSubtype.Defender:
                    return DefenderBotRecipe.CanBuildBot(resourceCounts);
                case ActorSubtype.Warrior:
                    return WarriorBotRecipe.CanBuildBot(resourceCounts);
                case ActorSubtype.Default:
                    return false;
                default:
                    Debug.LogError("{{BuildingManager.cs}} Unrecognized " +
                        "ActorSubtype passed to BuildBotCheck.");
                    return false;
            }
        }

        /// <summary>
        /// Assigns an alignment to a bot prefab of the given type, then returns
        /// a GameObject representing a prefab of the requested bot type.
        /// </summary>
        /// <param name="type">Bot type requested to be built.</param>
        /// <param name="alignment">Alignment of the requestor.</param>
        /// <returns>Gameobject representing the newly aligned prefab of the
        /// requested bot type.</returns>
        public GameObject GetBotPrefab(ActorSubtype type, Alignment alignment)
        {
            GameObject botObject;

            switch (type)
            {
                case (ActorSubtype.Collector):
                    if (alignment == Alignment.Player)
                    {
                        botObject = CollectorBotPrefabAlly;
                    }
                    else
                    {
                        botObject = CollectorBotPrefabEnemy;
                    }
                    break;
                case (ActorSubtype.Defender):
                    if (alignment == Alignment.Player)
                    {
                        botObject = DefenderBotPrefabAlly;
                    }
                    else
                    {
                        botObject = DefenderBotPrefabEnemy;
                    }
                    break;
                case (ActorSubtype.Warrior):
                    if (alignment == Alignment.Player)
                    {
                        botObject = WarriorBotPrefabAlly;
                    }
                    else
                    {
                        botObject = WarriorBotPrefabEnemy;
                    }
                    break;
                default:
                    Debug.LogError("{{BuildingManager.cs}} Unrecognized  " +
                        "BotType passed to GetBotPrefab.");
                    botObject = null;
                    break;
            }

            return botObject;
        }
    }
}
