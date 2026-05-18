using UnityEngine;
namespace MMBGame
{
    public partial class TurnStatusDisplay
    {
        private string GetColorText(PieceColor color)
        {
            return PieceDisplayNames.GetColorName(color);
        }

        private string GetPhaseText(GamePhase phase)
        {
            if (phase == GamePhase.ChessPhase)
            {
                return "체스";
            }

            if (phase == GamePhase.PrisonerPhase)
            {
                return "포로";
            }

            return "정치";
        }


        private string GetKingStateIconText(KingState state)
        {
            switch (state)
            {
                case KingState.Sage: return "성";
                case KingState.Autocrat: return "독";
                case KingState.DarkKing: return "암";
                case KingState.Tyrant: return "폭";
                default: return "중";
            }
        }

        private Color GetKingStateColor(KingState state)
        {
            switch (state)
            {
                case KingState.Sage: return new Color(0.15f, 0.48f, 0.82f, 0.86f);
                case KingState.Autocrat: return new Color(0.38f, 0.28f, 0.58f, 0.86f);
                case KingState.DarkKing: return new Color(0.5f, 0.5f, 0.18f, 0.86f);
                case KingState.Tyrant: return new Color(0.66f, 0.12f, 0.1f, 0.86f);
                default: return new Color(0.24f, 0.24f, 0.24f, 0.86f);
            }
        }

        private string GetKingStateTooltipText(KingState state)
        {
            if (state == KingState.DarkKing)
            {
                return GetDarkKingTooltipText();
            }

            switch (state)
            {
                case KingState.Sage:
                    return "성군\n정치 행동 스킵 시 다음 턴 횟수 +1(최대 1회)\n매 턴 모든 기물 지지도 +2";
                case KingState.Autocrat:
                    return "독재\n명령 거부 확률 0%\n군사 행동 비용 -30%";
                case KingState.DarkKing:
                    return "암군\n최초 진입 시 세금/수락/지지도 -50%\n20턴 생존 시 성군 고정";
                case KingState.Tyrant:
                    return "폭군\n처형 행동 턴 소모 0\n처형 시 특수 폰 소환\n세금 수입 +100%";
                default:
                    return "중립\n적용 중인 특수 효과 없음";
            }
        }

        private string GetDarkKingTooltipText()
        {
            KingStateEffectApplier applier = KingStateEffectApplier.Instance;
            int elapsed = applier != null ? applier.GetIncompetentTurnsElapsed(currentColor) : 0;
            int remaining = applier != null ? applier.GetIncompetentTurnsRemaining(currentColor) : 20;
            int total = applier != null ? applier.GetIncompetentSurvivalTurns() : 20;
            return "암군\n현재 " + elapsed + "/" + total + "턴 진행\n성군 고정까지 " + remaining + "턴 남음";
        }

    }
}
