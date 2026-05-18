using System.Collections.Generic;

namespace MMBGame
{
    public partial class PoliticsManager
    {
        private FiscalResultSnapshot CaptureFiscalSnapshot(PlayerState actor, ChessPiece target)
        {
            FiscalResultSnapshot snapshot = new FiscalResultSnapshot();
            snapshot.gold = actor != null ? actor.gold : 0;
            snapshot.goldPerTurn = actor != null ? actor.goldPerTurn : 0;
            snapshot.target = target;
            snapshot.targetSupport = target != null ? target.support : 0;
            snapshot.targetTaxModifier = target != null ? target.taxModifier : 1;
            snapshot.targetIsOffBoard = target != null && target.isOffBoard;
            snapshot.pieceSupports = new Dictionary<ChessPiece, int>();

            if (boardManager != null && boardManager.BoardState != null)
            {
                List<ChessPiece> pieces = boardManager.BoardState.GetAllPieces();
                for (int i = 0; i < pieces.Count; i++)
                {
                    ChessPiece piece = pieces[i];
                    if (piece != null)
                    {
                        snapshot.pieceSupports[piece] = piece.support;
                    }
                }
            }

            return snapshot;
        }

        private string BuildFiscalResultText(string actionName, PlayerState actor, ChessPiece target, FiscalResultSnapshot before, int inputValue)
        {
            ChessPiece resultTarget = target;
            int supportDelta = target != null ? target.support - before.targetSupport : 0;
            if (actionName == "LoanAction" && actor != null)
            {
                ChessPiece changedPiece = FindSupportChangedPiece(before);
                resultTarget = changedPiece;
                supportDelta = changedPiece != null ? changedPiece.support - before.pieceSupports[changedPiece] : 0;
            }

            ActionResultContext context = new ActionResultContext
            {
                actionName = actionName,
                targetName = resultTarget != null ? PieceDisplayNames.GetShortName(resultTarget) : "대상",
                inputValue = inputValue,
                supportDelta = supportDelta,
                goldDelta = actor != null ? actor.gold - before.gold : 0,
                goldPerTurnDelta = actor != null ? actor.goldPerTurn - before.goldPerTurn : 0,
                taxModifierBefore = before.targetTaxModifier,
                taxModifierAfter = resultTarget != null ? resultTarget.taxModifier : before.targetTaxModifier,
                targetFile = resultTarget != null ? resultTarget.file : -1,
                targetRank = resultTarget != null ? resultTarget.rank : -1
            };

            return ActionResultText.Resolve(actionName, context);
        }

        private ChessPiece FindSupportChangedPiece(FiscalResultSnapshot before)
        {
            foreach (KeyValuePair<ChessPiece, int> pair in before.pieceSupports)
            {
                if (pair.Key != null && pair.Key.support != pair.Value)
                {
                    return pair.Key;
                }
            }

            return null;
        }

        private struct FiscalResultSnapshot
        {
            public int gold;
            public int goldPerTurn;
            public ChessPiece target;
            public int targetSupport;
            public int targetTaxModifier;
            public bool targetIsOffBoard;
            public Dictionary<ChessPiece, int> pieceSupports;
        }
    }
}
