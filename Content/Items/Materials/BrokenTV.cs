using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Items.Materials;

public class BrokenTV : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 25;
        ItemID.Sets.SortingPriorityMaterials[Type] = 59; // Adjust this value as needed
    }

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 20;
        Item.maxStack = 10;
        Item.value = Item.sellPrice(silver: 15);
        Item.rare = ItemRarityID.White;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.IronBar, 10)
            .AddIngredient(ItemID.Glass, 1)
            .AddTile(TileID.Anvils) // Or change to TileID.WorkBenches if you prefer
            .Register();
    }
}