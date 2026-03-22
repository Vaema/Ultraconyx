using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Features;

public class FloatingItemSystem : ModSystem
{
    public override void PostUpdateItems()
    {
        for (int i = 0; i < Main.maxItems; i++)
        {
            Item item = Main.item[i];
            if (item.active && item.type > ItemID.None && item.TryGetGlobalItem<FloatingItemData>(out var floatingData))
            {
                if (floatingData.IsFloating)
                {
                    // Update orbit angle
                    floatingData.OrbitAngle += floatingData.OrbitSpeed * 0.01f;
                    
                    // Calculate orbit position
                    float orbitX = (float)Math.Cos(floatingData.OrbitAngle) * floatingData.OrbitRadius;
                    float orbitY = (float)Math.Sin(floatingData.OrbitAngle) * floatingData.OrbitRadius;
                    
                    // Add gentle bobbing
                    floatingData.FloatTimer += 0.05f;
                    float bobY = (float)Math.Sin(floatingData.FloatTimer) * 5f;
                    
                    // Set item position
                    item.position = floatingData.ChestCenter + new Vector2(orbitX, orbitY + bobY) - new Vector2(item.width / 2, item.height / 2);
                    
                    // PREVENT AUTOMATIC PICKUP
                    item.beingGrabbed = false;
                    item.velocity = Vector2.Zero;
                    item.noGrabDelay = 9999;
                    
                    // Also set these properties to prevent pickup
                    item.ownIgnore = Player.FindClosest(item.position, item.width, item.height);
                    item.ownTime = 100;
                }
            }
        }
    }
}