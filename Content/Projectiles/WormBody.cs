using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;

namespace Ultraconyx.Content.Projectiles;

public class WormBody : ModProjectile
{
    private const float SegmentLength = 20f;
    private float waveStrength;
    private int waveTimer;

    public override void SetDefaults()
    {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.minion = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        Player player = Main.player[Projectile.owner];

        Vector2 targetPos;

        if (Projectile.ai[1] == -1)
        {
            targetPos = player.Center;
        }
        else
        {
            Projectile previous = Main.projectile[(int)Projectile.ai[0]];
            if (!previous.active)
            {
                Projectile.Kill();
                return;
            }

            targetPos = previous.Center;
        }

        FollowTarget(targetPos);

        // Handle wave effects
        if (waveTimer > 0)
        {
            waveTimer--;
            ApplyWaveEffect();
        }
        else
        {
            waveStrength = 0;
            ApplyIdleWave();
        }
    }

    private void FollowTarget(Vector2 targetPos)
    {
        Vector2 dir = targetPos - Projectile.Center;
        float dist = dir.Length();

        if (dist > SegmentLength)
        {
            dir.Normalize();
            Projectile.Center = targetPos - dir * SegmentLength;
        }
        else if (dist < SegmentLength - 3f && dist > 0.1f)
        {
            dir.Normalize();
            Projectile.Center = targetPos - dir * SegmentLength;
        }

        // Update rotation to face the target (away from player/previous segment)
        if (dir.LengthSquared() > 0.01f)
        {
            Projectile.rotation = dir.ToRotation() + MathHelper.PiOver2;
        }
    }

    public void TriggerWave(float strength)
    {
        waveStrength = strength;
        waveTimer = 30; // Wave lasts 30 ticks
    }

    private void ApplyWaveEffect()
    {
        // Wave travels from head to tail
        float progress = 1f - (waveTimer / 30f); // 0 to 1

        // Get direction perpendicular to segment
        Vector2 dir = Projectile.rotation.ToRotationVector2();
        Vector2 perpendicular = dir.RotatedBy(MathHelper.PiOver2);

        // Wave intensity decreases as it travels
        float segmentProgress = Projectile.ai[1] / 2f; // 0, 0.5, 1 for segments 0,1,2
        float waveAtSegment = Math.Max(0, 1f - Math.Abs(progress - segmentProgress) * 2f);

        float currentWave = waveStrength * waveAtSegment * (float)Math.Sin(waveTimer * 0.5f);

        Projectile.Center += perpendicular * currentWave;
    }

    private void ApplyIdleWave()
    {
        // Subtle idle wave that flows from head to tail
        float time = Main.GameUpdateCount * 0.03f;
        float segmentPhase = Projectile.ai[1] * 0.8f; // Different phase for each segment

        float wave = (float)Math.Sin(time + segmentPhase) * 0.3f;

        Vector2 dir = Projectile.rotation.ToRotationVector2();
        Vector2 perpendicular = dir.RotatedBy(MathHelper.PiOver2);

        Projectile.Center += perpendicular * wave;

        // Add slight rotation wave
        Projectile.rotation += (float)Math.Sin(time + segmentPhase) * 0.03f;
    }
}