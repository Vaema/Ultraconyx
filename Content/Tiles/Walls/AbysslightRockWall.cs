using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Tiles.Walls;

public class AbysslightRockWall : ModWall
{
    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = false; // Not valid for housing
        AddMapEntry(new Color(120, 40, 40)); // Dark red color
        
        // Optional: Set dust type that appears when breaking
        DustType = DustID.Stone;
        
        // Optional: Set sound when hit
        HitSound = SoundID.Tink;
    }
    
    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
    
    // Optional: Modify lighting if you want the wall to glow
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        // Uncomment to make the wall glow faintly red
        // r = 0.3f;
        // g = 0.1f;
        // b = 0.1f;
    }
}