using System;
using MasterHand.Players;
using MasterHand.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ID;

namespace MasterHand.Items;

public class MasterHand : ToolItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.width = 66;
        Item.height = 46;

        Item.crit = 15;
        Item.rare = ItemRarityID.Red;
    }

    public override Type ToolType { get; } = typeof(GrabEntity);
}