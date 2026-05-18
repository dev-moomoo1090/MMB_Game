using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public partial class PoliticsManager : MonoBehaviour
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
            boardManager = SceneComponentResolver.Resolve<BoardManager>();
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
            QaLog.Write("행동", "재정 행동 시도 행동=" + actionName + " 색상=" + currentTurnColor + " 입력값=" + value + " 대상=" + QaLog.PieceLabel(target));
            if (fiscalActions == null)
            {
                QaLog.Write("행동", "재정 행동 실패 행동=" + actionName + " 사유=재정행동목록없음");
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
                QaLog.Write("행동", "재정 행동 실패 행동=" + actionName + " 사유=행동없음");
                return false;
            }

            if (action.RequiresPieceSelection && target == null)
            {
                QaLog.Write("행동", "재정 행동 실패 행동=" + actionName + " 사유=대상필요");
                return false;
            }

            if (action.RequiresPieceSelection && target != null && target.type == PieceType.King)
            {
                QaLog.Write("행동", "재정 행동 실패 행동=" + actionName + " 사유=킹대상불가");
                return false;
            }

            if (target != null && target.color != currentTurnColor)
            {
                QaLog.Write("행동", "재정 행동 실패 행동=" + actionName + " 사유=대상색상불일치 대상색상=" + target.color + " 현재색상=" + currentTurnColor);
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
            else
            {
                QaLog.Write("행동", "재정 행동 실패 행동=" + actionName + " 사유=실행결과실패");
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
            boardManager = SceneComponentResolver.Resolve(boardManager);

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
            boardManager = SceneComponentResolver.Resolve(boardManager);

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
    }
}
