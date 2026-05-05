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
            int basePenalty = Math.Max(1, legitimacyFactor * legitimacyFactor / 3);
            BoardState state = manager.BoardManager.BoardState;
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
            return true;
        }
    }
}
