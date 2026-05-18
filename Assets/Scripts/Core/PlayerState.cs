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
            QaLog.Write("플레이어상태", "생성 색상=" + color + " 골드=" + gold + " 명예=" + honor + " 턴수입=" + goldPerTurn);
        }

        public bool SpendGold(int amount)
        {
            if (gold < amount)
            {
                QaLog.Write("골드", "소비 실패 색상=" + color + " 금액=" + amount + " 이전=" + gold + " 이후=" + gold);
                return false;
            }

            int before = gold;
            gold -= amount;
            QaLog.Write("골드", "소비 성공 색상=" + color + " 금액=" + amount + " 이전=" + before + " 이후=" + gold);
            return true;
        }

        public void AddGold(int amount)
        {
            int before = gold;
            gold += amount;
            QaLog.Write("골드", "증가 색상=" + color + " 금액=" + amount + " 이전=" + before + " 이후=" + gold);
        }

        public void AddHonor(int amount)
        {
            int before = honor;
            honor += amount;
            QaLog.Write("명예", "변경 색상=" + color + " 변화량=" + amount + " 이전=" + before + " 이후=" + honor);
            OnHonorChanged?.Invoke(amount);
        }
    }
}
