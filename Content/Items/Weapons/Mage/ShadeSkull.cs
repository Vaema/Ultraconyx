using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Items.Weapons.Mage;

public class ShadeSkull : ModItem
{
    public int satanPower = 0;
    public bool satanMode = false;
    public int satanTimer = 0;
    public int flashTimer = 0; // Changed from private to public so projectile can access it

    public override string Texture => "Ultracronyx/Content/Items/Weapons/Mage/ShadeSkull";

    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 4));
        ItemID.Sets.AnimatesAsSoul[Item.type] = true;
    }

    public override void SetDefaults()
    {
        Item.damage = 120;
        Item.DamageType = DamageClass.Magic;
        Item.mana = 12;
        Item.knockBack = 12f;
        Item.useTime = 10;
        Item.useAnimation = 10;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<Projectiles.ShadeSkullProjectile>();
        Item.shootSpeed = 10f;
        Item.rare = ItemRarityID.Red;
        Item.autoReuse = true;
        
        Item.width = 28;
        Item.height = 28;
    }

    public override bool CanUseItem(Player player)
    {
        // Count projectiles
        int projectileCount = 0;
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile proj = Main.projectile[i];
            if (proj.active && proj.type == Item.shoot && proj.owner == player.whoAmI)
            {
                projectileCount++;
            }
        }

        // Only shoot if less than 8 projectiles
        if (projectileCount >= 8)
        {
            Item.mana = 0;
            return false;
        }

        Item.mana = satanMode ? 9 : 12;

        if (satanMode)
        {
            Item.damage = (int)(120 * 1.2f);
            Item.useTime = 9;
            Item.useAnimation = 9;
            Item.shootSpeed = 12f;
        }
        else
        {
            Item.damage = 120;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.shootSpeed = 10f;
        }

        return true;
    }

    public override void UpdateInventory(Player player)
    {
        if (satanMode)
        {
            satanTimer--;
            flashTimer++;
            
            if (satanTimer <= 0)
            {
                satanMode = false;
                satanPower = 0;
                flashTimer = 0;
            }
        }
        else if (satanPower >= 90)
        {
            flashTimer++;
        }
        else
        {
            flashTimer = 0;
        }
    }

    // REMOVED: The old ModifyHitNPC method since charging is now handled in the projectile

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        base.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        
        if (satanPower > 0 && !satanMode && Main.playerInventory)
        {
            Vector2 textPosition = position + new Vector2(0, -30);
            Utils.DrawBorderString(spriteBatch, satanPower + "%", textPosition, Color.DarkRed, 0.8f);
        }
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, 
        Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = TextureAssets.Item[Item.type].Value;
        Color color = drawColor;
        
        if (satanMode)
        {
            if (flashTimer % 10 < 5)
            {
                color = Color.Lerp(drawColor, Color.Red, 0.7f);
            }
        }
        else if (satanPower >= 90)
        {
            if (flashTimer % 20 < 10)
            {
                color = Color.Lerp(drawColor, Color.Red, 0.5f);
            }
        }
        
        spriteBatch.Draw(
            texture,
            position,
            frame,
            color,
            0f,
            origin,
            scale,
            SpriteEffects.None,
            0f
        );
        
        return false;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, 
        ref float rotation, ref float scale, int whoAmI)
    {
        // Get the current frame for animation
        Rectangle frame;
        Main.instance.LoadItem(Item.type);
        Texture2D texture = TextureAssets.Item[Item.type].Value;
        
        // Calculate animation frame for world items
        // Using the same logic as DrawAnimationVertical with 4 frames
        int frameCount = 4;
        int frameHeight = texture.Height / frameCount;
        int frameIndex = (int)(Main.GameUpdateCount / 6) % frameCount; // 6 ticks per frame (same as DrawAnimationVertical)
        
        frame = new Rectangle(0, frameIndex * frameHeight, texture.Width, frameHeight);
        
        Vector2 position = Item.position - Main.screenPosition + new Vector2(Item.width / 2, Item.height / 2);
        Color color = lightColor;
        
        // Apply your existing color logic for satan mode
        if (satanMode)
        {
            if (flashTimer % 10 < 5)
            {
                color = Color.Lerp(lightColor, Color.Red, 0.7f);
            }
        }
        else if (satanPower >= 90)
        {
            if (flashTimer % 20 < 10)
            {
                color = Color.Lerp(lightColor, Color.Red, 0.5f);
            }
        }
        
        spriteBatch.Draw(
            texture,
            position,
            frame, // Use the calculated frame instead of null
            color,
            rotation,
            new Vector2(texture.Width / 2f, frameHeight / 2f), // Adjust origin for frame height
            scale,
            SpriteEffects.None,
            0f
        );
        
        return false; // Return false to prevent default drawing
    }

    public override bool Shoot(
        Player player,
        Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
        Vector2 position,
        Vector2 velocity,
        int type,
        int damage,
        float knockback)
    {
        int projectileCount = 0;
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            if (Main.projectile[i].active && Main.projectile[i].type == type && Main.projectile[i].owner == player.whoAmI)
            {
                projectileCount++;
            }
        }
        
        if (projectileCount < 8)
        {
            // Give each projectile a unique ai[0] value for orbit positioning
            int newProjectile = Projectile.NewProjectile(source, player.Center, velocity, type, damage, knockback, player.whoAmI);
            Main.projectile[newProjectile].ai[0] = projectileCount; // Use count as orbit position index
        }
        
        return false;
    }
    
    public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
    {
        if (satanMode)
        {
            tooltips.Add(new TooltipLine(Mod, "SatanMode", "[c/FF0000:SATAN MODE ACTIVE] - " + (satanTimer / 60) + " seconds remaining"));
        }
        else
        {
            tooltips.Add(new TooltipLine(Mod, "SatanPower", "Satan Power: " + satanPower + "%"));
            tooltips.Add(new TooltipLine(Mod, "ProjectileLimit", "Max Projectiles: 8"));
        }
    }
}