using System;

namespace MMBGame
{
    public class PropagandaAction : PoliticalAction
    {
        public override string ActionName => "선전";
        public override bool RequiresPieceSelection => false;
        public override bool RequiresValueInput => false;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            BoardState state = manager.BoardManager.BoardState;
            int totalSupport = 0;
            System.Collections.Generic.List<ChessPiece> pieces = state.GetAllPieces();

            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece != null && piece.color == actorColor)
                {
                    totalSupport += piece.support;
                }
            }

            if (totalSupport == 0)
            {
                return false;
            }

            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece != null && piece.color == actorColor)
                {
                    piece.support += Math.Max(1, piece.support / 10);
                }
            }

            return true;
        }
    }
}
