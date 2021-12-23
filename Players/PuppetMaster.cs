using System;
using System.Collections.Generic;
using MasterHand.Items;
using MasterHand.Tools;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using WebmilioCommons.Players;

namespace MasterHand.Players;

public class PuppetMaster : BetterModPlayer
{
    private Dictionary<Type, Tool> _tools;

    private static readonly Type _defaultTool = typeof(None);
    private Type _toolType;

    public override void Initialize()
    {
        _tools = new();

        CreateTool(typeof(None));
        CreateTool(typeof(GrabEntity));

        ToolType = typeof(None);
    }

    private Tool CreateTool(Type type)
    {
        var tool = Activator.CreateInstance(type) as Tool;
        tool.Owner = this;

        _tools.Add(type, tool);
        return tool;
    }

    public override void PreUpdate()
    {
        if (Player.HeldItem != null && Player.HeldItem.ModItem is ToolItem ti)
            ToolType = ti.ToolType;
        else
            ToolType = _defaultTool;

        Tool.PreUpdate();
    }

    public override void ProcessTriggers(TriggersSet triggersSet) => Tool.ProcessTriggers(triggersSet);
    public override void PostUpdate() => Tool.PostUpdate();

    public Type ToolType
    {
        get => _toolType;
        set
        {
            if (_toolType == value)
                return;

            Tool?.Deselect();

            Tool = _tools[value];
            _toolType = value;
            
            Tool.Select();
        }
    }
    public Tool Tool { get; private set; }

    public static PuppetMaster Get(Player player) => player.GetModPlayer<PuppetMaster>();
    public static PuppetMaster LocalPlayer => Get(Main.LocalPlayer);
}

internal class ManagerSystem : ModSystem
{
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        PuppetMaster.LocalPlayer.Tool.ModifyInterfaceLayers(layers);
    }
}