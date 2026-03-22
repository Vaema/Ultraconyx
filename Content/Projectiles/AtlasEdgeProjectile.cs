using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Projectiles;

public class AtlasEdgeProjectile : ModProjectile
{
    public override void SetStaticDefaults()
    {
        // ✅ Simple built-in afterimage
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
    }

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;

        Projectile.friendly = true;
        Projectile.hostile = false;

        Projectile.DamageType = DamageClass.Melee;

        Projectile.penetrate = 1;
        Projectile.timeLeft = 300;

        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        
        // ✅ Enable drawing from the trail cache
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2; // Use mode 2 for better control
    }

    public override void AI()
    {
        // ✅ FACE TOWARD MOUSE (NORMAL PROJECTILE BEHAVIOR)
        if (Projectile.velocity.Length() > 0.1f)
            Projectile.rotation = Projectile.velocity.ToRotation();

        // ✅ Light motion dust for flair (optional but nice)
        if (Main.rand.NextBool(4))
        {
            Dust d = Dust.NewDustPerfect(
                Projectile.Center,
                DustID.BlueTorch,
                Projectile.velocity * 0.1f,
                150,
                default,
                1.05f
            );
            d.noGravity = true;
        }
        
        // ✅ Update trail positions manually for better control
        UpdateTrailPositions();
    }
    
    private void UpdateTrailPositions()
    {
        // Shift old positions down the array
        for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
        {
            Projectile.oldPos[i] = Projectile.oldPos[i - 1];
            Projectile.oldRot[i] = Projectile.oldRot[i - 1];
        }
        
        // Store current position and rotation
        Projectile.oldPos[0] = Projectile.position;
        Projectile.oldRot[0] = Projectile.rotation;
    }
    
    // ✅ Custom afterimage drawing
    public override bool PreDraw(ref Color lightColor)
    {
        // Draw afterimages
        DrawAfterimages();
        
        // Draw main projectile
        DrawMainProjectile(lightColor);
        
        return false; // Skip default drawing since we're doing custom drawing
    }
    
    private void DrawAfterimages()
    {
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        Rectangle frame = texture.Frame();
        
        // ✅ CRITICAL FIX: Use the correct origin (center of texture, not center of hitbox)
        // The texture might be larger than the hitbox, so we need to account for that
        Vector2 textureCenter = new(texture.Width / 2f, texture.Height / 2f);
        
        // ✅ Calculate the offset between the texture center and the projectile's draw position
        // This is what was causing the afterimages to be on the wrong side
        Vector2 drawOffset = Projectile.Center - Projectile.position;
        
        // Draw afterimages (trails)
        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            // Skip if no position stored
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;
                
            float progress = 1f - (float)i / Projectile.oldPos.Length;
            
            // Fade out older afterimages
            Color color = Color.Lerp(Color.Blue, Color.Cyan, progress) * (progress * 0.5f);
            
            // ✅ FIXED: Apply the same offset to afterimages as the main projectile
            Vector2 afterimagePos = Projectile.oldPos[i] + drawOffset;
            Vector2 drawPos = afterimagePos - Main.screenPosition;
            
            Main.EntitySpriteDraw(
                texture,
                drawPos,
                frame,
                color,
                Projectile.oldRot[i],
                textureCenter,
                Projectile.scale * (0.8f + progress * 0.2f), // Slight size variation
                SpriteEffects.None,
                0
            );
        }
    }
    
    private void DrawMainProjectile(Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        Rectangle frame = texture.Frame();
        Vector2 textureCenter = new(texture.Width / 2f, texture.Height / 2f);
        
        // Main projectile with lighting
        Color drawColor = lightColor;
        
        // Add a blue tint to the main projectile
        drawColor = Color.Lerp(lightColor, Color.Blue, 0.3f);
        
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        
        Main.EntitySpriteDraw(
            texture,
            drawPos,
            frame,
            drawColor,
            Projectile.rotation,
            textureCenter,
            Projectile.scale,
            SpriteEffects.None,
            0
        );
        
        // Optional: Add a glow effect to the main projectile
        if (Main.rand.NextBool(5))
        {
            Main.EntitySpriteDraw(
                texture,
                drawPos,
                frame,
                Color.Cyan * 0.3f,
                Projectile.rotation,
                textureCenter,
                Projectile.scale * 1.1f,
                SpriteEffects.None,
                0
            );
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Projectile.owner != Main.myPlayer)
            return;

        Vector2 above = target.Center + new Vector2(0, -180f);
        Vector2 below = target.Center + new Vector2(0, 180f);

        Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            above,
            Vector2.Zero,
            ModContent.ProjectileType<AtlasEdgeClone>(),
            Projectile.damage,
            Projectile.knockBack,
            Projectile.owner,
            target.whoAmI
        );

        Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            below,
            Vector2.Zero,
            ModContent.ProjectileType<AtlasEdgeClone>(),
            Projectile.damage,
            Projectile.knockBack,
            Projectile.owner,
            target.whoAmI
        );
    }
}