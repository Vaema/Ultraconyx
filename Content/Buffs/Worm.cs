using Terraria;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Buffs;

public class Worm : ModBuff
{
    public override void SetStaticDefaults()
    {
        // DisplayName.SetDefault("Worm");
        // Description.SetDefault("A worm fights for you");
        Main.buffNoSave[Type] = false;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        if (player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.WormHead>()] > 0)
        {
            player.buffTime[buffIndex] = 18000;
        }
        else
        {
            player.DelBuff(buffIndex);
            buffIndex--;
        }
    }
}