using System.Linq;
using UnityEngine;

namespace MMBGame.AI
{
    /// <summary>
    /// 군사 단계 이동 점수 계산 (의사코드 Part 6 EvaluateMove).
    /// 순수 정적 클래스 — 사이드이펙트 없음.
    /// </summary>
    public static class MoveEvaluator
    {
        public static float Evaluate(ChessPiece piece, Move move, BoardState state, PieceColor aiSide)
        {
            float score = 0f;
            PieceColor enemy = aiSide == PieceColor.White ? PieceColor.Black : PieceColor.White;
            BoardState sim = MoveValidator.SimulateMove(state, move);

            // 1. 기물 가치 합산 (간이)
            score += EvalMaterial(sim, aiSide);

            // 2. 위치 테이블 적용
            score += GetPositionTableValue(piece.type, move.toFile, move.toRank, aiSide);

            // 3. 체크메이트 판정
            var enemyMoves = MoveValidator.GetAllLegalMoves(sim, enemy);
            if (enemyMoves.Count == 0 && CheckDetector.IsInCheck(sim, enemy))
                return 100000f;

            // 4. 체크 부여 보너스
            if (CheckDetector.IsInCheck(sim, enemy))
                score += 50f;

            // 5. 기물 교환 평가 (성군: 교환 억제)
            var captured = state.GetPiece(move.toFile, move.toRank);
            if (captured != null && captured.color == enemy)
            {
                int capturedVal = AIContext.PieceValue(captured.type);
                int myVal       = AIContext.PieceValue(piece.type);
                int netGain     = capturedVal - myVal;
                score += netGain > 0 ? netGain * 8f : netGain * 20f;
                score -= myVal * 3f;  // 교환 후 지지도 하락 페널티 (성군 특성)
            }

            // 6. 반란 위험 기물 이동 억제
            if (piece.rebellionWeight >= 0.35f)
                score -= 60f;

            // 7. 공격받는 칸에서 벗어나는 이동 보너스
            if (CheckDetector.IsSquareAttacked(state, piece.file, piece.rank, enemy) &&
                !CheckDetector.IsSquareAttacked(sim, move.toFile, move.toRank, enemy))
                score += 40f;

            // 8. 이동 후 적 기물 위협 보너스
            foreach (var ep in sim.GetAllPieces().Where(p => p.color == enemy))
            {
                if (CheckDetector.IsSquareAttacked(sim, ep.file, ep.rank, aiSide))
                    score += AIContext.PieceValue(ep.type) * 2f;
            }

            return score;
        }

        // ── 기물 가치 합산 ────────────────────────────────────────────────
        private static float EvalMaterial(BoardState state, PieceColor aiSide)
        {
            PieceColor enemy = aiSide == PieceColor.White ? PieceColor.Black : PieceColor.White;
            float myVal = 0f, enemyVal = 0f;
            foreach (var p in state.GetAllPieces())
            {
                if (p.type == PieceType.Barricade || p.type == PieceType.Trebuchet) continue;
                if (p.color == aiSide)  myVal    += AIContext.PieceValue(p.type);
                else if (p.color == enemy) enemyVal += AIContext.PieceValue(p.type);
            }
            return (myVal - enemyVal) * 10f;
        }

        // ── 위치 테이블 (간이) — 중앙 4칸 선호 ──────────────────────────
        private static float GetPositionTableValue(PieceType type, int file, int rank, PieceColor side)
        {
            float centerBonus = Mathf.Max(0f,
                3f - Mathf.Max(Mathf.Abs(file - 3.5f), Mathf.Abs(rank - 3.5f)));
            switch (type)
            {
                case PieceType.Pawn:
                    return centerBonus * 3f + (side == PieceColor.White ? rank : 7 - rank) * 2f;
                case PieceType.Knight:
                    return centerBonus * 8f;
                case PieceType.Bishop:
                    return centerBonus * 4f;
                case PieceType.Rook:
                    return (rank == 6 || rank == 7) ? 10f : centerBonus * 2f;
                case PieceType.Queen:
                    return centerBonus * 3f;
                case PieceType.King:
                    return -centerBonus * 5f;  // 킹은 중앙 회피
                default:
                    return 0f;
            }
        }
    }
}
