using System.Collections.Generic;
using System.Linq;

namespace MMBGame.AI
{
    /// <summary>
    /// AI가 게임 상태에 접근하기 위한 유틸리티 래퍼.
    /// 순수 C# 클래스 — MonoBehaviour 상속 없음.
    /// </summary>
    public class AIContext
    {
        private readonly GameManager _gm;

        public AIContext(GameManager gm) { _gm = gm; }

        public BoardState        BoardState       => _gm.BoardManager.BoardState;
        public BoardManager      BoardManager     => _gm.BoardManager;
        public PoliticsManager   PoliticsManager  => _gm.PoliticsManager;
        public MilitaryManager   MilitaryManager  => _gm.MilitaryManager;
        public PoliticalManager  PoliticalManager => _gm.PoliticalManager;

        /// <summary>보드 위 기물 + offBoardPieces(후방배치) 모두 포함.</summary>
        public List<ChessPiece> GetAllPieces(PieceColor color)
        {
            var result = new List<ChessPiece>();
            foreach (var p in BoardState.GetAllPieces())
                if (p.color == color) result.Add(p);
            foreach (var p in BoardState.offBoardPieces)
                if (p.color == color && !result.Contains(p)) result.Add(p);
            return result;
        }

        /// <summary>보드 위에만 있는 기물 (후방배치 제외).</summary>
        public List<ChessPiece> GetActivePieces(PieceColor color)
            => BoardState.GetAllPieces().Where(p => p.color == color).ToList();

        public List<ChessPiece> GetControlledActivePieces(PieceColor color)
            => BoardState.GetAllPieces().Where(p => p.GetMovementControllerColor() == color).ToList();

        public PlayerState GetPlayerState(PieceColor color)
            => _gm.PoliticsManager.GetCurrentPlayer(color);

        public KingState GetKingState(PieceColor color)
            => KingStateEvaluator.Evaluate(GetPlayerState(color), BoardState);

        public bool IsBenevolent(PieceColor color)
            => KingStateEvaluator.IsBenevolent(color) || GetKingState(color) == KingState.Sage;

        public bool IsDictatorship(PieceColor color)
            => KingStateEvaluator.IsDictatorship(color) || GetKingState(color) == KingState.Autocrat;

        public PieceColor Opponent(PieceColor color)
            => color == PieceColor.White ? PieceColor.Black : PieceColor.White;

        public bool IsSpecial(ChessPiece p)
            => PieceClassifier.IsObstacle(p);

        public static int PieceValue(PieceType t)
        {
            switch (t)
            {
                case PieceType.King:   return 10000;
                default:               return BoardEvaluator.GetPieceValue(t) * 100;
            }
        }
    }
}
