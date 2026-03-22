using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ultraconyx.Content.Projectiles;

namespace Ultraconyx.Content.Items.Weapons.Melee;

public class LivingWoodSword : ModItem
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
    }

    public override void SetDefaults()
    {
        Item.damage = 7;
        Item.DamageType = DamageClass.Melee;
        Item.width = 40;
        Item.height = 40;
        Item.useTime = 25;
        Item.useAnimation = 25;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 5f;
        Item.value = Item.sellPrice(silver: 20);
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;
        Item.useTurn = true;

        Item.shoot = ModContent.ProjectileType<LivingWoodSwordProjectile>();
        Item.shootSpeed = 8f;
        Item.noMelee = false;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddRecipeGroup("Wood", 15)
            .AddIngredient(ItemID.Acorn, 5)
            .AddTile(TileID.LivingLoom)
            .Register();
    }
}