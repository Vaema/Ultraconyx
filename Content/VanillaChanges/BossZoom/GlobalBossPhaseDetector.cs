using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;

namespace Ultraconyx.Content.VanillaChanges.BossZoom;

public class GlobalBossPhaseDetector : GlobalNPC
{
    private Dictionary<int, int> lastLife = [];
    private Dictionary<int, float> lastHealthPercent = [];
    private Dictionary<int, bool> hasTriggeredPhase2 = [];
    private Dictionary<int, bool> hasTriggeredPhase3 = [];

    public override bool InstancePerEntity => true;

    public override void AI(NPC npc)
    {
        if (!npc.active || !npc.boss)
            return;

        int npcIndex = npc.whoAmI;

        // Initialize tracking
        if (!lastLife.ContainsKey(npcIndex))
        {
            lastLife[npcIndex] = npc.life;
            lastHealthPercent[npcIndex] = (float)npc.life / npc.lifeMax;
            hasTriggeredPhase2[npcIndex] = false;
            hasTriggeredPhase3[npcIndex] = false;
            return;
        }

        float currentHealthPercent = (float)npc.life / npc.lifeMax;
        float previousHealthPercent = lastHealthPercent[npcIndex];

        // Check for phase transitions based on boss type
        switch (npc.type)
        {
            case NPCID.EyeofCthulhu:
                CheckPhaseTransition(npc, npcIndex, currentHealthPercent, previousHealthPercent, 0.5f, "PHASE 2!", ref hasTriggeredPhase2);
                break;

            case NPCID.SkeletronHead:
            case NPCID.SkeletronHand:
                CheckPhaseTransition(npc, npcIndex, currentHealthPercent, previousHealthPercent, 0.75f, "PHASE 2!", ref hasTriggeredPhase2);
                CheckPhaseTransition(npc, npcIndex, currentHealthPercent, previousHealthPercent, 0.25f, "FINAL PHASE!", ref hasTriggeredPhase3);
                break;

            case NPCID.WallofFlesh:
            case NPCID.WallofFleshEye:
                CheckPhaseTransition(npc, npcIndex, currentHealthPercent, previousHealthPercent, 0.5f, "PHASE 2!", ref hasTriggeredPhase2);
                break;

            case NPCID.Retinazer:
            case NPCID.Spazmatism:
                CheckPhaseTransition(npc, npcIndex, currentHealthPercent, previousHealthPercent, 0.5f, "PHASE 2!", ref hasTriggeredPhase2);
                break;

            case NPCID.TheDestroyer:
            case NPCID.TheDestroyerBody:
            case NPCID.TheDestroyerTail:
                CheckPhaseTransition(npc, npcIndex, currentHealthPercent, previousHealthPercent, 0.5f, "PHASE 2!", ref hasTriggeredPhase2);
                break;

            case NPCID.SkeletronPrime:
                CheckPhaseTransition(npc, npcIndex, currentHealthPercent, previousHealthPercent, 0.5f, "PHASE 2!", ref hasTriggeredPhase2);
                break;

            case NPCID.Plantera:
            case NPCID.PlanterasHook:
            case NPCID.PlanterasTentacle:
                CheckPhaseTransition(npc, npcIndex, currentHealthPercent, previousHealthPercent, 0.5f, "PHASE 2!", ref hasTriggeredPhase2);
                break;

            case NPCID.Golem:
            case NPCID.GolemFistLeft:
            case NPCID.GolemFistRight:
            case NPCID.GolemHead:
                CheckPhaseTransition(npc, npcIndex, currentHealthPercent, previousHealthPercent, 0.5f, "PHASE 2!", ref hasTriggeredPhase2);
                break;

            case NPCID.DukeFishron:
                CheckPhaseTransition(npc, npcIndex, currentHealthPercent, previousHealthPercent, 0.5f, "PHASE 2!", ref hasTriggeredPhase2);
                CheckPhaseTransition(npc, npcIndex, currentHealthPercent, previousHealthPercent, 0.25f, "FINAL PHASE!", ref hasTriggeredPhase3);
                break;

            case NPCID.CultistBoss:
                CheckPhaseTransition(npc, npcIndex, currentHealthPercent, previousHealthPercent, 0.5f, "PHASE 2!", ref hasTriggeredPhase2);
                break;

            case NPCID.MoonLordCore:
            case NPCID.MoonLordHand:
            case NPCID.MoonLordHead:
                CheckPhaseTransition(npc, npcIndex, currentHealthPercent, previousHealthPercent, 0.66f, "PHASE 2!", ref hasTriggeredPhase2);
                CheckPhaseTransition(npc, npcIndex, currentHealthPercent, previousHealthPercent, 0.33f, "FINAL PHASE!", ref hasTriggeredPhase3);
                break;

            default:
                // Default 2-phase boss at 50% health
                CheckPhaseTransition(npc, npcIndex, currentHealthPercent, previousHealthPercent, 0.5f, "PHASE 2!", ref hasTriggeredPhase2);
                break;
        }

        // Update tracking
        lastLife[npcIndex] = npc.life;
        lastHealthPercent[npcIndex] = currentHealthPercent;
    }

    private void CheckPhaseTransition(NPC npc, int npcIndex, float currentPercent, float previousPercent,
        float threshold, string phaseText, ref Dictionary<int, bool> triggerDict)
    {
        // Check if we crossed the threshold from above to below
        if (previousPercent > threshold && currentPercent <= threshold && !triggerDict[npcIndex])
        {
            triggerDict[npcIndex] = true;

            // Pass the boss index so the camera can follow it
            BossZoomSystem.TriggerBossZoom(npc.Center, npc.whoAmI);

            // Add visual feedback
            CombatText.NewText(npc.Hitbox, Color.Yellow, phaseText, true);

            // Spawn some dust effects
            for (int i = 0; i < 20; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(3f, 3f);
                Dust.NewDustPerfect(npc.Center, DustID.GemDiamond, speed, 100, Color.White, 1.5f);
            }
        }
    }

    public override void OnKill(NPC npc)
    {
        // Clean up tracking when boss dies
        if (npc.boss)
        {
            int npcIndex = npc.whoAmI;
            lastLife.Remove(npcIndex);
            lastHealthPercent.Remove(npcIndex);
            hasTriggeredPhase2.Remove(npcIndex);
            hasTriggeredPhase3.Remove(npcIndex);
        }
    }
}