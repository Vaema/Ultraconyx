using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Ultraconyx.Content.Projectiles;

public class ArcadiumRayProjectile : ModProjectile
{
    private float opacity = 1f;
    private const float MAX_LENGTH = 2000f;
    private const float FADE_OUT_TIME = 20f;
    private float beamLength = 0f;
    private float beamGrowthSpeed = 40f;
    
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 1;
    }

    public override void SetDefaults()
    {
        Projectile.width = 1;
        Projectile.height = 1;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 60;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.aiStyle = -1;
        Projectile.alpha = 255;
        Projectile.scale = 1f;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        
        if (player.active && !player.dead)
        {
            Projectile.Center = player.Center;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        else
        {
            Projectile.Kill();
            return;
        }
        
        UpdateOpacity();
        UpdateBeamLength();
        CreateBeamEffects();
        DamageEnemiesInBeam();
    }

    private void UpdateOpacity()
    {
        if (Projectile.timeLeft < FADE_OUT_TIME)
        {
            opacity = (float)Projectile.timeLeft / FADE_OUT_TIME;
        }
        else
        {
            opacity = 1f;
        }
        
        Projectile.alpha = (int)(255 * (1f - opacity));
    }

    private void UpdateBeamLength()
    {
        if (beamLength < MAX_LENGTH)
        {
            beamLength += beamGrowthSpeed;
            if (beamLength > MAX_LENGTH)
                beamLength = MAX_LENGTH;
        }
        
        Vector2 direction = Projectile.rotation.ToRotationVector2();
        Vector2 endPos = Projectile.Center + direction * beamLength;
        
        float[] samples = new float[3];
        Collision.LaserScan(Projectile.Center, direction, 12, beamLength, samples);
        
        float actualLength = 0;
        for (int i = 0; i < samples.Length; i++)
            actualLength += samples[i];
        actualLength /= samples.Length;
        
        beamLength = Math.Min(beamLength, actualLength);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float collisionPoint = 0f;
        Vector2 direction = Projectile.rotation.ToRotationVector2();
        Vector2 beamEnd = Projectile.Center + direction * beamLength;
        
        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(), targetHitbox.Size(),
            Projectile.Center, beamEnd,
            12,
            ref collisionPoint
        );
    }

    private void DamageEnemiesInBeam()
    {
        Vector2 direction = Projectile.rotation.ToRotationVector2();
        Vector2 beamEnd = Projectile.Center + direction * beamLength;
        
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            
            if (npc.active && !npc.friendly && npc.life > 0 && !npc.dontTakeDamage)
            {
                float collisionPoint = 0f;
                bool colliding = Collision.CheckAABBvLineCollision(
                    npc.Hitbox.TopLeft(), npc.Hitbox.Size(),
                    Projectile.Center, beamEnd,
                    12,
                    ref collisionPoint
                );
                
                if (colliding && Projectile.timeLeft % 10 == 0)
                {
                    int damage = Projectile.damage;
                    bool crit = Main.rand.Next(100) < Main.player[Projectile.owner].GetCritChance(DamageClass.Magic);
                    
                    npc.SimpleStrikeNPC(damage, (int)Math.Sign(direction.X), crit, 0f, DamageClass.Magic, false, Projectile.owner);
                    
                    for (int j = 0; j < 3; j++)
                    {
                        Vector2 dustPos = npc.Center + new Vector2(
                            Main.rand.NextFloat(-npc.width * 0.5f, npc.width * 0.5f),
                            Main.rand.NextFloat(-npc.height * 0.5f, npc.height * 0.5f)
                        );
                        Dust dust = Dust.NewDustPerfect(dustPos, DustID.GemDiamond, Vector2.Zero, 100, 
                            Color.Cyan * opacity, 0.8f);
                        dust.noGravity = true; // NO GRAVITY
                    }
                }
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        DrawBeam();
        return false;
    }

    private void DrawBeam()
    {
        Texture2D beamTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        
        Vector2 start = Projectile.Center;
        Vector2 direction = Projectile.rotation.ToRotationVector2();
        
        float visibleLength = beamLength;
        float fadeStartDistance = 0f;
        
        if (Projectile.timeLeft < FADE_OUT_TIME)
        {
            float fadeAmount = 1f - opacity;
            fadeStartDistance = beamLength * fadeAmount;
            visibleLength = beamLength - fadeStartDistance;
        }
        
        if (visibleLength <= 0)
            return;
        
        Vector2 visibleStart = start + direction * fadeStartDistance;
        Vector2 end = visibleStart + direction * visibleLength;
        
        float rotation = direction.ToRotation();
        
        Color beamColor = Color.White * opacity * 0.9f;
        
        int beamHeight = beamTexture.Height;
        int beamSegmentWidth = beamTexture.Width;
        int segmentsNeeded = (int)Math.Ceiling(visibleLength / beamSegmentWidth);
        
        for (int i = 0; i < segmentsNeeded; i++)
        {
            Vector2 segmentPosition = visibleStart + direction * (i * beamSegmentWidth);
            float remainingDistance = visibleLength - (i * beamSegmentWidth);
            
            if (remainingDistance < beamSegmentWidth)
            {
                int drawWidth = (int)remainingDistance;
                Rectangle sourceRect = new Rectangle(0, 0, drawWidth, beamHeight);
                Vector2 drawPosition = segmentPosition - Main.screenPosition;
                
                Main.EntitySpriteDraw(beamTexture,
                    drawPosition,
                    sourceRect,
                    beamColor,
                    rotation,
                    new Vector2(0, beamHeight / 2f),
                    1f,
                    SpriteEffects.None,
                    0);
                break;
            }
            else
            {
                Vector2 drawPosition = segmentPosition - Main.screenPosition;
                
                Main.EntitySpriteDraw(beamTexture,
                    drawPosition,
                    null,
                    beamColor,
                    rotation,
                    new Vector2(0, beamHeight / 2f),
                    1f,
                    SpriteEffects.None,
                    0);
            }
        }
        
        CreateTravelingDust(visibleStart, direction, visibleLength);
        
        Lighting.AddLight(visibleStart, 0.2f * opacity, 0.6f * opacity, 1f * opacity);
        if (visibleLength > 0)
            Lighting.AddLight(end, 0.1f * opacity, 0.3f * opacity, 0.5f * opacity);
    }
    
    private void CreateTravelingDust(Vector2 start, Vector2 direction, float distance)
    {
        if (distance > 0 && opacity > 0.1f)
        {
            float travelSpeed = 10f;
            float time = Main.GameUpdateCount * 0.05f;
            
            for (int i = 0; i < 3; i++)
            {
                float offset = i * 0.3f;
                float progress = (time + offset) % 1f;
                
                Vector2 dustPos = start + direction * (distance * progress);
                
                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                float sideOffset = (float)Math.Sin(time * 2f + i) * 3f;
                dustPos += perpendicular * sideOffset;
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.GemDiamond, Vector2.Zero, 100, 
                    Color.Cyan * opacity * 0.8f, 0.7f);
                dust.noGravity = true; // NO GRAVITY
                dust.velocity = direction * travelSpeed;
                dust.scale = 0.6f + Main.rand.NextFloat(0.3f);
                
                if (Main.rand.NextBool(3))
                {
                    Dust trailDust = Dust.NewDustPerfect(dustPos - direction * 5f, DustID.GemDiamond, 
                        direction * travelSpeed * 0.5f, 100, Color.Cyan * opacity * 0.5f, 0.4f);
                    trailDust.noGravity = true; // NO GRAVITY
                }
            }
            
            if (distance < MAX_LENGTH - 50)
            {
                Vector2 endPos = start + direction * distance;
                for (int i = 0; i < 2; i++)
                {
                    Dust impactDust = Dust.NewDustPerfect(endPos, DustID.GemDiamond, 
                        Vector2.Zero, 100, Color.Cyan * opacity * 0.9f, 0.8f);
                    impactDust.noGravity = true; // NO GRAVITY
                    impactDust.velocity = direction.RotatedByRandom(0.3f) * 2f;
                }
            }
        }
    }

    private void CreateBeamEffects()
    {
        if (Main.rand.NextBool(3))
        {
            Vector2 direction = Projectile.rotation.ToRotationVector2();
            Vector2 muzzlePos = Projectile.Center + direction * 5f;
            Dust dust = Dust.NewDustPerfect(muzzlePos, DustID.GemDiamond, 
                direction * 2f, 100,
                Color.Cyan * opacity, 1f);
            dust.noGravity = true; // NO GRAVITY
        }
        
        Lighting.AddLight(Projectile.Center, 
            0.2f * opacity, 
            0.6f * opacity, 
            1f * opacity);
    }

    public override void OnKill(int timeLeft)
    {
        Vector2 direction = Projectile.rotation.ToRotationVector2();
        Vector2 endPos = Projectile.Center + direction * beamLength;
        
        for (int i = 0; i < 10; i++)
        {
            Vector2 velocity = direction.RotatedByRandom(0.8f) * Main.rand.NextFloat(2f, 4f);
            Dust dust = Dust.NewDustPerfect(endPos, DustID.GemDiamond, velocity, 100, 
                Color.Cyan * opacity, 1f);
            dust.noGravity = true; // NO GRAVITY
        }
        
        Lighting.AddLight(Projectile.Center, 
            0.5f * opacity, 
            1f * opacity, 
            1.5f * opacity);
        Lighting.AddLight(endPos, 
            0.3f * opacity, 
            0.8f * opacity, 
            1.2f * opacity);
    }
}