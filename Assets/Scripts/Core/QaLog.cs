using UnityEngine;

namespace MMBGame
{
    public static class QaLog
    {
        public static void Write(string category, string message)
        {
            Debug.Log("[QA][" + category + "] " + message);
        }

        public static string PieceLabel(ChessPiece piece)
        {
            if (piece == null)
            {
                return "null";
            }

            return piece.pieceName + " 색상=" + piece.color + " 지휘권=" + piece.GetMovementControllerColor() + " 종류=" + piece.type + " 위치=(" + piece.file + "," + piece.rank + ")";
        }
    }
}
