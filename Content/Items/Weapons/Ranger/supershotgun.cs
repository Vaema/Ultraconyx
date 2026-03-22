using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Ultraconyx.Content.Projectiles;

namespace Ultraconyx.Content.Items.Weapons.Ranger;

public class supershotgun : ModItem
{
    public override void SetStaticDefaults()
    {
        // DisplayName.SetDefault("Super Shotgun");
        // Tooltip.SetDefault("[c/FF0000:RIP AND TEAR UNTIL ITS DONE]");
    }

    public override void SetDefaults()
    {
        Item.width = 54;
        Item.height = 24;
        Item.scale = 1.2f;

        Item.damage = 240;
        Item.DamageType = DamageClass.Ranged;
        Item.knockBack = 14f;
        Item.useTime = 35;
        Item.useAnimation = 35;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.autoReuse = false;
        Item.shoot = ProjectileID.Bullet;
        Item.shootSpeed = 12f;
        Item.useAmmo = AmmoID.Bullet;
        Item.rare = ItemRarityID.Yellow;
        Item.value = Item.sellPrice(platinum: 2);

        // Use a normal shotgun sound for shooting, not the doomjingle
        Item.UseSound = SoundID.Item36; // This is the standard shotgun sound
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.Boomstick);
        recipe.AddIngredient(ItemID.SoulofNight, 50);

        // Handle both crimson and corruption worlds
        if (Main.hardMode)
        {
            if (WorldGen.crimson)
                recipe.AddIngredient(ItemID.Vertebrae, 50);
            else
                recipe.AddIngredient(ItemID.RottenChunk, 50);
        }
        else
        {
            // Fallback for pre-hardmode testing
            recipe.AddIngredient(ItemID.Vertebrae, 50);
        }

        recipe.AddTile(TileID.MythrilAnvil);
        recipe.Register();
    }

    public override Vector2? HoldoutOffset()
    {
        return new Vector2(-40f, 10f);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        // Add recoil - push player backward
        Vector2 recoilDirection = -velocity.SafeNormalize(Vector2.UnitX);
        player.velocity += recoilDirection * 8f; // Half a tile (8 pixels) recoil

        // Spawn the shotgun shell projectiles going backward
        Vector2 backwardDirection = -velocity.SafeNormalize(Vector2.UnitX);

        // First shell
        Projectile.NewProjectile(source,
            position,
            backwardDirection * 8f + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)),
            ModContent.ProjectileType<shotgunshell>(),
            0,
            0f,
            player.whoAmI);

        // Second shell
        Projectile.NewProjectile(source,
            position,
            backwardDirection * 8f + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)),
            ModContent.ProjectileType<shotgunshell>(),
            0,
            0f,
            player.whoAmI);

        // Modify the actual shot to be a spread
        int numberProjectiles = 12; // Number of pellets
        for (int i = 0; i < numberProjectiles; i++)
        {
            Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(15)); // Spread angle
            float scale = 1f - (Main.rand.NextFloat() * .3f);
            perturbedSpeed *= scale;

            Projectile.NewProjectile(source,
                position,
                perturbedSpeed,
                type,
                damage,
                knockback,
                player.whoAmI);
        }

        return false; // Return false because we already spawned projectiles
    }

    // This gets called when the item is crafted
    public override void OnCreated(ItemCreationContext context)
    {
        if (Main.netMode != NetmodeID.Server && context is RecipeItemCreationContext)
        {
            // Play custom sound when crafted
            SoundEngine.PlaySound(new SoundStyle("Ultracronyx/Content/Sounds/doomjingle"), Main.LocalPlayer.Center);
        }
    }
}