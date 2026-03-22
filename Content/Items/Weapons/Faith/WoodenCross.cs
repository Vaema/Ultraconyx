using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;

namespace Ultraconyx.Content.Items.Weapons.Faith;

public class WoodenCross : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 28;
        Item.value = Item.sellPrice(copper: 50);
        Item.rare = ItemRarityID.White;
        
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 25;
        Item.useAnimation = 25;
        Item.autoReuse = true;
        
        Item.DamageType = DamageClass.Magic;
        Item.damage = 15;
        Item.knockBack = 1f;
        Item.mana = 2;
        Item.crit = 10;
        
        Item.noMelee = true;
        Item.noUseGraphic = false;
    }

    public override bool CanUseItem(Player player)
    {
        // Get mouse position in world coordinates
        Vector2 mouseWorld = Main.MouseWorld;
        
        // Find the nearest enemy within 20 blocks AND within a 25 degree aiming cone
        NPC target = FindEnemyInAimingCone(player.Center, mouseWorld, 320f, 25f);
        return target != null;
    }

    public override Nullable<bool> UseItem(Player player)/* tModPorter Suggestion: Return null instead of false */
    {
        if (player.whoAmI == Main.myPlayer)
        {
            // Get mouse position in world coordinates
            Vector2 mouseWorld = Main.MouseWorld;
            
            // Find nearest enemy within aiming cone
            NPC target = FindEnemyInAimingCone(player.Center, mouseWorld, 320f, 25f);
            
            if (target != null && target.active && !target.friendly)
            {
                // Calculate beam start position: above player's center
                Vector2 beamStart = player.Center + new Vector2(0, -20);
                
                // Create a straight line of dust from above player to target
                CreateBeamDust(beamStart, target.Center);
                
                // Deal damage to the target
                int damage = player.GetWeaponDamage(Item);
                float knockback = Item.knockBack;
                bool crit = Main.rand.Next(100) < Item.crit;
                
                player.ApplyDamageToNPC(target, damage, knockback, player.direction, crit);
                
                // Visual effect at target position
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDust(target.position, target.width, target.height, DustID.Torch, 0f, 0f, 100, default(Color), 1.5f);
                }
                
                // Visual effect at beam start position (above player)
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDust(beamStart, 0, 0, DustID.Torch, 0f, 0f, 100, default(Color), 1.5f);
                }
            }
            else
            {
                // Player aimed at empty space or no enemy in cone
                // Still consume mana but no effect
                for (int i = 0; i < 3; i++)
                {
                    Dust.NewDust(player.Center, 0, 0, DustID.Torch, 0f, 0f, 100, default(Color), 0.8f);
                }
            }
        }
        
        return true;
    }

    private NPC FindEnemyInAimingCone(Vector2 playerPosition, Vector2 aimDirection, float maxDistance, float coneAngleDegrees)
    {
        NPC bestTarget = null;
        float bestScore = 0f;
        
        // Convert cone angle to radians for calculations
        float coneAngleRadians = MathHelper.ToRadians(coneAngleDegrees);
        
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            
            if (npc.active && !npc.friendly && npc.life > 0 && !npc.dontTakeDamage)
            {
                Vector2 toNPC = npc.Center - playerPosition;
                float distance = toNPC.Length();
                
                // Check if within max distance
                if (distance <= maxDistance)
                {
                    // Calculate angle between aim direction and enemy direction
                    Vector2 aimVector = aimDirection - playerPosition;
                    aimVector.Normalize();
                    toNPC.Normalize();
                    
                    float dot = Vector2.Dot(aimVector, toNPC);
                    float angle = (float)Math.Acos(dot);
                    
                    // Check if enemy is within aiming cone
                    if (angle <= coneAngleRadians)
                    {
                        // Calculate a score based on angle and distance
                        // Closer enemies and enemies more directly in aim get higher scores
                        float angleScore = 1f - (angle / coneAngleRadians); // 1 when directly aimed, 0 at edge
                        float distanceScore = 1f - (distance / maxDistance); // 1 when close, 0 at max distance
                        
                        // Combined score (weighted more toward angle accuracy)
                        float totalScore = (angleScore * 0.7f) + (distanceScore * 0.3f);
                        
                        if (totalScore > bestScore)
                        {
                            bestScore = totalScore;
                            bestTarget = npc;
                        }
                    }
                }
            }
        }
        
        return bestTarget;
    }

    private void CreateBeamDust(Vector2 start, Vector2 end)
    {
        Vector2 direction = end - start;
        float distance = direction.Length();
        direction.Normalize();
        
        // Number of dust particles based on distance (1 per 8 pixels)
        int dustCount = (int)(distance / 8f);
        
        for (int i = 0; i < dustCount; i++)
        {
            // Position along the line
            Vector2 dustPosition = start + direction * (i * 8f);
            
            // Create torch dust with some variation
            Dust dust = Dust.NewDustDirect(dustPosition, 0, 0, DustID.Torch, 0f, 0f, 100, default(Color), 1.2f);
            dust.noGravity = true;
            dust.velocity = Vector2.Zero;
            
            // Occasionally create larger dust particles
            if (i % 3 == 0)
            {
                dust = Dust.NewDustDirect(dustPosition, 0, 0, DustID.Torch, 0f, 0f, 100, default(Color), 2f);
                dust.noGravity = true;
                dust.velocity = Vector2.Zero;
            }
        }
        
        // Additional effect at start and end points
        for (int i = 0; i < 8; i++)
        {
            Dust.NewDust(start, 0, 0, DustID.Torch, 0f, 0f, 100, default(Color), 1.5f);
            Dust.NewDust(end, 0, 0, DustID.Torch, 0f, 0f, 100, default(Color), 1.5f);
        }
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Wood, 20)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}