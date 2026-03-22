using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Ultraconyx.Content.Items.Weapons.Mage;

namespace Ultraconyx.Content.Projectiles;

public class ShadeSkullProjectile : ModProjectile
{
    private class Afterimage
    {
        public Vector2 Position;
        public float Rotation;
        public float Scale;
        public float Alpha;
    }
    
    private readonly List<Afterimage> afterimages = [];
    
    // Orbit parameters - FIXED: Better orbit system
    private float orbitRadius = 100f;
    private float orbitSpeed = 0.08f;
    private float currentOrbitAngle;
    private Vector2 orbitCenter;
    private bool hasInitializedOrbit;

    // Homing parameters
    private bool isHoming;
    private NPC target;
    private float homingSpeed = 15f;
    private float maxHomingTurnSpeed = 0.15f;
    private float homingStrength = 0.1f;
    
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
    }

    public override void SetDefaults()
    {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 600;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.alpha = 50;
    }

    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        
        if (!owner.active || owner.dead)
        {
            Projectile.Kill();
            return;
        }

        SpawnDust();

        if (!isHoming)
        {
            // FIXED: Proper orbit initialization
            if (!hasInitializedOrbit)
            {
                InitializeOrbit(owner);
                hasInitializedOrbit = true;
            }
            
            OrbitAroundPlayer(owner);
            FindHomingTarget(owner);
        }
        else
        {
            HomeToTarget();
            AddAfterimage();
            
            if (target == null || !target.active || target.life <= 0)
            {
                ReturnToOrbit(owner);
            }
        }
        
        if (Projectile.velocity != Vector2.Zero)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        
        UpdateAfterimages();
    }
    
    private void InitializeOrbit(Player owner)
    {
        orbitCenter = owner.Center;
        // Give each projectile a different starting angle for better distribution
        currentOrbitAngle = Projectile.ai[0] * (MathHelper.TwoPi / 8f); // 8 possible positions
    }
    
    private void OrbitAroundPlayer(Player owner)
    {
        orbitCenter = owner.Center;
        currentOrbitAngle += orbitSpeed;
        
        // FIXED: Smooth orbit calculation
        Vector2 orbitOffset = new(
            (float)System.Math.Cos(currentOrbitAngle) * orbitRadius,
            (float)System.Math.Sin(currentOrbitAngle) * orbitRadius
        );
        
        // Smooth movement toward orbit position
        Vector2 targetPosition = orbitCenter + orbitOffset;
        Vector2 direction = targetPosition - Projectile.Center;
        float distance = direction.Length();
        
        if (distance > 10f)
        {
            direction.Normalize();
            Projectile.velocity = direction * MathHelper.Clamp(distance * 0.2f, 2f, 10f);
        }
        else
        {
            Projectile.velocity = Vector2.Zero;
            Projectile.Center = targetPosition;
        }
        
        // Face outward from player
        Projectile.rotation = currentOrbitAngle + MathHelper.PiOver2;
    }
    
    private void FindHomingTarget(Player owner)
    {
        float maxDetectDistance = 500f;
        
        NPC closestNPC = null;
        float closestDistance = maxDetectDistance;
        
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            
            if (npc.CanBeChasedBy() && npc.active && !npc.friendly && npc.life > 0)
            {
                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                
                if (distance < closestDistance && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1))
                {
                    closestDistance = distance;
                    closestNPC = npc;
                }
            }
        }
        
        if (closestNPC != null)
        {
            target = closestNPC;
            isHoming = true;
            SoundEngine.PlaySound(SoundID.Item8, Projectile.position);
        }
    }
    
    private void HomeToTarget()
    {
        if (target == null || !target.active)
        {
            isHoming = false;
            return;
        }
        
        Vector2 direction = target.Center - Projectile.Center;
        float distance = direction.Length();
        
        // Normalize direction
        if (distance > 0)
        {
            direction.Normalize();
        }
        
        // FIXED: Better homing with smoother turning
        Vector2 currentVelocity = Projectile.velocity;
        if (currentVelocity == Vector2.Zero)
        {
            currentVelocity = direction * homingSpeed;
        }
        
        // Steer toward target
        Vector2 desiredVelocity = direction * homingSpeed;
        Vector2 steer = desiredVelocity - currentVelocity;
        steer = Vector2.Clamp(steer, new Vector2(-maxHomingTurnSpeed), new Vector2(maxHomingTurnSpeed));
        
        Projectile.velocity = Vector2.Clamp(currentVelocity + steer, 
            new Vector2(-homingSpeed), new Vector2(homingSpeed));
        
        // If very close to target, accelerate
        if (distance < 80f)
        {
            Projectile.velocity *= 1.2f;
        }
    }
    
    private void ReturnToOrbit(Player owner)
    {
        isHoming = false;
        target = null;
        hasInitializedOrbit = false; // Reset orbit so it finds new position
        
        // Move toward player to reset orbit
        Vector2 toPlayer = owner.Center - Projectile.Center;
        float distanceToPlayer = toPlayer.Length();
        
        if (distanceToPlayer > 50f)
        {
            Vector2 direction = toPlayer;
            direction.Normalize();
            Projectile.velocity = direction * homingSpeed * 0.5f;
        }
        else
        {
            // Close enough, stop and let orbit take over
            Projectile.velocity = Vector2.Zero;
        }
    }
    
    private void SpawnDust()
    {
        if (Main.rand.NextBool(3))
        {
            int dustType = Main.rand.NextBool() ? 53 : 54;
            
            Dust dust = Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                dustType,
                0f, 0f, 100, default(Color), 1.2f
            );
            dust.noGravity = true;
            dust.velocity = Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(0.5f, 0.5f);
            
            if (Main.rand.NextBool(5))
            {
                int otherDustType = dustType == 53 ? 54 : 53;
                Dust dust2 = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    otherDustType,
                    0f, 0f, 100, default(Color), 0.8f
                );
                dust2.noGravity = true;
                dust2.velocity = dust.velocity * 0.7f;
            }
        }
    }
    
    private void AddAfterimage()
    {
        if (Projectile.velocity.Length() > 5f)
        {
            Afterimage afterimage = new()
            {
                Position = Projectile.Center,
                Rotation = Projectile.rotation,
                Scale = Projectile.scale * 0.9f,
                Alpha = 0.7f
            };
            
            afterimages.Add(afterimage);
            
            if (afterimages.Count > 6)
            {
                afterimages.RemoveAt(0);
            }
        }
    }
    
    private void UpdateAfterimages()
    {
        for (int i = afterimages.Count - 1; i >= 0; i--)
        {
            afterimages[i].Alpha -= 0.08f;
            afterimages[i].Scale *= 0.95f;
            
            if (afterimages[i].Alpha <= 0f)
            {
                afterimages.RemoveAt(i);
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        Vector2 drawOrigin = texture.Size() / 2f;
        
        for (int i = 0; i < afterimages.Count; i++)
        {
            Afterimage afterimage = afterimages[i];
            Color afterimageColor = Color.Lerp(Color.White, Color.Gray, 0.5f) * afterimage.Alpha * 0.5f;
            afterimageColor.A = 0;
            
            Main.EntitySpriteDraw(
                texture,
                afterimage.Position - Main.screenPosition,
                null,
                afterimageColor,
                afterimage.Rotation,
                drawOrigin,
                afterimage.Scale,
                SpriteEffects.None,
                0
            );
        }
        
        Color mainColor = Color.Lerp(Color.White, Color.Gray, 0.3f);
        mainColor.A = (byte)(255 - Projectile.alpha);
        
        Main.EntitySpriteDraw(
            texture,
            Projectile.Center - Main.screenPosition,
            null,
            mainColor,
            Projectile.rotation,
            drawOrigin,
            Projectile.scale,
            SpriteEffects.None,
            0
        );
        
        return false;
    }
    
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        // FIXED: Charge Satan Power here (moved from weapon's ModifyHitNPC)
        Player owner = Main.player[Projectile.owner];
        
        // Only charge if the player is holding the ShadeSkull weapon
        if (owner.HeldItem.ModItem is ShadeSkull shadeSkull && owner.active)
        {
            if (!shadeSkull.satanMode && shadeSkull.satanPower < 100)
            {
                // Calculate 10% of damage dealt
                int damageToAdd = (int)(damageDone * 0.1f);
                
                shadeSkull.satanPower += damageToAdd;
                
                if (shadeSkull.satanPower > 100)
                    shadeSkull.satanPower = 100;

                // Show dark red combat text
                if (damageToAdd > 0)
                {
                    Rectangle textRect = new((int)target.Center.X - 20, (int)target.Center.Y - 40, 40, 20);
                    CombatText.NewText(textRect, Color.DarkRed, "+" + damageToAdd, true, false);
                }

                // Check if Satan Power reached 100%
                if (shadeSkull.satanPower >= 100)
                {
                    shadeSkull.satanMode = true;
                    shadeSkull.satanPower = 0;
                    shadeSkull.satanTimer = 300;
                    
                    // Visual effect at player position
                    for (int i = 0; i < 15; i++)
                    {
                        Dust dust = Dust.NewDustDirect(
                            owner.position, 
                            owner.width, 
                            owner.height, 
                            53,
                            0f, 0f, 100, default(Color), 1.5f
                        );
                        dust.noGravity = true;
                        dust.velocity *= 2f;
                    }
                    
                    SoundEngine.PlaySound(SoundID.Item100, owner.position);
                }
                // Flash effect when near 100%
                else if (shadeSkull.satanPower >= 90)
                {
                    if (shadeSkull.flashTimer % 20 < 10)
                    {
                        owner.AddBuff(BuffID.Battle, 2);
                    }
                }
            }
        }
        
        // Spawn dust effects
        for (int i = 0; i < 8; i++)
        {
            int dustType = Main.rand.NextBool() ? 53 : 54;
            Dust dust = Dust.NewDustDirect(
                target.position,
                target.width,
                target.height,
                dustType,
                0f, 0f, 100, default(Color), 1.5f
            );
            dust.noGravity = true;
            dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
        }
        
        Projectile.penetrate--;
        
        if (Projectile.penetrate <= 0)
        {
            ReturnToOrbit(owner);
            Projectile.penetrate = 3;
        }
    }
    
    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 15; i++)
        {
            int dustType = Main.rand.NextBool() ? 53 : 54;
            Dust dust = Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                dustType,
                Main.rand.NextFloat(-2f, 2f),
                Main.rand.NextFloat(-2f, 2f),
                100, default(Color), 1.5f
            );
            dust.noGravity = true;
        }
        
        SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
    }
}