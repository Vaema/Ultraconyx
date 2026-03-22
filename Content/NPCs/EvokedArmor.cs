using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;

namespace Ultraconyx.Content.NPCs;

public class EvokedArmor : ModNPC
{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 18;
    }

    public override void SetDefaults()
    {
        NPC.width = 28;
        NPC.height = 44;

        NPC.damage = 45;
        NPC.defense = 20;
        NPC.lifeMax = 500;

        NPC.knockBackResist = 0.3f;
        NPC.value = Item.buyPrice(gold: 14, silver: 59);

        NPC.aiStyle = 3;
        AIType = NPCID.GoblinWarrior;
        AnimationType = -1;

        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;

        NPC.buffImmune[BuffID.Confused] = true;
    }

    public override void AI()
    {
        // Fix facing direction
        NPC.spriteDirection = NPC.direction;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        // Rare Hardmode Goblin Invasion enemy
        if (Main.hardMode && Main.invasionType == InvasionID.GoblinArmy)
            return 0.04f;

        return 0f;
    }

    public override void FindFrame(int frameHeight)
    {
        // Jumping frames: 0–2
        if (NPC.velocity.Y != 0)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter >= 6)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y >= frameHeight * 3)
                    NPC.frame.Y = 0;
            }
            return;
        }

        // Walking frames: 3–17
        NPC.frameCounter++;
        if (NPC.frameCounter >= 6)
        {
            NPC.frameCounter = 0;
            NPC.frame.Y += frameHeight;

            if (NPC.frame.Y < frameHeight * 3 || NPC.frame.Y >= frameHeight * 18)
                NPC.frame.Y = frameHeight * 3;
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 14, 14));
        npcLoot.Add(ItemDropRule.Common(ItemID.SilverCoin, 1, 59, 59));
    }
}
