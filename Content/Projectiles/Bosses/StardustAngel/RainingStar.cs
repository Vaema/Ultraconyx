using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ultraconyx.Content.Dusts;

namespace Ultraconyx.Content.Projectiles.Bosses.StardustAngel;

public class RainingStar : ModProjectile
{
    private Vector2[] trailPositions = new Vector2[3];
    private int trailIndex = 0;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 1;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 300;
        Projectile.alpha = 0;
        Projectile.penetrate = -1;
        Projectile.light = 0.5f;
        
        // No immunity frames so it can hit multiple times
        Projectile.usesLocalNPCImmunity = false;
        Projectile.usesIDStaticNPCImmunity = false;
    }

    public override void AI()
    {
        // Store position for afterimage
        trailPositions[trailIndex] = Projectile.Center;
        trailIndex = (trailIndex + 1) % trailPositions.Length;

        // Spawn stardust trail
        if (Main.rand.NextBool(4))
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Stardust>(), 
                Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100, default, 1f);
            dust.noGravity = true;
        }
        
        // Add some light
        Lighting.AddLight(Projectile.Center, 0.3f, 0.5f, 1f);
        
        // Slight rotation
        Projectile.rotation += 0.1f;
        
        // Add a little gravity/acceleration
        Projectile.velocity.Y += 0.1f;
        
        // Slight horizontal drift
        if (Projectile.velocity.X > 0)
            Projectile.velocity.X -= 0.02f;
        else if (Projectile.velocity.X < 0)
            Projectile.velocity.X += 0.02f;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
        
        // Draw afterimage trail
        for (int i = 0; i < trailPositions.Length; i++)
        {
            if (trailPositions[i] != Vector2.Zero)
            {
                float alpha = 0.25f * (1f - (i / (float)trailPositions.Length));
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

    public override bool CanHitPlayer(Player target)
    {
        return true;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
    {
        // Small knockback effect
        target.velocity.Y -= 2f;
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