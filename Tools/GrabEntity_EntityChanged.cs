using System.IO;
using MasterHand.Players;
using Terraria;
using WebmilioCommons;
using WebmilioCommons.Commons;
using WebmilioCommons.Extensions;
using WebmilioCommons.Networking.Packets;

namespace MasterHand.Tools;

public class GrabEntity_EntityChanged : ModPlayerNetworkPacket<PuppetMaster>
{
    public GrabEntity_EntityChanged()
    {
    }

    public GrabEntity_EntityChanged(Entity entity)
    {
        if (entity == null)
        {
            Type = EntityType.NPC;
            Who = -1;
        }
        else
        {
            Type = entity.GetEntityType();
            Who = entity.whoAmI;
        }
    }

    protected override bool PostReceive(BinaryReader reader, int fromWho)
    {
        var tool = ModPlayer.Tool as GrabEntity;

        if (Who == -1)
            tool.Drop();
        else
            tool.Grab(Type.GetMainEntities()[Who]);

        return base.PostReceive(reader, fromWho);
    }

    [NotMapped]
    public EntityType Type { get; set; }

    public byte TypeId
    {
        get => (byte)Type;
        set => Type = (EntityType)value;
    }
    public int Who { get; set; }
}