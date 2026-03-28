using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Projectiles.Melee;

public class NebulaSpearBolt : ModProjectile
{
    private const int AfterimageCount = 6;
    private Vector2[] afterimagePositions = new Vector2[AfterimageCount];
    private float[] afterimageRotations = new float[AfterimageCount];
    private int afterimageIndex;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = AfterimageCount;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }

    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 10;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 60;
        Projectile.extraUpdates = 1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = true;
        Projectile.scale = 1.2f;
    }

    public override void AI()
    {
        afterimagePositions[afterimageIndex] = Projectile.Center;
        afterimageRotations[afterimageIndex] = Projectile.rotation;
        afterimageIndex = (afterimageIndex + 1) % afterimagePositions.Length;

        if (Projectile.velocity != Vector2.Zero)
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);

        Projectile.velocity *= 0.98f;

        if (Main.rand.NextBool(1))
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch,
                0f, 0f, 100, default, 1.3f);
            dust.noGravity = true;
            dust.velocity = Projectile.velocity * 0.3f;
            dust.scale = 1.4f;

            if (Main.rand.NextBool(2))
            {
                Dust trailDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch,
                    Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.5f, 0.5f), 100, default, 0.9f);
                trailDust.noGravity = true;
                trailDust.velocity = Projectile.velocity * 0.2f;
            }
            if (Main.rand.NextBool(3))
            {
                Dust pinkDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PinkTorch,
                    0f, 0f, 100, default, 1f);
                pinkDust.noGravity = true;
                pinkDust.velocity = Projectile.velocity * 0.25f;
            }
        }

        if (Projectile.timeLeft < 30)
            Projectile.alpha = (int)(255 * (1f - (float)Projectile.timeLeft / 30f));

        Lighting.AddLight(Projectile.Center, new Vector3(0.8f, 0.2f, 0.8f) * 0.6f);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        DrawAfterimages();
        DrawMainProjectile(lightColor);

        return false;
    }

    private void DrawMainProjectile(Color lightColor)
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Rectangle frame = texture.Frame();
        Vector2 origin = new(texture.Width * 0.5f, texture.Height);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation,
            origin, Projectile.scale, SpriteEffects.None);

        Color glowColor = Color.Lerp(Color.Purple, Color.White, 0.2f) * 0.4f;
        glowColor.A = 0;

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, glowColor, Projectile.rotation,
            origin, Projectile.scale * 1.15f, SpriteEffects.None);
    }

    private void DrawAfterimages()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Rectangle frame = texture.Frame();
        Vector2 origin = new(texture.Width * 0.5f, texture.Height);

        for (int i = 0; i < AfterimageCount; i++)
        {
            int drawIndex = (afterimageIndex + i) % AfterimageCount;
            if (afterimagePositions[drawIndex] == Vector2.Zero)
                continue;
            if (afterimagePositions[drawIndex] == Projectile.Center)
                continue;

            float fade = 1f - (float)i / AfterimageCount;
            float opacity = 0.1f + fade * 0.5f;
            float scale = Projectile.scale * (0.8f + fade * 0.2f);

            Color afterimageColor = Color.Lerp(Color.Purple, Color.Magenta, fade * 0.3f) * opacity;
            afterimageColor.A = (byte)(200 * fade);

            Main.EntitySpriteDraw(texture, afterimagePositions[drawIndex] - Main.screenPosition, frame, afterimageColor,
                afterimageRotations[drawIndex], origin, scale, SpriteEffects.None);
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        for (int i = 0; i < 10; i++)
        {
            Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.PurpleTorch,
                Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 100, default, 1.6f);
            dust.noGravity = true;
            dust.velocity *= 1.5f;
        }

        Projectile.damage = (int)(Projectile.damage * 0.7f);
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 15; i++)
        {
            Dust purple = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch,
                Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1.3f);
            purple.noGravity = true;
            purple.velocity *= 1.2f;

            if (Main.rand.NextBool(2))
            {
                Dust pink = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PinkTorch,
                    Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 0.9f);
                pink.noGravity = true;
            }
        }
    }
}
