namespace MMBGame
{
    public static class PoliticalStatService
    {
        public static void ChangeSupport(ChessPiece piece, int delta, string reason)
        {
            if (piece == null || delta == 0)
            {
                return;
            }

            piece.support += delta;
            EventBus.Instance.PublishSupportChanged(piece, delta, reason);
        }

        public static void SetSupport(ChessPiece piece, int value)
        {
            if (piece == null)
            {
                return;
            }

            piece.support = value;
        }
    }
}
