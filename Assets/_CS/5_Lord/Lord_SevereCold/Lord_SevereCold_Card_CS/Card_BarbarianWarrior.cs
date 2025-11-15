// 파일명: Card_BarbarianWarrior.cs
using UnityEngine;
using System.Collections.Generic; // [신규!] List 사용을 위해 추가

/// <summary>
/// [바바리안 전투병] - (촉진/방해/치명타 테스트 버전)
/// 7초마다 스킬 발동 시:
/// 1. [치명타]: 50% 확률로 2배 피해
/// 2. [촉진]: '자신'의 쿨타임을 2초 즉시 감소
/// 3. [방해]: '무작위 적 1장'의 쿨타임을 2초 즉시 증가
/// </summary>
public class Card_BarbarianWarrior : Card // Card 뼈대를 상속
{
    // --- 1. 전투병의 고유 스탯 ---
    private const float COOLDOWN = 3f;

    // 테스트용 스탯
    private float m_ReduceCooldownAmount = 2.0f; // 2초 촉진
    private float m_IncreaseCooldownAmount = 2.0f; // 2초 방해
    private int m_IncreaseTargetCount = 1;     // 1명 대상

    // --- 2. 생성자 ---
    public Card_BarbarianWarrior(PlayerController owner, int index)
        : base(owner, index, COOLDOWN)
    {
        this.CardName = "바바리안 전투병 (촉진/방해 테스트)";
        this.Rarity = CardRarity.Bronze;

        // [역할 데이터]
        this.BaseDamage = 1f; // 기본 피해량 20
        this.BaseCritChance = 0.5f; // 치명타 확률 50%

        this.CardImage = Resources.Load<Sprite>("CardImages/Lord_SevereCold/Barbarian_Warrior");

        this.Tags.Add("#북방 야만전사");
        this.Tags.Add("#어태커");
    }

    // --- 3. 핵심 스킬 로직 (수정!) ---
    public override void ExecuteSkill()
    {
        // 1. 주인 및 타겟 확인
        PlayerController playerOwner = m_Owner as PlayerController;
        if (playerOwner == null) return;
        MonsterController target = playerOwner.GetTarget();
        if (target == null) return;

        // --- [로직 1: 치명타] ---
        float critMultiplier = CheckForCrit();
        float damageToDeal = this.BaseDamage * critMultiplier;
        target.TakeDamage(damageToDeal);

        // --- [로직 2: 셀프 촉진] ---
        this.ReduceCooldown(m_ReduceCooldownAmount);

        // --- [로직 3: 적 방해] (수정됨!) ---

        // 3a. 적의 '활성화된' 카드 목록을 만듭니다. (중요: 루프 밖으로 이동)
        List<Card> activeEnemyCards = new List<Card>();
        for (int i = 0; i < 7; i++)
        {
            Card enemyCard = target.GetCardAtIndex(i);
            if (enemyCard != null)
            {
                activeEnemyCards.Add(enemyCard);
            }
        }

        // [신규!] m_IncreaseTargetCount (1) 만큼 반복합니다.
        for (int i = 0; i < m_IncreaseTargetCount; i++)
        {
            // 3b. 방해할 대상이 남아있는지 확인합니다.
            if (activeEnemyCards.Count > 0)
            {
                // 3c. 1명을 무작위로 고릅니다.
                int randomIndex = Random.Range(0, activeEnemyCards.Count);
                Card targetCard = activeEnemyCards[randomIndex];

                // 3d. 쿨타임을 2초 늘립니다.
                targetCard.IncreaseCooldown(m_IncreaseCooldownAmount);
                Debug.Log($"... [{this.CardName}] (이)가 적 [{targetCard.CardName}] 쿨타임 {m_IncreaseCooldownAmount}초 '방해'!");

                // 3e. [중요!] 중복으로 방해하지 않도록, 목록에서 제거합니다.
                activeEnemyCards.RemoveAt(randomIndex);
            }
            else
            {
                // 방해할 카드가 더 이상 없으면 루프를 중단합니다.
                break;
            }
        }

        // 4. (디버그 로그)
        Debug.Log($"[{this.CardName}] 스킬! -> 몬스터에게 {damageToDeal} 피해, " +
                  $"자신 '촉진'({m_ReduceCooldownAmount}s)!");

        // --- (삭제!) ---
        // (가속, 감속, 메아리 로직 삭제됨)
    }

    // --- 4. GetCurrentCritChance (아우라 테스트용) ---
    // (이 카드는 '아우라'를 받을 수 있습니다)
    public override float GetCurrentCritChance()
    {
        float totalCrit = this.BaseCritChance;
        PlayerController playerOwner = m_Owner as PlayerController;
        if (playerOwner == null) return totalCrit;

        Card leftCard = playerOwner.GetLeftNeighbor(this.SlotIndex);
        if (leftCard != null)
        {
            totalCrit += leftCard.GetAuraBuffTo(this, "CritChance");
        }
        Card rightCard = playerOwner.GetRightNeighbor(this.SlotIndex);
        if (rightCard != null)
        {
            totalCrit += rightCard.GetAuraBuffTo(this, "CritChance");
        }

        return totalCrit;
    }
}