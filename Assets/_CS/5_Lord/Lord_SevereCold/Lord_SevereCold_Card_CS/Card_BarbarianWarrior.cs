// 파일명: Card_BarbarianWarrior.cs
using UnityEngine;

/// [바바리안 전투병]
/// '혹한의 성주' 덱의 가장 기본이 되는 브론즈 등급 어태커
public class Card_BarbarianWarrior : Card // [핵심!] Card 뼈대를 상속
{
    // --- 1. 전투병의 고유 스탯 ---
    private float m_Damage = 20f; // 피해 : 20 
    private const float COOLDOWN = 7f; // 쿨타임 : 7초

    // --- 2. 생성자 ---
    // '바바리안 전투병' 카드가 생성될 때 호출
    /// <param name="owner">나를 소유한 PlayerController</param>
    /// <param name="index">내가 배치된 슬롯 인덱스 (0~6)</param>
    public Card_BarbarianWarrior(PlayerController owner, int index)
        : base(owner, index, COOLDOWN) // 부모(Card)에게 주인, 인덱스, 쿨타임(7초)을 전달
    {
        // 1. 기본 정보 설정
        this.CardName = "바바리안 전투병";
        this.Rarity = CardRarity.Bronze; // [등급]
        this.BaseDamage = 20f;

        // 2. 이미지 로드 (Resources/CardImages/barbarian_warrior.png 파일이 있다고 가정)
        this.CardImage = Resources.Load<Sprite>("CardImages/Lord_SevereCold/Barbarian_Warrior"); 

        // 3. [Tag 설정]
        this.Tags.Add("#야만전사");
        this.Tags.Add("#어태커");
    }

    // --- 3. 핵심 스킬 로직 ---
    public override void ExecuteSkill()
    {
        // 1. 주인 -> PlayerController
        PlayerController playerOwner = m_Owner as PlayerController;
        if (playerOwner == null) return; // (혹시 모를 오류 방지)

        // 2. 주인의 '타겟'(몬스터 컨트롤러)을 가져옵니다.
        MonsterController target = playerOwner.GetTarget();
        if (target == null) return;

        // 3. 타겟의 '본체'에 피해를 줍니다.
        target.TakeDamage(m_Damage);

        // 4. (디버그 로그)
        Debug.Log($"[{this.CardName}] (슬롯 {this.SlotIndex}) 스킬! -> 몬스터에게 {m_Damage} 피해");
    }
}
