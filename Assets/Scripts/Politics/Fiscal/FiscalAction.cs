namespace MMBGame
{
    public abstract class FiscalAction
    {
        public abstract string ActionName { get; }
        public abstract bool RequiresPieceSelection { get; }
        public abstract bool RequiresValueInput { get; }
        public abstract bool Execute(ChessPiece targetPiece, PlayerState actor, int inputValue, PoliticsManager manager);
    }
}
