using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ultraconyx.Content.Projectiles;

public class SlimeWhipProj : ModProjectile
{
    public override void SetStaticDefaults()
    {
        // This makes the projectile use whip collision detection
        ProjectileID.Sets.IsAWhip[Type] = true;
    }

    public override void SetDefaults()
    {
        // Use the whip defaults
        Projectile.DefaultToWhip();

        // Customize whip properties
        Projectile.WhipSettings.Segments = 20;
        Projectile.WhipSettings.RangeMultiplier = 1f;
        
        // Additional properties
        Projectile.extraUpdates = 1; // Important for whip smoothness
    }

    private float Timer
    {
        get => Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    private float ChargeTime
    {
        get => Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        
        // Get whip settings
        Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
        
        // Check for whip tip position for slime ignition
        if (Timer > timeToFlyOut * 0.3f && Timer < timeToFlyOut * 0.7f)
        {
            // Get whip tip position
            List<Vector2> whipPoints = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, whipPoints);
            
            if (whipPoints.Count > 0)
            {
                Vector2 whipTip = whipPoints[whipPoints.Count - 1];
                CheckAndIgniteSlimeDust(whipTip);
            }
        }
        
        // Check for flamethrower ignition
        CheckForFlamethrowerIgnition(owner);
    }

    private void CheckForFlamethrowerIgnition(Player player)
    {
        // Check if player is using a flamethrower
        if (player.HeldItem.type == ItemID.Flamethrower || player.HeldItem.shoot == ProjectileID.Flames)
        {
            Vector2 mousePos = Main.MouseWorld;
            
            // Check for slime dust near mouse cursor
            for (int i = 0; i < Main.maxDust; i++)
            {
                Dust dust = Main.dust[i];
                if (dust.active && dust.type == DustID.t_Slime && Vector2.Distance(dust.position, mousePos) < 60f)
                {
                    IgniteSlimeDust(dust);
                }
            }
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        // Set minion target
        Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
        
        // 30% chance to ignite enemy
        if (Main.rand.NextFloat() < 0.3f)
        {
            target.AddBuff(BuffID.OnFire, 180); // 3 seconds
        }
        
        // Create slime trail at hit location
        CreateSlimeTrail(target.Center);
        
        // Play hit sound
        SoundEngine.PlaySound(SoundID.NPCHit1 with { Volume = 0.5f }, target.Center);
    }

    private void CreateSlimeTrail(Vector2 position)
    {
        // Create slime dust particles (DustID.t_Slime = 56)
        for (int i = 0; i < 5; i++)
        {
            Vector2 offset = new Vector2(Main.rand.NextFloat(-20f, 20f), Main.rand.NextFloat(-10f, 10f));
            Dust slimeDust = Dust.NewDustDirect(position + offset, 10, 10, 56, 0f, 0f, 100, new Color(0, 255, 0, 100), 1.5f);
            slimeDust.noGravity = false;
            slimeDust.velocity *= 0.3f;
            slimeDust.fadeIn = 1.2f;
            
            // Add slight upward movement
            slimeDust.velocity.Y -= 0.5f;
        }
    }

    private void CheckAndIgniteSlimeDust(Vector2 position)
    {
        // Check for slime dust near the position
        for (int i = 0; i < Main.maxDust; i++)
        {
            Dust dust = Main.dust[i];
            if (dust.active && dust.type == 56 && Vector2.Distance(dust.position, position) < 30f)
            {
                IgniteSlimeDust(dust);
            }
        }
    }

    private void IgniteSlimeDust(Dust dust)
    {
        // Create fire particles
        for (int i = 0; i < 8; i++)
        {
            Dust fireDust = Dust.NewDustDirect(dust.position, 10, 10, DustID.Torch, 0f, 0f, 100, default, 2f);
            fireDust.noGravity = true;
            fireDust.velocity = Main.rand.NextVector2Circular(3f, 3f);
        }
        
        // Create smoke effect
        for (int i = 0; i < 5; i++)
        {
            Dust smokeDust = Dust.NewDustDirect(dust.position - new Vector2(15, 15), 30, 30, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
            smokeDust.velocity = Main.rand.NextVector2Circular(2f, 2f);
            smokeDust.scale *= 0.8f;
        }
        
        // Play ignition sound
        SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.3f, Pitch = 0.5f }, dust.position);
        
        // Damage nearby enemies
        Rectangle explosionRect = new Rectangle((int)dust.position.X - 40, (int)dust.position.Y - 40, 80, 80);
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (npc.active && !npc.friendly && npc.life > 0 && npc.CanBeChasedBy())
            {
                Rectangle npcRect = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
                if (explosionRect.Intersects(npcRect))
                {
                    // Calculate damage (half of whip damage)
                    int damage = Projectile.damage / 2;
                    if (damage < 1) damage = 1;
                    
                    // Apply damage
                    npc.StrikeNPC(new NPC.HitInfo
                    {
                        Damage = damage,
                        Knockback = 2f,
                        HitDirection = npc.position.X < dust.position.X ? 1 : -1,
                        SourceDamage = Projectile.damage,
                        DamageType = DamageClass.SummonMeleeSpeed
                    });
                    
                    // Apply fire debuff
                    npc.AddBuff(BuffID.OnFire, 120); // 2 seconds
                }
            }
        }
        
        // Remove the slime dust
        dust.active = false;
    }

    // This method draws a line between all points of the whip
    private void DrawLine(List<Vector2> list)
    {
        Texture2D texture = TextureAssets.FishingLine.Value;
        Rectangle frame = texture.Frame();
        Vector2 origin = new Vector2(frame.Width / 2, 2);

        Vector2 pos = list[0];
        for (int i = 0; i < list.Count - 1; i++)
        {
            Vector2 element = list[i];
            Vector2 diff = list[i + 1] - element;

            float rotation = diff.ToRotation() - MathHelper.PiOver2;
            Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.White);
            Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

            pos += diff;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        List<Vector2> list = new List<Vector2>();
        Projectile.FillWhipControlPoints(Projectile, list);

        // Draw the line between whip segments (optional)
        DrawLine(list);

        // Use vanilla whip drawing or custom drawing
        // Main.DrawWhip_WhipBland(Projectile, list); // Uncomment for vanilla drawing
        
        // Custom drawing for more control
        SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        Texture2D texture = TextureAssets.Projectile[Type].Value;

        Vector2 pos = list[0];

        for (int i = 0; i < list.Count - 1; i++)
        {
            // Frame setup for whip segments
            Rectangle frame = new Rectangle(0, 0, 10, 26); // Base frame size
            Vector2 origin = new Vector2(5, 8);
            float scale = 1;

            // Handle different segments
            if (i == list.Count - 2)
            {
                // Whip tip
                frame.Y = 74;
                frame.Height = 18;
                
                // Scale tip based on whip extension
                Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                float t = Timer / timeToFlyOut;
                scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
            }
            else if (i > 10)
            {
                // Third segment
                frame.Y = 58;
                frame.Height = 16;
            }
            else if (i > 5)
            {
                // Second segment
                frame.Y = 42;
                frame.Height = 16;
            }
            else if (i > 0)
            {
                // First segment
                frame.Y = 26;
                frame.Height = 16;
            }

            Vector2 element = list[i];
            Vector2 diff = list[i + 1] - element;

            float rotation = diff.ToRotation() - MathHelper.PiOver2;
            Color color = Lighting.GetColor(element.ToTileCoordinates());

            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

            pos += diff;
        }
        
        return false;
    }
}