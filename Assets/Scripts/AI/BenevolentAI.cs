namespace MMBGame.AI
{
    public partial class BenevolentAI
    {
        private readonly AIContext _ctx;
        private readonly PieceColor _side;

        public BenevolentAI(GameManager gameManager, PieceColor side)
        {
            _ctx  = new AIContext(gameManager);
            _side = side;
        }

        public void TakeTurn()
        {
            ExecutePoliticalPhase();
            ExecuteMilitaryPhase();
            ExecuteTurnEnd();
        }
    }
}
