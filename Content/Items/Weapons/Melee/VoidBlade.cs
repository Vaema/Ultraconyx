using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using System.Collections.Generic;
using Ultraconyx.Content.Projectiles;

namespace Ultraconyx.Content.Items.Weapons.Melee;

public class VoidBlade : ModItem
{
    public override void SetDefaults()
    {
        // Weapon properties
        Item.damage = 250;
        Item.DamageType = DamageClass.Melee;
        Item.width = 64;
        Item.height = 64;
        Item.useTime = 25;
        Item.useAnimation = 25;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 8f;
        Item.value = Item.sellPrice(gold: 15);
        Item.rare = ItemRarityID.Purple;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<VoidBladeProjectile>();
        Item.shootSpeed = 18f;
        Item.noUseGraphic = true; // Don't draw the item sprite in hand
        Item.noMelee = true; // Don't do damage with the item itself
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        // Spawn the projectile
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        return false; // Return false to prevent multiple projectiles
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.LunarBar, 12)
            .AddIngredient(ItemID.FragmentSolar, 8)
            .AddIngredient(ItemID.FragmentVortex, 8)
            .AddIngredient(ItemID.FragmentNebula, 8)
            .AddIngredient(ItemID.FragmentStardust, 8)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
    }

    // Optional: Add tooltip
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        tooltips.Add(new TooltipLine(Mod, "VoidBlade", "Throws a void-infused blade that sticks to enemies"));
        tooltips.Add(new TooltipLine(Mod, "VoidBlade2", "Deals damage over time while embedded"));
    }
}