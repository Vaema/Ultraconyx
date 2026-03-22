using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Items.Weapons.Melee;

public class PoopEnator3000 : ModItem
{
    public override void SetStaticDefaults()
    {
        /* Tooltip.SetDefault(
            "Spawns poo projectile around the player\n" +
            "what the shit?"
        ); */
    }

    public override void SetDefaults()
    {
        Item.damage = 120;
        Item.DamageType = DamageClass.Melee;
        Item.width = 56;
        Item.height = 56;
        Item.useTime = 14;
        Item.useAnimation = 14;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 6f;
        Item.value = Item.buyPrice(0, 10);
        Item.rare = ItemRarityID.Red;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;
    }

    public override Nullable<bool> UseItem(Player player)/* tModPorter Suggestion: Return null instead of false */
    {
        if (player.whoAmI == Main.myPlayer)
        {
            const int projectileCount = 4;
            float radius = 48f;

            for (int i = 0; i < projectileCount; i++)
            {
                float angle = MathHelper.TwoPi / projectileCount * i;
                Vector2 spawnPos = player.Center + angle.ToRotationVector2() * radius;

                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    spawnPos,
                    Vector2.Zero,
                    ModContent.ProjectileType<Projectiles.ShitProjectile>(),
                    Item.damage,
                    Item.knockBack,
                    player.whoAmI
                );
            }
        }

        return true;
    }

    public override void AddRecipes()
    {
        // Gold Broadsword recipe
        CreateRecipe()
            .AddIngredient(ItemID.PoopBlock, 67)
            .AddIngredient(ItemID.GoldBroadsword)
            .AddIngredient(ItemID.SoulofNight, 21)
            .AddTile(TileID.MythrilAnvil)
            .Register();

        // Platinum Broadsword recipe
        CreateRecipe()
            .AddIngredient(ItemID.PoopBlock, 67)
            .AddIngredient(ItemID.PlatinumBroadsword)
            .AddIngredient(ItemID.SoulofNight, 21)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
