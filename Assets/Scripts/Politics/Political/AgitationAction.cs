using System;

namespace MMBGame
{
    public class AgitationAction : PoliticalAction
    {
        public override string ActionName => "선동";
        public override bool RequiresPieceSelection => false;
        public override bool RequiresValueInput => false;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            PieceColor enemyColor = actorColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            BoardState state = manager.BoardManager.BoardState;
            int totalEnemySupport = 0;
            System.Collections.Generic.List<ChessPiece> pieces = state.GetAllPieces();

            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece != null && piece.color == enemyColor)
                {
                    totalEnemySupport += piece.support;
                }
            }

            if (totalEnemySupport == 0)
            {
                return false;
            }

            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece != null && piece.color == enemyColor)
                {
                    piece.support -= Math.Max(1, piece.support / 10);
                }
            }

            return true;
        }
    }
}
