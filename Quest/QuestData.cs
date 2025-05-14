[CreateAssetMenu(fileName = "QuestData", menuName = "Game/QuestData")]
public class QuestData : ScriptableObject
{
    public string questID;                   // 퀘스트 고유 ID
    public QuestType questType;              // 메인 / 랜덤 여부
    public QuestCategory questCategory;      // 퀘스트 분류

    public string questName;                 // 이름
    public string questtDescription;         // 설명

    public int questGoal;                    // 목표 수치

    public RewardMethod rewardMethod;        // 보상 방식 
    public int rewardAmount;                 // 보상 수치

    public bool isCompleted;                 // 완료 여부
}
