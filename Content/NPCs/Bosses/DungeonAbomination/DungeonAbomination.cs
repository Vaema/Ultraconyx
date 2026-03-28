using System;
using System.IO;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.NPCs.Bosses.DungeonAbomination;

public class DungeonAbomination : ModNPC
{
    // TODO: This needs to be finished.
    // Refer to the document regarding the Dungeon Abomination (still needs work):
    // https://docs.google.com/document/d/1cZnO1Fa-YxzchrKpw9CSc_P0lHL0OgyZ1wMnQsBYIFk/edit?usp=sharing

    public enum State
    {
        SummonAnimation,

        // Phase one.
        PerformHandBehaviors,

        // Phase two.
        EnterPhase2
    }

    public State CurrentState
    {
        get => (State)(int)NPC.ai[0];
        set => NPC.ai[0] = (int)value;
    }

    public ref float AttackTimer => ref NPC.ai[1];

    public Player Target => Main.player[NPC.target];

    public override void SetStaticDefaults()
    {
        NPCID.Sets.TrailCacheLength[Type] = 5;
        NPCID.Sets.TrailingMode[Type] = 2;
        NPCID.Sets.MPAllowedEnemies[Type] = true;
        NPCID.Sets.BossBestiaryPriority.Add(Type);
    }

    public override void SetDefaults()
    {
        NPC.width = NPC.height = 120;

        NPC.lifeMax = 650000;
        NPC.damage = 300;
        NPC.defense = 100;
        NPC.knockBackResist = 0f;
        NPC.npcSlots = 25f;

        NPC.aiStyle = -1;
        AIType = -1;

        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.netAlways = true;
        NPC.value = Item.buyPrice(platinum: 1);

        NPC.HitSound = SoundID.NPCHit2;
        NPC.DeathSound = SoundID.NPCDeath2;
        Music = MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/ChaosBoss1");
        SceneEffectPriority = SceneEffectPriority.BossHigh;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange(
        [
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheDungeon,
            new FlavorTextBestiaryInfoElement("Mods.Ultraconyx.Bestiary.DungeonAbomination")
        ]);
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(NPC.Opacity);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        NPC.Opacity = reader.ReadSingle();
    }

    public override void AI()
    {
        // Find the nearest target.
        NPC.TargetClosest();

        // Despawn if all remaining targets are dead.
        if (Target.dead || !Target.active)
        {
            // Despawn by vanishing.
            NPC.Opacity--;
            if (NPC.Opacity <= 0f)
            {
                NPC.active = false;
                NPC.netUpdate = true;
            }

            return;
        }

        // Switch between states.
        switch (CurrentState)
        {
            case State.SummonAnimation:
                SummonAnimation();
                break;
        }

        // Increment the attack timer.
        AttackTimer++;
    }

    public void SummonAnimation()
    {
        int animationTime = 180;
        bool canHover = AttackTimer < animationTime;

        // Do not deal damage during the summon animation.
        NPC.damage = 0;

        // Ensure to hover over the player.
        if (canHover)
        {
            Vector2 hoverDestination = Target.Center - Vector2.UnitY * 500f;

            NPC.velocity = NPC.SafeDirectionTo(hoverDestination) * MathF.Min(NPC.Distance(hoverDestination), 35f);
            NPC.rotation = NPC.rotation.AngleLerp(NPC.velocity.X * 0.05f, 0.1f);

            if (NPC.WithinRange(Target.Center, 90f))
            {
                NPC.Center = Target.Center - NPC.SafeDirectionTo(Target.Center, Vector2.UnitY) * 90f;
                NPC.netUpdate = true;
            }
        }
    }

    public override bool CheckActive() => false;
}
