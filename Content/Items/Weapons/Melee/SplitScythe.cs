using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ultraconyx.Content.Projectiles;

namespace Ultraconyx.Content.Items.Weapons.Melee;

public class SplitScythe : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 1450;
        Item.DamageType = DamageClass.Melee;
        Item.width = 1;
        Item.height = 1;
        Item.useTime = 45;
        Item.useAnimation = 25;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 5;
        Item.value = Item.buyPrice(0, 1, 0);
        Item.rare = ItemRarityID.LightRed;
        Item.shootSpeed = 10f;
        Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<BigScytheProj>();
    }
}
