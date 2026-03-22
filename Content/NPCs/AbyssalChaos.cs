using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.NPCs;

public class AbyssalChaos : ModNPC
{
    // AI States
    private enum AIState
    {
        Spawn,
        ShowMessage,
        ApplyBlindness,
        Disappear
    }
    
    private AIState CurrentState
    {
        get => (AIState)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }
    
    private ref float Timer => ref NPC.ai[1];
    private ref float Alpha => ref NPC.ai[2]; // For fade effects
    
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 1;
        NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
        {
            Hide = true // Hide from bestiary
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        
        // Make it completely non-attackable
        NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = false;
        NPCID.Sets.CantTakeLunchMoney[Type] = true;
    }
    
    public override void SetDefaults()
    {
        NPC.width = 80;
        NPC.height = 80;
        NPC.lifeMax = 1;
        NPC.life = 1;
        NPC.defense = 0;
        NPC.damage = 0;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;
        AIType = -1;
        NPC.alpha = 0;
        NPC.dontTakeDamage = true; // CRITICAL: Can't take damage
        NPC.immortal = true; // CRITICAL: Immortal
        NPC.friendly = false;
        NPC.chaseable = false; // CRITICAL: Not chaseable by enemies
        NPC.value = 0f;
        
        // Make sure it's visible and stays on screen
        NPC.ShowNameOnHover = false;
    }
    
    public override void AI()
    {
        // Initialize on first frame
        if (CurrentState == 0 && Timer == 0)
        {
            CurrentState = AIState.Spawn;
            Timer = 0;
            Alpha = 0f;
            
            // Force spawn at player position
            Player targetPlayer = Main.player[NPC.target];
            if (targetPlayer.active && !targetPlayer.dead)
            {
                NPC.Center = targetPlayer.Center + new Vector2(0, -150);
            }
        }
        
        // Always target the nearest player
        NPC.TargetClosest(true);
        Player player = Main.player[NPC.target];
        
        if (!player.active || player.dead)
        {
            NPC.active = false;
            return;
        }
        
        // Stay centered above player (only when visible)
        if (CurrentState != AIState.Disappear)
        {
            Vector2 targetPosition = player.Center + new Vector2(0, -150);
            NPC.Center = Vector2.Lerp(NPC.Center, targetPosition, 0.05f);
        }
        
        Timer++;
        
        switch (CurrentState)
        {
            case AIState.Spawn:
                SpawnBehavior(player);
                break;
                
            case AIState.ShowMessage:
                ShowMessageBehavior(player);
                break;
                
            case AIState.ApplyBlindness:
                ApplyBlindnessBehavior(player);
                break;
                
            case AIState.Disappear:
                DisappearBehavior(player);
                break;
        }
        
        // Update alpha based on state
        switch (CurrentState)
        {
            case AIState.Spawn:
                Alpha = MathHelper.Lerp(Alpha, 1f, 0.1f);
                NPC.alpha = (int)((1f - Alpha) * 255);
                break;
                
            case AIState.Disappear:
                Alpha = MathHelper.Lerp(Alpha, 0f, 0.2f); // Faster fade out
                NPC.alpha = (int)((1f - Alpha) * 255);
                break;
                
            default:
                NPC.alpha = 0;
                break;
        }
        
        // Create particle effects when visible
        if (NPC.alpha < 200 && Timer % 3 == 0 && CurrentState != AIState.Disappear)
        {
            Dust dust = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.Shadowflame, 0f, 0f, 100, Color.DarkRed, 1.5f);
            dust.noGravity = true;
            dust.velocity = Main.rand.NextVector2Circular(2f, 2f);
        }
        
        // Auto-destruct after 15 seconds as safety
        if (Timer > 900)
        {
            NPC.active = false;
        }
    }
    
    private void SpawnBehavior(Player player)
    {
        if (Timer == 1)
        {
            // Spawn effects
            for (int i = 0; i < 30; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.Shadowflame, 0f, 0f, 100, Color.Black, 2.5f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }
            
            SoundEngine.PlaySound(SoundID.Item120, NPC.Center);
        }
        
        if (Timer > 40) // Shorter spawn time
        {
            CurrentState = AIState.ShowMessage;
            Timer = 0;
        }
    }
    
    private void ShowMessageBehavior(Player player)
    {
        if (Timer == 1)
        {
            // Show message in chat ONLY
            if (Main.netMode != NetmodeID.Server)
            {
                Main.NewText("Welcome to your doom...", Color.Red);
            }
            
            // Visual cue
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.Vortex, 0f, 0f, 100, Color.DarkRed, 2f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }
            
            SoundEngine.PlaySound(SoundID.Item115, NPC.Center);
        }
        
        if (Timer > 60) // Wait 1 second after message
        {
            CurrentState = AIState.ApplyBlindness;
            Timer = 0;
        }
    }
    
    private void ApplyBlindnessBehavior(Player player)
    {
        if (Timer == 1)
        {
            // Apply blindness debuff
            if (Main.netMode != NetmodeID.Server)
            {
                player.AddBuff(BuffID.Darkness, 180); // 3 seconds of darkness
                player.AddBuff(BuffID.Obstructed, 60); // 1 second of obstruction
                
                // Sound effect for blindness
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
            }
            
            // Visual effect for blindness application
            for (int i = 0; i < 50; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.Shadowflame, 0f, 0f, 100, Color.Black, 3f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(12f, 12f);
            }
            
            // Activate Chaos Mode
            ActivateChaosMode(player);
        }
        
        if (Timer > 20) // Wait 0.33 seconds, then disappear
        {
            CurrentState = AIState.Disappear;
            Timer = 0;
        }
    }
    
    private void DisappearBehavior(Player player)
    {
        if (Timer == 1)
        {
            // Disappear effects
            for (int i = 0; i < 40; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.Smoke, 0f, 0f, 100, Color.Black, 2.5f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }
            
            SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
            
            // Make sure NPC is fully invisible
            NPC.alpha = 255;
        }
        
        if (Timer > 180) // Wait 3 seconds after disappearing (blindness duration), then despawn
        {
            // Clear blindness buffs (player's vision returns naturally after duration)
            if (Main.netMode != NetmodeID.Server)
            {
                // Play low-pitched roar as blindness ends
                var lowRoarStyle = SoundID.ForceRoar with 
                { 
                    Pitch = -0.8f,
                    Volume = 1.0f,
                    PitchVariance = 0.05f
                };
                SoundEngine.PlaySound(lowRoarStyle, player.Center);
                
                // NO "Vision Restored" text - vision just returns naturally
            }
            
            NPC.active = false;
        }
    }
    
    private void ActivateChaosMode(Player player)
    {
        // This is where you would activate your Chaos Mode
        // Add your Chaos Mode activation logic here
        
        // Announcement
        if (Main.netMode != NetmodeID.Server)
        {
            Main.NewText("CHAOS MODE ACTIVATED!", Color.DarkRed);
        }
        
        // Placeholder - replace with your actual ChaosMode system
        player.AddBuff(BuffID.OnFire, 300);
    }
    
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        // Don't draw if fully invisible (alpha >= 255)
        if (NPC.alpha >= 255)
            return false;
        
        Texture2D texture = TextureAssets.Npc[Type].Value;
        Vector2 drawOrigin = new(texture.Width * 0.5f, texture.Height * 0.5f);
        Vector2 drawPos = NPC.Center - screenPos;
        
        // Simple solid draw
        Color color = Color.DarkRed * ((255f - NPC.alpha) / 255f);
        spriteBatch.Draw(texture, drawPos, null, color, NPC.rotation, drawOrigin, 1f, SpriteEffects.None, 0f);
        
        return false;
    }
    
    // Make sure it's completely non-attackable
    public override bool CanBeHitByNPC(NPC attacker)
    {
        return false;
    }
    
    public override bool? CanBeHitByProjectile(Projectile projectile)
    {
        return false;
    }
    
    public override bool? CanBeHitByItem(Player player, Item item)
    {
        return false;
    }
    
    public override bool CheckDead()
    {
        NPC.active = false;
        return false;
    }
    
    public override void OnKill()
    {
        NPC.active = false;
    }
}