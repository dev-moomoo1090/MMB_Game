using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public partial class HonorPiecePassiveSystem : MonoBehaviour
    {
        private const int HONOR_THRESHOLD = 20;
        private const int DISHONOR_THRESHOLD = -20;
        private const float LEDGER_TAX_MULTIPLIER = 0.8f;

        private readonly Dictionary<PieceColor, Dictionary<ChessPiece, int>> pawnSupplyCounts = new Dictionary<PieceColor, Dictionary<ChessPiece, int>>();
        private readonly Dictionary<PieceColor, HashSet<string>> freeMilitaryActions = new Dictionary<PieceColor, HashSet<string>>();
        private readonly Dictionary<PieceColor, float> taxIncomeMultipliers = new Dictionary<PieceColor, float>();
        private readonly Dictionary<PieceColor, int> supportLossByColor = new Dictionary<PieceColor, int>();
        private readonly HashSet<PieceColor> tradeAcceptedColors = new HashSet<PieceColor>();
        private bool suppressDishonorTracking;

        private BoardManager boardManager;
        private PoliticsManager politicsManager;
        private StockfishBridge stockfishBridge;

        public static HonorPiecePassiveSystem Instance { get; private set; }

        public void Initialize()
        {
            Instance = this;
            boardManager = SceneComponentResolver.Resolve<BoardManager>();
            politicsManager = SceneComponentResolver.Resolve<PoliticsManager>();
            stockfishBridge = SceneComponentResolver.Resolve<StockfishBridge>();
            EnsureColor(PieceColor.White);
            EnsureColor(PieceColor.Black);

            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
            EventBus.Instance.OnPhaseChanged += HandlePhaseChanged;
            EventBus.Instance.OnSupportChanged -= HandleSupportChanged;
            EventBus.Instance.OnSupportChanged += HandleSupportChanged;
            EventBus.Instance.OnPieceCapturePending -= HandlePieceCapturePending;
            EventBus.Instance.OnPieceCapturePending += HandlePieceCapturePending;
        }

        private void OnDestroy()
        {
            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
            EventBus.Instance.OnSupportChanged -= HandleSupportChanged;
            EventBus.Instance.OnPieceCapturePending -= HandlePieceCapturePending;
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void ApplyTurnStartBeforeIncome(PieceColor color)
        {
            EnsureReferences();
            EnsureColor(color);
            taxIncomeMultipliers[color] = 1f;

            PlayerState player = GetPlayer(color);
            if (!IsHonorActive(player))
            {
                TryApplyDishonorSupportLoss(color, player);
                return;
            }

            TryPawnSupplySupport(color, player);
            TryKnightQueensidePraise(color, player);
            TryBishopKingsideLedgerManipulation(color);
            TryRookQueensideTradeOffer(color, player);
            TryRookQueensideFundingRequest(color, player);
            TryApplyDishonorSupportLoss(color, player);
        }

        public float ConsumeTaxIncomeMultiplier(PieceColor color)
        {
            EnsureColor(color);
            float multiplier = taxIncomeMultipliers[color];
            taxIncomeMultipliers[color] = 1f;
            return multiplier;
        }

        public bool TryConsumeFreeMilitaryAction(PieceColor color, string actionName)
        {
            EnsureColor(color);
            if (string.IsNullOrEmpty(actionName) || !freeMilitaryActions[color].Contains(actionName))
            {
                return false;
            }

            freeMilitaryActions[color].Remove(actionName);
            return true;
        }

        public bool TryRerollMovementRefusal(ChessPiece refusedPiece)
        {
            EnsureReferences();
            if (refusedPiece == null)
            {
                return false;
            }

            PlayerState player = GetPlayer(refusedPiece.color);
            if (!IsHonorActive(player) || !HasPiece(refusedPiece.color, PieceType.Knight, PieceSide.Kingside))
            {
                return false;
            }

            float chance = Mathf.Sqrt(Mathf.Max(0, player.honor));
            if (!RollPercent(chance))
            {
                return false;
            }

            return !ObedienceSystem.RollRefusal(refusedPiece, boardManager != null ? boardManager.BoardState : null);
        }

        public void HandlePieceMoved(ChessPiece piece, BoardState state)
        {
            EnsureReferences();
            if (piece == null || state == null)
            {
                return;
            }

            PlayerState player = GetPlayer(piece.color);
            if (!IsHonorActive(player) || piece.type != PieceType.Knight || piece.side != PieceSide.Kingside)
            {
                return;
            }

            PieceColor enemy = piece.color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            int attackers = BoardEvaluator.CountAttackers(state, piece.file, piece.rank, enemy);
            int defenders = BoardEvaluator.CountDefenders(state, piece.file, piece.rank, piece.color);
            if (attackers <= 0 || defenders > attackers)
            {
                return;
            }

            int value = BoardEvaluator.GetPieceValue(piece.type);
            player.AddHonor(-value);
            piece.punishCount += 1;
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.ChessPhase)
            {
                return;
            }

            EnsureReferences();
            if (boardManager == null || boardManager.BoardState == null)
            {
                return;
            }

            PieceColor color = boardManager.BoardState.currentTurn;
            PlayerState player = GetPlayer(color);
            if (!IsHonorActive(player))
            {
                return;
            }

            TryBishopKingsideStrategy(color, player);
        }

        private void HandleSupportChanged(ChessPiece piece, int delta, string reason)
        {
            if (suppressDishonorTracking || piece == null || delta >= 0)
            {
                return;
            }

            EnsureColor(piece.color);
            supportLossByColor[piece.color] += -delta;
        }

        private void HandlePieceCapturePending(ChessPiece piece)
        {
            EnsureReferences();
            if (piece == null)
            {
                return;
            }

            PlayerState player = GetPlayer(piece.color);
            if (!IsDishonorActive(player))
            {
                return;
            }

            int value = BoardEvaluator.GetPieceValue(piece.type);
            if (value <= 0)
            {
                return;
            }

            suppressDishonorTracking = true;
            ApplyToAllPieces(piece.color, targetPiece => PoliticalStatService.ChangeSupport(targetPiece, -value, "DishonorCapturePenalty"));
            suppressDishonorTracking = false;
            player.AddHonor(-value);
        }

        private void TryApplyDishonorSupportLoss(PieceColor color, PlayerState player)
        {
            if (!IsDishonorActive(player))
            {
                supportLossByColor[color] = 0;
                return;
            }

            int penalty = supportLossByColor[color] / 10;
            supportLossByColor[color] = 0;
            if (penalty <= 0)
            {
                return;
            }

            suppressDishonorTracking = true;
            ApplyToAllPieces(color, piece => PoliticalStatService.ChangeSupport(piece, -penalty, "DishonorSupportLossPenalty"));
            suppressDishonorTracking = false;
        }

    }
}
