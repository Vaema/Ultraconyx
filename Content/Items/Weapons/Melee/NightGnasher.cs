using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ultraconyx.Content.Projectiles;

namespace Ultraconyx.Content.Items.Weapons.Melee;

public class NightGnasher : ModItem
{
    private int swingCount = 0;
    private bool isThrowing = false;
    private bool hasThrownThisSwing = false;
    private bool boomerangIsOut = false;

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.damage = 35;
        Item.DamageType = DamageClass.Melee;
        Item.width = 42;
        Item.height = 42;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 5f;
        Item.value = Item.sellPrice(0, 1, 50, 0);
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;
        Item.noMelee = false;
        Item.noUseGraphic = false;
        Item.shoot = ProjectileID.None;
        Item.shootSpeed = 12f;
        Item.crit = 5;
    }

    public override bool CanUseItem(Player player)
    {
        UpdateBoomerangStatus(player);
        
        if (boomerangIsOut)
        {
            return false;
        }
        
        return true;
    }

    private void UpdateBoomerangStatus(Player player)
    {
        boomerangIsOut = false;
        
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile proj = Main.projectile[i];
            if (proj.active && proj.owner == player.whoAmI && 
                proj.type == ModContent.ProjectileType<NightGnasherProjectile>())
            {
                boomerangIsOut = true;
                break;
            }
        }
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        if (player.itemAnimation == player.itemAnimationMax - 1)
        {
            hasThrownThisSwing = false;
            
            int patternIndex = swingCount % 3;
            
            if (patternIndex == 2) 
            {
                isThrowing = true;
                Item.noUseGraphic = true;
                Item.noMelee = true;
                Item.useStyle = ItemUseStyleID.Shoot;
            }
            else 
            {
                isThrowing = false;
                Item.noUseGraphic = false;
                Item.noMelee = false;
                Item.useStyle = ItemUseStyleID.Swing;
            }
            
            swingCount++;
        }
        
        if (isThrowing && !hasThrownThisSwing && player.itemAnimation <= player.itemAnimationMax * 0.75f)
        {
            hasThrownThisSwing = true;
            
            Vector2 mousePos = Main.MouseWorld;
            Vector2 direction = mousePos - player.Center;
            if (direction == Vector2.Zero)
            {
                direction = new Vector2(player.direction, 0);
            }
            direction.Normalize();
            Vector2 velocity = direction * Item.shootSpeed;
            
            Vector2 spawnPosition = player.RotatedRelativePoint(player.MountedCenter) + direction * 20f;
            
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), spawnPosition, velocity, 
                ModContent.ProjectileType<NightGnasherProjectile>(), Item.damage, Item.knockBack, player.whoAmI);
            
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item7, player.Center);
            
            boomerangIsOut = true;
        }
    }

    public override void UpdateInventory(Player player)
    {
        UpdateBoomerangStatus(player);
        
        if (player.itemAnimation <= 0 && player.channel == false && !boomerangIsOut)
        {
            swingCount = 0;
            isThrowing = false;
            hasThrownThisSwing = false;
            Item.noUseGraphic = false;
            Item.noMelee = false;
            Item.useStyle = ItemUseStyleID.Swing;
        }
        
        if (player.itemAnimation <= 0 && player.channel)
        {
            isThrowing = false;
            hasThrownThisSwing = false;
            Item.noUseGraphic = false;
            Item.noMelee = false;
            Item.useStyle = ItemUseStyleID.Swing;
        }
    }

    public override void HoldItem(Player player)
    {
        UpdateBoomerangStatus(player);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.DemoniteBar, 12)
            .AddIngredient(ItemID.ShadowScale, 6)
            .AddTile(TileID.Anvils)
            .Register();
    }

    public override Vector2? HoldoutOffset()
    {
        return new Vector2(-2f, 0f);
    }
}