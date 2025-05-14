public class CustomerManager : MonoBehaviour
{
    private static CustomerManager _instance;
    public static CustomerManager Instance => _instance;

    [Header("Spawn Settings")]
    private const float SpawnDelayMin = 2f;
    private const float SpawnDelayMax = 7f;

    [Header("Customer Data")]
    public CustomerDatabase customerDatabase;
    public GameObject customerParent;

    // 스폰 위치
    private const float SpawnXLeft = -7.5f;
    private const float SpawnXRight = 22f;
    private const float SpawnYMin = -12.5f;
    private const float SpawnYMax = -9.5f;

    public const float WalkSpeed = 3f;
    public const float ExitXMin = -8f;
    public const float ExitXMax = 23f;

    public const float Eat_Time = 5f;

    private void Awake()
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
    }

    private void Start()
    {
        LoadCustomerData();
        LoadTMIData();

        InvokeRepeating(nameof(SpawnRandomCustomer), 0f, Random.Range(SpawnDelayMin, SpawnDelayMax));
    }

    private void LoadTMIData()
    {
        var asset = Resources.Load<TextAsset>("Json/CustomerTMIData");
        if (asset == null) return;

        var data = JsonUtility.FromJson<CustomerTMIData>(asset.text);
        if (data == null) return;

        foreach (var entry in data.customers)
        {
            var customer = customerDatabase.customers.Find(obj => obj.GetComponent<CustomerBehavier>().customerID == entry.id);
            if (customer != null)
                customer.GetComponent<CustomerBehavier>().customerTMI = entry.tmis;
        }
    }

    private void SpawnRandomCustomer()
    {
        if (InteriorManager.Instance.itemMoveMode) return;

        var openList = customerDatabase.customers.FindAll(c => c.GetComponent<CustomerBehavier>().customerOpen);
        if (openList.Count == 0) return;

        var prefab = openList[Random.Range(0, openList.Count)];
        float spawnX = Random.Range(0, 2) == 0 ? SpawnXLeft : SpawnXRight;
        float spawnY = Random.Range(SpawnYMin, SpawnYMax);

        var instance = Instantiate(prefab, new Vector3(spawnX, spawnY, 0f), Quaternion.identity);

        if (customerParent != null)
            instance.transform.parent = customerParent.transform;
    }

}