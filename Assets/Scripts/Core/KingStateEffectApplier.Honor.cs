using System;
using UnityEngine;

namespace MMBGame
{
    public partial class KingStateEffectApplier
    {
        private void EnsureHonorChangeHandler(PlayerState player, PieceColor color)
        {
            if (honorChangeHandlers.ContainsKey(color))
            {
                return;
            }

            Action<int> handler = delta =>
            {
                if (delta >= 0 || boardManager == null || boardManager.BoardState == null)
                {
                    return;
                }

                if (suppressNextHonorDecreasePenaltyColors.Contains(color))
                {
                    suppressNextHonorDecreasePenaltyColors.Remove(color);
                    return;
                }

                int penalty = Mathf.CeilToInt(Mathf.Abs(delta) / 2f);
                if (penalty <= 0)
                {
                    return;
                }

                ApplyToAllPieces(boardManager.BoardState, color, piece => PoliticalStatService.ChangeSupport(piece, -penalty, "DictatorshipHonorPenalty"));
            };

            honorChangeHandlers[color] = handler;
            player.OnHonorChanged += handler;
        }

        private void RemoveHonorChangeHandler(PieceColor color)
        {
            if (!honorChangeHandlers.TryGetValue(color, out Action<int> handler) || politicsManager == null)
            {
                honorChangeHandlers.Remove(color);
                return;
            }

            PlayerState player = politicsManager.GetCurrentPlayer(color);
            if (player != null)
            {
                player.OnHonorChanged -= handler;
            }

            honorChangeHandlers.Remove(color);
        }
    }
}
