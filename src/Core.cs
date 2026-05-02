using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace WaypointTogetherReborn;

public class Core : ModSystem
{
    public Client client;
    public Server server;

    public Patcher patcher;

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        ModConfig.ReadConfig(api);
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);

        client = new Client(api);
        
        patcher = new Patcher("waypointtogetherreborn");
        patcher.PatchAll(api);
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);

        server = new Server(api);
    }

    public override void Dispose()
    {
        patcher?.Dispose();

        base.Dispose();
    }
}
