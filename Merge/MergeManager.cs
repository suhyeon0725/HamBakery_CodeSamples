public class MergeManager : MonoBehaviour
{
    private static MergeManager _instance;
    public static MergeManager Instance { get { return _instance; } }

    [Header("Bakery Slots")]
    public GameObject[] BakerySlot; // ����Ŀ�� ����
    List<GameObject> UnlockedSlot; // ���µ� ����
    List<GameObject> LockedSlot; // ��ݵ� ����

    [Header("Slot Sprites")]
    public Sprite unlockedSlotImg; // �����ִ� ���� �̹��� (��Ȳ)
    public Sprite lockedSlotImg; // ��ݵ� ���� �̹��� (ȸ��)

    [Header("Bakery Prefabs")]
    public GameObject DoughPrefab; // ���� ������ ���� ����
    public GameObject[] BreadPrefabs; // �� ������ ���� ����
    public GameObject[] CookiePrefabs; // ��Ű ������ ���� ����
    public GameObject[] CakePrefabs; // ����ũ ������ ���� ����

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

        UnlockedSlot = new List<GameObject>();
        LockedSlot = new List<GameObject>();
    }

    private void Start()
    {
        LoadBakerySlotData();
        LoadMergeSlotData();
    }

    public void SaveBakerySlotNumData()
    {
        ES3.Save(SaveDataManager.Instance.BakerySlotNum, currentButtonIndex);
    }

    public void LoadBakerySlotData()
    {
        if (ES3.KeyExists(SaveDataManager.Instance.BakerySlotNum))
        {
            currentButtonIndex = ES3.Load<int>(SaveDataManager.Instance.BakerySlotNum);
        }
        else
        {
            currentButtonIndex = 0;
        }
    }

    public void SaveMergeSlotData()
    {
        SlotSaveDataList saveDataList = new SlotSaveDataList
        {
            slots = new List<SlotSaveData>()
        };

        foreach (GameObject slot in BakerySlot)
        {
            SlotState slotState = slot.GetComponent<SlotState>();

            SlotSaveData slotSaveData = new SlotSaveData
            {
                isUnLocked = slotState.GetIsUnLocked(),
                isFull = slotState.GetIsFull()
            };

            if (slotState.GetIsFull())
            {
                BreadPreScript bakeryScript = slotState.bakery.GetComponent<BreadPreScript>();
                slotSaveData.bakeryName = bakeryScript.bakeryName;
                slotSaveData.bakeryLevel = bakeryScript.level;
                slotSaveData.bakeryType = bakeryScript.bakeryType;
            }
            else
            {
                slotSaveData.bakeryName = null;
                slotSaveData.bakeryLevel = 0;
                slotSaveData.bakeryType = BakeryType.dough; // �⺻�� ����
            }

            saveDataList.slots.Add(slotSaveData);
        }

        ES3.Save(SaveDataManager.Instance.MergeSlotData, saveDataList);
    }

    public void LoadMergeSlotData()
    {
        if (ES3.KeyExists(SaveDataManager.Instance.MergeSlotData))
        {
            // SlotSaveDataList�� �ҷ�����
            SlotSaveDataList slotDataListWrapper = ES3.Load<SlotSaveDataList>(SaveDataManager.Instance.MergeSlotData);

            if (slotDataListWrapper != null && slotDataListWrapper.slots != null)
            {
                List<SlotSaveData> slotDataList = slotDataListWrapper.slots;

                for (int i = 0; i < Mathf.Min(BakerySlot.Length, slotDataList.Count); i++)
                {
                    SlotState slotState = BakerySlot[i].GetComponent<SlotState>();
                    SlotSaveData slotSaveData = slotDataList[i];

                    slotState.isUnLocked = slotSaveData.isUnLocked;
                    slotState.isFull = slotSaveData.isFull;

                    if (slotState.isFull)
                    {
                        GameObject bakeryPrefab = null;
                        switch (slotSaveData.bakeryType)
                        {
                            case BakeryType.dough:
                                bakeryPrefab = DoughPrefab;
                                break;
                            case BakeryType.bread:
                                if (slotSaveData.bakeryLevel > 0 && slotSaveData.bakeryLevel <= BreadPrefabs.Length)
                                    bakeryPrefab = BreadPrefabs[slotSaveData.bakeryLevel - 1];
                                break;
                            case BakeryType.cookie:
                                if (slotSaveData.bakeryLevel > 0 && slotSaveData.bakeryLevel <= CookiePrefabs.Length)
                                    bakeryPrefab = CookiePrefabs[slotSaveData.bakeryLevel - 1];
                                break;
                            case BakeryType.cake:
                                if (slotSaveData.bakeryLevel > 0 && slotSaveData.bakeryLevel <= CakePrefabs.Length)
                                    bakeryPrefab = CakePrefabs[slotSaveData.bakeryLevel - 1];
                                break;
                        }

                        if (bakeryPrefab != null)
                        {
                            GameObject bakery = Instantiate(bakeryPrefab);
                            slotState.AddBakery(bakery);
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("�ε�� MergeSlotData�� ��� �ֽ��ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning("����� MergeSlotData�� �����ϴ�.");
        }

        CheckSlot();
    }

    void CheckSlot()
    {
        UnlockedSlot.Clear();
        LockedSlot.Clear();
        for(int i = 0; i < BakerySlot.Length; i++)
        {
            if(BakerySlot[i].GetComponent<SlotState>().GetIsUnLocked() == true)
            {
                UnlockedSlot.Add(BakerySlot[i]);
                BakerySlot[i].GetComponent<SpriteRenderer>().sprite = unlockedSlotImg;
            }
            else
            {
                LockedSlot.Add(BakerySlot[i]);
                BakerySlot[i].GetComponent<SpriteRenderer>().sprite = lockedSlotImg;
            }
        }
    }

#region --------------------------------- �� ����� --------------------------------------
    public void BakeryCreateButton() // ���� ��ư Ŭ��
    {
        bool allSlotFull = true;
        for(int i = 0; i < UnlockedSlot.Count; i++)
        {
            if(UnlockedSlot[i].GetComponent<SlotState>().GetIsFull() == false)
            {
                DoughCreate(i);
                SoundManager.Instance.PlaySFX("UIClick");
                allSlotFull = false;
                break;
            }
        }

        // �� �ڸ��� �ִ°�
        if (allSlotFull) 
        {
            Debug.Log("���ڸ��� �����");
        }
    }

    void DoughCreate(int n) // �����
    {
        if (EnergyManager.Instance.GetCurrentEnergy() <= 0)
        {
            StartCoroutine(PopupManager.Instance.AlertPopup("alert_energy"));
            Debug.Log("������ ����");
            return;
        }

        GameObject bakery = MonoBehaviour.Instantiate(DoughPrefab) as GameObject;
        
        UnlockedSlot[n].GetComponent<SlotState>().AddBakery(bakery);

        EnergyManager.Instance.DecreaseEnergy(1);

        if (bakery != null)
            CheckNewBakery(bakery);

    }

    void CheckNewBakery(GameObject bakery)
    {
        // ����Ŀ�� ���� �������� (Ÿ�԰� ����)
        var bakeryScript = bakery.GetComponent<BreadPreScript>();
        BakeryType type = bakeryScript.bakeryType;
        int levelIndex = bakeryScript.level - 1;

        // ����Ŀ�� �����ͺ��̽����� �ش� ������ ��������
        GameObject targetItem = null;

        if (type == BakeryType.bread)
        {
            targetItem = BakeryManager.Instance.bakeryDatabase.breadItem[levelIndex];
        }
        else if (type == BakeryType.cookie)
        {
            targetItem = BakeryManager.Instance.bakeryDatabase.cookieItem[levelIndex];
        }
        else if (type == BakeryType.cake)
        {
            targetItem = BakeryManager.Instance.bakeryDatabase.cakeItem[levelIndex];
        }

        // �������� ������ ���, �ر� ���� Ȯ�� �� ó��
        if (targetItem != null)
        {
            var behavior = targetItem.GetComponent<BakeryBehavior>();

            if (!behavior.unlocked)
            {
                // �ű� ����Ŀ�� �ر� ó��
                behavior.unlocked = true;
                behavior.bakeryStar = 1;

                // �˾� ȣ��
                StartCoroutine(PopupManager.Instance.GetNewBakery(targetItem));

                // ����ġ ���� �� ������ üũ
                LevelManager.Instance.IncreasePlayerExp(5);
                PlayerStatsManager.Instance.CheckRecipeCount();
            }
        }
    }

    public GameObject MergeCreateBakery(BreadPreScript incomingBakery)
    {
        GameObject bakery = null;

        switch (incomingBakery.bakeryType)
        {
            case BakeryType.dough:
                bakery = Instantiate(BakeryCreate());
                break;
            case BakeryType.bread:
                bakery = Instantiate(BreadPrefabs[incomingBakery.level]);
                break;
            case BakeryType.cookie:
                bakery = Instantiate(CookiePrefabs[incomingBakery.level]);
                break;
            case BakeryType.cake:
                bakery = Instantiate(CakePrefabs[incomingBakery.level]);
                break;
        }

        CheckNewBakery(bakery);
        return bakery;
    }

    GameObject BakeryCreate()
    {
        int randomIndex = Random.Range(0, 3); // 0: bread, 1: cookie, 2: cake
        int level;
        GameObject bakery = null;

        switch (randomIndex)
        {
            case 0:
                level = MergeUpgradeManager.Instance.SetBreadCreateNum();
                bakery = BreadPrefabs[level - 1];
                break;
            case 1:
                level = MergeUpgradeManager.Instance.SetCookieCreateNum();
                bakery = CookiePrefabs[level - 1];
                break;
            case 2:
                level = MergeUpgradeManager.Instance.SetCakeCreateNum();
                bakery = CakePrefabs[level - 1];
                break;
        }

        return bakery;
    }

    public void BreadDelete(GameObject gameObject)
    {
        Destroy(gameObject);
    }
    #endregion
}
