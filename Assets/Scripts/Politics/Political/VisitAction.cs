namespace MMBGame
{
    public class VisitAction : PoliticalAction
    {
        public override string ActionName => "방문";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => false;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            if (target.color != actorColor)
            {
                return false;
            }

            PoliticalStatService.ChangeSupport(target, 3, ActionName);
            return true;
        }
    }
}
