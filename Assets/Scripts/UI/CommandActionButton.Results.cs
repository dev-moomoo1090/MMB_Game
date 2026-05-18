namespace MMBGame
{
    public partial class CommandActionButton
    {
        private void ShowFiscalResultThenAdvance()
        {
            string resultText = politicsManager != null && !string.IsNullOrEmpty(politicsManager.LastFiscalActionResultText)
                ? politicsManager.LastFiscalActionResultText
                : "재정 행동이 실행되었습니다.";
            ActionResultPanel.Show("재정 행동 결과", resultText, AdvanceTurn);
        }

        private void ShowActionResultThenContinue(ChessPiece target, int value, int file, int rank, PieceColor color)
        {
            ActionResultContext context = new ActionResultContext
            {
                actionName = actionName,
                targetName = target != null ? PieceDisplayNames.GetShortName(target) : "대상",
                inputValue = value,
                supportDelta = 0,
                goldDelta = 0,
                goldPerTurnDelta = 0,
                taxModifierBefore = target != null ? target.taxModifier : 1,
                taxModifierAfter = target != null ? target.taxModifier : 1,
                targetFile = file,
                targetRank = rank
            };

            string resultText = ActionResultText.Resolve(actionName, context);
            System.Action confirmAction = IsFreeAction(color) ? null : AdvanceTurn;
            ActionResultPanel.Show("행동 결과", resultText, confirmAction);
        }

        private bool IsFreeAction(PieceColor color)
        {
            return category == CommandActionCategory.Political &&
                actionName == "처형" &&
                KingStateEvaluator.IsTyrant(color);
        }

    }
}
