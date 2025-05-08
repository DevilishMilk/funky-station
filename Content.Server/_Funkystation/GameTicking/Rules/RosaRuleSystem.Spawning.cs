using System.Linq;
using Content.Server.GameTicking.Rules;
using Content.Server._Funkystation.GameTicking.Rules.Components;
using Content.Server._Funkystation.Roles;
using Content.Server.Roles;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;
using Content.Shared.Nutrition.Components;
using Content.Shared.Players;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Funkystation.GameTicking.Rules;

public sealed partial class RosaRuleSystem : GameRuleSystem<RosaRuleComponent>
{

    private void OnGetBriefing(Entity<RosaRoleComponent> role, ref GetBriefingEvent args)
    {
        args.Briefing = role.Comp.Role switch
        {
            RosaAgentRole.Red => Loc.GetString("Bwuh Red"),
            RosaAgentRole.Green => Loc.GetString("Bwuh Green"),
            RosaAgentRole.Blue => Loc.GetString("Bwuh Blue"),
            _ => Loc.GetString("zup")
        };
    }

    private void StartRound(EntityUid uid, RosaRuleComponent component, GameRuleComponent gameRule)
    {
        component.GameState = RosaGameState.InProgress;
        component.EndAt = TimeSpan.FromSeconds(component.RoundDuration);

        var allPlayerData = _playerManager.GetAllPlayerData().ToList();
        var participatingPlayers = new List<(EntityUid mind, RosaRoleComponent comp)>();
        foreach (var sessionData in allPlayerData)
        {
            var contentData = sessionData.ContentData();
            if (contentData == null)
                continue;

            if (!contentData.Mind.HasValue)
                continue;

            if (!_roleSystem.MindHasRole<RosaRoleComponent>(contentData.Mind.Value, out var role))
                continue; // Player is not participating in the game.

            participatingPlayers.Add((contentData.Mind.Value, role));
        }


        if (participatingPlayers.Count == 0)
        {
            _chatManager.DispatchServerAnnouncement(
                "The round has started but there are no players participating. Restarting",
                Color.Red);
            _roundEndSystem.EndRound(TimeSpan.FromSeconds(5));
            return;
        }

        foreach (var participatingPlayer in participatingPlayers)
        {
            var ent = Comp<MindComponent>(participatingPlayer.mind).OwnedEntity;
            if (ent.HasValue)
                _rejuvenate.PerformRejuvenate(ent.Value);
        }

        //this doesn't matter i don't think but i'm keeping it in because it's funny - mish
        RobustRandom.Shuffle(participatingPlayers); // Shuffle the list so we can just take the first N players
        RobustRandom.Shuffle(participatingPlayers);
        RobustRandom.Shuffle(participatingPlayers); // I don't trust the shuffle.
        RobustRandom.Shuffle(participatingPlayers);
        RobustRandom.Shuffle(participatingPlayers); // I really don't trust the shuffle.

        //assign players one by one into three teams
        //TODO: steal or think of a better way to do this because this isn't actually even
        for (var i = 0; i < participatingPlayers.Count(); i++)
        {
            var role = participatingPlayers[i];
            if (i % 2 == 0) {
                role.comp.Role = RosaAgentRole.Red;
            }

            if (i % 3 == 0) {
                role.comp.Role = RosaAgentRole.Green;
            }
            else
            {
                role.comp.Role = RosaAgentRole.Blue;
            }

            var ownedEntity = Comp<MindComponent>(role.mind).OwnedEntity;
            if (!ownedEntity.HasValue)
            {
                Log.Error("Player mind has no entity.");
            }
        }
    }

    private void OnBeforeSpawn(PlayerBeforeSpawnEvent ev)
    {
        var allAccess = _prototypeManager
            .EnumeratePrototypes<AccessLevelPrototype>()
            .Select(p => new ProtoId<AccessLevelPrototype>(p.ID))
            .ToArray();

        var query = EntityQueryEnumerator<RosaRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var ros, out var gameRule))
        {
            if (!GameTicker.IsGameRuleActive(uid, gameRule))
                continue;

            if (ros.GameState != RosaGameState.Preparing)
            {
                Log.Debug("Player tried to join a game of Rosa but the game is not in the preparing state.");
                _chatManager.DispatchServerMessage(ev.Player, "Sorry, the game has already started. You have been made an observer.");
                GameTicker.SpawnObserver(ev.Player); // Players can't join mid-round.
                ev.Handled = true;
                return;
            }

            var newMind = _mindSystem.CreateMind(ev.Player.UserId, ev.Profile.Name);
            _mindSystem.SetUserId(newMind, ev.Player.UserId);

            var mobMaybe = _stationSpawningSystem.SpawnPlayerCharacterOnStation(ev.Station, null, ev.Profile);
            var mob = mobMaybe!.Value;

            _mindSystem.TransferTo(newMind, mob);
            // SetOutfitCommand.SetOutfit(mob, sus.Gear, EntityManager);
            _roleSystem.MindAddRole(newMind, "MindRoleRosa");

            // Rounds only last like 5 minutes, so players shouldn't need to eat or drink.
            RemComp<ThirstComponent>(mob);
            RemComp<HungerComponent>(mob);

            _accessSystem.TrySetTags(mob, allAccess, EnsureComp<AccessComponent>(mob));

            EnsureComp<RosaPlayerComponent>(mob);

            if (_idCardSystem.TryFindIdCard(mob, out var idCard))
            {
                idCard.Comp.FullName = MetaData(mob).EntityName;
                idCard.Comp.LocalizedJobTitle = Loc.GetString("job-name-psychologist");
            }

            ev.Handled = true;
            break;
        }
    }
}
