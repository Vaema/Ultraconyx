using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ultraconyx.Content.Projectiles;

public class SmallHomingScythe : ModProjectile
{
    private bool isBouncing = true;
    private int bounceTimer;
    private const int BounceDuration = 18; // 0.3 seconds at 60 FPS
    private Vector2 originalVelocity;

    public override void SetDefaults()
    {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1; // Infinite penetrations during bounce phase
        Projectile.tileCollide = true; // Enable tile collision for wall bouncing
        Projectile.timeLeft = 180;
        
        // Enable afterimage effect
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (isBouncing)
        {
            // Bounce off walls like Shimmer
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.8f;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 0.8f;
            }
            return false; // Don't kill the projectile
        }
        return true; // Kill the projectile after bounce phase
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (isBouncing)
        {
            // Bounce off enemies like Shimmer
            Vector2 bounceDir = Projectile.Center - target.Center;
            bounceDir.Normalize();
            Projectile.velocity = bounceDir * originalVelocity.Length() * 0.8f;
            
            // Reset penetration to keep bouncing
            Projectile.penetrate = -1;
        }
    }

    public override void AI()
    {
        Projectile.rotation += 0.5f;

        // Manage afterimage positions - only track 3 positions
        if (Projectile.velocity.Length() > 2f)
        {
            // Update the trail positions
            Projectile.oldPos[2] = Projectile.oldPos[1];
            Projectile.oldPos[1] = Projectile.oldPos[0];
            Projectile.oldPos[0] = Projectile.position;
        }

        if (isBouncing)
        {
            HandleBouncePhase();
        }
        else
        {
            HandleHomingPhase();
        }
    }

    private void HandleBouncePhase()
    {
        // Store original velocity on first frame
        if (bounceTimer == 0)
        {
            originalVelocity = Projectile.velocity;
        }

        bounceTimer++;

        // Apply some drag to make it look more like Shimmer
        Projectile.velocity *= 0.98f;

        // Check if bounce phase should end
        if (bounceTimer >= BounceDuration)
        {
            isBouncing = false;
            Projectile.penetrate = 1; // Only penetrate once during homing phase
            Projectile.tileCollide = false; // Disable tile collision during homing
            
            // Give it a small initial velocity for homing
            if (Projectile.velocity.Length() < 3f)
            {
                Projectile.velocity = Vector2.UnitX * 3f;
            }
        }
    }

    private void HandleHomingPhase()
    {
        // FASTER HOMING: Increased homing strength and max speed
        float homingStrength = 0.3f; // Increased from 0.2f
        float maxSpeed = 14f; // Increased from 10f

        NPC target = FindNearestTarget(500f);
        
        if (target != null)
        {
            Vector2 direction = target.Center - Projectile.Center;
            direction.Normalize();
            direction *= maxSpeed;

            Projectile.velocity = 
                (Projectile.velocity * (1f - homingStrength)) + 
                (direction * homingStrength);

            // Limit maximum speed
            if (Projectile.velocity.Length() > maxSpeed)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= maxSpeed;
            }
        }
    }

    private NPC FindNearestTarget(float maxDistance)
    {
        NPC nearestTarget = null;
        float currentDistance = maxDistance;

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (npc.CanBeChasedBy(this))
            {
                float distance = Vector2.Distance(npc.Center, Projectile.Center);
                if (distance < currentDistance)
                {
                    nearestTarget = npc;
                    currentDistance = distance;
                }
            }
        }

        return nearestTarget;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Rectangle frame = texture.Frame();
        Vector2 origin = frame.Size() / 2f;
        
        // Draw 3 afterimages
        for (int i = 0; i < 3; i++)
        {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;
                
            float progress = 1f - (i / 3f); // Fades from 1.0 to 0.33
            Color afterimageColor = lightColor * progress * 0.6f; // Reduced opacity multiplier
            Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + origin;
            
            Main.EntitySpriteDraw(
                texture,
                drawPos,
                frame,
                afterimageColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );
        }
        
        // Draw main projectile
        Main.EntitySpriteDraw(
            texture,
            Projectile.Center - Main.screenPosition,
            frame,
            lightColor,
            Projectile.rotation,
            origin,
            Projectile.scale,
            SpriteEffects.None,
            0
        );
        
        return false; // Return false so we don't draw the original
    }
}