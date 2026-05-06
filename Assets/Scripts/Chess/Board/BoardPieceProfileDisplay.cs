using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MMBGame
{
    public class BoardPieceProfileDisplay : MonoBehaviour
    {
        private const string PROFILE_PATH = "Assets/Prefabs/Profile/";
        private const int SORTING_ORDER = 40;

        private Transform profileTarget;
        private SpriteRenderer profileImageRenderer;
        private TextMesh nameText;
        private TextMesh goldText;
        private TextMesh supportText;
        private TextMesh moveText;

        public void Show(BoardManager boardManager, BoardPieceVisual visual)
        {
            if (boardManager == null || boardManager.BoardState == null || visual == null)
            {
                Clear();
                return;
            }

            ChessPiece piece = boardManager.BoardState.GetPiece(visual.File, visual.Rank);
            if (piece == null)
            {
                Clear();
                return;
            }

            EnsureBindings();
            SetProfile(piece);
            SetText(nameText, GetDisplayName(piece));
            SetText(goldText, piece.GetEffectiveTax().ToString());
            SetText(supportText, piece.support.ToString());
            SetText(moveText, GetMoveText(piece));
        }

        public void Clear()
        {
            EnsureBindings();
            if (profileImageRenderer != null)
            {
                profileImageRenderer.sprite = null;
            }

            SetText(nameText, string.Empty);
            SetText(goldText, string.Empty);
            SetText(supportText, string.Empty);
            SetText(moveText, string.Empty);
        }

        private void EnsureBindings()
        {
            profileTarget = EnsureTransform("Profile_Position");
            profileImageRenderer = EnsureProfileImageRenderer(profileTarget);
            nameText = EnsureText("Name_Position", nameText);
            goldText = EnsureText("GoldperTurn_Position", goldText);
            supportText = EnsureText("Support_Position", supportText);
            moveText = EnsureText("PieceMove_Position", moveText);
        }

        private Transform EnsureTransform(string objectName)
        {
            GameObject target = GameObject.Find(objectName);
            if (target == null)
            {
                return null;
            }

            return target.transform;
        }

        private SpriteRenderer EnsureProfileImageRenderer(Transform target)
        {
            if (target == null)
            {
                return null;
            }

            Transform existing = target.Find("ProfileImage");
            GameObject imageObject = existing != null ? existing.gameObject : new GameObject("ProfileImage");
            imageObject.transform.SetParent(target, false);
            imageObject.transform.localPosition = new Vector3(0f, 0f, -0.1f);
            SpriteRenderer renderer = imageObject.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = imageObject.AddComponent<SpriteRenderer>();
            }

            renderer.sortingOrder = SORTING_ORDER;
            return renderer;
        }

        private TextMesh EnsureText(string objectName, TextMesh currentText)
        {
            if (currentText != null)
            {
                FitTextScale(currentText.transform);
                return currentText;
            }

            GameObject target = GameObject.Find(objectName);
            if (target == null)
            {
                return null;
            }

            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(target.transform, false);
            textObject.transform.localPosition = new Vector3(0f, 0f, -0.1f);
            FitTextScale(textObject.transform);
            TextMesh textMesh = textObject.AddComponent<TextMesh>();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 40;
            textMesh.characterSize = 0.18f;
            textMesh.color = new Color(0.1f, 0.08f, 0.05f, 1f);
            MeshRenderer renderer = textObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = SORTING_ORDER + 1;
            }

            return textMesh;
        }

        private void FitTextScale(Transform textTransform)
        {
            if (textTransform == null || textTransform.parent == null)
            {
                return;
            }

            Vector3 parentScale = textTransform.parent.lossyScale;
            textTransform.localScale = new Vector3(GetInverseScale(parentScale.x), GetInverseScale(parentScale.y), 1f);
        }

        private float GetInverseScale(float scale)
        {
            return Mathf.Abs(scale) > 0.001f ? 1f / scale : 1f;
        }

        private void SetProfile(ChessPiece piece)
        {
            if (profileImageRenderer == null)
            {
                return;
            }

            Sprite sprite = LoadProfileSprite(GetProfileAssetPath(piece));
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

        private string GetDisplayName(ChessPiece piece)
        {
            return GetColorName(piece.color) + " " + GetSideName(piece.side) + " " + GetTypeName(piece.type);
        }

        private string GetColorName(PieceColor color)
        {
            if (color == PieceColor.White)
            {
                return "백";
            }

            return color == PieceColor.Black ? "흑" : "무색";
        }

        private string GetSideName(PieceSide side)
        {
            if (side == PieceSide.Kingside)
            {
                return "킹사이드";
            }

            return side == PieceSide.Queenside ? "퀸사이드" : string.Empty;
        }

        private string GetTypeName(PieceType type)
        {
            switch (type)
            {
                case PieceType.Pawn: return "폰";
                case PieceType.Rook: return "룩";
                case PieceType.Knight: return "나이트";
                case PieceType.Bishop: return "비숍";
                case PieceType.Queen: return "퀸";
                case PieceType.King: return "킹";
                case PieceType.Barricade: return "바리케이드";
                case PieceType.Trebuchet: return "트레뷰셋";
                default: return type.ToString();
            }
        }

        private string GetMoveText(ChessPiece piece)
        {
            return GetTypeName(piece.type);
        }

        private void SetText(TextMesh textMesh, string value)
        {
            if (textMesh != null)
            {
                textMesh.text = value;
            }
        }
    }
}
