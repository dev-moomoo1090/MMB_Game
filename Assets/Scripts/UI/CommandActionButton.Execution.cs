using UnityEngine;

namespace MMBGame
{
    public partial class CommandActionButton
    {
        private void HandleClick()
        {
            if (!CanExecuteInCurrentPhase())
            {
                return;
            }

            if (actionName == "FrontDeployAction")
            {
                ShowFrontDeployPanel();
                return;
            }

            if (CommandActionInputRequirements.RequiresValue(actionName))
            {
                CommandActionInputPrompt.Show(actionName, value => ExecuteWithValue(value));
                return;
            }

            Execute();
        }

        private bool CanExecuteInCurrentPhase()
        {
            turnManager = SceneComponentResolver.Resolve(turnManager);

            return turnManager == null || turnManager.CurrentPhase == GamePhase.PoliticsPhase;
        }

        private void ShowFrontDeployPanel()
        {
            if (politicsManager == null)
            {
                return;
            }

            BoardManager boardManager = politicsManager.BoardManager;
            if (boardManager?.BoardState == null)
            {
                return;
            }

            BoardPieceSetupManager setupManager = SceneComponentResolver.Resolve<BoardPieceSetupManager>();
            PieceColor color = ResolveActorColor();

            FrontDeployPanel.Show(color, boardManager.BoardState, setupManager, piece =>
            {
                bool result = politicsManager.ExecuteFiscalAction("FrontDeployAction", piece, 0);
                RefreshProfileIfNeeded(result);
                if (result)
                {
                    ShowFiscalResultThenAdvance();
                }
            });
        }

        private bool ExecuteWithValue(int value)
        {
            if (!CanExecuteInCurrentPhase())
            {
                return false;
            }

            if (string.IsNullOrEmpty(actionName))
            {
                return false;
            }

            ChessPiece target = ResolveSelectedPiece();
            PieceColor color = ResolveActorColor();
            int resolvedFile = targetFile >= 0 ? targetFile : ResolveTargetFile();
            int resolvedRank = targetRank >= 0 ? targetRank : ResolveTargetRank();
            bool result;

            if (category == CommandActionCategory.Fiscal)
            {
                result = politicsManager != null && politicsManager.ExecuteFiscalAction(actionName, target, value);
            }
            else if (category == CommandActionCategory.Military)
            {
                result = militaryManager != null && militaryManager.ExecuteMilitaryAction(actionName, target, resolvedFile, resolvedRank, color);
            }
            else
            {
                result = politicalManager != null && politicalManager.ExecutePoliticalAction(actionName, target, color, value);
            }

            if (!result && category == CommandActionCategory.Political && politicalManager != null && !string.IsNullOrEmpty(politicalManager.LastFailureReason))
            {
                ActionResultPanel.Show("?됰룞 寃곌낵", politicalManager.LastFailureReason, null);
            }

            RefreshProfileIfNeeded(result);
            if (result && category == CommandActionCategory.Fiscal)
            {
                ShowFiscalResultThenAdvance();
            }
            else if (result)
            {
                ShowActionResultThenContinue(target, value, resolvedFile, resolvedRank, color);
            }

            return result;
        }

        private void AdvanceTurn()
        {
            turnManager = SceneComponentResolver.Resolve(turnManager);

            turnManager?.EndPhase();
        }

        private ChessPiece ResolveSelectedPiece()
        {
            if (selectedPiece != null)
            {
                return selectedPiece;
            }

            boardInteraction = SceneComponentResolver.Resolve(boardInteraction);

            return boardInteraction != null ? boardInteraction.SelectedPiece : null;
        }

        private PieceColor ResolveActorColor()
        {
            turnManager = SceneComponentResolver.Resolve(turnManager);

            if (turnManager != null && turnManager.CurrentColor != PieceColor.None)
            {
                return turnManager.CurrentColor;
            }

            return actorColor;
        }

        private int ResolveTargetFile()
        {
            boardInteraction = SceneComponentResolver.Resolve(boardInteraction);
            if (boardInteraction != null && boardInteraction.SelectedTargetFile >= 0)
            {
                return boardInteraction.SelectedTargetFile;
            }

            ChessPiece piece = ResolveSelectedPiece();
            return piece != null ? piece.file : -1;
        }

        private int ResolveTargetRank()
        {
            boardInteraction = SceneComponentResolver.Resolve(boardInteraction);
            if (boardInteraction != null && boardInteraction.SelectedTargetRank >= 0)
            {
                return boardInteraction.SelectedTargetRank;
            }

            ChessPiece piece = ResolveSelectedPiece();
            return piece != null ? piece.rank : -1;
        }

        private int ResolveInputValue(ChessPiece target)
        {
            if (inputValue > 0)
            {
                return inputValue;
            }

            if (target != null)
            {
                return Mathf.Max(1, target.taxPerTurn);
            }

            return 10;
        }

        private void RefreshProfileIfNeeded(bool actionResult)
        {
            if (!actionResult)
            {
                return;
            }

            boardInteraction = SceneComponentResolver.Resolve(boardInteraction);

            boardInteraction?.RefreshSelectionProfile();
        }
    }
}
