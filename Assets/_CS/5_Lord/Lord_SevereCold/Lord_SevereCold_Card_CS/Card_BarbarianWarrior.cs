using UnityEngine;

/// <summary>
/// 카드: 바바리안 전사
/// </summary>
public class Card_BarbarianWarrior : Card
{
    // --- 생성자 ---
    public Card_BarbarianWarrior(object owner, int index)
        : base(owner, index, 7.0f) // 부모(Card) 생성자 호출 (쿨타임 7.0초)
    {
        // --- 1. 기본 정보 설정 (키 할당) ---
        this.CardNameKey = "card_barbarian_warrior_name";
        // (TODO: "CardImages/BarbarianWarrior" 경로에 실제 이미지 파일이 있어야 합니다)
        this.CardImage = Resources.Load<Sprite>("CardImages/Lord_SevereCold/Barbarian_Warrior");
        SetInitPrice(3);
        this.Rarity = CardRarity.Bronze;

        // --- 2. 태그 키 할당 ---
        this.TagKeys.Add("tag_mercenary");
        this.TagKeys.Add("tag_barbarian");
        this.TagKeys.Add("tag_dealer");

        // --- 3. 툴팁 정보 설정 (키 할당) ---
        this.CardSkillDescriptionKey = "card_barbarian_warrior_skill_desc";
        this.FlavorTextKey = "card_barbarian_warrior_flavor";

        // --- 4. 퀘스트 정보 설정 ---
        this.HasQuest = true;
        this.QuestTitleKey = "card_barbarian_warrior_quest_title";
        this.QuestDescriptionKey = "card_barbarian_warrior_quest_desc";
        this.IsQuestComplete = false; // (전투 3회 승리 여부는 다른 시스템이 관리해야 함)

        // --- 5. 내구도 ---
        this.Durability = -1; // 무한 내구도

        // --- 6. 스탯 설정 (스킬 로직 및 툴팁용) ---
        this.BaseDamage = 20f;
        this.BaseCritChance = 0.1f; // 10%
    }

    // --- 7. 스킬 로직 구현 ---
    public override void ExecuteSkill()
    {
        // 1. 실시간 피해량 계산 (버프 + 치명타 포함)
        float realDamage = GetCurrentDamage() * CheckForCrit();

        // 2. 주인(Owner)이 플레이어인지 몬스터인지 확인
        PlayerController playerOwner = m_Owner as PlayerController;
        MonsterController monsterOwner = m_Owner as MonsterController;

        // 3. 타겟에게 피해 입히기
        if (playerOwner != null)
        {
            // 내가 플레이어 소속이면, 타겟(몬스터)을 공격
            playerOwner.GetTarget()?.TakeDamage(realDamage);
        }
        else if (monsterOwner != null)
        {
            // 내가 몬스터 소속이면, 타겟(플레이어)을 공격
            monsterOwner.GetTarget()?.TakeDamage(realDamage);
        }

        Debug.Log($"[{this.CardNameKey}] 스킬 발동! {realDamage} 피해!");
    }

    // --- 8. 실시간 스탯 계산 (툴팁용) ---

    // 툴팁이 GetCurrentDamage()를 호출할 때 이 함수가 실행됨
    public override float GetCurrentDamage()
    {
        // TODO: 나중에 이 카드에 버프가 걸리면 여기서 계산
        // (예: return this.BaseDamage + m_BuffAmount;)
        return this.BaseDamage;
    }

    // 툴팁이 GetCurrentCritChance()를 호출할 때 이 함수가 실행됨
    public override float GetCurrentCritChance()
    {
        return this.BaseCritChance;
    }
}
