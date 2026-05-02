using System;
using Vintagestory.API.Common;

namespace WaypointTogetherReborn;

class Config
{
    public bool DefaultSharing { get; set; } = false;
    public Config()
    {
    }
    public Config(Config? previousConfig)
    {
        DefaultSharing = previousConfig?.DefaultSharing ?? false;
    }
}

static class ModConfig
{
    public static Config ClientConfig { get; private set; }

    private const string ClientConfigFile = "WaypointTogetherReborn.json";

    public static void ReadConfig(ICoreAPI api)
    {
        try
        {
            if (api.Side == EnumAppSide.Client)
            {
                ClientConfig = LoadClientConfig(api);
                if (ClientConfig == null)
                {
                    GenerateClientConfig(api);
                    ClientConfig = LoadClientConfig(api);
                }
                else
                {
                    GenerateClientConfig(api, ClientConfig);
                }
            }
        }
        catch (Exception e)
        {
            api.World.Logger.Error("Failed to load config: {0}", e);
            GenerateClientConfig(api);
            ClientConfig = LoadClientConfig(api);
        }
    }

    private static void GenerateClientConfig(ICoreAPI api)
    {
        api.StoreModConfig(new Config(), ClientConfigFile);
    }
    
    private static void GenerateClientConfig(ICoreAPI api, Config previousConfig)
    {
        api.StoreModConfig(new Config(previousConfig), ClientConfigFile);
    }

    private static Config LoadClientConfig(ICoreAPI api)
    {
        return api.LoadModConfig<Config>(ClientConfigFile);
    }


}