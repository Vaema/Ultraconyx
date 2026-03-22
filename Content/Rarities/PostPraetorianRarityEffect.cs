using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace Ultraconyx.Content.Rarities;

public class PostPraetorianRarityEffect : GlobalItem
{
    // Glow radius setting - increase this for bigger glow
    private const int GLOW_RADIUS = 4;

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        // Check if item has our rarity
        if (item.rare == ModContent.RarityType<PostPraetorian>())
        {
            foreach (TooltipLine line in tooltips)
            {
                // Find the item name line
                if (line.Name == "ItemName" && line.Mod == "Terraria")
                {
                    // Dark pink text color
                    line.OverrideColor = new Color(255, 80, 180);
                    break;
                }
            }
        }
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        // Check if this is our rarity's name line - NO EXTRA FILTERING
        if (item.rare == ModContent.RarityType<PostPraetorian>() &&
            line.Name == "ItemName" &&
            line.Mod == "Terraria")
        {
            SpriteBatch spriteBatch = Main.spriteBatch;

            // Draw bright pink glow in a smooth circle
            for (int i = -GLOW_RADIUS; i <= GLOW_RADIUS; i++)
            {
                for (int j = -GLOW_RADIUS; j <= GLOW_RADIUS; j++)
                {
                    // Skip center
                    if (i == 0 && j == 0) continue;

                    // Calculate distance from center
                    float distance = (float)System.Math.Sqrt(i * i + j * j);

                    // Only draw within circular radius
                    if (distance <= GLOW_RADIUS + 0.5f)
                    {
                        // Bright opacity falloff based on distance
                        float opacity = 0.6f * (1f - (distance / (GLOW_RADIUS + 1f)) * 0.7f);

                        Utils.DrawBorderStringFourWay(
                            spriteBatch,
                            line.Font,
                            line.Text,
                            line.X + i,
                            line.Y + j,
                            new Color(255, 140, 255) * opacity,
                            Color.Transparent,
                            line.BaseScale
                        );
                    }
                }
            }

            // Draw dark pink text
            Utils.DrawBorderStringFourWay(
                spriteBatch,
                line.Font,
                line.Text,
                line.X,
                line.Y,
                new Color(255, 80, 180),
                Color.Black,
                line.BaseScale
            );

            // Draw white gradient shine on top-left
            float shineDistance = 1.5f;
            float shineOpacity = 0.4f;

            // Top-left shine (white with gradient)
            Utils.DrawBorderStringFourWay(
                spriteBatch,
                line.Font,
                line.Text,
                line.X - 1,
                line.Y - 1,
                new Color(255, 255, 255) * 0.3f,
                Color.Transparent,
                line.BaseScale
            );

            Utils.DrawBorderStringFourWay(
                spriteBatch,
                line.Font,
                line.Text,
                line.X - 2,
                line.Y - 2,
                new Color(255, 255, 255) * 0.15f,
                Color.Transparent,
                line.BaseScale
            );

            Utils.DrawBorderStringFourWay(
                spriteBatch,
                line.Font,
                line.Text,
                line.X - 3,
                line.Y - 3,
                new Color(255, 220, 255) * 0.08f,
                Color.Transparent,
                line.BaseScale
            );

            return false;
        }

        return true;
    }
}