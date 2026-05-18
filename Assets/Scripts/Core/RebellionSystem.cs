using UnityEngine;

namespace MMBGame
{
    public class RebellionSystem : MonoBehaviour
    {
        private BoardManager boardManager;

        public void Initialize()
        {
            boardManager = SceneComponentResolver.Resolve<BoardManager>();
            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
            EventBus.Instance.OnPhaseChanged += HandlePhaseChanged;
            EventBus.Instance.OnSupportChanged -= HandleSupportChanged;
            EventBus.Instance.OnSupportChanged += HandleSupportChanged;
        }

        private void OnDestroy()
        {
            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
            EventBus.Instance.OnSupportChanged -= HandleSupportChanged;
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.ChessPhase)
            {
                return;
            }

            CheckAllPieces();
        }

        private void HandleSupportChanged(ChessPiece piece, int delta, string reason)
        {
            if (delta >= 0 || boardManager == null || boardManager.BoardState == null || piece == null)
            {
                return;
            }

            RollImmediateRebellion(piece);
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
            BoardState state = boardManager.BoardState;
            float defectionChance = GetDefectionChance(piece, state);

            if (defectionChance > 0f)
            {
                float roll = Random.Range(0f, 100f);
                if (roll < defectionChance)
                {
                    piece.color = piece.color == PieceColor.White ? PieceColor.Black : PieceColor.White;
                    piece.side = PieceSideResolver.Resolve(piece.type, piece.rank);
                    piece.movementControllerColor = piece.color;
                    piece.isBetrayed = true;
                    boardManager.RefreshPieceVisuals();
                    EventBus.Instance.PublishDefectionTriggered(piece);
                    return;
                }
            }

            float rebellionChance = GetRebellionChance(piece, state);

            if (rebellionChance <= 0f)
            {
                return;
            }

            float rebellionRoll = Random.Range(0f, 100f);
            if (rebellionRoll < rebellionChance)
            {
                TriggerRebellion(piece);
            }
        }

        private void RollImmediateRebellion(ChessPiece piece)
        {
            float rebellionChance = GetRebellionChance(piece, boardManager.BoardState);
            if (rebellionChance <= 0f || Random.Range(0f, 100f) >= rebellionChance)
            {
                return;
            }

            TriggerRebellion(piece);
        }

        private void TriggerRebellion(ChessPiece piece)
        {
            piece.rebellionSuccessCount += 1;
            if (piece.rebellionSuccessCount >= 3)
            {
                piece.rebellionWeight = Mathf.Max(piece.rebellionWeight, 1f);
                piece.movementControllerColor = piece.color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            }

            if (piece.isOffBoard)
            {
                boardManager.FrontDeploy(piece);
            }

            boardManager.RefreshPieceVisuals();
            EventBus.Instance.PublishRebellionTriggered(piece);
        }

        public static float GetDefectionChance(ChessPiece piece, BoardState state)
        {
            if (piece == null)
            {
                return 0f;
            }

            float chance = 30 - piece.support;
            chance += state != null ? state.globalDefectionWeight : 0;
            chance += piece.defectionWeight;
            PieceColor opponent = piece.color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            if (KingStateEvaluator.IsBenevolent(piece.color))
            {
                chance -= 20f;
            }

            if (KingStateEvaluator.IsBenevolent(opponent))
            {
                chance += 10f;
            }

            if (KingStateEvaluator.IsTyrant(piece.color))
            {
                chance += 30f;
            }

            return Mathf.Clamp(chance, 0f, 100f);
        }

        public static float GetRebellionChance(ChessPiece piece, BoardState state)
        {
            if (piece == null)
            {
                return 0f;
            }

            float chance = 20 - piece.support;
            chance += state != null ? state.globalRebellionWeight : 0;
            chance += piece.rebellionWeight;
            if (KingStateEvaluator.IsTyrant(piece.color))
            {
                chance += 30f;
            }

            return Mathf.Clamp(chance, 0f, 100f);
        }
    }
}
