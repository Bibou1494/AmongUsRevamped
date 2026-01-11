using Hazel;
using InnerNet;
using System;
using UnityEngine;

// https://github.com/SuperNewRoles/SuperNewRoles/blob/master/SuperNewRoles/Patches/LobbyBehaviourPatch.cs
namespace AmongUsRevamped;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
internal static class ChatControllerUpdatePatch
{

    public static void Postfix(ChatController __instance)
    {
        if (Main.DarkTheme.Value)
        {
            __instance.freeChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);

            __instance.quickChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
            __instance.quickChatField.text.color = Color.white;
        }
        else __instance.freeChatField.textArea.outputText.color = Color.black;
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
internal static class SendChatPatch
{
    public static bool Prefix(ChatController __instance)
    {
        string text = __instance.freeChatField.textArea.text.Trim();

        if (text == "/h" || text == "/help")
        {
//            PlayerControl.LocalPlayer.SendChat("All AUR commands:\n/0kc - Sends the 0 Kill Cooldown gamemode description to everyone");
            return false;
        }
        
        if (__instance.timeSinceLastMessage < 3f) return false;

        if (text == "/0kc" || text == "/0kcd" || text == "/0killcooldown")
        {
            PlayerControl.LocalPlayer.RpcSendChat("0 Kill Cooldown Mode:\n\nImpostors have no kill cooldown, Crewmates have low tasks.\nThink fast and pay attention.");
            __instance.timeSinceLastMessage = 0.8f;
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text == "/sns" || text == "/shiftandseek" || text == "/shift&seek")
        {
            PlayerControl.LocalPlayer.RpcSendChat("Shift and Seek Mode:\n\nImps can only kill someone while shapeshifted as them.\nSabotages & Meetings = Off.");
            __instance.timeSinceLastMessage = 0.8f;
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);

            new LateTask(() =>
            {
                PlayerControl.LocalPlayer.RpcSendChat($"Crew wins by tasks/surviving {Options.CrewAutoWinsGameAfter.GetInt()}s.\nImp wins by killing Crew.\n1 Wrong kill = Can't kill for {Options.CantKillTime.GetInt()}s.\n{Options.MisfiresToSuicide.GetInt()} Wrong kills = suicide.");
                __instance.timeSinceLastMessage = 0.8f;
            }, 2.2f, "SNSTutorial2");                
            return false;
        }

        if (text == "/sp" || text == "/sr" || text == "/speedrun")
        {
            PlayerControl.LocalPlayer.RpcSendChat($"Speedrun Mode:\n\nEveryone is a crewmate. The 1st player to finish tasks wins the game. Game auto ends after {Options.GameAutoEndsAfter.GetInt()}s");
            __instance.timeSinceLastMessage = 0.8f;
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        else return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), "RpcSendChat")]
public static class PC_RpcSendChat
{
    // Banning works by Name and not Id
	public static void Prefix(PlayerControl __instance, ref string chatText)
	{

        if (!AmongUsClient.Instance.AmHost)
        return;

        bool isKick = chatText.StartsWith("/kick ");
        bool isBan  = chatText.StartsWith("/ban ");

        if (!isKick && !isBan)
        return;

        if (__instance == PlayerControl.LocalPlayer || Utils.IsPlayerModerator(__instance.Data.FriendCode))
        {
            string kickedPlayerName = chatText.Substring(isBan ? 5 : 6).Trim();

            PlayerControl target = null;

            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.Data != null && !Utils.IsPlayerModerator(p.Data.FriendCode) && p != PlayerControl.LocalPlayer && p.Data.PlayerName.Equals(kickedPlayerName, StringComparison.OrdinalIgnoreCase))
                {
                    target = p;
                    break;
               }
            }

            if (target == null)
            return;

            AmongUsClient.Instance.KickPlayer(target.Data.ClientId, isBan);
            Logger.Info($" {__instance.Data.PlayerName} {(isBan ? "banned" : "kicked")} {target.Data.PlayerName}", "Kick&BanCommand");
        }
    }
}