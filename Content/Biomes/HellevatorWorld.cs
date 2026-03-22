using System.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Ultraconyx.Content.Biomes;

public class HellevatorWorld : ModSystem
{
    public override void OnWorldLoad()
    {
        HellevatorGen.BiomeExists = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        if (HellevatorGen.BiomeExists)
        {
            tag["OvalBiomeExists"] = true;
            tag["BiomeCenterX"] = HellevatorGen.BiomeCenter.X;
            tag["BiomeCenterY"] = HellevatorGen.BiomeCenter.Y;
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        if (tag.ContainsKey("OvalBiomeExists"))
        {
            HellevatorGen.BiomeExists = tag.GetBool("OvalBiomeExists");
            int x = tag.GetInt("BiomeCenterX");
            int y = tag.GetInt("BiomeCenterY");
            HellevatorGen.BiomeCenter = new Microsoft.Xna.Framework.Point(x, y);
        }
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(HellevatorGen.BiomeExists);
        writer.Write(HellevatorGen.BiomeCenter.X);
        writer.Write(HellevatorGen.BiomeCenter.Y);
    }

    public override void NetReceive(BinaryReader reader)
    {
        HellevatorGen.BiomeExists = reader.ReadBoolean();
        int x = reader.ReadInt32();
        int y = reader.ReadInt32();
        HellevatorGen.BiomeCenter = new Microsoft.Xna.Framework.Point(x, y);
    }
}