using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Projectiles;

public class DeleRifleBigProjectile : ModProjectile
{
    private enum State
    {
        Growing,
        Shooting,
        Flying
    }
    
    private State currentState = State.Growing;
    private float scale = 0.1f;
    private const float MaxScale = 1f;
    private const float GrowSpeed = 0.015f;
    private Vector2 targetVelocity;
    private float baseRotation;

    public override void SetDefaults()
    {
        Projectile.width = 40;
        Projectile.height = 40;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 900;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
    }

    public override void AI()
    {
        switch (currentState)
        {
            case State.Growing:
                AI_Growing();
                break;
            case State.Shooting:
                AI_Shooting();
                break;
            case State.Flying:
                AI_Flying();
                break;
        }
        
        // Always create Topaz dust
        CreateDust();
        
        // Add light
        Lighting.AddLight(Projectile.Center, 1f, 0.85f, 0f);
    }
    
    private void AI_Growing()
    {
        // Smooth rotation from the base
        baseRotation += 0.02f;
        Projectile.rotation = baseRotation;
        
        // Increase scale
        scale += GrowSpeed;
        Projectile.scale = scale;
        
        // When reaching max size, transition to shooting state
        if (scale >= MaxScale)
        {
            scale = MaxScale;
            currentState = State.Shooting;
            
            // Calculate velocity toward nearest enemy or mouse position
            Vector2 targetPos = Main.MouseWorld;
            NPC nearestEnemy = FindNearestEnemy(Projectile.Center, 800f);
            if (nearestEnemy != null)
            {
                targetPos = nearestEnemy.Center;
            }
            
            Vector2 direction = Vector2.Normalize(targetPos - Projectile.Center);
            targetVelocity = direction * 20f;
            Projectile.timeLeft = 300; // Reset time left for flying state
            
            // Align rotation with firing direction
            baseRotation = direction.ToRotation() + MathHelper.PiOver2;
        }
        
        // Stay anchored to gun tip position (with offset for projectile size)
        Player player = Main.player[Projectile.owner];
        if (player.active)
        {
            // Calculate where the projectile should be based on gun tip
            Vector2 gunDirection = Vector2.Normalize(Main.MouseWorld - player.Center);
            if (float.IsNaN(gunDirection.X) || float.IsNaN(gunDirection.Y))
            {
                gunDirection = -Vector2.UnitX * player.direction;
            }
            
            // Position the projectile so its EDGE is at the gun tip, not its center
            Vector2 gunTip = player.Center + gunDirection * 40f;
            Vector2 projectileOffset = gunDirection * 20f; // Half of projectile width
            Projectile.Center = gunTip + projectileOffset;
        }
    }
    
    private void AI_Shooting()
    {
        // Brief pause before shooting - rotate smoothly
        Projectile.velocity = Vector2.Zero;
        baseRotation += 0.1f;
        Projectile.rotation = baseRotation;
        
        if (Projectile.timeLeft < 295) // After 5 frames of pause
        {
            currentState = State.Flying;
            Projectile.velocity = targetVelocity;
            Projectile.tileCollide = true;
        }
    }
    
    private void AI_Flying()
    {
        // Smooth rotation aligned with velocity
        if (Projectile.velocity.Length() > 0.1f)
        {
            float targetRotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            // Smooth interpolation toward target rotation
            baseRotation = MathHelper.Lerp(baseRotation, targetRotation, 0.15f);
            Projectile.rotation = baseRotation;
        }
        
        // Accelerate slightly
        if (Projectile.velocity.Length() < 30f)
        {
            Projectile.velocity *= 1.02f;
        }
    }
    
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        // Spawn 3-5 DeleRifleStickyProjectiles that stick to enemy
        int projectileCount = Main.rand.Next(3, 6);
        for (int i = 0; i < projectileCount; i++)
        {
            Vector2 offset = new(
                Main.rand.NextFloat(-target.width * 0.5f, target.width * 0.5f),
                Main.rand.NextFloat(-target.height * 0.5f, target.height * 0.5f)
            );
            
            int proj = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                target.Center + offset,
                Vector2.Zero,
                ModContent.ProjectileType<DeleRifleStickyProjectile>(),
                (int)(Projectile.damage * 0.4f),
                0f,
                Projectile.owner,
                target.whoAmI
            );
            
            if (Main.projectile.IndexInRange(proj))
            {
                Main.projectile[proj].timeLeft = 180;
            }
        }
        
        // Create hit effect
        for (int i = 0; i < 20; i++)
        {
            Dust dust = Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.GemTopaz,
                Main.rand.NextFloat(-3f, 3f),
                Main.rand.NextFloat(-3f, 3f),
                100,
                default(Color),
                1.5f
            );
            dust.noGravity = true;
        }
    }
    
    private void CreateDust()
    {
        if (Main.rand.NextBool(2))
        {
            Dust dust = Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.GemTopaz,
                Projectile.velocity.X * 0.1f,
                Projectile.velocity.Y * 0.1f,
                100,
                default(Color),
                1.2f * scale
            );
            dust.noGravity = true;
            dust.velocity *= 0.3f;
        }
    }
    
    private NPC FindNearestEnemy(Vector2 position, float maxDistance)
    {
        NPC nearest = null;
        float nearestDist = maxDistance * maxDistance;
        
        foreach (NPC npc in Main.npc)
        {
            if (npc.CanBeChasedBy())
            {
                float dist = Vector2.DistanceSquared(position, npc.Center);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = npc;
                }
            }
        }
        
        return nearest;
    }
    
    public override void OnKill(int timeLeft)
    {
        // Big explosion effect
        for (int i = 0; i < 30; i++)
        {
            Dust dust = Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.GemTopaz,
                Main.rand.NextFloat(-5f, 5f),
                Main.rand.NextFloat(-5f, 5f),
                100,
                default(Color),
                2f
            );
            dust.noGravity = true;
        }
    }
}