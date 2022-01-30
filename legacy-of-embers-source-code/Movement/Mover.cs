using Game.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Movement
{
    /// <summary>
    /// Component that allows a GameObject to move using a NavMeshAgent.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class Mover : MonoBehaviour
    {
        public static event UpdateMoveTime UpdateMoveTime;
        public static event UpdateMoveState UpdateMoveState;
        public event UpdateSpeed UpdateSpeed;
        public event MovementFailed MovementFailed;

        /// <summary>
        /// Actor Id of the owning actor.
        /// </summary>
        public ActorIDObject ActorId { get; set; }

        /// <summary>
        /// Actor stats of the owning actor.
        /// </summary>
        public ActorStatsObject ActorStats { get; set; }

        /// <summary>
        /// NavMeshAgent to be moved by the MoveComponent.
        /// </summary>
        public NavMeshAgent NavMeshAgent { get; set; }

        /// <summary>
        /// Current move state of the Mover.
        /// </summary>
        public bool IsMoving { get; set; }

        /// <summary>
        /// The amount of time remaining in the actor's turn. Time only moves 
        /// when the actor is moving.
        /// </summary>
        public float MoveTime
        {
            get
            {
                return moveTime;
            }
            set
            {
                if (value > 0.0f)
                {
                    CanMove = true;
                }
                else
                {
                    NavMeshAgent.destination = transform.position;
                    CanMove = false;
                }
                UpdateMoveTime?.Invoke(ActorStats.MaxMoveTime,value);
                moveTime = value;
            }
        }

        /// <summary>
        /// Determines if the Mover is currently taking a turn.
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Determines if the Mover is currently able to move.
        /// </summary>
        public bool CanMove { get; set; }

        /// <summary>
        /// The amount of time remaining in the actor's turn. Time only moves 
        /// when the actor is moving.
        /// </summary>
        private float moveTime;

        private float speedLastFrame = 0.0f;

        private void Awake()
        {
            NavMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            NavMeshAgent.speed = ActorStats.MaxSpeedStanding;
        }

        private void OnDisable()
        {
            UpdateSpeed = null;
            MovementFailed = null;
        }

        private void Update()
        {
            if (isActive)
            {
                if (NavMeshAgent.velocity.sqrMagnitude == 0.0f ||
                    HasReachedDestination())
                {
                    if (IsMoving)
                    {
                        IsMoving = false;
                        UpdateMoveState?.Invoke(ActorId, true);
                    }
                }
                else
                {
                    if (!IsMoving)
                    {
                        IsMoving = true;
                        UpdateMoveState?.Invoke(ActorId, false);
                    }
                    MoveTime -= Time.deltaTime;
                }
            }
            if (speedLastFrame != NavMeshAgent.velocity.magnitude)
            {
                UpdateSpeed?.Invoke(NavMeshAgent.velocity.magnitude);
            }
            speedLastFrame = NavMeshAgent.velocity.magnitude;
        }

        /// <summary>
        /// Moves until it is within a given range of the target position.
        /// </summary>
        /// <param name="targetPos">Position to get in range of.</param>
        /// <param name="range">Range the object can be in.</param>
        public IEnumerator GetInRange(Vector3 targetPos, float range)
        {
            float sqrDistanceFromTarget =
                (targetPos - transform.position).sqrMagnitude;
            float sqrRange = range * range;
            if (sqrDistanceFromTarget < sqrRange)
            {
                yield break;
            }
            else
            {
                NavMeshAgent.destination = targetPos;
                while (sqrDistanceFromTarget > sqrRange && CanMove)
                {
                    sqrDistanceFromTarget =
                        (targetPos - transform.position).sqrMagnitude;
                    yield return null;
                }
                NavMeshAgent.destination = transform.position;
            }
        }

        /// <summary>
        /// Estimates the Maximum travel distance this Mover can move based on 
        /// its remaining move time. Accounts for acceleration time as well.
        /// </summary>
        /// <returns>float representing the squared max distance this Mover can
        /// currently travel.</returns>
        public float GetSqrMaxTravelDistance()
        {
            float speedUpTime = NavMeshAgent.speed / NavMeshAgent.acceleration;
            float maxTravelDistance =
                (NavMeshAgent.speed * moveTime) - speedUpTime;
            return (maxTravelDistance * maxTravelDistance);
        }

        /// <summary>
        /// Uses a NavMeshAgent to move the mover from its current position to 
        /// the destination position while monitoring motion using a coroutine.
        /// </summary>
        /// <param name="destination">Location at which the mover will stop.
        /// </param>
        public IEnumerator MoveRoutine(Vector3 destination)
        {
            NavMeshHit hit;
            NavMesh.SamplePosition(destination, out hit, 1.0f,
                NavMesh.AllAreas);
            Vector3 endPoint = hit.position;

            Vector3 previousPosition = transform.position;

            if (CanMove)
            {
                NavMeshAgent.destination = endPoint;
                while (CanMove && !HasReachedDestination())
                {
                    previousPosition = transform.position;
                    yield return new WaitForSeconds(0.1f);
                    if (previousPosition == transform.position)
                    {
                        NavMeshAgent.destination = transform.position;
                        yield break;
                    }
                }
                NavMeshAgent.destination = transform.position;
            }
            else
            {
                MovementFailed?.Invoke("Out of Move Time!");
                yield break;
            }
        }

        /// <summary>
        /// Uses a NavMeshAgent to move the mover from its current position to 
        /// the destination position.
        /// </summary>
        /// <param name="destination">Location at which the mover will stop.
        /// </param>
        public void MoveTo(Vector3 destination)
        {
            if (CanMove)
            {
                NavMeshAgent.destination = destination;
            }
            else
            {
                MovementFailed?.Invoke("Out of Move Time!");
                Debug.Log("Out of Move Time!");
            }
        }

        /// <summary>
        /// Performed at the end of the Mover's turn every turn it takes.
        /// </summary>
        public void EndTurn()
        {
            isActive = false;
        }

        /// <summary>
        /// Performed at the start of the Mover's turn each turn it takes.
        /// </summary>
        public void StartTurn()
        {
            MoveTime = ActorStats.MaxMoveTime;
            isActive = true;
        }

        /// <summary>
        /// Returns true if the NavMeshAgent reaches is destination point.
        /// </summary>
        /// <returns></returns>
        private bool HasReachedDestination()
        {
            if (!NavMeshAgent.pathPending)
            {
                if (NavMeshAgent.remainingDistance <=
                    NavMeshAgent.stoppingDistance)
                {
                    if (!NavMeshAgent.hasPath ||
                        NavMeshAgent.velocity.sqrMagnitude == 0f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
