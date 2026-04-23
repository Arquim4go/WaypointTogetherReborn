using Vintagestory.API.Server;
using WaypointTogetherReborn.server;

namespace WaypointTogetherReborn;

public class Server
{
    public readonly ServerNetwork network;

    public Server(ICoreServerAPI api)
    {
        network = new ServerNetwork(api);
    }
}
