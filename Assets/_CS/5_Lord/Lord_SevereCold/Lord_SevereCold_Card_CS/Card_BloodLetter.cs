
using UnityEngine;

// [출혈 테스트용 카드] 8초마다 적에게 '출혈' 3스택을 부여합니다.
public class Card_Bloodletter : Card // Card 뼈대를 상속
{
    // --- 1. 생성자 ---
    public Card_Bloodletter(PlayerController owner, int index)
        : base(owner, index, 8f) // 부모(Card)에게 주인, 인덱스, 쿨타임(8초)을 전달
    {
        this.CardName = "출혈 테스트용 카드";
        this.Rarity = CardRarity.Bronze;

        // 출혈 3스택
        this.BleedStacksToApply = 3;

        this.CardImage = Resources.Load<Sprite>("CardImages/Lord_SevereCold/BloodLetter"); 

        this.Tags.Add("#야만전사");
        this.Tags.Add("#출혈");
    }

    // --- 2. 핵심 스킬 로직 ---
    public override void ExecuteSkill()
    {
        // 1. 주인을 PlayerController로 '형 변환'
        PlayerController playerOwner = m_Owner as PlayerController;
        if (playerOwner == null) return;

        // 2. 타겟(몬스터 컨트롤러)을 가져옵니다.
        MonsterController target = playerOwner.GetTarget();
        if (target == null) return;

        // 3. [핵심!] 타겟에게 '출혈' 상태 이상을 적용시킵니다.
        target.ApplyLordDoT(StatusEffectType.Bleed, this.BleedStacksToApply);

        Debug.Log($"[{this.CardName}] 스킬! -> 몬스터에게 출혈 {this.BleedStacksToApply} 중첩 부여!");
    }
}
