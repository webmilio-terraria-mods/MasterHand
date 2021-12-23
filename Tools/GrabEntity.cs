using System;
using System.Collections.Generic;
using MasterHand.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using WebmilioCommons.Extensions;
using WebmilioCommons.Tinq;

namespace MasterHand.Tools;

public class GrabEntity : Tool
{
    private int _closedDirection;

    private Entity[][] _entities = 
    {
        Main.player,
        Main.npc,
        Main.projectile,
        Main.item
    };

    private bool _mouseLeft;
    private Vector2 _previousMouseWorld;

    private readonly GameInterfaceLayer _cursorLayer;

    public GrabEntity()
    {
        _cursorLayer = new HandCursorLayer(this);
    }

    private bool Grab()
    {
        Entity entity = null;

        var cursor = Owner.Mod.Assets.Request<Texture2D>(Items.MasterHand.HandEmpty).Value;
        Rectangle container = new((int)Main.MouseWorld.X - cursor.Width / 2, (int)Main.MouseWorld.Y - cursor.Height / 2, cursor.Width, cursor.Height);

        for (int i = 0; i < _entities.Length && entity == null; i++)
        {
            entity = _entities[i].LastActiveOrDefault(e => container.Contains(e.Center.ToPoint()));
        }

        if (entity == null)
            return false;

        Entity = entity;

        if (Entity != Owner.Player)
            Entity.active = false;

        return true;
    }

    private void Drop()
    {
        if (Empty)
            return;

        Entity.velocity = (Main.MouseWorld - _previousMouseWorld) / 2;
        Entity.active = true;
        Entity = null;
    }

    public override void PreUpdate()
    {
        if (Empty)
            return;

        Main.cursorAlpha = 0;

        if (Entity is Player p)
            p.noFallDmg = true;

        Entity.Center = Main.MouseWorld;
        Entity.velocity = Vector2.Zero;
    }

    public override void PostUpdate()
    {
        _previousMouseWorld = Main.MouseWorld;
    }

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        _prevMouseLeft = _mouseLeft;
        _mouseLeft = triggersSet.MouseLeft;

        if (_mouseLeft)
        {
            if (Grabbed)
                _closedDirection = GetRelativeDirection();

            if (Empty && !Closed)
                if (!Grab())
                    _closedDirection = GetRelativeDirection();
        }
        else
        {
            if (Grabbed)
                Drop();

            _closedDirection = 0;
        }
    }

    public override void Deselect()
    {
        Drop();
    }

    private const string
        CursorLayerName = "Vanilla: Cursor",
        HoverLayerName = "Vanilla: Mouse Over";

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        var cursorIndex = layers.FindIndex(g => g.Name.Equals(CursorLayerName, StringComparison.OrdinalIgnoreCase));
        layers.RemoveAt(cursorIndex);

        // var hoverIndex = layers.FindIndex(g => g.Name.Equals(HoverLayerName, StringComparison.OrdinalIgnoreCase));
        layers.Add(_cursorLayer);
    }

    private int HandDirection()
    {
        return _closedDirection == 0 ? GetRelativeDirection() : _closedDirection;
    }

    private int GetRelativeDirection()
    {
        return Math.Clamp(Owner.Player.position.X.CompareTo(Main.MouseWorld.X), -1, 1);
    }

    public Entity Entity { get; private set; }

    public bool Grabbed => Entity != null;
    public bool Closed => Grabbed || _closedDirection != 0;
    public bool Empty => Entity == null;

    private class HandCursorLayer : GameInterfaceLayer
    {
        private readonly GrabEntity _tool;

        public HandCursorLayer(GrabEntity tool) : base(GrabEntity.CursorLayerName, InterfaceScaleType.UI)
        {
            _tool = tool;
        }

        protected override bool DrawSelf()
        {
            var hand = _tool.Owner.Mod.Assets.Request<Texture2D>(_tool.Closed ? Items.MasterHand.HandFull : Items.MasterHand.HandEmpty).Value;

            Main.spriteBatch.Draw(hand, Main.MouseScreen,
                null, Color.White, 0, new(hand.Width / 2, hand.Height / 2), 1,
                _tool.HandDirection() == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            return true;
        }
    }
}