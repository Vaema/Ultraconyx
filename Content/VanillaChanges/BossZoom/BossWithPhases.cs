using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace Ultraconyx.Content.VanillaChanges.BossZoom;

public abstract class BossWithPhases : ModNPC
{
    protected int currentPhase = 1;
    protected bool justChangedPhase;

    public override void AI()
    {
        // Reset phase change flag at start of AI
        justChangedPhase = false;

        // Your boss AI here

        // After AI, handle phase change detection
        if (justChangedPhase)
        {
            // Trigger the camera zoom and follow this boss
            BossZoomSystem.TriggerBossZoom(NPC.Center, NPC.whoAmI);

            // Optional: Add visual effects
            SpawnPhaseTransitionEffects();
        }
    }

    protected virtual void SpawnPhaseTransitionEffects()
    {
        // Override this to add custom effects like dusts or lights
        for (int i = 0; i < 20; i++)
        {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemDiamond, 0f, 0f, 100, Color.White, 2f);
        }
    }

    // Helper method to change phase
    protected void ChangePhase(int newPhase)
    {
        if (currentPhase != newPhase)
        {
            currentPhase = newPhase;
            justChangedPhase = true;

            // Call your phase change logic here
            OnPhaseChange(newPhase);
        }
    }

    protected virtual void OnPhaseChange(int newPhase)
    {
        // Override this for phase-specific behavior
    }
}