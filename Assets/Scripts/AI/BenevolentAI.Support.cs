using System.Collections.Generic;
using System.Linq;

namespace MMBGame.AI
{
    public partial class BenevolentAI
    {
        private void ExecuteTurnEnd()
        {
            var player = _ctx.GetPlayerState(_side);
            PrisonerFate _ = DecidePrisonerFate(player.honor, player.gold);
        }

        private PrisonerFate DecidePrisonerFate(int honor, int gold)
        {
            if (honor < 30) return PrisonerFate.Release;
            if (gold < 100) return PrisonerFate.Ransom;
            return PrisonerFate.Release;
        }

        private void HandlePieceEvents()
        {
        }

        private ChessPiece GetSafePiece(List<ChessPiece> pieces, PieceType type, PieceColor enemy)
        {
            return pieces
                .Where(p => p.type == type && !p.isOffBoard && !_ctx.IsSpecial(p))
                .FirstOrDefault(p => !CheckDetector.IsSquareAttacked(
                                         _ctx.BoardState, p.file, p.rank, enemy));
        }

        private ChessPiece SelectBetrayTarget()
        {
            var enemyPieces = _ctx.GetActivePieces(_ctx.Opponent(_side))
                .Where(p => !p.isBetrayed && !_ctx.IsSpecial(p))
                .ToList();
            if (enemyPieces.Count == 0) return null;

            var queen = enemyPieces.FirstOrDefault(p => p.type == PieceType.Queen);
            if (queen != null) return queen;
            var bishop = enemyPieces.FirstOrDefault(p => p.type == PieceType.Bishop);
            if (bishop != null) return bishop;
            return enemyPieces
                .OrderByDescending(p => AIContext.PieceValue(p.type) * 10 - p.taxPerTurn)
                .First();
        }

        private float AverageSupport(List<ChessPiece> pieces)
        {
            if (pieces.Count == 0) return 0f;
            float total = 0f;
            foreach (var p in pieces) total += p.support;
            return total / pieces.Count;
        }

        private enum ActionKind   { Fiscal, Military, Political }
        private enum PrisonerFate { Release, Ransom, Execute }

        private struct ActionCandidate
        {
            public readonly string      name;
            public readonly ChessPiece  target;
            public readonly int         value;
            public readonly ActionKind  kind;
            public readonly float       score;

            public ActionCandidate(string name, ChessPiece target, int value,
                                   ActionKind kind, float score)
            {
                this.name   = name;
                this.target = target;
                this.value  = value;
                this.kind   = kind;
                this.score  = score;
            }

            public static readonly ActionCandidate None =
                new ActionCandidate(null, null, 0, ActionKind.Fiscal, 0f);
        }
    }
}
