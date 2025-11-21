using UnityEngine;

// 기능은 없고 데이터(이름, 설명, 가격, 이미지)만 담는 공용 카드 클래스
public class Card_Item : Card
{
    public Card_Item(object owner, int index, string nameKey, string descKey, string iconPath, int price)
        : base(owner, index, 0f) // 
    {
        this.CardNameKey = nameKey;
        this.CardSkillDescriptionKey = descKey;
        this.CardPrice = price;
        this.CardImage = Resources.Load<Sprite>(iconPath);

        if (this.CardImage == null)
        {
            Debug.LogWarning($"[Card_Item] 이미지를 찾을 수 없습니다 (Sprite): {iconPath}");
        }
    }

    // 아이템(재료 등)은 스킬을 발동하지 않음
    public override void ExecuteSkill()
    {
        // Do Nothing
    }
}