using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.ModLoader.IO;
using Ultraconyx.Content.UI;

namespace Ultraconyx.Content.Items;

public class StarterPouch : ModItem
{
    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 999;
        Item.value = 0;
        Item.rare = ItemRarityID.White;
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }

    public override bool CanRightClick()
    {
        return true;
    }

    public override void RightClick(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            // Open the UI directly
            StarterPouchUI.OpenUI();
        }
    }
}

public class StarterPouchPlayer : ModPlayer
{
    public bool hasReceivedStarterKit;

    public override void OnEnterWorld()
    {
        // Give starter pouch to new characters
        if (!hasReceivedStarterKit && Player.whoAmI == Main.myPlayer)
        {
            // Check if player has any non-starter items
            bool hasItems = false;
            for (int i = 0; i < 50; i++)
            {
                if (Player.inventory[i].type != ItemID.None && 
                    Player.inventory[i].type != ItemID.FamiliarShirt &&
                    Player.inventory[i].type != ItemID.FamiliarPants &&
                    Player.inventory[i].type != ItemID.FamiliarWig)
                {
                    hasItems = true;
                    break;
                }
            }

            // If no items (new character), give the pouch
            if (!hasItems)
            {
                Player.QuickSpawnItem(Player.GetSource_GiftOrReward(), ModContent.ItemType<StarterPouch>());
                hasReceivedStarterKit = true;
            }
        }
    }

    public override void SaveData(TagCompound tag)
    {
        tag["hasReceivedStarterKit"] = hasReceivedStarterKit;
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.ContainsKey("hasReceivedStarterKit"))
        {
            hasReceivedStarterKit = tag.GetBool("hasReceivedStarterKit");
        }
    }
}