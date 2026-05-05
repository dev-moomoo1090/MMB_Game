using UnityEngine;

namespace MMBGame
{
    public enum GamePhase
    {
        PoliticsPhase,
        ChessPhase
    }

    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private PieceColor currentColor;
        [SerializeField] private GamePhase currentPhase;

        public PieceColor CurrentColor => currentColor;
        public GamePhase CurrentPhase => currentPhase;

        public void StartGame()
        {
            currentColor = PieceColor.White;
            currentPhase = GamePhase.PoliticsPhase;
            EventBus.Instance.PublishTurnChanged(currentColor);
            EventBus.Instance.PublishPhaseChanged(currentPhase);
        }

        public void EndPhase()
        {
            if (currentPhase == GamePhase.PoliticsPhase)
            {
                currentPhase = GamePhase.ChessPhase;
                EventBus.Instance.PublishPhaseChanged(currentPhase);
                return;
            }

            currentColor = currentColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            currentPhase = GamePhase.PoliticsPhase;
            EventBus.Instance.PublishTurnChanged(currentColor);
            EventBus.Instance.PublishPhaseChanged(currentPhase);
        }
    }
}
