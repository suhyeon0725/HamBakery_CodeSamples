public enum QuestType
{
    Main,
    Random
}

public enum QuestCategory
{
    Recipe, Touch, Baking, Selling, Serving,
    Tips, SpecialCustomer, Delivery, Gacha,
    Hiring, Advertising, LevelUp
}

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Game/QuestDatabase")]
public class QuestDatabase : ScriptableObject
{
    public List<QuestData> mainQuest;        // ¸ÞÀÎ Äù½ºÆ® ¸ñ·Ï
    public List<QuestData> randomQuest;      // ·£´ý Äù½ºÆ® ¸ñ·Ï
}

