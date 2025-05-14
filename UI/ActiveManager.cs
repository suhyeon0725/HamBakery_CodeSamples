public class ActiveManager : MonoBehaviour
{
    private static ActiveManager _instance;
    public static ActiveManager Instance { get { return _instance; } }

    public GameObject MainCanvas;
    public GameObject MergeCanvas;
    public GameObject KitchenCanvas;

    public GameObject IndexCanvas;
    public GameObject QuestCanvas;
    public GameObject ShopCanvas;
    public GameObject SettingsCanvas;

    // 현재 페이지 정보
    public mPageInfo CurrentPageInfo;
    public int CurrentPage = 0;

    // 페이지 매핑용 딕셔너리
    private Dictionary<mPageInfo, GameObject> pageMap;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 페이지 정보 등록
        pageMap = new Dictionary<mPageInfo, GameObject>
        {
            { mPageInfo.Main, MainCanvas },
            { mPageInfo.Merge, MergeCanvas },
            { mPageInfo.Kitchen, KitchenCanvas },
            { mPageInfo.Index, IndexCanvas },
            { mPageInfo.Quest, QuestCanvas },
            { mPageInfo.Shop, ShopCanvas },
            { mPageInfo.Settings, SettingsCanvas }
        };
    }

    // 현재 페이지 설정 및 UI 갱신
    public void SetCurrentPage(mPageInfo pageInfo)
    {
        CurrentPageInfo = pageInfo;
        CurrentPage = (int)pageInfo;

        foreach (var canvas in pageMap.Values)
        {
            canvas.SetActive(false); // 전체 끄기
        }

        if (pageMap.ContainsKey(CurrentPageInfo))
        {
            pageMap[CurrentPageInfo].SetActive(true); // 현재 페이지만 켜기
        }
    }
}
