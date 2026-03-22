using Terraria;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Projectiles;

public class Apple : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 180;
    }
}