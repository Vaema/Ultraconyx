using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace Ultraconyx.Content.Projectiles;

public class DeleProjectile : ModProjectile
{
    private int frame;
    private int frameCounter;

    public override void SetDefaults()
    {
        Projectile.width = 16;  // Width of your horizontal sprite
        Projectile.height = 8;   // Height of your horizontal sprite
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 600;
        Projectile.ignoreWater = false;
        Projectile.tileCollide = true;
        Projectile.extraUpdates = 1;
        Projectile.aiStyle = -1; // Custom AI
        
        // Important for animation
        Main.projFrames[Type] = 3; // Set to 3 frames
    }

    public override void AI()
    {
        // Handle animation
        frameCounter++;
        if (frameCounter >= 5) // Change frame every 5 ticks
        {
            frameCounter = 0;
            frame++;
            if (frame >= 3) // Loop through 3 frames
                frame = 0;
                
            Projectile.frame = frame;
        }
        
        // Correct rotation for horizontal sprites with proper facing
        Projectile.rotation = Projectile.velocity.ToRotation();
        
        // Ensure proper facing direction
        if (Projectile.velocity.X < 0)
        {
            Projectile.rotation += MathHelper.Pi; // Flip 180 degrees if moving left
            Projectile.spriteDirection = -1;
        }
        else
        {
            Projectile.spriteDirection = 1;
        }
        
        Lighting.AddLight(Projectile.Center, 0.9f, 0.8f, 0.1f);
        
        // Create topaz dust trail
        if (Main.rand.NextBool(2))
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemTopaz, 
                Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100, default, 1.2f);
            dust.noGravity = true;
            dust.velocity *= 0.3f;
        }
        
        // Fade out near the end of its lifetime
        if (Projectile.timeLeft < 30)
        {
            Projectile.alpha += 8;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        // Play slime hit sound on impact
        SoundEngine.PlaySound(SoundID.NPCHit1, Projectile.position);
        
        // Create impact dust
        for (int i = 0; i < 10; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemTopaz, 
                0f, 0f, 100, default, 1.5f);
            dust.velocity *= 1.4f;
        }
    }

    public override void OnKill(int timeLeft)
    {
        // Create death effect
        for (int i = 0; i < 15; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemTopaz, 
                Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 100, default, 1.2f);
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        // This will automatically handle the animation frames
        return true;
    }
}