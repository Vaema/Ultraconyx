using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ultraconyx.Content.Tiles;
using Ultraconyx.Content.Tiles.Walls;

namespace Ultraconyx.Content.Biomes;

public class HellevatorGen : ModSystem
{
    public static bool BiomeExists;
    public static Point BiomeCenter;

    public override void PostWorldGen()
    {
        if (BiomeExists) return;

        try
        {
            int centerX = WorldGen.genRand.Next(400, Main.maxTilesX - 400);
            int centerY = WorldGen.genRand.Next((int)Main.rockLayer + 100, Main.maxTilesY - 500);
            
            GenerateHollowOvalBiome(centerX, centerY);
            
            BiomeExists = true;
            BiomeCenter = new Point(centerX, centerY);
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Ultraconyx>().Logger.Error("Failed to generate oval biome: " + ex.Message);
        }
    }

    private static void GenerateHollowOvalBiome(int centerX, int centerY)
    {
        int horizontalRadius = WorldGen.genRand.Next(50, 70);
        int verticalRadius = WorldGen.genRand.Next(20, 30);
        int wallThickness = 7;
        
        ClearInteriorArea(centerX, centerY, horizontalRadius, verticalRadius);
        CreateHollowOvalWalls(centerX, centerY, horizontalRadius, verticalRadius, wallThickness);
        
        // Natural-looking stalactite/stalagmite decorations
        AddNaturalCaveFormations(centerX, centerY, horizontalRadius, verticalRadius, wallThickness);
        
        // Side entrances only (no top/bottom)
        AddSideEntrancesOnly(centerX, centerY, horizontalRadius, verticalRadius, wallThickness);
        
        AddInteriorFeatures(centerX, centerY, horizontalRadius - wallThickness, verticalRadius - wallThickness);
    }

    private static void ClearInteriorArea(int centerX, int centerY, int hRadius, int vRadius)
    {
        for (int x = centerX - hRadius; x <= centerX + hRadius; x++)
        {
            if (!WorldGen.InWorld(x, 0)) continue;
            
            for (int y = centerY - vRadius; y <= centerY + vRadius; y++)
            {
                if (!WorldGen.InWorld(x, y)) continue;
                
                float dx = (x - centerX) / (float)hRadius;
                float dy = (y - centerY) / (float)vRadius;
                if (dx * dx + dy * dy <= 1.0f)
                {
                    Tile tile = Framing.GetTileSafely(x, y);
                    tile.ClearTile();
                    tile.WallType = (ushort)ModContent.WallType<AbysslightRockWall>();
                }
            }
        }
    }

    private static void CreateHollowOvalWalls(int centerX, int centerY, int hRadius, int vRadius, int wallThickness)
    {
        int innerHRadius = Math.Max(5, hRadius - wallThickness);
        int innerVRadius = Math.Max(5, vRadius - wallThickness);
        
        for (int x = centerX - hRadius; x <= centerX + hRadius; x++)
        {
            if (!WorldGen.InWorld(x, 0)) continue;
            
            for (int y = centerY - vRadius; y <= centerY + vRadius; y++)
            {
                if (!WorldGen.InWorld(x, y)) continue;
                
                float outerDx = (x - centerX) / (float)hRadius;
                float outerDy = (y - centerY) / (float)vRadius;
                float outerDistSq = outerDx * outerDx + outerDy * outerDy;
                
                float innerDx = (x - centerX) / (float)innerHRadius;
                float innerDy = (y - centerY) / (float)innerVRadius;
                float innerDistSq = innerDx * innerDx + innerDy * innerDy;
                
                if (outerDistSq <= 1.0f && innerDistSq >= 1.0f)
                {
                    PlaceAbysslightRockTile(x, y);
                }
            }
        }
        
        MakeEdgesUneven(centerX, centerY, hRadius, vRadius, innerHRadius, innerVRadius);
    }

    private static void MakeEdgesUneven(int centerX, int centerY, int hRadius, int vRadius, int innerHRadius, int innerVRadius)
    {
        int noiseSeed = WorldGen.genRand.Next();
        
        for (int angleStep = 0; angleStep < 360; angleStep += 4)
        {
            float angle = MathHelper.ToRadians(angleStep);
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            
            int innerX = centerX + (int)(innerHRadius * cos);
            int innerY = centerY + (int)(innerVRadius * sin);
            
            float noise = (float)((Math.Sin(angle * 12 + noiseSeed) + 1) / 2);
            int carveDepth = (int)(noise * 3);
            
            for (int d = 0; d <= carveDepth; d++)
            {
                int carveX = innerX + (int)(d * cos * 0.8f);
                int carveY = innerY + (int)(d * sin * 0.8f);
                
                if (WorldGen.InWorld(carveX, carveY))
                {
                    Framing.GetTileSafely(carveX, carveY).ClearTile();
                }
            }
        }
    }

    private static void AddNaturalCaveFormations(int centerX, int centerY, int hRadius, int vRadius, int wallThickness)
    {
        int innerHRadius = hRadius - wallThickness;
        int innerVRadius = vRadius - wallThickness;
        
        // NATURAL-LOOKING STALACTITES (ceiling)
        int stalactiteCount = WorldGen.genRand.Next(20, 30);
        for (int i = 0; i < stalactiteCount; i++)
        {
            // Random position on upper half of oval
            float angle = WorldGen.genRand.NextFloat(MathHelper.Pi * 0.2f, MathHelper.Pi * 0.8f);
            angle = WorldGen.genRand.NextBool() ? angle : MathHelper.Pi + angle;
            
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            
            // Base position on inner wall (ceiling area)
            int baseX = centerX + (int)(innerHRadius * cos * WorldGen.genRand.NextFloat(0.95f, 1.05f));
            int baseY = centerY + (int)(innerVRadius * sin * WorldGen.genRand.NextFloat(0.95f, 1.05f));
            
            // Only create if it's on the ceiling (top 60% of oval)
            if (sin < -0.2f)
            {
                CreateNaturalStalactite(baseX, baseY);
            }
        }
        
        // NATURAL-LOOKING STALAGMITES (floor)
        int stalagmiteCount = WorldGen.genRand.Next(20, 30);
        for (int i = 0; i < stalagmiteCount; i++)
        {
            // Random position on lower half of oval
            float angle = WorldGen.genRand.NextFloat(MathHelper.Pi * 0.2f, MathHelper.Pi * 0.8f);
            angle = WorldGen.genRand.NextBool() ? angle : MathHelper.Pi + angle;
            
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            
            // Base position on inner wall (floor area)
            int baseX = centerX + (int)(innerHRadius * cos * WorldGen.genRand.NextFloat(0.95f, 1.05f));
            int baseY = centerY + (int)(innerVRadius * sin * WorldGen.genRand.NextFloat(0.95f, 1.05f));
            
            // Only create if it's on the floor (bottom 60% of oval)
            if (sin > 0.2f)
            {
                CreateNaturalStalagmite(baseX, baseY);
            }
        }
        
        // NATURAL COLUMNS (where stalactite and stalagmite meet)
        int columnCount = WorldGen.genRand.Next(3, 8);
        for (int i = 0; i < columnCount; i++)
        {
            float angle = WorldGen.genRand.NextFloat(0, MathHelper.TwoPi);
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            
            int columnX = centerX + (int)(innerHRadius * cos * WorldGen.genRand.NextFloat(0.7f, 0.85f));
            int midY = centerY;
            
            CreateNaturalColumn(columnX, midY);
        }
        
        // WALL FORMATIONS (natural rock outcroppings)
        int wallFormationCount = WorldGen.genRand.Next(15, 25);
        for (int i = 0; i < wallFormationCount; i++)
        {
            float angle = WorldGen.genRand.NextFloat(0, MathHelper.TwoPi);
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            
            int formationX = centerX + (int)(innerHRadius * cos * 1.02f);
            int formationY = centerY + (int)(innerVRadius * sin * 1.02f);
            
            CreateWallFormation(formationX, formationY, angle);
        }
    }

    private static void CreateNaturalStalactite(int baseX, int baseY)
    {
        // Natural stalactites are thicker at top, thinner at bottom
        int maxLength = WorldGen.genRand.Next(4, 10);
        
        for (int segment = 0; segment < maxLength; segment++)
        {
            int currentY = baseY + segment;
            
            // Determine width at this segment (wider at top, narrower at bottom)
            int maxWidth = (int)((maxLength - segment) * 0.5f) + 1;
            if (maxWidth < 1) maxWidth = 1;
            
            for (int width = -maxWidth; width <= maxWidth; width++)
            {
                // Probability decreases with distance from center
                if (Math.Abs(width) <= 1 || WorldGen.genRand.NextBool(3))
                {
                    int currentX = baseX + width;
                    
                    if (WorldGen.InWorld(currentX, currentY))
                    {
                        PlaceAbysslightRockTile(currentX, currentY);
                        
                        // Add occasional side drips
                        if (segment > 1 && WorldGen.genRand.NextBool(5) && Math.Abs(width) == maxWidth)
                        {
                            int dripX = currentX + (width > 0 ? 1 : -1);
                            if (WorldGen.InWorld(dripX, currentY))
                            {
                                PlaceAbysslightRockTile(dripX, currentY);
                            }
                        }
                    }
                }
            }
            
            // Occasionally create branching stalactites
            if (segment > 2 && WorldGen.genRand.NextBool(8))
            {
                int branchX = baseX + (WorldGen.genRand.NextBool() ? 2 : -2);
                int branchY = currentY;
                CreateSmallStalactiteTip(branchX, branchY);
            }
        }
    }

    private static void CreateSmallStalactiteTip(int x, int y)
    {
        int tipLength = WorldGen.genRand.Next(2, 5);
        for (int i = 0; i < tipLength; i++)
        {
            if (WorldGen.InWorld(x, y + i))
            {
                PlaceAbysslightRockTile(x, y + i);
            }
        }
    }

    private static void CreateNaturalStalagmite(int baseX, int baseY)
    {
        // Natural stalagmites are thicker at bottom, thinner at top
        int maxLength = WorldGen.genRand.Next(4, 10);
        
        for (int segment = 0; segment < maxLength; segment++)
        {
            int currentY = baseY - segment; // Going UP from floor
            
            // Determine width at this segment (wider at bottom, narrower at top)
            int maxWidth = (int)((maxLength - segment) * 0.5f) + 1;
            if (maxWidth < 1) maxWidth = 1;
            
            for (int width = -maxWidth; width <= maxWidth; width++)
            {
                // Probability decreases with distance from center
                if (Math.Abs(width) <= 1 || WorldGen.genRand.NextBool(3))
                {
                    int currentX = baseX + width;
                    
                    if (WorldGen.InWorld(currentX, currentY))
                    {
                        PlaceAbysslightRockTile(currentX, currentY);
                        
                        // Add occasional side growths
                        if (segment > 1 && WorldGen.genRand.NextBool(5) && Math.Abs(width) == maxWidth)
                        {
                            int growthX = currentX + (width > 0 ? 1 : -1);
                            if (WorldGen.InWorld(growthX, currentY))
                            {
                                PlaceAbysslightRockTile(growthX, currentY);
                            }
                        }
                    }
                }
            }
            
            // Occasionally create branching stalagmites
            if (segment > 2 && WorldGen.genRand.NextBool(8))
            {
                int branchX = baseX + (WorldGen.genRand.NextBool() ? 2 : -2);
                int branchY = currentY;
                CreateSmallStalagmiteTip(branchX, branchY);
            }
        }
    }

    private static void CreateSmallStalagmiteTip(int x, int y)
    {
        int tipLength = WorldGen.genRand.Next(2, 5);
        for (int i = 0; i < tipLength; i++)
        {
            if (WorldGen.InWorld(x, y - i)) // Going UP
            {
                PlaceAbysslightRockTile(x, y - i);
            }
        }
    }

    private static void CreateNaturalColumn(int baseX, int baseY)
    {
        int columnHeight = WorldGen.genRand.Next(6, 15);
        int startY = baseY - columnHeight / 2;
        
        for (int h = 0; h < columnHeight; h++)
        {
            int currentY = startY + h;
            
            // Column is wider in middle, thinner at ends
            float t = (float)h / columnHeight;
            float widthFactor = (float)Math.Sin(t * MathHelper.Pi); // Sine wave: 0 at ends, 1 in middle
            int maxWidth = (int)(widthFactor * 2) + 1;
            
            for (int w = -maxWidth; w <= maxWidth; w++)
            {
                // Higher probability near center
                if (Math.Abs(w) <= 1 || WorldGen.genRand.NextBool(4))
                {
                    int currentX = baseX + w;
                    
                    if (WorldGen.InWorld(currentX, currentY))
                    {
                        PlaceAbysslightRockTile(currentX, currentY);
                    }
                }
            }
            
            // Add irregular bumps
            if (WorldGen.genRand.NextBool(5))
            {
                int bumpX = baseX + (WorldGen.genRand.NextBool() ? maxWidth + 1 : -maxWidth - 1);
                if (WorldGen.InWorld(bumpX, currentY))
                {
                    PlaceAbysslightRockTile(bumpX, currentY);
                }
            }
        }
    }

    private static void CreateWallFormation(int baseX, int baseY, float angle)
    {
        int formationSize = WorldGen.genRand.Next(3, 8);
        
        // Determine direction (mostly perpendicular to wall)
        float perpAngle = angle + MathHelper.PiOver2;
        float perpCos = (float)Math.Cos(perpAngle);
        float perpSin = (float)Math.Sin(perpAngle);
        
        for (int i = 0; i < formationSize; i++)
        {
            // Random offset from base position
            float offsetDist = WorldGen.genRand.NextFloat(0, 3);
            int offsetX = baseX + (int)(offsetDist * perpCos);
            int offsetY = baseY + (int)(offsetDist * perpSin);
            
            // Create small cluster
            int clusterSize = WorldGen.genRand.Next(2, 5);
            for (int j = 0; j < clusterSize; j++)
            {
                int clusterX = offsetX + WorldGen.genRand.Next(-1, 2);
                int clusterY = offsetY + WorldGen.genRand.Next(-1, 2);
                
                if (WorldGen.InWorld(clusterX, clusterY))
                {
                    PlaceAbysslightRockTile(clusterX, clusterY);
                }
            }
        }
    }

    private static void AddSideEntrancesOnly(int centerX, int centerY, int hRadius, int vRadius, int wallThickness)
    {
        // LEFT SIDE ENTRANCE only (π radians)
        CreateNaturalEntrance(centerX, centerY, hRadius, vRadius, wallThickness, MathHelper.Pi);
        
        // RIGHT SIDE ENTRANCE only (0 radians)
        CreateNaturalEntrance(centerX, centerY, hRadius, vRadius, wallThickness, 0f);
        
        // NO top or bottom entrances
    }

    private static void CreateNaturalEntrance(int centerX, int centerY, int hRadius, int vRadius, int wallThickness, float angle)
    {
        float cos = (float)Math.Cos(angle);
        float sin = (float)Math.Sin(angle);
        
        int entranceWidth = WorldGen.genRand.Next(5, 7);
        
        for (int w = -entranceWidth/2; w <= entranceWidth/2; w++)
        {
            int perpXOffset = (int)(w * sin);
            int perpYOffset = -(int)(w * cos);
            
            // Clear natural-looking tunnel
            for (int depth = -3; depth <= hRadius + 5; depth++)
            {
                int tunnelX = centerX + (int)(depth * cos) + perpXOffset;
                int tunnelY = centerY + (int)(depth * sin) + perpYOffset;
                
                if (!WorldGen.InWorld(tunnelX, tunnelY)) continue;
                
                // Clear path with occasional natural irregularities
                if (depth <= hRadius + 2 || Math.Abs(w) < entranceWidth/2 - 1)
                {
                    Tile tile = Framing.GetTileSafely(tunnelX, tunnelY);
                    tile.ClearTile();
                    tile.WallType = (ushort)ModContent.WallType<AbysslightRockWall>();
                }
                
                // Add natural floor at side entrances
                if (Math.Abs(cos) > 0.7f && depth == hRadius + 2)
                {
                    // Create uneven natural floor
                    for (int floorY = 0; floorY <= 2; floorY++)
                    {
                        int floorX = tunnelX;
                        int floorTileY = tunnelY + floorY;
                        
                        if (WorldGen.InWorld(floorX, floorTileY))
                        {
                            if (floorY == 2 || (floorY == 1 && WorldGen.genRand.NextBool()))
                            {
                                WorldGen.PlaceTile(floorX, floorTileY, TileID.Stone, true, true);
                            }
                            else
                            {
                                Framing.GetTileSafely(floorX, floorTileY).ClearTile();
                            }
                        }
                    }
                }
            }
        }
        
        // Add natural-looking entrance decorations
        AddNaturalEntranceDecorations(centerX, centerY, hRadius, vRadius, angle, entranceWidth);
    }

    private static void AddNaturalEntranceDecorations(int centerX, int centerY, int hRadius, int vRadius, float angle, int width)
    {
        float cos = (float)Math.Cos(angle);
        float sin = (float)Math.Sin(angle);
        
        // Create natural-looking rock formations around entrance
        for (int w = -width/2 - 1; w <= width/2 + 1; w += width + 2)
        {
            int edgeX = centerX + (int)(hRadius * cos) + (int)(w * sin);
            int edgeY = centerY + (int)(vRadius * sin) - (int)(w * cos);
            
            // Create irregular rock formations
            int formationHeight = WorldGen.genRand.Next(3, 6);
            for (int h = 0; h < formationHeight; h++)
            {
                // Make formation irregular
                int currentX = edgeX + WorldGen.genRand.Next(-1, 2);
                int currentY = edgeY + (int)(h * sin * 0.3f) + WorldGen.genRand.Next(-1, 2);
                
                if (WorldGen.InWorld(currentX, currentY))
                {
                    // Place tiles with some gaps for natural look
                    if (WorldGen.genRand.NextBool(3))
                    {
                        PlaceAbysslightRockTile(currentX, currentY);
                    }
                }
            }
        }
    }

    private static void PlaceAbysslightRockTile(int x, int y)
    {
        if (!WorldGen.InWorld(x, y)) return;

        try
        {
            int type = ModContent.TileType<AbysslightRocksTile>();
            if (type > 0)
            {
                WorldGen.PlaceTile(x, y, type, true, true);
                Tile tile = Framing.GetTileSafely(x, y);
                tile.WallType = (ushort)ModContent.WallType<AbysslightRockWall>();
                WorldGen.TileFrame(x, y);
            }
        }
        catch
        {
            // Silent fail
        }
    }

    private static void AddInteriorFeatures(int centerX, int centerY, int innerHRadius, int innerVRadius)
    {
        int features = WorldGen.genRand.Next(2, 4);
        
        for (int i = 0; i < features; i++)
        {
            float angle = WorldGen.genRand.NextFloat(0, MathHelper.TwoPi);
            float radiusRatio = WorldGen.genRand.NextFloat(0.2f, 0.4f);
            
            int featureX = centerX + (int)(innerHRadius * radiusRatio * (float)Math.Cos(angle));
            int featureY = centerY + (int)(innerVRadius * radiusRatio * (float)Math.Sin(angle));
            
            featureX = Math.Clamp(featureX, 50, Main.maxTilesX - 50);
            featureY = Math.Clamp(featureY, 50, Main.maxTilesY - 50);
            
            // Simple natural features
            if (WorldGen.genRand.NextBool())
            {
                CreateNaturalRockPile(featureX, featureY);
            }
            else
            {
                CreateNaturalPlatform(featureX, featureY);
            }
        }
    }

    private static void CreateNaturalRockPile(int x, int y)
    {
        int pileSize = WorldGen.genRand.Next(4, 8);
        for (int i = 0; i < pileSize; i++)
        {
            int offsetX = WorldGen.genRand.Next(-2, 3);
            int offsetY = WorldGen.genRand.Next(-1, 2);
            if (WorldGen.InWorld(x + offsetX, y + offsetY))
            {
                PlaceAbysslightRockTile(x + offsetX, y + offsetY);
            }
        }
    }

    private static void CreateNaturalPlatform(int x, int y)
    {
        int length = WorldGen.genRand.Next(3, 6);
        bool direction = WorldGen.genRand.NextBool();
        
        for (int i = 0; i < length; i++)
        {
            int currentX = x + (direction ? i : -i);
            
            // Make platform uneven
            if (WorldGen.InWorld(currentX, y) && WorldGen.genRand.NextBool())
            {
                try 
                { 
                    WorldGen.PlaceTile(currentX, y, TileID.Stone, true, true); 
                }
                catch { }
            }
        }
    }
}