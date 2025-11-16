using UnityEngine;

/// <summary>
/// 카드: 야만전사 방패병 (용병)
/// </summary>
public class Card_BarbarianShieldbearer : Card
{
    // --- 생성자 ---
    public Card_BarbarianShieldbearer(object owner, int index)
        : base(owner, index, 7.0f) // 부모(Card) 생성자 호출 (쿨타임 7.0초)
    {
        // --- 1. 기본 정보 설정 (키 할당) ---
        this.CardNameKey = "card_barbarianshield_name";
        this.CardImage = Resources.Load<Sprite>("CardImages/Lord_SevereCold/Barbarian_Tanker");
        this.CardPrice = 3; 
        this.Rarity = CardRarity.Bronze;

        // --- 2. 태그 키 할당 ---
        this.TagKeys.Add("tag_mercenary");
        this.TagKeys.Add("tag_barbarian");
        this.TagKeys.Add("tag_tanker");

        // --- 3. 툴팁 정보 설정 (키 할당) ---
        this.CardSkillDescriptionKey = "card_barbarianshield_skill_desc";
        this.FlavorTextKey = "card_barbarianshield_flavor";

        // --- 4. 퀘스트 정보 설정 ---
        this.HasQuest = false; // 퀘스트 없음

        // --- 5. 내구도 ---
        this.Durability = -1; // 무한 내구도

        // --- 6. 스탯 설정 (스킬 로직 및 툴팁용) ---
        this.BaseShield = 20f; // 스킬 쉴드량 20
        this.BaseCritChance = 0.1f; // 치명타 10%
    }

    // --- 7. 스킬 로직 구현 ---
    public override void ExecuteSkill()
    {
        // 1. 실시간 쉴드량 계산 (치명타 적용!)
        // (치명타 시 2배의 쉴드를 얻음)
        float critMultiplier = CheckForCrit();
        float realShield = GetCurrentShield() * critMultiplier;

        // 2. 주인(Owner)이 플레이어인지 몬스터인지 확인
        PlayerController playerOwner = m_Owner as PlayerController;
        MonsterController monsterOwner = m_Owner as MonsterController;

        // 3. '본체(Owner)'에게 쉴드 추가
        if (playerOwner != null)
        {
            playerOwner.AddShield(realShield);
        }
        else if (monsterOwner != null)
        {
            monsterOwner.AddShield(realShield);
        }

        if (critMultiplier > 1.0f)
        {
            Debug.Log($"[{this.CardNameKey}] 치명타 쉴드! {realShield} 쉴드 획득!");
        }
        else
        {
            Debug.Log($"[{this.CardNameKey}] 스킬 발동! {realShield} 쉴드 획득!");
        }
    }

    // --- 8. 실시간 스탯 계산 (툴팁용) ---

    // 툴팁이 GetCurrentShield()를 호출할 때 이 함수가 실행됨
    public override float GetCurrentShield()
    {
        // TODO: 나중에 이 카드에 버프가 걸리면 여기서 계산
        return this.BaseShield;
    }

    // 툴팁이 GetCurrentCritChance()를 호출할 때 이 함수가 실행됨
    public override float GetCurrentCritChance()
    {
        return this.BaseCritChance;
    }
}
