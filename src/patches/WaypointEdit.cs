using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace WaypointTogetherReborn.patches;

[HarmonyPatch(typeof(GuiDialogEditWayPoint), "onSave")]
class PatchGuiDialogEditWayPointOnSave
{

    public static readonly MethodInfo toReplaceWith = AccessTools.Method(typeof(PatchGuiDialogEditWayPointOnSave), nameof(BroadcastWaypoint));
    public static void BroadcastWaypoint(ICoreClientAPI capi, string message, GuiDialogEditWayPoint instance)
    {
        if (capi != null)
        {
            if (instance.SingleComposer.GetSwitch(Settings.ShouldShareSwitchName).On)
            {
                WaypointTogetherReborn.Core mod = capi.ModLoader.GetModSystem<WaypointTogetherReborn.Core>();
                var maplayers = capi.ModLoader.GetModSystem<WorldMapManager>().MapLayers;
                var mapLayer = (maplayers.Find(x => x is WaypointMapLayer) as WaypointMapLayer);
                var waypoint = Traverse.Create(instance).Field("waypoint").GetValue<Waypoint>();
                mod.client.network.ShareWaypoint(message, waypoint.Guid);
                string messageToTheUser = Lang.Get("WaypointTogetherReborn.waypoint-shared");
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