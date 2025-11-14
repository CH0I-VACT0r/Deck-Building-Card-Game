
using UnityEngine;

// [피의 서약 방패병]
// 스스로 영주 체력을 소모하여 '격노' 상태를 유도하고, '격노' 시 막대한 쉴드를 얻는 핵심 탱커입니다.
public class Card_Shieldbearer : Card // [핵심!] Card 뼈대를 상속
{
    // --- 1. 방패병의 고유 스탯 ---
    private float m_SelfDamage = 5f;    // 영주 체력 5 소모
    private float m_BaseShield = 20f;   // 기본 쉴드
    private float m_EnragedShield = 45f;  // 격노 시 쉴드
    private const float COOLDOWN = 5f; // 10초 쿨타임

    // --- 2. 생성자 ---
    public Card_Shieldbearer(PlayerController owner, int index)
        : base(owner, index, COOLDOWN) // 부모(Card)에게 주인, 인덱스, 쿨타임(10초)을 전달
    {
        // 1. 기본 정보 설정
        this.CardName = "피의 서약 방패병";
        this.Rarity = CardRarity.Silver; // (핵심 카드이므로 실버로 설정)

        // 2. 이미지 로드 (Resources/CardImages/shieldbearer_art.png 파일이 있다고 가정)
        this.CardImage = Resources.Load<Sprite>("CardImages/Lord_SevereCold/Barbarian_Shieldbearer"); 

        // 3. [Tag 설정]
        this.Tags.Add("#야만전사");
        this.Tags.Add("#탱커");
    }

    // --- 3. 핵심 스킬 로직 ---
    public override void ExecuteSkill()
    {
        PlayerController playerOwner = m_Owner as PlayerController;
        if (playerOwner == null) return;

        float shieldToGain = m_BaseShield;
        bool isEnraged = false;

        // 1. '혹한의 성주'인지 확인하고 '격노' 상태를 가져옴
        Lord_SevereCold_Controller coldLord = playerOwner as Lord_SevereCold_Controller;
        if (coldLord != null)
        {
            isEnraged = coldLord.IsEnraged; 
        }

        // 2. '격노' 여부에 따라 쉴드량을 결정
        if (isEnraged)
        {
            shieldToGain = m_EnragedShield; 
        }

        // 4. 영주 체력을 소모 (격노 유도)
        // (이 함수는 쉴드를 무시하고 체력만 깎지 않고, 쉴드부터 깎습니다.)
        playerOwner.TakeDamage(m_SelfDamage);

        // 5. 영주에게 쉴드 추가
        playerOwner.AddShield(shieldToGain);

        // 6. (디버그 로그)
        Debug.Log($"[{this.CardName}] (슬롯 {this.SlotIndex}) 스킬! (격노:{isEnraged}) -> 체력 {m_SelfDamage} 소모, 쉴드 +{shieldToGain}");
    }
}
