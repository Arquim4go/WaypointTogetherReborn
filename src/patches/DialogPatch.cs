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

public static class WaypointShareSwitchPatch
{
    public static readonly MethodInfo AddShareComponentMethod =
        AccessTools.Method(typeof(WaypointShareSwitchPatch), nameof(AddShareComponent));

    public static GuiComposer AddShareComponent(GuiComposer composer, ref ElementBounds leftColumn, ref ElementBounds rightColumn)
    {
        if (composer.GetSwitch(Settings.ShouldShareSwitchName) == null)
        {
            composer = composer.AddStaticText(Lang.Get("waypointtogetherreborn:share"), CairoFont.WhiteSmallText(), leftColumn = leftColumn.BelowCopy(0, 9))
                .AddSwitch((bool _) => { }, rightColumn = rightColumn.BelowCopy(0, 40).WithFixedWidth(200), Settings.ShouldShareSwitchName);

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
            if (instruction.opcode == OpCodes.Ldstr && (string)instruction.operand == "waypoint-color")
            {
                yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                yield return new CodeInstruction(OpCodes.Ldloca_S, 1);
                yield return new CodeInstruction(OpCodes.Call, AddShareComponentMethod);

                found = true;
            }

            yield return instruction;
        }

        if (found is false)
        {
            throw new ArgumentException("Cannot find `waypoint-color` in GuiDialogAddWayPoint.ComposeDialog");
        }
    }
}

[HarmonyPatch(typeof(GuiDialogAddWayPoint), "ComposeDialog")]
public static class AddWaypointShareSwitchPatch
{
    public static GuiComposer AddShareComponent(GuiComposer composer, ref ElementBounds leftColumn, ref ElementBounds rightColumn)
    {
        return WaypointShareSwitchPatch.AddShareComponent(composer, ref leftColumn, ref rightColumn);
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return WaypointShareSwitchPatch.Transpiler(instructions);
    }
}

[HarmonyPatch(typeof(GuiDialogEditWayPoint), "ComposeDialog")]
public static class EditWaypointShareSwitchPatch
{
    public static GuiComposer AddShareComponent(GuiComposer composer, ref ElementBounds leftColumn, ref ElementBounds rightColumn)
    {
        return WaypointShareSwitchPatch.AddShareComponent(composer, ref leftColumn,  ref rightColumn);
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return WaypointShareSwitchPatch.Transpiler(instructions);
    }
}