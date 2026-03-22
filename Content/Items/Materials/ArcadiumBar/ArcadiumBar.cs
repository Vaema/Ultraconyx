using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using System;
using Ultraconyx.Content.Rarities;

namespace Ultraconyx.Content.Items.Materials.ArcadiumBar;

public class ArcadiumBar : ModItem
{
    private Asset<Texture2D> glowTexture;

    public override void SetStaticDefaults()
    {
        ItemID.Sets.IgnoresEncumberingStone[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 24;
        Item.maxStack = 9999;
        Item.value = Item.sellPrice(silver: 10);

        // Set custom rarity
        Item.rare = ModContent.RarityType<PostPraetorian>();

        Item.material = true;
        Item.autoReuse = true;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<ArcadiumBarTile>();
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        // Draw pulsing glow behind the item in world
        if (glowTexture == null)
            glowTexture = ModContent.Request<Texture2D>("Ultracronyx/Content/Items/Materials/ArcadiumBar/ArcadiumBar_Glow");

        float pulseScale = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 1.0f;

        Vector2 origin = glowTexture.Size() * 0.5f;
        Vector2 position = new(
            Item.position.X - Main.screenPosition.X + Item.width * 0.5f,
            Item.position.Y - Main.screenPosition.Y + Item.height * 0.5f
        );

        spriteBatch.Draw(
            glowTexture.Value,
            position,
            null,
            Color.White * 0.5f,
            rotation,
            origin,
            scale * pulseScale,
            SpriteEffects.None,
            0f
        );

        return true;
    }

    // No PreDrawInInventory override = no inventory glow

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.AdamantiteBar, 1);
        recipe.AddIngredient(ItemID.SoulofLight, 2);
        recipe.AddTile(TileID.AdamantiteForge);
        recipe.Register();
    }
}