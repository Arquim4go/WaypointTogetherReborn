using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace WaypointTogetherReborn.patches;

[HarmonyPatch(typeof(GuiDialogAddWayPoint), "OnSave")]
class PatchGuiDialogAddWayPointOnSave
{
    public static readonly MethodInfo toReplaceWith = AccessTools.Method(typeof(PatchGuiDialogAddWayPointOnSave), nameof(BroadcastWaypoint));
    public static void BroadcastWaypoint(ICoreClientAPI capi, string message, GuiDialogEditWayPoint instance)
    {
        if (capi != null)
        {
            if (instance.SingleComposer.GetSwitch(Settings.ShouldShareSwitchName).On)
            {
                Core mod = capi.ModLoader.GetModSystem<Core>();
                mod.client.network.ShareWaypoint(message, capi.World.Player.PlayerUID);
                string messageToTheUser = Lang.Get("waypointtogetherreborn:waypoint-shared");
                capi.ShowChatMessage(messageToTheUser);
            }
            capi.SendChatMessage(message);
        }
    }
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> list = new();
        foreach (var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Callvirt && (MethodInfo)instruction.operand == Settings.SendChatMessageMethod)
            {
                list.RemoveAt(list.Count - 1); // remove 'ldnull'
                list.Add(new CodeInstruction(OpCodes.Ldarg_0)); // load 'this'
                list.Add(new CodeInstruction(OpCodes.Call, toReplaceWith));
            }
            else
            {
                list.Add(instruction);
            }
        }
        return list;
    }
}