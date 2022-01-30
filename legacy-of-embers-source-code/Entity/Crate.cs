using Game.Combat;
using Game.Combat.Reserves;
using Game.Core;
using Game.Inventories;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Entity
{
    /// <summary>
    /// An attackable object containing resources (pickups) to build bots. Can 
    /// be attacked by any alignment and destroyed to reveal contents.
    /// </summary>
    [RequireComponent(typeof(HealthReserve))]
    public class Crate : MonoBehaviour, ICombatTarget
    {
        public static event RequestDamagePopup RequestDamagePopup;

        public static event CrateDestroyed CrateDestroyed;

        [Header("Crate Setup")]

        [Tooltip("Physical collider for the crate.")]
        public BoxCollider BoxCollider;

        [Tooltip("Maximum damage the crate can take before breaking.")]
        public int MaxHealth;

        [Tooltip("Health bar to be displayed above the crate when it is " +
            "first attacked.")]
        public GameObject HealthBarCanvas;

        [Header("Crate Graphic Models")]
        public GameObject CrateModel;
        public GameObject BrokenCrateModel;

        [Header("Contents")]

        [Tooltip("Minimum number of crystals that can come out of crates.")]
        public int MinCrystalsInCrate;

        [Tooltip("Maximum number of crystals that can come out of crates.")]
        public int MaxCrystalsInCrate;

        [Tooltip("Minimum number of pickups that can come out of crates.")]
        public int MinPickupsInCrate;

        [Tooltip("Maximum number of pickups that can come out of crates.")]
        public int MaxPickupsInCrate;

        [Tooltip("Prefab array of possible crystals contained in the crate.")]
        public GameObject[] CrystalPickupPrefabs;

        [Tooltip("Prefab array of possible other pickups contained in the " +
            "crate.")]
        public GameObject[] PickupPrefabs;

        [Header("Audio")]

        [Tooltip("AudioSource for crate noises.")]
        public AudioSource CrateAudioSource;
        public AudioClip CrateImpactClip;
        public AudioClip CrateBreakClip;
        public AudioClip CrateBreakClipAlt;
        public AudioClip CrateBreakClipAlt2;

        /// <summary>
        /// Transform to act as a parent to all pickups spawned.
        /// </summary>
        [HideInInspector]
        public Transform PickupParent;
        
        // ----- Interface properties ------------------------------------------

        public Alignment Alignment { get; private set; }

        public IReserve HealthReserve { get; private set; }

        public Transform Transform
        {
            get
            {
                return transform;
            }
        }

        public Transform TargetPoint
        {
            get
            {
                return targetPoint;
            }
        }

        // ---------------------------------------------------------------------

        [SerializeField]
        private Transform targetPoint;

        /// <summary>
        /// List of pickup GameObjects contained within the crate.
        /// </summary>
        private List<GameObject> pickupsToSpawn;

        /// <summary>
        /// Total count of all pickups contained in the crate.
        /// </summary>
        private int totalCount;

        private AudioClip[] crateBreakAudioClips;

        // ----- Monobehaviour Methods -----------------------------------------

        public void Awake()
        {
            HealthReserve = GetComponent<HealthReserve>();
            Alignment = Alignment.Default;
            pickupsToSpawn = new List<GameObject>();

            crateBreakAudioClips = new AudioClip[3]
            {
                CrateBreakClip, CrateBreakClipAlt, CrateBreakClipAlt2
            };
        }

        public void Start()
        {
            BrokenCrateModel.SetActive(false);
            HealthReserve.Max = MaxHealth;
            HealthBarCanvas.SetActive(false);
            LoadCrate();
        }

        public void OnEnable()
        {
            HealthReserve.ReserveEmpty += Die;
        }

        // ----- Crate Methods - Public ----------------------------------------

        /// <summary>
        /// "Loads" the crate full of objects by randomly selecting them based 
        /// on inspector parameters, instantiating them, and deactivating them 
        /// until the crate is destroyed.
        /// </summary>
        public void LoadCrate()
        {
            int crystalCount =
                Random.Range(MinCrystalsInCrate, MaxCrystalsInCrate);
            int pickupCount =
                Random.Range(MinPickupsInCrate, MaxPickupsInCrate);
            totalCount = crystalCount + pickupCount;

            for (int i = 0; i < crystalCount; i++)
            {
                Vector3 pickupTargetPosition =
                    transform.position + (Random.insideUnitSphere * 1.5f);
                pickupTargetPosition.y = 0;

                int crystalIndex = Random.Range(0, CrystalPickupPrefabs.Length);
                GameObject crystal =
                    Instantiate(CrystalPickupPrefabs[crystalIndex],
                    transform.position, Quaternion.identity, PickupParent);

                crystal.GetComponent<Pickup>().TargetPoint =
                    pickupTargetPosition;
                crystal.GetComponent<Pickup>().InCrate = true;

                pickupsToSpawn.Add(crystal);
                crystal.SetActive(false);
            }
            for (int i = 0; i < pickupCount; i++)
            {
                Vector3 pickupTargetPosition =
                    transform.position + (Random.insideUnitSphere * 1.5f);
                pickupTargetPosition.y = 0;

                int pickupIndex = Random.Range(0, PickupPrefabs.Length);
                GameObject pickup = Instantiate(PickupPrefabs[pickupIndex],
                    transform.position, Quaternion.identity, PickupParent);

                pickup.GetComponent<Pickup>().TargetPoint =
                    pickupTargetPosition;
                pickup.GetComponent<Pickup>().InCrate = true;

                pickupsToSpawn.Add(pickup);
                pickup.SetActive(false);
            }
        }

        // ----- Interface Methods ---------------------------------------------

        public void Die()
        {
            // Activate all contained objects and destroy (deactivate) the 
            // crate.
            BrokenCrateModel.SetActive(true);
            CrateModel.SetActive(false);
            HealthBarCanvas.SetActive(false);
            BoxCollider.enabled = false;
            for (int i = 0; i < totalCount; i++)
            {
                pickupsToSpawn[i].SetActive(true);
                if (CrateManager.Instance != null)
                {
                    CrateManager.Instance.ExposedPickups.Add(pickupsToSpawn[i]);
                }
            }
            PlayCrateBreakClip();
            CrateDestroyed?.Invoke();
            CrateManager.Instance.RemoveCrate(gameObject);
        }

        public void TakeDamage(int damage, bool critical)
        {
            RequestDamagePopup?.Invoke(transform.position, damage, critical);
            if (HealthReserve.Current == HealthReserve.Max &&
                damage > 0)
            {
                HealthBarCanvas.SetActive(true);
            }
            if (damage > 0)
            {
                PlayCrateImpactClip();
            }
            HealthReserve.Modify(-damage);
        }

        // ----- Crate Methods - Private ---------------------------------------

        private void PlayCrateBreakClip()
        {
            CrateAudioSource.pitch = Random.Range(0.95f, 1.05f);
            AudioClip randomCrateBreakClip =
                crateBreakAudioClips[Random.Range(0, crateBreakAudioClips.Length)];
            CrateAudioSource.PlayOneShot(randomCrateBreakClip, 1.0f);
        }

        private void PlayCrateImpactClip()
        {
            CrateAudioSource.pitch = Random.Range(1.0f, 1.3f);
            CrateAudioSource.PlayOneShot(CrateImpactClip, 1.5f);
        }
    }
}
