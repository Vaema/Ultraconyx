using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace Ultraconyx.Common.Globals.Items;

public class FloatingGlobalItem : GlobalItem
{
    public override bool InstancePerEntity => true;

    public float FloatTimer { get; set; }
    public float BaseY { get; set; }
    public bool IsFloating { get; set; }
    public Vector2 ChestCenter { get; set; }
    public float OrbitRadius { get; set; }
    public float OrbitAngle { get; set; }
    public float OrbitSpeed { get; set; }
}
