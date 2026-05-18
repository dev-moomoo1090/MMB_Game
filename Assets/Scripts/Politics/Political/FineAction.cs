using System;

namespace MMBGame
{
    public class FineAction : PoliticalAction
    {
        public override string ActionName => "벌금";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => true;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            if (target == null || target.color != actorColor)
            {
                return false;
            }

            PlayerState actor = manager.GetActorState(actorColor);
            if (actor == null)
            {
                return false;
            }

            actor.AddGold(value);
            int baseTax = Math.Max(1, target.taxPerTurn);
            int legitimacyFactor = 5 - target.punishCount;
            int penalty = Math.Max(1, value * legitimacyFactor * legitimacyFactor / (baseTax * 3));
            PoliticalStatService.ChangeSupportFromPoliticalAction(target, -penalty, actorColor, ActionName);
            target.punishCount = 0;
            return true;
        }
    }
}
