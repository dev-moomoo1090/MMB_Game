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
        public event Action<ChessPiece> OnRebellionAttempt;
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
            QaLog.Write("이벤트", "페이즈 변경 페이즈=" + newPhase);
            OnPhaseChanged?.Invoke(newPhase);
        }

        public void PublishTurnChanged(PieceColor activeColor)
        {
            QaLog.Write("이벤트", "턴 변경 색상=" + activeColor);
            OnTurnChanged?.Invoke(activeColor);
        }

        public void PublishGameEnded(PieceColor winner)
        {
            QaLog.Write("이벤트", "게임 종료 승자=" + winner);
            OnGameEnded?.Invoke(winner);
        }

        public void PublishPieceCapturePending(ChessPiece piece)
        {
            QaLog.Write("이벤트", "포획 처리 대기 기물=" + QaLog.PieceLabel(piece));
            OnPieceCapturePending?.Invoke(piece);
        }

        public void PublishMovementRefused(ChessPiece piece)
        {
            QaLog.Write("이벤트", "명령 거부 기물=" + QaLog.PieceLabel(piece));
            OnMovementRefused?.Invoke(piece);
        }

        public void PublishRebellionTriggered(ChessPiece piece)
        {
            QaLog.Write("이벤트", "반란 이벤트 기물=" + QaLog.PieceLabel(piece));
            OnRebellionTriggered?.Invoke(piece);
        }

        public void PublishRebellionAttempt(ChessPiece piece)
        {
            QaLog.Write("이벤트", "반란 시도 이벤트 기물=" + QaLog.PieceLabel(piece));
            OnRebellionAttempt?.Invoke(piece);
        }

        public void PublishDefectionTriggered(ChessPiece piece)
        {
            QaLog.Write("이벤트", "배신 이벤트 기물=" + QaLog.PieceLabel(piece));
            OnDefectionTriggered?.Invoke(piece);
        }

        public void PublishSupportChanged(ChessPiece piece, int delta, string reason)
        {
            QaLog.Write("이벤트", "지지도 변경 이벤트 사유=" + reason + " 변화량=" + delta + " 기물=" + QaLog.PieceLabel(piece));
            OnSupportChanged?.Invoke(piece, delta, reason);
        }

        public void PublishActionExecuted(PieceColor color, string actionName)
        {
            QaLog.Write("이벤트", "행동 실행 색상=" + color + " 행동=" + actionName);
            OnActionExecuted?.Invoke(color, actionName);
        }

        public void PublishIntelligenceGathered(ChessPiece piece)
        {
            QaLog.Write("이벤트", "정보 획득 기물=" + QaLog.PieceLabel(piece));
            OnIntelligenceGathered?.Invoke(piece);
        }

        public void PublishReconResult(string actionName)
        {
            QaLog.Write("이벤트", "정찰 결과 행동=" + actionName);
            OnReconResult?.Invoke(actionName);
        }

        public void PublishAutonomousMoveExecuted(ChessPiece piece, int toFile, int toRank)
        {
            QaLog.Write("이벤트", "자율 이동 기물=" + QaLog.PieceLabel(piece) + " 도착=(" + toFile + "," + toRank + ")");
            OnAutonomousMoveExecuted?.Invoke(piece, toFile, toRank);
        }
    }
}
