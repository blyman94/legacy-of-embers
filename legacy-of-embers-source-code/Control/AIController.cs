using Game.Combat;
using Game.Core;
using Game.Entity;
using Game.Inventories;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Control
{
    /// <summary>
    /// Works with the AIDecisionManager singleton to dictate the behaviour of
    /// AI during enemy turns.
    /// </summary>
    public class AIController : MonoBehaviour
    {
        /// <summary>
        /// Determines if debug logs will be printed during execution.
        /// </summary>
        public bool DebugMode = false;

        /// <summary>
        /// Actor the controller is currently controlling.
        /// </summary>
        public Actor ActorToControl { get; set; }

        /// <summary>
        /// Determines if this is the first decision the AI controller is making
        /// this turn.
        /// </summary>
        private bool firstDecision;

        /// <summary>
        /// Starts this controllers turn.
        /// </summary>
        public void StartTurn()
        {
            ActorToControl.StartTurn();
            firstDecision = true;
            StartCoroutine(MakeNextMove());
        }

        /// <summary>
        /// AI Action to get in range of the player and attack with its primary
        /// weapon.
        /// </summary>
        private IEnumerator AttackPlayer()
        {
            if (DebugMode)
            {
                Debug.Log("Attacking Player...");
            }
            Actor player = EntityManager.Instance.PlayerActor;

            Vector3 playerPos = player.gameObject.transform.position;

            float sqrDistanceFromPlayer =
                (ActorToControl.transform.position - playerPos).sqrMagnitude;

            yield return ActorToControl.Mover.GetInRange(playerPos,
                    ActorToControl.Fighter.CurrentWeapon.AttackRange);
            if (ActorToControl.Mover.MoveTime <= 0)
            {
                if (DebugMode)
                {
                    Debug.Log("Couldn't get in range!");
                }
                StartCoroutine(MakeNextMove());
                yield break;
            }

            ICombatTarget playerTarget = (ICombatTarget)player.Fighter;

            while (ActorToControl.Fighter.Attacks > 0 && player.Fighter.IsAlive)
            {
                yield return ActorToControl.Fighter.Attack(playerTarget);
                yield return new WaitForSeconds(0.5f);
            }

            StartCoroutine(MakeNextMove());
        }

        /// <summary>
        /// AI Action to get in to range of a player's allies and attack them.
        /// Warrior Bots are prioritized as they are the most dangerous, 
        /// followed by Collector Bots and finally Defender Bots.
        /// </summary>
        private IEnumerator AttackPlayerAllies()
        {
            if (DebugMode)
            {
                Debug.Log("Attacking Player Allies...");
            }

            List<ActorSubtype> allyTypes =
                EntityManager.Instance.GetAllyBotTypes();

            Actor allyToAttack = null;

            float sqrMaxTravelDistance =
                ActorToControl.Mover.GetSqrMaxTravelDistance();

            float sqrAttackRange =
                ActorToControl.Fighter.GetSqrAttackRange();

            // Does this come from the ActorBrain???
            ActorSubtype[] priorityList =
                new ActorSubtype[3] {ActorSubtype.Warrior,
                ActorSubtype.Collector, ActorSubtype.Defender};

            foreach (ActorSubtype subtype in priorityList)
            {
                if (allyTypes.Contains(subtype))
                {
                    List<Actor> alliesOfSubtype =
                        EntityManager.Instance.PlayerAllies.Where(x => x.ActorId.ActorSubtype == subtype).ToList();
                    foreach (Actor ally in alliesOfSubtype)
                    {
                        float sqrDistanceFromAlly =
                            (ActorToControl.transform.position - ally.transform.position).sqrMagnitude;
                        if ((sqrMaxTravelDistance + sqrAttackRange) >= sqrDistanceFromAlly)
                        {
                            allyToAttack = ally;
                            break;
                        }
                    }
                    break;
                }
            }

            yield return ActorToControl.Mover.GetInRange(allyToAttack.transform.position,
                ActorToControl.Fighter.CurrentWeapon.AttackRange);

            ICombatTarget allyTarget = (ICombatTarget)allyToAttack.Fighter;
            while (ActorToControl.Fighter.Attacks > 0 && allyToAttack.Fighter.IsAlive)
            {
                int currentAttacks = ActorToControl.Fighter.Attacks;
                yield return ActorToControl.Fighter.Attack(allyTarget);
                if (ActorToControl.Fighter.Attacks == currentAttacks)
                {
                    if (DebugMode)
                    {
                        Debug.Log("Got stuck.");
                    }
                    yield return EndTurn();
                    yield break;
                }
                yield return new WaitForSeconds(0.5f);
            }
            if (ActorToControl.Fighter.Attacks == 0 && allyToAttack.Fighter.IsAlive)
            {
                if (DebugMode)
                {
                    Debug.Log("Failed to kill ally.");
                }

                StartCoroutine(MakeNextMove());
                yield break;
            }
            else
            {
                StartCoroutine(MakeNextMove());
                yield break;
            }
        }

        /// <summary>
        /// AI Action to build a robot. An arbitrary priority list is hardcoded
        /// for demonstration purposes.
        /// </summary>
        private IEnumerator BuildBot()
        {
            if (DebugMode)
            {
                Debug.Log("Building Bot...");
            }

            // To be gathered from the Actor stats later on.
            ActorSubtype[] priorityList =
                new ActorSubtype[3] {ActorSubtype.Warrior,
                ActorSubtype.Collector, ActorSubtype.Defender};

            int[] resourceCounts =
                ActorToControl.Builder.Inventory.ResourceCounts;

            GameObject builtBot = null;

            foreach (ActorSubtype subtype in priorityList)
            {
                if (BuildingManager.Instance.BuildBotCheck(subtype,
                    resourceCounts))
                {
                    builtBot = ActorToControl.Builder.BuildBot(subtype);
                    StartCoroutine(MakeNextMove());
                    yield break;
                }
            }
        }

        /// <summary>
        /// AI Action to prompt the TurnManager to move to the next turn.
        /// </summary>
        private IEnumerator EndTurn()
        {
            if (DebugMode)
            {
                Debug.Log("Ending Turn...");
            }
            yield return null;
            TurnManager.Instance.MoveToNextTurn();
        }

        /// <summary>
        /// AI Action to attack crates and collect resources within, or to go
        /// after already exposed resources.
        /// </summary>
        private IEnumerator GatherResources()
        {
            if (DebugMode)
            {
                Debug.Log("Gathering Resources...");
            }

            int startingResourceCount;
            if (ActorToControl.Builder != null)
            {
                startingResourceCount =
                    ActorToControl.Builder.Inventory.GetTotalResourceCount();
            }
            else
            {
                Inventory inventory = ActorToControl.gameObject.GetComponent<Inventory>();
                startingResourceCount = inventory.GetTotalResourceCount();
            }

            int currentResourceCount;

            // Grab exposed pickups
            if (CrateManager.Instance.ExposedPickups.Count > 0)
            {
                GameObject closestExposedPickup =
                CrateManager.Instance.GetClosestExposedPickup(ActorToControl.transform.position);
                Vector3 closestExposePickupPos = closestExposedPickup.transform.position;

                yield return ActorToControl.Mover.MoveRoutine(closestExposePickupPos);

                currentResourceCount = ActorToControl.Builder.Inventory.GetTotalResourceCount();
                if (currentResourceCount != startingResourceCount)
                {
                    if (DebugMode)
                    {
                        Debug.Log("Components successfully gathered.");
                    }
                    StartCoroutine(MakeNextMove());
                    yield break;
                }
            }

            // Break create
            GameObject closestCrate =
                CrateManager.Instance.GetClosestCrate(ActorToControl.gameObject.transform.position);
            Vector3 closestCratePos = closestCrate.transform.position;

            yield return ActorToControl.Mover.GetInRange(closestCrate.transform.position,
                ActorToControl.Fighter.CurrentWeapon.AttackRange);

            if (!ActorToControl.Mover.CanMove)
            {
                if (DebugMode)
                {
                    Debug.Log("Failed to get in range.");
                }
                StartCoroutine(MakeNextMove());
                yield break;
            }

            ICombatTarget crateTarget = (ICombatTarget)closestCrate.GetComponent<Crate>();
            while (ActorToControl.Fighter.Attacks > 0 && closestCrate.activeInHierarchy)
            {
                yield return ActorToControl.Fighter.Attack(crateTarget);
            }
            if (ActorToControl.Fighter.Attacks == 0 && closestCrate.activeInHierarchy)
            {
                if (DebugMode)
                {
                    Debug.Log("Failed to break crate.");
                }
                StartCoroutine(MakeNextMove());
                yield break;
            }

            currentResourceCount = ActorToControl.Builder.Inventory.GetTotalResourceCount();
            if (currentResourceCount != startingResourceCount)
            {
                if (DebugMode)
                {
                    Debug.Log("Components successfully gathered.");
                }
                StartCoroutine(MakeNextMove());
                yield break;
            }

            yield return ActorToControl.Mover.MoveRoutine(closestCratePos);

            currentResourceCount = ActorToControl.Builder.Inventory.GetTotalResourceCount();
            if (currentResourceCount != startingResourceCount)
            {
                if (DebugMode)
                {
                    Debug.Log("Components successfully gathered.");
                }
                StartCoroutine(MakeNextMove());
                yield break;
            }
            else
            {
                if (DebugMode)
                {
                    Debug.Log("Failed to pick up components.");
                }
                StartCoroutine(MakeNextMove());
                yield break;
            }
        }

        /// <summary>
        /// Prompts the AIDecisionManager for an action, then starts the 
        /// coroutine representing that action.
        /// </summary>
        private IEnumerator MakeNextMove()
        {
            if (firstDecision)
            {
                yield return new WaitForSeconds(2.0f);
                firstDecision = false;
            }
            AIAction nextAction =
                AIDecisionManager.Instance.DecideNextAction(ActorToControl);
            switch (nextAction)
            {
                case AIAction.AttackPlayer:
                    yield return AttackPlayer();
                    break;
                case AIAction.AttackPlayerAllies:
                    yield return AttackPlayerAllies();
                    break;
                case AIAction.BuildBot:
                    yield return BuildBot();
                    break;
                case AIAction.GatherResources:
                    yield return GatherResources();
                    break;
                case AIAction.TakeCover:
                    yield return TakeCover();
                    break;
                case AIAction.EndTurn:
                    yield return EndTurn();
                    break;
                default:
                    Debug.Log("{{AIController.cs}} Unrecognized AIAction " +
                        "passed to MakeNextMove.");
                    yield return EndTurn();
                    break;
            }
        }

        /// <summary>
        /// AI Action to find the nearest "cover point" on the map that puts a
        /// cover object between the actor and the player.
        /// </summary>
        private IEnumerator TakeCover()
        {
            if (DebugMode)
            {
                Debug.Log("Taking Cover...");
            }

            Actor playerActor = EntityManager.Instance.PlayerActor;

            Vector3 playerFirePos = playerActor.Fighter.HandgunSlot.position;

            GameObject[] coverGameObjects =
                CoverManager.Instance.GetClosestCoverObjectOrder(ActorToControl.transform.position);

            GameObject coverObjectSelected =
                coverGameObjects[Random.Range(0, coverGameObjects.Length)];

            Vector3[] destinations =
                coverObjectSelected.GetComponent<CoverObject>().OrderPointsByDistance(Vector3.zero);

            Vector3 destination =
                destinations[Random.Range(0, destinations.Length)];

            yield return
                ActorToControl.Mover.MoveRoutine(destination);

            if (ActorToControl.Mover.MoveTime <= 0)
            {
                if (DebugMode)
                {
                    Debug.Log("Couldn't make it to cover!");
                }
                StartCoroutine(MakeNextMove());
                yield break;
            }

            StartCoroutine(MakeNextMove());
        }
    }
}
