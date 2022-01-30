using Game.Core;
using Game.Entity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Decision-making engine for AI. "Considers" every possible action the 
    /// AI can take by evaluating the Actor's current resources (move time 
    /// remaining, attacks, etc.). Adds actions that are achievable with current
    /// resources to a list of Available actions, then randomly chooses among
    /// them. Ends the actor's turn if there are no available actions after
    /// evaluation.
    /// </summary>
    public class AIDecisionManager : Singleton<AIDecisionManager>
    {
        [Tooltip("Debug Log statements will be printed in debug mode.")]
        public bool DebugMode = false;

        /// <summary>
        /// List of available actions to decide among. This is cleared and 
        /// repopulated every time the "DecideNextAction" method is called.
        /// </summary>
        public List<AIAction> AvailableActions { get; set; }

        private void Start()
        {
            AvailableActions = new List<AIAction>();
        }

        /// <summary>
        /// Considers many factors about the actor's current state, like number
        /// of attacks left, position relative to the player and its allies, 
        /// remaining move time, etc. to generate a list of actions it could 
        /// possibly take. Then, (for now), randomly chooses an action from the 
        /// list of available actions. Will return the "End Turn" action if the
        /// list of available actions is empty.
        /// </summary>
        /// <param name="actor">Actor for which to decide next action</param>
        /// <returns>(AIAction) decision</returns>
        public AIAction DecideNextAction(Actor actor)
        {
            if (DebugMode)
            {
                Debug.Log("Deciding Next Action...");
            }
            
            AvailableActions.Clear();

            Vector3 playerPos =
                EntityManager.Instance.PlayerActor.gameObject.transform.position;

            ConsiderAttackPlayerAction(actor, playerPos);
            ConsiderAttackPlayerAlliesAction(actor);
            if (actor.Builder != null)
            {
                ConsiderBuildBotAction(actor);
                ConsiderGatherResourceAction(actor);
            }
            ConsiderTakeCoverAction(actor);

            // Random decision based available actions
            if (AvailableActions.Count == 0)
            {
                return AIAction.EndTurn;
            }
            else
            {
                if (actor.ActorId.ActorSubtype == ActorSubtype.Collector &&
                    AvailableActions.Contains(AIAction.GatherResources))
                {
                    return AIAction.GatherResources;
                }
                int randomActionIndex = UnityEngine.Random.Range(0,
                    AvailableActions.Count);
                return AvailableActions[randomActionIndex];
            }
        }

        /// <summary>
        /// Evaluates the state of the actor to determine if AttackPlayer is an
        /// available action.
        /// </summary>
        /// <param name="actor">Actor whose state to evaluate.</param>
        /// <param name="playerPos">Position of the player actor to 
        /// attack.</param>
        private void ConsiderAttackPlayerAction(Actor actor, Vector3 playerPos)
        {
            float sqrAttackRange = actor.Fighter.GetSqrAttackRange();
            float sqrMaxTravelDistance = actor.Mover.GetSqrMaxTravelDistance();
            float sqrDistanceFromPlayer =
                (actor.transform.position - playerPos).sqrMagnitude;
            bool inAttackRangeofPlayer =
                sqrDistanceFromPlayer <= sqrAttackRange;

            if (actor.Fighter.Attacks > 0)
            {
                if (inAttackRangeofPlayer)
                {
                    AvailableActions.Add(AIAction.AttackPlayer);
                }
                else if ((sqrMaxTravelDistance + sqrAttackRange) >=
                    sqrDistanceFromPlayer)
                {
                    AvailableActions.Add(AIAction.AttackPlayer);
                }
            }
        }

        /// <summary>
        /// Evaluates the state of the actor to determine if AttackPlayerAllies
        /// is an available action.
        /// </summary>
        /// <param name="actor">Actor whose state to evaluate.</param>
        private void ConsiderAttackPlayerAlliesAction(Actor actor)
        {
            float sqrAttackRange = actor.Fighter.GetSqrAttackRange();
            float sqrMaxTravelDistance = actor.Mover.GetSqrMaxTravelDistance();
            int playerAllyCount = EntityManager.Instance.PlayerAllies.Count;

            if (playerAllyCount > 0)
            {
                if (actor.Fighter.Attacks > 0)
                {
                    Actor closestAlly =
                        EntityManager.Instance.GetClosestAlly(actor.transform.position);
                    float sqrDistanceFromClosestAlly =
                        (actor.transform.position -
                        closestAlly.transform.position).sqrMagnitude;

                    if ((sqrMaxTravelDistance + sqrAttackRange) >=
                        sqrDistanceFromClosestAlly)
                    {
                        AvailableActions.Add(AIAction.AttackPlayerAllies);
                    }
                }
            }
        }

        /// <summary>
        /// Evaluates the state of the actor to determine if BuildBot is an 
        /// available action.
        /// </summary>
        /// <param name="actor">Actor whose state to evaluate.</param>
        private void ConsiderBuildBotAction(Actor actor)
        {
            bool canBuildBot = false;
            if (actor.Builder != null)
            {
                int[] resourceCounts =
                actor.Builder.Inventory.ResourceCounts;
                foreach (ActorSubtype subtype in Enum.GetValues(typeof(ActorSubtype)))
                {
                    if (BuildingManager.Instance.BuildBotCheck(subtype,
                        resourceCounts))
                    {
                        canBuildBot = true;
                        break;
                    }
                }

                if (actor.Builder.Builds == actor.ActorStats.MaxBuilds)
                {
                    canBuildBot = false;
                }
            }
            if (canBuildBot)
            {
                AvailableActions.Add(AIAction.BuildBot);
            }
        }

        /// <summary>
        /// Evaluates the state of the actor to determine if GatherResource is 
        /// an available action.
        /// </summary>
        /// <param name="actor">Actor whose state to evaluate.</param>
        private void ConsiderGatherResourceAction(Actor actor)
        {
            float sqrAttackRange = actor.Fighter.GetSqrAttackRange();
            float sqrMaxTravelDistance = actor.Mover.GetSqrMaxTravelDistance();
            int numCrates = CrateManager.Instance.CrateCount;
            float sqrDistanceFromCrate = Mathf.Infinity;

            GameObject closestCrate =
                CrateManager.Instance.GetClosestCrate(actor.transform.position);
            if (closestCrate != null)
            {
                Vector3 closestCratePos = closestCrate.transform.position;
                sqrDistanceFromCrate =
                    (actor.transform.position - closestCratePos).sqrMagnitude;
            }


            int numExposedPickups = CrateManager.Instance.ExposedPickups.Count;
            float sqrDistanceFromPickup = Mathf.Infinity;
            GameObject closestExposedPickup =
                CrateManager.Instance.GetClosestExposedPickup(actor.transform.position);
            if (closestExposedPickup != null)
            {
                Vector3 closestExposePickupPos =
                    closestExposedPickup.transform.position;
                sqrDistanceFromPickup =
                    (actor.transform.position - closestExposePickupPos).sqrMagnitude;
            }

            if (actor.ActorId.ActorSubtype == ActorSubtype.Default ||
                actor.ActorId.ActorSubtype == ActorSubtype.Collector)
            {
                if (actor.Fighter.Attacks > 0)
                {
                    if (numCrates > 0)
                    {
                        if ((sqrMaxTravelDistance + sqrAttackRange) >=
                            sqrDistanceFromCrate)
                        {
                            AvailableActions.Add(AIAction.GatherResources);
                        }
                    }
                }
                if (numExposedPickups > 0)
                {
                    if (sqrMaxTravelDistance >= sqrDistanceFromPickup)
                    {
                        AvailableActions.Add(AIAction.GatherResources);
                    }
                }
            }
        }

        /// <summary>
        /// Evaluates the state of the actor to determine if TakeCover is an 
        /// available action.
        /// </summary>
        /// <param name="actor">Actor whose state to evaluate.</param>
        public void ConsiderTakeCoverAction(Actor actor)
        {
            Actor playerActor = EntityManager.Instance.PlayerActor;
            Vector3 playerFirePos = playerActor.Fighter.HandgunSlot.position;

            bool inCover =
                CoverManager.Instance.CheckCover(playerFirePos,
                actor.Fighter.TargetPoint.position);

            GameObject[] coverGameObjects =
                CoverManager.Instance.GetClosestCoverObjectOrder(actor.transform.position);

            Vector3 closestInCoverPosition = Vector3.negativeInfinity;

            foreach (GameObject coverGameObject in coverGameObjects)
            {
                CoverObject coverObject =
                    coverGameObject.GetComponent<CoverObject>();
                closestInCoverPosition =
                    coverObject.GetClosestInCoverPosition(playerFirePos,
                    actor.Fighter.TargetPoint.position);
                if (closestInCoverPosition != Vector3.negativeInfinity)
                {
                    break;
                }
            }

            if (!inCover && closestInCoverPosition != Vector3.negativeInfinity &&
               actor.Mover.MoveTime > 0 && (closestInCoverPosition - actor.transform.position).sqrMagnitude > 0.1f)
            {
                AvailableActions.Add(AIAction.TakeCover);
            }
        }
    }
}
