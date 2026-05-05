namespace MMBGame
{
    public class ReconAction : PoliticalAction
    {
        public override string ActionName => "정찰";
        public override bool RequiresPieceSelection => false;
        public override bool RequiresValueInput => false;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            string lastAction = manager.GetLastOpponentAction(actorColor);
            EventBus.Instance.PublishReconResult(lastAction ?? "없음");
            return true;
        }
    }
}
