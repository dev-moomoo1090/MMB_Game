using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public class PoliticalManager : MonoBehaviour
    {
        private readonly List<PoliticalAction> politicalActions = new List<PoliticalAction>();
        private readonly Dictionary<PieceColor, string> lastActions = new Dictionary<PieceColor, string>();
        private BoardManager boardManager;
        private PoliticsManager politicsManager;

        public BoardManager BoardManager => boardManager;

        public void Initialize()
        {
            boardManager = FindObjectOfType<BoardManager>();
            politicsManager = FindObjectOfType<PoliticsManager>();
            lastActions.Clear();
            politicalActions.Clear();
            politicalActions.Add(new ReconAction());
            politicalActions.Add(new FeudalStateAction());
            politicalActions.Add(new InterrogationAction());
            politicalActions.Add(new BribeAction());
            politicalActions.Add(new IntelAction());
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

        public bool ExecutePoliticalAction(string actionName, ChessPiece target, PieceColor actorColor, int value = 0)
        {
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

        private void HandleActionExecuted(PieceColor color, string actionName)
        {
            lastActions[color] = actionName;
        }

        private void HandleTurnChanged(PieceColor color)
        {
            ClearIntelTargets();
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
