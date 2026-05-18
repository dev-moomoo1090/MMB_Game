using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public partial class PrisonerPanel
    {
        private void ApplyChoice(PieceColor attackerColor, int choice, ChessPiece capturedPiece = null)
        {
            PoliticsManager pm = SceneComponentResolver.Resolve<PoliticsManager>();
            if (pm != null)
            {
                PlayerState player = pm.GetCurrentPlayer(attackerColor);
                if (player != null)
                {
                    int pieceValue = capturedPiece != null ? BoardEvaluator.GetPieceValue(capturedPiece.type) : 0;
                    switch (choice)
                    {
                        case 0: player.AddHonor(pieceValue * 5); break;
                        case 1:
                            KingStateEffectApplier.Instance?.SuppressNextHonorDecreasePenalty(attackerColor);
                            player.AddHonor(-pieceValue * 5);
                            ApplyDictatorshipPrisonerExecutionBonus(attackerColor, capturedPiece);
                            break;
                        case 2: player.AddGold(pieceValue * 100); break;
                    }
                }
            }

            AdvanceTurn();
        }

        private void ApplyDictatorshipPrisonerExecutionBonus(PieceColor attackerColor, ChessPiece capturedPiece)
        {
            if (!KingStateEvaluator.IsDictatorship(attackerColor) || capturedPiece == null)
            {
                return;
            }

            BoardManager boardManager = SceneComponentResolver.Resolve<BoardManager>();
            if (boardManager == null || boardManager.BoardState == null)
            {
                return;
            }

            int bonus = Mathf.CeilToInt(BoardEvaluator.GetPieceValue(capturedPiece.type) / 2f);
            if (bonus <= 0)
            {
                return;
            }

            List<ChessPiece> pieces = boardManager.BoardState.GetAllPieces();
            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece != null && piece.color == attackerColor)
                {
                    PoliticalStatService.ChangeSupport(piece, bonus, "PrisonerExecutionBonus");
                }
            }
        }

        private void AdvanceTurn()
        {
            TurnManager tm = SceneComponentResolver.Resolve<TurnManager>();
            tm?.EndPhase();
        }
    }
}
