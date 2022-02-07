using System;
using System.IO;
using MasterHand.Players;
using Microsoft.Xna.Framework;
using WebmilioCommons.Networking.Packets;

namespace MasterHand.Tools;

public class GrabEntity_Sync : ModPlayerNetworkPacket<PuppetMaster>
{
    protected override bool PreReceive(BinaryReader reader, int fromWho)
    {
        return ModPlayer.Tool is GrabEntity;
    }

    public GrabEntity Tool => (GrabEntity)ModPlayer.Tool;

    public byte HandLookingDirection
    {
        get => Tool.LookingDirection;
        set => Tool.LookingDirection = value;
    }

    public Vector2 Position
    {
        get => Tool.Position;
        set => Tool.Position = value;
    }
}