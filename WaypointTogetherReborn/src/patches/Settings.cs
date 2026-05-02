using System;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;

namespace WaypointTogetherReborn.patches;

public static class Settings
{
    public static readonly string ShouldShareSwitchName = "shouldShareButton";
    public static readonly MethodInfo SendChatMessageMethod = AccessTools.Method(typeof(ICoreClientAPI), 
        nameof(ICoreClientAPI.SendChatMessage), new Type[2] { typeof(string), typeof(string) });
}