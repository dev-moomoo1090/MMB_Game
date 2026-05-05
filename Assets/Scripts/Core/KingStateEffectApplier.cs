using System;
using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public class KingStateEffectApplier : MonoBehaviour
    {
        private readonly Dictionary<PieceColor, int> darkKingTurnCounts = new Dictionary<PieceColor, int>();
        private readonly Dictionary<PieceColor, Action<int>> honorChangeHandlers = new Dictionary<PieceColor, Action<int>>();

        private BoardManager boardManager;
        private PoliticsManager politicsManager;

        public void Initialize()
        {
            boardManager = FindObjectOfType<BoardManager>();
            politicsManager = FindObjectOfType<PoliticsManager>();
            darkKingTurnCounts[PieceColor.White] = 0;
            darkKingTurnCounts[PieceColor.Black] = 0;

            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
            EventBus.Instance.OnPhaseChanged += HandlePhaseChanged;
            EventBus.Instance.OnPieceCapturePending -= HandlePieceCapture;
            EventBus.Instance.OnPieceCapturePending += HandlePieceCapture;
        }

        private void OnDestroy()
        {
            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
            EventBus.Instance.OnPieceCapturePending -= HandlePieceCapture;
            RemoveHonorChangeHandler(PieceColor.White);
            RemoveHonorChangeHandler(PieceColor.Black);
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.PoliticsPhase)
            {
                return;
            }

            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (politicsManager == null)
            {
                politicsManager = FindObjectOfType<PoliticsManager>();
            }

            if (boardManager == null || boardManager.BoardState == null || politicsManager == null)
            {
                return;
            }

            PieceColor color = boardManager.BoardState.currentTurn;
            PlayerState player = politicsManager.GetCurrentPlayer(color);
            if (player == null)
            {
                return;
            }

            KingState state = KingStateEvaluator.Evaluate(player, boardManager.BoardState);
            ApplyKingStateEffect(state, player, color);
        }

        private void ApplyKingStateEffect(KingState state, PlayerState player, PieceColor color)
        {
            BoardState board = boardManager.BoardState;

            if (state == KingState.Sage)
            {
                darkKingTurnCounts[color] = 0;
                RemoveHonorChangeHandler(color);
                int bonus = Mathf.FloorToInt(player.honor / 50f);
                if (bonus > 0)
                {
                    ApplyToAllPieces(board, color, piece => piece.support += bonus);
                }

                return;
            }

            if (state == KingState.DarkKing)
            {
                RemoveHonorChangeHandler(color);
                darkKingTurnCounts[color]++;
                player.AddHonor(1);
                ApplyToAllPieces(board, color, piece => piece.support += 1);

                if (darkKingTurnCounts[color] >= 20 && player.honor < 20)
                {
                    player.AddHonor(20 - player.honor);
                    darkKingTurnCounts[color] = 0;
                }

                return;
            }

            darkKingTurnCounts[color] = 0;

            if (state == KingState.Autocrat)
            {
                player.AddGold(player.goldPerTurn / 2);
                EnsureHonorChangeHandler(player, color);
                return;
            }

            RemoveHonorChangeHandler(color);
            if (state == KingState.Tyrant)
            {
                ApplyToAllPieces(board, color, piece => piece.rebellionWeight += 0.3f);
            }
        }

        private void HandlePieceCapture(ChessPiece piece)
        {
            if (piece == null)
            {
                return;
            }

            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (politicsManager == null)
            {
                politicsManager = FindObjectOfType<PoliticsManager>();
            }

            if (boardManager == null || boardManager.BoardState == null || politicsManager == null)
            {
                return;
            }

            PlayerState player = politicsManager.GetCurrentPlayer(piece.color);
            if (player == null)
            {
                return;
            }

            KingState state = KingStateEvaluator.Evaluate(player, boardManager.BoardState);
            if (state != KingState.Sage)
            {
                return;
            }

            int value = BoardEvaluator.GetPieceValue(piece.type);
            if (value <= 0)
            {
                return;
            }

            ApplyToAllPieces(boardManager.BoardState, piece.color, targetPiece => targetPiece.support -= value);
            player.AddHonor(-value);
        }

        private void EnsureHonorChangeHandler(PlayerState player, PieceColor color)
        {
            RemoveHonorChangeHandler(color);

            Action<int> handler = delta =>
            {
                if (delta >= 0 || boardManager == null || boardManager.BoardState == null)
                {
                    return;
                }

                int penalty = Mathf.Abs(delta) / 2;
                if (penalty <= 0)
                {
                    return;
                }

                ApplyToAllPieces(boardManager.BoardState, color, piece => piece.support -= penalty);
            };

            honorChangeHandlers[color] = handler;
            player.OnHonorChanged += handler;
        }

        private void RemoveHonorChangeHandler(PieceColor color)
        {
            if (!honorChangeHandlers.TryGetValue(color, out Action<int> handler) || politicsManager == null)
            {
                honorChangeHandlers.Remove(color);
                return;
            }

            PlayerState player = politicsManager.GetCurrentPlayer(color);
            if (player != null)
            {
                player.OnHonorChanged -= handler;
            }

            honorChangeHandlers.Remove(color);
        }

        private void ApplyToAllPieces(BoardState board, PieceColor color, Action<ChessPiece> effect)
        {
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    ChessPiece piece = board.GetPiece(file, rank);
                    if (piece != null && piece.color == color)
                    {
                        effect(piece);
                    }
                }
            }
        }
    }
}
