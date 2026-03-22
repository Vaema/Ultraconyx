using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Ultraconyx.Content.Items.Materials.ArcadiumBar;

public class ArcadiumBarTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileShine[Type] = 1100;
        Main.tileSolid[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileWaterDeath[Type] = false;

        TileID.Sets.Ore[Type] = false;
        TileID.Sets.BlocksStairs[Type] = true;
        TileID.Sets.BlocksStairsAbove[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        AddMapEntry(new Color(100, 200, 255), Language.GetText("MapObject.Bar"));

        // 1x1 tile placement (16x16)
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.WaterDeath = false;
        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.DrawYOffset = 0;
        TileObjectData.addTile(Type);

        DustType = DustID.Electric;
        HitSound = SoundID.Tink;
        MineResist = 2f;
        MinPick = 50;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override bool CanExplode(int i, int j)
    {
        return false;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        // Only drop if not failed and not just effect only
        if (!fail && !effectOnly)
        {
            // Set noItem to true to prevent default item drop
            noItem = true;

            // Manually drop the item
            int item = Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<ArcadiumBar>());

            // Sync in multiplayer
            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
        }
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        Tile tile = Main.tile[i, j];

        // Check if the tile below is air (floating)
        if (!Main.tile[i, j + 1].HasTile)
        {
            // Drip PinkStarfury dust
            if (Main.rand.NextBool(3))
            {
                int dust = Dust.NewDust(
                    new Vector2(i * 16 + Main.rand.Next(-4, 20), j * 16 + 16),
                    8, 8,
                    DustID.PurpleTorch,
                    0f, Main.rand.Next(1, 3),
                    50,
                    new Color(255, 120, 255),
                    0.8f
                );
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.3f;
            }
        }

        // Simplified frame logic - always use frame 0
        tile.TileFrameX = 0;
        tile.TileFrameY = 0;
        return false;
    }

    public override void RandomUpdate(int i, int j)
    {
        if (Main.rand.NextBool(10))
        {
            Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, DustType, 0f, 0f, 50, default(Color), 0.5f);
        }
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        // Optional: Uncomment to add light emission
        // r = 0.1f;
        // g = 0.2f;
        // b = 0.3f;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        // Draw glow effect on tile
        Tile tile = Main.tile[i, j];

        Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
        Vector2 position = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero;

        // Load and draw glow texture if it exists
        if (ModContent.RequestIfExists<Texture2D>("Ultracronyx/Content/Items/Materials/ArcadiumBar/ArcadiumBarTile_Glow", out var glowTexture))
        {
            spriteBatch.Draw(
                glowTexture.Value,
                position,
                new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16),
                Color.White * 0.5f,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0f
            );
        }
    }

    public override bool IsTileDangerous(int i, int j, Player player)
    {
        return false;
    }
}