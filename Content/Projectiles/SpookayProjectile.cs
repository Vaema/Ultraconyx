using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Ultraconyx.Content.Dusts;

namespace Ultraconyx.Content.Projectiles;

public class SpookayProjectile : ModProjectile
{
    private int dustSpawnCounter;
    private const int DustSpawnRate = 3;

    public override void SetDefaults()
    {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 300;
        Projectile.ignoreWater = false;
        Projectile.tileCollide = true;
        Projectile.extraUpdates = 1;
        Projectile.alpha = 50;
        Projectile.light = 0.5f;
    }

    public override void AI()
    {
        if (Projectile.timeLeft < 270)
        {
            float maxDetectRadius = 300f;
            float projSpeed = 8f;

            NPC target = FindClosestNPC(maxDetectRadius);
            if (target != null)
            {
                Vector2 direction = target.Center - Projectile.Center;
                direction.Normalize();
                direction *= projSpeed;
                Projectile.velocity = (Projectile.velocity * 20f + direction) / 21f;
            }
        }

        // Make projectile face the direction it's moving
        Projectile.rotation = Projectile.velocity.ToRotation();

        dustSpawnCounter++;
        if (dustSpawnCounter >= DustSpawnRate)
        {
            dustSpawnCounter = 0;

            for (int i = 0; i < 3; i++)
            {
                Vector2 dustPosition = Projectile.Center + Main.rand.NextVector2Circular(10, 10);
                int dust = Dust.NewDust(dustPosition, 2, 2, ModContent.DustType<Spookay>());
                Main.dust[dust].velocity = Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(1, 1);
                Main.dust[dust].scale = Main.rand.NextFloat(0.8f, 1.2f);
                Main.dust[dust].noGravity = true;
            }
        }
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 15; i++)
        {
            int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                ModContent.DustType<Spookay>());
            Main.dust[dust].velocity = Main.rand.NextVector2Circular(5, 5);
            Main.dust[dust].scale = Main.rand.NextFloat(1f, 1.5f);
            Main.dust[dust].noGravity = true;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        for (int i = 0; i < 5; i++)
        {
            int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                ModContent.DustType<Spookay>());
            Main.dust[dust].velocity = oldVelocity * 0.3f;
        }

        if (Projectile.velocity.X != oldVelocity.X)
            Projectile.velocity.X = -oldVelocity.X * 0.5f;
        if (Projectile.velocity.Y != oldVelocity.Y)
            Projectile.velocity.Y = -oldVelocity.Y * 0.5f;

        return false;
    }

    private NPC FindClosestNPC(float maxDetectDistance)
    {
        NPC closestNPC = null;
        float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;

        foreach (NPC target in Main.npc)
        {
            if (target.CanBeChasedBy())
            {
                float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);

                if (sqrDistanceToTarget < sqrMaxDetectDistance)
                {
                    sqrMaxDetectDistance = sqrDistanceToTarget;
                    closestNPC = target;
                }
            }
        }

        return closestNPC;
    }
}