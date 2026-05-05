using System.Collections.Generic;

namespace MMBGame
{
    public class AssassinationAction : PoliticalAction
    {
        public override string ActionName => "암살";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => false;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            if (target == null)
            {
                return false;
            }

            if (target.color == actorColor)
            {
                return false;
            }

            BoardState state = manager.BoardManager.BoardState;
            List<Move> targetMoves = MoveGenerator.GeneratePseudoLegalMoves(state, target.file, target.rank);
            HashSet<(int, int)> targetSquares = new HashSet<(int, int)>();
            for (int i = 0; i < targetMoves.Count; i++)
            {
                targetSquares.Add((targetMoves[i].toFile, targetMoves[i].toRank));
            }

            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    ChessPiece piece = state.GetPiece(file, rank);
                    if (piece == null || piece.color != actorColor)
                    {
                        continue;
                    }

                    List<Move> myMoves = MoveGenerator.GeneratePseudoLegalMoves(state, file, rank);
                    for (int i = 0; i < myMoves.Count; i++)
                    {
                        if (!targetSquares.Contains((myMoves[i].toFile, myMoves[i].toRank)))
                        {
                            continue;
                        }

                        state.squares[target.file, target.rank].piece = null;
                        target.ClearOneTimeMovePatterns();
                        EventBus.Instance.PublishPieceCapturePending(target);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
