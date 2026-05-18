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
            if (target == null || manager == null || manager.BoardManager == null)
            {
                manager?.SetLastFailureReason("대상을 선택해야 합니다.");
                return false;
            }

            if (target.color == actorColor)
            {
                manager.SetLastFailureReason("적 기물을 선택해야 합니다.");
                return false;
            }

            if (target.isBetrayed)
            {
                manager.SetLastFailureReason("이미 배신한 기물입니다.");
                return false;
            }

            PlayerState actor = manager.GetActorState(actorColor);
            int cost = value * 3;
            if (actor == null || !actor.SpendGold(cost))
            {
                manager.SetLastFailureReason("골드 부족: " + cost + " 필요 (보유: " + (actor != null ? actor.gold : 0) + ")");
                return false;
            }

            int bonus = value / Math.Max(1, target.taxPerTurn);
            target.defectionWeight += bonus;
            QaLog.Write("기물가중치", "배신 가중치 변경 행동=" + ActionName + " 기물=" + QaLog.PieceLabel(target) + " 변화량=" + bonus + " 이후=" + target.defectionWeight);

            BoardState state = manager.BoardManager.BoardState;
            float chance = RebellionSystem.GetDefectionChance(target, state);
            for (int i = 0; i < 3; i++)
            {
                float roll = UnityEngine.Random.Range(0f, 100f);
                if (roll < chance)
                {
                    target.isBetrayed = true;
                    manager.BoardManager.RefreshPieceVisuals();
                    EventBus.Instance.PublishDefectionTriggered(target);
                    QaLog.Write("배신", "매수 판정 성공 기물=" + QaLog.PieceLabel(target) + " 공식=30-지지도(" + target.support + ")+전역(" + (state != null ? state.globalDefectionWeight : 0) + ")+개별가중치(" + target.defectionWeight + ")+정치형태보정 확률=" + chance + " 굴림=" + roll + " 시도=" + (i + 1) + "/3 배신상태=true");
                    return true;
                }

                QaLog.Write("배신", "매수 판정 실패 기물=" + QaLog.PieceLabel(target) + " 공식=30-지지도(" + target.support + ")+전역(" + (state != null ? state.globalDefectionWeight : 0) + ")+개별가중치(" + target.defectionWeight + ")+정치형태보정 확률=" + chance + " 굴림=" + roll + " 시도=" + (i + 1) + "/3");
            }

            return false;
        }
    }
}
