using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ultraconyx.Content.Projectiles;

public class EnchantedWD : ModProjectile
{
    private int timer;
    private bool homing;
    private bool hitEnemy;
    private float fadeTimer;
    private const float FadeTime = 20f; // Fade duration in frames

    public override void SetStaticDefaults()
    {
        // DisplayName.SetDefault("EnchantedWD");
    }

    public override void SetDefaults()
    {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.aiStyle = 0;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 300;
        Projectile.tileCollide = false;
        Projectile.alpha = 0;

        Projectile.frame = 0;
        Projectile.frameCounter = 0;
    }

    public override void AI()
    {
        timer++;

        // Handle fade out after hitting enemy
        if (hitEnemy)
        {
            fadeTimer++;
            Projectile.alpha = (int)(255 * (fadeTimer / FadeTime));
            if (fadeTimer >= FadeTime)
            {
                Projectile.Kill();
                return;
            }
        }

        // Animation: 5 frames, 6 ticks each (~0.1s per frame)
        Projectile.frameCounter++;
        if (Projectile.frameCounter >= 6)
        {
            Projectile.frameCounter = 0;
            Projectile.frame = (Projectile.frame + 1) % 5;
        }

        // Homing after 0.5s
        if (timer >= 30)
            homing = true;

        if (homing && !hitEnemy)
        {
            NPC target = null;
            float minDist = 500f;

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy() && Vector2.Distance(Projectile.Center, npc.Center) < minDist)
                {
                    target = npc;
                    minDist = Vector2.Distance(Projectile.Center, npc.Center);
                }
            }

            if (target != null)
            {
                Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * 10f, 0.1f);
            }
        }

        // Add purple glow dust effects
        if (Main.rand.NextBool(3) && !hitEnemy)
        {
            // Inner glow (bright purple)
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                DustID.PurpleTorch, 0f, 0f, 100, default(Color), 1.5f);
            dust.noGravity = true;
            dust.velocity *= 0.2f;

            // Outer glow (softer purple)
            if (Main.rand.NextBool(2))
            {
                dust = Dust.NewDustDirect(Projectile.position - new Vector2(5, 5), Projectile.width + 10, Projectile.height + 10,
                    DustID.PurpleTorch, 0f, 0f, 150, default(Color), 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.1f;
            }
        }

        // Add light at projectile position (makes it actually glow in the dark)
        if (!hitEnemy)
        {
            Lighting.AddLight(Projectile.Center, 0.6f, 0.2f, 0.8f); // Purple light
        }

        // Slow rotation for visual effect
        Projectile.rotation += 0.1f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        // Start fading when hitting an enemy
        if (!hitEnemy)
        {
            hitEnemy = true;
            Projectile.damage = 0; // Prevent multiple damage instances during fade
            Projectile.velocity *= 0.1f; // Slow down dramatically

            // Create hit effect
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.PurpleTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default(Color), 2f);
                dust.noGravity = true;
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
        int frameHeight = tex.Height / 5;
        Rectangle sourceRect = new(0, Projectile.frame * frameHeight, tex.Width, frameHeight);

        Vector2 origin = sourceRect.Size() / 2f;

        SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        float alphaMultiplier = 1f - Projectile.alpha / 255f;

        // Method 1: Draw multiple layers with different colors and scales for glow
        if (!hitEnemy)
        {
            // Outer glow (larger, more transparent)
            Main.spriteBatch.Draw(
                tex,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                sourceRect,
                new Color(150, 50, 200, 100) * alphaMultiplier, // Soft purple
                Projectile.rotation,
                origin,
                Projectile.scale * 1.3f, // Larger scale for outer glow
                effects,
                0f);

            // Middle glow
            Main.spriteBatch.Draw(
                tex,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                sourceRect,
                new Color(200, 100, 255, 150) * alphaMultiplier, // Medium purple
                Projectile.rotation,
                origin,
                Projectile.scale * 1.15f, // Medium scale
                effects,
                0f);
        }

        // Main projectile (original colors)
        Main.spriteBatch.Draw(
            tex,
            Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
            sourceRect,
            Color.Lerp(lightColor, Color.White, 0.3f) * alphaMultiplier,
            Projectile.rotation,
            origin,
            Projectile.scale,
            effects,
            0f);

        // Method 2: Add pulsing effect to the glow
        if (!hitEnemy)
        {
            float pulse = (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 10f) * 0.1f + 0.9f;

            Main.spriteBatch.Draw(
                tex,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                sourceRect,
                new Color(255, 150, 255, 50) * pulse * alphaMultiplier, // Bright pulsing purple
                Projectile.rotation,
                origin,
                Projectile.scale * (1.1f + pulse * 0.1f),
                effects,
                0f);
        }

        return false;
    }
}