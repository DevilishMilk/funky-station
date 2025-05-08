namespace Content.Server._Funkystation.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(RosaRuleSystem))]
public sealed partial class RosaRuleComponent : Component
{
    #region State management

    public RosaGameState GameState = RosaGameState.Preparing;

    /// <summary>
    /// Defines when the round will end.
    /// </summary>
    public TimeSpan EndAt = TimeSpan.MinValue;

    public List<int> AnnouncedTimeLeft = new List<int>();

    #endregion
     /// <summary>
     /// How long to wait before the game starts after the round starts.
     /// </summary>
     [DataField]
     public int PreparingDuration = 30;

     /// <summary>
     /// How long the round lasts in seconds.
     /// </summary>
     [DataField]
     public int RoundDuration = 60;

     /// <summary>
     /// How long to wait before restarting the round after the summary is displayed.
     /// </summary>
     [DataField]
     public int PostRoundDuration = 30;
//
//     /// <summary>
//     /// The gear all players spawn with.
//     /// </summary>
//     [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<StartingGearPrototype>))]
//     public string Gear = "SuspicionGear";
}
    public enum RosaGameState
    {
        /// <summary>
        /// The game is preparing to start. No roles have been assigned yet and new joining players will be spawned in.
        /// </summary>
        Preparing,

        /// <summary>
        /// The game is in progress. Roles have been assigned and players are hopefully killing each other. New joining players will be forced to spectate.
        /// </summary>
        InProgress,

        /// <summary>
        /// The game has ended. The summary is being displayed and players are waiting for the round to restart.
        /// </summary>
        PostRound
    }

