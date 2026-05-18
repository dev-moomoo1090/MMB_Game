using UnityEngine;

namespace MMBGame
{
    public class RebellionSystem : MonoBehaviour
    {
        private BoardManager boardManager;

        public void Initialize()
        {
            boardManager = SceneComponentResolver.Resolve<BoardManager>();
            EventBus.Instance.OnSupportChanged -= HandleSupportChanged;
            EventBus.Instance.OnSupportChanged += HandleSupportChanged;
            EventBus.Instance.OnRebellionAttempt -= HandleRebellionAttempt;
            EventBus.Instance.OnRebellionAttempt += HandleRebellionAttempt;
        }

        private void OnDestroy()
        {
            EventBus.Instance.OnSupportChanged -= HandleSupportChanged;
            EventBus.Instance.OnRebellionAttempt -= HandleRebellionAttempt;
        }

        private void HandleRebellionAttempt(ChessPiece piece)
        {
            if (boardManager == null || boardManager.BoardState == null || piece == null || piece.type == PieceType.King)
            {
                return;
            }

            TriggerRebellion(piece);
        }

        private void HandleSupportChanged(ChessPiece piece, int delta, string reason)
        {
            if (delta >= 0 || boardManager == null || boardManager.BoardState == null || piece == null || piece.type == PieceType.King)
            {
                return;
            }

            QaLog.Write("반란", "지지도 하락으로 반란 판정 시작 사유=" + reason + " 변화량=" + delta + " 기물=" + QaLog.PieceLabel(piece));
            RollImmediateRebellion(piece);
        }

        private void RollImmediateRebellion(ChessPiece piece)
        {
            float rebellionChance = GetRebellionChance(piece, boardManager.BoardState);
            int globalWeight = boardManager.BoardState != null ? boardManager.BoardState.globalRebellionWeight : 0;
            float tyrantModifier = KingStateEvaluator.IsTyrant(piece.color) ? 30f : 0f;
            float roll = Random.Range(0f, 100f);
            if (rebellionChance <= 0f)
            {
                QaLog.Write("반란", "확률 없음 기물=" + QaLog.PieceLabel(piece) + " 공식=20-지지도(" + piece.support + ")+전역(" + globalWeight + ")+개별가중치(" + piece.rebellionWeight + ")+폭군보정(" + tyrantModifier + ") 확률=" + rebellionChance + " 누적=" + piece.rebellionSuccessCount + "/3");
                return;
            }

            if (roll >= rebellionChance)
            {
                QaLog.Write("반란", "판정 실패 기물=" + QaLog.PieceLabel(piece) + " 공식=20-지지도(" + piece.support + ")+전역(" + globalWeight + ")+개별가중치(" + piece.rebellionWeight + ")+폭군보정(" + tyrantModifier + ") 확률=" + rebellionChance + " 굴림=" + roll + " 누적=" + piece.rebellionSuccessCount + "/3");
                return;
            }

            QaLog.Write("반란", "판정 성공 기물=" + QaLog.PieceLabel(piece) + " 공식=20-지지도(" + piece.support + ")+전역(" + globalWeight + ")+개별가중치(" + piece.rebellionWeight + ")+폭군보정(" + tyrantModifier + ") 확률=" + rebellionChance + " 굴림=" + roll + " 이전누적=" + piece.rebellionSuccessCount + "/3");
            TriggerRebellion(piece);
        }

        private void TriggerRebellion(ChessPiece piece)
        {
            piece.rebellionSuccessCount += 1;
            if (piece.rebellionSuccessCount < 3)
            {
                boardManager.RefreshPieceVisuals();
                EventBus.Instance.PublishRebellionTriggered(piece);
                QaLog.Write("반란", "누적 기물=" + QaLog.PieceLabel(piece) + " 누적=" + piece.rebellionSuccessCount + "/3");
                return;
            }

            ResolveRebellion(piece);
        }

        private void ResolveRebellion(ChessPiece piece)
        {
            PieceColor previousColor = piece.color;
            PieceColor newColor = GetOpponent(previousColor);
            if (newColor == PieceColor.None)
            {
                return;
            }

            int previousFile = piece.file;
            int previousRank = piece.rank;
            int previousSupport = piece.support;
            if (piece.isOffBoard)
            {
                boardManager.FrontDeploy(piece);
                previousFile = piece.file;
                previousRank = piece.rank;
            }

            piece.color = newColor;
            piece.movementControllerColor = newColor;
            piece.side = PieceSideResolver.Resolve(piece.type, piece.rank);
            piece.lane = PieceSideResolver.ResolveLane(piece.type, piece.rank);
            piece.support = PoliticalStatService.ClampSupport(100 - piece.support);
            piece.isBetrayed = false;
            piece.rebellionSuccessCount = 0;
            piece.rebellionWeight = 0f;
            boardManager.RefreshPieceVisuals();
            EventBus.Instance.PublishRebellionTriggered(piece);
            QaLog.Write("반란", "실제 반란 발동 기물=" + QaLog.PieceLabel(piece) + " 이전색상=" + previousColor + " 새색상=" + newColor + " 지지도=" + previousSupport + "->" + piece.support + " 누적=3->0");
            RollAdjacentRebellions(previousColor, previousFile, previousRank, piece);
        }

        private void RollAdjacentRebellions(PieceColor targetColor, int centerFile, int centerRank, ChessPiece origin)
        {
            BoardState state = boardManager.BoardState;
            for (int file = centerFile - 1; file <= centerFile + 1; file++)
            {
                for (int rank = centerRank - 1; rank <= centerRank + 1; rank++)
                {
                    if (!state.IsInBounds(file, rank))
                    {
                        continue;
                    }

                    ChessPiece neighbor = state.GetPiece(file, rank);
                    if (neighbor == null || neighbor == origin || neighbor.color != targetColor)
                    {
                        continue;
                    }

                    QaLog.Write("반란", "주변 연쇄 판정 대상=" + QaLog.PieceLabel(neighbor) + " 시작점=" + QaLog.PieceLabel(origin) + " 중심=(" + centerFile + "," + centerRank + ")");
                    RollImmediateRebellion(neighbor);
                }
            }
        }

        private PieceColor GetOpponent(PieceColor color)
        {
            if (color == PieceColor.White)
            {
                return PieceColor.Black;
            }

            if (color == PieceColor.Black)
            {
                return PieceColor.White;
            }

            return PieceColor.None;
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
