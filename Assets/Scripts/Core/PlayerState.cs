namespace MMBGame
{
    public class PlayerState
    {
        public System.Action<int> OnHonorChanged;
        public PieceColor color;
        public int gold;
        public int honor;
        public int goldPerTurn;

        public PlayerState(PieceColor color, int startingGold, int startingGoldPerTurn = 10)
        {
            this.color = color;
            gold = startingGold;
            honor = 0;
            goldPerTurn = startingGoldPerTurn;
        }

        public bool SpendGold(int amount)
        {
            if (gold < amount)
            {
                return false;
            }

            gold -= amount;
            return true;
        }

        public void AddGold(int amount)
        {
            gold += amount;
        }

        public void AddHonor(int amount)
        {
            honor += amount;
            OnHonorChanged?.Invoke(amount);
        }
    }
}
