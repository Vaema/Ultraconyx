using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Tiles;

public class TeleQuartzOre : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileSpelunker[Type] = true;
        Main.tileOreFinderPriority[Type] = 410; // like Adamantite value
        Main.tileShine[Type] = 975; 
        Main.tileShine2[Type] = true;
        Main.tileOreFinderPriority[Type] = 700; 
        Main.tileLighted[Type] = true;
        Main.tileBlockLight[Type] = true;

        HitSound = SoundID.Tink;
        DustType = DustID.BlueCrystalShard;
        MinPick = 200; // pickaxe power required to mine
        MineResist = 4.5f;

        AddMapEntry(new Color(80, 150, 255), CreateMapEntryName());
    }

    public override bool CanExplode(int i, int j)
    {
        return true; // or false if you want "Demonite-style" explosion proof
    }
}
