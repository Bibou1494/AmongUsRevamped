namespace AmongUsRevamped;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
public static class CheckForEndVotingPatch
{
    public static bool Prefix(MeetingHud __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        if (!__instance.playerStates.All(ps => ps.AmDead || ps.DidVote)) return true;

        var votes = __instance.CalculateVotes();
        var visualVotes = new List<MeetingHud.VoterState>();

        foreach (var ps in __instance.playerStates)
        {
            if (ps == null) continue;

            byte voterId = ps.TargetPlayerId;
            byte votedFor = ps.VotedFor;
            if (votedFor == byte.MaxValue) continue;

            visualVotes.Add(new MeetingHud.VoterState { VoterId = voterId, VotedForId = votedFor });

            if (CustomRoleManagement.PlayerRoles.TryGetValue(voterId, out string role) && role == "Mayor")
            {
                int extraVotes = Options.MayorExtraVoteCount.GetInt();
                if (!votes.TryGetValue(votedFor, out int currentVotes)) currentVotes = 0;
                votes[votedFor] = currentVotes + extraVotes;

                for (int i = 0; i < extraVotes; i++)
                    visualVotes.Add(new MeetingHud.VoterState { VoterId = voterId, VotedForId = votedFor });
            }
        }

        var max = votes.MaxPair(out var tie);
        var exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(p => !tie && p.PlayerId == max.Key);
        var statesArray = visualVotes.ToArray();

        __instance.RpcVotingComplete(statesArray, exiled, tie);

        return false;
    }
}