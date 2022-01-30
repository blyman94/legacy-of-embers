using Game.Core;
using Game.Entity;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Keeps track of all entities in battle and allows for global access of 
    /// information about these entities.
    /// </summary>
    public class EntityManager : Singleton<EntityManager>
    {
        /// <summary>
        /// Actor object representing the player.
        /// </summary>
        public Actor PlayerActor { get; set; }

        /// <summary>
        /// List of Allies the Player has built during combat.
        /// </summary>
        public List<Actor> PlayerAllies { get; set; }

        /// <summary>
        /// List of enemies that are involved in the combat.
        /// </summary>
        public List<Actor> Enemies { get; set; }

        protected override void Awake()
        {
            base.Awake();
            PlayerAllies = new List<Actor>();
            Enemies = new List<Actor>();
        }

        /// <summary>
        /// Returns an Actor object representing the closest Player Ally (Bot)
        /// to the compare position.
        /// </summary>
        /// <param name="comparePos">Position to compare distance to.</param>
        public Actor GetClosestAlly(Vector3 comparePos)
        {
            if (PlayerAllies.Count == 0)
            {
                return null;
            }
            float shortestSqrDistance = Mathf.Infinity;
            int closestAllyIndex = -1;
            for (int i = 0; i < PlayerAllies.Count; i++)
            {
                float sqrDistance = (PlayerAllies[i].transform.position -
                    comparePos).sqrMagnitude;
                if (sqrDistance < shortestSqrDistance)
                {
                    shortestSqrDistance = sqrDistance;
                    closestAllyIndex = i;
                }
            }
            return PlayerAllies[closestAllyIndex];
        }

        /// <summary>
        /// Removes an enemy from the Enemies list, then ends the game with the
        /// win sequence if there are no enemies remaining in that list.
        /// </summary>
        /// <param name="enemy"></param>
        public void RemoveEnemy(Actor enemy)
        {
            Enemies.Remove(enemy);
            if (Enemies.Count <= 0)
            {
                GameManager.Instance.EndGame(true);
            }
        }

        /// <summary>
        /// Returns a list of all BotTypes the Player currently has built as 
        /// allies.
        /// </summary>
        public List<ActorSubtype> GetAllyBotTypes()
        {
            if (PlayerAllies.Count == 0)
            {
                return null;
            }

            List<ActorSubtype> presentTypes = new List<ActorSubtype>();
            foreach (Actor ally in PlayerAllies)
            {
                if (!presentTypes.Contains(ally.ActorId.ActorSubtype))
                {
                    presentTypes.Add(ally.ActorId.ActorSubtype);
                }
            }
            return presentTypes;
        }
    }
}
