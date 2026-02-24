using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using System;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.CoShowIntro))]
internal static class CoShowIntroPatch
{
    public static void Postfix(IntroCutscene __instance)
    {
        Logger.Info(" Intro initiated", "CoShowIntro");

        if (!AmongUsClient.Instance.AmHost) return;

        NormalGameEndChecker.customRoles = CustomRoleManagement.PlayerToCustomRole();
        CustomRoleManagement.SendRoleMessages(new Dictionary<string, string>
        {
            { "Jester", Translator.Get("jesterPriv")},
            { "Mayor", Translator.Get("mayorPriv", Options.MayorExtraVoteCount.GetInt())},
        });
        
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            p.cosmetics.nameText.text = p.Data.PlayerName;

            MurderPlayerPatch.killCount[p.Data.PlayerId] = 0;
            MurderPlayerPatch.misfireCount[p.Data.PlayerId] = 0;
            PlayerControlCompleteTaskPatch.playerTasksCompleted[p] = 0;
            PlayerControlCompleteTaskPatch.tasksPerPlayer[p] = 0;
        }

        if (Options.DisableAnnoyingMeetingCalls.GetBool())
        {
            Utils.CanCallMeetings = false;
            _ = new LateTask(() =>
            {       
                Utils.CanCallMeetings = true;
            }, Options.ChatBeforeFirstMeeting.GetBool() ? 39.5f : 33f, "MeetingEnabled");     
        }
    }
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
class BeginCrewmatePatch
{
    public static void Postfix(IntroCutscene __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        if (Options.Gamemode.GetValue() == 2 && Options.SNSChatInGame.GetBool() || Options.Gamemode.GetValue() == 0 && Options.ChatBeforeFirstMeeting.GetBool())
        {
            _ = new LateTask(() =>
            {  
                PlayerControl.LocalPlayer.CmdReportDeadBody(null);
                MeetingHud.Instance.RpcClose(); 

            }, 7f, "SetChatVisible");  
        }

        if (Main.GM.Value)
        {
            __instance.TeamTitle.text = "Game Master";
        }
    }
}