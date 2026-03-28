using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Projectiles.Melee;

public class NebulaSpearProjectile : ModProjectile
{
    public static float HoldoutRangeMin => 24f;
    public static float HoldoutRangeMax => 160f;

    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.Spear);
    }

    public override bool PreAI()
    {
        Player player = Main.player[Projectile.owner];
        player.heldProj = Projectile.whoAmI;

        int duration = player.itemAnimationMax;
        if (Projectile.timeLeft > duration)
            Projectile.timeLeft = duration;

        float halfDuration = duration * 0.5f;
        float progress;

        if (Projectile.timeLeft < halfDuration)
            progress = Projectile.timeLeft / halfDuration;
        else
            progress = (duration - Projectile.timeLeft) / halfDuration;

        Projectile.velocity = Vector2.Normalize(Projectile.velocity);
        Projectile.Center = player.MountedCenter + Vector2.SmoothStep(Projectile.velocity * HoldoutRangeMin,
            Projectile.velocity * HoldoutRangeMax, progress);

        if (Projectile.spriteDirection == -1)
            Projectile.rotation += MathHelper.ToRadians(45f);
        else
            Projectile.rotation += MathHelper.ToRadians(135f);

        if (!Main.dedServ)
        {
            if (Main.rand.NextBool(3))
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch,
                    Projectile.velocity.X * 2f, Projectile.velocity.Y * 2f, 128, default, 1.2f);
            }
        }

        Lighting.AddLight(Projectile.Center, new Vector3(0.8f, 0.2f, 0.8f) * 0.3f);

        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        // Heal the player.
        if (Main.rand.NextBool(5))
        {
            Player player = Main.player[Projectile.owner];
            int healAmount = 3;
            player.statLife += healAmount;
            if (player.statLife > player.statLifeMax2)
                player.statLife = player.statLifeMax2;
            player.HealEffect(healAmount, true);
        }

        // Emit impact dust.
        for (int i = 0; i < 8; i++)
        {
            Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.PurpleTorch,
                Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1.5f);
            dust.noGravity = true;
        }
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 12; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch,
                Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 100, default, 1.5f);
            dust.noGravity = true;

            if (Main.rand.NextBool(3))
            {
                Dust pinkDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PinkTorch,
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 100, default, 1f);
                pinkDust.noGravity = true;
            }
        }
    }
}
