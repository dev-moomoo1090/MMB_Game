using UnityEngine;

namespace MMBGame
{
    public class RebellionSystem : MonoBehaviour
    {
        private BoardManager boardManager;

        public void Initialize()
        {
            boardManager = FindObjectOfType<BoardManager>();
            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
            EventBus.Instance.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDestroy()
        {
            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.ChessPhase)
            {
                return;
            }

            CheckAllPieces();
        }

        private void CheckAllPieces()
        {
            if (boardManager == null || boardManager.BoardState == null)
            {
                return;
            }

            BoardState state = boardManager.BoardState;
            PieceColor currentTurn = state.currentTurn;

            System.Collections.Generic.List<ChessPiece> pieces = state.GetAllPieces();
            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece == null || piece.color != currentTurn)
                {
                    continue;
                }

                CheckRebellion(piece);
            }
        }

        private void CheckRebellion(ChessPiece piece)
        {
            float defectionChance = (10 - piece.support) * 2f + (-piece.disposition) * 0.1f;
            defectionChance = ApplyKingStateDefectionModifiers(piece, defectionChance);
            defectionChance = Mathf.Clamp(defectionChance, 0f, 80f);

            if (defectionChance > 0f)
            {
                float roll = Random.Range(0f, 100f);
                if (roll < defectionChance)
                {
                    piece.color = piece.color == PieceColor.White ? PieceColor.Black : PieceColor.White;
                    piece.side = PieceSideResolver.Resolve(piece.type, piece.rank);
                    piece.isBetrayed = true;
                    boardManager.RefreshPieceVisuals();
                    EventBus.Instance.PublishDefectionTriggered(piece);
                    return;
                }
            }

            float rebellionChance = (20 - piece.support) * 1.5f;
            rebellionChance += piece.rebellionWeight * 10f;
            if (KingStateEvaluator.IsTyrant(piece.color))
            {
                rebellionChance += 30f;
            }

            rebellionChance = Mathf.Clamp(rebellionChance, 0f, 60f);

            if (rebellionChance <= 0f)
            {
                return;
            }

            float rebellionRoll = Random.Range(0f, 100f);
            if (rebellionRoll < rebellionChance)
            {
                if (piece.isOffBoard)
                {
                    boardManager.FrontDeploy(piece);
                }

                EventBus.Instance.PublishRebellionTriggered(piece);
            }
        }

        private float ApplyKingStateDefectionModifiers(ChessPiece piece, float defectionChance)
        {
            PieceColor opponent = piece.color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            if (KingStateEvaluator.IsBenevolent(piece.color))
            {
                defectionChance -= 20f;
            }

            if (KingStateEvaluator.IsBenevolent(opponent))
            {
                defectionChance += 10f;
            }

            if (KingStateEvaluator.IsTyrant(piece.color))
            {
                defectionChance += 30f;
            }

            return defectionChance;
        }
    }
}
