using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Ultraconyx.Content.UI.ChaosMode;

public class ChaosModeUI : UIState
{
    private static bool chaosModeActive;

    private UIImageButton chaosModeButton;
    private static Texture2D buttonTexture;

    // Blinking animation variables.
    private int blinkTimer;
    private int blinkDuration;
    private bool isBlinking;
    private Random random = new();

    // Frame states.
    private const int FRAME_CLOSED = 0;
    private const int FRAME_OPEN = 1;

    public override void OnInitialize()
    {
        // Load the texture.
        buttonTexture = ModContent.Request<Texture2D>("Ultraconyx/Content/UI/ChaosMode/ChaosMode", AssetRequestMode.ImmediateLoad).Value;

        // Create a button.
        chaosModeButton = new UIImageButton(buttonTexture);
        chaosModeButton.SetVisibility(1f, 0.8f);

        // Set the button position.
        chaosModeButton.Left.Set(Main.screenWidth / 2 - buttonTexture.Width / 2, 0f);
        chaosModeButton.Top.Set(20f, 0f);
        chaosModeButton.Width.Set(buttonTexture.Width, 0f);
        chaosModeButton.Height.Set(buttonTexture.Height / 2, 0f);

        // Set initial frame (closed eye).
        chaosModeButton.SetFrame(FRAME_CLOSED);

        // Add hover text and lock the mouse interface.
        chaosModeButton.OnUpdate += (uiElement) =>
        {
            if (uiElement.IsMouseHovering)
            {
                string hoverText = chaosModeActive ? "Chaos Mode (Active)" : "Chaos Mode";
                Main.hoverItemName = hoverText;

                // Prevent item swinging while hovering over the button.
                Main.LocalPlayer.mouseInterface = true;
            }
        };

        // Add the click handler.
        chaosModeButton.OnLeftClick += (evt, listeningElement) =>
        {
            ToggleChaosMode();
        };

        // Add the button to the UI.
        Append(chaosModeButton);
    }

    private void ToggleChaosMode()
    {
        chaosModeActive = !chaosModeActive;

        if (chaosModeActive)
        {
            // Display the activation message in dark red.
            string message = "Chaos Mode activated. Have fun.";
            Color darkRed = new(139, 0, 0);

            if (Main.netMode == NetmodeID.SinglePlayer)
                Main.NewText(message, darkRed);
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // For multiplayer, we need to sync.
                ModPacket packet = ModContent.GetInstance<Ultraconyx>().GetPacket();
                packet.Write((byte)Ultraconyx.MessageType.ChaosModeActivated);
                packet.Write(chaosModeActive);
                packet.Send();

                Main.NewText(message, darkRed);
            }

            // Start with the open eye frame.
            chaosModeButton.SetFrame(FRAME_OPEN);

            // Initialize blink timer.
            blinkTimer = 0;
            isBlinking = false;
        }
        else
        {
            if (Main.netMode == Terraria.ID.NetmodeID.SinglePlayer)
                Main.NewText("Chaos Mode deactivated.", Color.Gray);

            // Return to the closed eye frame.
            chaosModeButton.SetFrame(FRAME_CLOSED);
        }
    }

    public override void Update(GameTime gameTime)
    {
        // Update button position if the screen size changes.
        if (chaosModeButton != null && buttonTexture != null)
        {
            chaosModeButton.Left.Set(Main.screenWidth / 2 - buttonTexture.Width / 2, 0f);
            chaosModeButton.Top.Set(20f, 0f);
            chaosModeButton.Recalculate();
        }

        // Handle the blinking animation when Chaos Mode is active.
        if (chaosModeActive)
            HandleBlinking();

        // Additional safety: ensure mouseInterface is true while hovering.
        // This is already handled in OnUpdate, but this is a backup.
        if (chaosModeButton != null && chaosModeButton.IsMouseHovering)
            Main.LocalPlayer.mouseInterface = true;
    }

    private void HandleBlinking()
    {
        if (isBlinking)
        {
            blinkTimer++;
            if (blinkTimer >= blinkDuration)
            {
                // Return to the open eye frame when finished.
                chaosModeButton.SetFrame(FRAME_OPEN);
                isBlinking = false;
                blinkTimer = 0;
            }
        }
        else
        {
            // When not blinking, have a chance to start blinking.
            if (random.Next(1000) < 5)
                StartBlink();
        }
    }

    private void StartBlink()
    {
        isBlinking = true;

        // Blink duration.
        blinkDuration = random.Next(2, 8);
        blinkTimer = 0;

        // Show the closed eye frame.
        chaosModeButton.SetFrame(FRAME_CLOSED);
    }

    public static bool IsChaosModeActive() => chaosModeActive;

    public static void SetChaosMode(bool active) => chaosModeActive = active;
}
