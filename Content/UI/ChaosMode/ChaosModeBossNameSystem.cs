using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.GameContent;
using ReLogic.Graphics;

namespace Ultraconyx.Content.UI.ChaosMode;

// This system handles drawing the boss name animation
public class ChaosModeBossNameSystem : ModSystem
{
    // We need a way to access the animation data from the player
    public static string CurrentBossName = "";
    public static int DisplayTimer = 0;
    public static bool IsTyping = false;
    public static int TypingTimer = 0;
    public static string FullBossName = "";
    public static int TypingIndex = 0;
    
    // Typing delay - consistent for regular typing animation
    private const int TYPING_DELAY = 6; // Frames between each character (steady pace)
    
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        // Only draw if we have something to show
        if (DisplayTimer <= 0 || string.IsNullOrEmpty(CurrentBossName))
            return;
        
        // Find the right place to insert our layer
        int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (index == -1)
            index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
        
        if (index != -1)
        {
            // Insert our custom drawing layer
            layers.Insert(index, new LegacyGameInterfaceLayer(
                "Ultracronyx: Chaos Mode Boss Name",
                delegate {
                    DrawBossName();
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
    
    private void DrawBossName()
    {
        // Don't draw on server
        if (Main.netMode == Terraria.ID.NetmodeID.Server)
            return;
        
        // Calculate alpha based on display timer (fade in/out)
        float alpha = 1f;
        if (DisplayTimer < 30) // Fade out in last 30 frames
        {
            alpha = DisplayTimer / 30f;
        }
        else if (DisplayTimer > 150) // Fade in first 30 frames
        {
            alpha = (180 - DisplayTimer) / 30f;
            if (alpha < 0) alpha = 0;
            if (alpha > 1) alpha = 1;
        }
        
        // Calculate position (centered at bottom of screen)
        int centerX = Main.screenWidth / 2;
        int centerY = Main.screenHeight - 150; // 150 pixels from bottom
        
        // Draw the text
        string displayText = CurrentBossName;
        
        // Add blinking cursor while typing
        if (IsTyping && (TypingTimer / 6) % 2 == 0) // Blink every 12 frames
        {
            displayText += "_";
        }
        
        // Get the font
        var font = FontAssets.DeathText.Value;
        
        // Measure text size
        Vector2 textSize = font.MeasureString(displayText);
        Vector2 position = new Vector2(centerX - textSize.X / 2, centerY);
        
        // Boss health bar colors - dark red on both sides, white in the middle
        Color leftColor = new Color(180, 40, 40) * alpha; // Dark red (left side)
        Color middleColor = new Color(255, 255, 200) * alpha; // Warm white (middle)
        Color rightColor = new Color(180, 40, 40) * alpha; // Dark red (right side)
        
        // Outline colors - slightly brighter versions
        Color leftOutlineColor = new Color(220, 60, 60) * alpha;
        Color middleOutlineColor = new Color(255, 255, 220) * alpha;
        Color rightOutlineColor = new Color(220, 60, 60) * alpha;
        
        // Draw multiple outline layers for thickness
        // Outer outline (darker)
        DrawGradientString(Main.spriteBatch, font, displayText, new Vector2(position.X - 2, position.Y - 2), 
            leftOutlineColor * 0.4f, middleOutlineColor * 0.4f, rightOutlineColor * 0.4f);
        DrawGradientString(Main.spriteBatch, font, displayText, new Vector2(position.X + 2, position.Y - 2), 
            leftOutlineColor * 0.4f, middleOutlineColor * 0.4f, rightOutlineColor * 0.4f);
        DrawGradientString(Main.spriteBatch, font, displayText, new Vector2(position.X - 2, position.Y + 2), 
            leftOutlineColor * 0.4f, middleOutlineColor * 0.4f, rightOutlineColor * 0.4f);
        DrawGradientString(Main.spriteBatch, font, displayText, new Vector2(position.X + 2, position.Y + 2), 
            leftOutlineColor * 0.4f, middleOutlineColor * 0.4f, rightOutlineColor * 0.4f);
        
        // Inner outline (brighter)
        DrawGradientString(Main.spriteBatch, font, displayText, new Vector2(position.X - 1, position.Y - 1), 
            leftOutlineColor, middleOutlineColor, rightOutlineColor);
        DrawGradientString(Main.spriteBatch, font, displayText, new Vector2(position.X + 1, position.Y - 1), 
            leftOutlineColor, middleOutlineColor, rightOutlineColor);
        DrawGradientString(Main.spriteBatch, font, displayText, new Vector2(position.X - 1, position.Y + 1), 
            leftOutlineColor, middleOutlineColor, rightOutlineColor);
        DrawGradientString(Main.spriteBatch, font, displayText, new Vector2(position.X + 1, position.Y + 1), 
            leftOutlineColor, middleOutlineColor, rightOutlineColor);
        
        // Draw shadow (darker version behind everything)
        Vector2 shadowPos = new Vector2(position.X + 4, position.Y + 4);
        DrawGradientString(Main.spriteBatch, font, displayText, shadowPos, 
            new Color(40, 10, 10) * alpha * 0.6f, 
            new Color(60, 40, 40) * alpha * 0.6f,
            new Color(40, 10, 10) * alpha * 0.6f);
        
        // Draw main text with boss health bar colors
        DrawGradientString(Main.spriteBatch, font, displayText, position, leftColor, middleColor, rightColor);
    }
    
    private void DrawGradientString(SpriteBatch spriteBatch, DynamicSpriteFont font, string text, Vector2 position, 
        Color leftColor, Color middleColor, Color rightColor)
    {
        // Draw each character individually with its own color based on position in the string
        Vector2 charPos = position;
        
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            string charStr = c.ToString();
            Vector2 charSize = font.MeasureString(charStr);
            
            // Calculate gradient factor (0 = left, 0.5 = middle, 1 = right)
            float factor = (float)i / (text.Length - 1);
            if (text.Length == 1) factor = 0.5f;
            
            // Interpolate between colors based on factor
            Color charColor;
            if (factor <= 0.5f)
            {
                // Left half: interpolate between left and middle
                float t = factor * 2f; // Map 0-0.5 to 0-1
                charColor = Color.Lerp(leftColor, middleColor, t);
            }
            else
            {
                // Right half: interpolate between middle and right
                float t = (factor - 0.5f) * 2f; // Map 0.5-1 to 0-1
                charColor = Color.Lerp(middleColor, rightColor, t);
            }
            
            // Draw the character
            spriteBatch.DrawString(font, charStr, charPos, charColor);
            
            // Move position for next character
            charPos.X += charSize.X;
        }
    }
    
    // Called from ChaosModePlayer to update typing with regular animation
    public static void UpdateTyping()
    {
        if (!IsTyping || TypingIndex >= FullBossName.Length)
        {
            IsTyping = false;
            return;
        }
        
        TypingTimer++;
        
        // Regular typing - consistent delay between characters
        if (TypingTimer >= TYPING_DELAY)
        {
            // Add the next character
            char nextChar = FullBossName[TypingIndex];
            CurrentBossName += nextChar;
            TypingIndex++;
            TypingTimer = 0;
        }
    }
    
    // Call this to start typing a new boss name
    public static void StartTyping(string name)
    {
        FullBossName = name.ToUpper();
        CurrentBossName = "";
        TypingIndex = 0;
        TypingTimer = 0;
        IsTyping = true;
        DisplayTimer = 180; // 3 seconds at 60fps
    }
    
    public override void Unload()
    {
        CurrentBossName = "";
        FullBossName = "";
    }
}