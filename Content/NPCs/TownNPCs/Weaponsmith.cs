using Microsoft.Xna.Framework;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Bestiary;

namespace Ultraconyx.Content.NPCs.TownNPCs;

[AutoloadHead]
public class Weaponsmith : ModNPC
{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 16;

        NPCID.Sets.ExtraFramesCount[Type] = 9;
        NPCID.Sets.AttackFrameCount[Type] = 4;
        NPCID.Sets.DangerDetectRange[Type] = 700;
        NPCID.Sets.AttackType[Type] = 0;
        NPCID.Sets.AttackTime[Type] = 90;
        NPCID.Sets.AttackAverageChance[Type] = 30;
        NPCID.Sets.HatOffsetY[Type] = 4;
        NPCID.Sets.FaceEmote[Type] = 87;

        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
        {
            Velocity = 1f,
            Direction = -1
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
    }

    public override void SetDefaults()
    {
        NPC.width = 18;
        NPC.height = 40;

        NPC.lifeMax = 250;
        NPC.damage = 20;
        NPC.defense = 15;
        NPC.knockBackResist = 0.5f;

        NPC.aiStyle = NPCAIStyleID.Passive;
        AnimationType = NPCID.Guide;

        NPC.townNPC = true;
        NPC.friendly = true;

        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange([
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
            new FlavorTextBestiaryInfoElement("Mods.Ultraconyx.Bestiary.Weaponsmith"),
        ]);
    }

    // TODO: Add gore textures for this town NPC.
    //public override void HitEffect(NPC.HitInfo hit)
    //{
    //}

    // Make this town NPC teleport to a King or Queen Statue when triggered.
    public override bool CanGoToStatue(bool toKingStatue) => true;

    public override bool CanTownNPCSpawn(int numTownNPCs) => NPC.downedBoss1;

    public override List<string> SetNPCNameList()
    {
        return
        [
            "Gunther",
            "Smithson",
            "Forge",
            "Anvil",
            "Hephaestus",
            "Vulcan",
            "Wayland",
            "Bjorn",
            "Takeda",
            "Mikhail"
        ];
    }

    public override string GetChat()
    {
        Player player = Main.LocalPlayer;

        bool firstTime = Main.time < 60 && !NPC.homeless && NPC.homeTileX >= 0 && NPC.homeTileY >= 0;
        if (firstTime)
            return "Hey " + player.name + ", it's nice meeting you!";

        List<string> dialogue =
        [
            "Heya, what do you brought for me this time?",
            "Honestly, where do you even find these?",
            "Got something interesting for me to work on?",
            "Every weapon tells a story. Let me see what you've got.",
            "I can turn your old gear into something special.",
            "The right weapon can make all the difference.",
            "I've been working on some new designs lately.",
            "Trade in your old weapons for something better!"
        ];
        
        return Main.rand.Next(dialogue);
    }

    public override void SetChatButtons(ref string button, ref string button2)
    {
        button = "Exchange";
        // Hide the other button.
        button2 = "";
    }

    public override void OnChatButtonClicked(bool firstButton, ref string shopName)
    {
        if (firstButton)
        {
            Player player = Main.LocalPlayer;
            
            // Check what item the player has selected in hotbar.
            Item selectedItem = player.HeldItem;
            if (selectedItem.type == ItemID.None)
                Main.npcChatText = "You're empty-handed.";

            // Check if the player has a Musket.
            else if (selectedItem.type == ItemID.Musket)
            {
                bool foundMusket = false;
                for (int i = 0; i < player.inventory.Length; i++)
                {
                    if (player.inventory[i].type == ItemID.Musket && player.inventory[i].stack > 0)
                    {
                        foundMusket = true;
                        
                        player.inventory[i].stack--;
                        if (player.inventory[i].stack <= 0)
                            player.inventory[i] = new Item();

                        break;
                    }
                }

                if (foundMusket)
                {
                    // Give a Blunderbuss.
                    int blunderbussType = ModContent.ItemType<Items.Weapons.Ranger.Blunderbuss>();
                    player.QuickSpawnItem(null, blunderbussType, 1);
                    
                    Main.NewText("Exchanged Musket for Blunderbuss!", Color.Orange);
                    Main.npcChatText = "There you go! A brand new Blunderbuss!";
                    Recipe.FindRecipes();
                }
                else
                    Main.npcChatText = "You need to have a Musket in your inventory.";
            }
            else if (selectedItem.type == ItemID.PhoenixBlaster)
            {
                // Receive the Phoenix Blaster from the player's inventory.
                bool foundPhoenix = false;
                for (int i = 0; i < player.inventory.Length; i++)
                {
                    if (player.inventory[i].type == ItemID.PhoenixBlaster && player.inventory[i].stack > 0)
                    {
                        foundPhoenix = true;

                        player.inventory[i].stack--;
                        if (player.inventory[i].stack <= 0)
                            player.inventory[i] = new Item();

                        break;
                    }
                }
                
                if (foundPhoenix)
                {
                    // Give the Doomslinger.
                    int doomslingerType = ModContent.ItemType<Items.Weapons.Ranger.Doomslinger>();
                    player.QuickSpawnItem(null, doomslingerType, 1);
                    
                    Main.NewText("Exchanged Phoenix Blaster for Doomslinger!", Color.Orange);
                    Main.npcChatText = "There you go! A brand new Doomslinger!";
                    Recipe.FindRecipes();
                }
                else
                    Main.npcChatText = "You need to have a Phoenix Blaster in your inventory.";
            }
            else
            {
                // Check if it's a weapon.
                bool isWeapon = selectedItem.damage > 0 && selectedItem.DamageType != DamageClass.Summon;
                if (isWeapon)
                    Main.npcChatText = "That weapon is too advanced for me to work with.";
                else
                    Main.npcChatText = "Sorry, but I can't work with that.";
            }
        }
    }

    public override void FindFrame(int frameHeight)
    {
        if (NPC.velocity.X == 0 && NPC.velocity.Y == 0)
        {
            NPC.frame.Y = 0;
            NPC.frameCounter = 0;
        }
        else
        {
            NPC.frameCounter++;
            if (NPC.frameCounter >= 8.0)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y < 3 * frameHeight || NPC.frame.Y >= 16 * frameHeight)
                    NPC.frame.Y = 3 * frameHeight;
            }
        }
    }
}
