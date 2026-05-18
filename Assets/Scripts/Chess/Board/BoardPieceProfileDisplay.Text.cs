using UnityEngine;

namespace MMBGame
{
    public partial class BoardPieceProfileDisplay
    {
        private string GetMoveText(ChessPiece piece)
        {
            return PieceDisplayNames.GetTypeName(piece.type);
        }

        private PlayerState GetPlayerState(PieceColor color)
        {
            if (politicsManager == null)
            {
                politicsManager = SceneComponentResolver.Resolve<PoliticsManager>();
            }

            return politicsManager?.GetCurrentPlayer(color);
        }

        private string GetKingStateText(KingState state, int totalSupport, int honor)
        {
            string stateName = GetKingStateName(state);
            string supportStr = totalSupport >= 0 ? "+" + totalSupport : totalSupport.ToString();
            string honorStr = honor >= 0 ? "+" + honor : honor.ToString();
            return stateName + " : " + supportStr + " / " + honorStr;
        }

        private string GetKingStateName(KingState state)
        {
            switch (state)
            {
                case KingState.Sage: return "성군";
                case KingState.Autocrat: return "독재";
                case KingState.DarkKing: return "암군";
                case KingState.Tyrant: return "폭군";
                default: return "중립";
            }
        }
    }
}
