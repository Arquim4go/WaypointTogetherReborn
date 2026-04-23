using ProtoBuf;
using Vintagestory.GameContent;

namespace WaypointTogetherReborn.network.packets;

[ProtoContract]
internal class ShareWaypointPacket(string message, string waypointGuid)
{
    [ProtoMember(1)]
    public string Message { get; set; } = message;

    [ProtoMember(2)]
    public Waypoint Waypoint { get; set; }

    public ShareWaypointPacket() : this("", null) {}
}