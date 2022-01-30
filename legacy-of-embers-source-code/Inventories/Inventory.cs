using Game.Core;
using UnityEngine;

namespace Game.Inventories
{
    /// <summary>
    /// Tracks resources gathered during a skirmish.
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        public event ChangedResourceCount changedResourceCount;

        /// <summary>
        /// Tracks the amount of each resource the player has gathered during
        /// a skirmish. Indexing is as follows:
        /// 
        ///     0 - Cyan Crystal,
        ///     1 - Magenta Crystal
        ///     2 - Yellow Crystal,
        ///     3 - Steel,
        ///     4 - Wood
        ///     
        /// </summary>
        [HideInInspector]
        public int[] ResourceCounts;

        /// <summary>
        /// A reference to another inventory that is not on the current actor. 
        /// This allows Collector bots to act as delegates and give their 
        /// resources directly to the actor that built them.
        /// </summary>
        [HideInInspector]
        public Inventory ReferenceInventory;

        private void Awake()
        {
            ResourceCounts = new int[5];
        }

        private void OnDisable()
        {
            changedResourceCount = null;
        }

        /// <summary>
        /// Adds a specified count of a component to the resource pool.
        /// </summary>
        /// <param name="resourceId">Id number of the component being
        /// added.</param>
        /// <param name="count">count of the component being added.</param>
        public void AddResource(int resourceId, int count)
        {
            if (ReferenceInventory != null)
            {
                ReferenceInventory.AddResource(resourceId, count);
            }
            else
            {
                ResourceCounts[resourceId] += count;
                changedResourceCount?.Invoke(resourceId,
                    ResourceCounts[resourceId]);
            }

        }

        /// <summary>
        /// Adds a single component to the resource pool.
        /// </summary>
        /// <param name="resourceId">Id number of the component being
        /// added.</param>
        public void AddResource(int resourceId)
        {
            if (ReferenceInventory != null)
            {
                ReferenceInventory.AddResource(resourceId, 1);
            }
            else
            {
                AddResource(resourceId, 1);
            }
        }

        /// <summary>
        /// Returns the total count of resources in the inventory.
        /// </summary>
        /// <returns>Total count of resources in the inventory.</returns>
        public int GetTotalResourceCount()
        {
            int sum = 0;
            foreach (int count in ResourceCounts)
            {
                sum += count;
            }
            return sum;
        }

        /// <summary>
        /// Removes a specified count of a resource from the resource pool.
        /// </summary>
        /// <param name="resourceId">Id number of the resource being
        /// removed.</param>
        /// <param name="count">count of the resource being removed.</param>
        public void RemoveResource(int resourceId, int count)
        {
            if (ReferenceInventory != null)
            {
                ReferenceInventory.RemoveResource(resourceId, count);
            }
            else
            {
                if (ResourceCounts[resourceId] - count >= 0)
                {
                    ResourceCounts[resourceId] -= count;
                }
                else
                {
                    ResourceCounts[resourceId] = 0;
                }
                changedResourceCount?.Invoke(resourceId,
                    ResourceCounts[resourceId]);
            }
        }

        /// <summary>
        /// Removes a single resource to the resource pool.
        /// </summary>
        /// <param name="resourceId">Id number of the resource being
        /// removed.</param>
        public void RemoveResource(int resourceId)
        {
            if (ReferenceInventory != null)
            {
                ReferenceInventory.RemoveResource(resourceId);
            }
            else
            {
                RemoveResource(resourceId, 1);
            }
        }

        /// <summary>
        /// Removes a set of resource counts from the resource pool.
        /// </summary>
        /// <param name="resourcesUsed">The set of resources that have been
        /// consumed.</param>
        public void RemoveResource(int[] resourcesUsed)
        {
            if (ReferenceInventory != null)
            {
                ReferenceInventory.RemoveResource(resourcesUsed);
            }
            else
            {
                for (int i = 0; i < ResourceCounts.Length; i++)
                {
                    ResourceCounts[i] -= resourcesUsed[i];
                    changedResourceCount?.Invoke(i, ResourceCounts[i]);
                }
            }
        }
    }
}
