using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Ultraconyx.Content.Items.Weapons.Faith;

namespace Ultraconyx.Content.Projectiles;

public class GoldenCrossProjectile : ModProjectile
{
    private NPC currentTarget = null;
    private int damageTimer = 0;
    private Vector2 targetPosition;
    private float opacity = 0f;
    private float scale = 1f;
    private bool reachedTarget = false;
    private const float FADE_IN_TIME = 15f;
    private const float FADE_OUT_TIME = 30f;
    private int weaponDamage = 0;
    
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 1;
    }

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 600;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.aiStyle = -1;
        Projectile.alpha = 255;
        Projectile.damage = 0;
        Projectile.scale = 0f; // Start at 0 scale
    }

    public override void AI()
    {
        // Get weapon damage on first frame if we haven't already
        if (weaponDamage == 0 && Main.player[Projectile.owner].active)
        {
            Player player = Main.player[Projectile.owner];
            
            if (player.HeldItem.type == ModContent.ItemType<GoldenCross>())
            {
                weaponDamage = player.GetWeaponDamage(player.HeldItem);
            }
            else
            {
                weaponDamage = 35;
            }
        }
        
        // Handle fade in/out and scale
        UpdateOpacityAndScale();
        
        // If we haven't reached target position, drop down to it
        if (!reachedTarget)
        {
            DropToTarget();
        }
        else
        {
            // After reaching target, update enemy target and damage
            UpdateTarget();
            
            // If we have a target, connect and damage it
            if (currentTarget != null && currentTarget.active)
            {
                DamageTarget();
            }
            
            // Create light and particles
            CreateEffects();
        }
    }

    private void UpdateOpacityAndScale()
    {
        // Fade in when dropping
        if (!reachedTarget)
        {
            float dropProgress = 1f - (Projectile.timeLeft / 600f);
            opacity = MathHelper.Clamp(dropProgress * (600f / FADE_IN_TIME), 0f, 1f);
            scale = opacity; // Grow as fades in (0 → 1)
        }
        // Fade out when about to die
        else if (Projectile.timeLeft < FADE_OUT_TIME)
        {
            float fadeOutProgress = (float)Projectile.timeLeft / FADE_OUT_TIME;
            opacity = fadeOutProgress; // 1 → 0
            scale = fadeOutProgress * 0.3f + 0.7f; // 1 → 0.3 (shrink as fades out)
        }
        // Fully visible in middle
        else
        {
            opacity = 1f;
            scale = 1f;
        }
        
        // Apply opacity to alpha (0-255)
        Projectile.alpha = (int)(255 * (1f - opacity));
        // Apply scale to projectile
        Projectile.scale = scale;
    }

    private void DropToTarget()
    {
        if (targetPosition == Vector2.Zero)
        {
            targetPosition = Projectile.Center + new Vector2(0, 48 * 3);
        }
        
        float distanceToTarget = Vector2.Distance(Projectile.Center, targetPosition);
        
        if (distanceToTarget < 2f)
        {
            reachedTarget = true;
            Projectile.velocity = Vector2.Zero;
            Projectile.Center = targetPosition;
            return;
        }
        
        Vector2 direction = targetPosition - Projectile.Center;
        direction.Normalize();
        
        float speed = 8f + (distanceToTarget * 0.1f);
        speed = MathHelper.Clamp(speed, 4f, 20f);
        
        Projectile.velocity = direction * speed;
        
        CreateDropParticles();
    }

    private void CreateDropParticles()
    {
        if (Main.rand.NextBool(2))
        {
            Vector2 trailPosition = Projectile.Center - Projectile.velocity * 0.3f;
            Dust trailDust = Dust.NewDustPerfect(trailPosition, DustID.GoldFlame, 
                Vector2.Zero, 100, Color.Gold, 0.6f);
            trailDust.noGravity = true;
            trailDust.velocity = Projectile.velocity * -0.2f;
            trailDust.scale = 0.5f;
        }
    }

    private void UpdateTarget()
    {
        // If no target or target died, find new one
        if (currentTarget == null || !currentTarget.active || currentTarget.life <= 0)
        {
            currentTarget = FindNearestEnemy(Projectile.Center);
        }
    }

    private NPC FindNearestEnemy(Vector2 position)
    {
        NPC nearest = null;
        float nearestDistance = float.MaxValue;
        
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            
            if (npc.active && !npc.friendly && npc.life > 0 && !npc.dontTakeDamage)
            {
                float distance = Vector2.Distance(position, npc.Center);
                
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = npc;
                }
            }
        }
        
        return nearest;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        DrawProjectileSprite();
        
        if (reachedTarget && currentTarget != null && currentTarget.active)
        {
            DrawBeam();
        }
        
        return false;
    }

    private void DrawProjectileSprite()
    {
        Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        Color drawColor = Color.White * opacity;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Vector2 origin = texture.Size() / 2f;
        
        // Use the scale we calculated (not Projectile.scale directly)
        float drawScale = scale;
        
        Main.EntitySpriteDraw(texture,
            drawPosition,
            null,
            drawColor,
            Projectile.rotation,
            origin,
            drawScale,
            SpriteEffects.None,
            0);
    }

    private void DrawBeam()
    {
        Texture2D beamTexture = ModContent.Request<Texture2D>("Ultracronyx/Content/Projectiles/GoldenCrossBeam").Value;
        
        Vector2 start = Projectile.Center;
        Vector2 end = currentTarget.Center;
        Vector2 direction = end - start;
        float distance = direction.Length();
        float rotation = direction.ToRotation() - MathHelper.PiOver2;
        
        if (distance > 0)
            direction.Normalize();
        else
            return;
        
        // Apply both opacity and scale to beam
        Color beamColor = Color.White * opacity * 0.9f;
        float beamScale = scale; // Beam also shrinks with projectile
        
        int beamWidth = beamTexture.Width;
        int beamSegmentHeight = beamTexture.Height;
        int segmentsNeeded = (int)Math.Ceiling(distance / beamSegmentHeight);
        
        for (int i = 0; i < segmentsNeeded; i++)
        {
            Vector2 segmentPosition = start + direction * (i * beamSegmentHeight);
            float remainingDistance = distance - (i * beamSegmentHeight);
            
            if (remainingDistance < beamSegmentHeight)
            {
                int drawHeight = (int)remainingDistance;
                Rectangle sourceRect = new Rectangle(0, 0, beamWidth, drawHeight);
                Vector2 drawPosition = segmentPosition - Main.screenPosition;
                
                Main.EntitySpriteDraw(beamTexture,
                    drawPosition,
                    sourceRect,
                    beamColor,
                    rotation,
                    new Vector2(beamWidth / 2f, 0),
                    beamScale, // Apply scale to beam
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
                    new Vector2(beamWidth / 2f, 0),
                    beamScale, // Apply scale to beam
                    SpriteEffects.None,
                    0);
            }
        }
        
        CreateBeamParticles(start, end, direction, distance, beamSegmentHeight);
        
        // Light intensity also scales with opacity
        Lighting.AddLight(start, 0.8f * opacity, 0.7f * opacity, 0.2f * opacity);
        Lighting.AddLight(end, 0.6f * opacity, 0.5f * opacity, 0.15f * opacity);
    }
    
    private void CreateBeamParticles(Vector2 start, Vector2 end, Vector2 direction, float distance, int beamSegmentHeight)
    {
        // Only create particles if the beam isn't too long (for performance)
        if (distance < 1000f)
        {
            int particleCount = 3;
            
            for (int i = 0; i < particleCount; i++)
            {
                float progress = (Main.GameUpdateCount * 0.05f + i * 0.3f) % 1f;
                Vector2 particlePos = start + direction * (distance * progress);
                
                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                float offset = Main.rand.NextFloat(-2f, 2f);
                particlePos += perpendicular * offset;
                
                Dust dust = Dust.NewDustPerfect(particlePos, DustID.GemTopaz, Vector2.Zero, 100, 
                    Color.Orange * opacity, 0.7f * opacity * scale); // Scale affects particle size too
                dust.noGravity = true;
                dust.velocity = direction * 2f;
                dust.scale = 0.5f * scale + Main.rand.NextFloat(0.3f);
            }
        }
    }

    private void DamageTarget()
    {
        damageTimer++;
        
        // Damage enemy every 10 frames (6 times per second)
        if (damageTimer >= 10)
        {
            damageTimer = 0;
            
            // Use the FULL weapon damage for the beam
            int damage = weaponDamage;
            bool crit = Main.rand.Next(100) < 5; // 5% crit chance
            
            // Apply damage directly
            currentTarget.SimpleStrikeNPC(damage, 0, crit, 0f, DamageClass.Magic, false, Projectile.owner);
            
            // Visual effect on enemy
            for (int i = 0; i < 4; i++)
            {
                Vector2 dustPos = currentTarget.Center + new Vector2(
                    Main.rand.NextFloat(-currentTarget.width * 0.5f, currentTarget.width * 0.5f),
                    Main.rand.NextFloat(-currentTarget.height * 0.5f, currentTarget.height * 0.5f)
                );
                Dust.NewDustPerfect(dustPos, DustID.GoldFlame, Vector2.Zero, 100, 
                    Color.Gold * opacity, 0.8f * opacity * scale);
            }
        }
    }

    private void CreateEffects()
    {
        if (Main.rand.NextBool(4))
        {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float dist = Main.rand.NextFloat(20f, 40f);
            Vector2 position = Projectile.Center + new Vector2(dist, 0).RotatedBy(angle);
            
            Dust.NewDustPerfect(position, DustID.GoldFlame, 
                (Projectile.Center - position) * 0.02f, 100, 
                Color.Gold * opacity, 0.6f * opacity * scale);
        }
        
        Lighting.AddLight(Projectile.Center, 
            0.6f * opacity, 
            0.5f * opacity, 
            0.15f * opacity);
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 15; i++)
        {
            Vector2 velocity = new Vector2(
                Main.rand.NextFloat(-3f, 3f), 
                Main.rand.NextFloat(-3f, 3f)
            );
            Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, velocity, 100, 
                Color.Gold * opacity, 1f * opacity * scale);
        }
        
        Lighting.AddLight(Projectile.Center, 
            1f * opacity, 
            0.8f * opacity, 
            0.3f * opacity);
    }
}