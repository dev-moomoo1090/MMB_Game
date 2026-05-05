namespace MMBGame
{
    public class InterrogationAction : PoliticalAction
    {
        public override string ActionName => "심문";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => false;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            if (target == null || target.color != actorColor)
            {
                return false;
            }

            EventBus.Instance.PublishIntelligenceGathered(target);
            return true;
        }
    }
}
