using UnityEngine;

namespace Game.Inventories
{
    /// <summary>
    /// A collider that pulls pickups toward it.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class InventoryMagnet : MonoBehaviour
    {
        [Header("Magnet Properties")]

        [Tooltip("Magnet's radius of influence")]
        public float PullRadius = 2.0f;

        [Tooltip("Speed of object moving toward magnet at maximum distance" +
            "of influence.")]
        public float pullStartSpeed = 1.0f;

        [Tooltip("Speed increase for every frame the object is influenced by" +
            "the magnet.")]
        public float pullSpeedDelta = 1.0f;

        public bool MagnetOn;

        /// <summary>
        /// Collider to represent the magnet's influence.
        /// </summary>
        public SphereCollider MagnetCollider { get; set; }

        private void OnEnable()
        {
            TurnManager.Instance.TurnSequenceUpdated += ToggleMagnet;
        }

        private void Start()
        {
            MagnetCollider = GetComponent<SphereCollider>();
            MagnetCollider.isTrigger = true;
            MagnetCollider.radius = PullRadius;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (MagnetOn)
            {
                if (other.transform.CompareTag("Pickup"))
                {
                    Pickup pickup = other.GetComponent<Pickup>();
                    pickup.PullSpeed = pullStartSpeed;
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (MagnetOn)
            {
                if (other.transform.CompareTag("Pickup"))
                {
                    Pickup pickup = other.GetComponent<Pickup>();
                    pickup.PullSpeed += pullSpeedDelta;
                    PullPickup(pickup);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (MagnetOn)
            {
                if (other.transform.CompareTag("Pickup"))
                {
                    Pickup pickup = other.GetComponent<Pickup>();
                    pickup.PullSpeed = 0;
                }
            }
        }

        /// <summary>
        /// Turns the magnet on an off.
        /// </summary>
        private void ToggleMagnet()
        {
            if (TurnManager.Instance.TurnQueue[0].InventoryMagnet == this)
            {
                MagnetOn = true;
            }
            else
            {
                MagnetOn = false;
            }
        }

        /// <summary>
        /// Uses Vector3.MoveTowards to simulate a pickup being drawn in by 
        /// a magnet.
        /// </summary>
        /// <param name="pickup">Pickup to be pulled by the magnet.</param>
        private void PullPickup(Pickup pickup)
        {
            pickup.transform.position =
                Vector3.MoveTowards(pickup.transform.position,
                transform.position, pickup.PullSpeed * Time.deltaTime);
        }
    }
}
