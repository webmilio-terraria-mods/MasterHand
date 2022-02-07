using System;
using System.Collections.Generic;
using MasterHand.Items;
using MasterHand.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using WebmilioCommons.Extensions;
using WebmilioCommons.Helpers;
using WebmilioCommons.Tinq;

namespace MasterHand.Tools;

public class GrabEntity : Tool
{
    public const int Width = 66, Height = 46;

    private Entity[][] _entities =
    {
        Main.player,
        Main.npc,
        Main.projectile,
        Main.item
    };

    private bool _mouseLeft;
    private Vector2[] _mouseBuffer = new Vector2[2];

    private Rectangle _container;

    private readonly GameInterfaceLayer _cursorLayer;

    public GrabEntity(PuppetMaster owner) : base(owner)
    {
        _cursorLayer = new HandCursorLayer(this);
    }

    private bool Grab()
    {
        Entity entity = null;

        for (int i = 0; i < _entities.Length && entity == null; i++)
        {
            entity = _entities[i].LastActiveOrDefault(e => _container.Contains(e.Center.ToPoint()));
        }

        if (entity == null)
            return false;

        Grab(entity);
        return true;
    }

    public void Grab(Entity entity)
    {
        Entity = entity;

        if (Entity != Owner.Player)
            Entity.active = false;
    }

    public void Drop()
    {
        if (Empty)
            return;

        if (Entity is Projectile p)
        {
            p.damage += (int)(MouseVelocity.Length() * .66f);
            p.friendly = true;
            p.hostile = true;
        }

        Entity.velocity = MouseVelocity / 2;
        Entity.active = true;
        Entity = null;
    }

    #region Hooks

    public override void PreUpdate()
    {
        if (Owner.IsLocalPlayer())
            Position = Main.MouseWorld;

        _container = new((int)Position.X - Width / 2, (int)Position.Y - Height / 2, Width, Height);

        if (Empty)
        {
            if (Closed)
            {
                var velocity = MouseVelocity;

                var absX = MathF.Abs(velocity.X);
                var absY = MathF.Abs(velocity.Y);
                var stomp = velocity.Y > 0 && absY * 2 > absX && absX < 10;

                if (!stomp &&
                    (MathF.Abs(velocity.X) < 10 ||
                     velocity.X > 0 && LookingDirection > 0 ||
                     velocity.X < 0 && LookingDirection < 0))
                {
                    return; // THE CART'S GOING THE WRONG WAY!!!
                }

                Punch(velocity);
            }
        }
        else
        {
            Main.cursorAlpha = 0;

            if (Entity is Player p)
                p.noFallDmg = true;

            Entity.Center = Position;
            Entity.velocity = Vector2.Zero;
        }

        Owner.SendIfLocal<GrabEntity_Sync>();
    }

    private void Punch(Vector2 velocity)
    {
        // Paounch
        var damage = velocity.Length();

        _entities.Do(delegate (Entity[] entities)
        {
            entities.WhereActive(entity => _container.Intersects(entity.Hitbox))
                .Do(delegate (Entity entity)
                {
                    if (entity is NPC npc)
                    {
                        npc.StrikeNPC((int)damage, 0, -LookingDirection, Main.rand.Next(0, 100) < 15);
                        npc.velocity += (npc.knockBackResist + 0.1f) * velocity * .33f;
                    }
                    else if (entity is Player player)
                    {
                        player.Hurt(PlayerDeathReason.ByPlayer(Owner.Player.whoAmI), (int)damage, -LookingDirection);

                        if (!player.noKnockback)
                            player.velocity += velocity * .20f;
                    }
                    else if (entity is Projectile projectile)
                    {
                        projectile.velocity += velocity;
                        projectile.hostile = true;
                        projectile.friendly = true;
                    }
                });
        });
    }

    public override void PostUpdate()
    {
        _mouseBuffer[1] = _mouseBuffer[0];
        _mouseBuffer[0] = Position;
    }

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        _mouseLeft = triggersSet.MouseLeft;

        if (_mouseLeft)
        {
            if (Full)
                LookingDirection = GetRelativeDirection();

            if (Empty && !Closed)
                if (!Grab())
                    LookingDirection = GetRelativeDirection();
        }
        else
        {
            if (Full)
                Drop();

            LookingDirection = 0;
        }
    }

    public override void Deselect()
    {
        Drop();
    }

    #endregion

    #region UI

    private const string
        CursorLayerName = "Vanilla: Cursor",
        HoverLayerName = "Vanilla: Mouse Over";

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (Owner.IsLocalPlayer())
        {
            var cursorIndex = layers.Find(g => g.Name.Equals(CursorLayerName, StringComparison.OrdinalIgnoreCase));
            layers.Remove(cursorIndex);

            // var hoverIndex = layers.FindIndex(g => g.Name.Equals(HoverLayerName, StringComparison.OrdinalIgnoreCase));
            layers.Add(_cursorLayer);
        }
        else
        {
            var il4 = layers.FindIndex(g => g.Name.Equals(HoverLayerName, StringComparison.OrdinalIgnoreCase));
            layers.Insert(il4, _cursorLayer);
        }
    }

    private int HandDirection()
    {
        if (LookingDirection != 0)
            return LookingDirection;

        return GetRelativeDirection();
    }

    private byte GetRelativeDirection()
    {
        return (byte) Math.Clamp(Owner.Player.position.X.CompareTo(Position.X), -1, 1);
    }

    #endregion

    private Entity _entity;

    public Entity Entity
    {
        get => _entity;
        set
        {
            if (_entity == value)
                return;

            _entity = value;
            Owner.SendIfLocal(new GrabEntity_EntityChanged(value));
        }
    }

    public Vector2 MouseVelocity => Position - _mouseBuffer[1];
    public Vector2 Position { get; set; }

    public bool Empty => Entity == null;
    public bool Full => !Empty;

    public byte LookingDirection { get; set; }
    public bool Closed => Full || LookingDirection != 0;

    private class HandCursorLayer : GameInterfaceLayer
    {
        private readonly GrabEntity _tool;
        private readonly Asset<Texture2D> _cursor;

        private const int
            FrameCols = 3,
            FrameRows = 2,
            FrameTime = 5,
            Padding = 2,
            FullWidth = (Width + Padding) * FrameCols,
            FullHeight = (Height + Padding) * FrameRows,

            XOffset = Width + Padding,
            YOffset = Height + Padding;

        private int _frameTimer;
        private Rectangle _source = new(0, 0, Width, Height);

        public HandCursorLayer(GrabEntity tool) : base(GrabEntity.CursorLayerName, InterfaceScaleType.UI)
        {
            _tool = tool;
            _cursor = _tool.Owner.Mod.Assets.Request<Texture2D>(@"Tools\GrabEntity_Cursor");
        }

        protected override bool DrawSelf()
        {
            void OnFrame(Predicate<Rectangle> condition, Action action)
            {
                if (!condition(_source))
                    return;

                if (_frameTimer < 2)
                {
                    _frameTimer++;
                }
                else
                {
                    action();
                    _frameTimer = 0;
                }
            }

            if (_tool.Closed)
            {
                if (_tool.Full)
                {
                    _source.Y = 0;

                    OnFrame(
                        r => r.Right < FullWidth - Padding,
                        () => _source.Offset(XOffset, 0));
                }
                else
                {
                    OnFrame(r => r.Right < FullWidth - Padding,
                        delegate
                        {
                            if (_source.Y == 0)
                            {
                                _source.Offset(0, YOffset);
                            }
                            else
                            {
                                _source.Offset(XOffset, 0);
                            }
                        });
                }
            }
            else
            {
                OnFrame(
                    r => r.X > 0,
                    () => _source.Offset(-XOffset, 0));

                OnFrame(
                    r => r.Y > 0 && r.X == 0,
                    () => _source.Offset(0, -YOffset));
            }

            Main.spriteBatch.Draw(_cursor.Value, _tool.Position.ToScreenPosition(),
                _source, Color.White, 0, new(Width / 2, Height / 2), 1,
                _tool.HandDirection() == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

            return true;
        }
    }
}