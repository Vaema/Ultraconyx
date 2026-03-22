using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.DataStructures;

namespace Ultraconyx.Content.Items.Weapons.Ranger;

public class DeleGun : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 55;
        Item.DamageType = DamageClass.Ranged;
        Item.width = 58;
        Item.height = 26;
        Item.useTime = 12;
        Item.useAnimation = 12;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 2f;
        Item.value = Item.sellPrice(gold: 5);
        Item.rare = ItemRarityID.Orange;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<Projectiles.DeleProjectile>();
        Item.shootSpeed = 16f;
        Item.useAmmo = AmmoID.Gel;
        Item.UseSound = null; // We'll handle sound in Shoot override
    }

    public override bool CanConsumeAmmo(Item ammo, Player player)
    {
        return Main.rand.NextFloat() >= 0.10f; // 10% chance not to consume ammo
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        // Add slight spread
        velocity = velocity.RotatedByRandom(MathHelper.ToRadians(2f));
    }

    public override Vector2? HoldoutOffset()
    {
        return new Vector2(-2f, 0f); // Adjusts the gun's position when held
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        // Play slime hit sound instead of normal gunshot
        SoundEngine.PlaySound(SoundID.NPCHit1, player.Center);

        // Create the projectile
        Projectile.NewProjectile(source, position, velocity,
            ModContent.ProjectileType<Projectiles.DeleProjectile>(), damage, knockback, player.whoAmI);

        return false; // Return false because we manually spawned the projectile
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.FlintlockPistol)
            .AddIngredient(ItemID.Gel, 250)
            .AddIngredient(ItemID.Topaz, 10)
            .AddTile(TileID.Anvils)
            .Register();
    }
}