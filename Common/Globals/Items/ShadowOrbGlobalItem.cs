using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Graphics;

namespace Ultraconyx.Common.Globals.Items;

public class ShadowOrbGlobalItem : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return entity.type == ItemID.Musket ||
               entity.type == ItemID.Vilethorn ||
               entity.type == ItemID.BallOHurt ||
               entity.type == ItemID.BandofStarpower ||
               entity.type == ItemID.ShadowOrb;
    }

    public override void OnSpawn(Item item, IEntitySource source)
    {
        if (!WorldGen.crimson && source is EntitySource_TileBreak tileSource)
        {
            item.TurnToAir();

            Vector2 orbPos = tileSource.TileCoords.ToWorldCoordinates(8, 8);
            CreateLootDisplay(orbPos);
        }
    }

    private void CreateLootDisplay(Vector2 center)
    {
        Vector2[] positions = new Vector2[5];
        float spacing = 120f;
        float startY = -240f;

        for (int i = 0; i < 5; i++)
            positions[i] = center + new Vector2(0, startY + i * spacing);

        int[] itemIds =
        [
            ItemID.BandofStarpower,
            ItemID.BallOHurt,
            ItemID.Vilethorn,
            ItemID.Musket,
            ItemID.ShadowOrb
        ];

        int npcIndex = NPC.NewNPC(new EntitySource_WorldEvent(),
            (int)center.X, (int)center.Y,
            ModContent.NPCType<ShadowOrbItemPickerNPC>());

        if (Main.npc[npcIndex].ModNPC is ShadowOrbItemPickerNPC npc)
            npc.SetupDisplay(positions, itemIds);
    }
}

public class ShadowOrbItemPickerNPC : ModNPC
{
    private Vector2[] itemPositions = new Vector2[5];
    private float[] baseY = new float[5];
    private int[] itemIds = new int[5];
    private bool[] active = new bool[5];

    private int timer;
    private int hovered = -1;
    private bool processing;

    private const int Duration = 7200;
    private const float HoverRange = 60f;

    public override string Texture => "Terraria/Images/Item_0";

    public override void SetDefaults()
    {
        NPC.width = NPC.height = 1;
        NPC.lifeMax = 1;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.dontTakeDamage = true;
        NPC.immortal = true;
        NPC.aiStyle = -1;
        NPC.Opacity = 0f;
    }

    public void SetupDisplay(Vector2[] positions, int[] ids)
    {
        for (int i = 0; i < 5; i++)
        {
            itemPositions[i] = positions[i];
            baseY[i] = positions[i].Y;
            itemIds[i] = ids[i];
            active[i] = true;
        }
    }

    public override void AI()
    {
        timer++;
        if (timer > Duration) NPC.active = false;

        float pulse = (float)Math.Sin(timer * 0.03f) * 6f;
        for (int i = 0; i < 5; i++)
            itemPositions[i] = new Vector2(NPC.Center.X, baseY[i] + pulse);

        if (Main.netMode != NetmodeID.Server)
        {
            hovered = -1;
            for (int i = 0; i < 5; i++)
            {
                if (!active[i]) continue;
                if (Vector2.Distance(Main.MouseWorld, itemPositions[i]) < HoverRange)
                {
                    hovered = i;
                    break;
                }
            }

            if (hovered >= 0 && Main.mouseLeft && Main.mouseLeftRelease && !processing)
                SelectItem(hovered);
        }
    }

    private void SelectItem(int index)
    {
        processing = true;

        Player player = Main.LocalPlayer;
        Item.NewItem(new EntitySource_WorldEvent(), player.getRect(), itemIds[index]);

        SoundEngine.PlaySound(SoundID.NPCDeath13 with { Pitch = -0.3f });

        for (int i = 0; i < 60; i++)
        {
            float a = MathHelper.TwoPi * i / 60f;
            Dust.NewDustPerfect(itemPositions[index],
                DustID.Shadowflame,
                new Vector2((float)Math.Cos(a), (float)Math.Sin(a)) * 4f,
                0, default, 2.5f).noGravity = true;
        }

        NPC.active = false;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        DrawCentralSpiral(spriteBatch);
        DrawOuterRing();
        DrawItems(spriteBatch);

        if (hovered >= 0)
            DrawHoverText(spriteBatch);
    }

    private void DrawCentralSpiral(SpriteBatch spriteBatch)
    {
        Vector2 top = itemPositions[0];
        Vector2 bottom = itemPositions[4];

        int segments = 140;
        float height = bottom.Y - top.Y;
        float radius = 18f;

        for (int i = 0; i < segments; i++)
        {
            float t1 = i / (float)segments;
            float t2 = (i + 1f) / segments;

            float a1 = t1 * MathHelper.TwoPi * 6f + timer * 0.05f;
            float a2 = t2 * MathHelper.TwoPi * 6f + timer * 0.05f;

            Vector2 p1 = new(NPC.Center.X + (float)Math.Cos(a1) * radius, top.Y + height * t1);
            Vector2 p2 = new(NPC.Center.X + (float)Math.Cos(a2) * radius, top.Y + height * t2);

            Vector2 s1 = p1 - Main.screenPosition;
            Vector2 s2 = p2 - Main.screenPosition;
            Vector2 edge = s2 - s1;

            spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle((int)s1.X, (int)s1.Y, (int)edge.Length(), 2),
                null,
                new Color(160, 90, 255, 220),
                edge.ToRotation(),
                new Vector2(0, 0.5f),
                SpriteEffects.None, 0);
        }
    }

    private void DrawOuterRing()
    {
        float radius = 240f;
        for (int i = 0; i < 80; i++)
        {
            float a = MathHelper.TwoPi * i / 80f;
            Vector2 pos = NPC.Center + new Vector2((float)Math.Cos(a), (float)Math.Sin(a)) * radius;
            Dust.NewDustPerfect(pos, DustID.Shadowflame,
                new Vector2(-(float)Math.Sin(a), (float)Math.Cos(a)) * 0.8f,
                0, default, 1.8f).noGravity = true;
        }
    }

    private void DrawItems(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < 5; i++)
        {
            if (!active[i]) continue;

            Texture2D tex = TextureAssets.Item[itemIds[i]].Value;
            Vector2 pos = itemPositions[i] - Main.screenPosition;

            float scale = i == hovered ? 1.35f : 1.2f;
            spriteBatch.Draw(tex, pos, null, Color.White, 0f,
                tex.Size() / 2f, scale, SpriteEffects.None, 0);
        }
    }

    private void DrawHoverText(SpriteBatch spriteBatch)
    {
        string name = Lang.GetItemNameValue(itemIds[hovered]);
        Vector2 pos = itemPositions[hovered] - Main.screenPosition + new Vector2(0, -48f);

        DynamicSpriteFont font = FontAssets.MouseText.Value;
        Vector2 size = font.MeasureString(name);

        spriteBatch.DrawString(font, name, pos, Color.White,
            0f, size / 2f, 1f, SpriteEffects.None, 0);
    }

    public override bool CheckActive() => false;
}
