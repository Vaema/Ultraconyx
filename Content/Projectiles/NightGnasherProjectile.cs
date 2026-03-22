using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Projectiles;

public class NightGnasherProjectile : ModProjectile
{
    private float baseRotation = 0f;
    private bool wasThrownLeft = false;
    
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2; // For rotating trails
    }

    public override void SetDefaults()
    {
        // Use vanilla boomerang AI style (style 3)
        Projectile.CloneDefaults(ProjectileID.EnchantedBoomerang);
        
        // Override specific properties
        Projectile.width = 42;
        Projectile.height = 42;
        Projectile.penetrate = -1; // Infinite pierce while returning
        Projectile.timeLeft = 600; // Longer time for 12 tiles
        Projectile.light = 0.5f;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    // Override the AI to customize it
    public override void AI()
    {
        // Keep most of the vanilla boomerang AI but adjust range
        Player player = Main.player[Projectile.owner];
        
        // Store initial throw direction on first frame
        if (Projectile.ai[1] == 0f)
        {
            wasThrownLeft = Projectile.velocity.X < 0;
            Projectile.ai[1] = 1f; // Mark as initialized
        }
        
        // Adjust the AI to have 12 tile range instead of default
        float maxDistance = 12 * 16f; // 12 tiles
        
        // Check if we need to return based on distance
        if (Projectile.ai[0] == 0f) // Going out
        {
            float distanceFromPlayer = Vector2.Distance(Projectile.Center, player.Center);
            
            // Return after 12 tiles
            if (distanceFromPlayer > maxDistance)
            {
                Projectile.ai[0] = 1f; // Start returning
                Projectile.netUpdate = true;
            }
        }
        
        // Call base AI (vanilla boomerang behavior)
        base.AI();
        
        // Always rotate at consistent fast speed
        float rotationSpeed = 0.3f;
        baseRotation += rotationSpeed;
        
        // Apply rotation based on ORIGINAL throw direction, not current velocity
        if (wasThrownLeft)
        {
            // Was thrown left - maintain consistent rotation
            Projectile.rotation = -baseRotation;
        }
        else
        {
            // Was thrown right - maintain consistent rotation
            Projectile.rotation = baseRotation;
        }
        
        // Add shadowflame dust
        if (Main.rand.NextBool(3))
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                DustID.Shadowflame, 0f, 0f, 100, default, 1f);
            dust.noGravity = true;
            dust.velocity *= 0.5f;
        }
    }
    
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        // Make it return immediately when hitting an enemy
        if (Projectile.ai[0] == 0f) // Only if still going out
        {
            Projectile.ai[0] = 1f; // Start returning
            Projectile.netUpdate = true;
        }
        
        SoundEngine.PlaySound(SoundID.NPCHit4 with { Pitch = 0.5f }, Projectile.Center);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.instance.LoadProjectile(Projectile.type);
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

        // Draw trails with rotation
        for (int k = 0; k < Projectile.oldPos.Length; k++)
        {
            Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + Projectile.Size / 2f;
            Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length * 0.5f);
            
            // Use the rotation from that frame
            Main.EntitySpriteDraw(texture, drawPos, null, color, 
                Projectile.oldRot[k], texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
        }

        // Draw main projectile
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, 
            Projectile.GetAlpha(lightColor), Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
        
        return false; // We already drew it
    }

    public override void OnKill(int timeLeft)
    {
        // Dust explosion on catch
        for (int i = 0; i < 10; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                DustID.Shadowflame, 0f, 0f, 100, default, 1.5f);
            dust.noGravity = true;
            dust.velocity *= 2f;
        }
        
        SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
    }
}