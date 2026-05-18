using UnityEngine;

namespace MMBGame
{
    public static class ObedienceSystem
    {
        public static bool IsRefused(ChessPiece piece)
        {
            return IsRefused(piece, null);
        }

        public static bool IsRefused(ChessPiece piece, BoardState state)
        {
            return IsRefused(piece, state, piece != null ? piece.file : -1, piece != null ? piece.rank : -1);
        }

        public static bool IsRefused(ChessPiece piece, BoardState state, int targetFile, int targetRank)
        {
            if (piece == null)
            {
                QaLog.Write("명령수락", "기물이 null이라 판정 생략");
                return false;
            }

            PieceColor controllerColor = piece.GetMovementControllerColor();
            if (KingStateEvaluator.SuppressesRefusal(controllerColor))
            {
                QaLog.Write("명령수락", "정치상태 효과로 자동 수락 기물=" + QaLog.PieceLabel(piece) + " 지휘권=" + controllerColor);
                return false;
            }

            return RollRefusal(piece, state, targetFile, targetRank);
        }

        public static bool RollRefusal(ChessPiece piece)
        {
            return RollRefusal(piece, null);
        }

        public static bool RollRefusal(ChessPiece piece, BoardState state)
        {
            return RollRefusal(piece, state, piece != null ? piece.file : -1, piece != null ? piece.rank : -1);
        }

        public static bool RollRefusal(ChessPiece piece, BoardState state, int targetFile, int targetRank)
        {
            MovementAcceptanceBreakdown breakdown = GetMovementAcceptanceBreakdown(piece, state, targetFile, targetRank);
            float refusalChance = Mathf.Clamp(100f - breakdown.acceptanceChance, 0f, 100f);
            if (refusalChance <= 0f)
            {
                QaLog.Write("명령수락", "굴림 없이 수락 기물=" + QaLog.PieceLabel(piece) + " 목적지=(" + breakdown.targetFile + "," + breakdown.targetRank + ") 공식=50+지지도(" + breakdown.support + ")+수비자(" + breakdown.defenders + "*50)-공격자(" + breakdown.attackers + "*50)+전역(" + breakdown.globalWeight + ")+개별가중치(" + breakdown.pieceWeight + ") 원값=" + breakdown.rawChance + " 수락확률=" + breakdown.acceptanceChance + " 거부확률=" + refusalChance);
                return false;
            }

            float roll = Random.Range(0f, 100f);
            bool refused = roll < refusalChance;
            QaLog.Write("명령수락", (refused ? "거부" : "수락") + " 기물=" + QaLog.PieceLabel(piece) + " 목적지=(" + breakdown.targetFile + "," + breakdown.targetRank + ") 공식=50+지지도(" + breakdown.support + ")+수비자(" + breakdown.defenders + "*50)-공격자(" + breakdown.attackers + "*50)+전역(" + breakdown.globalWeight + ")+개별가중치(" + breakdown.pieceWeight + ") 원값=" + breakdown.rawChance + " 수락확률=" + breakdown.acceptanceChance + " 거부확률=" + refusalChance + " 굴림=" + roll);
            return refused;
        }

        public static float GetRefusalChance(ChessPiece piece)
        {
            return GetRefusalChance(piece, null);
        }

        public static float GetRefusalChance(ChessPiece piece, BoardState state)
        {
            if (piece == null)
            {
                return 0f;
            }

            float acceptanceChance = GetMovementAcceptanceChance(piece, state);
            return Mathf.Clamp(100f - acceptanceChance, 0f, 100f);
        }

        public static float GetMovementAcceptanceChance(ChessPiece piece, BoardState state)
        {
            return GetMovementAcceptanceBreakdown(piece, state).acceptanceChance;
        }

        public static float GetMovementAcceptanceChance(ChessPiece piece, BoardState state, int targetFile, int targetRank)
        {
            return GetMovementAcceptanceBreakdown(piece, state, targetFile, targetRank).acceptanceChance;
        }

        private static MovementAcceptanceBreakdown GetMovementAcceptanceBreakdown(ChessPiece piece, BoardState state)
        {
            return GetMovementAcceptanceBreakdown(piece, state, piece != null ? piece.file : -1, piece != null ? piece.rank : -1);
        }

        private static MovementAcceptanceBreakdown GetMovementAcceptanceBreakdown(ChessPiece piece, BoardState state, int targetFile, int targetRank)
        {
            if (piece == null)
            {
                return new MovementAcceptanceBreakdown();
            }

            int defenders = 0;
            int attackers = 0;
            int globalWeight = state != null ? state.globalAcceptanceWeight : 0;
            PieceColor controllerColor = piece.GetMovementControllerColor();
            globalWeight += KingStateEvaluator.GetAcceptanceWeightModifier(controllerColor);
            if (state != null && state.IsInBounds(targetFile, targetRank))
            {
                PieceColor opponent = controllerColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
                defenders = BoardEvaluator.CountDefenders(state, targetFile, targetRank, controllerColor);
                attackers = BoardEvaluator.CountAttackers(state, targetFile, targetRank, opponent);
            }

            int pieceWeight = GetPieceAcceptanceWeight(piece);
            float rawChance = 50 + piece.support + defenders * 50 - attackers * 50 + globalWeight + pieceWeight;
            MovementAcceptanceBreakdown breakdown = new MovementAcceptanceBreakdown();
            breakdown.targetFile = targetFile;
            breakdown.targetRank = targetRank;
            breakdown.support = piece.support;
            breakdown.defenders = defenders;
            breakdown.attackers = attackers;
            breakdown.globalWeight = globalWeight;
            breakdown.pieceWeight = pieceWeight;
            breakdown.rawChance = rawChance;
            breakdown.acceptanceChance = Mathf.Clamp(rawChance, 0f, 100f);
            return breakdown;
        }

        public static float GetGeneralAcceptanceChance(ChessPiece piece, BoardState state)
        {
            if (piece == null)
            {
                return 0f;
            }

            int globalWeight = state != null ? state.globalAcceptanceWeight : 0;
            globalWeight += KingStateEvaluator.GetAcceptanceWeightModifier(piece.GetMovementControllerColor());
            return Mathf.Clamp(50 + piece.support + globalWeight + GetPieceAcceptanceWeight(piece), 0f, 100f);
        }

        private static int GetPieceAcceptanceWeight(ChessPiece piece)
        {
            return piece.acceptWeight - 50 + piece.disposition;
        }

        private struct MovementAcceptanceBreakdown
        {
            public int targetFile;
            public int targetRank;
            public int support;
            public int defenders;
            public int attackers;
            public int globalWeight;
            public int pieceWeight;
            public float rawChance;
            public float acceptanceChance;
        }
    }
}
