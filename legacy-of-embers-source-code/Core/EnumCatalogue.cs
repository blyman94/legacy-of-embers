namespace Game.Core
{
    /// <summary>
    /// Dictates all possible actor subtypes.
    /// </summary>
    public enum ActorSubtype
    {
        Collector, Defender, Warrior, Default
    }

    /// <summary>
    /// Dictates all possible types of actor.
    /// </summary>
    public enum ActorType
    {
        Bot, Enemy, Player, Default
    }

    /// <summary>
    /// Dictates all possible AIActions a brain can take.
    /// </summary>
    public enum AIAction
    {
        AttackPlayer, AttackPlayerAllies, BuildBot, EndTurn, GatherResources, TakeCover, Default
    }

    /// <summary>
    /// Dictates all possible alignments of each actor.
    /// </summary>
    public enum Alignment
    {
        Enemy, Player, Default
    }

    /// <summary>
    /// Dictates all possible attack types of all weapons in the game.
    /// </summary>
    public enum AttackType
    {
        BurstFire, Melee, SingleFire, SpreadFire, Default
    }

    /// <summary>
    /// Dictates all possible weapon classes in the game.
    /// </summary>
    public enum WeaponClass
    {
        Rifle, LongRifle, Handgun, Melee, Default
    }

    /// <summary>
    /// Dictates all possible states the game can be in.
    /// </summary>
    public enum GameState
    {
        Running, Paused, Pregame, Postgame, Default
    }

    /// <summary>
    /// Dictates all possible range projector types.
    /// </summary>
    public enum ProjectorType
    {
        Melee, Ranged, Default
    }
}
