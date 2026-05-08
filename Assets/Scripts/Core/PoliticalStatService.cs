namespace MMBGame
{
    public static class PoliticalStatService
    {
        public const int MIN_SUPPORT = 0;
        public const int MAX_SUPPORT = 100;

        public static void ChangeSupport(ChessPiece piece, int delta, string reason)
        {
            ChangeSupport(piece, delta, reason, PieceColor.None, false);
        }

        public static void ChangeSupportFromPoliticalAction(ChessPiece piece, int delta, PieceColor actorColor, string reason)
        {
            ChangeSupport(piece, delta, reason, actorColor, true);
        }

        public static void ChangeHonor(PlayerState player, int delta, PieceColor actorColor, bool isPoliticalAction)
        {
            if (player == null || delta == 0)
            {
                return;
            }

            player.AddHonor(ApplyIncomingSageReduction(player.color, delta, actorColor, isPoliticalAction));
        }

        private static void ChangeSupport(ChessPiece piece, int delta, string reason, PieceColor actorColor, bool isPoliticalAction)
        {
            if (piece == null || delta == 0)
            {
                return;
            }

            int appliedDelta = ApplyIncomingSageReduction(piece.color, delta, actorColor, isPoliticalAction);
            int before = piece.support;
            piece.support = ClampSupport(piece.support + appliedDelta);
            int actualDelta = piece.support - before;
            if (actualDelta != 0)
            {
                EventBus.Instance.PublishSupportChanged(piece, actualDelta, reason);
            }
        }

        public static void SetSupport(ChessPiece piece, int value)
        {
            if (piece == null)
            {
                return;
            }

            piece.support = ClampSupport(value);
        }

        public static int ClampSupport(int value)
        {
            return UnityEngine.Mathf.Clamp(value, MIN_SUPPORT, MAX_SUPPORT);
        }

        private static int ApplyIncomingSageReduction(PieceColor targetColor, int delta, PieceColor actorColor, bool isPoliticalAction)
        {
            if (!isPoliticalAction || delta >= 0 || actorColor == PieceColor.None || actorColor == targetColor || !KingStateEvaluator.IsBenevolent(targetColor))
            {
                return delta;
            }

            return -UnityEngine.Mathf.CeilToInt(UnityEngine.Mathf.Abs(delta) * KingStateEvaluator.GetIncomingPoliticalPenaltyMultiplier(targetColor));
        }
    }
}
