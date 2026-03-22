using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ultraconyx.Content.Projectiles;

namespace Ultraconyx.Content.Items.Weapons.Summoner;

public class SlimeWhip : ModItem
{
    public override void SetStaticDefaults()
    {
        // Tooltip
        // Tooltip.SetDefault("Chance to ignite enemies on hit\nCreates slime trails that can be ignited\n'Sticky and flammable!'");

        // Required for whips
        ItemID.Sets.ToolTipDamageMultiplier[Type] = 1f;
    }

    public override void SetDefaults()
    {
        // Basic properties
        Item.width = 40;
        Item.height = 32;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.shootSpeed = 8f;
        Item.knockBack = 1f;
        Item.damage = 12;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(copper: 50);

        // Weapon properties
        Item.DamageType = DamageClass.SummonMeleeSpeed;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.autoReuse = false;

        // Whip properties
        Item.shoot = ModContent.ProjectileType<SlimeWhipProj>();

        // Make it a summon weapon
        Item.UseSound = SoundID.Item152; // Whip sound
    }

    public override bool MeleePrefix()
    {
        return true; // Can have prefixes
    }

    public override void AddRecipes()
    {
        // Simple recipe
        CreateRecipe()
            .AddIngredient(ItemID.Gel, 25)
            .AddIngredient(ItemID.Chain, 8)
            .AddTile(TileID.Anvils)
            .Register();
    }
}