using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Items.Weapons.Melee;

public class AtlasEdge : ModItem
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
    }

    public override void SetDefaults()
    {
        Item.damage = 210;
        Item.DamageType = DamageClass.Melee;
        Item.knockBack = 12f;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.crit = 6;

        Item.useStyle = ItemUseStyleID.Swing;
        Item.autoReuse = true;

        Item.width = 64;
        Item.height = 64;

        Item.rare = ItemRarityID.Yellow;
        Item.value = Item.buyPrice(0, 25);

        Item.shoot = ModContent.ProjectileType<Projectiles.AtlasEdgeProjectile>();
        Item.shootSpeed = 14f;

        Item.noMelee = true;
        Item.noUseGraphic = false;
    }
}
