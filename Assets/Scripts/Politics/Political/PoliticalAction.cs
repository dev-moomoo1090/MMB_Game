namespace MMBGame
{
    public abstract class PoliticalAction
    {
        public abstract string ActionName { get; }
        public abstract bool RequiresPieceSelection { get; }
        public abstract bool RequiresValueInput { get; }
        public abstract bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value);
    }
}
