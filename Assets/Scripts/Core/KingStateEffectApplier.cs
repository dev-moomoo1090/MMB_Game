using System;
using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public partial class KingStateEffectApplier : MonoBehaviour
    {
        private const int INCOMPETENT_SURVIVAL_TURNS = 20;
        private const float INCOMPETENT_INITIAL_TAX_RATE = 0.7f;
        private const int INCOMPETENT_INITIAL_ACCEPTANCE_PENALTY = 30;
        private const int INCOMPETENT_INITIAL_SUPPORT_PENALTY = 10;
        private const float INCOMPETENT_TURN_TAX_RATE = 1.05f;
        private const int INCOMPETENT_TURN_ACCEPTANCE_GAIN = 5;
        private const int INCOMPETENT_TURN_SUPPORT_GAIN = 1;
        private const float INCOMPETENT_FIXED_TAX_RATE = 3f;
        private const int INCOMPETENT_FIXED_ACCEPTANCE_GAIN = 100;
        private const int INCOMPETENT_FIXED_SUPPORT_GAIN = 5;

        private static KingStateEffectApplier instance;

        private readonly Dictionary<PieceColor, int> incompetentTurnCounts = new Dictionary<PieceColor, int>();
        private readonly HashSet<PieceColor> incompetentPenaltyAppliedColors = new HashSet<PieceColor>();
        private readonly Dictionary<PieceColor, Action<int>> honorChangeHandlers = new Dictionary<PieceColor, Action<int>>();
        private readonly HashSet<PieceColor> suppressNextHonorDecreasePenaltyColors = new HashSet<PieceColor>();

        private BoardManager boardManager;
        private PoliticsManager politicsManager;

        public static KingStateEffectApplier Instance => instance;

        public int GetIncompetentTurnsElapsed(PieceColor color)
        {
            return incompetentTurnCounts.TryGetValue(color, out int count) ? count : 0;
        }

        public int GetIncompetentTurnsRemaining(PieceColor color)
        {
            return Mathf.Max(0, INCOMPETENT_SURVIVAL_TURNS - GetIncompetentTurnsElapsed(color));
        }

        public int GetIncompetentSurvivalTurns()
        {
            return INCOMPETENT_SURVIVAL_TURNS;
        }

        public void Initialize()
        {
            instance = this;
            boardManager = SceneComponentResolver.Resolve<BoardManager>();
            politicsManager = SceneComponentResolver.Resolve<PoliticsManager>();
            incompetentTurnCounts[PieceColor.White] = 0;
            incompetentTurnCounts[PieceColor.Black] = 0;
            incompetentPenaltyAppliedColors.Clear();
            suppressNextHonorDecreasePenaltyColors.Clear();
            KingStateEvaluator.ResetRuntimeState();

            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
            EventBus.Instance.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDestroy()
        {
            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
            RemoveHonorChangeHandler(PieceColor.White);
            RemoveHonorChangeHandler(PieceColor.Black);
            if (instance == this)
            {
                instance = null;
            }
        }

        public void SuppressNextHonorDecreasePenalty(PieceColor color)
        {
            suppressNextHonorDecreasePenaltyColors.Add(color);
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.PoliticsPhase)
            {
                return;
            }

            EnsureReferences();
            if (boardManager == null || boardManager.BoardState == null || politicsManager == null)
            {
                return;
            }

            PieceColor color = boardManager.BoardState.currentTurn;
            PlayerState player = politicsManager.GetCurrentPlayer(color);
            if (player == null)
            {
                return;
            }

            KingState state = KingStateEvaluator.Evaluate(player, boardManager.BoardState);
            KingStateEvaluator.SetCurrentState(color, state);
            ApplyKingStateEffect(state, player, color);
        }

        private void ApplyKingStateEffect(KingState state, PlayerState player, PieceColor color)
        {
            BoardState board = boardManager.BoardState;

            if (state == KingState.Sage)
            {
                incompetentTurnCounts[color] = 0;
                RemoveHonorChangeHandler(color);
                ApplyBenevolentEffect(board, player, color);
                return;
            }

            if (state == KingState.DarkKing)
            {
                RemoveHonorChangeHandler(color);
                ApplyIncompetentEffect(board, color);
                return;
            }

            incompetentTurnCounts[color] = 0;

            if (state == KingState.Autocrat)
            {
                EnsureHonorChangeHandler(player, color);
                return;
            }

            RemoveHonorChangeHandler(color);
        }
    }
}
