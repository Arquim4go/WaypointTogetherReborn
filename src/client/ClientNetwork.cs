using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using WaypointTogetherReborn.network.packets;

namespace WaypointTogetherReborn.client;

public class ClientNetwork
{
    private readonly ICoreClientAPI api;
    private readonly IClientNetworkChannel channel;
    private string lastMessage = "";

    public ClientNetwork(ICoreClientAPI api)
    {
        this.api = api;
        channel = api.Network.RegisterChannel("jeff.waypointtogetherreborn");
        channel.RegisterMessageType<ShareWaypointPacket>();
        channel.SetMessageHandler<ShareWaypointPacket>(HandlePacket);
    }

    // Pour le Add (sans position)
    public void ShareWaypoint(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        channel.SendPacket(new ShareWaypointPacket(message, api.World.Player.PlayerUID));
    }

    // Pour le Edit (avec position)
    public void ShareWaypoint(string message, Vec3d pos)
    {
        if (string.IsNullOrEmpty(message)) return;
        if (pos == null)
        {
            ShareWaypoint(message);
            return;
        }
        channel.SendPacket(new ShareWaypointPacket(message, api.World.Player.PlayerUID, pos.X, pos.Y, pos.Z));
    }

    private void HandlePacket(ShareWaypointPacket packet)
    {
        if (lastMessage == packet.Message) return;
        lastMessage = packet.Message;

        if (!packet.Message.StartsWith("/waypoint modify"))
        {
            api.SendChatMessage(packet.Message);
            return;
        }

        // Edit : cherche le waypoint par position
        string[] split = packet.Message.Split(' ');
        string color = split[3];
        string icon = split[4];
        string pinned = split[5];
        string name = string.Join(' ', split.Skip(6));

        var maplayers = api.ModLoader.GetModSystem<WorldMapManager>().MapLayers;
        var waypointLayer = maplayers.Find(x => x is WaypointMapLayer) as WaypointMapLayer;
        if (waypointLayer == null) return;

        int myExistingId = -1;
        if (waypointLayer.Waypoints != null)
        {
            myExistingId = waypointLayer.Waypoints.FindIndex(x =>
                x.Position != null &&
                x.Position.X == packet.PosX &&
                x.Position.Y == packet.PosY &&
                x.Position.Z == packet.PosZ);
        }

        if (myExistingId != -1)
        {
            api.SendChatMessage($"/waypoint modify {myExistingId} {color} {icon} {pinned} {name}");
        }
        else
        {
            double x = packet.PosX - (api.World.BlockAccessor.MapSizeX / 2);
            double y = packet.PosY;
            double z = packet.PosZ - (api.World.BlockAccessor.MapSizeZ / 2);
            api.SendChatMessage($"/waypoint addati {icon} {x} {y} {z} {pinned} {color} {name}");
        }
    }
}