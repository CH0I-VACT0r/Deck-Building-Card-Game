using UnityEngine;

public class Card_Sheep : Card
{
    public Card_Sheep(object owner, int index) : base(owner, index, 99f) // 쿨타임 길게 (스킬 못 쓰게)
    {
        this.CardNameKey = "card_sheep_name";
        this.CardImage = Resources.Load<Sprite>("CardImages/Monster/Sheep"); // (이미지 필요)
        this.CardPrice = 0;
        this.Rarity = CardRarity.Bronze;

        this.TagKeys.Add("tag_beast");
        this.FlavorTextKey = "card_sheep_desc";

        // 약한 스탯
        this.BaseDamage = 1;
        this.BaseCooldownTime = 99f; // 사실상 공격 불가
    }

    public override void ExecuteSkill()
    {
        // 양은 아무것도 하지 않습니다 (매에에~)
        Debug.Log("[양] 매에에~");
    }
}