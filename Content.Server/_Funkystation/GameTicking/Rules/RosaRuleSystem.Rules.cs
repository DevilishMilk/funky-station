using Content.Server.GameTicking.Rules;
using Content.Server._Funkystation.GameTicking.Rules.Components;

namespace Content.Server._Funkystation.GameTicking.Rules;

/// <summary>
/// Contains the non-core logic for SSS.
/// </summary>
public sealed partial class RosaRuleSystem : GameRuleSystem<RosaRuleComponent>
{
    private void UpdateTimer(ref RosaRuleComponent ros, float frameTime)
    {
        ros.EndAt -= TimeSpan.FromSeconds(frameTime);

        var timeLeft = ros.EndAt.TotalSeconds;
        switch (timeLeft)
        {
            case <= 240 when !ros.AnnouncedTimeLeft.Contains(240):
                _chatManager.DispatchServerAnnouncement($"The round will end in {Math.Round(ros.EndAt.TotalMinutes)}:{ros.EndAt.Seconds}.");
                ros.AnnouncedTimeLeft.Add(240);
                break;
            case <= 180 when !ros.AnnouncedTimeLeft.Contains(180):
                _chatManager.DispatchServerAnnouncement($"The round will end in {Math.Round(ros.EndAt.TotalMinutes)}:{ros.EndAt.Seconds}.");
                ros.AnnouncedTimeLeft.Add(180);
                break;
            case <= 120 when !ros.AnnouncedTimeLeft.Contains(120):
                _chatManager.DispatchServerAnnouncement($"The round will end in {Math.Round(ros.EndAt.TotalMinutes)}:{ros.EndAt.Seconds}.");
                ros.AnnouncedTimeLeft.Add(120);
                break;
            case <= 60 when !ros.AnnouncedTimeLeft.Contains(60):
                _chatManager.DispatchServerAnnouncement($"The round will end in {Math.Round(ros.EndAt.TotalMinutes)}:{ros.EndAt.Seconds}.");
                ros.AnnouncedTimeLeft.Add(60);
                break;
            case <= 30 when !ros.AnnouncedTimeLeft.Contains(30):
                _chatManager.DispatchServerAnnouncement($"The round will end in 30 seconds.");
                ros.AnnouncedTimeLeft.Add(30);
                break;
            case <= 10 when !ros.AnnouncedTimeLeft.Contains(10):
                _chatManager.DispatchServerAnnouncement($"The round will end in 10 seconds.");
                ros.AnnouncedTimeLeft.Add(10);
                break;
            case <= 5 when !ros.AnnouncedTimeLeft.Contains(5):
                _chatManager.DispatchServerAnnouncement($"The round will end in 5 seconds.");
                ros.AnnouncedTimeLeft.Add(5);
                break;
        }

        if (ros.EndAt > TimeSpan.Zero)
        {
            return;
        }

        //STARTIN A NEW DAY BUB
        StartNewDay(ros);
    }
}
