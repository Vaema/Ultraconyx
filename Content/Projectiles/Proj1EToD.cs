using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Ultraconyx.Content.Projectiles;

public class Proj1EToD : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.penetrate = 3;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.timeLeft = 300;

        // DON'T use the bullet ai - we'll control rotation ourselves
        Projectile.aiStyle = 0;
        // AIType is not set; custom AI below
        Projectile.extraUpdates = 0; // optional: set >0 for smoother movement
        Projectile.tileCollide = true;
    }

    public override void AI()
    {
        // Ensure sprite faces the velocity direction + 90 degrees
        // (use PiOver2 for +90°; try -PiOver2 if your sprite still offset)
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        // Fix sprite flipping for left/right facing sprites
        if (Projectile.velocity.X > 0f)
        {
            Projectile.spriteDirection = 1;
            Projectile.direction = 1;
        }
        else if (Projectile.velocity.X < 0f)
        {
            Projectile.spriteDirection = -1;
            Projectile.direction = -1;
        }

        // Water Bolt dust (ID 27)
        int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ice);
        if (dustIndex >= 0 && dustIndex < Main.maxDust)
        {
            Main.dust[dustIndex].noGravity = true;
            Main.dust[dustIndex].scale = 1.1f;
            // Slightly follow the projectile
            Main.dust[dustIndex].velocity *= 0.2f;
        }
    }
}
