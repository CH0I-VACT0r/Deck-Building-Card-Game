// 파일명: Card_FrostFowl.cs
using UnityEngine;

/// [빙닭] 10초마다 무작위 적 카드 1장을 1초간 '빙결'시킵니다.
public class Card_FrostFowl : Card // [핵심!] Card 뼈대를 상속
{
    // --- 1. 서리 주술사의 고유 스탯 ---
    private int m_TargetCount = 1;      // 1장을 대상으로
    private float m_FreezeDuration = 1.0f; // 3초 동안

    // --- 2. 생성자 ---
    public Card_FrostFowl(PlayerController owner, int index)
        : base(owner, index, 10f) // 부모(Card)에게 주인, 인덱스, 쿨타임(10초)을 전달
    {
        this.CardName = "빙닭";
        this.Rarity = CardRarity.Bronze;

        // 1초 빙결
        this.FreezeDurationToApply = m_FreezeDuration;

        this.CardImage = Resources.Load<Sprite>("CardImages/Lord_SevereCold/FrostFowl"); 

        this.Tags.Add("#야수");
        this.Tags.Add("#빙결"); // (CC를 거는 역할)

        // (예시) 이 카드는 '빙결'에 면역입니다.
        this.Immunities.Add(StatusEffectType.Freeze);
    }

    // --- 3. 핵심 스킬 로직 ---
    public override void ExecuteSkill()
    {
        // 1. 주인을 PlayerController로 '형 변환'
        PlayerController playerOwner = m_Owner as PlayerController;
        if (playerOwner == null) return;

        // 2. 타겟(몬스터 컨트롤러)을 가져옵니다.
        MonsterController target = playerOwner.GetTarget();
        if (target == null) return;

        target.ApplyStatusToRandomCards(m_TargetCount, StatusEffectType.Freeze, m_FreezeDuration);

        Debug.Log($"[{this.CardName}] 스킬! -> 무작위 적 {m_TargetCount}장에게 {m_FreezeDuration}초 빙결 부여!");
    }
}