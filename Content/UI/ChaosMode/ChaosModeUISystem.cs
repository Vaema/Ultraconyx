using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace Ultraconyx.Content.UI.ChaosMode;

public class ChaosModeUISystem : ModSystem
{
    private UserInterface chaosModeInterface;
    private ChaosModeUI chaosModeUI;
    private GameTime _lastGameTime;

    public override void Load()
    {
        if (!Main.dedServ)
        {
            chaosModeUI = new ChaosModeUI();
            chaosModeUI.Activate();

            chaosModeInterface = new UserInterface();
            chaosModeInterface.SetState(chaosModeUI);
        }
    }

    public override void Unload()
    {
        chaosModeUI = null;
        chaosModeInterface = null;
    }

    public override void UpdateUI(GameTime gameTime)
    {
        _lastGameTime = gameTime;
        chaosModeInterface?.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "Ultracronyx: Chaos Mode Button",
                delegate
                {
                    if (chaosModeInterface != null && _lastGameTime != null)
                    {
                        chaosModeInterface.Draw(Main.spriteBatch, _lastGameTime);
                    }
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
}