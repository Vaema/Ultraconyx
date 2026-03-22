using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.VanillaChanges;

public class RecoilGlobalItem : GlobalItem
{
    // Track recoil state per player
    private static Dictionary<int, RecoilState> playerRecoilStates = new Dictionary<int, RecoilState>();
    
    // Recoil settings
    private const float MaxRecoilAngle = 0.15f; // Reduced for better control
    private const int RecoilDuration = 4; // Frames for recoil up
    private const int ReturnDuration = 8; // Frames for return to normal
    private const float DefaultRecoilMultiplier = 1.0f;
    
    // Custom recoil multipliers for specific guns
    public static Dictionary<int, float> gunRecoilMultipliers = new Dictionary<int, float>()
    {
        // Weak guns - less recoil
        { ItemID.FlintlockPistol, 0.4f },
        { ItemID.Musket, 0.5f },
        
        // Handguns - moderate recoil
        { ItemID.PhoenixBlaster, 0.8f },
        { ItemID.Handgun, 0.8f },
        
        // Shotguns - strong recoil
        { ItemID.Shotgun, 1.2f },
        { ItemID.OnyxBlaster, 1.0f },
        { ItemID.Boomstick, 0.9f },
        
        // Sniper rifles - very strong recoil
        { ItemID.SniperRifle, 1.5f },
        { ItemID.ChainGun, 0.6f },
        
        // Energy weapons - less recoil
        { ItemID.LaserRifle, 0.2f },
        { ItemID.SpaceGun, 0.15f },
        
        // Endgame guns
        { ItemID.VortexBeater, 0.3f },
        { ItemID.SDMG, 0.5f },
        { ItemID.Tsunami, 0.4f },
        { ItemID.Megashark, 0.4f },
        { ItemID.Gatligator, 0.3f },
        
        // Special cases
        { ItemID.StarCannon, 0.7f },
        { ItemID.PewMaticHorn, 0.6f },
    };
    
    // Class to track recoil state
    private class RecoilState
    {
        public float baseRotation;
        public float recoilOffset;
        public int timer;
        public bool isActive;
        public float maxRecoil;
        public int direction; // 1 for right, -1 for left
        
        public RecoilState(float baseRot, int dir)
        {
            baseRotation = baseRot;
            recoilOffset = 0f;
            timer = 0;
            isActive = false;
            maxRecoil = 0f;
            direction = dir;
        }
    }
    
    public override bool InstancePerEntity => true;
    
    // Check if item is a gun - made public for other classes
    public bool IsGun(Item item)
    {
        // Guns that use bullet ammo
        if (item.useAmmo == AmmoID.Bullet && 
            item.DamageType == DamageClass.Ranged &&
            item.useStyle == ItemUseStyleID.Shoot)
        {
            return true;
        }
        
        // Specific guns
        int[] specialGuns = {
            ItemID.PewMaticHorn,
            ItemID.SnowmanCannon,
            ItemID.StarCannon,
            ItemID.FlintlockPistol,
            ItemID.Musket
        };
        
        foreach (int gunId in specialGuns)
        {
            if (item.type == gunId)
                return true;
        }
        
        return false;
    }
    
    public override void HoldItem(Item item, Player player)
    {
        if (!IsGun(item) || player.itemAnimation <= 0)
            return;
            
        int playerId = player.whoAmI;
        int direction = player.direction; // 1 for right, -1 for left
        
        // Get or create recoil state
        if (!playerRecoilStates.ContainsKey(playerId) || playerRecoilStates[playerId].direction != direction)
        {
            playerRecoilStates[playerId] = new RecoilState(player.itemRotation, direction);
        }
        
        RecoilState state = playerRecoilStates[playerId];
        
        // Check if we just started firing
        if (player.itemAnimation == player.itemAnimationMax - 1)
        {
            // Start new recoil
            state.baseRotation = player.itemRotation;
            state.timer = 0;
            state.isActive = true;
            state.direction = direction;
            
            // Calculate recoil strength
            float multiplier = GetRecoilMultiplier(item.type);
            state.maxRecoil = MaxRecoilAngle * multiplier;
            state.recoilOffset = 0f;
        }
        
        // Update recoil if active
        if (state.isActive)
        {
            state.timer++;
            
            // Calculate recoil amount based on direction
            float recoilAmount = 0f;
            
            if (state.timer <= RecoilDuration)
            {
                // Recoil up phase
                float progress = (float)state.timer / RecoilDuration;
                recoilAmount = -state.maxRecoil * progress; // Negative for upward tilt
            }
            else if (state.timer <= RecoilDuration + ReturnDuration)
            {
                // Return phase
                float progress = (float)(state.timer - RecoilDuration) / ReturnDuration;
                recoilAmount = -state.maxRecoil * (1f - progress);
            }
            else
            {
                // Recoil complete
                state.isActive = false;
                recoilAmount = 0f;
            }
            
            // Apply recoil based on direction
            // When facing left, we need to flip the recoil direction
            state.recoilOffset = recoilAmount * state.direction;
            player.itemRotation = state.baseRotation + state.recoilOffset;
        }
    }
    
    public override void UpdateInventory(Item item, Player player)
    {
        // Clean up recoil state when not holding a gun
        int playerId = player.whoAmI;
        if (playerRecoilStates.ContainsKey(playerId) && !IsGun(player.HeldItem))
        {
            playerRecoilStates.Remove(playerId);
        }
    }
    
    private float GetRecoilMultiplier(int itemType)
    {
        if (gunRecoilMultipliers.ContainsKey(itemType))
            return gunRecoilMultipliers[itemType];
        return DefaultRecoilMultiplier;
    }
    
    public static float GetRecoilStrength(int itemType)
    {
        if (gunRecoilMultipliers.ContainsKey(itemType))
            return gunRecoilMultipliers[itemType];
        return DefaultRecoilMultiplier;
    }
}