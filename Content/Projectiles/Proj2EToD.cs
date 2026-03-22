using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Ultraconyx.Content.Projectiles;

public class Proj2EToD : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.timeLeft = 240;
        Projectile.tileCollide = true;
    }

    public override void AI()
    {
        float homingStrength = 0.15f;
        NPC target = null;
        float maxDetect = 400f;

        // Locate target
        foreach (NPC npc in Main.npc)
        {
            if (npc.CanBeChasedBy() &&
                Vector2.Distance(Projectile.Center, npc.Center) < maxDetect)
            {
                maxDetect = Vector2.Distance(Projectile.Center, npc.Center);
                target = npc;
            }
        }

        // Homing behavior
        if (target != null)
        {
            Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * 12f;
            Projectile.velocity = Vector2.Lerp(
                Projectile.velocity,
                desiredVelocity,
                homingStrength
            );
        }

        // Spin effect
        Projectile.rotation += 0.35f;

        // Water Bolt dust (ID 27)
        int dust = Dust.NewDust(
            Projectile.position, 
            Projectile.width, Projectile.height, 
            80
        );

        Main.dust[dust].noGravity = true;
        Main.dust[dust].scale = 1.15f;
    }
}
