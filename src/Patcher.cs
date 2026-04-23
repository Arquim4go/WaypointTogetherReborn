using System;
using HarmonyLib;
using Vintagestory.API.Common;

namespace WaypointTogetherReborn;

public class Patcher
{
    private readonly Harmony instance;

    public Patcher(string id)
    {
        instance = new Harmony(id);
    }

    public void PatchAll(ICoreAPI api)
    {
        try
        {
            instance.PatchAll();
        }
        catch (Exception e)
        {
            api.Logger.Error(e.ToString());
            if (e.InnerException != null) api.Logger.Error(e.InnerException.ToString());
            throw;
        }
    }

    public void Dispose()
    {
        instance.UnpatchAll();
    }
}