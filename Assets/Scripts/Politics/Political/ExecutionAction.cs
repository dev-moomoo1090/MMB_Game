using System;

namespace MMBGame
{
    public class ExecutionAction : PoliticalAction
    {
        public override string ActionName => "처형";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => false;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            if (target.color != actorColor)
            {
                return false;
            }

            int legitimacyFactor = 5 - target.punishCount;
            bool isTyrant = KingStateEvaluator.IsTyrant(actorColor);
            int basePenalty = Math.Max(1, legitimacyFactor * legitimacyFactor / 3);
            if (isTyrant)
            {
                basePenalty = (int)Math.Ceiling(basePenalty * 0.2f);
            }

            BoardState state = manager.BoardManager.BoardState;
            if (basePenalty > 0)
            {
                System.Collections.Generic.List<ChessPiece> pieces = state.GetAllPieces();
                for (int i = 0; i < pieces.Count; i++)
                {
                    ChessPiece piece = pieces[i];
                    if (piece != null && piece.color == actorColor)
                    {
                        piece.support -= basePenalty;
                    }
                }

                PlayerState actor = manager.GetActorState(actorColor);
                actor?.AddHonor(-basePenalty);
            }

            int summonFile = target.isOffBoard ? target.offBoardOrigin.col : target.file;
            int summonRank = target.isOffBoard ? target.offBoardOrigin.row : target.rank;
            target.ClearOneTimeMovePatterns();
            EventBus.Instance.PublishPieceCapturePending(target);
            if (target.isOffBoard)
            {
                state.offBoardPieces.Remove(target);
            }
            else
            {
                state.squares[target.file, target.rank].piece = null;
            }

            if (isTyrant && target.type != PieceType.Pawn)
            {
                TrySummonExecutionPawn(state, actorColor, summonFile, summonRank);
                manager.BoardManager.RefreshPieceVisuals();
            }

            return true;
        }

        private void TrySummonExecutionPawn(BoardState state, PieceColor color, int file, int rank)
        {
            if (TryPlaceExecutionPawn(state, color, file, rank))
            {
                return;
            }

            for (int radius = 1; radius <= 7; radius++)
            {
                for (int candidateFile = file - radius; candidateFile <= file + radius; candidateFile++)
                {
                    for (int candidateRank = rank - radius; candidateRank <= rank + radius; candidateRank++)
                    {
                        if (TryPlaceExecutionPawn(state, color, candidateFile, candidateRank))
                        {
                            return;
                        }
                    }
                }
            }
        }

        private bool TryPlaceExecutionPawn(BoardState state, PieceColor color, int file, int rank)
        {
            if (!state.IsInBounds(file, rank) || state.GetPiece(file, rank) != null)
            {
                return false;
            }

            Pawn pawn = new Pawn(color, file, rank);
            pawn.pieceName = color == PieceColor.White ? "White Execution Pawn" : "Black Execution Pawn";
            pawn.support = 50;
            pawn.taxPerTurn = 0;
            state.squares[file, rank].piece = pawn;
            return true;
        }
    }
}
