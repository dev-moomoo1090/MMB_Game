using System.Collections.Generic;

namespace MMBGame
{
    public partial class MilitaryManager
    {
        private void HandlePhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.ChessPhase)
            {
                return;
            }

            List<DelayedEffect> processedEffects = new List<DelayedEffect>();
            for (int i = 0; i < pendingEffects.Count; i++)
            {
                DelayedEffect effect = pendingEffects[i];
                if (effect.turnsRemaining != 0)
                {
                    continue;
                }

                if (effect.effectType == DelayedEffectType.ApplyEnhancement)
                {
                    ApplyEnhancement(effect);
                    processedEffects.Add(effect);
                }
            }

            RemoveProcessedEffects(processedEffects);
        }

        private void HandleTurnChanged(PieceColor color)
        {
            List<DelayedEffect> processedEffects = new List<DelayedEffect>();

            for (int i = 0; i < pendingEffects.Count; i++)
            {
                DelayedEffect effect = pendingEffects[i];
                if ((effect.effectType == DelayedEffectType.PlaceBarricade || effect.effectType == DelayedEffectType.PlaceTrebuchet) &&
                    boardManager != null &&
                    boardManager.BoardState != null &&
                    boardManager.BoardState.GetPiece(effect.targetFile, effect.targetRank) != null)
                {
                    processedEffects.Add(effect);
                    continue;
                }

                if (effect.ownerColor != color)
                {
                    continue;
                }

                if (effect.turnsRemaining > 0)
                {
                    effect.turnsRemaining--;
                }

                if (effect.turnsRemaining != 0)
                {
                    continue;
                }

                if (effect.effectType == DelayedEffectType.PlaceBarricade)
                {
                    PlaceOnBoard(effect.effectType, effect.targetFile, effect.targetRank, effect.ownerColor);
                    processedEffects.Add(effect);
                }
                else if (effect.effectType == DelayedEffectType.PlaceTrebuchet)
                {
                    PlaceOnBoard(effect.effectType, effect.targetFile, effect.targetRank, effect.ownerColor);
                    processedEffects.Add(effect);
                }
                else if (effect.effectType == DelayedEffectType.Bombard)
                {
                    ExecuteBombard(effect.targetFile, effect.targetRank);
                    processedEffects.Add(effect);
                }
            }

            RemoveProcessedEffects(processedEffects);
        }

        private void PlaceOnBoard(DelayedEffectType type, int file, int rank, PieceColor color)
        {
            boardManager = SceneComponentResolver.Resolve(boardManager);

            if (boardManager == null)
            {
                return;
            }

            ChessPiece obstacle = null;
            if (type == DelayedEffectType.PlaceBarricade)
            {
                obstacle = new Barricade(color == PieceColor.White ? PieceColor.Black : PieceColor.White, file, rank);
            }
            else if (type == DelayedEffectType.PlaceTrebuchet)
            {
                obstacle = new Trebuchet(color, file, rank);
            }

            if (obstacle != null)
            {
                boardManager.PlaceObstacle(obstacle, file, rank);
            }
        }

        private void ExecuteBombard(int file, int rank)
        {
            if (boardManager == null || boardManager.BoardState == null || !boardManager.BoardState.IsInBounds(file, rank))
            {
                return;
            }

            ChessPiece piece = boardManager.BoardState.GetPiece(file, rank);
            if (piece == null)
            {
                return;
            }

            piece.ClearOneTimeMovePatterns();
            EventBus.Instance.PublishPieceCapturePending(piece);
            boardManager.BoardState.squares[file, rank].piece = null;
            boardManager.RefreshPieceVisuals();
        }

        private void ApplyEnhancement(DelayedEffect effect)
        {
            if (effect.targetPiece == null || effect.patternsToAdd == null || effect.patternsToAdd.Count == 0)
            {
                return;
            }

            if (boardManager == null || boardManager.BoardState == null)
            {
                return;
            }

            ChessPiece livePiece = boardManager.BoardState.GetPiece(effect.targetPiece.file, effect.targetPiece.rank);
            if (livePiece == null || livePiece.color != effect.ownerColor || livePiece.type != effect.targetPiece.type)
            {
                return;
            }

            livePiece.AddOneTimeMovePatterns(effect.patternsToAdd);
        }

        private void RemoveProcessedEffects(List<DelayedEffect> processedEffects)
        {
            for (int i = 0; i < processedEffects.Count; i++)
            {
                pendingEffects.Remove(processedEffects[i]);
            }
        }
    }
}
