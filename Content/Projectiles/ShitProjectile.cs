using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace Ultraconyx.Content.Projectiles;

public class ShitProjectile : ModProjectile
{
    private const float HomingRange = 400f;
    private const float HomingStrength = 0.08f;
    private const float MaxSpeed = 6f;

    public override void SetDefaults()
    {
        Projectile.width = 8;
        Projectile.height = 8;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 300;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
    }

    public override void OnSpawn(IEntitySource source)
    {
        // Initial gentle random drift
        Projectile.velocity = Main.rand.NextVector2Circular(2f, 2f);
    }

    public override void AI()
    {
        // ─── FLOATING WOBBLE ───
        Projectile.velocity.Y += (float)System.Math.Sin(Projectile.timeLeft * 0.1f) * 0.02f;

        // ─── HOMING ───
        NPC target = FindClosestNPC(HomingRange);
        if (target != null)
        {
            Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * MaxSpeed;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, HomingStrength);
        }

        // ─── SPEED CAP ───
        if (Projectile.velocity.Length() > MaxSpeed)
            Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;

        // ─── PIXEL-PERFECT CENTERED POOP DUST ───
        if (Main.rand.NextBool(2))
        {
            Dust dust = Dust.NewDustPerfect(
                Projectile.Center,
                DustID.Poop,
                Projectile.velocity * 0.2f,
                150,
                default,
                1f
            );

            // Correct exact centering
            dust.position -= dust.frame.Size() / 2f;

            // No gravity, floats behind projectile
            dust.noGravity = true;

            // Add drifting "fall-off" effect
            dust.velocity *= 0.1f;           // slow down dust
            dust.velocity.Y += 0.2f;         // gravity-like downward drift
            dust.fadeIn = 0.5f;              // optional subtle fade
        }
    }

    private NPC FindClosestNPC(float maxDetectDistance)
    {
        NPC closestNPC = null;
        float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;

        foreach (NPC npc in Main.npc)
        {
            if (!npc.CanBeChasedBy(this))
                continue;

            float sqrDistance = Vector2.DistanceSquared(npc.Center, Projectile.Center);
            if (sqrDistance < sqrMaxDetectDistance)
            {
                sqrMaxDetectDistance = sqrDistance;
                closestNPC = npc;
            }
        }

        return closestNPC;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        SoundEngine.PlaySound(
            new SoundStyle("Ultracronyx/Content/Sounds/boom"),
            Projectile.Center
        );
    }
}
