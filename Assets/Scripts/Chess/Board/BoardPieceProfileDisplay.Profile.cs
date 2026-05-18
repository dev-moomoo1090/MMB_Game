using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MMBGame
{
    public partial class BoardPieceProfileDisplay
    {
        private void SetProfile(ChessPiece piece)
        {
            if (profileImageRenderer == null)
            {
                return;
            }

            Sprite sprite = LoadProfileSprite(GetProfileAssetPath(piece));
            if (sprite == null)
            {
                sprite = LoadProfileSprite(GetSideProfileAssetPath(piece));
            }

            if (sprite == null)
            {
                sprite = LoadProfileSprite(GetFallbackProfileAssetPath(piece.side));
            }

            profileImageRenderer.sprite = sprite;
            FitProfileImage();
        }

        private void FitProfileImage()
        {
            if (profileTarget == null || profileImageRenderer == null || profileImageRenderer.sprite == null)
            {
                return;
            }

            Bounds bounds = profileImageRenderer.sprite.bounds;
            if (bounds.size.x <= 0f || bounds.size.y <= 0f)
            {
                return;
            }

            Vector3 parentScale = profileTarget.lossyScale;
            float targetWidth = Mathf.Max(0.1f, parentScale.x);
            float targetHeight = Mathf.Max(0.1f, parentScale.y);
            float scale = Mathf.Min(targetWidth / bounds.size.x / parentScale.x, targetHeight / bounds.size.y / parentScale.y);
            profileImageRenderer.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private Sprite LoadProfileSprite(string path)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
#else
            return null;
#endif
        }

        private string GetProfileAssetPath(ChessPiece piece)
        {
            string laneName = piece.lane == PieceLane.None ? string.Empty : "_" + piece.lane;
            return PROFILE_PATH + piece.color + "_" + piece.type + laneName + "_Profile.png";
        }

        private string GetSideProfileAssetPath(ChessPiece piece)
        {
            string sideName = piece.side == PieceSide.None ? string.Empty : "_" + piece.side;
            return PROFILE_PATH + piece.color + "_" + piece.type + sideName + "_Profile.png";
        }

        private string GetFallbackProfileAssetPath(PieceSide side)
        {
            if (side == PieceSide.Kingside)
            {
                return PROFILE_PATH + "Black_Pawn_Kingside_Profile.png";
            }

            return PROFILE_PATH + "Black_Pawn_Queenside_Profile.png";
        }
    }
}
