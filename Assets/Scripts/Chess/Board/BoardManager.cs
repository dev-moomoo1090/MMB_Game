using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private BoardPieceVisuals pieceVisuals;
        [SerializeField] private List<PieceSetupDefinition> pieceDefinitions = new List<PieceSetupDefinition>();

        public BoardState BoardState { get; private set; }

        private void Awake()
        {
            BoardState = new BoardState();
            SetupInitialPosition();
            BoardState.RecordPosition();
            SyncPieceVisuals();
        }

        private void SetupInitialPosition()
        {
            Place(new Rook(PieceColor.White, 0, 0));
            Place(new Knight(PieceColor.White, 1, 0));
            Place(new Bishop(PieceColor.White, 2, 0));
            Place(new Queen(PieceColor.White, 3, 0));
            Place(new King(PieceColor.White, 4, 0));
            Place(new Bishop(PieceColor.White, 5, 0));
            Place(new Knight(PieceColor.White, 6, 0));
            Place(new Rook(PieceColor.White, 7, 0));
            for (int f = 0; f < 8; f++) Place(new Pawn(PieceColor.White, f, 1));

            Place(new Rook(PieceColor.Black, 0, 7));
            Place(new Knight(PieceColor.Black, 1, 7));
            Place(new Bishop(PieceColor.Black, 2, 7));
            Place(new Queen(PieceColor.Black, 3, 7));
            Place(new King(PieceColor.Black, 4, 7));
            Place(new Bishop(PieceColor.Black, 5, 7));
            Place(new Knight(PieceColor.Black, 6, 7));
            Place(new Rook(PieceColor.Black, 7, 7));
            for (int f = 0; f < 8; f++) Place(new Pawn(PieceColor.Black, f, 6));
        }

        private void Place(ChessPiece piece)
        {
            piece.ApplySetup(GetPieceDefinition(piece.color, piece.side, piece.type));
            BoardState.squares[piece.file, piece.rank].piece = piece;
        }

        public List<Move> GetLegalMoves(int file, int rank)
        {
            return MoveValidator.GetLegalMoves(BoardState, file, rank);
        }

        public bool TryMove(int fromFile, int fromRank, int toFile, int toRank, PieceType promotionPiece = PieceType.Queen)
        {
            ChessPiece piece = BoardState.GetPiece(fromFile, fromRank);
            if (piece == null || piece.color != BoardState.currentTurn) return false;
            if (ObedienceSystem.IsRefused(piece))
            {
                EventBus.Instance.PublishMovementRefused(piece);
                return false;
            }

            List<Move> legal = GetLegalMoves(fromFile, fromRank);
            foreach (Move lm in legal)
            {
                if (lm.toFile != toFile || lm.toRank != toRank) continue;
                if (lm.specialMove == SpecialMoveType.Promotion && lm.promotionPiece != promotionPiece) continue;
                ChessPiece capturedPiece = GetCapturedPiece(lm);
                if (capturedPiece != null)
                {
                    capturedPiece.ClearOneTimeMovePatterns();
                }

                SpecialMoves.ApplyMove(BoardState, lm);
                ApplySetupAfterMove(lm);
                piece.ClearOneTimeMovePatterns();
                BoardState.RecordPosition();
                SyncPieceVisuals();
                return true;
            }
            return false;
        }

        public bool PlaceObstacle(ChessPiece obstacle, int file, int rank)
        {
            if (obstacle == null || !BoardState.IsInBounds(file, rank) || BoardState.GetPiece(file, rank) != null)
            {
                return false;
            }

            obstacle.file = file;
            obstacle.rank = rank;
            obstacle.side = PieceSideResolver.Resolve(obstacle.type, file);
            obstacle.ApplySetup(GetPieceDefinition(obstacle.color, obstacle.side, obstacle.type));
            BoardState.squares[file, rank].piece = obstacle;
            SyncPieceVisuals();
            return true;
        }

        public bool RearDeploy(ChessPiece piece)
        {
            if (piece == null || piece.isOffBoard || !BoardState.IsInBounds(piece.file, piece.rank))
            {
                return false;
            }

            if (CheckDetector.IsSquareAttacked(BoardState, piece.file, piece.rank, piece.color == PieceColor.White ? PieceColor.Black : PieceColor.White))
            {
                return false;
            }

            piece.offBoardOrigin = (piece.file, piece.rank);
            piece.isOffBoard = true;
            BoardState.squares[piece.file, piece.rank].piece = null;
            if (!BoardState.offBoardPieces.Contains(piece))
            {
                BoardState.offBoardPieces.Add(piece);
            }

            SyncPieceVisuals();
            return true;
        }

        public bool FrontDeploy(ChessPiece piece)
        {
            if (piece == null || !piece.isOffBoard)
            {
                return false;
            }

            for (int radius = 0; radius <= 3; radius++)
            {
                for (int file = piece.offBoardOrigin.col - radius; file <= piece.offBoardOrigin.col + radius; file++)
                {
                    for (int rank = piece.offBoardOrigin.row - radius; rank <= piece.offBoardOrigin.row + radius; rank++)
                    {
                        if (TryPlaceFrontDeployedPiece(piece, file, rank))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool TryPlaceFrontDeployedPiece(ChessPiece piece, int file, int rank)
        {
            if (!BoardState.IsInBounds(file, rank) || BoardState.GetPiece(file, rank) != null)
            {
                return false;
            }

            BoardState.squares[file, rank].piece = piece;
            int previousFile = piece.file;
            int previousRank = piece.rank;
            piece.file = file;
            piece.rank = rank;
            piece.isOffBoard = false;

            if (CheckDetector.IsInCheck(BoardState, piece.color))
            {
                BoardState.squares[file, rank].piece = null;
                piece.file = previousFile;
                piece.rank = previousRank;
                piece.isOffBoard = true;
                return false;
            }

            BoardState.offBoardPieces.Remove(piece);
            SyncPieceVisuals();
            return true;
        }

        private ChessPiece GetCapturedPiece(Move move)
        {
            if (move.specialMove == SpecialMoveType.EnPassant)
            {
                return BoardState.GetPiece(move.toFile, move.fromRank);
            }

            return BoardState.GetPiece(move.toFile, move.toRank);
        }

        public GameResult CheckGameResult()
        {
            PieceColor turn = BoardState.currentTurn;
            bool inCheck = CheckDetector.IsInCheck(BoardState, turn);
            bool hasLegal = MoveValidator.GetAllLegalMoves(BoardState, turn).Count > 0;

            if (inCheck && !hasLegal) return GameResult.Checkmate;
            if (!inCheck && !hasLegal) return GameResult.Stalemate;
            if (DrawDetector.IsThreefoldRepetition(BoardState)) return GameResult.Draw;
            if (DrawDetector.IsInsufficientMaterial(BoardState)) return GameResult.Draw;
            if (inCheck) return GameResult.Check;
            return GameResult.InProgress;
        }

        public void RefreshPieceVisuals()
        {
            SyncPieceVisuals();
        }

        private PieceSetupDefinition GetPieceDefinition(PieceColor color, PieceSide side, PieceType type)
        {
            PieceSetupDefinition fallbackDefinition = null;
            for (int i = 0; i < pieceDefinitions.Count; i++)
            {
                PieceSetupDefinition definition = pieceDefinitions[i];
                if (definition == null || definition.color != color || definition.type != type)
                {
                    continue;
                }

                if (definition.side == side)
                {
                    return definition;
                }

                if (definition.side == PieceSide.None)
                {
                    fallbackDefinition = definition;
                }
            }

            return fallbackDefinition;
        }

        private void ApplySetupAfterMove(Move move)
        {
            if (move.specialMove != SpecialMoveType.Promotion)
            {
                return;
            }

            ChessPiece promotedPiece = BoardState.GetPiece(move.toFile, move.toRank);
            if (promotedPiece != null)
            {
                promotedPiece.side = PieceSideResolver.Resolve(promotedPiece.type, move.toFile);
                promotedPiece.ApplySetup(GetPieceDefinition(promotedPiece.color, promotedPiece.side, promotedPiece.type));
                promotedPiece.hasMoved = true;
            }
        }

        private void SyncPieceVisuals()
        {
            if (pieceVisuals == null)
            {
                pieceVisuals = FindObjectOfType<BoardPieceVisuals>();
            }

            if (pieceVisuals == null)
            {
                pieceVisuals = gameObject.AddComponent<BoardPieceVisuals>();
            }

            if (pieceVisuals != null)
            {
                pieceVisuals.Sync(BoardState, pieceDefinitions);
            }
        }

        [ContextMenu("Fill Default Piece Definitions")]
        private void FillDefaultPieceDefinitions()
        {
            PieceSetupDefaults.Fill(pieceDefinitions);
        }
    }
}
