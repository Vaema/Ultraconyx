using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Items.Misc;

public class Delefishtuno : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.width = 34;
        Item.height = 34;
        Item.maxStack = 1;
        Item.value = Item.sellPrice(gold: 1);
        Item.rare = ItemRarityID.Yellow;
        Item.consumable = true;
        Item.questItem = true;
        Item.bait = 0;
        Item.makeNPC = (short)NPCID.Angler;
    }
    public override bool IsQuestFish() => true;
    public override bool IsAnglerQuestAvailable() => Main.dayTime;
    public override void AnglerQuestChat(ref string description, ref string catchLocation)
    {
        description = "I feel yellow today. I saw a yellow fish wandering on the lake the other day. I want you to catch it and give it to me. Chop chop, the fish won't catch itself!";
        catchLocation = "Found on the surface at exactly midnight.";
    }
}