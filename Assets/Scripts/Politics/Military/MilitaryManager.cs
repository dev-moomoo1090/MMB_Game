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
                return false;
            }

            if (action.RequiresPieceSelection && target == null)
            {
                return false;
            }

            if (action.RequiresPositionSelection && (file < 0 || file > 7 || rank < 0 || rank > 7))
            {
                return false;
            }

            if (target != null && target.color != actorColor)
            {
                return false;
            }

            bool result = action.Execute(target, file, rank, actorColor, this);
            if (result)
            {
                EventBus.Instance.PublishActionExecuted(actorColor, actionName);
            }

            return result;
        }
    }
}
