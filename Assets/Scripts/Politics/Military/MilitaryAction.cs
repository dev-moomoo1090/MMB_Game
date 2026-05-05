namespace MMBGame
{
    public abstract class MilitaryAction
    {
        public abstract string ActionName { get; }
        public abstract bool RequiresPieceSelection { get; }
        public abstract bool RequiresPositionSelection { get; }
        public abstract bool Execute(ChessPiece targetPiece, int targetFile, int targetRank, PieceColor actorColor, MilitaryManager manager);
    }
}
