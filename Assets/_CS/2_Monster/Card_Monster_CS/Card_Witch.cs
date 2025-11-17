using UnityEngine;

public class Card_Witch : Card
{
    public Card_Witch(object owner, int index) : base(owner, index, 10f)
    {
        // ... (기본 설정) ...
        this.CardNameKey = "card_witch_name";
        this.CardImage = Resources.Load<Sprite>("CardImages/Monster/Witch"); // (이미지 필요)
        // [신규!] 변이 스탯 설정 (툴팁용)
        this.PolymorphDurationToApply = 5.0f;            // 5초간
        this.PolymorphTargetNameKey = "card_sheep_name"; // "양"으로 변이
    }

    public override void ExecuteSkill()
    {
        MonsterController owner = m_Owner as MonsterController;
        if (owner == null)
        {
            UnityEngine.Debug.LogError("[WITCH] ExecuteSkill: Owner is not PlayerController!");
            return;
        }

        // 2. 타겟 컨트롤러를 통해 공격 대상(몬스터)을 얻어옵니다.
        PlayerController targetController = owner.GetTarget() as PlayerController;
        targetController.ApplyStatusToRandomCards(1, StatusEffectType.Polymorph, 5.0f, "card_sheep");
    }
}
