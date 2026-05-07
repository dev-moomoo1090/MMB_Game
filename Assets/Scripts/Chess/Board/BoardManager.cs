using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private BoardPieceSetupManager pieceSetupManager;

        public BoardState BoardState { get; private set; }

        private TurnManager turnManager;

        private void Awake()
        {
            EnsurePieceSetupManager();
            BoardState = new BoardState();
            SetupInitialPosition();
            BoardState.RecordPosition();
            RefreshPieceVisuals();
        }

        private void SetupInitialPosition()
        {
            Place(new Rook(PieceColor.White, 0, 0));
            Place(new Knight(PieceColor.White, 0, 1));
            Place(new Bishop(PieceColor.White, 0, 2));
            Place(new Queen(PieceColor.White, 0, 3));
            Place(new King(PieceColor.White, 0, 4));
            Place(new Bishop(PieceColor.White, 0, 5));
            Place(new Knight(PieceColor.White, 0, 6));
            Place(new Rook(PieceColor.White, 0, 7));
            for (int r = 0; r < 8; r++) Place(new Pawn(PieceColor.White, 1, r));

            Place(new Rook(PieceColor.Black, 7, 0));
            Place(new Knight(PieceColor.Black, 7, 1));
            Place(new Bishop(PieceColor.Black, 7, 2));
            Place(new Queen(PieceColor.Black, 7, 3));
            Place(new King(PieceColor.Black, 7, 4));
            Place(new Bishop(PieceColor.Black, 7, 5));
            Place(new Knight(PieceColor.Black, 7, 6));
            Place(new Rook(PieceColor.Black, 7, 7));
            for (int r = 0; r < 8; r++) Place(new Pawn(PieceColor.Black, 6, r));
        }

        private void Place(ChessPiece piece)
        {
            pieceSetupManager.ApplySetup(piece);
            BoardState.squares[piece.file, piece.rank].piece = piece;
        }

        public List<Move> GetLegalMoves(int file, int rank)
        {
            return MoveValidator.GetLegalMoves(BoardState, file, rank);
        }

        public bool TryMove(int fromFile, int fromRank, int toFile, int toRank, PieceType promotionPiece = PieceType.Queen)
        {
            if (!IsChessPhase())
            {
                return false;
            }

            ChessPiece piece = BoardState.GetPiece(fromFile, fromRank);
            if (piece == null || piece.color != BoardState.currentTurn) return false;
            if (ObedienceSystem.IsRefused(piece))
            {
                if (HonorPiecePassiveSystem.Instance == null || !HonorPiecePassiveSystem.Instance.TryRerollMovementRefusal(piece))
                {
                    EventBus.Instance.PublishMovementRefused(piece);
                    return false;
                }
            }

            List<Move> legal = GetLegalMoves(fromFile, fromRank);
            foreach (Move lm in legal)
            {
                if (lm.toFile != toFile || lm.toRank != toRank) continue;
                if (lm.specialMove == SpecialMoveType.Promotion && lm.promotionPiece != promotionPiece) continue;
                BoardState.capturedThisTurn.Clear();
                ChessPiece capturedPiece = GetCapturedPiece(lm);
                if (capturedPiece != null)
                {
                    capturedPiece.ClearOneTimeMovePatterns();
                    BoardState.capturedThisTurn.Add(capturedPiece);
                }

                SpecialMoves.ApplyMove(BoardState, lm);
                pieceSetupManager.ApplyPromotionSetup(BoardState, lm);
                piece.ClearOneTimeMovePatterns();
                BoardState.RecordPosition();
                HonorPiecePassiveSystem.Instance?.HandlePieceMoved(piece, BoardState);
                RefreshPieceVisuals();
                AdvanceTurnAfterMove();
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
            pieceSetupManager.ApplySetup(obstacle);
            BoardState.squares[file, rank].piece = obstacle;
            RefreshPieceVisuals();
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

            RefreshPieceVisuals();
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
            RefreshPieceVisuals();
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
            EnsurePieceSetupManager();
            pieceSetupManager.SyncVisuals(BoardState);
        }

        private void EnsurePieceSetupManager()
        {
            if (pieceSetupManager == null)
            {
                pieceSetupManager = FindObjectOfType<BoardPieceSetupManager>();
            }

            if (pieceSetupManager == null)
            {
                GameObject managerObject = new GameObject("PieceSetupManager");
                pieceSetupManager = managerObject.AddComponent<BoardPieceSetupManager>();
            }
        }

        private void AdvanceTurnAfterMove()
        {
            if (turnManager == null)
            {
                turnManager = FindFirstObjectByType<TurnManager>();
            }

            if (BoardState.capturedThisTurn.Count > 0)
            {
                PieceColor attacker = turnManager != null ? turnManager.CurrentColor : PieceColor.White;
                turnManager?.EnterPrisonerPhase();
                PrisonerPanel.Show(attacker, BoardState.capturedThisTurn, pieceSetupManager);
            }
            else
            {
                turnManager?.EndPhase();
            }
        }

        private bool IsChessPhase()
        {
            if (turnManager == null)
            {
                turnManager = FindFirstObjectByType<TurnManager>();
            }

            return turnManager == null || turnManager.CurrentPhase == GamePhase.ChessPhase;
        }
    }
}
