using Game.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Inventories
{
    /// <summary>
    /// Represents collectible components with which the player can build bots.
    /// </summary>
    public class Pickup : MonoBehaviour
    {
        public static event PickupGrabbed PickupGrabbed;

        public bool UseNavMesh = true;

        [Tooltip("The pickup will change behaviour depending on whether it " +
            "is in a crate when it is instatiated.")]
        public bool InCrate;

        [Tooltip("How long it takes for the pickup to move to its " +
            "destination when exiting a crate.")]
        public float MoveTime;

        /// <summary>
        /// Point to which the pickup will move when it is enabled by a crate
        /// breaking.
        /// </summary>
        [HideInInspector]
        public Vector3 TargetPoint;

        [SerializeField]
        [Tooltip("0 - Cyan Crystal, 1 - Yellow Crystal, 2 - Magenta Crystal, " +
            "3 - Steel, 4 - Wood")]
        private int resourceId;

        /// <summary>
        /// Speed at which the pickup moves toward an inventory magnet. Set by 
        /// the inventory magnet when the collider representing magnet influence
        /// collides with the pickup.
        /// </summary>
        [HideInInspector]
        public float PullSpeed;

        [SerializeField]
        [Tooltip("Number of this component added to the colliding inventory.")]
        private int stackSize;

        public void OnEnable()
        {
            if (InCrate)
            {
                StartCoroutine(MoveToTargetPoint());
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            Inventory inventory = other.gameObject.GetComponent<Inventory>();
            if (inventory != null)
            {
                inventory.AddResource(resourceId, stackSize);
                PickupGrabbed?.Invoke(gameObject);
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Method called to launch the pickup from a crate being destroyed.
        /// </summary>
        public IEnumerator MoveToTargetPoint()
        {
            Vector3 startPoint = transform.position;

            Vector3 endPoint;
            if (UseNavMesh)
            {
                NavMeshHit hit;
                NavMesh.SamplePosition(TargetPoint, out hit, 1.0f, NavMesh.AllAreas);
                endPoint = hit.position;
            }
            else
            {
                endPoint = TargetPoint;
            }


            float elapsedTime = 0.0f;

            while (elapsedTime < MoveTime)
            {
                transform.position =
                    Vector3.Lerp(startPoint, endPoint,
                    (elapsedTime / MoveTime));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}
