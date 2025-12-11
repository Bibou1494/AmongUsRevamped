using System;
using UnityEngine;

namespace HNSRevamped;

// https://github.com/Yumenopai/TownOfHost_Y
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class Zoom
{
    private static bool ResetButtons = false;
    public static void Postfix()
    {
        if (Utils.IsShip && !Utils.IsMeeting && Utils.IsCanMove && PlayerControl.LocalPlayer.Data.IsDead || Utils.IsLobby && Utils.IsCanMove)
        {
            if (Camera.main.orthographicSize > 3.0f)
                ResetButtons = true;

            if (Input.mouseScrollDelta.y > 0)
            {
                if (Camera.main.orthographicSize > 3.0f)
                {
                    SetZoomSize(times: false);
                }

            }
            if (Input.mouseScrollDelta.y < 0)
            {
                if (Utils.IsDead || Utils.IsFreePlay || Utils.IsLobby)
                {
                    if (Camera.main.orthographicSize < 18.0f)
                    {
                        SetZoomSize(times: true);
                    }
                }
            }
            Flag.NewFlag("Zoom");
        }
        else
        {
            Flag.Run(() =>
            {
                SetZoomSize(reset: true);
            }, "Zoom");
        }
    }

    private static void SetZoomSize(bool times = false, bool reset = false)
    {
        var size = 1.2f;
        if (!times) size = 1 / size;
        if (reset)
        {
            Camera.main.orthographicSize = 3.0f;
            HudManager.Instance.UICamera.orthographicSize = 3.0f;
            HudManager.Instance.Chat.transform.localScale = Vector3.one;
            if (Utils.IsMeeting) MeetingHud.Instance.transform.localScale = Vector3.one;
        }
        else
        {
            Camera.main.orthographicSize *= size;
            HudManager.Instance.UICamera.orthographicSize *= size;
        }
        DestroyableSingleton<HudManager>.Instance?.ShadowQuad?.gameObject?.SetActive((reset || Camera.main.orthographicSize == 3.0f) && !Utils.IsDead);

        if (ResetButtons)
        {
            ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
            ResetButtons = false;
        }
    }

    public static void OnFixedUpdate()
        => DestroyableSingleton<HudManager>.Instance?.ShadowQuad?.gameObject?.SetActive((Camera.main.orthographicSize == 3.0f) && !Utils.IsDead);
}

public static class Flag
{
    private static readonly List<string> OneTimeList = [];
    private static readonly List<string> FirstRunList = [];
    public static void Run(Action action, string type, bool firstrun = false)
    {
        if (OneTimeList.Contains(type) || (firstrun && !FirstRunList.Contains(type)))
        {
            if (!FirstRunList.Contains(type)) FirstRunList.Add(type);
            OneTimeList.Remove(type);
            action();
        }

    }
    public static void NewFlag(string type)
    {
        if (!OneTimeList.Contains(type)) OneTimeList.Add(type);
    }

    public static void DeleteFlag(string type)
    {
        if (OneTimeList.Contains(type)) OneTimeList.Remove(type);
    }
}