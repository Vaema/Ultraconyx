using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;

namespace Ultraconyx.Content.UI.ChaosMode;

public class ChaosModeCameraSystem : ModSystem
{
    // Camera follow variables
    private static Vector2 targetPosition = Vector2.Zero;
    private static float followFactor; // 0 = normal center, 1 = follow target
    private static int followTimer;
    private const int FOLLOW_DURATION = 180; // 3 seconds at 60fps (matches intro duration)
    private static int activeBossIndex = -1;

    // Smoothing variables
    private static Vector2 currentCameraOffset = Vector2.Zero;
    private static float smoothSpeed = 0.1f;

    public override void ModifyScreenPosition()
    {
        // Only modify if we're actively following
        if (followFactor <= 0f || followTimer <= 0)
            return;

        // Find the Eye of Cthulhu if we don't have a valid boss index
        if (activeBossIndex == -1 || !Main.npc[activeBossIndex].active || Main.npc[activeBossIndex].type != NPCID.EyeofCthulhu)
        {
            FindEyeOfCthulhu();
        }

        // If we have a valid boss to follow
        if (activeBossIndex != -1 && Main.npc[activeBossIndex].active)
        {
            NPC boss = Main.npc[activeBossIndex];
            targetPosition = boss.Center;

            // Calculate the desired camera offset
            Vector2 desiredOffset = targetPosition - Main.screenPosition - new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);

            // Smoothly move towards the desired offset
            currentCameraOffset = Vector2.Lerp(currentCameraOffset, desiredOffset * followFactor, smoothSpeed);

            // Apply the offset to the screen position
            Main.screenPosition += currentCameraOffset;
        }

        followTimer--;
        if (followTimer <= 0)
        {
            // Smoothly return to normal
            followFactor = Math.Max(0, followFactor - 0.02f);
            if (followFactor <= 0)
            {
                followFactor = 0;
                activeBossIndex = -1;
                currentCameraOffset = Vector2.Zero;
            }
        }
    }

    private void FindEyeOfCthulhu()
    {
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            if (Main.npc[i].active && Main.npc[i].type == NPCID.EyeofCthulhu)
            {
                activeBossIndex = i;
                break;
            }
        }
    }

    // Call this to start following the Eye of Cthulhu
    public static void StartFollowing()
    {
        followFactor = 1f;
        followTimer = FOLLOW_DURATION;
        currentCameraOffset = Vector2.Zero;

        // Immediately find the boss
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            if (Main.npc[i].active && Main.npc[i].type == NPCID.EyeofCthulhu)
            {
                activeBossIndex = i;
                break;
            }
        }
    }

    // Check if we're currently following
    public static bool IsFollowing()
    {
        return followFactor > 0 && followTimer > 0;
    }

    public override void Unload()
    {
        targetPosition = Vector2.Zero;
        currentCameraOffset = Vector2.Zero;
        activeBossIndex = -1;
    }
}