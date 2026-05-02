using ProtoBuf;

namespace WaypointTogetherReborn.network.packets;

[ProtoContract]
public class ShareWaypointPacket
{
    [ProtoMember(1)] public string Message { get; set; } = "";
    [ProtoMember(2)] public string ByPlayerUid { get; set; } = "";
    [ProtoMember(3)] public double PosX { get; set; }
    [ProtoMember(4)] public double PosY { get; set; }
    [ProtoMember(5)] public double PosZ { get; set; }

    public ShareWaypointPacket() {}

    public ShareWaypointPacket(string message, string byPlayerUid)
    {
        Message = message;
        ByPlayerUid = byPlayerUid;
    }

    public ShareWaypointPacket(string message, string byPlayerUid, double posX, double posY, double posZ)
    {
        Message = message;
        ByPlayerUid = byPlayerUid;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
    }
}