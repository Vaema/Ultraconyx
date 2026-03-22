using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Items.Materials;

public class AuriHat : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1; 
    }

    public override void SetDefaults()
    {
        Item.width = 28; // Visual width in pixels
        Item.height = 24; // Visual height in pixels
        Item.maxStack = 9999; // Maximum stack size
        Item.value = Item.sellPrice(gold: 2, silver: 50); // Sell price: 2 gold 50 silver
        Item.rare = ItemRarityID.Pink; // Post-WoF appropriate rarity
    }
}