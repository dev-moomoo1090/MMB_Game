using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public partial class HonorPiecePassiveSystem
    {
        private bool IsHonorActive(PlayerState player)
        {
            return player != null && player.honor >= HONOR_THRESHOLD;
        }

        private bool IsDishonorActive(PlayerState player)
        {
            return player != null && player.honor <= DISHONOR_THRESHOLD;
        }

        private bool RollPercent(float percent)
        {
            if (percent <= 0f)
            {
                return false;
            }

            return Random.Range(0f, 100f) < percent;
        }

        private void ApplyToAllPieces(PieceColor color, System.Action<ChessPiece> effect)
        {
            if (boardManager == null || boardManager.BoardState == null)
            {
                return;
            }

            List<ChessPiece> pieces = boardManager.BoardState.GetAllPieces();
            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece != null && piece.color == color)
                {
                    effect(piece);
                }
            }
        }

        private bool HasPiece(PieceColor color, PieceType type, PieceSide side)
        {
            return FindPiece(color, type, side) != null;
        }

        private ChessPiece FindPiece(PieceColor color, PieceType type, PieceSide side)
        {
            List<ChessPiece> pieces = GetPieces(color, type, side, false);
            return pieces.Count > 0 ? pieces[0] : null;
        }

        private List<ChessPiece> GetPieces(PieceColor color, PieceType type, PieceSide side, bool ignoreSide)
        {
            List<ChessPiece> result = new List<ChessPiece>();
            if (boardManager == null || boardManager.BoardState == null)
            {
                return result;
            }

            List<ChessPiece> pieces = boardManager.BoardState.GetAllPieces();
            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece == null || piece.color != color || piece.type != type)
                {
                    continue;
                }

                if (!ignoreSide && piece.side != side)
                {
                    continue;
                }

                result.Add(piece);
            }

            result.Sort(ComparePassivePriority);
            return result;
        }

        private int ComparePassivePriority(ChessPiece first, ChessPiece second)
        {
            int typeCompare = GetPassiveTypeOrder(first.type).CompareTo(GetPassiveTypeOrder(second.type));
            if (typeCompare != 0)
            {
                return typeCompare;
            }

            int sideCompare = GetPassiveSideOrder(first.side).CompareTo(GetPassiveSideOrder(second.side));
            if (sideCompare != 0)
            {
                return sideCompare;
            }

            return first.lane.CompareTo(second.lane);
        }

        private int GetPassiveTypeOrder(PieceType type)
        {
            switch (type)
            {
                case PieceType.Pawn: return 0;
                case PieceType.Knight: return 1;
                case PieceType.Bishop: return 2;
                case PieceType.Rook: return 3;
                case PieceType.Queen: return 4;
                default: return 5;
            }
        }

        private int GetPassiveSideOrder(PieceSide side)
        {
            switch (side)
            {
                case PieceSide.Queenside: return 0;
                case PieceSide.Kingside: return 1;
                default: return 2;
            }
        }

        private PlayerState GetPlayer(PieceColor color)
        {
            EnsureReferences();
            return politicsManager != null ? politicsManager.GetCurrentPlayer(color) : null;
        }

        private void EnsureColor(PieceColor color)
        {
            if (color == PieceColor.None)
            {
                return;
            }

            if (!pawnSupplyCounts.ContainsKey(color))
            {
                pawnSupplyCounts[color] = new Dictionary<ChessPiece, int>();
            }

            if (!freeMilitaryActions.ContainsKey(color))
            {
                freeMilitaryActions[color] = new HashSet<string>();
            }

            if (!taxIncomeMultipliers.ContainsKey(color))
            {
                taxIncomeMultipliers[color] = 1f;
            }

            if (!supportLossByColor.ContainsKey(color))
            {
                supportLossByColor[color] = 0;
            }
        }

        private void EnsureReferences()
        {
            boardManager = SceneComponentResolver.Resolve(boardManager);
            politicsManager = SceneComponentResolver.Resolve(politicsManager);
            stockfishBridge = SceneComponentResolver.Resolve(stockfishBridge);
        }
    }
}
