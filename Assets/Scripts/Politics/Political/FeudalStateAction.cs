using System;
using UnityEngine;

namespace MMBGame
{
    public class FeudalStateAction : PoliticalAction
    {
        public override string ActionName => "제후국";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => true;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            if (target == null || target.color == actorColor)
            {
                return false;
            }

            PlayerState actor = manager.GetActorState(actorColor);
            if (actor == null || !actor.SpendGold(value))
            {
                return false;
            }

            float chance = Mathf.Clamp((20 - target.support) * 1.5f + value / Math.Max(1, target.taxPerTurn), 0f, 90f);
            target.rebellionWeight += value / 10f;
            if (UnityEngine.Random.Range(0f, 100f) < chance)
            {
                EventBus.Instance.PublishRebellionTriggered(target);
            }

            return true;
        }
    }
}
