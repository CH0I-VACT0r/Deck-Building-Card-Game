// 파일명: Card_Regenerator.cs
using UnityEngine;

/// <summary>
/// [지속 힐 테스트용 카드] 10초마다 영주에게 '지속 회복' 5스택을 부여합니다.
public class Card_Regenerator : Card // Card 뼈대를 상속
{
    // --- 1. 생성자 ---
    public Card_Regenerator(PlayerController owner, int index)
        : base(owner, index, 10f) // 15초 쿨타임
    {
        this.CardName = "재생의 토템";
        this.Rarity = CardRarity.Silver;

        //
        this.HealStacksToApply = 1;

        this.CardImage = Resources.Load<Sprite>("CardImages/Lord_SevereCold/Regenerator"); 

        this.Tags.Add("#야수");
        this.Tags.Add("#지속 힐러");
    }

    // --- 2. 핵심 스킬 로직 ---
    public override void ExecuteSkill()
    {
        // 1. 주인을 PlayerController로 '형 변환'
        PlayerController playerOwner = m_Owner as PlayerController;
        if (playerOwner == null) return;

        // 2. [핵심!] 주인(플레이어)에게 '지속 회복' 상태 이상을 적용시킵니다.
        playerOwner.ApplyLordStatus(StatusEffectType.Heal, this.HealStacksToApply);

        Debug.Log($"[{this.CardName}] 스킬! -> 영주에게 지속 회복 {this.HealStacksToApply} 중첩 부여!");
    }
}
