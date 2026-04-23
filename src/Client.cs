using Vintagestory.API.Client;
using WaypointTogetherReborn.client;

namespace WaypointTogetherReborn;

public class Client
{
    public readonly ClientNetwork network;

    public Client(ICoreClientAPI api)
    {
        network = new ClientNetwork(api);
    }
}
