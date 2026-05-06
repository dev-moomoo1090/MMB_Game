using System;

namespace MMBGame
{
    public class CivilAidAction : PoliticalAction
    {
        public override string ActionName => "대민지원";
        public override bool RequiresPieceSelection => false;
        public override bool RequiresValueInput => true;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            PlayerState actor = manager.GetActorState(actorColor);
            if (actor == null || !actor.SpendGold(value))
            {
                return false;
            }

            BoardState state = manager.BoardManager.BoardState;
            int totalTax = 0;
            System.Collections.Generic.List<ChessPiece> pieces = state.GetAllPieces();

            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece != null && piece.color == actorColor)
                {
                    totalTax += Math.Max(1, piece.taxPerTurn);
                }
            }

            if (totalTax == 0)
            {
                return true;
            }

            int supportGain = Math.Max(1, (int)Math.Ceiling(value / Math.Max(1f, totalTax * 0.5f)));
            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece != null && piece.color == actorColor)
                {
                    piece.support += supportGain;
                }
            }

            return true;
        }
    }
}
