using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MMBGame.AI
{
    public partial class BenevolentAI
    {
        private void ExecuteMilitaryPhase()
        {
            var movable = _ctx.GetControlledActivePieces(_side)
                .Where(p => !p.isOffBoard && !_ctx.IsSpecial(p))
                .ToList();

            var rebels = movable.Where(p => p.rebellionWeight >= 1.0f).ToList();
            foreach (var rp in rebels)
            {
                Move safeMove = GetSafestMove(rp);
                if (safeMove != null)
                    _ctx.BoardManager.TryMove(rp.file, rp.rank, safeMove.toFile, safeMove.toRank);
            }
            movable = movable.Where(p => p.rebellionWeight < 1.0f).ToList();

            var scored = new List<(ChessPiece piece, Move move, float score)>();
            foreach (var piece in movable)
            {
                var legalMoves = MoveValidator.GetLegalMoves(_ctx.BoardState, piece.file, piece.rank);
                foreach (var mv in legalMoves)
                    scored.Add((piece, mv, MoveEvaluator.Evaluate(piece, mv, _ctx.BoardState, _side)));
            }
            scored.Sort((a, b) => b.score.CompareTo(a.score));

            TryMoveWithFallback(scored);
        }

        private Move GetSafestMove(ChessPiece piece)
        {
            var moves = MoveValidator.GetLegalMoves(_ctx.BoardState, piece.file, piece.rank);
            if (moves.Count == 0) return null;
            PieceColor enemy = _ctx.Opponent(piece.color);
            foreach (var m in moves)
            {
                var sim = MoveValidator.SimulateMove(_ctx.BoardState, m);
                if (!CheckDetector.IsSquareAttacked(sim, m.toFile, m.toRank, enemy))
                    return m;
            }
            return moves[0];
        }

        private void TryMoveWithFallback(List<(ChessPiece piece, Move move, float score)> candidates)
        {
            var attempted = new HashSet<ChessPiece>();
            int maxTries = 3;
            int idx = 0;
            while (maxTries > 0 && idx < candidates.Count)
            {
                var (piece, move, _) = candidates[idx++];
                if (attempted.Contains(piece)) continue;

                if (AcceptanceCheck(piece))
                {
                    _ctx.BoardManager.TryMove(piece.file, piece.rank,
                                              move.toFile, move.toRank, move.promotionPiece);
                    return;
                }
                attempted.Add(piece);
                maxTries--;
            }
        }

        private bool AcceptanceCheck(ChessPiece piece)
        {
            if (_ctx.IsDictatorship(_side)) return true;

            float rate = piece.support / 100f + piece.acceptWeight / 100f;
            if (_ctx.IsBenevolent(_side)) rate += 0.1f;
            rate = Mathf.Clamp(rate, 0.05f, 0.99f);
            return Random.value < rate;
        }
    }
}
