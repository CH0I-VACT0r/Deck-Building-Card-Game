// 파일명: Card_Goblin.cs
using UnityEngine;

/// <summary>
/// [고블린 돌격병]
/// 6초마다 20의 피해를 주는 간단한 테스트용 몬스터입니다.
/// </summary>
public class Card_Goblin : Card // Card 뼈대를 상속
{
    // --- 1. 고블린의 고유 스탯 ---
    private float m_Damage = 20f;
    private const float COOLDOWN = 6f;

    // --- 2. 생성자 ---
    /// <summary>
    /// '고블린' 카드가 생성될 때 호출됩니다.
    /// [중요!] 몬스터 카드는 'MonsterController'를 주인으로 받습니다.
    /// </summary>
    /// <param name="owner">나를 소유한 MonsterController</param>
    /// <param name="index">내가 배치된 슬롯 인덱스 (0~6)</param>
    public Card_Goblin(MonsterController owner, int index)
        : base(owner, index, COOLDOWN) // 부모(Card)에게 주인, 인덱스, 쿨타임(6초)을 전달
    {
        // 1. 기본 정보 설정
        this.CardName = "고블린 돌격병";
        this.Rarity = CardRarity.Bronze;

        // 2. 이미지 로드 (Resources/CardImages/goblin_art.png 파일이 있다고 가정)
        this.CardImage = Resources.Load<Sprite>("CardImages/Monster/Goblin"); 

        // 3. [Tag 설정]
        this.Tags.Add("#몬스터");
        this.Tags.Add("#어태커");
    }

    // --- 3. 핵심 스킬 로직 ---
    public override void ExecuteSkill()
    {
        // 1. 주인을 MonsterController로 '형 변환'
        MonsterController monsterOwner = m_Owner as MonsterController;
        if (monsterOwner == null) return;

        // 2. 주인의 '타겟'(플레이어 컨트롤러)을 가져옵니다.
        PlayerController target = monsterOwner.GetTarget();
        if (target == null) return;

        // 3. 타겟의 '본체'에 피해를 줍니다.
        target.TakeDamage(m_Damage);

        // 4. (디버그 로그)
        Debug.LogWarning($"[{this.CardName}] (슬롯 {this.SlotIndex}) 스킬! -> 플레이어에게 {m_Damage} 피해");
    }
}
