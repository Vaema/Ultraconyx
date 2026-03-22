using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Ultraconyx.Content.UI.ChaosMode.EoC;

namespace Ultraconyx.Content.UI.ChaosMode;

public class ChaosModePlayer : ModPlayer
{
    // Track if Eye of Cthulhu was just summoned
    private bool eyeOfCthulhuWasActive;

    public override void PreUpdate()
    {
        if (ChaosModeUI.IsChaosModeActive())
        {
            // Add your chaos mode effects here
        }

        // Update the typing animation in the system
        ChaosModeBossNameSystem.UpdateTyping();

        // Update the display timer in the system
        if (ChaosModeBossNameSystem.DisplayTimer > 0)
        {
            ChaosModeBossNameSystem.DisplayTimer--;
        }

        // Check if Eye of Cthulhu was just summoned
        bool eyeIsActive = NPC.AnyNPCs(NPCID.EyeofCthulhu);

        if (ChaosModeUI.IsChaosModeActive() && eyeIsActive && !eyeOfCthulhuWasActive && !EoCIntroSystem.IsPlayingIntro())
        {
            // Eye of Cthulhu was just summoned! Start the cinematic intro
            ChaosModeBossNameSystem.StartTyping("Eye of Cthulhu");
            ChaosModeCameraSystem.StartFollowing();
            EoCIntroSystem.StartIntro();
        }

        eyeOfCthulhuWasActive = eyeIsActive;
    }

    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
    {
        // Check if the player died while typing a boss name
        if (ChaosModeBossNameSystem.IsTyping)
        {
            ChaosModeBossNameSystem.CurrentBossName = "";
            ChaosModeBossNameSystem.DisplayTimer = 0;
            ChaosModeBossNameSystem.IsTyping = false;
        }
        return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);
    }

    public override void OnEnterWorld()
    {
        // Sync chaos mode state when player joins
        if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
        {
            ModPacket packet = ModContent.GetInstance<Ultraconyx>().GetPacket();
            packet.Write((byte)Ultraconyx.MessageType.ChaosModeActivated);
            packet.Write(ChaosModeUI.IsChaosModeActive());
            packet.Send();
        }
    }
}