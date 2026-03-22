using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Common.Globals.Items;

public class FlyingGlobalItem : GlobalItem
{
    public override bool InstancePerEntity => true;

    public bool IsFlyingToPlayer { get; set; }
    public int FlyTimer { get; set; }
    public int PlayerTarget { get; set; }

    public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
    {
        // Handle items flying to player
        if (IsFlyingToPlayer && PlayerTarget >= 0 && PlayerTarget < Main.maxPlayers)
        {
            Player targetPlayer = Main.player[PlayerTarget];
            if (targetPlayer != null && targetPlayer.active)
            {
                FlyTimer++;

                // Fly toward player for 30 frames (0.5 seconds)
                if (FlyTimer < 30)
                {
                    Vector2 direction = targetPlayer.Center - item.Center;
                    direction.Normalize();
                    item.velocity = direction * 12f; // Fast flight
                    gravity = 0f; // No gravity while flying
                    maxFallSpeed = 0f;

                    // Sparkle trail effect
                    if (Main.rand.NextBool(3))
                    {
                        Dust dust = Dust.NewDustPerfect(item.Center,
                            DustID.GoldCoin,
                            item.velocity * 0.5f,
                            0,
                            default,
                            0.7f);
                        dust.noGravity = true;
                    }
                }
                else
                {
                    // After flying, let Terraria handle normal pickup
                    IsFlyingToPlayer = false;
                    item.noGrabDelay = 0;
                    item.beingGrabbed = true;
                }
            }
        }
    }
}
