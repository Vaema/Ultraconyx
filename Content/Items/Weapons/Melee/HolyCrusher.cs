using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Ultraconyx.Content.Rarities;
using Ultraconyx.Content.Projectiles;

namespace Ultraconyx.Content.Items.Weapons.Melee;

public class HolyCrusher : ModItem
{
    public override void SetDefaults()
    {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useAnimation = 20;
        Item.useTime = 15;
        Item.damage = 250;
        Item.knockBack = 4.5f;
        Item.width = 40;
        Item.height = 40;
        Item.scale = 1f;
        Item.UseSound = SoundID.Item1;
        Item.rare = ModContent.RarityType<PostPraetorian>();
        Item.value = Item.buyPrice(gold: 23);
        Item.DamageType = DamageClass.Melee;
        Item.shoot = ModContent.ProjectileType<HolyCrusherProjectile>();
        Item.noMelee = true;
        Item.shootsEveryUse = true;
        Item.autoReuse = true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        float adjustedItemScale = player.GetAdjustedItemScale(Item);
        Projectile.NewProjectile(source, player.MountedCenter, new Vector2(player.direction, 0f), type, damage, knockback, player.whoAmI, player.direction * player.gravDir, player.itemAnimationMax, adjustedItemScale);
        NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI);

        return base.Shoot(player, source, position, velocity, type, damage, knockback);
    }
}