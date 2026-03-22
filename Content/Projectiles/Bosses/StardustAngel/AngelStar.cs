using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Ultraconyx.Content.Dusts;

namespace Ultraconyx.Content.Projectiles.Bosses.StardustAngel;

public class AngelStar : ModProjectile
{
    private int orbitTimer = 0;
    private int state = 0; // 0 = orbiting, 1 = charging
    private int shootCount = 0;
    private int starIndex = 0; // Which position this star is (0-3)
    private bool hasCharged = false;
    
    private Vector2[] trailPositions = new Vector2[5];
    private int trailIndex = 0;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 1;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    public override void SetDefaults()
    {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 600;
        Projectile.alpha = 0;
        Projectile.penetrate = -1;
        
        Projectile.light = 0.5f;
    }

    public void SetStarIndex(int index)
    {
        starIndex = index;
    }

    public override void AI()
    {
        // Store position for afterimage
        trailPositions[trailIndex] = Projectile.Center;
        trailIndex = (trailIndex + 1) % trailPositions.Length;

        // Spawn occasional stardust trail
        if (Main.rand.NextBool(5))
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Stardust>(), 
                Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 100, default, 0.8f);
            dust.noGravity = true;
        }

        // Check if this is an orbiting star (has parent NPC)
        if (Projectile.ai[1] >= 0 && Main.npc[(int)Projectile.ai[1]].active)
        {
            NPC parent = Main.npc[(int)Projectile.ai[1]];
            
            if (state == 0) // Orbiting state
            {
                orbitTimer++;
                
                // Orbit around the boss
                float orbitSpeed = 0.03f;
                float orbitAngle = Projectile.ai[0] + orbitTimer * orbitSpeed;
                float orbitRadius = 120f;
                
                Vector2 orbitPosition = parent.Center + new Vector2(
                    (float)Math.Cos(orbitAngle) * orbitRadius,
                    (float)Math.Sin(orbitAngle) * orbitRadius
                );
                
                Projectile.Center = orbitPosition;
                Projectile.velocity = Vector2.Zero;
                
                // Regular orbiting stars (ai[2] between 0-3)
                if (Projectile.ai[2] >= 0 && Projectile.ai[2] <= 3)
                {
                    // Shoot twice during orbiting
                    if (orbitTimer == 60) // First shot at 1 second
                    {
                        ShootAtPlayer(parent);
                        shootCount++;
                    }
                    else if (orbitTimer == 120) // Second shot at 2 seconds
                    {
                        ShootAtPlayer(parent);
                        shootCount++;
                    }
                    
                    // After shooting twice, all stars charge together
                    if (orbitTimer >= 150 && !hasCharged) // 2.5 seconds
                    {
                        state = 1;
                        hasCharged = true;
                        
                        Player target = Main.player[parent.target];
                        if (target != null && target.active)
                        {
                            Vector2 direction = target.Center - Projectile.Center;
                            direction.Normalize();
                            
                            // Start charging toward player
                            Projectile.velocity = direction * 15f;
                        }
                    }
                }
            }
            else if (state == 1) // Charging state
            {
                // Keep moving in the charge direction
                // Add trail dust while charging
                if (Main.rand.NextBool(2))
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Stardust>(), 
                        -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 100, default, 1.5f);
                    dust.noGravity = true;
                }
                
                // Star will despawn naturally when time runs out or goes off screen
            }
        }
        else
        {
            // No parent - just continue moving and despawn
            if (state == 1)
            {
                // Keep charging
                if (Main.rand.NextBool(2))
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Stardust>(), 
                        -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 100, default, 1.5f);
                    dust.noGravity = true;
                }
            }
        }
        
        Projectile.rotation += 0.05f;
    }

    private void ShootAtPlayer(NPC parent)
    {
        Player target = Main.player[parent.target];
        if (target != null && target.active)
        {
            Vector2 direction = target.Center - Projectile.Center;
            direction.Normalize();
            
            // Shoot 2 AngelMiniStars in a spread
            for (int i = -1; i <= 1; i += 2)
            {
                Vector2 spreadDir = direction.RotatedBy(i * 0.2f); // ~11.5 degree spread
                
                Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    Projectile.Center,
                    spreadDir * 8f,
                    ModContent.ProjectileType<AngelMiniStar>(),
                    Projectile.damage / 2,
                    1f,
                    Main.myPlayer
                );
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        // Draw afterimage trail
        Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
        
        for (int i = 0; i < trailPositions.Length; i++)
        {
            if (trailPositions[i] != Vector2.Zero)
            {
                float alpha = 0.3f * (1f - (i / (float)trailPositions.Length));
                Color trailColor = Color.White * alpha;
                Main.EntitySpriteDraw(
                    texture,
                    trailPositions[i] - Main.screenPosition,
                    null,
                    trailColor,
                    Projectile.rotation,
                    origin,
                    Projectile.scale * 0.8f,
                    SpriteEffects.None,
                    0
                );
            }
        }
        
        // Draw main projectile
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

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 8; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Stardust>(), 
                Projectile.velocity.X * 0.3f, Projectile.velocity.Y * 0.3f, 100, default, 1f);
            dust.noGravity = true;
        }
    }
}