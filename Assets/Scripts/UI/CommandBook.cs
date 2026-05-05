using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 커맨드 북 - 페이지 3개(재정/정치/군사)를 클릭 방향에 따라 넘겨줌.
/// 스프라이트 오른쪽 절반 클릭 → 다음 페이지, 왼쪽 절반 클릭 → 이전 페이지.
/// </summary>
public class CommandBook : MonoBehaviour
{
    public enum PageCategory { Fiscal = 0, Political = 1, Military = 2 }

    [SerializeField] private PageTurnAnimation[] pages; // [0]=재정, [1]=정치, [2]=군사
    [SerializeField] private PageCategory startPage = PageCategory.Fiscal;

    private int _currentIndex;

    public PageCategory CurrentPage => (PageCategory)_currentIndex;

    void Start()
    {
        _currentIndex = (int)startPage;
        for (int i = 0; i < pages.Length; i++)
            pages[i].gameObject.SetActive(i == _currentIndex);
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;

        // 현재 페이지 bounds에서 클릭 위치 확인
        var sr = pages[_currentIndex].GetComponent<SpriteRenderer>();
        var screenPos = mouse.position.ReadValue();
        var worldClick = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        worldClick.z = sr.transform.position.z;

        RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(screenPos));
        if (hit.collider != null && hit.collider.GetComponent("CommandActionButton") != null) return;

        if (!sr.bounds.Contains(worldClick)) return;

        bool goRight = worldClick.x > sr.bounds.center.x;
        if (goRight) TurnPage(+1);
    }

    void TurnPage(int direction)
    {
        int next = (_currentIndex + direction + pages.Length) % pages.Length;
        int prev = _currentIndex;
        _currentIndex = next;
        pages[prev].AnimateOut(false);
        pages[_currentIndex].AnimateIn(false);
    }
}
