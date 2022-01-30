using Game.Core;
using Game.Inventories;
using Game.Entity;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Spawns crates randomly at the beginning of a battle.
    /// </summary>
    public class CrateManager : Singleton<CrateManager>
    {
        [Header("Crate Prefab")]

        [Tooltip("GameObject to represent a crate containing components.")]
        public GameObject CratePrefab;

        [Header("Spawn Data")]

        [Tooltip("Transform to act as a parent for the crates being created.")]
        [SerializeField]
        private Transform CrateParent;

        [Tooltip("Transform to act as a parent for the pickups spawned when " +
            "a crate breaks.")]
        [SerializeField]
        private Transform PickupParent;

        [Tooltip("Total number of crates to spawn at the beginning of combat.")]
        public int CrateCount;

        [Tooltip("Possible locations for crates to spawn.")]
        public List<Transform> CrateSpawnPoints;

        public List<GameObject> ExposedPickups { get; set; }

        /// <summary>
        /// Object pool to hold crates after instantiation.
        /// </summary>
        private List<GameObject> cratePool;

        protected override void Awake()
        {
            base.Awake();
            ExposedPickups = new List<GameObject>();
            InitializeCrates();
        }

        private void OnEnable()
        {
            Crate.CrateDestroyed += UpdateActiveCrates;
            Pickup.PickupGrabbed += UpdateExposedPickups;
        }

        private void Start()
        {
            SpawnCrates();
        }

        private void OnDisable()
        {
            Crate.CrateDestroyed -= UpdateActiveCrates;
        }

        /// <summary>
        /// Returns the GameObject representing the crate closest to the compare
        /// pos.
        /// </summary>
        /// <param name="comparePos">Position to compare distance to.</param>
        /// <returns>Gameobject representing the closest crate.</returns>
        public GameObject GetClosestCrate(Vector3 comparePos)
        {
            float shortestDistance = Mathf.Infinity;
            int closestCrateIndex = -1;
            if (cratePool.Count == 0)
            {
                return null;
            }
            for (int i = 0; i < cratePool.Count; i++)
            {
                float sqrDistance =
                    (cratePool[i].transform.position - comparePos).sqrMagnitude;
                if (sqrDistance < shortestDistance)
                {
                    shortestDistance = sqrDistance;
                    closestCrateIndex = i;
                }
            }
            return cratePool[closestCrateIndex];
        }

        /// <summary>
        /// Returns the GameObject representing the closest exposed (out of 
        /// crate) pickup to the comparePos.
        /// </summary>
        /// <param name="comparePos">Position to compare distance to.</param>
        /// <returns>GameObject representing the closest pickup.</returns>
        public GameObject GetClosestExposedPickup(Vector3 comparePos)
        {
            float shortestDistance = Mathf.Infinity;
            int closestPickupIndex = -1;
            if (ExposedPickups.Count == 0)
            {
                return null;
            }
            for (int i = 0; i < ExposedPickups.Count; i++)
            {
                float sqrDistance =
                    (ExposedPickups[i].transform.position - comparePos).sqrMagnitude;
                if (sqrDistance < shortestDistance)
                {
                    shortestDistance = sqrDistance;
                    closestPickupIndex = i;
                }
            }
            return ExposedPickups[closestPickupIndex];
        }

        /// <summary>
        /// Instantiates all crates to be spawned randomly and stores them in an
        /// object pool.
        /// </summary>
        private void InitializeCrates()
        {
            cratePool = new List<GameObject>();
            for (int i = 0; i < CrateCount; i++)
            {
                GameObject crateObject = Instantiate(CratePrefab, Vector3.zero,
                    Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f),
                    CrateParent);
                crateObject.GetComponent<Crate>().PickupParent = PickupParent;
                crateObject.SetActive(false);
                cratePool.Add(crateObject);
            }
        }

        /// <summary>
        /// Selects random positions for the crates from the given CrateSpawnPoints
        /// array and sets activates their GameObjects.
        /// </summary>
        private void SpawnCrates()
        {
            List<Transform> unusedSpawnPoints = CrateSpawnPoints;
            for (int i = 0; i < CrateCount; i++)
            {
                int spawnPointIndex = Random.Range(0, unusedSpawnPoints.Count);
                cratePool[i].transform.position =
                    unusedSpawnPoints[spawnPointIndex].transform.position;
                cratePool[i].SetActive(true);
                unusedSpawnPoints.RemoveAt(spawnPointIndex);
            }
        }

        /// <summary>
        /// Loops through the crate pool and determines which ones are still
        /// active. Removes inactive crates.
        /// </summary>
        private void UpdateActiveCrates()
        {
            for (int i = 0; i < cratePool.Count; i++)
            {
                if (!cratePool[i].activeInHierarchy)
                {
                    cratePool.RemoveAt(i);
                    CrateCount--;
                }
            }
        }

        /// <summary>
        /// Removes a crate from the crate pool.
        /// </summary>
        /// <param name="crateToRemove">Crate to be removed from the 
        /// pool.</param>
        public void RemoveCrate(GameObject crateToRemove)
        {
            cratePool.Remove(crateToRemove);
        }

        /// <summary>
        /// Removes a pickup from the ExposedPickups list when it is gathered.
        /// </summary>
        /// <param name="pickupGrabbed">The GameObject representing the pickup
        /// that has been... well, picked up.</param>
        private void UpdateExposedPickups(GameObject pickupGrabbed)
        {
            ExposedPickups.Remove(pickupGrabbed);
        }
    }
}
