using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.VanillaChanges;

// Applies mirror behavior changes
public class MirrorRevamp : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return entity.type == ItemID.MagicMirror || entity.type == ItemID.IceMirror;
    }

    public override Nullable<bool> UseItem(Item item, Player player)/* tModPorter Suggestion: Return null instead of false */
    {
        MirrorRevampPlayer modPlayer = player.GetModPlayer<MirrorRevampPlayer>();

        if (modPlayer.IsUsingMirror)
            return false;

        // Cancel vanilla use animation
        player.itemAnimation = 0;
        player.itemTime = 0;

        modPlayer.StartMirror(item.type);
        return true;
    }
}

// Handles mirror timing, dust, teleport
public class MirrorRevampPlayer : ModPlayer
{
    public bool IsUsingMirror;
    private int timer;
    private int mirrorType;

    public void StartMirror(int type)
    {
        IsUsingMirror = true;
        timer = 0;
        mirrorType = type;

        if (Player.whoAmI == Main.myPlayer)
        {
            Projectile.NewProjectile(
                Player.GetSource_Misc("MirrorRevamp"),
                Player.Center,
                Vector2.Zero,
                ModContent.ProjectileType<FloatingMirrorProjectile>(),
                0,
                0f,
                Player.whoAmI,
                type
            );
        }
    }

    public override void PostUpdate()
    {
        if (!IsUsingMirror)
            return;

        timer++;

        // Large dust circle
        int dustType = mirrorType == ItemID.IceMirror ? 76 : DustID.Cloud;
        int dustCount = 28;
        float radius = 80f;

        for (int i = 0; i < dustCount; i++)
        {
            float angle = MathHelper.TwoPi / dustCount * i + timer * 0.05f;
            Vector2 offset = angle.ToRotationVector2() * radius;

            Dust dust = Dust.NewDustPerfect(
                Player.Center + offset,
                dustType,
                Vector2.Zero,
                150,
                default,
                1.3f
            );
            dust.noGravity = true;
        }

        // Teleport after 1 second
        if (timer >= 60)
        {
            Player.Spawn(PlayerSpawnContext.RecallFromItem);
            IsUsingMirror = false;
        }
    }
}

// Floating mirror visual + glow
public class FloatingMirrorProjectile : ModProjectile
{
    // Dummy texture to avoid missing resource error
    public override string Texture => "Terraria/Images/Item_0";

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.timeLeft = 60;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.friendly = false;
    }

    public override void AI()
    {
        Player player = Main.player[Projectile.owner];

        if (!player.active || !player.GetModPlayer<MirrorRevampPlayer>().IsUsingMirror)
        {
            Projectile.Kill();
            return;
        }

        // Progress: 0 → 1 over 1 second
        float progress = 1f - (Projectile.timeLeft / 60f);

        // Smooth upward hover
        float height = MathHelper.Lerp(0f, -48f, progress);
        Projectile.Center = player.Center + new Vector2(0f, height);

        // Glow light ramps up with progress
        float lightStrength = MathHelper.SmoothStep(0.1f, 0.45f, progress);
        Lighting.AddLight(
            Projectile.Center,
            lightStrength * 0.6f,
            lightStrength * 0.75f,
            lightStrength
        );

        Projectile.rotation = 0f;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        int itemType = (int)Projectile.ai[0];
        Texture2D texture = Terraria.GameContent.TextureAssets.Item[itemType].Value;

        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Vector2 origin = texture.Size() / 2f;

        // Progress-based glow ramp (PvZ-style)
        float progress = 1f - (Projectile.timeLeft / 60f);
        float glowStrength = MathHelper.SmoothStep(0f, 1f, progress);

        Color glowColor = itemType == ItemID.IceMirror
            ? new Color(200, 240, 255, 100)
            : new Color(180, 220, 255, 100);

        glowColor *= glowStrength;

        float glowScale = MathHelper.Lerp(1f, 1.15f, glowStrength);

        // Glow aura layers
        for (int i = 0; i < 6; i++)
        {
            float angle = MathHelper.TwoPi / 6f * i;
            Vector2 offset = angle.ToRotationVector2() * (2f + glowStrength * 3f);

            Main.EntitySpriteDraw(
                texture,
                drawPos + offset,
                null,
                glowColor,
                0f,
                origin,
                glowScale,
                SpriteEffects.None,
                0
            );
        }

        // Main mirror
        Main.EntitySpriteDraw(
            texture,
            drawPos,
            null,
            Color.White,
            0f,
            origin,
            1f,
            SpriteEffects.None,
            0
        );

        return false;
    }
}
