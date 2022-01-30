using System.Linq;
using UnityEngine;

namespace Game.Entity
{
    /// <summary>
    /// Static on the battlefield that actors can take cover behind to reduce
    /// incoming damage.
    /// </summary>
    public class CoverObject : MonoBehaviour
    {
        [Tooltip("Points around this object where an (AI) actor may " + 
            "take cover")]
        public Transform[] CoverPoints;

        /// <summary>
        /// Reorders the CoverPoints array by distance from the ComparePos, 
        /// returning the result.
        /// </summary>
        /// <param name="comparePos">Position for distance comparison.</param>
        /// <returns>Ordered array of CoverPoints (Vector3[]).</returns>
        public Vector3[] OrderPointsByDistance(Vector3 comparePos)
        {
            Vector3[] CoverPositions = new Vector3[CoverPoints.Length];
            for (int i = 0; i < CoverPoints.Length; i++)
            {
                CoverPositions[i] = CoverPoints[i].position;
            }

            Vector3[] OrderedPositions =
                CoverPositions.OrderBy(x => (x - comparePos).sqrMagnitude).ToArray();
            return OrderedPositions;
        }

        /// <summary>
        /// Calculates the closest position an (AI) actor (querying actor)can 
        /// move to where it will be in cover, based on an agressor position 
        /// (A weapon fire point) and its current position for comparison.
        /// </summary>
        /// <param name="aggressorPos">Weapon fire point of the actor that could
        /// attack the querying actor.</param>
        /// <param name="comparePos">Position of the querying actor for 
        /// distance comparison</param>
        /// <returns>(Vector3) the closest position the querying actor can move
        /// to and be considered in cover with respect to the aggressor pos.
        /// </returns>
        public Vector3 GetClosestInCoverPosition(Vector3 aggressorPos, Vector3 comparePos)
        {
            Vector3[] orderedPositions = OrderPointsByDistance(comparePos);

            for (int i = 0; i < orderedPositions.Length; i++)
            {
                bool willBeInCover = CoverManager.Instance.CheckCover(aggressorPos, orderedPositions[i]);
                if (willBeInCover)
                {
                    return orderedPositions[i];
                }
            }
            Debug.Log("No position on this cover object will grant cover " + 
                "for this actor.");
            return Vector3.negativeInfinity;
        }
    }
}

