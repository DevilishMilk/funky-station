using Content.Server.Access.Systems;
using Content.Server.Administration.Systems;
using Content.Server.Antag;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking.Rules;
using Content.Server._Funkystation.GameTicking.Rules.Components;
using Content.Server._Funkystation.Roles;
using Content.Server.Implants;
using Content.Server.Mind;
using Content.Server.Pinpointer;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Server.RoundEnd;
using Content.Server.Station.Systems;
using Content.Server.Store.Systems;
using Content.Shared.Damage;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Systems;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server._Funkystation.GameTicking.Rules;

public sealed partial class RosaRuleSystem : GameRuleSystem<RosaRuleComponent>
{
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly RoleSystem _roleSystem = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawningSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;
    [Dependency] private readonly AccessSystem _accessSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StoreSystem _storeSystem = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly IdCardSystem _idCardSystem = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookupSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

     public override void Initialize()
     {
         base.Initialize();

         SubscribeLocalEvent<PlayerBeforeSpawnEvent>(OnBeforeSpawn);
         SubscribeLocalEvent<RosaRoleComponent, GetBriefingEvent>(OnGetBriefing);
     }
     protected override void Started(EntityUid uid, RosaRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
     {
         base.Started(uid, component, gameRule, args);

         component.GameState = RosaGameState.Preparing;

         Timer.Spawn(TimeSpan.FromSeconds(component.PreparingDuration - 10), () =>  _chatManager.DispatchServerAnnouncement("The round will start in 10 seconds."));
         Timer.Spawn(TimeSpan.FromSeconds(component.PreparingDuration - 5), () =>  _chatManager.DispatchServerAnnouncement("The round will start in 5 seconds."));
         Timer.Spawn(TimeSpan.FromSeconds(component.PreparingDuration), () =>  StartRound(uid, component, gameRule));
         Log.Debug("Starting a game of Station Rosa.");
     }

     public override void Update(float frameTime)
     {
         base.Update(frameTime);

         var query = EntityQueryEnumerator<RosaRuleComponent, GameRuleComponent>();
         while (query.MoveNext(out var uid, out var ros, out var gameRule))
         {
             if (!GameTicker.IsGameRuleActive(uid, gameRule))
                 continue;

             if (ros.GameState != RosaGameState.InProgress)
                 continue;

             UpdateTimer(ref ros, frameTime);
         }
     }
 }
