using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

namespace Ultraconyx.Content.UI.ChaosMode;

// Custom UIImageButton that supports multiple frames (vertical layout)
public class UIImageButton : UIElement
{
    private Texture2D _texture;
    private float _visibilityActive = 1f;
    private float _visibilityInactive = 0.4f;
    private Rectangle _currentFrame;

    public UIImageButton(Texture2D texture)
    {
        _texture = texture;
        Width.Set(_texture.Width, 0f);              // Full width
        Height.Set(_texture.Height / 2, 0f);        // Half height for one frame
        _currentFrame = new Rectangle(0, 0, _texture.Width, _texture.Height / 2);
    }

    public void SetFrame(int frameIndex)
    {
        int frameHeight = _texture.Height / 2;
        _currentFrame = new Rectangle(0, frameIndex * frameHeight, _texture.Width, frameHeight);
    }

    public void SetVisibility(float whenActive, float whenInactive)
    {
        _visibilityActive = whenActive;
        _visibilityInactive = whenInactive;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle dimensions = GetDimensions();

        float visibility = IsMouseHovering ? _visibilityActive : _visibilityInactive;
        spriteBatch.Draw(_texture, dimensions.Position(), _currentFrame, Color.White * visibility, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
    }
}