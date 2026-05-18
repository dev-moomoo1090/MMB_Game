using System.Collections.Generic;

namespace MMBGame
{
    public abstract class ChessPiece
    {
        private static readonly Dictionary<string, List<MovePattern>> registeredOneTimePatterns = new Dictionary<string, List<MovePattern>>();

        public string pieceName;
        public int support;
        public int taxPerTurn;
        public List<MovePattern> originalMovePatterns;
        public List<MovePattern> currentMovePatterns;
        public List<MovePattern> oneTimeMovePatterns;
        public float rebellionWeight;
        public PieceColor color;
        public PieceSide side;
        public PieceLane lane;
        public PieceColor movementControllerColor;
        public PieceType type;
        public int file;
        public int rank;
        public int taxModifier;
        public bool isOffBoard;
        public (int col, int row) offBoardOrigin;
        public bool hasMoved;
        public bool isBetrayed;
        public bool isIntelTarget;
        public int punishCount;
        public int disposition;
        public int acceptWeight;
        public int defectionWeight;
        public int rebellionSuccessCount;

        protected ChessPiece(PieceColor color, PieceType type, int file, int rank)
        {
            this.color = color;
            this.type = type;
            this.file = file;
            this.rank = rank;
            side = PieceSideResolver.Resolve(type, rank);
            lane = PieceSideResolver.ResolveLane(type, rank);
            movementControllerColor = color;
            support = 50;
            taxPerTurn = 1;
            taxModifier = 1;
            rebellionWeight = 0f;
            isOffBoard = false;
            offBoardOrigin = (file, rank);
            hasMoved = false;
            isBetrayed = false;
            isIntelTarget = false;
            punishCount = 0;
            disposition = 0;
            acceptWeight = 50;
            defectionWeight = 0;
            rebellionSuccessCount = 0;
            originalMovePatterns = new List<MovePattern>();
            currentMovePatterns = new List<MovePattern>();
            oneTimeMovePatterns = new List<MovePattern>();
            SetupMovePatterns();
            currentMovePatterns.AddRange(originalMovePatterns);
            RestoreOneTimePatterns();
        }

        protected abstract void SetupMovePatterns();
        public abstract ChessPiece Clone();

        public void ResetMovePatterns()
        {
            currentMovePatterns.Clear();
            currentMovePatterns.AddRange(originalMovePatterns);
        }

        public void ApplySetup(PieceSetupDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            support = PoliticalStatService.ClampSupport(definition.initialSupport);
            taxPerTurn = definition.initialTaxPerTurn;
            if (definition.baseMovePatterns == null || definition.baseMovePatterns.Count == 0)
            {
                return;
            }

            originalMovePatterns.Clear();
            currentMovePatterns.Clear();
            for (int i = 0; i < definition.baseMovePatterns.Count; i++)
            {
                MovePattern pattern = definition.baseMovePatterns[i];
                if (pattern == null)
                {
                    continue;
                }

                MovePattern copiedPattern = new MovePattern(pattern.deltaFile, pattern.deltaRank, pattern.isSliding, pattern.captureOnly, pattern.moveOnly, pattern.isCannon);
                originalMovePatterns.Add(copiedPattern);
                currentMovePatterns.Add(new MovePattern(pattern.deltaFile, pattern.deltaRank, pattern.isSliding, pattern.captureOnly, pattern.moveOnly, pattern.isCannon));
            }
        }

        public void CopyStateTo(ChessPiece target)
        {
            if (target == null)
            {
                return;
            }

            target.pieceName = pieceName;
            target.support = support;
            target.taxPerTurn = taxPerTurn;
            target.rebellionWeight = rebellionWeight;
            target.side = side;
            target.lane = lane;
            target.movementControllerColor = movementControllerColor;
            target.taxModifier = taxModifier;
            target.isOffBoard = isOffBoard;
            target.offBoardOrigin = offBoardOrigin;
            target.hasMoved = hasMoved;
            target.isBetrayed = isBetrayed;
            target.isIntelTarget = isIntelTarget;
            target.punishCount = punishCount;
            target.disposition = disposition;
            target.acceptWeight = acceptWeight;
            target.defectionWeight = defectionWeight;
            target.rebellionSuccessCount = rebellionSuccessCount;
            CopyPatterns(originalMovePatterns, target.originalMovePatterns);
            CopyPatterns(currentMovePatterns, target.currentMovePatterns);
            CopyPatterns(oneTimeMovePatterns, target.oneTimeMovePatterns);
        }

        public void AddOneTimeMovePattern(MovePattern pattern)
        {
            if (pattern == null)
            {
                return;
            }

            oneTimeMovePatterns.Add(pattern);
            RegisterOneTimePatterns();
        }

        public void AddOneTimeMovePatterns(IEnumerable<MovePattern> patterns)
        {
            if (patterns == null)
            {
                return;
            }

            foreach (MovePattern pattern in patterns)
            {
                if (pattern != null)
                {
                    oneTimeMovePatterns.Add(pattern);
                }
            }

            RegisterOneTimePatterns();
        }

        public void ClearOneTimeMovePatterns()
        {
            oneTimeMovePatterns.Clear();
            registeredOneTimePatterns.Remove(GetRegistryKey(color, type, file, rank));
        }

        public int GetEffectiveTax()
        {
            return taxPerTurn * taxModifier;
        }

        public PieceColor GetMovementControllerColor()
        {
            return movementControllerColor == PieceColor.None ? color : movementControllerColor;
        }

        public void ResetTurnModifiers()
        {
            int beforeTaxModifier = taxModifier;
            float beforeRebellionWeight = rebellionWeight;
            taxModifier = 1;
            rebellionWeight = 0f;
            if (beforeTaxModifier != taxModifier)
            {
                QaLog.Write("세금", "세금 배율 초기화 기물=" + QaLog.PieceLabel(this) + " 이전=" + beforeTaxModifier + " 이후=" + taxModifier);
            }

            if (!UnityEngine.Mathf.Approximately(beforeRebellionWeight, rebellionWeight))
            {
                QaLog.Write("기물가중치", "반란 가중치 초기화 기물=" + QaLog.PieceLabel(this) + " 이전=" + beforeRebellionWeight + " 이후=" + rebellionWeight);
            }
        }

        private void RestoreOneTimePatterns()
        {
            string key = GetRegistryKey(color, type, file, rank);
            if (!registeredOneTimePatterns.TryGetValue(key, out List<MovePattern> patterns))
            {
                return;
            }

            for (int i = 0; i < patterns.Count; i++)
            {
                MovePattern pattern = patterns[i];
                oneTimeMovePatterns.Add(new MovePattern(pattern.deltaFile, pattern.deltaRank, pattern.isSliding, pattern.captureOnly, pattern.moveOnly, pattern.isCannon));
            }
        }

        private void RegisterOneTimePatterns()
        {
            string key = GetRegistryKey(color, type, file, rank);
            if (oneTimeMovePatterns.Count == 0)
            {
                registeredOneTimePatterns.Remove(key);
                return;
            }

            List<MovePattern> copiedPatterns = new List<MovePattern>(oneTimeMovePatterns.Count);
            for (int i = 0; i < oneTimeMovePatterns.Count; i++)
            {
                MovePattern pattern = oneTimeMovePatterns[i];
                copiedPatterns.Add(new MovePattern(pattern.deltaFile, pattern.deltaRank, pattern.isSliding, pattern.captureOnly, pattern.moveOnly, pattern.isCannon));
            }

            registeredOneTimePatterns[key] = copiedPatterns;
        }

        private static string GetRegistryKey(PieceColor pieceColor, PieceType pieceType, int pieceFile, int pieceRank)
        {
            return pieceColor + ":" + pieceType + ":" + pieceFile + ":" + pieceRank;
        }

        private void CopyPatterns(List<MovePattern> source, List<MovePattern> target)
        {
            target.Clear();
            for (int i = 0; i < source.Count; i++)
            {
                MovePattern pattern = source[i];
                if (pattern != null)
                {
                    target.Add(new MovePattern(pattern.deltaFile, pattern.deltaRank, pattern.isSliding, pattern.captureOnly, pattern.moveOnly, pattern.isCannon));
                }
            }
        }
    }
}
