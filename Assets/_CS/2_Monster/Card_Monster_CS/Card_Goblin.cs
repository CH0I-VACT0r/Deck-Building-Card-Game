using UnityEngine;

/// <summary>
/// 카드: 고블린 (몬스터 전용)
/// </summary>
public class Card_Goblin : Card
{
    // --- 생성자 ---
    public Card_Goblin(object owner, int index)
        : base(owner, index, 7.0f) // 부모(Card) 생성자 호출 (쿨타임 7.0초)
    {
        // --- 1. 기본 정보 설정 (키 할당) ---
        this.CardNameKey = "card_goblin_name";
        // (TODO: "CardImages/Goblin" 경로에 실제 이미지 파일이 있어야 합니다)
        this.CardImage = Resources.Load<Sprite>("CardImages/Monster/Goblin");
        SetInitPrice(1); // (가격은 임의로 5로 설정)
        this.Rarity = CardRarity.Bronze; // (몬스터 등급)

        // --- 2. 태그 키 할당 ---
        this.TagKeys.Add("tag_monster");
        this.TagKeys.Add("tag_goblin");

        // --- 3. 툴팁 정보 설정 (키 할당) ---
        this.CardSkillDescriptionKey = "card_goblin_skill_desc";
        this.FlavorTextKey = "card_goblin_flavor";

        // --- 4. 퀘스트 정보 설정 ---
        this.HasQuest = false; // 퀘스트 없음

        // --- 5. 내구도 ---
        this.Durability = -1; // 무한 내구도

        // --- 6. 스탯 설정 (스킬 로직 및 툴팁용) ---
        this.BaseDamage = 5f; // 스킬 피해량 5
        this.BaseCritChance = 0.05f; // 치명타 5%
    }

    // --- 7. 스킬 로직 구현 ---
    public override void ExecuteSkill()
    {
        // 1. 실시간 피해량 계산 (버프 + 치명타 포함)
        float realDamage = GetCurrentDamage() * CheckForCrit();

        // 2. 이 카드는 '몬스터' 전용이므로, 주인(Owner)을 MonsterController로 간주
        MonsterController monsterOwner = m_Owner as MonsterController;

        // 3. 타겟(플레이어)에게 피해 입히기
        if (monsterOwner != null)
        {
            monsterOwner.GetTarget()?.TakeDamage(realDamage);
            Debug.Log($"[{this.CardNameKey}] 스킬 발동! {realDamage} 피해!");
        }
        else
        {
            Debug.LogError($"[{this.CardNameKey}] 주인이 MonsterController가 아닙니다!");
        }
    }

    // --- 8. 실시간 스탯 계산 (툴팁용) ---

    // 툴팁이 GetCurrentDamage()를 호출할 때 이 함수가 실행됨
    public override float GetCurrentDamage()
    {
        // TODO: 나중에 이 카드에 버프가 걸리면 여기서 계산
        return this.BaseDamage;
    }

    // 툴팁이 GetCurrentCritChance()를 호출할 때 이 함수가 실행됨
    public override float GetCurrentCritChance()
    {
        return this.BaseCritChance;
    }
}
