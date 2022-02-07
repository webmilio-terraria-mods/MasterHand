using System.Collections.Generic;
using MasterHand.Players;
using Terraria;
using Terraria.GameInput;
using Terraria.UI;

namespace MasterHand.Tools;

public abstract class Tool
{
    protected Tool(PuppetMaster owner)
    {
        Owner = owner;
    }

    public virtual bool? UseItem() => null;

    public virtual void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) { }

    public virtual void PreUpdate() { }
    public virtual void ProcessTriggers(TriggersSet triggersSet) { }
    public virtual void PostUpdate() { }

    public virtual void Deselect() { }
    public virtual void Select() { }

    public PuppetMaster Owner { get; }
}