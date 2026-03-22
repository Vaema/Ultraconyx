using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Ultraconyx.Content.Biomes;

public class AbyssalOvalBiome : ModBiome
{
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
    
    // Use peaceful music for the gentle lumflies
    public override int Music => MusicID.OtherworldlyRain;
    
    // Check if player is in the biome
    public override bool IsBiomeActive(Player player)
    {
        // Check if we're near the generated oval biome
        if (HellevatorGen.BiomeExists)
        {
            Point biomeCenter = HellevatorGen.BiomeCenter;
            float distance = Vector2.Distance(player.Center / 16f, new Vector2(biomeCenter.X, biomeCenter.Y));
            
            // Consider player in biome if within 100 tiles of center
            return distance < 100f;
        }
        return false;
    }
    
    public override void SetStaticDefaults()
    {
        // DisplayName.SetDefault("Abyssal Oval Cavern");
    }
    
    // Bestiary display - only include methods that actually exist in base class
    public override string BestiaryIcon => base.BestiaryIcon;
    public override string BackgroundPath => base.BackgroundPath;
    public override Color? BackgroundColor => base.BackgroundColor;
    
    // Remove IsBestiaryBackgroundAvailable() as it doesn't exist in ModBiome
}