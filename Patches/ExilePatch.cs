namespace AmongUsRevamped;

class ExileControllerWrapUpPatch
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    class ExileControllerPatch
    {
        public static void Postfix(ExileController __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;

            var ejectedPlayer = __instance.initData?.networkedPlayer;
            if (ejectedPlayer == null) return;

            PlayerControl pc = null;
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p.PlayerId == ejectedPlayer.PlayerId)
                {
                    pc = p;
                    break;
                }
            }

            Logger.Info($" {ejectedPlayer.PlayerName} was ejected", "ExileController");

            if (CustomRoleManagement.PlayerRoles.TryGetValue(ejectedPlayer.PlayerId, out var role) && role == "Jester")
            {
                Utils.CustomWinnerEndGame(pc, 1);
                NormalGameEndChecker.CheckWinnerText("Jester");
                return;
            }

            if (CustomRoleManagement.HandlingRoleMessages)
            {
                Logger.Info(" SRM2 is called before SRM1 finished. This should not happen.", "ExileController");
            }
            else
            {
                CustomRoleManagement.SendRoleMessages(new Dictionary<string, string>
                {
                    { "Jester", Translator.Get("jesterPriv")},
                    { "Mayor", Translator.Get("mayorPriv", Options.MayorExtraVoteCount.GetInt())},
                });
            }
        }
    }
}