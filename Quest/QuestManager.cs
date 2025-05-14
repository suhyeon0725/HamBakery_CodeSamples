public class QuestManager : MonoBehaviour
{
    private static QuestManager _instance;
    public static QuestManager Instance { get { return _instance; } }

    public QuestDatabase questDatabase; // 퀘스트 데이터베이스
    public QuestData currentQuest;      // 현재 퀘스트 데이터
    public int currentQuestCount;   // 현재 퀘스트 목표까지 남은 양
    public int mainQuestProgress;   // 현재 메인 퀘스트 진행 상황 (Index)

    private QuestData previousQuest; // 이전 퀘스트를 저장

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

    // 이전 퀘스트와 겹치지 않는 랜덤 퀘스트를 선택
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
