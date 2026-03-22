using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Dusts;

public class Spookay : ModDust
{
    public override void OnSpawn(Dust dust)
    {
        dust.velocity *= 0.4f;
        dust.noGravity = true;
        dust.noLight = false;
        dust.scale *= 1.5f;
        dust.color = default; // Reset color to use texture's colors
        dust.alpha = 50; // Slight transparency
    }

    public override bool Update(Dust dust)
    {
        dust.position += dust.velocity;
        dust.rotation += dust.velocity.X * 0.1f;
        dust.scale -= 0.02f;

        float lightStrength = dust.scale * 0.5f;
        Lighting.AddLight(dust.position, lightStrength * 0.5f, lightStrength * 0.5f, lightStrength * 1f);

        if (dust.scale < 0.3f)
        {
            dust.active = false;
        }

        return false;
    }

    public override bool PreDraw(Dust dust)
    {
        // Custom drawing if you need more control
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = ModContent.Request<Texture2D>("Ultracronyx/Content/Dusts/Spookay").Value;

        Rectangle frame = texture.Frame(1, 1, 0, 0);
        Vector2 origin = frame.Size() / 2f;

        Color color = dust.color;

        // Time-based color variation
        if (Main.dayTime)
            color = Color.Lerp(Color.White, Color.LightBlue, 0.7f) * ((255 - dust.alpha) / 255f);
        else
            color = Color.Lerp(Color.Purple, Color.Blue, 0.5f) * ((255 - dust.alpha) / 255f);

        spriteBatch.Draw(texture, dust.position - Main.screenPosition, frame, color, dust.rotation, origin, dust.scale, SpriteEffects.None, 0f);

        return false;
    }
}