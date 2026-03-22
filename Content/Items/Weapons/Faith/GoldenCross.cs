using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Ultraconyx.Content.Projectiles;

namespace Ultraconyx.Content.Items.Weapons.Faith;

public class GoldenCross : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 26;
        Item.height = 40;
        Item.value = Item.sellPrice(gold: 1);
        Item.rare = ItemRarityID.Green;
        
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.autoReuse = true;
        
        Item.DamageType = DamageClass.Magic;
        Item.damage = 35; // Weapon damage (beam will use this)
        Item.knockBack = 0f;
        Item.mana = 10;
        Item.crit = 5;
        
        Item.noMelee = true;
        Item.noUseGraphic = false;
    }

    public override bool AltFunctionUse(Player player)
    {
        return true; // Allow right click
    }

    public override bool CanUseItem(Player player)
    {
        if (player.altFunctionUse == 2) // Right click
        {
            Item.mana = 2;
            Item.useTime = 5;
            Item.useAnimation = 5;
            return true;
        }
        else // Left click
        {
            Item.mana = 10;
            Item.useTime = 20;
            Item.useAnimation = 20;
            
            // Check if player has enough mana
            if (player.statMana < Item.mana)
                return false;
            
            // Check if a projectile from this player already exists
            if (ProjectileExistsForPlayer(player))
            {
                // A projectile already exists, don't spawn another
                return false;
            }
            
            return true;
        }
    }

    private bool ProjectileExistsForPlayer(Player player)
    {
        // Check all projectiles to see if this player already has a GoldenCrossProjectile
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile proj = Main.projectile[i];
            
            if (proj.active && 
                proj.type == ModContent.ProjectileType<GoldenCrossProjectile>() && 
                proj.owner == player.whoAmI)
            {
                return true;
            }
        }
        
        return false;
    }

    public override Nullable<bool> UseItem(Player player)/* tModPorter Suggestion: Return null instead of false */
    {
        if (player.altFunctionUse == 2) // Right click - just hold
        {
            if (player.whoAmI == Main.myPlayer)
            {
                EmitLight(player);
            }
            return true;
        }
        else // Left click - spawn projectile
        {
            if (player.whoAmI == Main.myPlayer)
            {
                // Get mouse position
                Vector2 mouseWorld = Main.MouseWorld;
                
                // Spawn projectile 3 tiles above cursor
                // Note: We pass 0 for projectile damage since projectile itself doesn't deal damage
                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    mouseWorld - new Vector2(0, 48 * 3),
                    Vector2.Zero,
                    ModContent.ProjectileType<GoldenCrossProjectile>(),
                    0, // Projectile damage is 0 - beam will handle damage
                    Item.knockBack,
                    player.whoAmI
                );
            }
            return true;
        }
    }

    private void EmitLight(Player player)
    {
        float radius = 160f;
        
        Vector2 crossPos = player.Center;
        if (player.direction < 0)
        {
            crossPos.X -= 20f;
        }
        
        Lighting.AddLight(crossPos, 1f, 0.9f, 0.3f);
        
        for (int i = 0; i < 12; i++)
        {
            float angle = MathHelper.TwoPi * i / 12f;
            Vector2 lightPos = player.Center + new Vector2(radius, 0).RotatedBy(angle);
            Lighting.AddLight(lightPos, 0.4f, 0.35f, 0.1f);
        }
        
        if (Main.rand.NextBool(3))
        {
            Vector2 particlePos = crossPos + new Vector2(
                Main.rand.NextFloat(-10f, 10f),
                Main.rand.NextFloat(-10f, 10f)
            );
            Dust.NewDustPerfect(particlePos, DustID.GoldFlame, Vector2.Zero, 100, Color.Gold, 0.8f);
        }
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.GoldBar, 12)
            .AddIngredient(ItemID.FallenStar, 3)
            .AddTile(TileID.Anvils)
            .Register();
    }
}