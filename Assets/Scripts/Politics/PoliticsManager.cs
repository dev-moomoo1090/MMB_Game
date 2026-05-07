using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public class PoliticsManager : MonoBehaviour
    {
        [SerializeField] private int startingGold = 100;

        private PlayerState whitePlayer;
        private PlayerState blackPlayer;
        private List<FiscalAction> fiscalActions;
        private BoardManager boardManager;
        private PieceColor currentTurnColor;
        private LoanAction loanAction;

        public BoardManager BoardManager => boardManager;
        public string LastFiscalActionResultText { get; private set; }

        private void OnDestroy()
        {
            EventBus.Instance.OnTurnChanged -= HandleTurnChanged;
        }

        public void Initialize()
        {
            whitePlayer = new PlayerState(PieceColor.White, startingGold);
            blackPlayer = new PlayerState(PieceColor.Black, startingGold);
            loanAction = new LoanAction();
            fiscalActions = new List<FiscalAction>
            {
                new SpecialTaxAction(),
                new TaxExemptionAction(),
                new AidAction(),
                new RequisitionAction(),
                loanAction,
                new RearDeployAction(),
                new FrontDeployAction()
            };
            boardManager = FindObjectOfType<BoardManager>();
            EventBus.Instance.OnTurnChanged -= HandleTurnChanged;
            EventBus.Instance.OnTurnChanged += HandleTurnChanged;
        }

        public PlayerState GetCurrentPlayer(PieceColor color)
        {
            if (color == PieceColor.White)
            {
                return whitePlayer;
            }

            if (color == PieceColor.Black)
            {
                return blackPlayer;
            }

            return null;
        }

        public bool ExecuteFiscalAction(string actionName, ChessPiece target, int value)
        {
            if (fiscalActions == null)
            {
                return false;
            }

            FiscalAction action = null;
            for (int i = 0; i < fiscalActions.Count; i++)
            {
                if (fiscalActions[i].ActionName == actionName)
                {
                    action = fiscalActions[i];
                    break;
                }
            }

            if (action == null)
            {
                return false;
            }

            if (action.RequiresPieceSelection && target == null)
            {
                return false;
            }

            if (target != null && target.color != currentTurnColor)
            {
                return false;
            }

            PlayerState actor = GetCurrentPlayer(currentTurnColor);
            FiscalResultSnapshot before = CaptureFiscalSnapshot(actor, target);
            bool result = action.Execute(target, actor, value, this);
            if (result)
            {
                LastFiscalActionResultText = BuildFiscalResultText(actionName, actor, target, before, value);
                EventBus.Instance.PublishActionExecuted(currentTurnColor, actionName);
            }

            return result;
        }

        private void HandleTurnChanged(PieceColor color)
        {
            currentTurnColor = color;
            ResetPiecesForTurn();
            PlayerState currentPlayer = GetCurrentPlayer(color);
            if (currentPlayer != null)
            {
                if (boardManager != null && boardManager.BoardState != null)
                {
                    KingState state = KingStateEvaluator.Evaluate(currentPlayer, boardManager.BoardState);
                    KingStateEvaluator.SetCurrentState(color, state);
                }

                HonorPiecePassiveSystem.Instance?.ApplyTurnStartBeforeIncome(color);
                loanAction?.AdvanceLoans(currentPlayer);
                currentPlayer.AddGold(currentPlayer.goldPerTurn);
                currentPlayer.AddGold(GetPieceTaxIncome(color));
            }
        }

        private int GetPieceTaxIncome(PieceColor color)
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (boardManager == null || boardManager.BoardState == null)
            {
                return 0;
            }

            int total = 0;
            List<ChessPiece> pieces = boardManager.BoardState.GetAllPieces();
            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece != null && piece.color == color && !piece.isOffBoard)
                {
                    total += piece.taxPerTurn;
                }
            }

            float passiveMultiplier = HonorPiecePassiveSystem.Instance != null
                ? HonorPiecePassiveSystem.Instance.ConsumeTaxIncomeMultiplier(color)
                : 1f;
            return Mathf.FloorToInt(total * KingStateEvaluator.GetTaxIncomeMultiplier(color) * passiveMultiplier);
        }

        private void ResetPiecesForTurn()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (boardManager == null || boardManager.BoardState == null)
            {
                return;
            }

            List<ChessPiece> pieces = boardManager.BoardState.GetAllPieces();
            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece == null || piece.isOffBoard)
                {
                    continue;
                }

                piece.ResetTurnModifiers();
            }
        }

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
                targetName = resultTarget != null ? GetPieceShortName(resultTarget) : "대상",
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

        private string GetPieceShortName(ChessPiece piece)
        {
            if (piece == null)
            {
                return "기물";
            }

            return GetColorName(piece.color) + GetSideName(piece.side) + GetTypeName(piece.type);
        }

        private string GetColorName(PieceColor color)
        {
            return color == PieceColor.Black ? "흑" : "백";
        }

        private string GetSideName(PieceSide side)
        {
            if (side == PieceSide.Kingside)
            {
                return "K";
            }

            if (side == PieceSide.Queenside)
            {
                return "Q";
            }

            return string.Empty;
        }

        private string GetTypeName(PieceType type)
        {
            switch (type)
            {
                case PieceType.Pawn: return "폰";
                case PieceType.Rook: return "룩";
                case PieceType.Knight: return "나이트";
                case PieceType.Bishop: return "비숍";
                case PieceType.Queen: return "퀸";
                case PieceType.King: return "킹";
                case PieceType.Barricade: return "바리케이드";
                case PieceType.Trebuchet: return "트레뷰셋";
                default: return type.ToString();
            }
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
