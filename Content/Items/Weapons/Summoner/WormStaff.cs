using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Ultraconyx.Content.Buffs;

namespace Ultraconyx.Content.Items.Weapons.Summoner;

public class WormStaff : ModItem
{
    public override void SetStaticDefaults()
    {
        // DisplayName.SetDefault("Worm Staff");
        // Tooltip.SetDefault("Summons a loyal worm that fights for you");
    }

    public override void SetDefaults()
    {
        Item.damage = 9;
        Item.DamageType = DamageClass.Summon;
        Item.mana = 10;
        Item.width = 40;
        Item.height = 40;
        Item.useTime = 36;
        Item.useAnimation = 36;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.noMelee = true;
        Item.knockBack = 2f;
        Item.value = Item.buyPrice(silver: 50);
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item44;

        Item.buffType = ModContent.BuffType<Worm>();
        Item.shoot = ModContent.ProjectileType<Projectiles.WormHead>();
    }

    public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
Vector2 position, Vector2 velocity, int type, int damage, float knockback)
{
player.AddBuff(Item.buffType, 2);

int segmentCount = 6;
int previous = -1;

// Spawn body segments
for (int i = 0; i < segmentCount; i++)
{
    int body = Projectile.NewProjectile(
        source,
        player.Center,
        Vector2.Zero,
        ModContent.ProjectileType<Projectiles.WormBody>(),
        damage,
        knockback,
        player.whoAmI,
        previous,
        i == 0 ? -1 : 0  // first segment anchors to player
    );

    previous = body;
}

// Spawn head at end
Projectile.NewProjectile(
    source,
    player.Center,
    Vector2.Zero,
    ModContent.ProjectileType<Projectiles.WormHead>(),
    damage,
    knockback,
    player.whoAmI,
    previous
);

return false;
}
}