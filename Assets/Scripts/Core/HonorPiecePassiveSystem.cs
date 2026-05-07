using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public class HonorPiecePassiveSystem : MonoBehaviour
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
            boardManager = FindObjectOfType<BoardManager>();
            politicsManager = FindObjectOfType<PoliticsManager>();
            stockfishBridge = FindObjectOfType<StockfishBridge>();
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

        private void TryPawnSupplySupport(PieceColor color, PlayerState player)
        {
            List<ChessPiece> pawns = GetPieces(color, PieceType.Pawn, PieceSide.None, true);
            if (pawns.Count == 0)
            {
                return;
            }

            float chance = Mathf.Sqrt(Mathf.Max(0f, player.honor / 2f));
            ChessPiece selectedPawn = null;
            int bestCount = int.MaxValue;
            for (int i = 0; i < pawns.Count; i++)
            {
                ChessPiece pawn = pawns[i];
                if (!RollPercent(chance))
                {
                    continue;
                }

                int count = pawnSupplyCounts[color].TryGetValue(pawn, out int savedCount) ? savedCount : 0;
                if (count < bestCount)
                {
                    bestCount = count;
                    selectedPawn = pawn;
                }
            }

            if (selectedPawn == null)
            {
                return;
            }

            pawnSupplyCounts[color][selectedPawn] = bestCount + 1;
            string actionName = GetRandomMilitaryActionName();
            freeMilitaryActions[color].Add(actionName);
        }

        private void TryKnightQueensidePraise(PieceColor color, PlayerState player)
        {
            if (!HasPiece(color, PieceType.Knight, PieceSide.Queenside))
            {
                return;
            }

            float chance = Mathf.Sqrt(Mathf.Max(0, player.honor));
            if (!RollPercent(chance))
            {
                return;
            }

            ApplyToAllPieces(color, piece => PoliticalStatService.ChangeSupport(piece, 5, "KnightPraise"));
            player.AddHonor(5);
        }

        private void TryBishopKingsideLedgerManipulation(PieceColor color)
        {
            if (!HasPiece(color, PieceType.Bishop, PieceSide.Kingside) || !RollPercent(5f))
            {
                return;
            }

            taxIncomeMultipliers[color] *= LEDGER_TAX_MULTIPLIER;
        }

        private void TryBishopKingsideStrategy(PieceColor color, PlayerState player)
        {
            if (!HasPiece(color, PieceType.Bishop, PieceSide.Kingside))
            {
                return;
            }

            float chance = Mathf.Sqrt(Mathf.Max(0, player.honor));
            if (!RollPercent(chance))
            {
                return;
            }

            EventBus.Instance.PublishReconResult(FindBestMoveText(color));
        }

        private void TryRookQueensideTradeOffer(PieceColor color, PlayerState player)
        {
            if (tradeAcceptedColors.Contains(color) || !HasPiece(color, PieceType.Rook, PieceSide.Queenside))
            {
                return;
            }

            float chance = Mathf.Sqrt(Mathf.Max(0, player.honor));
            if (!RollPercent(chance))
            {
                return;
            }

            player.goldPerTurn += 30;
            tradeAcceptedColors.Add(color);
        }

        private void TryRookQueensideFundingRequest(PieceColor color, PlayerState player)
        {
            if (!HasPiece(color, PieceType.Rook, PieceSide.Queenside) || !RollPercent(5f))
            {
                return;
            }

            ChessPiece rook = FindPiece(color, PieceType.Rook, PieceSide.Queenside);
            if (rook == null)
            {
                return;
            }

            if (player.SpendGold(30))
            {
                PoliticalStatService.ChangeSupport(rook, 5, "RookFundingRequest");
            }
            else
            {
                PoliticalStatService.ChangeSupport(rook, -10, "RookFundingRequest");
            }
        }

        private string FindBestMoveText(PieceColor color)
        {
            if (boardManager == null || boardManager.BoardState == null)
            {
                return "추천 수 없음";
            }

            List<Move> legalMoves = MoveValidator.GetAllLegalMoves(boardManager.BoardState, color);
            if (legalMoves.Count == 0)
            {
                return "추천 수 없음";
            }

            Move bestMove = legalMoves[0];
            int bestScore = int.MinValue;
            for (int i = 0; i < legalMoves.Count; i++)
            {
                Move move = legalMoves[i];
                ChessPiece target = boardManager.BoardState.GetPiece(move.toFile, move.toRank);
                int score = target != null && target.color != color ? BoardEvaluator.GetPieceValue(target.type) : 0;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            return "병법 추천: " + ToSquare(bestMove.fromFile, bestMove.fromRank) + " -> " + ToSquare(bestMove.toFile, bestMove.toRank);
        }

        private string ToSquare(int file, int rank)
        {
            char fileChar = (char)('a' + file);
            return fileChar.ToString() + (rank + 1);
        }

        private string GetRandomMilitaryActionName()
        {
            string[] actionNames =
            {
                "BarricadeAction",
                "RoadPlanAction",
                "TrebuchetAction",
                "BombardAction",
                "OutpostAction",
                "DivinePowerAction",
                "MiracleAction",
                "MilitaryExemptionAction"
            };

            return actionNames[Random.Range(0, actionNames.Length)];
        }

        private bool IsHonorActive(PlayerState player)
        {
            return player != null && player.honor >= HONOR_THRESHOLD;
        }

        private bool IsDishonorActive(PlayerState player)
        {
            return player != null && player.honor <= DISHONOR_THRESHOLD;
        }

        private bool RollPercent(float percent)
        {
            if (percent <= 0f)
            {
                return false;
            }

            return Random.Range(0f, 100f) < percent;
        }

        private void ApplyToAllPieces(PieceColor color, System.Action<ChessPiece> effect)
        {
            if (boardManager == null || boardManager.BoardState == null)
            {
                return;
            }

            List<ChessPiece> pieces = boardManager.BoardState.GetAllPieces();
            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece != null && piece.color == color)
                {
                    effect(piece);
                }
            }
        }

        private bool HasPiece(PieceColor color, PieceType type, PieceSide side)
        {
            return FindPiece(color, type, side) != null;
        }

        private ChessPiece FindPiece(PieceColor color, PieceType type, PieceSide side)
        {
            List<ChessPiece> pieces = GetPieces(color, type, side, false);
            return pieces.Count > 0 ? pieces[0] : null;
        }

        private List<ChessPiece> GetPieces(PieceColor color, PieceType type, PieceSide side, bool ignoreSide)
        {
            List<ChessPiece> result = new List<ChessPiece>();
            if (boardManager == null || boardManager.BoardState == null)
            {
                return result;
            }

            List<ChessPiece> pieces = boardManager.BoardState.GetAllPieces();
            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece == null || piece.color != color || piece.type != type)
                {
                    continue;
                }

                if (!ignoreSide && piece.side != side)
                {
                    continue;
                }

                result.Add(piece);
            }

            result.Sort(ComparePassivePriority);
            return result;
        }

        private int ComparePassivePriority(ChessPiece first, ChessPiece second)
        {
            int typeCompare = GetPassiveTypeOrder(first.type).CompareTo(GetPassiveTypeOrder(second.type));
            if (typeCompare != 0)
            {
                return typeCompare;
            }

            int sideCompare = GetPassiveSideOrder(first.side).CompareTo(GetPassiveSideOrder(second.side));
            if (sideCompare != 0)
            {
                return sideCompare;
            }

            return first.lane.CompareTo(second.lane);
        }

        private int GetPassiveTypeOrder(PieceType type)
        {
            switch (type)
            {
                case PieceType.Pawn: return 0;
                case PieceType.Knight: return 1;
                case PieceType.Bishop: return 2;
                case PieceType.Rook: return 3;
                case PieceType.Queen: return 4;
                default: return 5;
            }
        }

        private int GetPassiveSideOrder(PieceSide side)
        {
            switch (side)
            {
                case PieceSide.Queenside: return 0;
                case PieceSide.Kingside: return 1;
                default: return 2;
            }
        }

        private PlayerState GetPlayer(PieceColor color)
        {
            EnsureReferences();
            return politicsManager != null ? politicsManager.GetCurrentPlayer(color) : null;
        }

        private void EnsureColor(PieceColor color)
        {
            if (color == PieceColor.None)
            {
                return;
            }

            if (!pawnSupplyCounts.ContainsKey(color))
            {
                pawnSupplyCounts[color] = new Dictionary<ChessPiece, int>();
            }

            if (!freeMilitaryActions.ContainsKey(color))
            {
                freeMilitaryActions[color] = new HashSet<string>();
            }

            if (!taxIncomeMultipliers.ContainsKey(color))
            {
                taxIncomeMultipliers[color] = 1f;
            }

            if (!supportLossByColor.ContainsKey(color))
            {
                supportLossByColor[color] = 0;
            }
        }

        private void EnsureReferences()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (politicsManager == null)
            {
                politicsManager = FindObjectOfType<PoliticsManager>();
            }

            if (stockfishBridge == null)
            {
                stockfishBridge = FindObjectOfType<StockfishBridge>();
            }
        }
    }
}
