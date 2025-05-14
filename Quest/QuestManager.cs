public class QuestManager : MonoBehaviour
{
    private static QuestManager _instance;
    public static QuestManager Instance { get { return _instance; } }

    public QuestDatabase questDatabase; // ����Ʈ �����ͺ��̽�
    public QuestData currentQuest;      // ���� ����Ʈ ������
    public int currentQuestCount;   // ���� ����Ʈ ��ǥ���� ���� ��
    public int mainQuestProgress;   // ���� ���� ����Ʈ ���� ��Ȳ (Index)

    private QuestData previousQuest; // ���� ����Ʈ�� ����

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
        if (currentQuest == null)
            currentQuest = GetRandomQuest();

        CheckQuests();
    }

    public void CheckQuests()
    {
        switch (currentQuest.questCategory)
        {
            case QuestCategory.Recipe:
                currentQuestCount = PlayerStatsManager.Instance.TotalGetRecipes;
                break;
            case QuestCategory.Hiring:
                currentQuestCount = PlayerStatsManager.Instance.CheckHireHamster();
                break;
            case QuestCategory.LevelUp:
                currentQuestCount = LevelManager.Instance.GetPlayerLV();
                break;
        }

        if (currentQuestCount >= currentQuest.questGoal)
            CompleteQuest();
    }

    public void IncreaseQuestCount(int amount = 1)
    {
        currentQuestCount += amount;
        CheckQuests();
    }

    public void CompleteQuest()
    {
        currentQuest.isCompleted = true;
    }

    public void ClickCompleteButton()
    {
        RewardPlayer(currentQuest.rewardMethod, currentQuest.rewardAmount);
        currentQuestCount = 0;
        mainQuestProgress++;
        LevelManager.Instance.IncreasePlayerExp(5);

        if(currentQuest.questType == QuestType.Random)
        {
            currentQuest.isCompleted = false;
        }

        NextQuest();
    }

    public void NextQuest()
    {
        previousQuest = currentQuest;

        if (mainQuestProgress < questDatabase.mainQuest.Count)
            currentQuest = questDatabase.mainQuest[mainQuestProgress];
        else
            currentQuest = GetRandomQuest();

        CheckQuests();
    }

    // ���� ����Ʈ�� ��ġ�� �ʴ� ���� ����Ʈ�� ����
    private QuestData GetNonRepeatingRandomQuest()
    {
        var list = questDatabase.randomQuest;
        if (list.Count == 1) return list[0];

        QuestData selected;
        int safetyCounter = 0;
        do
        {
            selected = list[Random.Range(0, list.Count)];
            safetyCounter++;
        } while (selected == previousQuest && safetyCounter < 10);

        return selected;
    }

    public void RewardPlayer(RewardMethod questMethod, int reward)
    {
        switch (questMethod)
        {
            case RewardMethod.Gold:
                CurrencyManager.Instance.IncreaseGold(reward);
                break;
            case RewardMethod.Gems:
                CurrencyManager.Instance.IncreaseGems(reward);
                break;
            case RewardMethod.Gacha:
                GachaManager.Instance.IncreaseBasicCoupon(reward);
                break;
        }
    }
}
