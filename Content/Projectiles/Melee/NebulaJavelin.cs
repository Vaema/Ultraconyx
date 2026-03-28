using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Projectiles.Melee;

public class NebulaJavelin : ModProjectile
{
    public override string Texture => "Ultracronyx/Content/Projectiles/Melee/NebulaSpearProjectile";

    private const float MaxDistance = 600f;
    private const float ReturnSpeed = 18f;

    private bool isReturning;
    private bool hasHitSomething;
    private Vector2 throwDirection;
    private float rotationSpeed = 0.2f;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.penetrate = 3;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.scale = 1.2f;
        Projectile.tileCollide = true;
        Projectile.timeLeft = 300;
        Projectile.extraUpdates = 1;
    }

    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        if (player.dead || !player.active)
        {
            Projectile.Kill();
            return;
        }

        if (Projectile.ai[0] == 0f)
        {
            throwDirection = Projectile.velocity;
            throwDirection.Normalize();
            Projectile.ai[0] = 1f;
            Projectile.rotation = throwDirection.ToRotation() + MathHelper.ToRadians(135f);
        }

        float distanceToPlayer = Vector2.Distance(player.Center, Projectile.Center);
        if (!isReturning)
        {
            if (distanceToPlayer >= MaxDistance || Projectile.timeLeft < 150 || hasHitSomething)
                isReturning = true;

            Projectile.velocity *= 0.99f;
            Projectile.velocity.Y += 0.1f;

            Projectile.rotation += rotationSpeed;
        }
        else
        {
            Vector2 directionToPlayer = player.Center - Projectile.Center;
            float speed = ReturnSpeed;

            if (distanceToPlayer > 200f)
                speed = ReturnSpeed * 1.5f;
            if (distanceToPlayer < 100f)
                speed = MathHelper.Lerp(5f, ReturnSpeed, distanceToPlayer / 100f);

            Projectile.velocity = Vector2.Normalize(directionToPlayer) * speed;

            float rotation = directionToPlayer.ToRotation() + MathHelper.ToRadians(135f);
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, rotation, 0.1f);

            if (distanceToPlayer < 30f)
            {
                Projectile.Kill();
                return;
            }

            Projectile.tileCollide = false;
        }

        // Emit a dust trail.
        if (Main.rand.NextBool(2))
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch,
                0f, 0f, 100, default, 1.3f);
            dust.noGravity = true;
            dust.velocity = Projectile.velocity * -0.3f;

            if (Main.rand.NextBool(3))
            {
                Dust pinkDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PinkTorch,
                    0f, 0f, 100, default, 1f);
                pinkDust.noGravity = true;
                pinkDust.velocity = Projectile.velocity * -0.2f;
            }
        }

        // Rotate the player's arm.
        if (isReturning)
        {
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
        }

        Lighting.AddLight(Projectile.Center, new Vector3(0.8f, 0.2f, 0.8f) * 0.6f);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

        // Draw the trail.
        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;

            float fade = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
            Color trailColor = Color.Purple * fade * 0.5f;
            trailColor.A = 0;
            Vector2 position = Projectile.oldPos[i] - Main.screenPosition + Projectile.Size / 2f;

            Main.EntitySpriteDraw(texture, position, null, trailColor, Projectile.oldRot[i],
                new Vector2(texture.Width * 0.5f, texture.Height * 0.5f), Projectile.scale * (0.8f + fade * 0.2f), SpriteEffects.None);
        }

        // Draw the main projectile.
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation,
            new Vector2(texture.Width * 0.5f, texture.Height * 0.5f), Projectile.scale, SpriteEffects.None);

        return false;
    }

    public override void PostDraw(Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Color glowColor = Color.Purple * 0.4f;
        glowColor.A = 0;

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation,
            new Vector2(texture.Width * 0.5f, texture.Height * 0.5f), Projectile.scale * 1.1f, SpriteEffects.None, 0);
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        // Stick onto tiles briefly, then return.
        if (!isReturning)
        {
            isReturning = true;

            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;

            // Emit impact dust.
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.PurpleTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f),
                    100, default, 1.5f);
                dust.noGravity = true;
            }

            // Wait a moment before returning.
            Projectile.timeLeft = 60;
        }

        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        // Start returning after hitting.
        hasHitSomething = true;
        if (!isReturning)
        {
            isReturning = true;
            Projectile.timeLeft = 30;
        }

        // Emit impact dust.
        for (int i = 0; i < 10; i++)
        {
            Dust dust = Dust.NewDustDirect(target.position, target.width, target.height,
                DustID.PurpleTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f),
                100, default, 1.6f);
            dust.noGravity = true;
        }

        // Reduce damage on subsequent hits.
        Projectile.damage = (int)(Projectile.damage * 0.8f);
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 20; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch,
                Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 100, default, 1.2f);
            dust.noGravity = true;
        }
    }
}
