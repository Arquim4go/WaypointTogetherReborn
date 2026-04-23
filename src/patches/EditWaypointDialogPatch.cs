using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace WaypointTogetherReborn.patches;

public static class EditWaypointDialogPatch
{
    public static readonly MethodInfo EditShareComponentMethod =
        AccessTools.Method(typeof(EditWaypointDialogPatch), nameof(EditShareComponent));

    public static GuiComposer EditShareComponent(GuiComposer composer, ref ElementBounds leftColumn,
        ref ElementBounds rightColumn)
    {
        if (composer.GetSwitch(Settings.ShouldShareSwitchName) == null)
        {
            composer = composer.AddStaticText(Lang.Get("WaypointTogetherReborn:share"), CairoFont.WhiteSmallText(),
                    leftColumn = leftColumn.BelowCopy(0, 9))
                .AddSwitch((bool _) => { }, rightColumn = rightColumn.BelowCopy(0, 5).WithFixedWidth(200),
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
            if (instruction.opcode == OpCodes.Ldstr && (string)instruction.operand == "waypoint-color")
            {
                yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                yield return new CodeInstruction(OpCodes.Ldloca_S, 1);
                yield return new CodeInstruction(OpCodes.Call, EditShareComponentMethod);

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

[HarmonyPatch(typeof(GuiDialogEditWayPoint), "ComposeDialog")]
public static class EditWaypointSharePatch
{
    public static GuiComposer EditShareComponent(GuiComposer composer, ref ElementBounds leftColumn,
        ref ElementBounds rightColumn)
    {
        return EditWaypointDialogPatch.EditShareComponent(composer, ref leftColumn, ref rightColumn);
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return EditWaypointDialogPatch.Transpiler(instructions);
    }
}