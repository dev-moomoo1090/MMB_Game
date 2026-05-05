namespace MMBGame
{
    public class IntelAction : PoliticalAction
    {
        public override string ActionName => "정보";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => false;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            if (target.color == actorColor || !target.isBetrayed)
            {
                return false;
            }

            target.isIntelTarget = true;
            EventBus.Instance.PublishIntelligenceGathered(target);
            return true;
        }
    }
}
