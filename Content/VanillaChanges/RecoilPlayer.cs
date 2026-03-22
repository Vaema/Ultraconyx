using Terraria;
using Terraria.ModLoader;

namespace Ultraconyx.Content.VanillaChanges;

public class RecoilPlayer : ModPlayer
{
    // Add slight screen shake for stronger guns
    public override void ModifyScreenPosition()
    {
        var heldItem = Player.HeldItem;
        if (!heldItem.IsAir && heldItem.active)
        {
            var globalItem = heldItem.GetGlobalItem<RecoilGlobalItem>();
            if (globalItem != null && globalItem.IsGun(heldItem)) // Fixed: Use IsGun method
            {
                if (Player.itemAnimation > 0 && Player.itemAnimation == Player.itemAnimationMax - 1)
                {
                    float recoilStrength = RecoilGlobalItem.GetRecoilStrength(heldItem.type);

                    // Stronger guns cause more screen shake
                    if (recoilStrength > 1.2f)
                    {
                        Main.screenPosition += new Microsoft.Xna.Framework.Vector2(
                            Main.rand.NextFloat(-1f, 1f) * recoilStrength * 0.5f,
                            Main.rand.NextFloat(-0.5f, 0.5f) * recoilStrength * 0.5f
                        );
                    }
                }
            }
        }
    }
}