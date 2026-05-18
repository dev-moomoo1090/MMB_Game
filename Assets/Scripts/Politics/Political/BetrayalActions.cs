using System;
using System.Collections.Generic;

namespace MMBGame
{
    public class BetrayalContactAction : PoliticalAction
    {
        public override string ActionName => "배신 - 접촉";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => false;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            if (target == null || target.color == actorColor || !target.isBetrayed)
            {
                return false;
            }

            EventBus.Instance.PublishIntelligenceGathered(target);
            return true;
        }
    }

    public class BetrayalInfoAction : PoliticalAction
    {
        public override string ActionName => "배신 - 정보";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => false;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            if (target == null || target.color == actorColor || !target.isBetrayed)
            {
                return false;
            }

            target.isIntelTarget = true;
            EventBus.Instance.PublishIntelligenceGathered(target);
            return true;
        }
    }

    public class BetrayalBlunderAction : PoliticalAction
    {
        public override string ActionName => "배신 - 실책";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => false;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            if (target == null || target.color == actorColor || !target.isBetrayed || target.type != PieceType.Bishop)
            {
                return false;
            }

            PoliticalStatService.ChangeSupportFromPoliticalAction(target, -5, actorColor, ActionName);
            int beforeAcceptWeight = target.acceptWeight;
            target.acceptWeight += 5;
            QaLog.Write("기물가중치", "수락 가중치 변경 행동=" + ActionName + " 기물=" + QaLog.PieceLabel(target) + " 변화량=5 이전=" + beforeAcceptWeight + " 이후=" + target.acceptWeight);
            EventBus.Instance.PublishIntelligenceGathered(target);
            return true;
        }
    }

    public class BetrayalFactionAction : PoliticalAction
    {
        public override string ActionName => "배신 - 파벌";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => false;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            if (target == null || target.color == actorColor || !target.isBetrayed || target.type != PieceType.Queen)
            {
                return false;
            }

            PoliticalStatService.ChangeSupportFromPoliticalAction(target, -8, actorColor, ActionName);
            float beforeRebellionWeight = target.rebellionWeight;
            target.rebellionWeight += 1f;
            QaLog.Write("기물가중치", "반란 가중치 변경 행동=" + ActionName + " 기물=" + QaLog.PieceLabel(target) + " 변화량=1 이전=" + beforeRebellionWeight + " 이후=" + target.rebellionWeight);
            EventBus.Instance.PublishIntelligenceGathered(target);
            return true;
        }
    }

    public class BetrayalAssassinationAction : PoliticalAction
    {
        public override string ActionName => "배신 - 암살";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => false;

        public override bool Execute(ChessPiece target, PieceColor actorColor, PoliticalManager manager, int value)
        {
            if (target == null || target.color == actorColor || !target.isBetrayed || manager == null || manager.BoardManager == null)
            {
                return false;
            }

            BoardState state = manager.BoardManager.BoardState;
            Move assassinationMove = FindAssassinationMove(state, target);
            if (assassinationMove == null || !IsAccepted(target))
            {
                return false;
            }

            ChessPiece victim = state.GetPiece(assassinationMove.toFile, assassinationMove.toRank);
            if (victim == null || victim.color == actorColor || victim.isBetrayed)
            {
                return false;
            }

            victim.ClearOneTimeMovePatterns();
            EventBus.Instance.PublishPieceCapturePending(victim);
            SpecialMoves.ApplyMove(state, assassinationMove);
            target.ClearOneTimeMovePatterns();
            float beforeRebellionWeight = target.rebellionWeight;
            target.rebellionWeight += 3f;
            QaLog.Write("기물가중치", "반란 가중치 변경 행동=" + ActionName + " 기물=" + QaLog.PieceLabel(target) + " 변화량=3 이전=" + beforeRebellionWeight + " 이후=" + target.rebellionWeight);
            EventBus.Instance.PublishRebellionTriggered(target);
            manager.BoardManager.RefreshPieceVisuals();
            if (victim.type == PieceType.King)
            {
                manager.BoardManager.TryEndGameByKingCapture(actorColor);
            }

            return true;
        }

        private Move FindAssassinationMove(BoardState state, ChessPiece assassin)
        {
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    ChessPiece victim = state.GetPiece(file, rank);
                    if (victim == null || victim.color != assassin.color || victim.isBetrayed)
                    {
                        continue;
                    }

                    PieceColor originalColor = victim.color;
                    victim.color = assassin.color == PieceColor.White ? PieceColor.Black : PieceColor.White;
                    List<Move> moves = MoveGenerator.GeneratePseudoLegalMoves(state, assassin.file, assassin.rank);
                    victim.color = originalColor;
                    for (int i = 0; i < moves.Count; i++)
                    {
                        if (moves[i].toFile == file && moves[i].toRank == rank)
                        {
                            return moves[i];
                        }
                    }
                }
            }

            return null;
        }

        private bool IsAccepted(ChessPiece assassin)
        {
            int chance = Math.Max(5, 100 - assassin.support - assassin.acceptWeight);
            return UnityEngine.Random.Range(0, 100) < chance;
        }
    }
}
