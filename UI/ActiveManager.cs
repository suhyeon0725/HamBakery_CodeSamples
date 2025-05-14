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

    // ���� ������ ����
    public mPageInfo CurrentPageInfo;
    public int CurrentPage = 0;

    // ������ ���ο� ��ųʸ�
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

        // ������ ���� ���
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

    // ���� ������ ���� �� UI ����
    public void SetCurrentPage(mPageInfo pageInfo)
    {
        CurrentPageInfo = pageInfo;
        CurrentPage = (int)pageInfo;

        foreach (var canvas in pageMap.Values)
        {
            canvas.SetActive(false); // ��ü ����
        }

        if (pageMap.ContainsKey(CurrentPageInfo))
        {
            pageMap[CurrentPageInfo].SetActive(true); // ���� �������� �ѱ�
        }
    }
}
