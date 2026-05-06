using System;

namespace MMBGame
{
    public class RoadPlanAction : MilitaryAction
    {
        public override string ActionName => "RoadPlanAction";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresPositionSelection => true;

        public override bool Execute(ChessPiece targetPiece, int targetFile, int targetRank, PieceColor actorColor, MilitaryManager manager)
        {
            if (targetPiece == null || targetPiece.color != actorColor || manager == null || manager.BoardManager == null)
            {
                return false;
            }

            BoardState state = manager.BoardManager.BoardState;
            if (state == null || !state.IsInBounds(targetFile, targetRank))
            {
                return false;
            }

            if (!IsValidArea(targetPiece.rank, actorColor) || !IsValidArea(targetRank, actorColor))
            {
                return false;
            }

            if (!IsAdjacentLine(targetPiece.file, targetPiece.rank, targetFile, targetRank))
            {
                return false;
            }

            state.SetRoad(targetPiece.file, targetPiece.rank, targetFile, targetRank);
            return true;
        }

        private bool IsValidArea(int rank, PieceColor color)
        {
            if (color == PieceColor.White)
            {
                return rank >= 0 && rank <= 2;
            }

            if (color == PieceColor.Black)
            {
                return rank >= 5 && rank <= 7;
            }

            return false;
        }

        private bool IsAdjacentLine(int firstFile, int firstRank, int secondFile, int secondRank)
        {
            int fileDistance = Math.Abs(firstFile - secondFile);
            int rankDistance = Math.Abs(firstRank - secondRank);
            return fileDistance <= 1 && rankDistance <= 1 && fileDistance + rankDistance > 0;
        }
    }
}
