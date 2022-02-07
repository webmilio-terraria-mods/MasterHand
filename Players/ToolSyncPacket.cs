using System;
using WebmilioCommons;
using WebmilioCommons.Networking.Packets;

namespace MasterHand.Players;

public class ToolSyncPacket : ModPlayerNetworkPacket<PuppetMaster>
{
    public ToolSyncPacket()
    {
    }

    public string ToolName
    {
        get => ToolType.FullName;
        set => ToolType = Type.GetType(value);
    }

    [NotMapped]
    public Type ToolType
    {
        get => ModPlayer.ToolType;
        set => ModPlayer.ToolType = value;
    }
}