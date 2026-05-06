using UnityEngine;
using UnityEditor;

public class BuildChessBoard
{
    public static void Execute()
    {
        const float halfW = 1.17f;
        const float halfH = 0.59f;

        string whitePrefabPath = "Assets/Art/White_tile.prefab";
        string blackPrefabPath = "Assets/Art/Black_tile.prefab";

        GameObject whitePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(whitePrefabPath);
        GameObject blackPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(blackPrefabPath);

        if (whitePrefab == null || blackPrefab == null)
        {
            Debug.LogError("BuildChessBoard: prefab not found.");
            return;
        }

        GameObject boardRoot = GameObject.Find("ChessBoard");
        if (boardRoot == null)
        {
            boardRoot = new GameObject("ChessBoard");
        }

        // 기존 자식 제거
        while (boardRoot.transform.childCount > 0)
            GameObject.DestroyImmediate(boardRoot.transform.GetChild(0).gameObject);

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                float x = (file - rank) * halfW;
                float y = -(file + rank) * halfH;
                int logicalFile = 7 - file;
                int logicalRank = 7 - rank;

                bool isWhite = (logicalFile + logicalRank) % 2 == 0;
                GameObject prefab = isWhite ? whitePrefab : blackPrefab;

                GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(prefab, boardRoot.transform);
                tile.name = MMBGame.BoardCoordinateMapper.GetTileName(logicalFile, logicalRank);
                tile.transform.localPosition = new Vector3(x, y, 0f);
            }
        }

        EditorUtility.SetDirty(boardRoot);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(boardRoot.scene);
        Debug.Log("ChessBoard built: 64 tiles placed.");
    }
}
