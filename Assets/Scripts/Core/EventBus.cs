using System;

namespace MMBGame
{
    public class EventBus
    {
        private static EventBus instance;

        public static EventBus Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EventBus();
                }

                return instance;
            }
        }

        public event Action<GamePhase> OnPhaseChanged;
        public event Action<PieceColor> OnTurnChanged;
        public event Action<PieceColor> OnGameEnded;
        public event Action<ChessPiece> OnPieceCapturePending;
        public event Action<ChessPiece> OnMovementRefused;
        public event Action<ChessPiece> OnRebellionTriggered;
        public event Action<ChessPiece> OnDefectionTriggered;
        public event Action<ChessPiece, int, string> OnSupportChanged;
        public event Action<PieceColor, string> OnActionExecuted;
        public event Action<ChessPiece> OnIntelligenceGathered;
        public event Action<string> OnReconResult;
        public event Action<ChessPiece, int, int> OnAutonomousMoveExecuted;

        private EventBus()
        {
        }

        public void PublishPhaseChanged(GamePhase newPhase)
        {
            OnPhaseChanged?.Invoke(newPhase);
        }

        public void PublishTurnChanged(PieceColor activeColor)
        {
            OnTurnChanged?.Invoke(activeColor);
        }

        public void PublishGameEnded(PieceColor winner)
        {
            OnGameEnded?.Invoke(winner);
        }

        public void PublishPieceCapturePending(ChessPiece piece)
        {
            OnPieceCapturePending?.Invoke(piece);
        }

        public void PublishMovementRefused(ChessPiece piece)
        {
            OnMovementRefused?.Invoke(piece);
        }

        public void PublishRebellionTriggered(ChessPiece piece)
        {
            OnRebellionTriggered?.Invoke(piece);
        }

        public void PublishDefectionTriggered(ChessPiece piece)
        {
            OnDefectionTriggered?.Invoke(piece);
        }

        public void PublishSupportChanged(ChessPiece piece, int delta, string reason)
        {
            OnSupportChanged?.Invoke(piece, delta, reason);
        }

        public void PublishActionExecuted(PieceColor color, string actionName)
        {
            OnActionExecuted?.Invoke(color, actionName);
        }

        public void PublishIntelligenceGathered(ChessPiece piece)
        {
            OnIntelligenceGathered?.Invoke(piece);
        }

        public void PublishReconResult(string actionName)
        {
            OnReconResult?.Invoke(actionName);
        }

        public void PublishAutonomousMoveExecuted(ChessPiece piece, int toFile, int toRank)
        {
            OnAutonomousMoveExecuted?.Invoke(piece, toFile, toRank);
        }
    }
}
