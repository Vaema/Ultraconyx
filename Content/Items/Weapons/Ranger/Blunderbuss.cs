using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Ultraconyx.Content.Items.Weapons.Ranger;

public class Blunderbuss : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.damage = 40;
        Item.DamageType = DamageClass.Ranged;
        Item.width = 56;
        Item.height = 24;
        Item.useTime = 22;
        Item.useAnimation = 22;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 6f;
        Item.value = Item.sellPrice(0, 5, 0, 0);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = SoundID.Item36;
        Item.autoReuse = true;
        Item.shoot = ProjectileID.Bullet;
        Item.shootSpeed = 12f;
        Item.useAmmo = AmmoID.Bullet;
        Item.crit = 12;
    }

    public override Vector2? HoldoutOffset()
    {
        return new Vector2(-2f, 0f);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Musket) // Exchange from Musket
            .AddTile(TileID.Anvils)
            .Register();
    }
}