using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Tiles;

public class AbysslightRocksTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileLighted[Type] = false;

        // Make it completely unbreakable
        Main.tileNoAttach[Type] = true;
        Main.tileNoFail[Type] = true;

        // Blend with stone
        TileID.Sets.Stone[Type] = true;
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;

        // Unbreakable properties
        MinPick = 9999; // Impossible to mine
        MineResist = 9999f; // Extremely resistant
        TileID.Sets.Ore[Type] = false;

        AddMapEntry(new Color(80, 80, 100), Language.GetText("Abysslight Rocks"));

        // No item drop - this is a worldgen-only tile
        // Don't register an item drop
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        // Subtle blue glow
        r = 0.1f;
        g = 0.2f;
        b = 0.3f;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override bool CanExplode(int i, int j)
    {
        return false;
    }

    public override bool CanKillTile(int i, int j, ref bool blockDamaged)
    {
        blockDamaged = false;
        return false;
    }

    public override bool CanPlace(int i, int j)
    {
        return false; // Can't be placed by players
    }

    public override bool CanReplace(int i, int j, int tileTypeBeingPlaced)
    {
        return false;
    }
}

// No item class needed since players shouldn't place it