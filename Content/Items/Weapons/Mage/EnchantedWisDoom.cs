using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Ultraconyx.Content.Projectiles;

namespace Ultraconyx.Content.Items.Weapons.Mage;

public class EnchantedWisDoom : ModItem
{
    public override void SetStaticDefaults()
    {
        // DisplayName.SetDefault("Enchanted Wis-Doom");
        // Tooltip.SetDefault("Spawns homing enchanted projectiles.\nCan be acquired by shimmering an Enchanted Tome of Despair after defeating all 3 mechanical bosses.");
    }

    public override void SetDefaults()
    {
        Item.damage = 45;
        Item.DamageType = DamageClass.Magic;
        Item.mana = 15;
        Item.width = 40;
        Item.height = 40;
        Item.useTime = 25;
        Item.useAnimation = 25;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 3f;
        Item.value = Item.sellPrice(gold: 10);
        Item.rare = ItemRarityID.Pink;
        Item.UseSound = SoundID.Item20;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<EnchantedWD>();
        Item.shootSpeed = 10f;
    }

    public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        // Add slight spread to the two projectiles
        for (int i = 0; i < 2; i++)
        {
            Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(15));
            Projectile.NewProjectile(source, position, newVelocity, type, damage, knockback, player.whoAmI);
        }
        return false; // prevent default projectile spawn
    }
}