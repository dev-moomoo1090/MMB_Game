using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public partial class MilitaryManager : MonoBehaviour
    {
        private readonly List<DelayedEffect> pendingEffects = new List<DelayedEffect>();
        private readonly List<MilitaryAction> militaryActions = new List<MilitaryAction>();

        private BoardManager boardManager;
        public BoardManager BoardManager => boardManager;

        public int ApplyCostModifier(int baseCost, PieceColor color)
        {
            return ApplyCostModifier(baseCost, color, string.Empty);
        }

        public int ApplyCostModifier(int baseCost, PieceColor color, string actionName)
        {
            if (HonorPiecePassiveSystem.Instance != null && HonorPiecePassiveSystem.Instance.TryConsumeFreeMilitaryAction(color, actionName))
            {
                return 0;
            }

            return Mathf.Max(0, Mathf.CeilToInt(baseCost * KingStateEvaluator.GetMilitaryCostMultiplier(color)));
        }

        public void Initialize()
        {
            boardManager = SceneComponentResolver.Resolve<BoardManager>();
            pendingEffects.Clear();
            militaryActions.Clear();
            militaryActions.Add(new BarricadeAction());
            militaryActions.Add(new RoadPlanAction());
            militaryActions.Add(new TrebuchetAction());
            militaryActions.Add(new BombardAction());
            militaryActions.Add(new OutpostAction());
            militaryActions.Add(new DivinePowerAction());
            militaryActions.Add(new MiracleAction());
            militaryActions.Add(new MilitaryExemptionAction());
            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
            EventBus.Instance.OnPhaseChanged += HandlePhaseChanged;
            EventBus.Instance.OnTurnChanged -= HandleTurnChanged;
            EventBus.Instance.OnTurnChanged += HandleTurnChanged;
        }

        private void OnDestroy()
        {
            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
            EventBus.Instance.OnTurnChanged -= HandleTurnChanged;
        }

        public void AddDelayedEffect(DelayedEffect effect)
        {
            if (effect != null)
            {
                pendingEffects.Add(effect);
            }
        }

        public bool ExecuteMilitaryAction(string actionName, ChessPiece target, int file, int rank, PieceColor actorColor)
        {
            QaLog.Write("행동", "군사 행동 시도 행동=" + actionName + " 색상=" + actorColor + " 대상=" + QaLog.PieceLabel(target) + " 위치=(" + file + "," + rank + ")");
            MilitaryAction action = null;
            for (int i = 0; i < militaryActions.Count; i++)
            {
                if (militaryActions[i].ActionName == actionName)
                {
                    action = militaryActions[i];
                    break;
                }
            }

            if (action == null)
            {
                QaLog.Write("행동", "군사 행동 실패 행동=" + actionName + " 사유=행동없음");
                return false;
            }

            if (action.RequiresPieceSelection && target == null)
            {
                QaLog.Write("행동", "군사 행동 실패 행동=" + actionName + " 사유=대상필요");
                return false;
            }

            if (action.RequiresPositionSelection && (file < 0 || file > 7 || rank < 0 || rank > 7))
            {
                QaLog.Write("행동", "군사 행동 실패 행동=" + actionName + " 사유=위치필요또는범위밖 위치=(" + file + "," + rank + ")");
                return false;
            }

            if (target != null && target.color != actorColor)
            {
                QaLog.Write("행동", "군사 행동 실패 행동=" + actionName + " 사유=대상색상불일치 대상색상=" + target.color + " 행동자색상=" + actorColor);
                return false;
            }

            bool result = action.Execute(target, file, rank, actorColor, this);
            if (result)
            {
                EventBus.Instance.PublishActionExecuted(actorColor, actionName);
            }
            else
            {
                QaLog.Write("행동", "군사 행동 실패 행동=" + actionName + " 사유=실행결과실패");
            }

            return result;
        }
    }
}
