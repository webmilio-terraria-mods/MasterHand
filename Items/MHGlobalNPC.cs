using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace MasterHand.Items;

public class MHGlobalNPC : GlobalNPC
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type != NPCID.SkeletronHand)
            return;

        npcLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<MasterHand>(), 100, 0, 1,
            new DaBababooeyDrop()));
    }

    private class DaBababooeyDrop : IItemDropRuleCondition
    {
        public string GetConditionDescription()
        {
            return "Only drops after the Moon Lord is defeated.";
        }

        public bool CanDrop(DropAttemptInfo info)
        {
            return NPC.downedMoonlord;
        }

        public bool CanShowItemDropInUI()
        {
            return true;
        }
    }
}