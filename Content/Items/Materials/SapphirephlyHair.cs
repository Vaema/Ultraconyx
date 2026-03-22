using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Items.Materials;

public class SapphirephlyHair : ModItem
{
    public override void SetStaticDefaults()
    {
        // How many are needed to fully research in Journey Mode
        Item.ResearchUnlockCount = 25;
    }

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
        Item.maxStack = 9999;
        Item.value = Item.buyPrice(silver: 5);
        Item.rare = ItemRarityID.Blue; // Early-game rarity
        Item.material = true; // Marks it as a crafting material
    }
}
