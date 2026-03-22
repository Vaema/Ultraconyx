using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using System.Collections.Generic;

namespace Ultraconyx.Content.UI;

public class StarterPouchUI : UIState
{
    public static bool Visible = false;
    
    private UIImageButton mageButton;
    private UIImageButton meleeButton;
    private UIImageButton rangerButton;
    private UIImageButton summonerButton;
    
    private UIText mageText;
    private UIText meleeText;
    private UIText rangerText;
    private UIText summonerText;
    
    private UIText titleText;

    public override void OnInitialize()
    {
        // Get screen dimensions
        int screenWidth = Main.screenWidth;
        int screenHeight = Main.screenHeight;
        
        // Icon size (68x68 as specified)
        int iconSize = 68;
        int textOffset = 30;
        int verticalSpacing = 120; // Increased vertical spacing between top and bottom rows
        
        // Title text - perfectly centered
        titleText = new UIText("Choose your class:", 1.2f, true);
        titleText.Left.Set(screenWidth / 2 - 80, 0f);
        titleText.Top.Set(screenHeight / 4 - 50, 0f);
        Append(titleText);
        
        // Mage button and text (top-left)
        mageButton = new UIImageButton(ModContent.Request<Texture2D>("Ultracronyx/Content/UI/MageSelect"));
        mageButton.Width.Set(iconSize, 0f);
        mageButton.Height.Set(iconSize, 0f);
        mageButton.Left.Set(screenWidth / 3 - iconSize / 2, 0f);
        mageButton.Top.Set(screenHeight / 2 - iconSize - verticalSpacing / 2, 0f); // Top row
        mageButton.SetPadding(0);
        mageButton.OnLeftClick += SelectMage;
        Append(mageButton);
        
        // Mage text - centered under button
        mageText = new UIText("Mage");
        mageText.Left.Set(mageButton.Left.Pixels + iconSize / 2 - 20, 0f);
        mageText.Top.Set(mageButton.Top.Pixels + iconSize + textOffset, 0f);
        Append(mageText);

        // Ranger button and text (top-right)
        rangerButton = new UIImageButton(ModContent.Request<Texture2D>("Ultracronyx/Content/UI/RangerSelect"));
        rangerButton.Width.Set(iconSize, 0f);
        rangerButton.Height.Set(iconSize, 0f);
        rangerButton.Left.Set(screenWidth * 2 / 3 - iconSize / 2, 0f);
        rangerButton.Top.Set(screenHeight / 2 - iconSize - verticalSpacing / 2, 0f); // Top row
        rangerButton.SetPadding(0);
        rangerButton.OnLeftClick += SelectRanger;
        Append(rangerButton);
        
        // Ranger text - centered under button
        rangerText = new UIText("Ranger");
        rangerText.Left.Set(rangerButton.Left.Pixels + iconSize / 2 - 25, 0f);
        rangerText.Top.Set(rangerButton.Top.Pixels + iconSize + textOffset, 0f);
        Append(rangerText);

        // Melee button and text (bottom-left)
        meleeButton = new UIImageButton(ModContent.Request<Texture2D>("Ultracronyx/Content/UI/MeleeSelect"));
        meleeButton.Width.Set(iconSize, 0f);
        meleeButton.Height.Set(iconSize, 0f);
        meleeButton.Left.Set(screenWidth / 3 - iconSize / 2, 0f);
        meleeButton.Top.Set(screenHeight / 2 + verticalSpacing / 2, 0f); // Bottom row
        meleeButton.SetPadding(0);
        meleeButton.OnLeftClick += SelectMelee;
        Append(meleeButton);
        
        // Melee text - centered under button
        meleeText = new UIText("Melee");
        meleeText.Left.Set(meleeButton.Left.Pixels + iconSize / 2 - 22, 0f);
        meleeText.Top.Set(meleeButton.Top.Pixels + iconSize + textOffset, 0f);
        Append(meleeText);

        // Summoner button and text (bottom-right)
        summonerButton = new UIImageButton(ModContent.Request<Texture2D>("Ultracronyx/Content/UI/SummonerSelect"));
        summonerButton.Width.Set(iconSize, 0f);
        summonerButton.Height.Set(iconSize, 0f);
        summonerButton.Left.Set(screenWidth * 2 / 3 - iconSize / 2, 0f);
        summonerButton.Top.Set(screenHeight / 2 + verticalSpacing / 2, 0f); // Bottom row
        summonerButton.SetPadding(0);
        summonerButton.OnLeftClick += SelectSummoner;
        Append(summonerButton);
        
        // Summoner text - centered under button
        summonerText = new UIText("Summoner");
        summonerText.Left.Set(summonerButton.Left.Pixels + iconSize / 2 - 35, 0f);
        summonerText.Top.Set(summonerButton.Top.Pixels + iconSize + textOffset, 0f);
        Append(summonerText);
    }

    private void SelectMage(UIMouseEvent evt, UIElement listeningElement)
    {
        SoundEngine.PlaySound(SoundID.MenuTick);
        
        // Give mage items
        Player player = Main.LocalPlayer;
        
        // Amethyst Staff
        player.QuickSpawnItem(player.GetSource_GiftOrReward(), ItemID.AmethystStaff);
        
        // 5 Mana Crystals
        for (int i = 0; i < 5; i++)
        {
            player.QuickSpawnItem(player.GetSource_GiftOrReward(), ItemID.ManaCrystal);
        }

        // Remove starter pouch and close UI
        RemoveStarterPouch();
        CloseUI();
    }

    private void SelectMelee(UIMouseEvent evt, UIElement listeningElement)
    {
        SoundEngine.PlaySound(SoundID.MenuTick);
        
        // Give melee items
        Player player = Main.LocalPlayer;
        
        // Copper Broadsword
        player.QuickSpawnItem(player.GetSource_GiftOrReward(), ItemID.CopperBroadsword);

        // Remove starter pouch and close UI
        RemoveStarterPouch();
        CloseUI();
    }

    private void SelectRanger(UIMouseEvent evt, UIElement listeningElement)
    {
        SoundEngine.PlaySound(SoundID.MenuTick);
        
        // Give ranger items
        Player player = Main.LocalPlayer;
        
        // Copper Bow
        player.QuickSpawnItem(player.GetSource_GiftOrReward(), ItemID.CopperBow);
        
        // 100 Wooden Arrows
        player.QuickSpawnItem(player.GetSource_GiftOrReward(), ItemID.WoodenArrow, 100);

        // Remove starter pouch and close UI
        RemoveStarterPouch();
        CloseUI();
    }

    private void SelectSummoner(UIMouseEvent evt, UIElement listeningElement)
    {
        SoundEngine.PlaySound(SoundID.MenuTick);
        
        // Give summoner items
        Player player = Main.LocalPlayer;
        
        // Leather Whip (using BlandWhip for now)
        player.QuickSpawnItem(player.GetSource_GiftOrReward(), ItemID.BlandWhip);
        
        // Slime Staff
        player.QuickSpawnItem(player.GetSource_GiftOrReward(), ItemID.SlimeStaff);

        // Remove starter pouch and close UI
        RemoveStarterPouch();
        CloseUI();
    }

    private void RemoveStarterPouch()
    {
        Player player = Main.LocalPlayer;
        
        // Remove one starter pouch from inventory
        for (int i = 0; i < 50; i++)
        {
            if (player.inventory[i].type == ModContent.ItemType<Items.StarterPouch>())
            {
                player.inventory[i].TurnToAir();
                break;
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (Visible)
        {
            // Force close any other UI that might be open
            if (Main.playerInventory)
                Main.playerInventory = false;
                
            Main.blockInput = true;
            Main.blockMouse = true;
        }
    }

    public override void OnActivate()
    {
        base.OnActivate();
        IgnoresMouseInteraction = false;
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
        Main.blockInput = false;
        Main.blockMouse = false;
    }

    public static void OpenUI()
    {
        // Close any other UI that might be open
        Main.playerInventory = false;
        
        Visible = true;
        Main.blockInput = true;
        Main.blockMouse = true;
    }

    public static void CloseUI()
    {
        Visible = false;
        Main.blockInput = false;
        Main.blockMouse = false;
    }
}

public class StarterPouchUISystem : ModSystem
{
    private UserInterface _userInterface;
    private StarterPouchUI _starterPouchUI;

    public override void Load()
    {
        if (!Main.dedServ)
        {
            _starterPouchUI = new();
            _starterPouchUI.Activate();
            _userInterface = new();
            _userInterface.SetState(_starterPouchUI);
        }
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (_userInterface != null && StarterPouchUI.Visible)
        {
            _userInterface.Update(gameTime);
        }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "Ultracronyx: Starter Pouch UI",
                delegate
                {
                    if (StarterPouchUI.Visible && _userInterface != null)
                    {
                        if (_userInterface.CurrentState == null)
                        {
                            _userInterface.SetState(_starterPouchUI);
                        }
                        
                        _userInterface.Draw(Main.spriteBatch, new GameTime());
                    }
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
}