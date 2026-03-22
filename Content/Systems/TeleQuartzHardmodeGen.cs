using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using System.IO;
using Ultraconyx.Content.Tiles;

namespace Ultraconyx.Content.Systems;

public class TeleQuartzHardmodeGen : ModSystem
{
    private static bool generatedTeleQuartz;

    public override void OnWorldLoad()
    {
        generatedTeleQuartz = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["generatedTeleQuartz"] = generatedTeleQuartz;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        generatedTeleQuartz = tag.GetBool("generatedTeleQuartz");
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(generatedTeleQuartz);
    }

    public override void NetReceive(BinaryReader reader)
    {
        generatedTeleQuartz = reader.ReadBoolean();
    }

    public override void PostUpdateWorld()
    {
        // Only generate on server or in singleplayer
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (Main.hardMode && !generatedTeleQuartz)
        {
            GenerateTeleQuartzHardmode();
            generatedTeleQuartz = true;

            // Broadcast message appropriately
            if (Main.netMode == NetmodeID.Server)
            {
                // Server sends network message to all clients
                Terraria.Chat.ChatHelper.BroadcastChatMessage(
                    Terraria.Localization.NetworkText.FromLiteral("Your world has been blessed with TeleQuartz"),
                    new Color(100, 200, 255)
                );
                
                // Sync world data to all clients
                NetMessage.SendData(MessageID.WorldData);
            }
            else
            {
                // Singleplayer message
                Main.NewText("Your world has been blessed with TeleQuartz", 100, 200, 255);
            }
        }
    }

    private void GenerateTeleQuartzHardmode()
    {
        int oreCount = (int)(Main.maxTilesX * Main.maxTilesY * 0.00012);

        for (int i = 0; i < oreCount; i++)
        {
            int x = WorldGen.genRand.Next(0, Main.maxTilesX);
            int y = WorldGen.genRand.Next((int)Main.rockLayer, Main.maxTilesY - 150);

            WorldGen.TileRunner(
                x,
                y,
                WorldGen.genRand.Next(4, 7),
                WorldGen.genRand.Next(5, 12),
                ModContent.TileType<TeleQuartzOre>()
            );
        }
    }
}