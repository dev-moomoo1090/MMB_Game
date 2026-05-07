using UnityEngine;

namespace MMBGame
{
    public enum GamePhase
    {
        PoliticsPhase,
        ChessPhase,
        PrisonerPhase
    }

    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private PieceColor currentColor;
        [SerializeField] private GamePhase currentPhase;

        private int politicsActionsRemaining;
        private int whiteQueuedPoliticsBonus;
        private int blackQueuedPoliticsBonus;

        public PieceColor CurrentColor => currentColor;
        public GamePhase CurrentPhase => currentPhase;
        public int PoliticsActionsRemaining => politicsActionsRemaining;

        public void StartGame()
        {
            currentColor = PieceColor.White;
            currentPhase = GamePhase.PoliticsPhase;
            politicsActionsRemaining = GetPoliticsActionsForTurn(currentColor);
            EventBus.Instance.PublishTurnChanged(currentColor);
            EventBus.Instance.PublishPhaseChanged(currentPhase);
        }

        public void EndPhase()
        {
            if (currentPhase == GamePhase.PoliticsPhase)
            {
                politicsActionsRemaining--;
                if (politicsActionsRemaining > 0)
                {
                    return;
                }

                currentPhase = GamePhase.ChessPhase;
                EventBus.Instance.PublishPhaseChanged(currentPhase);
                return;
            }

            currentColor = currentColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            currentPhase = GamePhase.PoliticsPhase;
            politicsActionsRemaining = GetPoliticsActionsForTurn(currentColor);
            EventBus.Instance.PublishTurnChanged(currentColor);
            EventBus.Instance.PublishPhaseChanged(currentPhase);
        }

        public void AddPoliticsActions(int amount)
        {
            if (amount > 0 && currentPhase == GamePhase.PoliticsPhase)
            {
                politicsActionsRemaining += amount;
            }
        }

        public bool TryConsumeFreePoliticsAction()
        {
            if (currentPhase != GamePhase.PoliticsPhase)
            {
                return false;
            }

            politicsActionsRemaining++;
            return true;
        }

        public bool TryQueuePoliticsSkipBonus(PieceColor color)
        {
            if (currentPhase != GamePhase.PoliticsPhase || currentColor != color || !KingStateEvaluator.IsBenevolent(color))
            {
                return false;
            }

            SetQueuedPoliticsBonus(color, 1);
            politicsActionsRemaining = 1;
            return true;
        }

        public bool CanQueuePoliticsSkipBonus(PieceColor color)
        {
            return currentPhase == GamePhase.PoliticsPhase && currentColor == color && KingStateEvaluator.IsBenevolent(color);
        }

        public void EnterPrisonerPhase()
        {
            currentPhase = GamePhase.PrisonerPhase;
            EventBus.Instance.PublishPhaseChanged(currentPhase);
        }

        private int GetPoliticsActionsForTurn(PieceColor color)
        {
            int queuedBonus = ConsumeQueuedPoliticsBonus(color);
            return Mathf.Min(2, 1 + queuedBonus);
        }

        private int ConsumeQueuedPoliticsBonus(PieceColor color)
        {
            if (color == PieceColor.White)
            {
                int bonus = whiteQueuedPoliticsBonus;
                whiteQueuedPoliticsBonus = 0;
                return bonus;
            }

            if (color == PieceColor.Black)
            {
                int bonus = blackQueuedPoliticsBonus;
                blackQueuedPoliticsBonus = 0;
                return bonus;
            }

            return 0;
        }

        private void SetQueuedPoliticsBonus(PieceColor color, int amount)
        {
            int clamped = Mathf.Clamp(amount, 0, 1);
            if (color == PieceColor.White)
            {
                whiteQueuedPoliticsBonus = clamped;
            }
            else if (color == PieceColor.Black)
            {
                blackQueuedPoliticsBonus = clamped;
            }
        }
    }
}
