using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.VanillaChanges;

public class RedRyderChange : GlobalItem
{
    public override void SetDefaults(Item item)
    {
        if (item.type == ItemID.RedRyder)
        {
            item.damage = 61;
            item.useTime = 64;
            item.useAnimation = 64;
            item.shootSpeed = 16f;
        }
    }

    public override Nullable<bool> UseItem(Item item, Player player)/* tModPorter Suggestion: Return null instead of false */
    {
        if (item.type == ItemID.RedRyder)
        {
            player.GetModPlayer<RedRyderSoundPlayer>().ScheduleCockingSound(10);
        }
        return base.UseItem(item, player);
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (item.type == ItemID.RedRyder)
        {
            tooltips.Add(new TooltipLine(Mod, "Modified", "[c/FF6B6B:Ultracronyx Modified]"));
        }
    }
}

// New class to handle the projectile trail
public class RedRyderProjectile : GlobalProjectile
{
    public bool hasTrail = false;
    
    public override bool InstancePerEntity => true;

    public override void SetDefaults(Projectile projectile)
    {
        base.SetDefaults(projectile);
    }

    // SIMPLE APPROACH: Check if player is holding Red Ryder when bullet AI runs
    public override void AI(Projectile projectile)
    {
        // Only check for bullets
        if (projectile.type == ProjectileID.Bullet && projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
        {
            Player player = Main.player[projectile.owner];
            
            // Check if player is holding Red Ryder
            if (player.active && player.HeldItem.type == ItemID.RedRyder)
            {
                hasTrail = true;
            }
            
            // If we have a trail, create dust
            if (hasTrail && projectile.active)
            {
                // Create dust trail (DustID.137) - 50% chance each frame
                if (Main.rand.NextBool(2))
                {
                    Dust dust = Dust.NewDustPerfect(
                        projectile.Center, 
                        185, // DustID.137
                        Vector2.Zero, 
                        100, 
                        default(Color), 
                        0.5f
                    );
                    
                    dust.noGravity = true;
                    dust.noLight = false;
                    dust.scale = 0.7f + Main.rand.NextFloat(0.3f);
                }
            }
        }
        
        // Call base AI
        base.AI(projectile);
    }

    public override void OnKill(Projectile projectile, int timeLeft)
    {
        if (!hasTrail || projectile.type != ProjectileID.Bullet)
            return;

        // Create a small burst of dust when the projectile dies
        for (int i = 0; i < 5; i++)
        {
            Dust dust = Dust.NewDustDirect(
                projectile.position, 
                projectile.width, 
                projectile.height, 
                185, // DustID.137
                0f, 0f, 100, default(Color), 0.8f
            );
            
            dust.velocity = Main.rand.NextVector2Circular(2f, 2f);
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale = 0.5f + Main.rand.NextFloat(0.5f);
        }
    }
}

public class RedRyderSoundPlayer : ModPlayer
{
    public int soundTimer = 0;
    public bool shouldPlaySound = false;
    public float animationProgress = 0f;
    public bool isAnimating = false;
    public const int ANIMATION_DURATION = 25;

    public override void Initialize()
    {
        soundTimer = 0;
        shouldPlaySound = false;
        animationProgress = 0f;
        isAnimating = false;
    }

    public void ScheduleCockingSound(int delay)
    {
        soundTimer = delay;
        shouldPlaySound = true;
    }

    public override void PostUpdate()
    {
        // Handle sound playback
        if (shouldPlaySound)
        {
            soundTimer--;
            if (soundTimer <= 0)
            {
                PlayCockingSound();
                shouldPlaySound = false;
                soundTimer = 0;
            }
        }

        // Handle lever-action animation
        if (isAnimating && Player.HeldItem.type == ItemID.RedRyder)
        {
            animationProgress += 1f / ANIMATION_DURATION;
            
            // Calculate rotation amount (0 to 90 degrees and back)
            float rotationAmount = 0f;
            if (animationProgress < 0.5f) // First half: lever moves
            {
                rotationAmount = MathHelper.PiOver2 * (animationProgress / 0.5f); // 0° to 90°
            }
            else // Second half: lever returns
            {
                rotationAmount = MathHelper.PiOver2 * (1f - (animationProgress - 0.5f) / 0.5f); // 90° to 0°
            }
            
            // BOTH SIDES RELOAD UPWARDS (visually on screen)
            // JUST SWAP THE SIGNS FROM BEFORE!
            if (Player.direction == 1) // Facing RIGHT
            {
                // If positive was DOWN, then NEGATIVE should be UP
                Player.itemRotation = -rotationAmount; // 0° → -90° = UP
            }
            else // Facing LEFT
            {
                // If negative was DOWN when flipped, then POSITIVE should be UP when flipped
                Player.itemRotation = rotationAmount; // 0° → 90° = when flipped, looks like UP
            }
            
            // End animation
            if (animationProgress >= 1f)
            {
                isAnimating = false;
                animationProgress = 0f;
                Player.itemRotation = 0f;
            }
        }
    }

    private void PlayCockingSound()
    {
        try
        {
            SoundStyle soundStyle = new SoundStyle("Ultracronyx/Content/Sounds/RedRyderCocking")
            {
                Volume = 0.8f,
                PitchVariance = 0.1f,
                MaxInstances = 3
            };
            SoundEngine.PlaySound(soundStyle, Player.Center);
        }
        catch
        {
            SoundEngine.PlaySound(SoundID.Item36 with { Volume = 0.8f }, Player.Center);
        }
        
        // Start animation
        isAnimating = true;
        animationProgress = 0f;
    }

    public override void OnRespawn()
    {
        shouldPlaySound = false;
        soundTimer = 0;
        isAnimating = false;
        animationProgress = 0f;
    }

    public override void PostItemCheck()
    {
        if (Player.HeldItem.type != ItemID.RedRyder && isAnimating)
        {
            isAnimating = false;
            animationProgress = 0f;
            Player.itemRotation = 0f;
        }
    }
}

public class RedRyderShopSystem : GlobalNPC
{
    public override void ModifyShop(NPCShop shop)
    {
        if (shop.NpcType == NPCID.ArmsDealer)
        {
            shop.Add(ItemID.RedRyder);
        }
    }
}