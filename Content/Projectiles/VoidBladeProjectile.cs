using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics;

namespace Ultraconyx.Content.Projectiles;

public class VoidBladeProjectile : ModProjectile
{
    private const int StickTime = 180; // 3 seconds (60 ticks per second)
    private const int DOTInterval = 30; // Damage every half second
    private int stuckEnemy = -1;
    private Vector2 offset; // Offset from enemy center when stuck
    
    // For VertexStrip trail
    private VertexStrip _trailStrip = new();
    private List<Vector2> _oldPositions = [];
    private const int TRAIL_LENGTH = 25; // Trail length

    public override void SetStaticDefaults()
    {
        // DisplayName.SetDefault("Void Blade");
        Main.projFrames[Projectile.type] = 1;
    }

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.aiStyle = -1; // Custom AI
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1; // Infinite penetration for sticking
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = 600; // 10 seconds max lifespan
        Projectile.extraUpdates = 1; // For smoother movement
        
        // Better scaling
        Projectile.scale = 1f;
    }

    public override void AI()
    {
        // If stuck to an enemy
        if (stuckEnemy != -1)
        {
            NPC target = Main.npc[stuckEnemy];
            
            // Check if enemy is still valid
            if (!target.active || target.life <= 0)
            {
                Projectile.Kill();
                return;
            }

            // Follow the enemy
            Projectile.position = target.position + offset;
            
            // Clear trail when stuck
            _oldPositions.Clear();
            
            // Deal DoT
            if (Projectile.ai[0] >= DOTInterval)
            {
                if (Projectile.ai[1] < StickTime)
                {
                    // Deal damage to the stuck enemy
                    int damage = Projectile.damage / 4; // 25% of base damage every half second
                    
                    // Create a HitInfo object for the damage
                    NPC.HitInfo hitInfo = new()
                    {
                        Damage = damage,
                        Knockback = 0f,
                        HitDirection = 0,
                        Crit = false
                    };
                    
                    target.StrikeNPC(hitInfo, false, true);
                    
                    // Visual effects
                    for (int i = 0; i < 5; i++)
                    {
                        Dust.NewDust(target.position, target.width, target.height, DustID.Shadowflame, 0f, 0f, 100, default, 1.5f);
                    }
                    
                    Projectile.ai[0] = 0;
                    Projectile.ai[1] += DOTInterval;
                }
                else
                {
                    // Time's up - projectile falls off
                    Projectile.Kill();
                }
            }
            else
            {
                Projectile.ai[0]++;
            }
            
            return;
        }

        // Free-moving AI
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4; // Adjust for sprite pointing top-right
        
        // Track old positions for trail
        if (_oldPositions.Count > 0)
        {
            // Only add new position if it's different enough from the last one
            Vector2 lastPos = _oldPositions[_oldPositions.Count - 1];
            if (Vector2.DistanceSquared(Projectile.Center, lastPos) > 4f)
            {
                _oldPositions.Add(Projectile.Center);
            }
        }
        else
        {
            _oldPositions.Add(Projectile.Center);
        }
        
        // Limit trail length
        while (_oldPositions.Count > TRAIL_LENGTH)
        {
            _oldPositions.RemoveAt(0);
        }

        // Gravity effect (slight downward curve)
        Projectile.velocity.Y += 0.1f;
        
        // Cap the velocity
        if (Projectile.velocity.Length() > 20f)
        {
            Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 20f;
        }
    }

    // Color function for the vertex strip
    private Color StripColor(float progressOnStrip)
    {
        // Progress is 0 at the projectile (front) and 1 at the trail end (back)
        
        // Create a purple to transparent gradient
        Color color = Color.Lerp(Color.Purple, Color.Magenta, progressOnStrip * 0.5f);
        
        // Fade out toward the tip
        float alpha = 1f - progressOnStrip * 0.8f;
        
        return color * alpha;
    }

    // Width function for the vertex strip - creates consistent triangle shape
    private float StripWidth(float progressOnStrip)
    {
        // Progress is 0 at the projectile (front) and 1 at the trail end (back)
        
        // For a consistent triangle shape pointing backward:
        // - Widest at the front (near projectile)
        // - Tapers linearly to a point at the back
        
        // Simple linear triangle (widest at front, point at back)
        return MathHelper.Lerp(24f, 0f, progressOnStrip) * Projectile.scale;
    }

    // Draw afterimage trail using VertexStrip
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        Vector2 origin = new(texture.Width * 0.5f, texture.Height * 0.5f);
        
        // Draw vertex strip trail if we have enough points and not stuck
        if (stuckEnemy == -1 && _oldPositions.Count > 3)
        {
            // Create rotations array that always points backward from the projectile
            // This ensures the trail consistently forms a ">" shape
            float[] rotations = new float[_oldPositions.Count];
            
            // Set all rotations to point in the direction opposite to the projectile's movement
            // This makes the trail always face away from the projectile
            float baseRotation = Projectile.velocity.ToRotation() + MathHelper.Pi; // Opposite direction
            
            for (int i = 0; i < _oldPositions.Count; i++)
            {
                // Use the same rotation for all points to keep the trail consistent
                // No twisting or shape-changing
                rotations[i] = baseRotation;
            }
            
            // Alternative: Use direction from each point to the next for a more natural flow
            // This creates a trail that follows the path but still maintains consistent orientation
            /*
            for (int i = 0; i < _oldPositions.Count - 1; i++)
            {
                Vector2 direction = _oldPositions[i + 1] - _oldPositions[i];
                if (direction != Vector2.Zero)
                {
                    rotations[i] = direction.ToRotation();
                }
                else
                {
                    rotations[i] = baseRotation;
                }
            }
            rotations[_oldPositions.Count - 1] = rotations[_oldPositions.Count - 2];
            */
            
            // Convert positions list to array
            Vector2[] positionsArray = _oldPositions.ToArray();
            
            // Draw the vertex strip trail
            _trailStrip.PrepareStrip(
                positionsArray,
                rotations,
                StripColor,
                StripWidth,
                -Main.screenPosition,
                _oldPositions.Count,
                includeBacksides: true
            );
            
            // Draw the trail
            _trailStrip.DrawTrail();
        }
        
        // Draw the projectile itself with full brightness
        Color drawColor = Projectile.GetAlpha(lightColor);
        drawColor.A = 255;
        
        Main.EntitySpriteDraw(
            texture,
            Projectile.Center - Main.screenPosition,
            null,
            drawColor,
            Projectile.rotation,
            origin,
            Projectile.scale,
            SpriteEffects.None,
            0
        );
        
        // Add an extra glow effect
        Main.EntitySpriteDraw(
            texture,
            Projectile.Center - Main.screenPosition,
            null,
            Color.Purple * 0.3f,
            Projectile.rotation,
            origin,
            Projectile.scale * 1.2f,
            SpriteEffects.None,
            0
        );
        
        return false; // Prevent default drawing
    }

    // Fixed OnHitNPC method for tModLoader 1.4.4
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        // Only stick if not already stuck to something
        if (stuckEnemy == -1 && target.life > 0)
        {
            // Stick to the enemy
            stuckEnemy = target.whoAmI;
            offset = Projectile.position - target.position;
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            
            // Clear trail when sticking
            _oldPositions.Clear();
            
            // Reset AI counters
            Projectile.ai[0] = 0;
            Projectile.ai[1] = 0;
            
            // Visual and sound feedback
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            
            // Create impact dust
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(target.position, target.width, target.height, DustID.Shadowflame, 0f, 0f, 150, default, 1.5f);
            }
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        // Instead of bouncing, just kill the projectile on tile collision
        if (stuckEnemy == -1)
        {
            // Visual effect on impact
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 100, default, 1.5f);
            }
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            
            Projectile.Kill();
        }
        
        return false;
    }

    public override void OnKill(int timeLeft)
    {
        // Visual effects on death
        for (int i = 0; i < 20; i++)
        {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 100, default, 2f);
        }
        SoundEngine.PlaySound(SoundID.DD2_BetsysWrathImpact, Projectile.position);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        // Initial hit does full damage
        if (stuckEnemy != -1)
        {
            // You can modify damage here if needed
            // For example, to prevent damage when already stuck:
            // modifiers.Disable();
        }
    }
}