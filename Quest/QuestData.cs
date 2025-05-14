[CreateAssetMenu(fileName = "QuestData", menuName = "Game/QuestData")]
public class QuestData : ScriptableObject
{
    public string questID;                   // ����Ʈ ���� ID
    public QuestType questType;              // ���� / ���� ����
    public QuestCategory questCategory;      // ����Ʈ �з�

    public string questName;                 // �̸�
    public string questtDescription;         // ����

    public int questGoal;                    // ��ǥ ��ġ

    public RewardMethod rewardMethod;        // ���� ��� 
    public int rewardAmount;                 // ���� ��ġ

    public bool isCompleted;                 // �Ϸ� ����
}
