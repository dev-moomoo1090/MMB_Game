namespace MMBGame
{
    public class BombardAction : MilitaryAction
    {
        /*delete this jusuck*/
        public override string ActionName => "BombardAction";
        public override bool RequiresPieceSelection => false;
        public override bool RequiresPositionSelection => true;

        public override bool Execute(ChessPiece targetPiece, int targetFile, int targetRank, PieceColor actorColor, MilitaryManager manager)
        {
            ChessPiece trebuchet = manager.GetTrebuchet(actorColor);
            if (trebuchet == null || manager.HasPendingBombard(actorColor))
            {
                return false;
            }

            int direction = actorColor == PieceColor.White ? 1 : -1;
            if (targetFile != trebuchet.file || (targetRank - trebuchet.rank) * direction <= 0)
            {
                return false;
            }

            manager.AddDelayedEffect(new DelayedEffect(DelayedEffectType.Bombard, targetFile, targetRank, actorColor, 3));
            return true;
        }
    }
}
