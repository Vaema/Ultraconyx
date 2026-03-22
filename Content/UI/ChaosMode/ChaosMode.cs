using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using System;

namespace Ultraconyx.Content.UI.ChaosMode;

public class ChaosModeUI : UIState
{
    private UIImageButton chaosModeButton;
    private static bool chaosModeActive = false;
    
    // Texture reference
    private static Texture2D buttonTexture;
    
    // Blinking animation variables
    private int blinkTimer = 0;
    private int blinkDuration = 0;
    private bool isBlinking = false;
    private Random random = new Random();
    
    // Frame states
    private const int FRAME_CLOSED = 0;  // Frame 0 - closed eye
    private const int FRAME_OPEN = 1;     // Frame 1 - open eye
    
    public override void OnInitialize()
    {
        // Load the texture
        buttonTexture = ModContent.Request<Texture2D>("Ultracronyx/Content/UI/ChaosMode/ChaosMode", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        
        // Create the button
        chaosModeButton = new UIImageButton(buttonTexture);
        chaosModeButton.SetVisibility(1f, 0.8f);
        
        // Set the button position (Top Middle of the screen)
        chaosModeButton.Left.Set(Main.screenWidth / 2 - buttonTexture.Width / 2, 0f);
        chaosModeButton.Top.Set(20f, 0f);
        chaosModeButton.Width.Set(buttonTexture.Width, 0f);
        chaosModeButton.Height.Set(buttonTexture.Height / 2, 0f);
        
        // Set initial frame (closed eye)
        chaosModeButton.SetFrame(FRAME_CLOSED);
        
        // Add hover text and mouse interface lock
        chaosModeButton.OnUpdate += (uiElement) => {
            if (uiElement.IsMouseHovering)
            {
                string hoverText = chaosModeActive ? "Chaos Mode (Active)" : "Chaos Mode";
                Main.hoverItemName = hoverText;
                
                // Prevent item swinging while hovering over the button
                Main.LocalPlayer.mouseInterface = true;
            }
        };
        
        // Add click handler
        chaosModeButton.OnLeftClick += (evt, listeningElement) => {
            ToggleChaosMode();
        };
        
        // Add the button to the UI
        Append(chaosModeButton);
    }
    
    private void ToggleChaosMode()
    {
        chaosModeActive = !chaosModeActive;
        
        if (chaosModeActive)
        {
            // Display activation message in dark red
            string message = "Chaos Mode activated... have fun...";
            Color darkRed = new Color(139, 0, 0);
            
            if (Main.netMode == Terraria.ID.NetmodeID.SinglePlayer)
            {
                Main.NewText(message, darkRed);
            }
            else if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
            {
                // For multiplayer, we need to sync
                ModPacket packet = ModContent.GetInstance<Ultraconyx>().GetPacket();
                packet.Write((byte)Ultraconyx.MessageType.ChaosModeActivated);
                packet.Write(chaosModeActive);
                packet.Send();
                
                Main.NewText(message, darkRed);
            }
            
            // Start with open eye
            chaosModeButton.SetFrame(FRAME_OPEN);
            
            // Initialize blink timer
            blinkTimer = 0;
            isBlinking = false;
        }
        else
        {
            if (Main.netMode == Terraria.ID.NetmodeID.SinglePlayer)
            {
                Main.NewText("Chaos Mode deactivated", Color.Gray);
            }
            
            // Return to closed eye
            chaosModeButton.SetFrame(FRAME_CLOSED);
        }
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Update button position if screen size changes
        if (chaosModeButton != null && buttonTexture != null)
        {
            chaosModeButton.Left.Set(Main.screenWidth / 2 - buttonTexture.Width / 2, 0f);
            chaosModeButton.Top.Set(20f, 0f);
            
            // Recalculate dimensions
            chaosModeButton.Recalculate();
        }
        
        // Handle blinking animation when Chaos Mode is active
        if (chaosModeActive)
        {
            HandleBlinking();
        }
        
        // Additional safety: ensure mouseInterface is true while hovering
        // This is already handled in OnUpdate, but this is a backup
        if (chaosModeButton != null && chaosModeButton.IsMouseHovering)
        {
            Main.LocalPlayer.mouseInterface = true;
        }
    }
    
    private void HandleBlinking()
    {
        if (isBlinking)
        {
            // Currently in a blink
            blinkTimer++;
            
            if (blinkTimer >= blinkDuration)
            {
                // Blink finished - return to open eye
                chaosModeButton.SetFrame(FRAME_OPEN);
                isBlinking = false;
                blinkTimer = 0;
            }
        }
        else
        {
            // Not blinking - chance to start a blink
            // Random chance per frame (about 1% chance every update, ~60 times per second)
            if (random.Next(1000) < 5) // 0.5% chance per update
            {
                StartBlink();
            }
        }
    }
    
    private void StartBlink()
    {
        isBlinking = true;
        
        // Blink duration between 2-8 frames (at 60fps, that's 0.03-0.13 seconds)
        blinkDuration = random.Next(2, 8);
        blinkTimer = 0;
        
        // Show closed eye (frame 0)
        chaosModeButton.SetFrame(FRAME_CLOSED);
    }
    
    public static bool IsChaosModeActive()
    {
        return chaosModeActive;
    }
    
    public static void SetChaosMode(bool active)
    {
        chaosModeActive = active;
    }
}