using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public partial class HonorPiecePassiveSystem
    {
        private void TryPawnSupplySupport(PieceColor color, PlayerState player)
        {
            List<ChessPiece> pawns = GetPieces(color, PieceType.Pawn, PieceSide.None, true);
            if (pawns.Count == 0)
            {
                return;
            }

            float chance = Mathf.Sqrt(Mathf.Max(0f, player.honor / 2f));
            ChessPiece selectedPawn = null;
            int bestCount = int.MaxValue;
            for (int i = 0; i < pawns.Count; i++)
            {
                ChessPiece pawn = pawns[i];
                if (!RollPercent(chance))
                {
                    continue;
                }

                int count = pawnSupplyCounts[color].TryGetValue(pawn, out int savedCount) ? savedCount : 0;
                if (count < bestCount)
                {
                    bestCount = count;
                    selectedPawn = pawn;
                }
            }

            if (selectedPawn == null)
            {
                return;
            }

            pawnSupplyCounts[color][selectedPawn] = bestCount + 1;
            string actionName = GetRandomMilitaryActionName();
            freeMilitaryActions[color].Add(actionName);
        }

        private void TryKnightQueensidePraise(PieceColor color, PlayerState player)
        {
            if (!HasPiece(color, PieceType.Knight, PieceSide.Queenside))
            {
                return;
            }

            float chance = Mathf.Sqrt(Mathf.Max(0, player.honor));
            if (!RollPercent(chance))
            {
                return;
            }

            ApplyToAllPieces(color, piece => PoliticalStatService.ChangeSupport(piece, 5, "KnightPraise"));
            player.AddHonor(5);
        }

        private void TryBishopKingsideLedgerManipulation(PieceColor color)
        {
            if (!HasPiece(color, PieceType.Bishop, PieceSide.Kingside) || !RollPercent(5f))
            {
                return;
            }

            taxIncomeMultipliers[color] *= LEDGER_TAX_MULTIPLIER;
        }

        private void TryBishopKingsideStrategy(PieceColor color, PlayerState player)
        {
            if (!HasPiece(color, PieceType.Bishop, PieceSide.Kingside))
            {
                return;
            }

            float chance = Mathf.Sqrt(Mathf.Max(0, player.honor));
            if (!RollPercent(chance))
            {
                return;
            }

            EventBus.Instance.PublishReconResult(FindBestMoveText(color));
        }

        private void TryRookQueensideTradeOffer(PieceColor color, PlayerState player)
        {
            if (tradeAcceptedColors.Contains(color) || !HasPiece(color, PieceType.Rook, PieceSide.Queenside))
            {
                return;
            }

            float chance = Mathf.Sqrt(Mathf.Max(0, player.honor));
            if (!RollPercent(chance))
            {
                return;
            }

            player.goldPerTurn += 30;
            tradeAcceptedColors.Add(color);
        }

        private void TryRookQueensideFundingRequest(PieceColor color, PlayerState player)
        {
            if (!HasPiece(color, PieceType.Rook, PieceSide.Queenside) || !RollPercent(5f))
            {
                return;
            }

            ChessPiece rook = FindPiece(color, PieceType.Rook, PieceSide.Queenside);
            if (rook == null)
            {
                return;
            }

            if (player.SpendGold(30))
            {
                PoliticalStatService.ChangeSupport(rook, 5, "RookFundingRequest");
            }
            else
            {
                PoliticalStatService.ChangeSupport(rook, -10, "RookFundingRequest");
            }
        }

        private string FindBestMoveText(PieceColor color)
        {
            if (boardManager == null || boardManager.BoardState == null)
            {
                return "추천 수 없음";
            }

            List<Move> legalMoves = MoveValidator.GetAllLegalMoves(boardManager.BoardState, color);
            if (legalMoves.Count == 0)
            {
                return "추천 수 없음";
            }

            Move bestMove = legalMoves[0];
            int bestScore = int.MinValue;
            for (int i = 0; i < legalMoves.Count; i++)
            {
                Move move = legalMoves[i];
                ChessPiece target = boardManager.BoardState.GetPiece(move.toFile, move.toRank);
                int score = target != null && target.color != color ? BoardEvaluator.GetPieceValue(target.type) : 0;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            return "병법 추천: " + ToSquare(bestMove.fromFile, bestMove.fromRank) + " -> " + ToSquare(bestMove.toFile, bestMove.toRank);
        }

        private string ToSquare(int file, int rank)
        {
            char fileChar = (char)('a' + file);
            return fileChar.ToString() + (rank + 1);
        }

        private string GetRandomMilitaryActionName()
        {
            string[] actionNames =
            {
                "BarricadeAction",
                "RoadPlanAction",
                "TrebuchetAction",
                "BombardAction",
                "OutpostAction",
                "DivinePowerAction",
                "MiracleAction",
                "MilitaryExemptionAction"
            };

            return actionNames[Random.Range(0, actionNames.Length)];
        }

    }
}
