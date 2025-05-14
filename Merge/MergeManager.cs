public class MergeManager : MonoBehaviour
{
    private static MergeManager _instance;
    public static MergeManager Instance { get { return _instance; } }

    [Header("Bakery Slots")]
    public GameObject[] BakerySlot; // 베이커리 슬롯
    List<GameObject> UnlockedSlot; // 오픈된 슬롯
    List<GameObject> LockedSlot; // 잠금된 슬롯

    [Header("Slot Sprites")]
    public Sprite unlockedSlotImg; // 열려있는 슬롯 이미지 (주황)
    public Sprite lockedSlotImg; // 잠금된 슬롯 이미지 (회색)

    [Header("Bakery Prefabs")]
    public GameObject DoughPrefab; // 반죽 프리펩 정보 저장
    public GameObject[] BreadPrefabs; // 빵 프리펩 정보 저장
    public GameObject[] CookiePrefabs; // 쿠키 프리펩 정보 저장
    public GameObject[] CakePrefabs; // 케이크 프리펩 정보 저장

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
                slotSaveData.bakeryType = BakeryType.dough; // 기본값 설정
            }

            saveDataList.slots.Add(slotSaveData);
        }

        ES3.Save(SaveDataManager.Instance.MergeSlotData, saveDataList);
    }

    public void LoadMergeSlotData()
    {
        if (ES3.KeyExists(SaveDataManager.Instance.MergeSlotData))
        {
            // SlotSaveDataList로 불러오기
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
                Debug.LogWarning("로드된 MergeSlotData가 비어 있습니다.");
            }
        }
        else
        {
            Debug.LogWarning("저장된 MergeSlotData가 없습니다.");
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

#region --------------------------------- 빵 만들기 --------------------------------------
    public void BakeryCreateButton() // 제작 버튼 클릭
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

        // 빈 자리가 있는가
        if (allSlotFull) 
        {
            Debug.Log("빈자리가 없어요");
        }
    }

    void DoughCreate(int n) // 만들기
    {
        if (EnergyManager.Instance.GetCurrentEnergy() <= 0)
        {
            StartCoroutine(PopupManager.Instance.AlertPopup("alert_energy"));
            Debug.Log("에너지 부족");
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
        // 베이커리 정보 가져오기 (타입과 레벨)
        var bakeryScript = bakery.GetComponent<BreadPreScript>();
        BakeryType type = bakeryScript.bakeryType;
        int levelIndex = bakeryScript.level - 1;

        // 베이커리 데이터베이스에서 해당 아이템 가져오기
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

        // 아이템이 존재할 경우, 해금 여부 확인 후 처리
        if (targetItem != null)
        {
            var behavior = targetItem.GetComponent<BakeryBehavior>();

            if (!behavior.unlocked)
            {
                // 신규 베이커리 해금 처리
                behavior.unlocked = true;
                behavior.bakeryStar = 1;

                // 팝업 호출
                StartCoroutine(PopupManager.Instance.GetNewBakery(targetItem));

                // 경험치 지급 및 레시피 체크
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
