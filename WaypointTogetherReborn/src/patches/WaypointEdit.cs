using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using WaypointTogetherReborn.client;
using WaypointTogetherReborn.network.packets;

namespace WaypointTogetherReborn.patches;

[HarmonyPatch(typeof(GuiDialogEditWayPoint), "ComposeDialog")]
public static class WaypointEdit
{
    public static readonly MethodInfo EditShareComponentMethod =
        AccessTools.Method(typeof(WaypointEdit), nameof(EditShareComponent));

    public static GuiComposer EditShareComponent(GuiComposer composer, ref ElementBounds leftColumn,
        ref ElementBounds rightColumn)
    {
        if (composer.GetSwitch(Settings.ShouldShareSwitchName) == null)
        {
            composer = composer
                .AddStaticText(Lang.Get("WaypointTogetherReborn:share"), CairoFont.WhiteSmallText(),
                    leftColumn = leftColumn.BelowCopy(0, 9))
                .AddSwitch((bool _) => { }, rightColumn = rightColumn.BelowCopy(0, 5).WithFixedWidth(200),
                    Settings.ShouldShareSwitchName);

            var sw = composer.GetSwitch(Settings.ShouldShareSwitchName);
            sw.On = ModConfig.ClientConfig?.DefaultSharing ?? false;
        }

        return composer;
    }

    public static void Postfix(GuiDialogEditWayPoint __instance)
    {
        var sw = __instance.SingleComposer?.GetSwitch(Settings.ShouldShareSwitchName);
        if (sw == null) return;
        sw.On = ModConfig.ClientConfig?.DefaultSharing ?? false;
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
                yield return new CodeInstruction(OpCodes.Call, EditShareComponentMethod);
                found = true;
            }

            yield return instruction;
        }

        if (!found)
            throw new ArgumentException("Cannot find `waypoint-color` in GuiDialogEditWayPoint.ComposeDialog");
    }
}

[HarmonyPatch(typeof(GuiDialogEditWayPoint), "onSave")]
public static class EditWaypointOnSavePatch
{
    public static readonly MethodInfo toReplaceWith =
        AccessTools.Method(typeof(EditWaypointOnSavePatch), nameof(BroadcastWaypoint));

    public static void BroadcastWaypoint(ICoreClientAPI capi, string message)
    {
        capi.Logger.Notification("[EditWaypoint] BroadcastWaypoint appelé : " + message);
        
        var dialog = capi.Gui.OpenedGuis
            .OfType<GuiDialogEditWayPoint>()
            .FirstOrDefault();

        if (dialog?.SingleComposer.GetSwitch(Settings.ShouldShareSwitchName)?.On == true)
        {
            var waypoint = Traverse.Create(dialog).Field("waypoint").GetValue<Waypoint>();
            Core mod = capi.ModLoader.GetModSystem<Core>();
            mod.client?.network.ShareWaypoint(message, waypoint?.Position);
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
                yield return new CodeInstruction(OpCodes.Pop); // retire null → [capi, command]
                yield return new CodeInstruction(OpCodes.Call, toReplaceWith); // consomme [capi, command]
                found = true;
                continue; // skip le Callvirt original
            }

            yield return instruction;
        }

        if (!found)
            throw new ArgumentException("Cannot find SendChatMessage in GuiDialogEditWayPoint.onSave");
    }
}