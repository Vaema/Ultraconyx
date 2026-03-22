using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;

using Ultraconyx.Content.Projectiles.Bosses.StardustAngel;
using Ultraconyx.Content.Dusts;

namespace Ultraconyx.Content.NPCs.Bosses;

[AutoloadBossHead]
public class StardustAngel : ModNPC
{
    private enum AttackState
    {
        StardustBurst,
        StarRain,
        AngelDash,
        OrbitingStars,
        CelestialStar
    }

    private enum Phase
    {
        Phase1,
        Transforming,
        Phase2
    }

    private AttackState CurrentAttack
    {
        get => (AttackState)NPC.ai[0];
        set => NPC.ai[0] = (int)value;
    }

    private float AttackTimer
    {
        get => NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    private float TransformationTimer
    {
        get => NPC.ai[2];
        set => NPC.ai[2] = value;
    }

    private Phase CurrentPhase
    {
        get => (Phase)NPC.ai[3];
        set => NPC.ai[3] = (int)value;
    }

    private bool IsPhase2 => CurrentPhase == Phase.Phase2;
    private bool IsTransforming => CurrentPhase == Phase.Transforming;
    private Player Target => Main.player[NPC.target];

    private const int AttackDuration = 180;
    private const int TeleportCooldown = 300;
    private const int TransformationDuration = 2940; // 49 seconds (49 * 60)
    
    private int teleportTimer;
    private int teleportTelegraphTimer;
    private bool isTeleporting;
    private float teleportAlpha = 1f;
    
    private int[] orbitingStarIndices = new int[4];
    private float transparency = 1f;
    private int starRainCounter;
    private bool hasStartedTransformation;
    private int phase2StarCheckTimer;

    // Cooldown system.
    private int spawnCooldown;
    private const int SpawnCooldownMax = 30;
    
    // General movement variables.
    private float hoverOffset;
    private float floatSpeed = 0.02f;
    private float swayAmount = 40f;
    private int movementStyle;
    private int styleTimer;
    private Vector2 floatTarget = Vector2.Zero;
    private bool isPausing;
    private int pauseTimer;

    // Dash variables.
    private int dashSide;
    private Vector2 dashStartPosition = Vector2.Zero;
    private bool dashPreparing;
    private int dashPrepareTimer;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 1;
        
        NPCID.Sets.MPAllowedEnemies[NPC.type] = true;
        NPCID.Sets.BossBestiaryPriority.Add(NPC.type);
    }

    public override void SetDefaults()
    {
        NPC.width = 80;
        NPC.height = 80;
        NPC.damage = 145;
        NPC.defense = 60;
        NPC.lifeMax = 155000;
        NPC.knockBackResist = 0f;
        
        NPC.value = Item.buyPrice(gold: 10);
        NPC.npcSlots = 10f;
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.netAlways = true;

        NPC.HitSound = SoundID.NPCHit5;
        NPC.DeathSound = SoundID.NPCDeath7;

        NPC.aiStyle = -1;
        AIType = -1;
        CurrentPhase = Phase.Phase1;

        for (int i = 0; i < 4; i++)
            orbitingStarIndices[i] = -1;

        Music = MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/StardustAngel");
        SceneEffectPriority = SceneEffectPriority.BossHigh;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange(
        [
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Sky,
            new FlavorTextBestiaryInfoElement("A celestial being made of pure stardust. She is also the guardian of the night sky.")
        ]);
    }

    public override void AI()
    {
        // Find the nearest target.
        NPC.TargetClosest();

        // Despawn if all remaining targets are dead.
        if (!Target.active || Target.dead)
        {
            NPC.TargetClosest(false);
            if (!Target.active || Target.dead)
            {
                NPC.velocity.Y += 0.5f;
                
                if (NPC.timeLeft > 600)
                    NPC.timeLeft = 600;

                return;
            }
        }

        // Set music based on phase each frame to ensure it plays.
        if (CurrentPhase == Phase.Phase1)
            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/StardustAngel");
        else if (CurrentPhase == Phase.Transforming)
            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/StardustAngelPhase2");
        else if (CurrentPhase == Phase.Phase2)
            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/StardustAngelPhase3");

        if (CurrentPhase == Phase.Phase1 && NPC.life <= NPC.lifeMax / 2 && !hasStartedTransformation)
            StartTransformation();

        switch (CurrentPhase)
        {
            case Phase.Phase1:
                UpdatePhase1();
                break;
            case Phase.Transforming:
                UpdateTransformation();
                break;
            case Phase.Phase2:
                UpdatePhase2();
                break;
        }

        Lighting.AddLight(NPC.Center, 0.3f, 0.5f, 1f);

        if (Main.rand.NextBool(IsTransforming ? 20 : 10))
        {
            Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, ModContent.DustType<Stardust>(), 
                NPC.velocity.X * 0.1f, NPC.velocity.Y * 0.1f, 100, default, IsTransforming ? 1.5f : 1.2f);
            dust.noGravity = true;
            dust.alpha = IsTransforming ? 100 : 0;
        }
        
        // Decrease spawn cooldown.
        if (spawnCooldown > 0)
            spawnCooldown--;
    }

    private void StartTransformation()
    {
        hasStartedTransformation = true;
        CurrentPhase = Phase.Transforming;
        TransformationTimer = 0;
        NPC.damage = 0;
        NPC.defense = 999;
        NPC.dontTakeDamage = true;
        
        AttackTimer = 0;
        
        // Kill any existing orbiting stars.
        for (int i = 0; i < 4; i++)
        {
            if (orbitingStarIndices[i] != -1)
            {
                Projectile proj = Main.projectile[orbitingStarIndices[i]];
                if (proj.active)
                    proj.Kill();

                orbitingStarIndices[i] = -1;
            }
        }
        
        for (int i = 0; i < 30; i++)
        {
            Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, ModContent.DustType<Stardust>(), 0f, 0f, 100, default, 2.5f);
            dust.noGravity = true;
            dust.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
        }
        
        Main.NewText("The Stardust Angel begins to transform...", Color.LightBlue);
    }

    private void UpdatePhase1()
    {
        // Handle teleportation with telegraphs.
        teleportTimer++;
        if (!isTeleporting && teleportTimer >= TeleportCooldown && Main.rand.NextBool(300))
            StartTeleport();
        if (isTeleporting)
            UpdateTeleport();

        AttackTimer++;
        if (AttackTimer >= AttackDuration)
        {
            SelectNextAttack();
            AttackTimer = 0;
        }

        // Only handle regular movement if not dashing.
        if (CurrentAttack != AttackState.AngelDash || AttackTimer < 30)
            HandleMovement();

        switch (CurrentAttack)
        {
            case AttackState.StardustBurst:
                DoStardustBurst();
                break;
            case AttackState.StarRain:
                DoStarRain();
                break;
            case AttackState.AngelDash:
                DoAngelDash();
                break;
        }
        
        transparency = 1f;
    }

    private void StartTeleport()
    {
        isTeleporting = true;
        teleportTelegraphTimer = 0;
        teleportAlpha = 0.3f;
        
        // Spawn telegraph dust.
        for (int i = 0; i < 20; i++)
        {
            Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, ModContent.DustType<Stardust>(), NPC.velocity.X, NPC.velocity.Y, 100, default, 2f);
            dust.noGravity = true;
            dust.velocity *= 2f;
        }
    }

    private void UpdateTeleport()
    {
        teleportTelegraphTimer++;
        
        // Pulsing alpha effect.
        teleportAlpha = 0.3f + (float)Math.Sin(teleportTelegraphTimer * 0.2f) * 0.2f;
        
        // Spawn telegraph dust.
        if (teleportTelegraphTimer % 5 == 0)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 offset = new(
                    Main.rand.NextFloat(-40f, 40f),
                    Main.rand.NextFloat(-40f, 40f)
                );
                
                Dust dust = Dust.NewDustDirect(NPC.position + offset, NPC.width, NPC.height, ModContent.DustType<Stardust>(), 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }
        }
        
        // After half of a second, perform the actual teleport.
        if (teleportTelegraphTimer >= 30)
        {
            ExecuteTeleport();
            isTeleporting = false;
            teleportTimer = 0;
        }
    }

    private void ExecuteTeleport()
    {
        // Spawn the final dust burst at the old position.
        for (int i = 0; i < 25; i++)
        {
            Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, ModContent.DustType<Stardust>(), NPC.velocity.X, NPC.velocity.Y, 100, default, 2f);
            dust.noGravity = true;
            dust.velocity *= 3f;
        }

        // Randomly position around the target.
        Vector2 teleportPos = Target.Center + new Vector2(Main.rand.Next(-250, 250), Main.rand.Next(-200, 150));
        
        NPC.Center = teleportPos;
        NPC.velocity = Vector2.Zero;

        // Emit dust each teleport position.
        for (int i = 0; i < 25; i++)
        {
            Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, ModContent.DustType<Stardust>(), 0, 0, 100, default, 2f);
            dust.noGravity = true;
            dust.velocity *= 2f;
        }

        SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
    }

    private void UpdateTransformation()
    {        
        transparency = 0.3f + (float)Math.Sin(TransformationTimer * 0.1f) * 0.2f;
        
        // Do not move.
        NPC.velocity = Vector2.Zero;
        
        // Increment the star rain attack timer.
        starRainCounter++;
        
        // Star rain attack.
        if (starRainCounter % 15 == 0)
        {
            Vector2 position = new(Target.Center.X + Main.rand.Next(-600, 600), Target.Center.Y - 700);
            Vector2 velocity = new(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(8f, 11f));

            int star = Projectile.NewProjectile(NPC.GetSource_FromAI(), position, velocity, ModContent.ProjectileType<RainingStar>(), 70, 1f, Main.myPlayer);
            Main.projectile[star].friendly = false;
            Main.projectile[star].hostile = true;
        }
        
        // Shoot another star occasionally.
        if (starRainCounter % 45 == 0)
        {
            int side = Main.rand.NextBool(2) ? -1 : 1;
            Vector2 position = Target.Center + new Vector2(side * 800, Main.rand.Next(-300, 0));
            Vector2 velocity = new(-side * Main.rand.NextFloat(3f, 5f), Main.rand.NextFloat(5f, 7f));
            
            int star = Projectile.NewProjectile(NPC.GetSource_FromAI(), position, velocity, ModContent.ProjectileType<RainingStar>(), 70, 1f, Main.myPlayer);
            Main.projectile[star].friendly = false;
            Main.projectile[star].hostile = true;
        }

        // Enter phase two.
        TransformationTimer++;
        if (TransformationTimer >= TransformationDuration)
            EnterPhase2();
    }

    private void EnterPhase2()
    {
        CurrentPhase = Phase.Phase2;
        NPC.damage = 145;
        NPC.defense = 60;
        NPC.dontTakeDamage = false;
        transparency = 1f;
        
        // Set a spawn cooldown to prevent immediate respawn.
        spawnCooldown = SpawnCooldownMax;
        
        // Spawn the initial orbiting stars.
        SpawnOrbitingStars();
        
        for (int i = 0; i < 50; i++)
        {
            Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, ModContent.DustType<Stardust>(), 0f, 0f, 100, default, 3f);
            dust.noGravity = true;
            dust.velocity = new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f));
        }
        
        Main.NewText("The Stardust Angel has reached its final form!", Color.LightBlue);
    }

    private void SpawnOrbitingStars()
    {
        // Do not spawn if the cooldown is active.
        if (spawnCooldown > 0)
            return;
            
        // Kill any existing orbiting stars first.
        for (int i = 0; i < 4; i++)
        {
            if (orbitingStarIndices[i] != -1)
            {
                Projectile proj = Main.projectile[orbitingStarIndices[i]];
                if (proj.active && proj.type == ModContent.ProjectileType<AngelStar>())
                    proj.Kill();

                orbitingStarIndices[i] = -1;
            }
        }
        
        // Start a small delay to ensure clean slate.
        // Create four new orbiting stars at cardinal directions.
        for (int i = 0; i < 4; i++)
        {
            float angle = i * MathHelper.PiOver2;
            int star = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<AngelStar>(),
                NPC.damage / 3, 1f, Main.myPlayer, angle, NPC.whoAmI, i);
            orbitingStarIndices[i] = star;
            
            // Set the star's position in the array for easy reference.
            if (Main.projectile[star].ModProjectile is AngelStar starIndex)
                starIndex.SetStarIndex(i);
        }
        
        // Set the cooldown.
        spawnCooldown = SpawnCooldownMax;
    }

    private void UpdatePhase2()
    {
        // Handle any teleportation with telegraphs.
        teleportTimer++;
        if (!isTeleporting && teleportTimer >= TeleportCooldown / 2 && Main.rand.NextBool(200))
            StartTeleport();
        if (isTeleporting)
            UpdateTeleport();

        AttackTimer++;
        if (AttackTimer >= AttackDuration - 30)
        {
            SelectNextAttack();
            AttackTimer = 0;
        }

        // Only handle regular movement if the boss is not dashing.
        if (CurrentAttack != AttackState.AngelDash || AttackTimer < 30)
            HandleMovement();

        // Switch between attacks.
        switch (CurrentAttack)
        {
            case AttackState.StardustBurst:
                DoStardustBurst();
                break;
            case AttackState.StarRain:
                DoStarRain();
                break;
            case AttackState.AngelDash:
                DoAngelDash();
                break;
            case AttackState.OrbitingStars:
                DoOrbitingStars();
                break;
            case AttackState.CelestialStar:
                DoCelestialStar();
                break;
        }
        
        // Check for dead stars and respawn them.
        phase2StarCheckTimer++;
        if (phase2StarCheckTimer % 30 == 0)
            CheckAndRespawnStars();

        transparency = 1f;
    }
    
    private void CheckAndRespawnStars()
    {
        int aliveCount = 0;
        List<int> deadIndices = [];
        
        // Check each tracked position.
        for (int i = 0; i < 4; i++)
        {
            if (orbitingStarIndices[i] != -1)
            {
                Projectile proj = Main.projectile[orbitingStarIndices[i]];
                if (proj.active && proj.type == ModContent.ProjectileType<AngelStar>())
                    aliveCount++;
                else
                {
                    orbitingStarIndices[i] = -1;
                    deadIndices.Add(i);
                }
            }
            else
                deadIndices.Add(i);
        }
        
        // If we have any dead stars, respawn them.
        if (deadIndices.Count > 0 && spawnCooldown == 0)
        {
            // Kill any stray AngelStars that might exist but aren't tracked
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<AngelStar>() && proj.ai[1] == NPC.whoAmI)
                {
                    bool isTracked = false;
                    for (int j = 0; j < 4; j++)
                    {
                        if (orbitingStarIndices[j] == i)
                        {
                            isTracked = true;
                            break;
                        }
                    }
                    
                    // Otherwise, kill any untracked stars.
                    if (!isTracked)
                        proj.Kill();
                }
            }
            
            // Respawn all stars.
            SpawnOrbitingStars();
        }
    }

    private void HandleMovement()
    {
        if (IsTransforming || isTeleporting)
            return;
        
        hoverOffset += floatSpeed;
        styleTimer++;
        
        // Change movement style every few seconds.
        if (styleTimer > 180)
        {
            styleTimer = 0;
            movementStyle = Main.rand.Next(4);
            
            // Sometimes pause briefly.
            if (Main.rand.NextBool(3))
            {
                isPausing = true;
                pauseTimer = 30;
            }
        }
        
        // Handle pausing.
        if (isPausing)
        {
            pauseTimer--;
            if (pauseTimer <= 0)
                isPausing = false;
            else
            {
                // Slow down to a stop during a pause.
                NPC.velocity *= 0.95f;
                return;
            }
        }
        
        Vector2 basePosition = Target.Center + new Vector2(0, -200);
        Vector2 offset = Vector2.Zero;
        
        switch (movementStyle)
        {
            // Circular.
            case 0:
                offset = new Vector2((float)Math.Sin(hoverOffset * 0.3f) * swayAmount, (float)Math.Cos(hoverOffset * 0.3f) * (swayAmount * 0.5f));
                break;
            // Figure-eight.
            case 1:
                offset = new Vector2((float)Math.Sin(hoverOffset * 0.4f) * swayAmount, (float)Math.Sin(hoverOffset * 0.8f) * (swayAmount * 0.3f));
                break;
            // Side-to-side with a vertical float.
            case 2:
                offset = new Vector2((float)Math.Sin(hoverOffset * 0.2f) * swayAmount * 1.2f, (float)Math.Sin(hoverOffset * 0.5f) * 20f);
                break;
            // Aimlessly floating.
            case 3:
                if (floatTarget == Vector2.Zero || Vector2.Distance(NPC.Center, floatTarget) < 50f)
                    floatTarget = basePosition + new Vector2(Main.rand.Next(-150, 150), Main.rand.Next(-50, 50));

                Vector2 toTarget = floatTarget - NPC.Center;
                float distance = toTarget.Length();
                if (distance > 10f)
                {
                    toTarget.Normalize();
                    NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 3f, 0.02f);
                }

                break;
        }
        
        Vector2 targetPosition = basePosition + offset;
        Vector2 desiredVelocity = targetPosition - NPC.Center;
        float dist = desiredVelocity.Length();
        
        // Movement speed based on target distance.
        if (dist > 5f)
        {
            desiredVelocity.Normalize();
            float speed = MathHelper.Clamp(dist * 0.03f, 1.5f, IsPhase2 ? 4f : 3f);
            desiredVelocity *= speed;
        }
        else
            desiredVelocity = Vector2.Zero;

        NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, 0.03f);
        if (Main.rand.NextBool(20))
            NPC.velocity += new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.5f, 0.5f));

        // Tilt slightly when moving.
        float targetRotation = NPC.velocity.X * 0.02f;
        NPC.rotation = MathHelper.Lerp(NPC.rotation, targetRotation, 0.05f);
    }

    private void SelectNextAttack()
    {
        if (IsTransforming)
            return;
            
        var availableAttacks = new List<AttackState>();
        if (!IsPhase2)
        {
            availableAttacks.Add(AttackState.StardustBurst);
            availableAttacks.Add(AttackState.StarRain);
            availableAttacks.Add(AttackState.AngelDash);
        }
        else
        {
            availableAttacks.Add(AttackState.OrbitingStars);
            availableAttacks.Add(AttackState.CelestialStar);
            if (Main.rand.NextBool(2))
            {
                availableAttacks.Add(AttackState.StardustBurst);
                availableAttacks.Add(AttackState.StarRain);
                availableAttacks.Add(AttackState.AngelDash);
            }
        }

        CurrentAttack = availableAttacks[Main.rand.Next(availableAttacks.Count)];
    }

    private void DoStardustBurst()
    {
        if (AttackTimer == 30 && !IsTransforming && !isTeleporting)
        {
            SoundEngine.PlaySound(SoundID.Item9, NPC.Center);
            
            int numProjectiles = IsPhase2 ? 16 : 12;
            float radius = 80f;
            for (int i = 0; i < numProjectiles; i++)
            {
                float angle = (i / (float)numProjectiles) * MathHelper.TwoPi;
                Vector2 offset = new((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
                int star = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset, Vector2.Zero,
                    ModContent.ProjectileType<AngelMiniStar>(), NPC.damage / 3, 1f, Main.myPlayer, angle, 0f, 0f);
                
                if (Main.projectile[star].ModProjectile is AngelMiniStar starIndex)
                    starIndex.SetCircleTarget(Target.Center, NPC.Center, radius, angle, 60);
            }
        }
    }

    private void DoStarRain()
    {
        if (IsTransforming || isTeleporting)
            return;
            
        if (AttackTimer % (IsPhase2 ? 20 : 30) == 0 && AttackTimer < 150)
        {
            Vector2 position = Target.Center + new Vector2(Main.rand.Next(-400, 400), -500);
            Vector2 velocity = Vector2.UnitY * 8f;
            Projectile.NewProjectile(NPC.GetSource_FromAI(), position, velocity, ModContent.ProjectileType<AngelMiniStar>(),
                NPC.damage / 4, 1f, Main.myPlayer);
        }
    }

    private void DoAngelDash()
    {
        if (IsTransforming || isTeleporting)
            return;
        
        // Prepare by moving to the side.
        if (AttackTimer == 1)
        {
            dashPreparing = true;
            dashPrepareTimer = 0;
            
            // Choose a random side to move to.
            dashSide = Main.rand.NextBool() ? -1 : 1;
            dashStartPosition = NPC.Center;
        }
        
        // Prepare to dash.
        if (dashPreparing && AttackTimer < 30)
        {
            dashPrepareTimer++;
            
            // Calculate target position on the chosen side.
            Vector2 sideTarget = Target.Center + new Vector2(dashSide * 300, -150);
            
            // Move smoothly to the side.
            Vector2 toTarget = sideTarget - NPC.Center;
            float distance = toTarget.Length();
            
            if (distance > 10f)
            {
                toTarget.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 8f, 0.1f);
            }
            else
                NPC.velocity *= 0.9f;

            // Perform a dust telegraph.
            if (dashPrepareTimer % 5 == 0)
            {
                Vector2 dustPos = NPC.Center + new Vector2(dashSide * 50, 0);
                for (int i = 0; i < 5; i++)
                {
                    Dust dust = Dust.NewDustDirect(dustPos, 10, 10, ModContent.DustType<Stardust>(), dashSide * 5f, 0f, 100, default, 1.5f);
                    dust.noGravity = true;
                }
            }
            
            return;
        }
        
        // Execute the dash.
        if (AttackTimer == 30)
        {
            dashPreparing = false;
            SoundEngine.PlaySound(SoundID.Item1, NPC.Center);

            // Dash in the opposite direction.
            Vector2 dashDirection = new(-dashSide, 0) { Y = 0.3f };
            dashDirection.Normalize();
            NPC.velocity = dashDirection * (IsPhase2 ? 20f : 15f);
            
            // Emit a dust trail.
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, ModContent.DustType<Stardust>(), 
                    -NPC.velocity.X * 0.3f, -NPC.velocity.Y * 0.3f, 100, default, 2f);
                dust.noGravity = true;
            }
        }
        else if (AttackTimer < 50)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, ModContent.DustType<Stardust>(), 
                    -NPC.velocity.X * 0.3f, -NPC.velocity.Y * 0.3f, 100, default, 1.5f);
                dust.noGravity = true;
            }
        }
        else if (AttackTimer == 60)
            NPC.velocity *= 0.5f;
    }

    private void DoOrbitingStars()
    {
        if (AttackTimer == 1 && IsPhase2 && !isTeleporting)
            Main.NewText("Orbiting Stars!", Color.LightBlue);
    }

    private void DoCelestialStar()
    {
        if (AttackTimer == 45 && IsPhase2 && !isTeleporting)
        {
            SoundEngine.PlaySound(SoundID.Item9, NPC.Center);
            
            Vector2 direction = Target.Center - NPC.Center;
            direction.Normalize();
            
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction * 8f, ModContent.ProjectileType<AngelStar>(),
                NPC.damage / 2, 1f, Main.myPlayer, 0f, NPC.whoAmI, 4f);
        }
    }

    public override bool CanHitPlayer(Player target, ref int cooldownSlot) =>
        !IsTransforming && !isTeleporting && CurrentAttack != AttackState.OrbitingStars;

    public override bool? CanBeHitByProjectile(Projectile projectile) =>
        !IsTransforming && !isTeleporting;

    public override bool? CanBeHitByItem(Player player, Item item) =>
        !IsTransforming && !isTeleporting;

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        float finalAlpha = transparency;
        if (isTeleporting)
            finalAlpha *= teleportAlpha;

        Color color = drawColor * finalAlpha;
        Texture2D texture = TextureAssets.Npc[NPC.type].Value;
        Vector2 origin = new(texture.Width / 2, texture.Height / 2);

        spriteBatch.Draw(texture, NPC.Center - screenPos, null, color, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);
        
        return false;
    }

    public override void OnKill()
    {
        // Kill all orbiting stars.
        for (int i = 0; i < 4; i++)
        {
            if (orbitingStarIndices[i] != -1)
            {
                Projectile proj = Main.projectile[orbitingStarIndices[i]];
                if (proj.active)
                    proj.Kill();
            }
        }
        
        // Kill any other angel stars that might exist.
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile proj = Main.projectile[i];
            if (proj.active && proj.type == ModContent.ProjectileType<AngelStar>())
                proj.Kill();
        }
        
        for (int i = 0; i < 30; i++)
        {
            Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, ModContent.DustType<Stardust>(),
                NPC.velocity.X, NPC.velocity.Y, 100, default, 2f);
            dust.noGravity = true;
            dust.velocity *= 3f;
        }
    }
}
