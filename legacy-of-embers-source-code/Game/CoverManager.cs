using Game.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Can be queried for useful information about the cover objects.
    /// </summary>
    public class CoverManager : Singleton<CoverManager>
    {
        /// <summary>
        /// Object pool to hold Cover objects.
        /// </summary>
        public List<GameObject> coverPool;

        /// <summary>
        /// Returns true if there is cover between the two given positions.
        /// </summary>
        /// <param name="targetPosition">Position of the target actor that is 
        /// potentially covered.</param>
        public bool CheckCover(Vector3 origin, Vector3 endPoint)
        {
            LayerMask mask = LayerMask.GetMask("Cover");

            Debug.DrawLine(origin, endPoint);
            return Physics.Linecast(origin, endPoint, mask);
        }

        /// <summary>
        /// Returns an array of available CoverObjects sorted by distance from
        /// the comparePos.
        /// </summary>
        /// <param name="comparePos">Position to compare distances to.</param>
        public GameObject[] GetClosestCoverObjectOrder(Vector3 comparePos)
        {
            return coverPool.OrderBy(x =>
                (x.transform.position - comparePos).sqrMagnitude).ToArray();
        }
    }
}

