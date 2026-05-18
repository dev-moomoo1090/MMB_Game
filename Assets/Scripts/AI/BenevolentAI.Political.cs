using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MMBGame.AI
{
    public partial class BenevolentAI
    {
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
            return 2;
        }

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

            if (!used.Contains("SpecialTax"))
            {
                foreach (var p in active.Where(p => p.support <= 60))
                    cands.Add(new ActionCandidate("SpecialTax", p, 0, ActionKind.Fiscal,
                                                  900f + (60 - p.support) * 5f));
            }

            if (!used.Contains("선전"))
            {
                if (avgSup < 8f)
                    cands.Add(new ActionCandidate("선전", null, 0, ActionKind.Political, 880f));
                if (honor < 25)
                    cands.Add(new ActionCandidate("선전", null, 0, ActionKind.Political, 870f));
            }

            if (!used.Contains("RearDeployAction"))
            {
                foreach (var p in active.Where(p => p.rebellionWeight >= 0.35f && !p.isOffBoard))
                {
                    if (!CheckDetector.IsSquareAttacked(_ctx.BoardState, p.file, p.rank, enemy))
                        cands.Add(new ActionCandidate("RearDeployAction", p, 0, ActionKind.Fiscal,
                                                      800f + p.rebellionWeight * 100f));
                }
            }

            if (!used.Contains("SpecialTax") && active.Count > 0)
            {
                var lowest = active.OrderBy(p => p.support).First();
                cands.Add(new ActionCandidate("SpecialTax", lowest, 0, ActionKind.Fiscal,
                                              700f + (100 - lowest.support)));
            }

            if (!used.Contains("선전"))
            {
                float eff = avgSup / 20f;
                cands.Add(new ActionCandidate("선전", null, 0, ActionKind.Political,
                                              600f + eff * 10f));
            }

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

            if (gold < 80 && !used.Contains("RequisitionAction"))
            {
                var richPiece = active.Where(p => p.support >= 70)
                                      .OrderByDescending(p => p.support)
                                      .FirstOrDefault();
                if (richPiece != null)
                    cands.Add(new ActionCandidate("RequisitionAction", richPiece,
                                                  richPiece.taxPerTurn * 2, ActionKind.Fiscal, 580f));
            }

            if (!used.Contains("OutpostAction"))
            {
                var knight = GetSafePiece(active, PieceType.Knight, enemy);
                if (knight != null && knight.oneTimeMovePatterns.Count == 0)
                    cands.Add(new ActionCandidate("OutpostAction", knight, 0, ActionKind.Military, 450f));
            }

            if (!used.Contains("DivinePowerAction"))
            {
                var rook = GetSafePiece(active, PieceType.Rook, enemy);
                if (rook != null && !rook.oneTimeMovePatterns.Any(mp => mp.isCannon))
                    cands.Add(new ActionCandidate("DivinePowerAction", rook, 0, ActionKind.Military, 440f));
            }

            if (!used.Contains("MiracleAction"))
            {
                var bishop = GetSafePiece(active, PieceType.Bishop, enemy);
                if (bishop != null && bishop.oneTimeMovePatterns.Count == 0)
                    cands.Add(new ActionCandidate("MiracleAction", bishop, 0, ActionKind.Military, 430f));
            }

            if (!used.Contains("MilitaryExemptionAction"))
            {
                var pawn = GetSafePiece(active, PieceType.Pawn, enemy);
                if (pawn != null && pawn.oneTimeMovePatterns.Count == 0)
                    cands.Add(new ActionCandidate("MilitaryExemptionAction", pawn, 0, ActionKind.Military, 400f));
            }

            if (gold >= 300 && !used.Contains("매수"))
            {
                var target = SelectBetrayTarget();
                if (target != null)
                    cands.Add(new ActionCandidate("매수", target, target.taxPerTurn, ActionKind.Political, 350f));
            }

            int approxTurn = _ctx.BoardState.moveHistory.Count / 2;
            if (approxTurn % 4 == 0 && !used.Contains("정찰"))
                cands.Add(new ActionCandidate("정찰", null, 0, ActionKind.Political, 300f));

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
    }
}
