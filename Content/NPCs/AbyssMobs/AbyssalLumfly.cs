using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.NPCs.AbyssMobs;

public class AbyssalLumfly : ModNPC
{
    private int frame;
    private int frameCounter;
    private const int FrameSpeed = 4;
    
    // For random wandering
    private Vector2 wanderTarget;
    private int wanderTimer;
    private const int MaxWanderTime = 120;
    
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 4;
        NPCID.Sets.CountsAsCritter[Type] = true;
    }
    
    public override void SetDefaults()
    {
        NPC.width = 20;
        NPC.height = 20;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 5;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.value = 25f;
        NPC.knockBackResist = 1f;
        NPC.noGravity = true;
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
    }
    
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("A peaceful glowing insect that floats aimlessly through the Abysslight caverns. Completely indifferent to travelers."));
    }
    
    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        if (IsInAbyssalOvalBiome(spawnInfo.Player))
        {
            return 1.2f;
        }
        return 0f;
    }
    
    private bool IsInAbyssalOvalBiome(Player player)
    {
        if (Content.Biomes.HellevatorGen.BiomeExists)
        {
            Microsoft.Xna.Framework.Point biomeCenter = Content.Biomes.HellevatorGen.BiomeCenter;
            float distance = Vector2.Distance(player.Center / 16f, new Vector2(biomeCenter.X, biomeCenter.Y));
            
            // Bigger check radius for 4x bigger biome
            return distance < 180f; // Was 120f, now larger for bigger biome
        }
        return false;
    }
    
    public override void AI()
    {
        // COMPLETELY CHILL - NO INTERACTION WITH PLAYER AT ALL
        
        // Simple random wandering AI
        wanderTimer++;
        
        // Pick new wander target when timer runs out
        if (wanderTimer >= MaxWanderTime || wanderTarget == Vector2.Zero)
        {
            PickNewWanderTarget();
            wanderTimer = 0;
        }
        
        // Move toward wander target
        if (wanderTarget != Vector2.Zero)
        {
            Vector2 direction = wanderTarget - NPC.Center;
            float distance = direction.Length();
            
            if (distance > 10f) // Only move if not at target
            {
                direction.Normalize();
                
                // Gentle, slow movement toward target
                float moveSpeed = 0.6f;
                NPC.velocity += direction * moveSpeed;
                
                // Limit speed
                if (NPC.velocity.Length() > 1.5f)
                {
                    NPC.velocity = Vector2.Normalize(NPC.velocity) * 1.5f;
                }
            }
        }
        
        // Add occasional random flutters
        if (Main.rand.NextBool(60))
        {
            // Tiny random flutter
            NPC.velocity.X += Main.rand.NextFloat(-0.15f, 0.15f);
            NPC.velocity.Y += Main.rand.NextFloat(-0.1f, 0.1f);
        }
        
        // Very gentle floating
        NPC.velocity.Y -= 0.02f;
        
        // Natural slowdown
        NPC.velocity *= 0.98f;
        
        // Keep within biome boundaries (optional - prevents drifting out)
        KeepInBiomeBounds();
        
        // Gentle bounce off walls
        if (NPC.collideX)
            NPC.velocity.X = -NPC.velocity.X * 0.6f;
        if (NPC.collideY)
            NPC.velocity.Y = -NPC.velocity.Y * 0.6f;
        
        // Animation
        frameCounter++;
        if (frameCounter >= FrameSpeed)
        {
            frameCounter = 0;
            frame = (frame + 1) % 4;
        }
        
        // Face direction
        NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        
        // Emit light
        AddLight();
        
        // Very occasional tiny sparkle
        if (Main.rand.NextBool(80) && NPC.velocity.Length() > 0.3f)
        {
            Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, 
                DustID.GemEmerald, 0f, 0f, 100, default(Color), 0.2f);
            dust.noGravity = true;
            dust.velocity = NPC.velocity * -0.05f;
        }
    }
    
    private void PickNewWanderTarget()
    {
        if (Content.Biomes.HellevatorGen.BiomeExists)
        {
            Point center = Content.Biomes.HellevatorGen.BiomeCenter;
            
            // Pick random point within biome (accounting for 4x bigger size)
            float maxRadius = 100f; // Approximate biome radius in tiles
            float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
            float distance = Main.rand.NextFloat(20f, maxRadius * 0.7f); // Stay within 70% of biome
            
            wanderTarget = new Vector2(
                center.X + (float)Math.Cos(angle) * distance,
                center.Y + (float)Math.Sin(angle) * distance
            ) * 16f; // Convert to pixel coordinates
        }
    }
    
    private void KeepInBiomeBounds()
    {
        if (Content.Biomes.HellevatorGen.BiomeExists)
        {
            Point center = Content.Biomes.HellevatorGen.BiomeCenter;
            Vector2 biomeCenterPixels = new Vector2(center.X, center.Y) * 16f;
            float distanceFromCenter = Vector2.Distance(NPC.Center, biomeCenterPixels);
            
            // If too far from center, gently nudge back
            float maxDistance = 120f * 16f; // 120 tiles in pixels
            if (distanceFromCenter > maxDistance * 0.8f) // 80% of max distance
            {
                Vector2 directionToCenter = biomeCenterPixels - NPC.Center;
                directionToCenter.Normalize();
                NPC.velocity += directionToCenter * 0.1f;
            }
        }
    }
    
    private void AddLight()
    {
        // Soft, steady green light (no pulsing)
        Lighting.AddLight(NPC.Center, 
            0.08f,  // Red
            0.35f,  // Green
            0.08f); // Blue
    }
    
    public override void FindFrame(int frameHeight)
    {
        NPC.frame.Y = frame * frameHeight;
    }
    
    public override Color? GetAlpha(Color drawColor)
    {
        return new Color(180, 255, 180, drawColor.A);
    }
    
    public override void HitEffect(NPC.HitInfo hit)
    {
        // Minimal reaction
        for (int i = 0; i < 5; i++)
        {
            Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, 
                DustID.GemEmerald, hit.HitDirection * 0.2f, -0.3f, 100, default(Color), 0.6f);
            dust.noGravity = true;
            dust.velocity *= 0.8f;
        }
    }
}