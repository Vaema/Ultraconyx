using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;
using System.Collections.Generic;
using ReLogic.Content;
using Terraria.UI;

namespace Ultraconyx.Content.UI.ChaosMode.EoC;

public class EoCIntroSystem : ModSystem
{
    // Intro state
    private static bool isPlayingIntro;
    private static int introTimer;
    private static int introPhase; // 0: spawn, 1: move naturally, 2: stop and sparkle, 3: complete
    private static NPC eoC;
    private static int eoCIndex = -1;
    
    // Sparkle effect
    private static Texture2D sparkleTexture;
    private static float sparkleAlpha;
    private static float sparkleScale = 1f;
    private static int sparkleTimer;
    private const int SPARKLE_DURATION = 45; // Frames for sparkle effect
    
    // Movement phase duration
    private const int MOVE_DURATION = 90; // 1.5 seconds of natural movement
    
    // Random
    private static Random random = new();
    
    // Store original AI values to restore later
    private static float[] originalAI = new float[4];
    
    // MANUAL SPARKLE POSITION OFFSET - Adjust these values!
    // ============================================================================
    // These are raw pixel offsets from the center of EoC
    // Positive X = right, Negative X = left
    // Positive Y = down, Negative Y = up
    private static float sparkleOffsetX = 0.2f;    // Horizontal offset
    private static float sparkleOffsetY = 65f;   // Vertical offset (negative = up toward tip)
    // ============================================================================
    
    public override void Load()
    {
        if (!Main.dedServ)
        {
            sparkleTexture = ModContent.Request<Texture2D>("Ultracronyx/Content/UI/ChaosMode/EoC/EoCFlash", AssetRequestMode.ImmediateLoad).Value;
        }
    }
    
    public override void PostUpdateWorld()
    {
        if (!isPlayingIntro) return;
        
        introTimer++;
        
        switch (introPhase)
        {
            case 0: // Spawn
                SpawnEoC();
                introPhase = 1;
                introTimer = 0;
                break;
                
            case 1: // Move naturally
                if (eoC != null && eoC.active)
                {
                    // Let EoC move naturally using its own AI
                    
                    // Make sure it's looking at player occasionally
                    if (introTimer % 30 == 0)
                    {
                        Vector2 directionToPlayer = Main.player[Main.myPlayer].Center - eoC.Center;
                        eoC.rotation = directionToPlayer.ToRotation() - MathHelper.PiOver2;
                    }
                    
                    if (introTimer >= MOVE_DURATION)
                    {
                        introPhase = 2;
                        introTimer = 0;
                        sparkleTimer = 0;
                        sparkleAlpha = 0f;
                        
                        // Stop the boss by setting velocity to zero
                        if (eoC != null && eoC.active)
                        {
                            eoC.velocity = Vector2.Zero;
                            eoC.netUpdate = true;
                        }
                    }
                }
                break;
                
            case 2: // Stop and sparkle
                if (eoC != null && eoC.active)
                {
                    // Keep the boss frozen
                    eoC.velocity = Vector2.Zero;
                    
                    sparkleTimer++;
                    
                    // Sparkle animation: quick fade in, hold, quick fade out
                    if (sparkleTimer <= 10)
                    {
                        // Quick fade in
                        sparkleAlpha = sparkleTimer / 10f;
                        sparkleScale = 0.3f + (sparkleTimer / 10f) * 0.7f;
                    }
                    else if (sparkleTimer <= 25)
                    {
                        // Hold at full
                        sparkleAlpha = 1f;
                        sparkleScale = 1f;
                    }
                    else if (sparkleTimer <= 40)
                    {
                        // Fade out
                        float fadeProgress = (sparkleTimer - 25) / 15f;
                        sparkleAlpha = 1f - fadeProgress;
                        sparkleScale = 1f + fadeProgress * 0.3f;
                    }
                    
                    // Make sure alpha doesn't go below 0
                    if (sparkleAlpha < 0) sparkleAlpha = 0;
                    
                    if (sparkleTimer >= SPARKLE_DURATION)
                    {
                        introPhase = 3;
                        sparkleAlpha = 0f;
                    }
                }
                break;
                
            case 3: // Complete
                EndIntro();
                break;
        }
    }
    
    private void SpawnEoC()
    {
        // Find EoC
        eoCIndex = -1;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            if (Main.npc[i].active && Main.npc[i].type == NPCID.EyeofCthulhu)
            {
                eoCIndex = i;
                eoC = Main.npc[i];
                break;
            }
        }
        
        if (eoC == null) return;
        
        // Store original AI values
        for (int i = 0; i < 4; i++)
        {
            originalAI[i] = eoC.ai[i];
        }
        
        // Random spawn position above the screen
        Player player = Main.player[Main.myPlayer];
        
        // Random horizontal offset (up to 400 pixels left or right)
        int randomXOffset = random.Next(-400, 401);
        
        // Random height above screen (between 300-600 pixels above)
        int randomYOffset = random.Next(300, 601);
        
        float spawnX = player.Center.X + randomXOffset;
        float spawnY = player.Center.Y - Main.screenHeight - randomYOffset;
        
        eoC.Center = new Vector2(spawnX, spawnY);
        
        // Give it a little initial velocity towards player for natural feel
        Vector2 directionToPlayer = player.Center - eoC.Center;
        directionToPlayer.Normalize();
        eoC.velocity = directionToPlayer * 2f;
        
        eoC.netUpdate = true;
    }
    
    private void EndIntro()
    {
        isPlayingIntro = false;
        
        if (eoC != null && eoC.active)
        {
            // Restore original AI values
            for (int i = 0; i < 4; i++)
            {
                eoC.ai[i] = originalAI[i];
            }
            
            // Give it a little push to restart its AI
            eoC.velocity = new Vector2(random.Next(-3, 4), random.Next(-2, 1));
            eoC.netUpdate = true;
        }
        
        eoC = null;
        eoCIndex = -1;
    }
    
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        // Only draw if we have a sparkle to show
        if (!isPlayingIntro || sparkleAlpha <= 0.01f || eoC == null || !eoC.active || sparkleTexture == null)
            return;
        
        // Find a good layer to draw on top of EoC
        int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: NPC Head"));
        if (index == -1)
            index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        
        if (index != -1)
        {
            layers.Insert(index, new LegacyGameInterfaceLayer(
                "Ultracronyx: EoC Sparkle",
                delegate {
                    DrawSparkle();
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
    
    private void DrawSparkle()
    {
        if (sparkleTexture == null || sparkleAlpha <= 0.01f || eoC == null || !eoC.active) 
            return;
        
        // Get EoC's position on screen
        Vector2 eoCScreenPos = eoC.Center - Main.screenPosition;
        
        // Apply manual offset
        Vector2 sparklePos = eoCScreenPos + new Vector2(sparkleOffsetX, sparkleOffsetY);
        
        // Draw main sparkle
        Color sparkleColor = Color.White * sparkleAlpha;
        
        Main.spriteBatch.Draw(
            sparkleTexture,
            sparklePos,
            null,
            sparkleColor,
            0f, // No rotation
            new Vector2(sparkleTexture.Width / 2, sparkleTexture.Height / 2),
            sparkleScale,
            SpriteEffects.None,
            0f
        );
        
        // Add a second smaller sparkle for more effect
        Main.spriteBatch.Draw(
            sparkleTexture,
            sparklePos - new Vector2(3, 3),
            null,
            sparkleColor * 0.6f,
            0.2f,
            new Vector2(sparkleTexture.Width / 2, sparkleTexture.Height / 2),
            sparkleScale * 0.5f,
            SpriteEffects.None,
            0f
        );
    }
    
    public static void StartIntro()
    {
        isPlayingIntro = true;
        introTimer = 0;
        introPhase = 0;
        sparkleAlpha = 0f;
        sparkleScale = 0f;
    }
    
    public static bool IsPlayingIntro()
    {
        return isPlayingIntro;
    }
    
    public override void Unload()
    {
        sparkleTexture = null;
    }
}