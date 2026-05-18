using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public class PoliticalManager : MonoBehaviour
    {
        private readonly List<PoliticalAction> politicalActions = new List<PoliticalAction>();
        private readonly Dictionary<PieceColor, string> lastActions = new Dictionary<PieceColor, string>();
        private readonly Dictionary<PieceColor, List<string>> recentActions = new Dictionary<PieceColor, List<string>>();
        private readonly HashSet<PieceColor> isolationUsedColors = new HashSet<PieceColor>();
        private BoardManager boardManager;
        private PoliticsManager politicsManager;
        private int isolationPoliticsPhasesRemaining;

        public BoardManager BoardManager => boardManager;
        public int IsolationPoliticsPhasesRemaining => isolationPoliticsPhasesRemaining;
        public string LastFailureReason { get; private set; }

        public void Initialize()
        {
            boardManager = SceneComponentResolver.Resolve<BoardManager>();
            politicsManager = SceneComponentResolver.Resolve<PoliticsManager>();
            lastActions.Clear();
            recentActions.Clear();
            isolationUsedColors.Clear();
            isolationPoliticsPhasesRemaining = 0;
            LastFailureReason = null;
            politicalActions.Clear();
            politicalActions.Add(new ReconAction());
            politicalActions.Add(new FeudalStateAction());
            politicalActions.Add(new InterrogationAction());
            politicalActions.Add(new BribeAction());
            politicalActions.Add(new IntelAction());
            politicalActions.Add(new BetrayalContactAction());
            politicalActions.Add(new BetrayalInfoAction());
            politicalActions.Add(new BetrayalBlunderAction());
            politicalActions.Add(new BetrayalFactionAction());
            politicalActions.Add(new BetrayalAssassinationAction());
            politicalActions.Add(new AssassinationAction());
            politicalActions.Add(new PropagandaAction());
            politicalActions.Add(new CivilAidAction());
            politicalActions.Add(new VisitAction());
            politicalActions.Add(new FineAction());
            politicalActions.Add(new ExecutionAction());
            politicalActions.Add(new AgitationAction());
            politicalActions.Add(new ManipulationAction());

            EventBus.Instance.OnActionExecuted -= HandleActionExecuted;
            EventBus.Instance.OnActionExecuted += HandleActionExecuted;
            EventBus.Instance.OnTurnChanged -= HandleTurnChanged;
            EventBus.Instance.OnTurnChanged += HandleTurnChanged;
        }

        private void OnDestroy()
        {
            EventBus.Instance.OnActionExecuted -= HandleActionExecuted;
            EventBus.Instance.OnTurnChanged -= HandleTurnChanged;
        }

        public PlayerState GetActorState(PieceColor color)
        {
            if (politicsManager == null)
            {
                return null;
            }

            return politicsManager.GetCurrentPlayer(color);
        }

        public string GetLastOpponentAction(PieceColor myColor)
        {
            PieceColor opponent = myColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            if (lastActions.TryGetValue(opponent, out string action))
            {
                return action;
            }

            return null;
        }

        public string GetRecentOpponentActions(PieceColor myColor)
        {
            PieceColor opponent = myColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            if (!recentActions.TryGetValue(opponent, out List<string> actions) || actions.Count == 0)
            {
                return null;
            }

            return string.Join(", ", actions);
        }

        public bool ExecutePoliticalAction(string actionName, ChessPiece target, PieceColor actorColor, int value = 0)
        {
            LastFailureReason = null;
            PoliticalAction action = null;
            for (int i = 0; i < politicalActions.Count; i++)
            {
                if (politicalActions[i].ActionName == actionName)
                {
                    action = politicalActions[i];
                    break;
                }
            }

            if (action == null)
            {
                return false;
            }

            if (IsIsolationBlocking(actionName))
            {
                LastFailureReason = "쇄국 중에는 상대에게 영향을 주는 정치행동을 사용할 수 없습니다.";
                return false;
            }

            if (action.RequiresPieceSelection && target == null)
            {
                return false;
            }

            if (action.RequiresValueInput && value <= 0)
            {
                return false;
            }

            bool result = action.Execute(target, actorColor, this, value);
            if (result)
            {
                EventBus.Instance.PublishActionExecuted(actorColor, actionName);
            }

            return result;
        }

        public bool CanExecuteIsolation(PieceColor actorColor)
        {
            return actorColor != PieceColor.None &&
                KingStateEvaluator.IsDictatorship(actorColor) &&
                !isolationUsedColors.Contains(actorColor) &&
                isolationPoliticsPhasesRemaining <= 0;
        }

        public bool ExecuteIsolation(PieceColor actorColor)
        {
            LastFailureReason = null;
            if (!CanExecuteIsolation(actorColor))
            {
                LastFailureReason = "쇄국은 독재 상태에서 게임당 한 번만 사용할 수 있습니다.";
                return false;
            }

            isolationUsedColors.Add(actorColor);
            isolationPoliticsPhasesRemaining = 7;
            EventBus.Instance.PublishActionExecuted(actorColor, "쇄국");
            return true;
        }

        private void HandleActionExecuted(PieceColor color, string actionName)
        {
            lastActions[color] = actionName;
            if (!recentActions.TryGetValue(color, out List<string> actions))
            {
                actions = new List<string>();
                recentActions[color] = actions;
            }

            actions.Insert(0, actionName);
            while (actions.Count > 3)
            {
                actions.RemoveAt(actions.Count - 1);
            }
        }

        private void HandleTurnChanged(PieceColor color)
        {
            if (isolationPoliticsPhasesRemaining > 0)
            {
                isolationPoliticsPhasesRemaining--;
            }

            ClearIntelTargets();
        }

        private bool IsIsolationBlocking(string actionName)
        {
            return isolationPoliticsPhasesRemaining > 0 && IsOpponentAffectingPoliticalAction(actionName);
        }

        private bool IsOpponentAffectingPoliticalAction(string actionName)
        {
            switch (actionName)
            {
                case "매수":
                case "제후국":
                case "선동":
                case "여론조작":
                case "배신 - 접촉":
                case "배신 - 정보":
                case "배신 - 실책":
                case "배신 - 파벌":
                case "배신 - 암살":
                case "암살":
                    return true;
                default:
                    return false;
            }
        }

        private void ClearIntelTargets()
        {
            if (boardManager == null || boardManager.BoardState == null)
            {
                return;
            }

            List<ChessPiece> pieces = boardManager.BoardState.GetAllPieces();
            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece != null)
                {
                    piece.isIntelTarget = false;
                }
            }
        }
    }
}
