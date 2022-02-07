using System.IO;
using Terraria.ModLoader;
using WebmilioCommons.Networking;

namespace MasterHand;

public class MasterHand : Mod
{
    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        NetworkPacketLoader.Instance.HandlePacket(reader, whoAmI);
    }
}