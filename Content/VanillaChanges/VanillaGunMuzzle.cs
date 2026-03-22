using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.VanillaChanges;

public class VanillaGunMuzzle : GlobalItem
{
    // List of vanilla gun item IDs that should have muzzle flashes
    private static readonly int[] GunIDs = new int[]
    {
        ItemID.FlintlockPistol,
        ItemID.Musket,
        ItemID.TheUndertaker,
        ItemID.RedRyder,
        ItemID.Handgun,
        ItemID.PhoenixBlaster,
        ItemID.Revolver,
        ItemID.ClockworkAssaultRifle,
        ItemID.Uzi,
        ItemID.Megashark,
        ItemID.VenusMagnum,
        ItemID.TacticalShotgun,
        ItemID.SniperRifle,
        ItemID.ChainGun,
        ItemID.SDMG,
        ItemID.VortexBeater,
        ItemID.Minishark
    };

    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        for (int i = 0; i < GunIDs.Length; i++)
        {
            if (entity.type == GunIDs[i])
                return true;
        }
        return false;
    }

    // Store shooting direction for muzzle flash calculation
    public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        // Get the shooting direction from player to mouse
        Vector2 mouseWorld = Main.MouseWorld;
        Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);
        Vector2 shootDirection = Vector2.Normalize(mouseWorld - playerCenter);
        
        if (float.IsNaN(shootDirection.X) || float.IsNaN(shootDirection.Y))
        {
            shootDirection = Vector2.UnitX * player.direction;
        }
        
        // Store the shooting direction for muzzle flash
        player.GetModPlayer<MuzzleFlashPlayer>().LastShotDirection = shootDirection;
        player.GetModPlayer<MuzzleFlashPlayer>().LastShotPlayerCenter = playerCenter;
        player.GetModPlayer<MuzzleFlashPlayer>().PlayerDirection = player.direction;
    }

    public override void UseItemFrame(Item item, Player player)
    {
        // Check if we just shot (item animation is at shooting frame)
        // Using itemAnimationMax - 2 or - 3 for better timing
        if (player.itemAnimation > 0 && player.itemAnimation >= player.itemAnimationMax - 3 && player.whoAmI == Main.myPlayer)
        {
            MuzzleFlashPlayer mPlayer = player.GetModPlayer<MuzzleFlashPlayer>();
            
            if (mPlayer.LastShotDirection != Vector2.Zero)
            {
                // Calculate muzzle position at gun tip
                // Adjust the multiplier (40f) to change how far from player the flash appears
                float muzzleDistance = 40f;
                
                // For different guns, you might want different distances
                if (item.type == ItemID.SniperRifle || item.type == ItemID.TacticalShotgun)
                    muzzleDistance = 45f;
                else if (item.type == ItemID.Minishark || item.type == ItemID.ChainGun)
                    muzzleDistance = 35f;
                
                Vector2 muzzleOffset = mPlayer.LastShotDirection * muzzleDistance;
                
                // IMPORTANT FIX: When player faces left, we need to properly handle the direction
                // The direction vector already accounts for where the player is aiming
                // We just need to use it as-is for the offset
                
                Vector2 muzzlePos = mPlayer.LastShotPlayerCenter + muzzleOffset;
                
                // Debug: Draw a point at the calculated position
                // Dust.NewDustPerfect(muzzlePos, DustID.Firework_Red, Vector2.Zero);
                
                // Create muzzle flash
                CreateMuzzleFlash(muzzlePos, mPlayer.LastShotDirection);
                
                // Reset for next shot
                mPlayer.LastShotDirection = Vector2.Zero;
            }
        }
    }

    private void CreateMuzzleFlash(Vector2 position, Vector2 direction)
    {
        // Create flash effect
        for (int i = 0; i < 10; i++)
        {
            // Random angle variation (0.5 radians ~ 28.6 degrees)
            float angle = Main.rand.NextFloat(-0.5f, 0.5f);
            float speed = Main.rand.NextFloat(2f, 6f);
            
            Dust dust = Dust.NewDustPerfect(position, DustID.Torch, 
                direction.RotatedBy(angle) * speed, 
                0, Color.OrangeRed, Main.rand.NextFloat(1f, 1.5f));
            dust.noGravity = true;
            dust.fadeIn = 1.2f;
        }

        // Smoke
        for (int i = 0; i < 5; i++)
        {
            float angle = Main.rand.NextFloat(-1f, 1f); // Wider angle for smoke
            float speed = Main.rand.NextFloat(1f, 3f);
            
            Dust dust = Dust.NewDustPerfect(position, DustID.Smoke, 
                direction.RotatedBy(angle) * speed, 
                0, Color.Gray, Main.rand.NextFloat(1f, 1.3f));
            dust.noGravity = true;
            dust.fadeIn = 1f;
        }

        // Sparks
        for (int i = 0; i < 4; i++)
        {
            float angle = Main.rand.NextFloat(-0.3f, 0.3f); // Tighter angle for sparks
            float speed = Main.rand.NextFloat(3f, 8f);
            
            Dust dust = Dust.NewDustPerfect(position, DustID.YellowTorch, 
                direction.RotatedBy(angle) * speed, 
                0, Color.Yellow, Main.rand.NextFloat(0.8f, 1.2f));
            dust.noGravity = true;
        }
    }
}

public class MuzzleFlashPlayer : ModPlayer
{
    public Vector2 LastShotDirection { get; set; }
    public Vector2 LastShotPlayerCenter { get; set; }
    public int PlayerDirection { get; set; }

    public override void Initialize()
    {
        LastShotDirection = Vector2.Zero;
        LastShotPlayerCenter = Vector2.Zero;
        PlayerDirection = 1;
    }

    public override void PostUpdate()
    {
        // Optional: Reset if not shooting for a while
        if (Player.itemAnimation == 0)
        {
            LastShotDirection = Vector2.Zero;
        }
    }
}