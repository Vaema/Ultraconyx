using Microsoft.Xna.Framework;
using Terraria;

namespace Ultraconyx;

public static partial class Utilities
{
    // Used from the Infernum Mode source code.
    // Their source uses the MIT license.

    /// <summary>
    /// Gets a unit direction towards an arbitrary destination for an entity based on its center. Has <see cref="float.NaN"/> safety in the form of a fallback vector.
    /// </summary>
    /// <param name="entity">The entity to check from.</param>
    /// <param name="destination">The destination to get the direction to.</param>
    /// <param name="fallback">A fallback value to use in the event of an unsafe normalization.</param>
    public static Vector2 SafeDirectionTo(this Entity entity, Vector2 destination, Vector2 fallback = default) => (destination - entity.Center).SafeNormalize(fallback);
}
