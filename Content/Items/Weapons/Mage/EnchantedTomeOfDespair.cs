using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Ultraconyx.Content.Projectiles;

namespace Ultraconyx.Content.Items.Weapons.Mage;

public class EnchantedTomeOfDespair : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 40;
        Item.DamageType = DamageClass.Magic;
        Item.useTime = 23;
        Item.useAnimation = 23;
        Item.mana = 5;
        Item.width = 28;
        Item.height = 30;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 3f;
        Item.value = Item.buyPrice(0, 1, 50, 0);
        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item20;
        Item.shoot = ModContent.ProjectileType<Proj1EToD>();
        Item.shootSpeed = 12f;
        Item.autoReuse = true;
    }

    public override bool Shoot(
        Player player,
        Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
        Vector2 position,
        Vector2 velocity,
        int type,
        int damage,
        float knockBack)
    {
        // Straight line projectile
        Projectile.NewProjectile(
            source,
            position,
            velocity,
            ModContent.ProjectileType<Proj1EToD>(),
            damage,
            knockBack,
            player.whoAmI
        );

        // Spread homing projectiles (25% damage)
        int secondaryDamage = (int)(damage * 0.75f);
        for (int i = 0; i < 2; i++)
        {
            Vector2 perturbed = velocity.RotatedByRandom(MathHelper.ToRadians(10));

            Projectile.NewProjectile(
                source,
                position,
                perturbed,
                ModContent.ProjectileType<Proj2EToD>(),
                secondaryDamage,
                knockBack,
                player.whoAmI
            );
        }

        return false;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.SpellTome, 1)
            .AddIngredient(ItemID.WaterBolt, 1)
            .AddIngredient(ItemID.WaterBucket, 5)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
