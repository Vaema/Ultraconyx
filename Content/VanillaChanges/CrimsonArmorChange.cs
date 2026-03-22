using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Ultraconyx.Content.Buffs;

namespace Ultraconyx.Content.VanillaChanges;

public class CrimsonArmorPlayer : ModPlayer
{
    public int lifeRegenBuffTimer = 0;
    private uint lastHitFrame = 0;

    public override void PostUpdate()
    {
        // Handle life regen buff timer
        if (lifeRegenBuffTimer > 0)
        {
            lifeRegenBuffTimer--;
            
            // Apply life regen boost
            Player.lifeRegen += 10;
            
            // Add visual buff effect - use your custom CrimsonUndertakerBuff
            // Assuming the buff is in a separate file in the Buffs namespace
            Player.AddBuff(ModContent.BuffType<CrimsonUndertakerBuff>(), 2);
        }
    }

    public override void OnHitAnything(float x, float y, Entity victim)
    {
        // This is a simpler way to detect hits
        if (victim is NPC && Player.HeldItem.type == ItemID.TheUndertaker && IsWearingFullCrimsonArmor())
        {
            // Prevent multiple triggers in the same frame
            if (Main.GameUpdateCount != lastHitFrame)
            {
                lifeRegenBuffTimer = 30 * 60; // 30 seconds
                lastHitFrame = Main.GameUpdateCount;
            }
        }
    }
    
    // Helper method to check if player is wearing full crimson armor
    private bool IsWearingFullCrimsonArmor()
    {
        return Player.armor[0].type == ItemID.CrimsonHelmet &&
               Player.armor[1].type == ItemID.CrimsonScalemail &&
               Player.armor[2].type == ItemID.CrimsonGreaves;
    }

    // This method reduces Undertaker damage by 50% when wearing full crimson armor
    public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
    {
        if (item.type == ItemID.TheUndertaker && IsWearingFullCrimsonArmor())
        {
            damage *= 0.5f;
        }
    }
}

// Add a GlobalProjectile class to add blood trail to bullets
public class CrimsonUndertakerProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;
    
    public override void PostAI(Projectile projectile)
    {
        // Check if this is a bullet projectile
        if (projectile.type == ProjectileID.Bullet)
        {
            // Check if the owner player exists
            if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
            {
                Player player = Main.player[projectile.owner];
                
                // Check if player has CrimsonUndertakerBuff active and is holding Undertaker with crimson armor
                if (player.HasBuff(ModContent.BuffType<CrimsonUndertakerBuff>()) && 
                    player.HeldItem.type == ItemID.TheUndertaker &&
                    player.armor[0].type == ItemID.CrimsonHelmet &&
                    player.armor[1].type == ItemID.CrimsonScalemail &&
                    player.armor[2].type == ItemID.CrimsonGreaves)
                {
                    // Create blood trail dust (using dust ID 5 for Blood)
                    for (int i = 0; i < 2; i++) // Create 2 dust particles per frame
                    {
                        Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 
                            5, 0f, 0f, 100, default, 1.5f); // 5 = Blood dust
                        dust.noGravity = true;
                        dust.velocity *= 0.5f;
                        
                        // Make dust follow projectile direction
                        dust.velocity += projectile.velocity * 0.1f;
                        
                        // Randomize dust position slightly
                        dust.position += Main.rand.NextVector2Circular(4f, 4f);
                    }
                }
            }
        }
    }
}

public class CrimsonArmorSetBonus : GlobalItem
{
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (item.type == ItemID.CrimsonHelmet || item.type == ItemID.CrimsonScalemail || item.type == ItemID.CrimsonGreaves)
        {
            for (int i = 0; i < tooltips.Count; i++)
            {
                if (tooltips[i].Text.Contains("Increased life regeneration"))
                {
                    tooltips.Insert(i + 1, new TooltipLine(Mod, "CrimsonUndertakerBonus", 
                        "Reduces the Undertaker damage by 50% but gives buffs to it."));
                    break;
                }
            }
        }
    }
}

public class UndertakerTooltipChange : GlobalItem
{
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        // Check if local player has full crimson armor equipped
        Player player = Main.LocalPlayer;
        bool hasFullCrimsonArmor = player.armor[0].type == ItemID.CrimsonHelmet &&
                                   player.armor[1].type == ItemID.CrimsonScalemail &&
                                   player.armor[2].type == ItemID.CrimsonGreaves;
        
        if (item.type == ItemID.TheUndertaker && hasFullCrimsonArmor)
        {
            tooltips.Add(new TooltipLine(Mod, "CrimsonArmorEffect", 
                "[c/FF6B6B:Increases life regen on hit]"));
        }
    }
}