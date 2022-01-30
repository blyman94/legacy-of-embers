using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Invoked by a Builder when it fails to build a bot.
    /// </summary>
    /// <param name="reasoning">Text explaining why the build failed.</param>
    public delegate void BuildFailed(string reasoning);

    /// <summary>
    /// Invoked when a Builder successfully builds a bot.
    /// </summary>
    public delegate void BuildStarted();

    /// <summary>
    /// Invoked by an Inventory when the component amount changes.
    /// </summary>
    /// <param name="resourceId">The component ID to change.</param>
    /// <param name="newCount">The new component count.</param>
    public delegate void ChangedResourceCount(int resourceId, int newCount);

    /// <summary>
    /// Invoked by a crate when it is destroyed.
    /// </summary>
    public delegate void CrateDestroyed();

    /// <summary>
    /// Invoked by an Actor when the actor should crouch.
    /// </summary>
    public delegate void Crouch();

    /// <summary>
    /// Invoked by a Health Reserve when current health has changed.
    /// </summary>
    /// <param name="newCurrent">New current health value of the 
    /// reserve.</param>
    /// <param name="damage">Whether or not the change signals that the health
    /// reserve has been damaged.</param>
    public delegate void CurrentChanged(float newCurrent, bool damage);

    /// <summary>
    /// Invoked by a Fighter when its weapon is drawn.
    /// </summary>
    public delegate void DrawWeapon();

    /// <summary>
    /// Invoked by a combat target when it takes damage.
    /// </summary>
    public delegate void DamageTaken();

    /// <summary>
    /// Invoked by a Fighter when it has run out of health.
    /// </summary>
    public delegate void FighterDied();

    /// <summary>
    /// Invoked by a Fighter when its weapon fires.
    /// </summary>
    public delegate void Fire();

    /// <summary>
    /// Invoked by the GameManager when the state of the game changes.
    /// </summary>
    /// <param name="oldState">The state the GameManager has changed 
    /// from.</param>
    /// <param name="newState">The state the GameManager has changed to.</param>
    public delegate void GameStateChanged(GameState oldState, GameState newState);

    /// <summary>
    /// Invoked by a Fighter when its weapon is holstered.
    /// </summary>
    public delegate void HolsterWeapon();

    /// <summary>
    /// Invoked by a Health Reserve when maximum health has changed.
    /// </summary>
    /// <param name="newMax">New amount of maximum health.</param>
    public delegate void MaxChanged(float newMax);

    /// <summary>
    /// Invoked by a Mover when it fails to move.
    /// </summary>
    /// <param name="reasoning">Text describing the reason why the move 
    /// failed.</param>
    public delegate void MovementFailed(string reasoning);

    /// <summary>
    /// Invoked by a Pickup object when it is picked up by an actor.
    /// </summary>
    /// <param name="pickedUpObject">GameObject of the signaling pickup.</param>
    public delegate void PickupGrabbed(GameObject pickedUpObject);

    /// <summary>
    /// Invoked by the Health Reserve when it is reduced to 0.
    /// </summary>
    public delegate void ReserveEmpty();

    /// <summary>
    /// Invoked by a CombatTarget to call for a damage indicator to be spawned
    /// at its location.
    /// </summary>
    /// <param name="origin">Location from which the damage popup will 
    /// emit.</param>
    /// <param name="damage">Amount of damage to be displayed.</param>
    /// <param name="critical">Whether or not the damage dealt is a critical 
    /// hit.</param>
    public delegate void RequestDamagePopup(Vector3 origin, int damage,
        bool critical);

    /// <summary>
    /// Invoked by a Fighter when it fails to shoot.
    /// </summary>
    /// <param name="reasoning">Reason why the shot failed.</param>
    public delegate void ShotFailed(string reasoning);

    /// <summary>
    /// Invoked by the Fighter when it begins a melee attack.
    /// </summary>
    public delegate void StartMelee();

    /// <summary>
    /// Invoked by the TurnManager when a new turn starts.
    /// </summary>
    public delegate void StartNewTurn();

    /// <summary>
    /// Invoked by an Actor when the cursor hovers over it and it is "in cover"
    /// in relation to the current actor, which may attack it.
    /// </summary>
    public delegate void ShowCoverIndicator();

    /// <summary>
    /// Invoked by an Actor when its turn ends.
    /// </summary>
    public delegate void TurnEnded(ActorIDObject actorId, bool turnStart);

    /// <summary>
    /// Invoked by an Actor when its turn starts.
    /// </summary>
    public delegate void TurnStarted(ActorIDObject actorId, bool turnStart);

    /// <summary>
    /// Invoked by the TurnManager when the turn sequence has progressed or
    /// changed.
    /// </summary>
    public delegate void TurnSequenceUpdated();

    /// <summary>
    /// Invoked by a Fighter to signal a change in the count of current attacks 
    /// it has in a given turn.
    /// </summary>
    /// <param name="newAttackCount">New count of attacks the fighter has this
    /// turn.</param>
    public delegate void UpdateAttackCount(int newAttackCount);

    /// <summary>
    /// Invoked by a Builder when its inventory has changed, and therefore the 
    /// types of bots it can build has changed.
    /// </summary>
    /// <param name="newBuildAbility">Array containing information on which bots
    /// the builder has the ability to build with current inventory.</param>
    public delegate void UpdateBuildAbility(ActorIDObject actorId, 
        bool[] newBuildAbility);

    /// <summary>
    /// Invoked by a builder when its number of built robots changes.
    /// </summary>
    /// <param name="newBuildCount">New number of robots built.</param>
    public delegate void UpdateBuildCount(int newBuildCount);

    /// <summary>
    /// Invoked by a mover when its move state (moving or unmoving) changes.
    /// </summary>
    /// <param name="isMoving">Dictates whethere is change in state is to or 
    /// from its moving state.</param>
    public delegate void UpdateMoveState(ActorIDObject actorId, bool isMoving);

    /// <summary>
    /// Invoked by a mover when its move time has changed.
    /// </summary>
    /// <param name="newMoveTime">New amount of time the mover has to move 
    /// before its turn ends.</param>
    public delegate void UpdateMoveTime(float totalMoveTime, float newMoveTime);

    /// <summary>
    /// Invoked by a mover when its move speed has changed. Used for animation.
    /// </summary>
    /// <param name="newSpeed">New speed after change.</param>
    public delegate void UpdateSpeed(float newSpeed);

    /// <summary>
    /// Invoked by a fighter when the type of weapon it is using has changed.
    /// This signals the animator to behave differently.
    /// </summary>
    /// <param name="weaponClass">The type of weapon the fighter has 
    /// switched to.</param>
    public delegate void UpdateWeaponType(WeaponClass weaponClass);

    /// <summary>
    /// Invoked by a Vaulter when it begins vaulting.
    /// </summary>
    public delegate void VaultStarted();

    /// <summary>
    /// Invoked by a Fighter when it switches weapons.
    /// </summary>
    public delegate void WeaponSwitched();
}
