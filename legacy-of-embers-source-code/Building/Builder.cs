using Game.Core;
using Game.Inventories;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Building
{
    /// <summary>
    /// Component that allows the actor to build bots during their turn.
    /// </summary>
    [RequireComponent(typeof(Inventory))]
    public class Builder : MonoBehaviour
    {
        public static event UpdateBuildCount UpdateBuildCount;
        public static event UpdateBuildAbility UpdateBuildAbility;
        public event BuildFailed BuildFailed;
        public event BuildStarted BuildStarted;

        /// <summary>
        /// Actor ID of the owning actor.
        /// </summary>
        public ActorIDObject ActorId { get; set; }

        /// <summary>
        /// Actor stats of the owning actor.
        /// </summary>
        public ActorStatsObject ActorStats { get; set; }

        /// <summary>
        /// Inventory from which this builder will draw parts to build bots.
        /// </summary>
        public Inventory Inventory { get; set; }

        /// <summary>
        /// Audio source component for builder-related audio.
        /// </summary>
        public AudioSource BuilderAudio { get; set; }

        /// <summary>
        /// Audio clip to be played when the Builder builds a bot.
        /// </summary>
        public AudioClip ConstructionClip { get; set; }

        /// <summary>
        /// Audio clip to be played when the builder builds a bot (alt).
        /// </summary>
        public AudioClip ConstructionClipAlt { get; set; }

        /// <summary>
        /// Determines which bots the builder currently has capacity to build 
        /// based on inventory contents.
        /// </summary>
        public bool[] BuildAbility
        {
            get
            {
                return buildAbility;
            }
            set
            {
                UpdateBuildAbility?.Invoke(ActorId, value);
                buildAbility = value;
            }
        }

        /// <summary>
        /// The current number of bots the actor has built.
        /// </summary>
        public int Builds
        {
            get
            {
                return builds;
            }
            set
            {
                UpdateBuildCount?.Invoke(value);
                builds = value;
            }
        }

        /// <summary>
        /// The current number of bots the actor has built.
        /// </summary>
        private int builds;

        /// <summary>
        /// List containing all bots built by the builder during this battle.
        /// </summary>
        private List<GameObject> builtBots;

        /// <summary>
        /// Determines which bots the builder currently has capacity to build 
        /// based on inventory contents.
        /// </summary>
        private bool[] buildAbility;

        private void Awake()
        {
            builtBots = new List<GameObject>();
            Inventory = GetComponent<Inventory>();
            buildAbility = new bool[3];
        }

        private void OnEnable()
        {
            Inventory.changedResourceCount += UpdateBuildAbilityArray;
        }

        private void OnDisable()
        {
            BuildStarted = null;
            BuildFailed = null;
        }

        /// <summary>
        /// Builds the given bot type if the builder has room for it and has the 
        /// required components.
        /// </summary>
        public GameObject BuildBot(ActorSubtype botType)
        {
            if (BuildAbility[(int)botType])
            {
                if (Builds < ActorStats.MaxBuilds)
                {
                    BotRecipeObject recipe =
                        BuildingManager.Instance.Recipes[(int)botType];

                    GameObject botPrefab =
                        BuildingManager.Instance.GetBotPrefab(botType,
                        ActorId.Alignment);

                    if (botType == ActorSubtype.Collector)
                    {
                        Inventory collectorInventory =
                                botPrefab.GetComponent<Inventory>();
                        collectorInventory.ReferenceInventory = Inventory;
                    }

                    GameObject bot = Instantiate(botPrefab,
                        transform.position + transform.forward,
                        Quaternion.identity);
                    BuildStarted?.Invoke();
                    PlayConstructionClip();

                    Inventory.RemoveResource(recipe.resourceRequirements);
                    builtBots.Add(bot);
                    Builds = builtBots.Count;

                    return bot;
                }
                else
                {
                    BuildFailed?.Invoke("Build limit reached! (Max: " + ActorStats.MaxBuilds + ")");
                    Debug.Log("Build limit reached!");
                    return null;
                }
            }
            BuildFailed?.Invoke("Insufficient components for " + botType.ToString());
            Debug.Log("Insufficient components for " + botType.ToString());
            return null;
        }

        /// <summary>
        /// Updates availability of bots whenever inventory resource counts 
        /// change.
        /// </summary>
        /// <param name="resoruceId">UNUSED</param>
        /// <param name="count">UNUSED</param>
        public void UpdateBuildAbilityArray(int resoruceId, int count)
        {
            BuildAbility =
                BuildingManager.Instance.BuildAbilityCheck(Inventory.ResourceCounts);
        }

        /// <summary>
        /// Builder's behaviour at the start of each turn.
        /// </summary>
        public void StartTurn()
        {
            UpdateBuildAbility?.Invoke(ActorId, BuildAbility);
            UpdateBuildCount?.Invoke(Builds);
        }

        /// <summary>
        /// Plays a random construction audio clip.
        /// </summary>
        private void PlayConstructionClip()
        {
            bool playBaseClip = Random.value > 0.5f;
            if (playBaseClip)
            {
                BuilderAudio.PlayOneShot(ConstructionClip, 1.0f);
            }
            else
            {
                BuilderAudio.PlayOneShot(ConstructionClipAlt, 1.0f);
            }
        }
    }
}
