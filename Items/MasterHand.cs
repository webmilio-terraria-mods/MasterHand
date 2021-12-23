using System;
using MasterHand.Players;
using MasterHand.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace MasterHand.Items;

public class MasterHand : ToolItem
{
    private Asset<Texture2D> _full;

    internal const string
        HandEmpty = @"Items\" + nameof(MasterHand),
        HandFull = HandEmpty + "_Closed";

    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.width = 66;
        Item.height = 46;

        _full = Mod.Assets.Request<Texture2D>(HandFull);
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (PuppetMaster.LocalPlayer.Tool is GrabEntity ge && ge.Entity != null)
        {
            spriteBatch.Draw(_full.Value, position, frame, drawColor, 0, origin, scale, SpriteEffects.None, 0);
            return false;
        }
        
        return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }

    public override Type ToolType { get; } = typeof(GrabEntity);
}