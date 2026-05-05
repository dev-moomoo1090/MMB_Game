namespace MMBGame
{
    [System.Serializable]
    public class MovePattern
    {
        public int deltaFile;
        public int deltaRank;
        public bool isSliding;
        public bool captureOnly;
        public bool moveOnly;
        public bool isCannon;

        public MovePattern()
        {
        }

        public MovePattern(int deltaFile, int deltaRank, bool isSliding = false, bool captureOnly = false, bool moveOnly = false, bool isCannon = false)
        {
            this.deltaFile = deltaFile;
            this.deltaRank = deltaRank;
            this.isSliding = isSliding;
            this.captureOnly = captureOnly;
            this.moveOnly = moveOnly;
            this.isCannon = isCannon;
        }
    }
}
