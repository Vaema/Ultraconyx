using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Dusts;

public class Stardust : ModDust
{
    public override void OnSpawn(Dust dust)
    {
        dust.velocity *= 0.4f;
        dust.noGravity = true;
        dust.noLight = false;
        dust.scale *= 1.2f;
    }

    public override bool Update(Dust dust)
    {
        dust.position += dust.velocity;
        dust.rotation += dust.velocity.X * 0.15f;
        dust.scale *= 0.98f;

        float light = 0.3f * dust.scale;
        Lighting.AddLight(dust.position, 0.2f * light, 0.3f * light, 0.8f * light);

        if (dust.scale < 0.3f)
        {
            dust.active = false;
        }

        return false;
    }

    public override Color? GetAlpha(Dust dust, Color lightColor)
    {
        return new Color(255, 255, 255, 100) * (dust.scale / 1.2f);
    }
}