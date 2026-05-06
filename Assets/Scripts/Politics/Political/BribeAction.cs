using System;

namespace MMBGame
{
    public class BribeAction : PoliticalAction
    {
        public override string ActionName => "매수";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => true;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            if (target.color == actorColor || target.isBetrayed)
            {
                return false;
            }

            PlayerState actor = manager.GetActorState(actorColor);
            if (actor == null || !actor.SpendGold(value * 3))
            {
                return false;
            }

            int bonus = Math.Max(1, value / Math.Max(1, target.taxPerTurn));
            int chance = Math.Max(5, bonus + (100 - target.support) + target.acceptWeight - target.taxPerTurn * 5);
            for (int i = 0; i < 3; i++)
            {
                if (UnityEngine.Random.Range(0, 100) < chance)
                {
                    target.isBetrayed = true;
                    return true;
                }
            }

            return false;
        }
    }
}
