using System;
using MasterHand.Players;
using Terraria;
using Terraria.ModLoader;

namespace MasterHand.Items;

public abstract class ToolItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
    }

    public override bool? UseItem(Player player)
    {
        return PuppetMaster.Get(player).Tool.UseItem();
    }

    public override void HoldItem(Player player)
    {
        PuppetMaster.Get(player).ToolType = ToolType;
    }

    public abstract Type ToolType { get; }
}