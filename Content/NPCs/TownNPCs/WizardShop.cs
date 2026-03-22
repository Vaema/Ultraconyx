using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.NPCs.TownNPCs;

public class WizardShop : GlobalNPC
{
    public override void ModifyShop(NPCShop shop)
    {
        if (shop.NpcType == NPCID.Wizard)
        {
            // Add Auri's Hat after defeating Wall of Flesh
            shop.Add<Items.Materials.AuriHat>(
                new Condition("Downed Wall of Flesh", () => NPC.downedBoss3)
            );
        }
    }
}