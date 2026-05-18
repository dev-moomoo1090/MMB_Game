using System;
using System.Collections.Generic;

namespace MMBGame
{
    public partial class KingStateEffectApplier
    {
        private int CountPieces(BoardState board, PieceColor color, Func<ChessPiece, bool> predicate)
        {
            int count = 0;
            List<ChessPiece> pieces = board.GetAllPieces();
            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece != null && piece.color == color && predicate(piece))
                {
                    count++;
                }
            }

            return count;
        }

        private int SumSupport(BoardState board, PieceColor color)
        {
            int total = 0;
            List<ChessPiece> pieces = board.GetAllPieces();
            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece != null && piece.color == color && IsPoliticalPiece(piece))
                {
                    total += piece.support;
                }
            }

            return total;
        }

        private void ApplyToAllPieces(BoardState board, PieceColor color, Action<ChessPiece> effect)
        {
            List<ChessPiece> pieces = board.GetAllPieces();
            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece != null && piece.color == color && IsPoliticalPiece(piece))
                {
                    effect(piece);
                }
            }
        }

        private bool IsPoliticalPiece(ChessPiece piece)
        {
            return PieceClassifier.IsPoliticalPiece(piece);
        }

        private void EnsureReferences()
        {
            boardManager = SceneComponentResolver.Resolve(boardManager);
            politicsManager = SceneComponentResolver.Resolve(politicsManager);
        }
    }
}
