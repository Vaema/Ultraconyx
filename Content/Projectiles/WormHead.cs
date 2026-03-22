using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Ultraconyx.Content.Buffs;

namespace Ultraconyx.Content.Projectiles;

public class WormHead : ModProjectile
{
    private const float SegmentLength = 22f;
    private const float LungeStrength = 12f;
    private const float MaxLungeDistance = 80f;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.MinionSacrificable[Type] = true;
        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = 22;
        Projectile.height = 22;
        Projectile.friendly = true;
        Projectile.minion = true;
        Projectile.minionSlots = 1f;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        Player player = Main.player[Projectile.owner];

        if (!player.active || player.dead)
        {
            player.ClearBuff(ModContent.BuffType<Worm>());
            return;
        }

        Projectile.timeLeft = 2;

        Projectile previous = Main.projectile[(int)Projectile.ai[0]];
        if (!previous.active)
        {
            Projectile.Kill();
            return;
        }

        FollowSegment(previous);

        NPC target = FindTarget();

        if (target != null)
        {
            Vector2 toTarget = target.Center - Projectile.Center;
            Projectile.rotation = toTarget.ToRotation() + MathHelper.PiOver2;

            // Store target position for lunging
            Projectile.ai[2] = target.whoAmI;
            LungeTowardTarget(target, previous);
        }
        else
        {
            Projectile.ai[2] = -1; // Clear target
            IdleWave(previous);
        }
    }

    private void FollowSegment(Projectile previous)
    {
        Vector2 dir = previous.Center - Projectile.Center;
        float dist = dir.Length();

        if (dist > SegmentLength)
        {
            dir.Normalize();
            Projectile.Center = previous.Center - dir * SegmentLength;
        }
    }

    private NPC FindTarget()
    {
        NPC target = null;
        float maxDistance = 500f;

        foreach (NPC npc in Main.npc)
        {
            if (npc.CanBeChasedBy())
            {
                float d = Vector2.Distance(npc.Center, Projectile.Center);
                if (d < maxDistance)
                {
                    maxDistance = d;
                    target = npc;
                }
            }
        }

        return target;
    }

    private void LungeTowardTarget(NPC target, Projectile previous)
    {
        if (Projectile.owner != Main.myPlayer)
            return;

        // Calculate direction to target
        Vector2 directionToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
        
        // Calculate how far we can lunge without breaking the chain
        float distanceToPrevious = Vector2.Distance(Projectile.Center, previous.Center);
        float maxLungeThisFrame = Math.Min(LungeStrength, (SegmentLength * 1.5f) - distanceToPrevious);
        
        if (maxLungeThisFrame > 0)
        {
            // Lunge toward target
            Vector2 lungeOffset = directionToTarget * maxLungeThisFrame;
            Projectile.Center += lungeOffset;
        }

        // Shooting logic
        Projectile.ai[1]++;
        if (Projectile.ai[1] >= 45)
        {
            Projectile.ai[1] = 0;

            // Create wave effect through body
            CreateShootWave();

            Vector2 shootDirection = directionToTarget;

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                shootDirection * 8f,
                ModContent.ProjectileType<Apple>(),
                Projectile.damage,
                1f,
                Projectile.owner
            );
        }
    }

    private void CreateShootWave()
    {
        // Tell all body segments to do a wave effect
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile proj = Main.projectile[i];
            if (proj.active && proj.type == ModContent.ProjectileType<WormBody>())
            {
                WormBody body = proj.ModProjectile as WormBody;
                if (body != null)
                {
                    body.TriggerWave(5f); // Wave strength
                }
            }
        }
    }

    private void IdleWave(Projectile previous)
    {
        Vector2 outward = Projectile.Center - previous.Center;
        outward = outward.SafeNormalize(Vector2.UnitY);

        // Idle wave - head sways side to side
        float wave = (float)Math.Sin(Main.GameUpdateCount * 0.05f + Projectile.whoAmI) * 0.6f;

        // Rotate the outward direction for side-to-side movement
        Vector2 swayDirection = outward.RotatedBy(wave * 0.5f);

        Projectile.rotation = swayDirection.ToRotation() + MathHelper.PiOver2;

        // Add slight positional wave
        Vector2 sideOffset = outward.RotatedBy(MathHelper.PiOver2) * wave * 2f;
        Projectile.Center += sideOffset;
    }
}