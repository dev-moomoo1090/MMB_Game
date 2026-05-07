using System.Collections.Generic;

namespace MMBGame
{
    public class LoanAction : FiscalAction
    {
        private const float INTEREST_RATE = 0.2f;

        private readonly List<(ChessPiece piece, int principal, int repayAmount, int turnsRemaining)> activeLoans;

        public LoanAction()
        {
            activeLoans = new List<(ChessPiece piece, int principal, int repayAmount, int turnsRemaining)>();
        }

        public override string ActionName => "LoanAction";
        public override bool RequiresPieceSelection => false;
        public override bool RequiresValueInput => true;

        public override bool Execute(ChessPiece targetPiece, PlayerState actor, int inputValue, PoliticsManager manager)
        {
            if (actor == null || manager == null || inputValue <= 0)
            {
                return false;
            }

            ChessPiece candidate = FindLoanCandidate(actor.color, manager.BoardManager.BoardState);
            if (candidate == null || HasActiveLoan(candidate))
            {
                return false;
            }

            if (!actor.SpendGold(inputValue))
            {
                return false;
            }

            int repayAmount = (int)(inputValue * (1f + INTEREST_RATE));
            int turnsRemaining = System.Math.Max(1, inputValue / System.Math.Max(1, candidate.taxPerTurn));
            PoliticalStatService.ChangeSupport(candidate, System.Math.Max(1, inputValue / 20), ActionName);
            activeLoans.Add((candidate, inputValue, repayAmount, turnsRemaining));
            return true;
        }

        public void AdvanceLoans(PlayerState actor)
        {
            if (actor == null)
            {
                return;
            }

            for (int i = activeLoans.Count - 1; i >= 0; i--)
            {
                if (activeLoans[i].piece == null || activeLoans[i].piece.color != actor.color)
                {
                    continue;
                }

                (ChessPiece piece, int principal, int repayAmount, int turnsRemaining) loan = activeLoans[i];
                loan.turnsRemaining--;
                if (loan.turnsRemaining > 0)
                {
                    activeLoans[i] = loan;
                    continue;
                }

                actor.AddGold(loan.repayAmount);
                activeLoans.RemoveAt(i);
            }
        }

        public void OnPieceDied(ChessPiece piece)
        {
            if (piece == null)
            {
                return;
            }

            for (int i = activeLoans.Count - 1; i >= 0; i--)
            {
                if (activeLoans[i].piece == piece)
                {
                    activeLoans.RemoveAt(i);
                }
            }
        }

        public bool Repay(PlayerState actor)
        {
            if (actor == null)
            {
                return false;
            }

            for (int i = 0; i < activeLoans.Count; i++)
            {
                if (activeLoans[i].piece == null || activeLoans[i].piece.color != actor.color)
                {
                    continue;
                }

                actor.AddGold(activeLoans[i].repayAmount);
                activeLoans.RemoveAt(i);
                return true;
            }

            return false;
        }

        private bool HasActiveLoan(ChessPiece piece)
        {
            for (int i = 0; i < activeLoans.Count; i++)
            {
                if (activeLoans[i].piece == piece)
                {
                    return true;
                }
            }

            return false;
        }

        private ChessPiece FindLoanCandidate(PieceColor color, BoardState state)
        {
            if (state == null)
            {
                return null;
            }

            ChessPiece bestCandidate = null;
            List<ChessPiece> pieces = state.GetAllPieces();
            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece == null || piece.color != color || piece.type == PieceType.King)
                {
                    continue;
                }

                int willingness = piece.acceptWeight + piece.taxPerTurn * 10 - piece.support;
                if (willingness <= 0)
                {
                    continue;
                }

                if (bestCandidate == null || piece.taxPerTurn > bestCandidate.taxPerTurn)
                {
                    bestCandidate = piece;
                }
            }

            return bestCandidate;
        }
    }
}
