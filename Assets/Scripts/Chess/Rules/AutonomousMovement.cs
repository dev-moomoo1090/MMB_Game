using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public class AutonomousMovement : MonoBehaviour
    {
        private BoardManager boardManager;

        public void Initialize()
        {
            boardManager = FindObjectOfType<BoardManager>();
            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
            EventBus.Instance.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDestroy()
        {
            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.ChessPhase)
            {
                return;
            }

            if (boardManager == null || boardManager.BoardState == null)
            {
                return;
            }

            BoardState state = boardManager.BoardState;
            PieceColor currentTurn = state.currentTurn;
            TryKingsideRookEvade(state, currentTurn);
        }

        private void TryKingsideRookEvade(BoardState state, PieceColor color)
        {
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    ChessPiece piece = state.GetPiece(file, rank);
                    if (piece == null)
                    {
                        continue;
                    }

                    if (piece.color != color || piece.type != PieceType.Rook)
                    {
                        continue;
                    }

                    if (piece.offBoardOrigin.col != 7)
                    {
                        continue;
                    }

                    PieceColor enemy = color == PieceColor.White ? PieceColor.Black : PieceColor.White;
                    bool inCheck = CheckDetector.IsInCheck(state, color);
                    bool isUnderAttack = BoardEvaluator.CountAttackers(state, file, rank, enemy) > 0;
                    if (inCheck || !isUnderAttack)
                    {
                        continue;
                    }

                    if (Random.Range(0f, 1f) > 0.5f)
                    {
                        continue;
                    }

                    List<Move> legalMoves = MoveValidator.GetLegalMoves(state, file, rank);
                    if (legalMoves.Count == 0)
                    {
                        continue;
                    }

                    Move bestMove = null;
                    int minAttackers = int.MaxValue;
                    for (int i = 0; i < legalMoves.Count; i++)
                    {
                        Move move = legalMoves[i];
                        BoardState simulatedState = MoveValidator.SimulateMove(state, move);
                        int attackers = BoardEvaluator.CountAttackers(simulatedState, move.toFile, move.toRank, enemy);
                        if (attackers >= minAttackers)
                        {
                            continue;
                        }

                        minAttackers = attackers;
                        bestMove = move;
                    }

                    if (bestMove == null)
                    {
                        continue;
                    }

                    ChessPiece capturedPiece = GetCapturedPiece(state, bestMove);
                    if (capturedPiece != null)
                    {
                        capturedPiece.ClearOneTimeMovePatterns();
                    }

                    SpecialMoves.ApplyMove(state, bestMove);
                    state.currentTurn = color;
                    piece.ClearOneTimeMovePatterns();
                    state.RecordPosition();
                    boardManager.RefreshPieceVisuals();
                    EventBus.Instance.PublishAutonomousMoveExecuted(piece, bestMove.toFile, bestMove.toRank);
                    return;
                }
            }
        }

        private ChessPiece GetCapturedPiece(BoardState state, Move move)
        {
            if (move.specialMove == SpecialMoveType.EnPassant)
            {
                return state.GetPiece(move.toFile, move.fromRank);
            }

            return state.GetPiece(move.toFile, move.toRank);
        }
    }
}
