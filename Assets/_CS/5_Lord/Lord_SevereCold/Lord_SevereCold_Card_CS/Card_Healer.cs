// 파일명: Card_Healer.cs
using UnityEngine;

// [힐러 테스트용 카드] 12초마다 영주 체력을 20 즉시 회복시킵니다.
public class Card_Healer : Card // Card 뼈대를 상속
{
    // --- 1. 생성자 ---
    public Card_Healer(PlayerController owner, int index)
        : base(owner, index, 12f) // 12초 쿨타임
    {
        this.CardName = "부족 치유사";
        this.Rarity = CardRarity.Bronze;

        // 20 힐
        this.BaseHeal = 20f;

        this.CardImage = Resources.Load<Sprite>("CardImages/Lord_SevereCold/Healer"); 

        this.Tags.Add("#야만전사");
        this.Tags.Add("#힐러");
    }

    // --- 2. 핵심 스킬 로직 ---
    public override void ExecuteSkill()
    {
        // 1. 주인을 PlayerController로 '형 변환'
        PlayerController playerOwner = m_Owner as PlayerController;
        if (playerOwner == null) return;

        // 2. [핵심!] 주인(플레이어)의 체력을 '즉시' 회복시킵니다.
        playerOwner.AddHealth(this.BaseHeal);

        Debug.Log($"[{this.CardName}] 스킬! -> 영주 체력 {this.BaseHeal} 회복!");
    }
}
