using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace Ultraconyx.Content.VanillaChanges;

public class MageChanges : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return entity.type == ItemID.DiamondStaff || entity.type == ItemID.AmethystStaff;
    }

    public override void SetDefaults(Item entity)
    {
        if (entity.type == ItemID.DiamondStaff)
        {
            entity.shoot = ModContent.ProjectileType<DiamondStaffProj>();
            entity.shootSpeed = 12f;
        }
        else if (entity.type == ItemID.AmethystStaff)
        {
            entity.shoot = ModContent.ProjectileType<AmethystStaffProj>();
            entity.shootSpeed = 16f; // Increased from 8f to 16f for much better velocity
        }
    }

    public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (item.type == ItemID.DiamondStaff)
        {
            float speed = velocity.Length();
            Vector2 baseVelocity = Vector2.Normalize(velocity) * speed;
            
            // Create two projectiles - use ai0 for phase (0 or 1), leave ai1 for center X, ai2 for center Y
            Projectile.NewProjectile(source, position, baseVelocity, ModContent.ProjectileType<DiamondStaffProj>(), damage, knockback, player.whoAmI, 0, 0); // Phase 0
            Projectile.NewProjectile(source, position, baseVelocity, ModContent.ProjectileType<DiamondStaffProj>(), damage, knockback, player.whoAmI, 1, 0); // Phase 1
            
            return false;
        }
        else if (item.type == ItemID.AmethystStaff)
        {
            // Fast-moving primary projectile with new velocity
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<AmethystStaffProj>(), damage, knockback, player.whoAmI, 0, 0);
            return false;
        }
        
        return true;
    }
}

public class DiamondStaffProj : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DiamondBolt;
    
    private float OrbitRadius = 20f; // Increased for more visible pattern
    private float OrbitSpeed = 0.25f;
    
    // Store phase in ai[0] (0 or 1)
    private float Phase => Projectile.ai[0];
    
    // Store center position in ai[1] and ai[2]
    private Vector2 CenterPosition
    {
        get => new(Projectile.ai[1], Projectile.ai[2]);
        set
        {
            Projectile.ai[1] = value.X;
            Projectile.ai[2] = value.Y;
        }
    }
    
    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.DiamondBolt);
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.penetrate = 2;
        Projectile.extraUpdates = 1;
        Projectile.tileCollide = true;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.alpha = 50;
        Projectile.aiStyle = -1;
        
        Projectile.localAI[0] = 0; // Time/progress
        Projectile.localAI[1] = 0; // Initialization flag
    }

    public override void AI()
    {
        // Initialize center position if this is the first frame
        if (Projectile.localAI[1] == 0)
        {
            CenterPosition = Projectile.Center;
            Projectile.localAI[1] = 1;
        }
        
        // Move the center position forward with the velocity
        CenterPosition += Projectile.velocity;
        
        // Get direction vectors
        Vector2 forward = Projectile.velocity;
        forward.Normalize();
        
        // Perpendicular vectors for orbit
        Vector2 up = new(-forward.Y, forward.X);
        Vector2 right = forward;
        
        // Update time
        Projectile.localAI[0] += OrbitSpeed;
        
        // Calculate orbit position
        float angle = Projectile.localAI[0];
        
        // Phase 1 is offset by 180 degrees
        if (Phase == 1)
            angle += MathHelper.Pi;
        
        // Create DNA helix pattern using sin and cos for circular orbit
        Vector2 orbitOffset = (up * (float)Math.Sin(angle) + right * (float)Math.Cos(angle)) * OrbitRadius;
        
        // Set the projectile position to center + orbit offset
        Projectile.Center = CenterPosition + orbitOffset;
        
        // Visual effects
        Lighting.AddLight(Projectile.Center, 0.3f, 0.6f, 1.0f);
        
        // MAIN TRAIL - Reduced dust
        if (Main.rand.NextBool(3)) // Reduced from 50% to 33% chance
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemDiamond, 
                Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 80, default, Main.rand.NextFloat(1.2f, 1.8f));
            dust.noGravity = true;
            dust.velocity *= 0.3f;
        }
        
        // ORBIT PATH DUST - Reduced frequency
        for (int i = 0; i < 2; i++) // Reduced from 4 to 2 points
        {
            if (Main.rand.NextBool(3)) // Reduced from 50% to 33% chance
            {
                float trailAngle = angle + (i * MathHelper.Pi);
                Vector2 trailPos = CenterPosition + (up * (float)Math.Sin(trailAngle) + right * (float)Math.Cos(trailAngle)) * OrbitRadius * 0.8f;
                
                Dust dust = Dust.NewDustDirect(trailPos - new Vector2(4, 4), 2, 2, DustID.BlueTorch, 
                    0f, 0f, 60, default, 0.9f);
                dust.noGravity = true;
                dust.velocity = Projectile.velocity * 0.1f;
            }
        }
        
        // SPARKLE EFFECT - Reduced chance
        if (Main.rand.NextBool(4)) // Reduced from 50% to 25% chance
        {
            Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(4, 4), 0, 0, DustID.WhiteTorch, 
                0f, 0f, 50, default, 1.1f);
            dust.noGravity = true;
            dust.velocity = Vector2.Zero;
        }
        
        // TRAIL BEHIND - Reduced chance
        if (Main.rand.NextBool(5)) // Reduced from 33% to 20% chance
        {
            Vector2 trailPos = Projectile.Center - Projectile.velocity * 0.5f;
            Dust dust = Dust.NewDustDirect(trailPos - new Vector2(4, 4), 2, 2, DustID.GemDiamond, 
                0f, 0f, 70, default, 1.0f);
            dust.noGravity = true;
            dust.velocity *= 0.1f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        for (int i = 0; i < 8; i++)
        {
            Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.GemDiamond, 
                Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1.5f);
            dust.noGravity = true;
            dust.velocity *= 1.5f;
        }
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 12; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemDiamond, 
                Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100, default, 1.8f);
            dust.noGravity = true;
            dust.velocity *= 2f;
        }
    }
    
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        
        Main.EntitySpriteDraw(texture, drawPosition, null, lightColor, Projectile.rotation, 
            texture.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
        
        return false;
    }
}

public class AmethystStaffProj : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.AmethystBolt;
    
    private bool hasSplit;
    private Vector2 hitPosition = Vector2.Zero;
    
    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.AmethystBolt);
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.penetrate = 2; // Pierces 1 enemy (hits 2 total)
        Projectile.extraUpdates = 1; // Added extraUpdates for smoother movement at high speed
        Projectile.tileCollide = true;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.alpha = 50;
        Projectile.aiStyle = -1;
        Projectile.timeLeft = 600; // Increased timeLeft to account for higher speed
    }

    public override void AI()
    {
        // Visual effects
        Lighting.AddLight(Projectile.Center, 0.6f, 0.2f, 0.8f);
        
        // Reduced dust trail - only occasional sparkles
        if (Main.rand.NextBool(5)) // Reduced from 33% to 20% chance
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemAmethyst, 
                Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 60, default, Main.rand.NextFloat(0.8f, 1.2f));
            dust.noGravity = true;
            dust.velocity *= 0.2f;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (!hasSplit)
        {
            hitPosition = Projectile.Center;
            SplitProjectile();
            hasSplit = true;
        }
        return true; // Destroy on tile collision
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (!hasSplit)
        {
            // Store the position where we hit the enemy
            hitPosition = target.Center;
            SplitProjectile();
            hasSplit = true;
        }
    }

    private void SplitProjectile()
    {
        if (Projectile.owner == Main.myPlayer)
        {
            int damage = Projectile.damage / 3; // Reduced damage even more for split projectiles (was /2)
            
            // Much wider spread angles
            float spreadAngle = 45f; // Increased from 20f to 45f for wider spread
            
            for (int i = -1; i <= 1; i++)
            {
                if (i == 0) continue; // Skip 0, shoot at -45°, 45° angles (much wider spread)
                
                // Base spread angle with wider range
                float baseAngle = MathHelper.ToRadians(spreadAngle * i);
                
                // Add more randomness for natural spread
                float randomOffset = Main.rand.NextFloat(-0.25f, 0.25f); // Increased randomness range
                
                // Calculate the direction away from the hit point
                if (hitPosition != Vector2.Zero)
                {
                    Vector2 toHitPoint = hitPosition - Projectile.Center;
                    toHitPoint.Normalize();
                    
                    // Calculate perpendicular direction to create a "burst" effect
                    Vector2 perpendicular = new(-toHitPoint.Y, toHitPoint.X);
                    
                    // Combine away direction with spread - make it more perpendicular for wider spread
                    Vector2 direction = Vector2.Lerp(-toHitPoint, perpendicular * i, 0.8f); // Increased from 0.7f to 0.8f for more perpendicular spread
                    direction.Normalize();
                    
                    // Add more randomness
                    direction = direction.RotatedBy(randomOffset);
                    
                    // Scale to appropriate speed
                    Vector2 newVelocity = direction * 6f; // Reduced from 8f to 6f for slower split projectiles
                    
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, newVelocity, 
                        ModContent.ProjectileType<AmethystSplitProj>(), damage, Projectile.knockBack * 0.3f, Projectile.owner, 0, 30); // ai1 = 30 frames (0.5 seconds)
                }
                else
                {
                    // Fallback if no hit position (shouldn't happen)
                    Vector2 newVelocity = Projectile.velocity.RotatedBy(baseAngle + randomOffset);
                    newVelocity.Normalize();
                    newVelocity *= 6f; // Reduced from 8f to 6f
                    
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, newVelocity, 
                        ModContent.ProjectileType<AmethystSplitProj>(), damage, Projectile.knockBack * 0.3f, Projectile.owner, 0, 30);
                }
            }
            
            // Reduced visual effect for splitting
            for (int j = 0; j < 5; j++) // Reduced from 10 to 5
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(4, 4), 0, 0, DustID.GemAmethyst, 
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 80, default, 1.0f);
                dust.noGravity = true;
                dust.velocity *= 1.2f;
            }
        }
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 5; i++) // Reduced from 8 to 5
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemAmethyst, 
                Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 80, default, 1.2f);
            dust.noGravity = true;
            dust.velocity *= 1.5f;
        }
    }
}

public class AmethystSplitProj : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.AmethystBolt;
    
    // Store linger time in ai[1] (frames)
    private int LingerTime => (int)Projectile.ai[1];
    private int lingerCounter;
    private bool isHoming;

    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.AmethystBolt);
        Projectile.width = 8;
        Projectile.height = 8;
        Projectile.penetrate = 1; // Can only hit one enemy
        Projectile.extraUpdates = 1;
        Projectile.tileCollide = true;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.alpha = 60;
        Projectile.aiStyle = -1;
        Projectile.timeLeft = 300; // Longer total lifespan
    }

    public override void AI()
    {
        // Count frames for linger period
        if (!isHoming)
        {
            lingerCounter++;
            
            // During linger, projectile cannot deal damage
            Projectile.friendly = false;
            
            // After linger time, start homing
            if (lingerCounter >= LingerTime)
            {
                isHoming = true;
                Projectile.friendly = true; // Re-enable damage
                
                // Visual pulse when homing starts
                for (int i = 0; i < 3; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(4, 4), 0, 0, DustID.GemAmethyst, 
                        0f, 0f, 70, default, 0.8f);
                    dust.noGravity = true;
                    dust.velocity = Main.rand.NextVector2Circular(1f, 1f);
                }
            }
            
            // During linger, almost no movement - just slight deceleration
            Projectile.velocity *= 0.98f;
        }
        
        if (isHoming)
        {
            // Much weaker homing behavior
            float homingRange = 350f; // Reduced from 500f
            float homingStrength = 0.08f; // Greatly reduced from 0.2f for much weaker homing
            
            NPC target = null;
            float closestDistance = homingRange;
            
            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy() && Vector2.Distance(Projectile.Center, npc.Center) < closestDistance)
                {
                    closestDistance = Vector2.Distance(Projectile.Center, npc.Center);
                    target = npc;
                }
            }
            
            if (target != null)
            {
                Vector2 direction = target.Center - Projectile.Center;
                direction.Normalize();
                
                // Slower acceleration and lower max speed
                float currentSpeed = Projectile.velocity.Length();
                float targetSpeed = 8f; // Reduced from 12f
                
                if (currentSpeed < targetSpeed)
                {
                    currentSpeed += 0.15f; // Slower acceleration (was 0.3f)
                    if (currentSpeed > targetSpeed)
                        currentSpeed = targetSpeed;
                }
                
                // Much weaker homing - only slightly adjusts trajectory
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * currentSpeed, homingStrength);
                
                // Add some random wobble to make homing less precise
                if (Main.rand.NextBool(3))
                {
                    Projectile.velocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.05f, 0.05f));
                }
            }
            else
            {
                // If no target, slowly decelerate
                Projectile.velocity *= 0.99f;
            }
        }
        
        // Visual effects - very minimal during linger
        Lighting.AddLight(Projectile.Center, 0.6f, 0.2f, 0.8f);
        
        // Occasional dust during linger, more when homing
        int dustChance = isHoming ? 4 : 8; // Even less dust overall (was 3 and 6)
        if (Main.rand.NextBool(dustChance))
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemAmethyst, 
                Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 60, default, Main.rand.NextFloat(0.5f, 0.8f)); // Smaller dust
            dust.noGravity = true;
            dust.velocity *= 0.1f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        for (int i = 0; i < 3; i++) // Reduced from 4 to 3
        {
            Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.GemAmethyst, 
                Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), 60, default, 0.9f); // Smaller dust
            dust.noGravity = true;
        }
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 3; i++) // Reduced from 4 to 3
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemAmethyst, 
                Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 60, default, 0.9f); // Smaller dust
            dust.noGravity = true;
        }
    }
}