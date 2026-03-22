using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using System.Collections.Generic;
using Terraria.Graphics;

namespace Ultraconyx.Content.Items.Weapons.Melee;

public class SpookaySword : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 241;
        Item.DamageType = DamageClass.Melee;
        Item.width = 60;
        Item.height = 60;
        Item.useTime = 18;
        Item.useAnimation = 18;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 6.5f;
        Item.value = Item.sellPrice(gold: 15);
        Item.rare = ItemRarityID.Red;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;

        Item.shoot = ModContent.ProjectileType<Projectiles.SpookayProjectile>();
        Item.shootSpeed = 12f;

        Item.noMelee = false;
        Item.scale = 1.2f;
    }

    public override bool Shoot(
        Player player,
        EntitySource_ItemUse_WithAmmo source,
        Vector2 position,
        Vector2 velocity,
        int type,
        int damage,
        float knockback)
    {
        for (int i = 0; i < 2; i++)
        {
            Vector2 newVelocity = velocity
                .RotatedByRandom(MathHelper.ToRadians(5f))
                * (1f - Main.rand.NextFloat(0.2f));

            Projectile.NewProjectile(
                source,
                position,
                newVelocity,
                type,
                damage,
                knockback,
                player.whoAmI
            );
        }

        return false;
    }
}

// ================= VERTEX STRIP TRAIL =================

public class SpookaySwordPlayer : ModPlayer
{
    private VertexStrip _trailStrip = new VertexStrip();
    private List<Vector2> _trailPositions = new List<Vector2>();
    private List<float> _trailRotations = new List<float>();
    
    private const int MAX_TRAIL_POINTS = 30;

    public override void PostUpdate()
    {
        Item item = Player.HeldItem;

        if (item.type == ModContent.ItemType<SpookaySword>() && Player.itemAnimation > 0)
        {
            // Get the position of the sword tip
            Vector2 swordPosition = Player.MountedCenter + Player.itemLocation;
            
            // Calculate rotation based on player direction and item rotation
            float rotation = Player.itemRotation;
            if (Player.direction == -1)
                rotation += MathHelper.Pi;
            
            // Add new position to trail
            _trailPositions.Add(swordPosition);
            _trailRotations.Add(rotation);
            
            // Keep trail at maximum length
            while (_trailPositions.Count > MAX_TRAIL_POINTS)
            {
                _trailPositions.RemoveAt(0);
                _trailRotations.RemoveAt(0);
            }
        }
        else
        {
            // Clear trail when not swinging
            _trailPositions.Clear();
            _trailRotations.Clear();
        }
    }

    public void DrawTrail()
    {
        if (_trailPositions.Count < 2)
            return;

        // Prepare the trail strip
        _trailStrip.PrepareStrip(
            _trailPositions.ToArray(),
            _trailRotations.ToArray(),
            StripColorFunction,
            StripWidthFunction,
            Vector2.Zero,
            _trailPositions.Count
        );

        // Get the texture for the trail (using the item's texture)
        Texture2D texture = TextureAssets.Item[
            ModContent.ItemType<SpookaySword>()
        ].Value;

        // Set up blend state for additive or normal blending
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        // Draw the trail
        _trailStrip.DrawTrail();

        // Restore the sprite batch
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }

    private Color StripColorFunction(float progressOnStrip)
    {
        // Progress goes from 0 to 1, where 0 is the newest point and 1 is the oldest
        float alpha = 1f - progressOnStrip; // Fade out as it gets older
        
        // Base color based on time of day
        Color baseColor = Main.dayTime 
            ? Color.Lerp(Color.Orange, Color.Yellow, progressOnStrip)
            : Color.Lerp(Color.Purple, Color.Blue, progressOnStrip);
        
        // Add some pulse effect
        float pulse = (float)Main.timeForVisualEffects * 0.1f;
        baseColor = Color.Lerp(baseColor, Color.White, MathHelper.Clamp((float)System.Math.Sin(progressOnStrip * 10f - pulse) * 0.5f + 0.5f, 0f, 0.3f));
        
        return baseColor * alpha * 0.8f;
    }

    private float StripWidthFunction(float progressOnStrip)
    {
        // Trail width - starts wide and narrows
        float baseWidth = 40f * (1f - progressOnStrip * 0.7f);
        
        // Add some variation
        float variation = (float)System.Math.Sin(progressOnStrip * 20f + Main.timeForVisualEffects * 0.2f) * 5f;
        
        return baseWidth + variation;
    }
}

// ================= DRAW LAYER =================

public class SpookaySwordDrawLayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition()
        => new AfterParent(PlayerDrawLayers.HeldItem);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return drawInfo.drawPlayer.HeldItem.type == ModContent.ItemType<SpookaySword>()
               && drawInfo.drawPlayer.itemAnimation > 0;
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        // Draw the trail behind the sword
        drawInfo.drawPlayer
            .GetModPlayer<SpookaySwordPlayer>()
            .DrawTrail();
        
        // The sword itself will be drawn by the HeldItem layer
    }
}