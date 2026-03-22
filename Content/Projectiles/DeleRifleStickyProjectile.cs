using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Projectiles;

public class DeleRifleStickyProjectile : ModProjectile
{
    private NPC targetNPC;
    private int damageTimer;
    private Vector2 stuckOffset;
    private bool isStuck;

    public override void SetDefaults()
    {
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 180;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.alpha = 128;
    }
    
    public override void AI()
    {
        // Find and stick to the target NPC
        if (Projectile.ai[0] >= 0 && Projectile.ai[0] < Main.maxNPCs)
        {
            targetNPC = Main.npc[(int)Projectile.ai[0]];
            
            if (targetNPC.active && !targetNPC.dontTakeDamage)
            {
                // First frame: calculate where to stick relative to NPC
                if (!isStuck)
                {
                    // Calculate a random offset on the NPC's surface
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float radius = Math.Max(targetNPC.width, targetNPC.height) * 0.4f;
                    stuckOffset = new Vector2(
                        (float)Math.Cos(angle) * radius,
                        (float)Math.Sin(angle) * radius
                    );
                    isStuck = true;
                    
                    // Stop all movement
                    Projectile.velocity = Vector2.Zero;
                    Projectile.tileCollide = false;
                }
                
                // Simply follow the NPC by updating position based on NPC's current position
                // This doesn't modify the NPC, just calculates where the projectile should be
                Projectile.Center = targetNPC.Center + stuckOffset;
                
                // Remove rotation to stop spinning
                Projectile.rotation = 0f;
                
                // Deal damage over time every 30 frames (0.5 seconds at 60 FPS)
                damageTimer++;
                if (damageTimer >= 30)
                {
                    damageTimer = 0;
                    
                    // Deal damage directly
                    targetNPC.StrikeNPC(
                        new NPC.HitInfo()
                        {
                            Damage = Projectile.damage,
                            Knockback = 0f, // No knockback to avoid moving NPC
                            HitDirection = 0
                        }
                    );
                    
                    // Create damage effect AT THE PROJECTILE'S POSITION, not NPC's center
                    for (int i = 0; i < 3; i++)
                    {
                        Dust dust = Dust.NewDustDirect(
                            Projectile.position,
                            Projectile.width,
                            Projectile.height,
                            DustID.GemTopaz,
                            Main.rand.NextFloat(-1f, 1f),
                            Main.rand.NextFloat(-1f, 1f),
                            100,
                            default(Color),
                            0.6f
                        );
                        dust.noGravity = true;
                    }
                }
                
                // Create continuous particle effect
                if (Main.rand.NextBool(5))
                {
                    Dust dust = Dust.NewDustDirect(
                        Projectile.position,
                        Projectile.width,
                        Projectile.height,
                        DustID.GemTopaz,
                        0f,
                        0f,
                        100,
                        default(Color),
                        0.4f
                    );
                    dust.noGravity = true;
                    dust.velocity *= 0.1f;
                }
                
                return;
            }
        }
        
        // If no valid target, fade out and die
        Projectile.alpha += 5;
        Projectile.velocity = Vector2.Zero;
        if (Projectile.alpha >= 255)
        {
            Projectile.Kill();
        }
    }
    
    public override void OnKill(int timeLeft)
    {
        // Small explosion on death
        for (int i = 0; i < 8; i++)
        {
            Dust dust = Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.GemTopaz,
                Main.rand.NextFloat(-1.5f, 1.5f),
                Main.rand.NextFloat(-1.5f, 1.5f),
                100,
                default(Color),
                0.7f
            );
            dust.noGravity = true;
        }
    }
    
    // Make sure the projectile doesn't do collision damage (we handle damage in AI)
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.FinalDamage *= 0f; // No initial hit damage
    }
}