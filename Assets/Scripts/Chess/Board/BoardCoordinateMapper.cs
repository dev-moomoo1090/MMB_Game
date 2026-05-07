using UnityEngine;

namespace MMBGame
{
    public static class BoardCoordinateMapper
    {
        private const float HALF_W = 1.17f;
        private const float HALF_H = 0.59f;

        public static Vector2Int ToDisplaySquare(int file, int rank)
        {
            return new Vector2Int(7 - file, 7 - rank);
        }

        public static Vector2Int ToLogicalSquare(int displayFile, int displayRank)
        {
            return new Vector2Int(7 - displayFile, 7 - displayRank);
        }

        public static string GetTileName(int file, int rank)
        {
            return "Tile_" + GetSquareName(file, rank);
        }

        public static string GetSquareName(int file, int rank)
        {
            return ((char)('a' + file)).ToString() + (rank + 1);
        }

        public static bool TryParseTileName(string tileName, out int file, out int rank)
        {
            file = -1;
            rank = -1;
            if (string.IsNullOrEmpty(tileName))
            {
                return false;
            }

            // "a1" format (현재 타일 이름 형식)
            if (tileName.Length == 2)
            {
                int parsedFile = tileName[0] - 'a';
                int parsedRank = tileName[1] - '1';
                if (parsedFile >= 0 && parsedFile <= 7 && parsedRank >= 0 && parsedRank <= 7)
                {
                    file = parsedFile;
                    rank = parsedRank;
                    return true;
                }
            }

            // "Tile_a1" format (이전 형식 호환)
            if (tileName.StartsWith("Tile_"))
            {
                string[] parts = tileName.Split('_');
                if (parts.Length == 2 && parts[1].Length == 2)
                {
                    int parsedFile = parts[1][0] - 'a';
                    int parsedRank = parts[1][1] - '1';
                    if (parsedFile >= 0 && parsedFile <= 7 && parsedRank >= 0 && parsedRank <= 7)
                    {
                        file = parsedFile;
                        rank = parsedRank;
                        return true;
                    }
                }

                if (parts.Length == 3 && int.TryParse(parts[1], out int displayFile) && int.TryParse(parts[2], out int displayRank))
                {
                    Vector2Int logicalSquare = ToLogicalSquare(displayFile, displayRank);
                    file = logicalSquare.x;
                    rank = logicalSquare.y;
                    return true;
                }
            }

            return false;
        }

        public static Vector3 GetFallbackWorldPosition(int file, int rank)
        {
            Vector2Int displaySquare = ToDisplaySquare(file, rank);
            return new Vector3((displaySquare.x - displaySquare.y) * HALF_W, -(displaySquare.x + displaySquare.y) * HALF_H, 0f);
        }

        public static bool TryGetLogicalSquare(Transform boardRoot, Vector3 worldPosition, out int file, out int rank)
        {
            file = -1;
            rank = -1;
            if (boardRoot == null)
            {
                return false;
            }

            float bestDistance = float.MaxValue;
            Transform bestTile = null;
            for (int i = 0; i < boardRoot.childCount; i++)
            {
                Transform tile = boardRoot.GetChild(i);
                if (!TryParseTileName(tile.name, out int tileFile, out int tileRank))
                {
                    continue;
                }

                SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
                if (renderer != null && !renderer.bounds.Contains(worldPosition))
                {
                    continue;
                }

                float distance = Vector2.Distance(worldPosition, tile.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTile = tile;
                }
            }

            if (bestTile == null || !TryParseTileName(bestTile.name, out file, out rank))
            {
                return false;
            }

            return file >= 0 && file <= 7 && rank >= 0 && rank <= 7;
        }
    }
}
