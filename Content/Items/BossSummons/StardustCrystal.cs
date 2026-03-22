using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Ultraconyx.Content.NPCs.Bosses;

namespace Ultraconyx.Content.Items.BossSummons;

public class StardustCrystal : ModItem
{
    public override void SetStaticDefaults()
    {
        // DisplayName.SetDefault("Stardust Crystal");
        /* Tooltip.SetDefault("Summons the Stardust Angel\n" +
                         "Only usable at night in the Sky layer"); */

        ItemID.Sets.SortingPriorityBossSpawns[Type] = 12; // This helps sort in inventory

        // Removed ShieldImmunity as it doesn't exist
    }

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
        Item.maxStack = 20;
        Item.value = 100;
        Item.rare = ItemRarityID.Cyan;
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.consumable = true;
        Item.noMelee = true;
        Item.noUseGraphic = false;
        Item.UseSound = SoundID.Item43; // Magic spell sound
    }

    public override bool CanUseItem(Player player)
    {
        // Can only use at night (between dusk and dawn)
        if (!Main.IsItDay())
        {
            // Check if player is in Space layer (above all terrain)
            if (player.ZoneSkyHeight)
            {
                // Check if boss already exists
                return !NPC.AnyNPCs(ModContent.NPCType<StardustAngel>());
            }
            else
            {
                Main.NewText("The crystal must be used in the Sky layer!", Color.LightBlue);
                return false;
            }
        }
        else
        {
            Main.NewText("The crystal only glows at night...", Color.LightBlue);
            return false;
        }
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            SoundEngine.PlaySound(SoundID.Roar, player.position);

            // Determine spawn position
            int spawnX = (int)player.Center.X;
            int spawnY = (int)player.Center.Y - 300; // Spawn above player

            // Spawn the boss
            int npcIndex = NPC.NewNPC(
                player.GetSource_ItemUse(Item),
                spawnX,
                spawnY,
                ModContent.NPCType<StardustAngel>()
            );

            // Get the spawned NPC
            NPC npc = Main.npc[npcIndex];

            // Set target to the player who summoned it
            npc.target = player.whoAmI;

            // Teleport the NPC to the correct position (just in case)
            npc.Center = new Vector2(spawnX, spawnY);

            // Sync for multiplayer
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcIndex);
            }

            // Visual effects at spawn location
            for (int i = 0; i < 25; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    new Vector2(spawnX - 25, spawnY - 25),
                    50, 50,
                    ModContent.DustType<Dusts.Stardust>(),
                    0f, 0f,
                    100,
                    default,
                    2f
                );
                dust.noGravity = true;
                dust.velocity *= 3f;
            }

            Main.NewText("The Stardust Angel descends from the heavens!", Color.LightBlue);
        }

        return true;
    }

    public override void AddRecipes()
    {
        // Example recipe - adjust as needed
        CreateRecipe()
            .AddIngredient(ItemID.FallenStar, 5)
            .AddIngredient(ItemID.CrystalShard, 10)
            .AddIngredient(ItemID.SoulofLight, 10)
            .AddIngredient(ItemID.SoulofNight, 10)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}