using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Buffs;

public class CrimsonUndertakerBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        // DisplayName.SetDefault("Crimson Vigor");
        // Description.SetDefault("Greatly increased life regeneration");
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = false;
        Main.debuff[Type] = false;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        // Boost health regen to 10 hp/s for 2 seconds (buff duration)
        player.lifeRegen += 10;

        // Add some visual effect for the buff
        if (Main.rand.NextBool(10))
        {
            // Use dust type 5 (Blood) instead of DustID.Blood
            Dust dust = Dust.NewDustDirect(player.position, player.width, player.height,
                DustID.Blood, 0f, 0f, 100, default, 1.5f); // 5 = Blood dust
            dust.noGravity = true;
            dust.velocity *= 0.5f;
        }
    }
}