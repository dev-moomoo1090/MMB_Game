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
            if (actor == null || !actor.SpendGold(value * 3))
            {
                return false;
            }

            float bonus = value / Math.Max(1f, target.taxPerTurn);
            float beforeRebellionWeight = target.rebellionWeight;
            target.rebellionWeight += bonus;
            QaLog.Write("기물가중치", "반란 가중치 변경 행동=" + ActionName + " 기물=" + QaLog.PieceLabel(target) + " 변화량=" + bonus + " 이전=" + beforeRebellionWeight + " 이후=" + target.rebellionWeight);
            float chance = Mathf.Clamp((20 - target.support) * 1.5f + bonus, 0f, 90f);
            for (int i = 0; i < 3; i++)
            {
                float roll = UnityEngine.Random.Range(0f, 100f);
                if (roll < chance)
                {
                    QaLog.Write("반란", "제후국 반란 판정 성공 기물=" + QaLog.PieceLabel(target) + " 공식=(20-지지도(" + target.support + "))*1.5+보너스(" + bonus + ") 확률=" + chance + " 굴림=" + roll + " 시도=" + (i + 1));
                    EventBus.Instance.PublishRebellionTriggered(target);
                    return true;
                }

                QaLog.Write("반란", "제후국 반란 판정 실패 기물=" + QaLog.PieceLabel(target) + " 공식=(20-지지도(" + target.support + "))*1.5+보너스(" + bonus + ") 확률=" + chance + " 굴림=" + roll + " 시도=" + (i + 1));
            }

            return true;
        }
    }
}
