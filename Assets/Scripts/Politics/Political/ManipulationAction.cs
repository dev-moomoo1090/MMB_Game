using System;

namespace MMBGame
{
    public class ManipulationAction : PoliticalAction
    {
        public override string ActionName => "여론조작";
        public override bool RequiresPieceSelection => false;
        public override bool RequiresValueInput => true;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            PlayerState actor = manager.GetActorState(actorColor);
            if (actor == null || !actor.SpendGold(value))
            {
                return false;
            }

            PieceColor enemyColor = actorColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            BoardState state = manager.BoardManager.BoardState;
            int totalTax = 0;
            System.Collections.Generic.List<ChessPiece> pieces = state.GetAllPieces();

            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece != null && piece.color == enemyColor)
                {
                    totalTax += Math.Max(1, piece.taxPerTurn);
                }
            }

            if (totalTax == 0)
            {
                return true;
            }

            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece != null && piece.color == enemyColor)
                {
                    piece.support -= Math.Max(1, value * Math.Max(1, piece.taxPerTurn) / totalTax);
                }
            }

            return true;
        }
    }
}
