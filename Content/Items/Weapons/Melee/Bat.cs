using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Items.Weapons.Melee;

public class Bat : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 10; // base damage, not important since bats die instantly
        Item.DamageType = DamageClass.Melee;
        Item.width = 40;
        Item.height = 40;
        Item.useTime = 15;
        Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 3;
        Item.value = Item.buyPrice(0, 0, 50);
        Item.rare = ItemRarityID.Blue;
        Item.autoReuse = true;
    }

    // This makes the weapon deal enormous bonus damage ONLY to bats.
    public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
    {
        // All vanilla bats use AIStyle 14, OR you can check by specific NPC IDs.
        if (target.type == NPCID.CaveBat ||
            target.type == NPCID.GiantBat ||
            target.type == NPCID.Hellbat ||
            target.type == NPCID.IceBat ||
            target.type == NPCID.JungleBat ||
            target.type == NPCID.Lavabat ||
            target.type == NPCID.SporeBat ||
            target.type == NPCID.Vampire) // optional
        {
            modifiers.FinalDamage *= 9999f; // guarantees instant kill
        }
    }
}
