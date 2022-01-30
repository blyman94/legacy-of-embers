using Game.Control;
using Game.Core;
using Game.Entity;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Game
{
    /// <summary>
    /// System for managing the turn-based flow of the game. Delegates all
    /// player and ally actors to a central player controller for input, and kicks
    /// off AI control of non-player-controlled actors.
    /// </summary>
    public class TurnManager : Singleton<TurnManager>
    {
        public event TurnSequenceUpdated TurnSequenceUpdated;
        public event StartNewTurn StartNewTurn;

        [Tooltip("Active FreeLook Camera GameObject.")]
        public GameObject ActiveCam;

        [Tooltip("Player controller object to control appropriate actors " +
            "during thier turn.")]
        public PlayerController PlayerController;

        [Tooltip("AI controller object to control appropriate actors " +
            "during their turn.")]
        public AIController AIController;

        /// <summary>
        /// List of actors that represents the turn sequence.
        /// </summary>
        public List<Actor> TurnQueue;

        protected override void Awake()
        {
            base.Awake();
            TurnQueue = new List<Actor>();
            ActiveCam = null;
        }

        private void OnEnable()
        {
            GameManager.Instance.GameStateChanged += StartGame;
        }

        private void OnDisable()
        {
            TurnSequenceUpdated = null;
            StartNewTurn = null;
        }

        /// <summary>
        /// Remove an actor from the turn registry.
        /// </summary>
        /// <param name="actor">Actor to be removed.</param>
        public void DeregisterActor(Actor actor)
        {
            ActorIDObject actorID = actor.ActorId;
            if (actorID.ActorType == ActorType.Bot)
            {
                if (actorID.Alignment == Alignment.Player)
                {
                    EntityManager.Instance.PlayerAllies.Remove(actor);
                }
                if (actorID.Alignment == Alignment.Enemy)
                {
                    EntityManager.Instance.RemoveEnemy(actor);
                }
            }
            else if (actorID.Alignment == Alignment.Enemy)
            {
                EntityManager.Instance.RemoveEnemy(actor);
            }
            TurnQueue.Remove(actor);

            TurnSequenceUpdated?.Invoke();
        }

        /// <summary>
        /// Ends the turn of the current actor, starts the turn of the next actor, 
        /// and updates the turn queue accordingly.
        /// </summary>
        public void MoveToNextTurn()
        {
            if (GameManager.Instance.GameState == GameState.Running)
            {
                // End the current actor's turn.
                Actor currentActor = TurnQueue[0];
                currentActor.EndTurn();
                currentActor.ActorCam.Priority = 0;

                // Move current actor to end of list.
                TurnQueue.RemoveAt(0);
                TurnQueue.Add(currentActor);
                TurnSequenceUpdated?.Invoke();

                // Start turn of the next actor.
                if (TurnQueue[0].ActorId.Alignment == Alignment.Player)
                {
                    AIController.ActorToControl = null;
                    PlayerController.ActorToControl = TurnQueue[0];
                    PlayerController.StartTurn();
                }
                else if (TurnQueue[0].ActorId.Alignment == Alignment.Enemy)
                {
                    AIController.ActorToControl = TurnQueue[0];
                    PlayerController.ActorToControl = null;
                    AIController.StartTurn();
                }
                StartNewTurn?.Invoke();
                TurnQueue[0].ActorCam.Priority = 10;
                ActiveCam = TurnQueue[0].ActorCam.gameObject;
            }
        }

        /// <summary>
        /// Adds an actor to the turn registry.
        /// </summary>
        /// <param name="actor">Actor to add.</param>
        public void RegisterActor(Actor actor)
        {
            ActorIDObject actorID = actor.ActorId;
            if (actorID.IdNumber == 0)
            {
                EntityManager.Instance.PlayerActor = actor;
            }
            if (actorID.ActorType == ActorType.Bot)
            {
                if (actorID.Alignment == Alignment.Player)
                {
                    EntityManager.Instance.PlayerAllies.Add(actor);
                }
                else if (actorID.Alignment == Alignment.Enemy)
                {
                    EntityManager.Instance.Enemies.Add(actor);
                }
                TurnQueue.Add(actor);
            }
            else if (actorID.ActorType == ActorType.Enemy)
            {
                EntityManager.Instance.Enemies.Add(actor);
                TurnQueue.Add(actor);
            }
            else
            {
                TurnQueue.Insert(0, actor);
            }
            TurnSequenceUpdated?.Invoke();
        }

        /// <summary>
        /// Method for other classes to invoke the TurnSequenceUpdated event.
        /// </summary>
        public void UpdateTurnSequence()
        {
            TurnSequenceUpdated?.Invoke();
        }

        /// <summary>
        /// Begins the game by handing control to the first actor in the queue,
        /// if the game state has moved from the Pregame state to the Running
        /// state.
        /// </summary>
        /// <param name="previous">Previous gamestate.</param>
        /// <param name="current">Current gamestate.</param>
        private void StartGame(GameState previous, GameState current)
        {
            if (previous == GameState.Pregame && current == GameState.Running)
            {
                TurnSequenceUpdated?.Invoke();
                if (TurnQueue[0].ActorId.IdNumber == 0)
                {
                    PlayerController.ActorToControl = TurnQueue[0];
                }
                if (TurnQueue[0].ActorId.Alignment == Alignment.Player)
                {
                    PlayerController.StartTurn();
                }
                else if (TurnQueue[0].ActorId.Alignment == Alignment.Enemy)
                {
                    PlayerController.EndTurnButton.interactable = false;
                    AIController.StartTurn();
                }
                StartNewTurn?.Invoke();
                ActiveCam = TurnQueue[0].ActorCam.gameObject;
            }
        }
    }
}
