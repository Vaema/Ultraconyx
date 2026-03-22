using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.VanillaChanges;

public class MusketToSilverBullet : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return entity.type == ItemID.Musket;
    }
    
    public override void ModifyShootStats(Item item, Player player, ref Microsoft.Xna.Framework.Vector2 position, 
        ref Microsoft.Xna.Framework.Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        if (item.type == ItemID.Musket && type == ProjectileID.Bullet)
        {
            type = ProjectileID.SilverBullet; //PLEASE WORK WAAHHH
        }
    }
}