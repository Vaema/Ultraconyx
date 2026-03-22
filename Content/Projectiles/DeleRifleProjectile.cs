using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Projectiles;

public class DeleRifleProjectile : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 8;
        Projectile.height = 8;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 600;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = true;
        Projectile.extraUpdates = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI()
    {
        // Rotate projectile to match velocity
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        // Create Topaz dust trail
        if (Main.rand.NextBool(3))
        {
            Dust dust = Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.GemTopaz,
                Projectile.velocity.X * 0.2f,
                Projectile.velocity.Y * 0.2f,
                100,
                default(Color),
                1.2f
            );
            dust.noGravity = true;
            dust.velocity *= 0.3f;
        }

        // Add light
        Lighting.AddLight(Projectile.Center, 1f, 0.85f, 0f); // Topaz color
    }

    public override void OnKill(int timeLeft)
    {
        // Create death effect
        for (int i = 0; i < 10; i++)
        {
            Dust dust = Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.GemTopaz,
                Projectile.velocity.X * 0.1f,
                Projectile.velocity.Y * 0.1f,
                100,
                default(Color),
                1f
            );
            dust.velocity *= 0.9f;
        }
    }
}