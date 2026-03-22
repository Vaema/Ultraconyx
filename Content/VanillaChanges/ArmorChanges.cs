using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Ultraconyx.Content.VanillaChanges;

public class CopperArmorPlayer : ModPlayer
{
    public bool hasCopperDoubleJump;
    public bool hasCopperDash;
    private bool canDoubleJump;
    private int dashCooldown;
    private int dashDuration;
    private const int DASH_COOLDOWN_MAX = 45; // 0.75 seconds at 60 FPS
    private const int DASH_DURATION_MAX = 10; // 0.167 seconds at 60 FPS
    private const float DASH_SPEED = 16f; // 3 tiles distance over dash duration

    public override void ResetEffects()
    {
        hasCopperDoubleJump = false;
        hasCopperDash = false;
    }

    public override void UpdateEquips()
    {
        // Check for full copper armor set
        if (Player.armor[0].type == ItemID.CopperHelmet &&
            Player.armor[1].type == ItemID.CopperChainmail &&
            Player.armor[2].type == ItemID.CopperGreaves)
        {
            hasCopperDoubleJump = true;
            hasCopperDash = true;
        }
    }

    public override void PostUpdateEquips()
    {
        // Handle dash cooldown
        if (dashCooldown > 0)
        {
            dashCooldown--;
        }

        // Handle dash duration
        if (dashDuration > 0)
        {
            dashDuration--;
            if (dashDuration <= 0)
            {
                // End dash
                Player.velocity.X *= 0.5f; // Slow down after dash
            }

            // Create copper dust during dash
            if (Main.rand.NextBool(2)) // 50% chance per frame
            {
                Dust dust = Dust.NewDustDirect(
                    new Vector2(Player.position.X - 4f, Player.position.Y + Player.height / 2f),
                    8,
                    8,
                    DustID.Copper,
                    0f,
                    0f,
                    100,
                    default(Color),
                    1.2f
                );
                dust.velocity = Player.velocity * -0.5f;
                dust.noGravity = true;
            }
        }

        // Handle double jump
        if (hasCopperDoubleJump)
        {
            // Reset double jump ability when on ground
            if (Player.velocity.Y == 0f)
            {
                canDoubleJump = true;
            }

            // Check for jump input while in air
            if (Player.controlJump && Player.releaseJump && Player.velocity.Y != 0f && canDoubleJump)
            {
                // Perform copper double jump
                Player.velocity.Y = -8.5f;
                canDoubleJump = false;

                // Create copper dust at player's feet
                for (int i = 0; i < 15; i++)
                {
                    Dust dust = Dust.NewDustDirect(
                        new Vector2(Player.position.X, Player.position.Y + Player.height - 4f),
                        Player.width,
                        4,
                        DustID.Copper,
                        0f,
                        0f,
                        100,
                        default(Color),
                        1.5f
                    );
                    dust.velocity *= 0.5f;
                    dust.noGravity = true;
                }

                // Also create some dust that falls down
                for (int i = 0; i < 10; i++)
                {
                    Dust dust = Dust.NewDustDirect(
                        new Vector2(Player.position.X - 2f, Player.position.Y + Player.height - 4f),
                        Player.width + 4,
                        6,
                        DustID.Copper,
                        0f,
                        0f,
                        100,
                        default(Color),
                        1f
                    );
                    dust.velocity.Y = 2f;
                    dust.velocity.X *= 0.5f;
                    dust.noGravity = true;
                }
            }
        }

        // Handle dash input
        if (hasCopperDash && dashCooldown <= 0 && dashDuration <= 0)
        {
            // Check for dash input (double-tap left/right)
            if (Player.controlLeft && Player.releaseLeft)
            {
                StartDash(-1); // Dash left
            }
            else if (Player.controlRight && Player.releaseRight)
            {
                StartDash(1); // Dash right
            }
        }
    }

    private void StartDash(int direction)
    {
        dashDuration = DASH_DURATION_MAX;
        dashCooldown = DASH_COOLDOWN_MAX;

        // Set dash velocity
        Player.velocity.X = DASH_SPEED * direction;

        // Create initial copper dust burst
        for (int i = 0; i < 20; i++)
        {
            Dust dust = Dust.NewDustDirect(
                new Vector2(Player.position.X, Player.position.Y + Player.height / 2f - 4f),
                Player.width,
                8,
                DustID.Copper,
                0f,
                0f,
                100,
                default(Color),
                1.5f
            );
            dust.velocity = new Vector2(-direction * 2f, Main.rand.NextFloat(-2f, 2f));
            dust.noGravity = true;
        }
    }

    public override bool CanUseItem(Item item)
    {
        // Prevent item use during dash
        if (dashDuration > 0)
        {
            return false;
        }
        return base.CanUseItem(item);
    }
}

public class ArmorChanges : GlobalItem
{
    public override void SetDefaults(Item item)
    {
        // Tin Armor changes
        if (item.type == ItemID.TinHelmet)
        {
            item.defense = 2;
            item.DamageType = DamageClass.Melee;
            item.damage = 0;
        }
        else if (item.type == ItemID.TinChainmail)
        {
            item.defense = 3;
        }
        else if (item.type == ItemID.TinGreaves)
        {
            item.defense = 2;
        }

        // Copper Armor changes
        else if (item.type == ItemID.CopperHelmet)
        {
            item.defense = 0; // No defense as requested
            item.DamageType = DamageClass.Summon;
        }
        else if (item.type == ItemID.CopperChainmail)
        {
            item.defense = 2; // 2 defense as requested
        }
        else if (item.type == ItemID.CopperGreaves)
        {
            item.defense = 1; // Keep default or adjust as needed
        }
    }

    public override void UpdateEquip(Item item, Player player)
    {
        // Tin Helmet: +10% melee damage
        if (item.type == ItemID.TinHelmet)
        {
            player.GetDamage(DamageClass.Melee) += 0.10f;
        }

        // Tin Chainmail: +1 minion slot
        if (item.type == ItemID.TinChainmail)
        {
            player.maxMinions++;
        }

        // Tin Greaves: +10% movement speed
        if (item.type == ItemID.TinGreaves)
        {
            player.moveSpeed += 0.10f;
        }

        // Copper Helmet: +15% summon damage
        if (item.type == ItemID.CopperHelmet)
        {
            player.GetDamage(DamageClass.Summon) += 0.15f;
        }

        // Copper Chainmail: +1 minion slot
        if (item.type == ItemID.CopperChainmail)
        {
            player.maxMinions++;
        }

        // Copper Greaves: +15% movement speed, +10% acceleration speed (changed from 20%)
        if (item.type == ItemID.CopperGreaves)
        {
            player.moveSpeed += 0.15f;
            player.runAcceleration += 0.10f; // Changed to +10% acceleration
            player.maxRunSpeed += 0.10f; // Slightly increase max speed
        }
    }

    public override string IsArmorSet(Item head, Item body, Item legs)
    {
        // Check if all pieces are Tin Armor
        if (head.type == ItemID.TinHelmet && body.type == ItemID.TinChainmail && legs.type == ItemID.TinGreaves)
        {
            return "TinArmorSet";
        }
        // Check if all pieces are Copper Armor
        if (head.type == ItemID.CopperHelmet && body.type == ItemID.CopperChainmail && legs.type == ItemID.CopperGreaves)
        {
            return "CopperArmorSet";
        }
        return base.IsArmorSet(head, body, legs);
    }

    public override void UpdateArmorSet(Player player, string set)
    {
        if (set == "TinArmorSet")
        {
            // Tin Armor Set Bonus
            player.statDefense += 3; // +3 defense
            player.GetAttackSpeed(DamageClass.Melee) += 0.05f; // +5% melee speed
            player.GetDamage(DamageClass.Ranged) += 0.10f; // +10% ranged damage

            // Enable summon critical hits
            player.GetCritChance(DamageClass.Summon) += 100f; // Makes summons have a chance to crit

            // Set bonus tooltip
            player.setBonus = "+3 defense, +5% melee speed, +10% ranged damage\nSummons have a chance to deal critical hits";
        }
        else if (set == "CopperArmorSet")
        {
            // Increase summon tag damage by 5%
            player.GetDamage(DamageClass.Summon) += 0.05f;

            // Set bonus tooltip - removed mention of default dash since we have custom dash
            player.setBonus = "Grants copper dash (3 tiles) and double jump (4 tiles high)\n+5% summon damage";
        }
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        // Update tooltips to reflect changes
        if (item.type == ItemID.TinHelmet)
        {
            AddOrReplaceTooltip(tooltips, "+10% melee damage");
        }
        else if (item.type == ItemID.TinChainmail)
        {
            AddOrReplaceTooltip(tooltips, "+1 minion slot");
        }
        else if (item.type == ItemID.TinGreaves)
        {
            AddOrReplaceTooltip(tooltips, "+10% movement speed");
        }
        else if (item.type == ItemID.CopperHelmet)
        {
            AddOrReplaceTooltip(tooltips, "+15% summon damage");
        }
        else if (item.type == ItemID.CopperChainmail)
        {
            AddOrReplaceTooltip(tooltips, "+1 minion slot");
        }
        else if (item.type == ItemID.CopperGreaves)
        {
            AddOrReplaceTooltip(tooltips, "+15% movement speed, +10% acceleration");
        }
    }

    private void AddOrReplaceTooltip(List<TooltipLine> tooltips, string newTooltip)
    {
        // Find the tooltip line that contains stat bonuses
        int statIndex = -1;
        for (int i = 0; i < tooltips.Count; i++)
        {
            if (tooltips[i].Name == "Tooltip0" || tooltips[i].Name == "Tooltip1")
            {
                statIndex = i;
                break;
            }
        }

        if (statIndex != -1)
        {
            // Replace existing tooltip
            tooltips[statIndex].Text = newTooltip;
        }
        else
        {
            // Add new tooltip if not found
            tooltips.Add(new TooltipLine(Mod, "CustomTooltip", newTooltip));
        }
    }
}