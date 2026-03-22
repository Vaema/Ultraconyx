using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Projectiles;

public class LivingWoodSwordProjectile : ModProjectile
{
    private float rotationSpeed = 0.3f;
    private int bounceCount;
    private const int maxBounces = 2; 
    
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 180;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        
        Projectile.aiStyle = -1; 
        Projectile.extraUpdates = 1; 
    }

    public override void AI()
    {

        Projectile.velocity.Y += 0.2f; 
        
        Projectile.rotation += rotationSpeed;
        
        rotationSpeed += Main.rand.NextFloat(-0.01f, 0.01f); 
        rotationSpeed = MathHelper.Clamp(rotationSpeed, 0.1f, 0.5f); 
        
        Projectile.velocity.X *= 0.99f;
        
        if (Main.rand.NextBool(5))
        {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 
                DustID.Grass, 0f, 0f, 150, default(Color), 0.8f);
        }
        
        if (Projectile.timeLeft < 30)
        {
            Projectile.alpha += 8;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Main.rand.NextBool(5))
        {
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(target.position, target.width, target.height, 
                    DustID.Grass, 0f, 0f, 0, default(Color), 1.2f);
            }
        }
        
        SoundEngine.PlaySound(SoundID.NPCHit1 with { Volume = 0.5f }, Projectile.position);
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        bounceCount++;
        
        rotationSpeed *= -1.5f; 
        
        if (Projectile.velocity.X != oldVelocity.X)
            Projectile.velocity.X = -oldVelocity.X * 0.7f;
        if (Projectile.velocity.Y != oldVelocity.Y)
            Projectile.velocity.Y = -oldVelocity.Y * 0.7f;
        
        SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.3f }, Projectile.position);
        
        for (int i = 0; i < 3; i++)
        {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 
                DustID.WoodFurniture, 0f, 0f, 0, default(Color), 0.6f);
        }
        
        if (bounceCount >= maxBounces)
        {
            SoundEngine.PlaySound(SoundID.NPCHit1 with { Volume = 0.5f }, Projectile.position);
            
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.Grass, 0f, 0f, 0, default(Color), 1f);
            }
            return true;
        }
        
        return false; 
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 5; i++)
        {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 
                DustID.Grass, 0f, 0f, 0, default(Color), 1f);
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return true;
    }
}