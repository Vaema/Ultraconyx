using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Projectiles;

public class AtlasEdgeClone : ModProjectile
{
    private const int HoverTime = 45;   // orbit duration in ticks
    private const float OrbitRadius = 90f;
    private const float OrbitSpeed = 0.05f; // slow & smooth orbit
    private const float DashSpeed = 20f;

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;

        Projectile.friendly = false; // OFF during orbit
        Projectile.hostile = false;

        Projectile.penetrate = 1;
        Projectile.timeLeft = 240;

        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.DamageType = DamageClass.Melee;
    }

    public override void AI()
    {
        // ai[0] should contain the target NPC index
        int targetIndex = (int)Projectile.ai[0];

        if (targetIndex < 0 || targetIndex >= Main.maxNPCs)
        {
            Projectile.Kill();
            return;
        }

        NPC target = Main.npc[targetIndex];
        if (target == null || !target.active)
        {
            Projectile.Kill();
            return;
        }

        // ORBIT PHASE (no damage)
        if (Projectile.localAI[0] < HoverTime)
        {
            Projectile.localAI[0]++;
            Projectile.friendly = false;

            if (Projectile.localAI[0] == 1)
            {
                // initialize angle relative to the target
                Projectile.ai[1] = Projectile.AngleFrom(target.Center);
            }

            Projectile.ai[1] += OrbitSpeed;

            Vector2 orbitPosition = target.Center + Projectile.ai[1].ToRotationVector2() * OrbitRadius;

            // Smooth, lerped orbit movement
            Projectile.Center = Vector2.Lerp(Projectile.Center, orbitPosition, 0.12f);
            Projectile.velocity *= 0.9f;

            // Dust spiral
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = (Projectile.Center - target.Center).SafeNormalize(Vector2.UnitY) * (OrbitRadius * 0.5f);
                Vector2 pos = Projectile.Center + (offset.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)));
                Dust d = Dust.NewDustPerfect(pos, DustID.BlueTorch, Vector2.Zero, 150, default, 1.0f);
                d.noGravity = true;
                d.velocity *= 0.05f;
                d.fadeIn = 0.4f;
            }
        }
        else
        {
            // DASH PHASE (smooth acceleration)
            Projectile.friendly = true;
            Projectile.tileCollide = true;

            Vector2 targetDir = Projectile.DirectionTo(target.Center);
            Vector2 desiredVelocity = targetDir * DashSpeed;

            // Lerp velocity to create smooth acceleration
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.08f);
        }

        // Clone faces the enemy (sprite is up-facing, so add Pi/2)
        Vector2 lookDir = target.Center - Projectile.Center;
        if (lookDir.LengthSquared() > 0.001f)
            Projectile.rotation = lookDir.ToRotation() + MathHelper.PiOver2;
    }

    // Ensure it only deals damage on dash: OnHitNPC will trigger when friendly=true (we already toggle it),
    // but we can also override Colliding or OnHit to be extra safe. For now toggling friendly is sufficient.
}
