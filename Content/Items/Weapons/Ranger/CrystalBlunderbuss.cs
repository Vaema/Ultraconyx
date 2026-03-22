using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Items.Weapons.Ranger;

public class CrystalBlunderbuss : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.damage = 45;
        Item.DamageType = DamageClass.Ranged;
        Item.width = 56;
        Item.height = 24;
        Item.useTime = 23;
        Item.useAnimation = 23;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 4f;
        Item.value = Item.sellPrice(0, 7, 50, 0);
        Item.rare = ItemRarityID.Pink;
        Item.UseSound = SoundID.Item36;
        Item.autoReuse = true;
        Item.shoot = ProjectileID.CrystalBullet;
        Item.shootSpeed = 12f;
        Item.useAmmo = AmmoID.Bullet;
        Item.crit = 4;
    }

    public override Vector2? HoldoutOffset()
    {
        return new Vector2(-2f, 0f);
    }

    // FOR tModLoader 1.4.4+ (Newer version)
    // This is the correct signature for newer tModLoader
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        // Convert musket balls to crystal bullets
        if (type == ProjectileID.Bullet)
        {
            type = ProjectileID.CrystalBullet;
        }

        // First shot
        Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(10));
        Projectile.NewProjectile(source, position, perturbedSpeed, type, damage, knockback, player.whoAmI);

        // Second shot with opposite spread
        Vector2 perturbedSpeed2 = velocity.RotatedByRandom(MathHelper.ToRadians(20));
        Projectile.NewProjectile(source, position, perturbedSpeed2, type, damage, knockback, player.whoAmI);

        return false; // Don't fire default projectile
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<Blunderbuss>(), 1)
            .AddIngredient(ItemID.CrystalShard, 50)
            .AddIngredient(ItemID.SoulofLight, 10)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}