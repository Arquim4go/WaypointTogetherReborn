using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace WaypointTogetherReborn.patches;

[HarmonyPatch(typeof(GuiDialogAddWayPoint), "ComposeDialog")]
public static class AddWaypointComposeDialogPatch
{
    public static readonly MethodInfo AddShareComponentMethod =
        AccessTools.Method(typeof(AddWaypointComposeDialogPatch), nameof(AddShareComponent));

    public static GuiComposer AddShareComponent(GuiComposer composer, ref ElementBounds leftColumn,
        ref ElementBounds rightColumn)
    {
        if (composer.GetSwitch(Settings.ShouldShareSwitchName) == null)
        {
            composer = composer
                .AddStaticText(Lang.Get("WaypointTogetherReborn:share"), CairoFont.WhiteSmallText(),
                    leftColumn = leftColumn.BelowCopy(0, 9))
                .AddSwitch((bool _) => { }, rightColumn = rightColumn.BelowCopy(0, 40).WithFixedWidth(200),
                    Settings.ShouldShareSwitchName);

            var sw = composer.GetSwitch(Settings.ShouldShareSwitchName);
            sw.On = ModConfig.ClientConfig?.DefaultSharing ?? false;
        }
        return composer;
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;
        foreach (var instruction in instructions)
        {
            if (!found && instruction.opcode == OpCodes.Ldstr && (string)instruction.operand == "waypoint-color")
            {
                yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                yield return new CodeInstruction(OpCodes.Ldloca_S, 1);
                yield return new CodeInstruction(OpCodes.Call, AddShareComponentMethod);
                found = true;
            }
            yield return instruction;
        }
        if (!found)
            throw new ArgumentException("Cannot find `waypoint-color` in GuiDialogAddWayPoint.ComposeDialog");
    }
}

[HarmonyPatch(typeof(GuiDialogAddWayPoint), "OnSave")]
public static class AddWaypointOnSavePatch
{
    public static readonly MethodInfo toReplaceWith =
        AccessTools.Method(typeof(AddWaypointOnSavePatch), nameof(BroadcastWaypoint));

    public static void BroadcastWaypoint(ICoreClientAPI capi, string message)
    {
        var sw = capi.Gui.OpenedGuis
            .OfType<GuiDialogAddWayPoint>()
            .FirstOrDefault()
            ?.SingleComposer
            .GetSwitch(Settings.ShouldShareSwitchName);

        if (sw?.On == true)
        {
            Core mod = capi.ModLoader.GetModSystem<Core>();
            mod.client?.network.ShareWaypoint(message);
            capi.ShowChatMessage(Lang.Get("WaypointTogetherReborn:waypoint-shared"));
        }

        capi.SendChatMessage(message);
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;
        foreach (var instruction in instructions)
        {
            if (!found && instruction.opcode == OpCodes.Callvirt
                       && instruction.operand is MethodInfo method
                       && method == Settings.SendChatMessageMethod)
            {
                // Stack: [capi, command, null]
                yield return new CodeInstruction(OpCodes.Pop);          // retire null → [capi, command]
                yield return new CodeInstruction(OpCodes.Call, toReplaceWith); // consomme [capi, command]
                found = true;
                continue; // skip le Callvirt original
            }
            yield return instruction;
        }
        if (!found)
            throw new ArgumentException("Cannot find SendChatMessage in GuiDialogAddWayPoint.OnSave");
    }
}