using AmongUs.GameOptions;
using System;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;

public static class CustomRolesAssigner
{
    private static readonly System.Random random = new System.Random();

    public static void AssignRoles()
    {
        List<(string roleName, int percentage)> crewmateRoles = new List<(string, int)>
        {
            ("Jester", Options.JesterPerc.GetInt()),
            ("Mayor", Options.MayorPerc.GetInt())
        };

        List<(string roleName, int percentage)> impostorRoles = new List<(string, int)>
        {

        };

        List<PlayerControl> availablePlayers = new List<PlayerControl>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            availablePlayers.Add(player);
        }

        availablePlayers = availablePlayers.OrderBy(x => random.Next()).ToList();

        Dictionary<PlayerControl, string> playerRoles = new Dictionary<PlayerControl, string>();
        HashSet<string> assignedRoles = new HashSet<string>();

        foreach (var player in availablePlayers)
        {
            bool isCrewmate = !player.Data.Role.IsImpostor;

            List<(string roleName, int percentage)> rolesToAssign = isCrewmate ? crewmateRoles : impostorRoles;

            foreach (var (roleName, percentage) in rolesToAssign)
            {
                if (assignedRoles.Contains(roleName))
                    continue;

                int randomValue = random.Next(0, 101);

                Logger.Info($"{roleName}, Value: {randomValue}, Percentage: {percentage}", "StartGameCustomRole1");

                if (randomValue <= percentage && !playerRoles.ContainsKey(player))
                {
                    playerRoles[player] = roleName;
                    assignedRoles.Add(roleName);
                    Logger.Info($"({player.PlayerId}) {player.Data.PlayerName} -> {roleName}", "StartGameCustomRole2");

                    break;
                }
            }
        }
    }
}