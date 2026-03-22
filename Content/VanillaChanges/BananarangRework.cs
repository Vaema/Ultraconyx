using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.VanillaChanges;

public class BananarangRework : GlobalProjectile
{
    private const float MaxDistance = 30f * 16f;

    public override bool AppliesToEntity(Projectile projectile, bool lateInstantiation) =>
        projectile.type == ProjectileID.Bananarang;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[ProjectileID.Bananarang] = 8;
        ProjectileID.Sets.TrailingMode[ProjectileID.Bananarang] = 2;
    }

    public override void AI(Projectile projectile)
    {
        Player owner = Main.player[projectile.owner];

        float distanceFromPlayer = Vector2.Distance(projectile.Center, owner.Center);
        if (distanceFromPlayer >= MaxDistance)
            projectile.ai[0] = 1f;
    }

    public override bool PreDraw(Projectile projectile, ref Color lightColor)
    {
        Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[projectile.type].Value;
        Vector2 origin = texture.Size() / 2f;

        for (int i = 0; i < projectile.oldPos.Length; i++)
        {
            Vector2 drawPos = projectile.oldPos[i] + projectile.Size / 2f - Main.screenPosition;
            float alpha = (projectile.oldPos.Length - i) / (float)projectile.oldPos.Length;

            Color color = lightColor * alpha * 0.6f;
            Main.spriteBatch.Draw(
                texture,
                drawPos,
                null,
                color,
                projectile.rotation,
                origin,
                projectile.scale,
                projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0f
            );
        }

        return true;
    }
}
