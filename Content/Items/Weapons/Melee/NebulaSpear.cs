using Microsoft.Xna.Framework;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

using Ultraconyx.Content.Projectiles.Melee;

namespace Ultraconyx.Content.Items.Weapons.Melee;

public class NebulaSpear : ModItem
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.SkipsInitialUseSound[Type] = true;
        ItemID.Sets.Spears[Type] = true;
        ItemID.Sets.Yoyo[Type] = false;
    }

    public override void SetDefaults()
    {
        Item.width = Item.height = 48;

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useAnimation = 24;
        Item.useTime = 24;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;

        Item.damage = 240;
        Item.DamageType = DamageClass.Melee;
        Item.knockBack = 7f;
        Item.noUseGraphic = true;
        Item.noMelee = true;

        Item.rare = ItemRarityID.Yellow;
        Item.value = Item.sellPrice(gold: 15);

        Item.shootSpeed = 3.7f;
        Item.shoot = ModContent.ProjectileType<NebulaSpearProjectile>();
    }

    public override void HoldItem(Player player)
    {
        if (player.altFunctionUse == 2 && player.whoAmI == Main.myPlayer)
        {
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item19;
            Item.shoot = ModContent.ProjectileType<NebulaJavelin>();
            Item.shootSpeed = 16f;
        }
        else
        {
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<NebulaSpearProjectile>();
            Item.shootSpeed = 3.7f;
        }
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool CanUseItem(Player player)
    {
        if (player.altFunctionUse == 2)
            return player.ownedProjectileCounts[ModContent.ProjectileType<NebulaJavelin>()] < 1;
        else
            return player.ownedProjectileCounts[Item.shoot] < 1;
    }

    public override bool? UseItem(Player player)
    {
        if (player.altFunctionUse != 2)
        {
            Vector2 target = Main.MouseWorld;
            Vector2 direction = target - player.Center;
            direction.Normalize();
            Vector2 velocity = direction * 15f;

            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, velocity,
                ModContent.ProjectileType<NebulaSpearBolt>(), Item.damage / 2, Item.knockBack * 0.5f, player.whoAmI);
        }

        if (!Main.dedServ && Item.UseSound.HasValue)
            SoundEngine.PlaySound(Item.UseSound.Value, player.Center);

        return null;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.Spear).
            AddIngredient(ItemID.FragmentNebula, 18).
            AddIngredient(ItemID.Ectoplasm, 10).
            AddIngredient(ItemID.BeetleHusk, 5).
            AddTile(TileID.MythrilAnvil).
            Register();
    }
}
