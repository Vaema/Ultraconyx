using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;
using Ultraconyx.Content.Dusts;

namespace Ultraconyx.Content.Projectiles.Bosses.StardustAngel;

public class AngelMiniStar : ModProjectile
{
    private Vector2 targetCenter;
    private Vector2 circleCenter;
    private float circleRadius;
    private float circleAngle;
    private bool isCircleFormation = false;
    private int splitTimer = 0;
    private int splitTime = 0;
    private int spreadTimer = 0;
    private const int SpreadDuration = 30;
    
    private Vector2[] trailPositions = new Vector2[3];
    private int trailIndex = 0;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 1;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    public override void SetDefaults()
    {
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 300;
        Projectile.alpha = 0;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
        
        Projectile.light = 0.3f;
    }

    public void SetCircleTarget(Vector2 target, Vector2 center, float radius, float angle, int splitAfter = 60)
    {
        targetCenter = target;
        circleCenter = center;
        circleRadius = radius;
        circleAngle = angle;
        isCircleFormation = true;
        splitTime = splitAfter;
        splitTimer = 0;
    }

    public override void AI()
    {
        trailPositions[trailIndex] = Projectile.Center;
        trailIndex = (trailIndex + 1) % trailPositions.Length;

        if (Main.rand.NextBool(6))
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Stardust>(), 
                Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 100, default, 0.6f);
            dust.noGravity = true;
        }
        
        if (isCircleFormation)
        {
            splitTimer++;
            
            Vector2 directionToTarget = targetCenter - circleCenter;
            
            if (directionToTarget.Length() > 5f)
            {
                directionToTarget.Normalize();
                circleCenter += directionToTarget * 5f;
            }
            
            Vector2 offset = new Vector2(
                (float)Math.Cos(circleAngle) * circleRadius,
                (float)Math.Sin(circleAngle) * circleRadius
            );
            
            Projectile.Center = circleCenter + offset;
            Projectile.velocity = Vector2.Zero;
            
            circleRadius *= 0.995f;
            
            if (splitTimer >= splitTime)
            {
                isCircleFormation = false;
                
                Vector2 spreadDirection = new Vector2(
                    (float)Math.Cos(circleAngle),
                    (float)Math.Sin(circleAngle)
                );
                
                Projectile.velocity = spreadDirection * 6f;
                
                SoundEngine.PlaySound(SoundID.Item9, Projectile.Center);
            }
        }
        else
        {
            if (spreadTimer < SpreadDuration)
            {
                spreadTimer++;
                Projectile.velocity *= 0.98f;
            }
            else
            {
                Vector2 directionToPlayer = targetCenter - Projectile.Center;
                if (directionToPlayer.Length() > 10f)
                {
                    directionToPlayer.Normalize();
                    
                    float homingStrength = MathHelper.Clamp((spreadTimer - SpreadDuration) / 30f, 0f, 1f);
                    Vector2 homingVelocity = directionToPlayer * 9f;
                    
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, homingVelocity, homingStrength * 0.1f);
                    
                    if (Projectile.velocity.Length() < 4f)
                    {
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 4f;
                    }
                }
            }
        }
        
        Projectile.rotation += 0.1f;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
        
        for (int i = 0; i < trailPositions.Length; i++)
        {
            if (trailPositions[i] != Vector2.Zero)
            {
                float alpha = 0.2f * (1f - (i / (float)trailPositions.Length));
                Color trailColor = Color.White * alpha;
                Main.EntitySpriteDraw(
                    texture,
                    trailPositions[i] - Main.screenPosition,
                    null,
                    trailColor,
                    Projectile.rotation,
                    origin,
                    Projectile.scale * 0.7f,
                    SpriteEffects.None,
                    0
                );
            }
        }
        
        Main.EntitySpriteDraw(
            texture,
            Projectile.Center - Main.screenPosition,
            null,
            Color.White,
            Projectile.rotation,
            origin,
            Projectile.scale,
            SpriteEffects.None,
            0
        );
        
        return false;
    }

    public override bool? CanHitNPC(NPC target)
    {
        return false; // Don't hit other NPCs
    }

    // Remove OnHitPlayer override since we don't need it
    // The projectile will naturally not disappear because penetrate = -1

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 5; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Stardust>(), 
                Projectile.velocity.X * 0.3f, Projectile.velocity.Y * 0.3f, 100, default, 0.8f);
            dust.noGravity = true;
        }
    }
}