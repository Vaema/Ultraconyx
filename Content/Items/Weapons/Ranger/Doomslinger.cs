using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace Ultraconyx.Content.Items.Weapons.Ranger;

public class Doomslinger : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.damage = 40;
        Item.DamageType = DamageClass.Ranged;
        Item.width = 40;
        Item.height = 20;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 4f;
        Item.value = Item.sellPrice(0, 5, 0, 0);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = SoundID.Item41;
        Item.autoReuse = true;
        Item.shoot = ProjectileID.ExplosiveBullet;
        Item.shootSpeed = 16f;
        Item.useAmmo = AmmoID.Bullet;
        Item.crit = 5;
    }

    public override Vector2? HoldoutOffset()
    {
        return new Vector2(-4f, 0f);
    }

    // Makes regular bullets become exploding bullets
    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        if (type == ProjectileID.Bullet)
        {
            type = ProjectileID.ExplosiveBullet;
        }

        // Offset the spawn position upward by 4 pixels
        // Adjust the Y value (negative = upward, positive = downward)
        position.Y -= 8f;

        // You could also offset based on player direction:
        // if (player.direction == 1) // facing right
        //     position.X += 2f;
        // else // facing left
        //     position.X -= 2f;
    }

    // Spawn the projectile and attach dust trail behavior
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        // Create the projectile
        int projIndex = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        Projectile proj = Main.projectile[projIndex];

        return false; // We already spawned the projectile
    }
}

// Add a GlobalProjectile to make explosive bullets spawn lava dust
public class DoomslingerGlobalProjectile : GlobalProjectile
{
    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
    {
        // Only apply to explosive bullets
        return entity.type == ProjectileID.ExplosiveBullet;
    }

    public override void AI(Projectile projectile)
    {
        // Spawn lava dust behind the projectile
        if (Main.rand.NextBool(3)) // 33% chance each frame
        {
            Vector2 dustPosition = projectile.Center - projectile.velocity.SafeNormalize(Vector2.Zero) * 10f;
            Dust dust = Dust.NewDustPerfect(dustPosition, DustID.Lava,
                Vector2.Zero, 0, default, 1.2f);
            dust.noGravity = true;
            dust.velocity = projectile.velocity * 0.1f;
        }
    }
}