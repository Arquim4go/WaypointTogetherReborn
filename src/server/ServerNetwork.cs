using Vintagestory.API.Server;
using WaypointTogetherReborn.network.packets;

namespace WaypointTogetherReborn.server;

public class ServerNetwork
{
    private readonly IServerNetworkChannel _channel;

    public ServerNetwork(ICoreServerAPI api)
    {
        _channel = api.Network.RegisterChannel("client.waypointtogetherreborn");
        _channel.RegisterMessageType<ShareWaypointPacket>();
        _channel.SetMessageHandler<ShareWaypointPacket>(HandlePacket);
    }

    private void HandlePacket(IServerPlayer player, ShareWaypointPacket packet)
    {
        _channel.BroadcastPacket(packet, player);
    }
}