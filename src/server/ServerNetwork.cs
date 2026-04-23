using ProtoBuf;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using WaypointTogetherReborn.network.packets;

namespace WaypointTogetherReborn.server;

public class ServerNetwork
{
    private readonly IServerNetworkChannel _channel;
    private readonly ICoreServerAPI _api;

    public ServerNetwork(ICoreServerAPI api)
    {
        _channel = api.Network.RegisterChannel("malin.waypointtogethercontinued");
        _channel.RegisterMessageType<ShareWaypointPacket>();
        _channel.RegisterMessageType<ShareWaypointPacketFromServer>();
        _channel.SetMessageHandler<ShareWaypointPacket>(this.HandlePacket);
        this._api = api;
    }

    public void ShareWaypoint(string message, string byPlayer)
    {
        if (message != null && message != "")
        {
            _channel.SendPacket(new ShareWaypointPacket(message, byPlayer));
        }
    }

    private void HandlePacket(IServerPlayer player, ShareWaypointPacket packet)
    {
        var maplayers = _api.ModLoader.GetModSystem<WorldMapManager>().MapLayers;
        var waypointLayer = (maplayers.Find(x => x is WaypointMapLayer) as WaypointMapLayer);
        Waypoint existing = waypointLayer.Waypoints.Find(x => x.Guid == packet.Waypoint.Guid);
        var newPacket = new ShareWaypointPacketFromServer(packet.Message, existing);
        _channel.BroadcastPacket(newPacket, player);
    }
}

[ProtoContract]
public class ShareWaypointPacketFromServer
{
    [ProtoMember(1)]
    public string Message { get; set; }

    [ProtoMember(2)]
    public Waypoint ExistingWaypoint { get; set; }

    public ShareWaypointPacketFromServer()
    {
        Message = "";
    }

    public ShareWaypointPacketFromServer(string message, Waypoint existingWaypoint)
    {
        Message = message;
        ExistingWaypoint = existingWaypoint;
    }
}

