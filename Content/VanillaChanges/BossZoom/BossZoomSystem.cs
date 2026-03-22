using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ultraconyx.Content.VanillaChanges.BossZoom;

public class BossZoomSystem : ModSystem
{
    private static Vector2 targetPosition;
    private static float zoomFactor;
    private static float targetZoomFactor;
    private static int zoomTimeLeft;
    private const int ZOOM_DURATION = 60; // 1 second at 60 FPS
    
    // Store the boss NPC to follow during transition
    private static int followingBossIndex = -1;
    
    public override void ModifyScreenPosition()
    {
        if (zoomFactor > 0f)
        {
            // Update target position to follow the boss if we're tracking one
            if (followingBossIndex != -1)
            {
                NPC boss = Main.npc[followingBossIndex];
                if (boss.active && boss.boss)
                {
                    targetPosition = boss.Center;
                }
                else
                {
                    // Boss died or despawned, end the zoom
                    zoomTimeLeft = 0;
                    followingBossIndex = -1;
                }
            }
            
            Vector2 normalPosition = Main.screenPosition;
            Vector2 bossCenteredPosition = targetPosition - new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
            
            Main.screenPosition = Vector2.Lerp(normalPosition, bossCenteredPosition, zoomFactor);
        }
    }
    
    public override void PostUpdatePlayers()
    {
        if (zoomTimeLeft > 0)
        {
            zoomTimeLeft--;
            
            // Keep zoom factor at 1 during the entire transition
            targetZoomFactor = 1f;
            
            // Check if we should stop following
            if (zoomTimeLeft <= 0)
            {
                followingBossIndex = -1;
                targetZoomFactor = 0f;
            }
        }
        else
        {
            targetZoomFactor = 0f;
            followingBossIndex = -1;
        }
        
        zoomFactor = MathHelper.Lerp(zoomFactor, targetZoomFactor, 0.1f);
        
        if (MathHelper.Distance(zoomFactor, targetZoomFactor) < 0.01f)
        {
            zoomFactor = targetZoomFactor;
        }
    }
    
    public static void TriggerBossZoom(Vector2 bossPosition, int bossIndex = -1)
    {
        targetPosition = bossPosition;
        zoomTimeLeft = ZOOM_DURATION;
        followingBossIndex = bossIndex; // Store which boss to follow
    }
}