using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ultraconyx.Content.Projectiles;

public class BigScytheProj : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.aiStyle = 0;
        
        // Enable afterimage effect
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI()
    {
        Projectile.rotation += 0.4f * Projectile.direction;
        
        // Manage afterimage positions - only track 3 positions
        if (Projectile.velocity.Length() > 2f)
        {
            // Update the trail positions
            Projectile.oldPos[2] = Projectile.oldPos[1];
            Projectile.oldPos[1] = Projectile.oldPos[0];
            Projectile.oldPos[0] = Projectile.position;
        }
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

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2 speed = new Vector2(0, -4).RotatedByRandom(MathHelper.ToRadians(360));

            Terraria.Projectile.NewProjectile(
                Projectile.GetSource_Death(),
                Projectile.Center,
                speed,
                ModContent.ProjectileType<SmallHomingScythe>(),
                Projectile.damage / 2,
                2f,
                Projectile.owner
            );
        }
    }
}