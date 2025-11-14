// 파일명: Card_FuriousWarrior.cs
using UnityEngine;

/// [광포한 전투병] 스킬을 사용할 때마다 '이번 전투 동안' 자신의 피해량이 영구적으로 5씩 증가합니다.

public class Card_FuriousWarrior : Card
{
    // --- 1. 광포한 전투병의 고유 스탯 ---
    private float m_DamageIncreasePerSkill = 5f; // 스킬당 성장 수치

    // 이번 전투 동안 쌓인 보너스 피해
    private float m_CurrentBonusDamage = 0f;


    // --- 2. 생성자 ---
    public Card_FuriousWarrior(PlayerController owner, int index)
        : base(owner, index, 7f) // (쿨타임 7초)
    {
        this.CardName = "광포한 전투병";
        this.Rarity = CardRarity.Silver;

        // [역할 데이터] '기본' 피해량은 10입니다.
        this.BaseDamage = 10f;

        this.CardImage = Resources.Load<Sprite>("CardImages/Lord_SevereCold/FuriousWarrior"); 

        this.Tags.Add("#야만전사");
        this.Tags.Add("#어태커");
    }


    // --- 3. 성장 반영 UI ---
    public override float GetCurrentDamage()
    {
        return this.BaseDamage + m_CurrentBonusDamage;
    }


    // --- 4. 핵심 스킬 로직 ---
    public override void ExecuteSkill()
    {
        PlayerController playerOwner = m_Owner as PlayerController;
        if (playerOwner == null) return;
        MonsterController target = playerOwner.GetTarget();
        if (target == null) return;

        // 1. 현재 총 피해량을 가져와서 공격
        float damageToDeal = GetCurrentDamage();
        target.TakeDamage(damageToDeal);

        Debug.Log($"[{this.CardName}] 스킬! -> 몬스터에게 {damageToDeal} 피해");

        // 2. 공격한 후 보너스 피해량
        m_CurrentBonusDamage += m_DamageIncreasePerSkill;
    }


    // --- 5. 전투 종료 시 스탯 보너스 초기화 로직 ---
    public override void ClearBattleStatBuffs()
    {
        base.ClearBattleStatBuffs();
        m_CurrentBonusDamage = 0f;
    }
}