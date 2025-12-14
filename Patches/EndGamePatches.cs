using AmongUs.Data;
using Hazel;
using InnerNet;
using UnityEngine;

namespace HNSRevamped;

[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.ShowButtons))]
public static class EndGameManagerPatch
{
    public static void Postfix(EndGameManager __instance)
    {
        RpcSetTasksPatch.GlobalTaskIds = null;
        OnGameJoinedPatch.AutoStartCheck = false;
        
        EndGameNavigation navigation = __instance.Navigation;
        if (!AmongUsClient.Instance.AmHost || __instance == null || navigation == null || !Options.AutoRejoinLobby.GetBool()) return;
        navigation.NextGame();
    }
}

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
class NormalGameEndChecker
{
    public static bool Prefix()
    {
        if (Options.NoGameEnd.GetBool()) return false;
        else return true;
    }
}
[HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.CheckEndCriteria))]
class HNSGameEndChecker
{
    public static bool Prefix()
    {
        if (Options.NoGameEnd.GetBool()) return false;
        else return true;
    }
}
[HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
internal class ControllerManagerUpdatePatch
{
    public static void Postfix()
    {
        if (Input.GetKey(KeyCode.L) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Return))
        {
            MessageWriter writer = AmongUsClient.Instance.StartEndGame();
            writer.Write((byte)GameOverReason.ImpostorDisconnect);
            AmongUsClient.Instance.FinishEndGame(writer);
        }
        
        if (Input.GetKey(KeyCode.M) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Return) && Utils.InGame)
        {
            if (Utils.IsMeeting)
            {
                MeetingHud.Instance.RpcClose();
            }
            else
                PlayerControl.LocalPlayer.ReportDeadBody(PlayerControl.LocalPlayer.Data);
            }
    }
}