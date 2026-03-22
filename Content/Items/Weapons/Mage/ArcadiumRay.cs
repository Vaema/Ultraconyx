using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Ultraconyx.Content.Projectiles;

namespace Ultraconyx.Content.Items.Weapons.Mage;

public class ArcadiumRay : ModItem
{
    public override void SetStaticDefaults()
    {
        // DisplayName.SetDefault("Arcadium Ray");
        // Tooltip.SetDefault("Shoots a powerful beam of arcane energy");
    }

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 30;

        Item.damage = 240;
        Item.knockBack = 5f;
        Item.mana = 10;
        Item.crit = 5;

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.reuseDelay = 0;
        Item.autoReuse = true;

        Item.DamageType = DamageClass.Magic;
        Item.noMelee = true;

        Item.value = Item.sellPrice(0, 10, 0, 0);
        Item.rare = ItemRarityID.Red;

        Item.shoot = ModContent.ProjectileType<ArcadiumRayProjectile>();
        Item.shootSpeed = 1f;

        Item.UseSound = SoundID.Item72;
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        Vector2 mousePos = Main.MouseWorld;
        Vector2 playerToMouse = mousePos - player.Center;
        playerToMouse.Normalize();

        position = player.Center;
        velocity = playerToMouse * 1f;
        damage = Item.damage;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

        // Spawn muzzle flash dust with NO GRAVITY
        for (int i = 0; i < 5; i++)
        {
            Vector2 dustVel = velocity.RotatedByRandom(MathHelper.ToRadians(15)) * 0.5f;
            Dust dust = Dust.NewDustPerfect(position, DustID.GemDiamond, dustVel, 0, Color.Cyan, 1.2f);
            dust.noGravity = true; // NO GRAVITY
        }

        return false;
    }

    public override Vector2? HoldoutOffset()
    {
        return new Vector2(4f, 0f);
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.Register();
    }
}