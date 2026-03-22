using System.IO;
using Terraria;
using Terraria.ModLoader;
using Ultraconyx.Content.UI.ChaosMode;

namespace Ultraconyx;

public class Ultraconyx : Mod
{
    // Message types for multiplayer syncing.
    public enum MessageType : byte
    {
        ChaosModeActivated
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        MessageType msgType = (MessageType)reader.ReadByte();

        switch (msgType)
        {
            case MessageType.ChaosModeActivated:
                bool isActive = reader.ReadBoolean();
                ChaosModeUI.SetChaosMode(isActive);

                if (Main.netMode == Terraria.ID.NetmodeID.Server)
                {
                    // Forward to other clients.
                    ModPacket packet = GetPacket();
                    packet.Write((byte)MessageType.ChaosModeActivated);
                    packet.Write(isActive);
                    packet.Send(-1, whoAmI);
                }
                break;
        }
    }
}