using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public class MilitaryManager : MonoBehaviour
    {
        private readonly List<DelayedEffect> pendingEffects = new List<DelayedEffect>();
        private readonly List<MilitaryAction> militaryActions = new List<MilitaryAction>();

        private BoardManager boardManager;
        public BoardManager BoardManager => boardManager;

        public void Initialize()
        {
            boardManager = FindObjectOfType<BoardManager>();
            pendingEffects.Clear();
            militaryActions.Clear();
            militaryActions.Add(new BarricadeAction());
            militaryActions.Add(new TrebuchetAction());
            militaryActions.Add(new BombardAction());
            militaryActions.Add(new OutpostAction());
            militaryActions.Add(new DivinePowerAction());
            militaryActions.Add(new MiracleAction());
            militaryActions.Add(new MilitaryExemptionAction());
            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
            EventBus.Instance.OnPhaseChanged += HandlePhaseChanged;
            EventBus.Instance.OnTurnChanged -= HandleTurnChanged;
            EventBus.Instance.OnTurnChanged += HandleTurnChanged;
        }

        private void OnDestroy()
        {
            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
            EventBus.Instance.OnTurnChanged -= HandleTurnChanged;
        }

        public void AddDelayedEffect(DelayedEffect effect)
        {
            if (effect != null)
            {
                pendingEffects.Add(effect);
            }
        }

        public bool ExecuteMilitaryAction(string actionName, ChessPiece target, int file, int rank, PieceColor actorColor)
        {
            MilitaryAction action = null;
            for (int i = 0; i < militaryActions.Count; i++)
            {
                if (militaryActions[i].ActionName == actionName)
                {
                    action = militaryActions[i];
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

            if (action.RequiresPositionSelection && (file < 0 || file > 7 || rank < 0 || rank > 7))
            {
                return false;
            }

            if (target != null && target.color != actorColor)
            {
                return false;
            }

            bool result = action.Execute(target, file, rank, actorColor, this);
            if (result)
            {
                EventBus.Instance.PublishActionExecuted(actorColor, actionName);
            }

            return result;
        }

        public bool HasTrebuchet(PieceColor color)
        {
            if (boardManager == null || boardManager.BoardState == null)
            {
                return false;
            }

            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    ChessPiece piece = boardManager.BoardState.GetPiece(file, rank);
                    if (piece != null && piece.type == PieceType.Trebuchet && piece.color == color)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public ChessPiece GetTrebuchet(PieceColor color)
        {
            if (boardManager == null || boardManager.BoardState == null)
            {
                return null;
            }

            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    ChessPiece piece = boardManager.BoardState.GetPiece(file, rank);
                    if (piece != null && piece.type == PieceType.Trebuchet && piece.color == color)
                    {
                        return piece;
                    }
                }
            }

            return null;
        }

        public bool HasPendingBombard(PieceColor color)
        {
            for (int i = 0; i < pendingEffects.Count; i++)
            {
                DelayedEffect effect = pendingEffects[i];
                if (effect.effectType == DelayedEffectType.Bombard && effect.ownerColor == color)
                {
                    return true;
                }
            }

            return false;
        }

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
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

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
