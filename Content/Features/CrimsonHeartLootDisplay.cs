using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Features;

public class ShadowOrbItemReplacer : GlobalItem
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
            item.active = false;
            item.type = 0;
            
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 orbPos = new Vector2(tileSource.TileCoords.X * 16 + 8, tileSource.TileCoords.Y * 16 + 8);
                CreateDelayedLootDisplay(orbPos);
            }
        }
    }
    
    private async void CreateDelayedLootDisplay(Vector2 center)
    {
        await System.Threading.Tasks.Task.Delay(50);
        CreateLootDisplay(center);
    }

    private void CreateLootDisplay(Vector2 center)
    {
        foreach (NPC npc in Main.npc)
        {
            if (npc.active && npc.ModNPC is ShadowOrbWormDisplayNPC && 
                Vector2.Distance(npc.Center, center) < 100f)
            {
                return;
            }
        }

        // Create initial vertical positions for the worm
        Vector2[] positions = new Vector2[5];
        
        // ENTIRE worm moves only half a tile (8 pixels)
        float verticalSpacing = 120f;
        float horizontalAmplitude = 8f;
        float startY = -240f;
        
        // Initial positions in a vertical line (will be animated)
        for (int i = 0; i < 5; i++)
        {
            float y = startY + (i * verticalSpacing);
            positions[i] = center + new Vector2(0, y);
        }

        // Item IDs in worm order
        int[] itemIds = new int[5]
        {
            ItemID.ShadowOrb,
            ItemID.Musket,
            ItemID.Vilethorn,
            ItemID.BallOHurt,
            ItemID.BandofStarpower
        };

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            int displayIndex = NPC.NewNPC(new EntitySource_WorldEvent(), (int)center.X, (int)center.Y,
                ModContent.NPCType<ShadowOrbWormDisplayNPC>());

            if (Main.npc[displayIndex].ModNPC is ShadowOrbWormDisplayNPC displayNPC)
            {
                displayNPC.SetupDisplay(positions, itemIds, horizontalAmplitude, verticalSpacing);

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncNPC, number: displayIndex);
                }
            }
        }
    }
}

public class ShadowOrbWormDisplayNPC : ModNPC
{
    internal Vector2[] itemPositions = new Vector2[5];
    internal int[] itemIds = new int[5];
    internal bool[] itemActive = new bool[5];
    
    // Base vertical positions (y only)
    private float[] baseYPositions = new float[5];
    private float horizontalAmplitude = 8f;
    private float verticalSpacing = 120f;
    
    // Worm wave parameters
    private float[] wavePhases = new float[5];
    private float waveSpeed = 0.03f;
    private float waveLength = 0.4f;
    
    private int timer = 0;
    private const int DisplayDuration = 7200;
    private int hoveredItemIndex = -1;
    private const float MouseHoverRange = 60f;
    
    // Like CrimsonHeartLootDisplayNPC - separate particle creation
    private bool createWormBodyParticles = false;
    private bool createItemCircleParticles = false;
    private bool createOuterParticles = false;
    
    // Item circle radius
    private const float ItemCircleRadius = 35f;

    public override string Texture => "Terraria/Images/Item_0";

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 1;
        NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new() { Hide = true };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, bestiaryData);
        NPCID.Sets.CountsAsCritter[Type] = true;
        NPCID.Sets.MPAllowedEnemies[Type] = true;
    }

    public override void SetDefaults()
    {
        NPC.width = 1;
        NPC.height = 1;
        NPC.lifeMax = 1;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.dontTakeDamage = true;
        NPC.immortal = true;
        NPC.aiStyle = -1;
        NPC.Opacity = 0f;
        NPC.life = 1;
        NPC.netAlways = true;
    }

    public void SetupDisplay(Vector2[] positions, int[] ids, float amplitude, float spacing)
    {
        itemPositions = positions;
        itemIds = ids;
        horizontalAmplitude = amplitude;
        verticalSpacing = spacing;
        
        // Store base Y positions
        for (int i = 0; i < 5; i++) 
        {
            baseYPositions[i] = positions[i].Y;
            itemActive[i] = true;
            
            // Set wave phases so items follow in sequence
            wavePhases[i] = i * waveLength;
        }
        
        timer = 0;
        hoveredItemIndex = -1;
        NPC.netUpdate = true;
    }

    public override void AI()
    {
        timer++;
        
        // UPDATE WORM/SLINKY MOTION
        UpdateWormMotion();
        
        // LIKE CRIMSONHEART - SEPARATE PARTICLE CREATION CYCLES
        int cycleFrame = timer % 3;
        
        createWormBodyParticles = (cycleFrame == 0);
        createItemCircleParticles = (cycleFrame == 1);
        createOuterParticles = (cycleFrame == 2);
        
        // Create particles based on cycle
        if (createWormBodyParticles)
        {
            CreateWormBodyParticles();
        }
        
        if (createItemCircleParticles)
        {
            CreateItemCircleParticles();
        }
        
        if (createOuterParticles)
        {
            CreateOuterParticles();
        }
        
        if (timer >= DisplayDuration)
        {
            NPC.active = false;
            NPC.netUpdate = true;
            return;
        }

        if (Main.netMode != NetmodeID.Server)
        {
            UpdateMouseHover();
            
            if (hoveredItemIndex >= 0 && Main.mouseLeft && Main.mouseLeftRelease)
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    ProcessItemSelection(hoveredItemIndex);
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)2);
                    packet.Write7BitEncodedInt(NPC.whoAmI);
                    packet.Write7BitEncodedInt(hoveredItemIndex);
                    packet.Send();
                    ProcessItemSelection(hoveredItemIndex);
                }
            }
        }
    }
    
    private void UpdateWormMotion()
    {
        float centerX = NPC.Center.X;
        float waveTime = timer * waveSpeed;
        
        for (int i = 0; i < 5; i++)
        {
            float phase = wavePhases[i] + waveTime;
            float xOffset = (float)Math.Sin(phase) * horizontalAmplitude;
            float yOffset = (float)Math.Cos(phase) * 8f;
            
            itemPositions[i] = new Vector2(
                centerX + xOffset,
                baseYPositions[i] + yOffset
            );
            
            wavePhases[i] += 0.0008f;
        }
    }
    
    // LIKE CRIMSONHEART: CreateWormBodyParticles (instead of pentagram)
    private void CreateWormBodyParticles()
    {
        // Create particles along the connecting lines between items
        for (int i = 0; i < 4; i++)
        {
            Vector2 start = itemPositions[i];
            Vector2 end = itemPositions[i + 1];
            Vector2 direction = (end - start).SafeNormalize(Vector2.UnitX);
            
            // Like CrimsonHeart: 12 particles per line
            int particlesPerLine = 12;
            
            for (int p = 0; p < particlesPerLine; p++)
            {
                float t = (p * 0.0833f) % 1f; // Evenly spaced
                Vector2 position = Vector2.Lerp(start, end, t);
                
                // Check if too close to any item
                bool tooClose = false;
                for (int itemCheck = 0; itemCheck < 5; itemCheck++)
                {
                    if (!itemActive[itemCheck]) continue;
                    if (Vector2.Distance(position, itemPositions[itemCheck]) < ItemCircleRadius)
                    {
                        tooClose = true;
                        break;
                    }
                }
                
                if (tooClose) continue;
                
                // LIKE CRIMSONHEART: Simple dust type based on position
                int dustType = (i + p) % 2 == 0 ? 75 : 118; // Alternate purple/green
                
                Dust lineDust = Dust.NewDustPerfect(
                    position,
                    dustType,
                    direction * 0.8f,
                    0,
                    default,
                    1.4f
                );
                lineDust.noGravity = true;
                lineDust.velocity *= 0.5f;
                lineDust.alpha = 50;
            }
        }
    }
    
    // LIKE CRIMSONHEART: CreateItemCircleParticles
    private void CreateItemCircleParticles()
    {
        int activeCount = 0;
        for (int i = 0; i < 5; i++) if (itemActive[i]) activeCount++;
        if (activeCount == 0) return;
        
        // LIKE CRIMSONHEART: 20 particles per circle
        int circleParticles = 20;
        float radius = 30f;
        
        for (int p = 0; p < circleParticles; p++)
        {
            float baseAngle = p * (MathHelper.TwoPi / circleParticles);
            
            for (int itemIndex = 0; itemIndex < 5; itemIndex++)
            {
                if (!itemActive[itemIndex]) continue;
                
                Vector2 itemPos = itemPositions[itemIndex];
                
                Vector2 position = itemPos + new Vector2(
                    (float)Math.Cos(baseAngle) * radius,
                    (float)Math.Sin(baseAngle) * radius
                );
                
                Vector2 tangent = new Vector2(-(float)Math.Sin(baseAngle), (float)Math.Cos(baseAngle));
                
                // LIKE CRIMSONHEART: Simple size and speed
                float particleSize = 1.5f;
                float particleSpeed = 0.7f;
                
                // Like CrimsonHeart: Special treatment for hovered item
                if (itemIndex == hoveredItemIndex)
                {
                    particleSize = 2.0f;
                    particleSpeed = 1.4f;
                }
                
                // LIKE CRIMSONHEART: Alternate dust types for fair distribution
                int dustType = (itemIndex + p) % 2 == 0 ? 75 : 118;
                
                Dust dust = Dust.NewDustPerfect(
                    position,
                    dustType,
                    tangent * particleSpeed,
                    0,
                    default,
                    particleSize
                );
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }
        }
    }
    
    // LIKE CRIMSONHEART: CreateOuterParticles
    private void CreateOuterParticles()
    {
        Vector2 center = NPC.Center;
        float outerRadius = 150f; // Bigger for worm formation
        int circleParticles = 40;
        
        for (int i = 0; i < circleParticles; i++)
        {
            float angle = i * (MathHelper.TwoPi / circleParticles);
            
            Vector2 position = center + new Vector2(
                (float)Math.Cos(angle) * outerRadius,
                (float)Math.Sin(angle) * outerRadius
            );
            
            Vector2 tangent = new Vector2(-(float)Math.Sin(angle), (float)Math.Cos(angle));
            
            // Find nearest item
            int nearestItemIndex = -1;
            float nearestItemDistance = float.MaxValue;
            
            for (int itemIndex = 0; itemIndex < 5; itemIndex++)
            {
                if (!itemActive[itemIndex]) continue;
                
                float distance = Vector2.Distance(position, itemPositions[itemIndex]);
                if (distance < nearestItemDistance)
                {
                    nearestItemDistance = distance;
                    nearestItemIndex = itemIndex;
                }
            }
            
            // Create particle
            float particleSize = 1.8f;
            Vector2 particleVelocity = tangent * 0.8f;
            
            // LIKE CRIMSONHEART: Check if too close to items
            if (nearestItemIndex >= 0 && nearestItemDistance < ItemCircleRadius + 20f)
            {
                // Don't create particles inside item circles
                continue;
            }
            
            // LIKE CRIMSONHEART: Simple dust type based on angle
            int dustType = i % 2 == 0 ? 75 : 118;
            
            Dust circleDust = Dust.NewDustPerfect(
                position,
                dustType,
                particleVelocity,
                0,
                default,
                particleSize
            );
            circleDust.noGravity = true;
            circleDust.velocity *= 0.3f;
            circleDust.alpha = 0;
        }
        
        // LIKE CRIMSONHEART: Add golden sparkles (outside item circles only)
        if (timer % 5 == 0)
        {
            for (int s = 0; s < 5; s++)
            {
                float sparkleAngle = s * MathHelper.PiOver2;
                float sparkleRadius = outerRadius + (float)Math.Sin(timer * 0.1f + s) * 10f;
                
                Vector2 sparklePos = center + new Vector2(
                    (float)Math.Cos(sparkleAngle) * sparkleRadius,
                    (float)Math.Sin(sparkleAngle) * sparkleRadius
                );
                
                // Check if sparkle is too close to any item
                bool tooClose = false;
                for (int itemIndex = 0; itemIndex < 5; itemIndex++)
                {
                    if (!itemActive[itemIndex]) continue;
                    if (Vector2.Distance(sparklePos, itemPositions[itemIndex]) < ItemCircleRadius + 20f)
                    {
                        tooClose = true;
                        break;
                    }
                }
                
                if (tooClose) continue;
                
                // Use corruption-themed dust
                int sparkleDustType = s % 2 == 0 ? 75 : 118;
                
                Dust sparkle = Dust.NewDustPerfect(
                    sparklePos,
                    sparkleDustType,
                    Vector2.Zero,
                    0,
                    default,
                    2.5f
                );
                sparkle.noGravity = true;
                sparkle.alpha = 30;
            }
        }
    }

    private void UpdateMouseHover()
    {
        hoveredItemIndex = -1;
        Vector2 mouseWorld = Main.MouseWorld;
        
        for (int i = 0; i < 5; i++)
        {
            if (!itemActive[i]) continue;
                
            float distance = Vector2.Distance(mouseWorld, itemPositions[i]);
            
            if (distance < MouseHoverRange)
            {
                hoveredItemIndex = i;
                break;
            }
        }
    }

    private void ProcessItemSelection(int selectedIndex)
    {
        // LIKE CRIMSONHEART: Fair explosion distribution
        for (int i = 0; i < 80; i++)
        {
            float angle = i * MathHelper.TwoPi / 80f;
            Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5f;
            Vector2 position = itemPositions[selectedIndex] + velocity * 2f;
            
            // LIKE CRIMSONHEART: Alternate dust types
            int dustType = i % 2 == 0 ? 75 : 118;
            
            Dust dust = Dust.NewDustPerfect(
                position,
                dustType,
                velocity,
                0,
                default,
                2.5f
            );
            dust.noGravity = true;
            dust.velocity *= 0.7f;
        }
        
        GiveItemToPlayer(Main.LocalPlayer, itemIds[selectedIndex]);
        
        for (int j = 0; j < 5; j++) itemActive[j] = false;
        
        SoundEngine.PlaySound(SoundID.NPCDeath13 with { Pitch = -0.3f, Volume = 1.0f }, itemPositions[selectedIndex]);
        
        NPC.active = false;
        NPC.netUpdate = true;
    }

    public void GiveItemToPlayer(Player player, int itemId)
    {
        int itemIndex = Item.NewItem(new EntitySource_WorldEvent(), player.getRect(), itemId, 1, noBroadcast: false);

        if (Main.netMode == NetmodeID.Server && itemIndex >= 0)
        {
            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex, 1f);
        }
        
        if (Main.netMode == NetmodeID.MultiplayerClient && itemIndex >= 0)
        {
            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex, 1f);
        }

        // LIKE CRIMSONHEART: Fair pickup effect
        for (int i = 0; i < 50; i++)
        {
            float angle = i * MathHelper.TwoPi / 50f;
            Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
            
            int dustType = i % 2 == 0 ? 75 : 118;
            
            Dust dust = Dust.NewDustPerfect(
                player.Center,
                dustType,
                velocity,
                0,
                default,
                2.0f
            );
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (Main.netMode == NetmodeID.Server) return;

        // LIKE CRIMSONHEART: Draw connecting lines first
        DrawWormBody(spriteBatch);
        DrawItems(spriteBatch);
    }
    
    // LIKE CRIMSONHEART: DrawWormBody (instead of pentagram)
    private void DrawWormBody(SpriteBatch spriteBatch)
    {
        bool allActive = true;
        for (int i = 0; i < 5; i++) if (!itemActive[i]) allActive = false;
        if (!allActive) return;
            
        Texture2D pixel = ModContent.Request<Texture2D>("Terraria/Images/Extra_0").Value;
        
        // Draw lines between consecutive items (worm body)
        for (int i = 0; i < 4; i++)
        {
            Vector2 start = itemPositions[i] - Main.screenPosition;
            Vector2 end = itemPositions[i + 1] - Main.screenPosition;
            
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            
            // LIKE CRIMSONHEART: Simple pulsing glow
            float glow = 0.5f + 0.3f * (float)Math.Sin(timer * 0.1f + i);
            
            // Color fades from purple (head) to green (tail)
            float colorFade = i / 4f;
            Color lineColor = Color.Lerp(
                new Color(120, 50, 180, 150), // Purple
                new Color(50, 100, 40, 150),  // Green
                colorFade
            ) * glow;
            
            // LIKE CRIMSONHEART: Draw thick line
            spriteBatch.Draw(pixel,
                new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), 5),
                null,
                lineColor,
                angle,
                new Vector2(0, 0.5f),
                SpriteEffects.None,
                0);
        }
    }
    
    // LIKE CRIMSONHEART: DrawItems
    private void DrawItems(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < 5; i++)
        {
            if (!itemActive[i]) continue;

            Main.instance.LoadItem(itemIds[i]);
            Texture2D texture = Terraria.GameContent.TextureAssets.Item[itemIds[i]].Value;
            
            if (texture == null || texture.IsDisposed) 
            {
                texture = Terraria.GameContent.TextureAssets.Item[ItemID.ShadowOrb].Value;
            }

            Vector2 position = itemPositions[i] - Main.screenPosition;

            // LIKE CRIMSONHEART: Bobbing motion
            float bob = (float)Math.Sin(timer * 0.05f + i) * 6f;
            position.Y += bob;

            // LIKE CRIMSONHEART: Simple pulsing
            float pulse = 0.9f + 0.1f * (float)Math.Sin(timer * 0.1f);
            
            // LIKE CRIMSONHEART: Glow effect
            for (int g = 0; g < 4; g++)
            {
                float glowSize = 1.5f + g * 0.25f;
                float glowAlpha = 0.25f - g * 0.05f;
                
                spriteBatch.Draw(
                    texture,
                    position,
                    null,
                    Color.White * glowAlpha,
                    0f,
                    texture.Size() * 0.5f,
                    glowSize,
                    SpriteEffects.None,
                    0f
                );
            }
            
            // Draw main item
            spriteBatch.Draw(
                texture,
                position,
                null,
                Color.White * pulse,
                0f,
                texture.Size() * 0.5f,
                1.2f,
                SpriteEffects.None,
                0f
            );

            // LIKE CRIMSONHEART: Hover effect
            if (i == hoveredItemIndex)
            {
                float hoverPulse = (float)Math.Sin(timer * 0.25f) * 0.3f + 0.7f;
                spriteBatch.Draw(
                    texture,
                    position,
                    null,
                    Color.Lerp(Color.Gold, Color.White, 0.5f) * hoverPulse,
                    0f,
                    texture.Size() * 0.5f,
                    1.35f,
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(timer);
        writer.Write(horizontalAmplitude);
        writer.Write(verticalSpacing);
        writer.Write(waveSpeed);
        writer.Write(waveLength);
        writer.Write(createWormBodyParticles);
        writer.Write(createItemCircleParticles);
        writer.Write(createOuterParticles);
        for (int i = 0; i < 5; i++)
        {
            writer.WriteVector2(itemPositions[i]);
            writer.Write(baseYPositions[i]);
            writer.Write(wavePhases[i]);
            writer.Write7BitEncodedInt(itemIds[i]);
            writer.Write(itemActive[i]);
        }
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        timer = reader.ReadInt32();
        horizontalAmplitude = reader.ReadSingle();
        verticalSpacing = reader.ReadSingle();
        waveSpeed = reader.ReadSingle();
        waveLength = reader.ReadSingle();
        createWormBodyParticles = reader.ReadBoolean();
        createItemCircleParticles = reader.ReadBoolean();
        createOuterParticles = reader.ReadBoolean();
        for (int i = 0; i < 5; i++)
        {
            itemPositions[i] = reader.ReadVector2();
            baseYPositions[i] = reader.ReadSingle();
            wavePhases[i] = reader.ReadSingle();
            itemIds[i] = reader.Read7BitEncodedInt();
            itemActive[i] = reader.ReadBoolean();
        }
    }

    public override bool CheckActive() => false;
}