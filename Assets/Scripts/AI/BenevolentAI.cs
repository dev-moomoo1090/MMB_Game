using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MMBGame.AI
{
    /// <summary>
    /// 성군(Sage) AI — STABLE 모드 1단계 MVP.
    /// 순수 C# 클래스, MonoBehaviour 상속 없음.
    ///
    /// 사용법:
    ///   var ai = new BenevolentAI(GameManager.Instance, PieceColor.Black);
    ///   ai.TakeTurn();  // 정치 → 군사 → 턴 종료 한 번에 실행
    /// </summary>
    public class BenevolentAI
    {
        private readonly AIContext _ctx;
        private readonly PieceColor _side;

        public BenevolentAI(GameManager gameManager, PieceColor side)
        {
            _ctx  = new AIContext(gameManager);
            _side = side;
        }

        // ─────────────────────────────────────────────────────────────────
        // PART 8 — 메인 루프
        // ─────────────────────────────────────────────────────────────────
        public void TakeTurn()
        {
            ExecutePoliticalPhase();
            ExecuteMilitaryPhase();
            ExecuteTurnEnd();
        }

        // ─────────────────────────────────────────────────────────────────
        // PART 5 — 정치 단계
        // ─────────────────────────────────────────────────────────────────
        private void ExecutePoliticalPhase()
        {
            HandlePieceEvents();

            int actionCount = GetBaseActionCount();
            var usedActions = new HashSet<string>();

            while (actionCount > 0)
            {
                ActionCandidate action = SelectBestAction(usedActions);
                if (action.name == null) break;

                bool ok = ExecuteAction(action);
                if (ok) usedActions.Add(action.name);
                actionCount--;
            }
        }

        private int GetBaseActionCount()
        {
            int count = 2;  // 기본 행동 횟수
            if (_ctx.IsBenevolent(_side)) count++;  // 성군: +1
            return count;
        }

        // ─────────────────────────────────────────────────────────────────
        // 행동 선택
        // ─────────────────────────────────────────────────────────────────
        private ActionCandidate SelectBestAction(HashSet<string> used)
        {
            var myPieces = _ctx.GetAllPieces(_side);
            var active   = myPieces.Where(p => !_ctx.IsSpecial(p)).ToList();
            var player   = _ctx.GetPlayerState(_side);
            int gold     = player.gold;
            int honor    = player.honor;
            float avgSup = AverageSupport(active);
            PieceColor enemy = _ctx.Opponent(_side);

            var cands = new List<ActionCandidate>();

            // ── 위기: 지지도 60 이하 기물에 특세 ──────────────────────
            if (!used.Contains("SpecialTax"))
            {
                foreach (var p in active.Where(p => p.support <= 60))
                    cands.Add(new ActionCandidate("SpecialTax", p, 0, ActionKind.Fiscal,
                                                  900f + (60 - p.support) * 5f));
            }

            // ── 성군 조건 위기: 평균 지지도/명예 부족 시 선전 ──────────
            if (!used.Contains("선전"))
            {
                if (avgSup < 8f)
                    cands.Add(new ActionCandidate("선전", null, 0, ActionKind.Political, 880f));
                if (honor < 25)
                    cands.Add(new ActionCandidate("선전", null, 0, ActionKind.Political, 870f));
            }

            // ── 반란 위험 기물 후방배치 ────────────────────────────────
            if (!used.Contains("RearDeployAction"))
            {
                foreach (var p in active.Where(p => p.rebellionWeight >= 0.35f && !p.isOffBoard))
                {
                    if (!CheckDetector.IsSquareAttacked(_ctx.BoardState, p.file, p.rank, enemy))
                        cands.Add(new ActionCandidate("RearDeployAction", p, 0, ActionKind.Fiscal,
                                                      800f + p.rebellionWeight * 100f));
                }
            }

            // ── 일반 지지도 관리: 특세 (지지도 최저 기물) ─────────────
            if (!used.Contains("SpecialTax") && active.Count > 0)
            {
                var lowest = active.OrderBy(p => p.support).First();
                cands.Add(new ActionCandidate("SpecialTax", lowest, 0, ActionKind.Fiscal,
                                              700f + (100 - lowest.support)));
            }

            // ── 선전 (광역 지지도 상승) ────────────────────────────────
            if (!used.Contains("선전"))
            {
                float eff = avgSup / 20f;
                cands.Add(new ActionCandidate("선전", null, 0, ActionKind.Political,
                                              600f + eff * 10f));
            }

            // ── 지원 — 골드 여유 있을 때 가치 높은 기물 지원 ──────────
            if (gold >= 100 && !used.Contains("AidAction"))
            {
                var needsAid = active.Where(p => p.support < 60)
                                     .OrderByDescending(p => AIContext.PieceValue(p.type))
                                     .FirstOrDefault();
                if (needsAid != null)
                {
                    int spend = Mathf.Min((int)(gold * 0.25f), needsAid.taxPerTurn * 3);
                    cands.Add(new ActionCandidate("AidAction", needsAid, spend, ActionKind.Fiscal, 650f));
                }
            }

            // ── 징발 — 골드 부족 시 지지도 여유 있는 기물에서 ────────
            if (gold < 80 && !used.Contains("RequisitionAction"))
            {
                var richPiece = active.Where(p => p.support >= 70)
                                      .OrderByDescending(p => p.support)
                                      .FirstOrDefault();
                if (richPiece != null)
                    cands.Add(new ActionCandidate("RequisitionAction", richPiece,
                                                  richPiece.taxPerTurn * 2, ActionKind.Fiscal, 580f));
            }

            // ── 군사 강화: 전초기지 (나이트) ──────────────────────────
            if (!used.Contains("OutpostAction"))
            {
                var knight = GetSafePiece(active, PieceType.Knight, enemy);
                if (knight != null && knight.oneTimeMovePatterns.Count == 0)
                    cands.Add(new ActionCandidate("OutpostAction", knight, 0, ActionKind.Military, 450f));
            }

            // ── 군사 강화: 신의힘 (룩) — 포(cannon) 행마 추가 ─────────
            if (!used.Contains("DivinePowerAction"))
            {
                var rook = GetSafePiece(active, PieceType.Rook, enemy);
                if (rook != null && !rook.oneTimeMovePatterns.Any(mp => mp.isCannon))
                    cands.Add(new ActionCandidate("DivinePowerAction", rook, 0, ActionKind.Military, 440f));
            }

            // ── 군사 강화: 기적 (비숍) — 전방 1칸 행마 추가 ───────────
            if (!used.Contains("MiracleAction"))
            {
                var bishop = GetSafePiece(active, PieceType.Bishop, enemy);
                if (bishop != null && bishop.oneTimeMovePatterns.Count == 0)
                    cands.Add(new ActionCandidate("MiracleAction", bishop, 0, ActionKind.Military, 430f));
            }

            // ── 군사 강화: 군법면제 (폰) — 후방 1칸 행마 추가 ─────────
            if (!used.Contains("MilitaryExemptionAction"))
            {
                var pawn = GetSafePiece(active, PieceType.Pawn, enemy);
                if (pawn != null && pawn.oneTimeMovePatterns.Count == 0)
                    cands.Add(new ActionCandidate("MilitaryExemptionAction", pawn, 0, ActionKind.Military, 400f));
            }

            // ── 매수 씨앗 — 골드 여유 클 때만 ────────────────────────
            if (gold >= 300 && !used.Contains("매수"))
            {
                var target = SelectBetrayTarget();
                if (target != null)
                    cands.Add(new ActionCandidate("매수", target, target.taxPerTurn, ActionKind.Political, 350f));
            }

            // ── 정찰 (4턴마다) ─────────────────────────────────────────
            int approxTurn = _ctx.BoardState.moveHistory.Count / 2;
            if (approxTurn % 4 == 0 && !used.Contains("정찰"))
                cands.Add(new ActionCandidate("정찰", null, 0, ActionKind.Political, 300f));

            // ── 선동 (공짜) ────────────────────────────────────────────
            if (!used.Contains("선동"))
                cands.Add(new ActionCandidate("선동", null, 0, ActionKind.Political, 250f));

            if (cands.Count == 0) return ActionCandidate.None;
            return cands.OrderByDescending(c => c.score).First();
        }

        private bool ExecuteAction(ActionCandidate action)
        {
            switch (action.kind)
            {
                case ActionKind.Fiscal:
                    return _ctx.PoliticsManager.ExecuteFiscalAction(
                        action.name, action.target, action.value);

                case ActionKind.Military:
                    return _ctx.MilitaryManager.ExecuteMilitaryAction(
                        action.name, action.target,
                        action.target != null ? action.target.file : 0,
                        action.target != null ? action.target.rank : 0,
                        _side);

                case ActionKind.Political:
                    return _ctx.PoliticalManager.ExecutePoliticalAction(
                        action.name, action.target, _side, action.value);

                default:
                    return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // PART 6 — 군사 단계
        // ─────────────────────────────────────────────────────────────────
        private void ExecuteMilitaryPhase()
        {
            var movable = _ctx.GetActivePieces(_side)
                .Where(p => !p.isOffBoard && !_ctx.IsSpecial(p))
                .ToList();

            // 반란 기물 처리 (rebellionWeight >= 1.0 → 군사권 상대 이전 간주)
            var rebels = movable.Where(p => p.rebellionWeight >= 1.0f).ToList();
            foreach (var rp in rebels)
            {
                Move safeMove = GetSafestMove(rp);
                if (safeMove != null)
                    _ctx.BoardManager.TryMove(rp.file, rp.rank, safeMove.toFile, safeMove.toRank);
            }
            movable = movable.Where(p => p.rebellionWeight < 1.0f).ToList();

            // 전체 합법 이동 평가 및 정렬
            var scored = new List<(ChessPiece piece, Move move, float score)>();
            foreach (var piece in movable)
            {
                var legalMoves = MoveValidator.GetLegalMoves(_ctx.BoardState, piece.file, piece.rank);
                foreach (var mv in legalMoves)
                    scored.Add((piece, mv, MoveEvaluator.Evaluate(piece, mv, _ctx.BoardState, _side)));
            }
            scored.Sort((a, b) => b.score.CompareTo(a.score));

            TryMoveWithFallback(scored);
        }

        /// <summary>반란 기물용: 가장 안전한 칸으로 이동.</summary>
        private Move GetSafestMove(ChessPiece piece)
        {
            var moves = MoveValidator.GetLegalMoves(_ctx.BoardState, piece.file, piece.rank);
            if (moves.Count == 0) return null;
            PieceColor enemy = _ctx.Opponent(piece.color);
            foreach (var m in moves)
            {
                var sim = MoveValidator.SimulateMove(_ctx.BoardState, m);
                if (!CheckDetector.IsSquareAttacked(sim, m.toFile, m.toRank, enemy))
                    return m;
            }
            return moves[0];
        }

        // ─────────────────────────────────────────────────────────────────
        // PART 3 — 수락 판정 포함 이동 시도 (최대 3회)
        // ─────────────────────────────────────────────────────────────────
        private void TryMoveWithFallback(List<(ChessPiece piece, Move move, float score)> candidates)
        {
            var attempted = new HashSet<ChessPiece>();
            int maxTries = 3;
            int idx = 0;
            while (maxTries > 0 && idx < candidates.Count)
            {
                var (piece, move, _) = candidates[idx++];
                if (attempted.Contains(piece)) continue;  // 이미 시도한 기물 skip

                if (AcceptanceCheck(piece))
                {
                    _ctx.BoardManager.TryMove(piece.file, piece.rank,
                                              move.toFile, move.toRank, move.promotionPiece);
                    return;
                }
                attempted.Add(piece);
                maxTries--;
            }
            // 3회 모두 거부 → 군사 단계 스킵
        }

        private bool AcceptanceCheck(ChessPiece piece)
        {
            if (_ctx.IsDictatorship(_side)) return true;

            // acceptWeight: int (-30 ~ +30) → float (-0.3 ~ +0.3) 변환
            float rate = piece.support / 100f + piece.acceptWeight / 100f;
            if (_ctx.IsBenevolent(_side)) rate += 0.1f;
            rate = Mathf.Clamp(rate, 0.05f, 0.99f);
            return Random.value < rate;
        }

        // ─────────────────────────────────────────────────────────────────
        // PART 7 — 턴 종료: 포로 처분
        // ─────────────────────────────────────────────────────────────────
        private void ExecuteTurnEnd()
        {
            var player = _ctx.GetPlayerState(_side);
            // 성군 AI: 절대 처형 안 함. 명예 위기 → 석방, 골드 부족 → 몸값, 그 외 → 석방.
            // 포로(prisoners) 목록은 향후 게임 시스템에서 주입 시 연결.
            PrisonerFate _ = DecidePrisonerFate(player.honor, player.gold);
        }

        private PrisonerFate DecidePrisonerFate(int honor, int gold)
        {
            if (honor < 30) return PrisonerFate.Release;
            if (gold < 100) return PrisonerFate.Ransom;
            return PrisonerFate.Release;
        }

        // ─────────────────────────────────────────────────────────────────
        // PART 4 — 이벤트 핸들러 (1단계 고정값)
        // ─────────────────────────────────────────────────────────────────
        private void HandlePieceEvents()
        {
            // Phase 6 기물 성격 시스템 완성 후 연결.
            // 1단계 STABLE MVP: 이벤트 시스템 미구현이므로 no-op.
        }

        // ─────────────────────────────────────────────────────────────────
        // 유틸리티
        // ─────────────────────────────────────────────────────────────────

        /// <summary>해당 타입의 기물 중 공격받지 않는 가장 첫 번째 기물 반환.</summary>
        private ChessPiece GetSafePiece(List<ChessPiece> pieces, PieceType type, PieceColor enemy)
        {
            return pieces
                .Where(p => p.type == type && !p.isOffBoard && !_ctx.IsSpecial(p))
                .FirstOrDefault(p => !CheckDetector.IsSquareAttacked(
                                         _ctx.BoardState, p.file, p.rank, enemy));
        }

        private ChessPiece SelectBetrayTarget()
        {
            var enemyPieces = _ctx.GetActivePieces(_ctx.Opponent(_side))
                .Where(p => !p.isBetrayed && !_ctx.IsSpecial(p))
                .ToList();
            if (enemyPieces.Count == 0) return null;

            // 퀸 → 비숍 → 가치 대비 세금 효율 최고 기물
            var queen = enemyPieces.FirstOrDefault(p => p.type == PieceType.Queen);
            if (queen != null) return queen;
            var bishop = enemyPieces.FirstOrDefault(p => p.type == PieceType.Bishop);
            if (bishop != null) return bishop;
            return enemyPieces
                .OrderByDescending(p => AIContext.PieceValue(p.type) * 10 - p.taxPerTurn)
                .First();
        }

        private float AverageSupport(List<ChessPiece> pieces)
        {
            if (pieces.Count == 0) return 0f;
            float total = 0f;
            foreach (var p in pieces) total += p.support;
            return total / pieces.Count;
        }

        // ─────────────────────────────────────────────────────────────────
        // 내부 타입
        // ─────────────────────────────────────────────────────────────────
        private enum ActionKind   { Fiscal, Military, Political }
        private enum PrisonerFate { Release, Ransom, Execute }

        private struct ActionCandidate
        {
            public readonly string      name;
            public readonly ChessPiece  target;
            public readonly int         value;
            public readonly ActionKind  kind;
            public readonly float       score;

            public ActionCandidate(string name, ChessPiece target, int value,
                                   ActionKind kind, float score)
            {
                this.name   = name;
                this.target = target;
                this.value  = value;
                this.kind   = kind;
                this.score  = score;
            }

            public static readonly ActionCandidate None =
                new ActionCandidate(null, null, 0, ActionKind.Fiscal, 0f);
        }
    }
}
