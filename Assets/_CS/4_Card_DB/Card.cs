/// 모든 카드(용병, 몬스터, 장비, 건축물 등등)의 공통 설계 추상 클래스
/// 이 클래스를 상속받아 실제 카드 제작하면 됨
using UnityEngine;
using System.Collections.Generic;

public abstract class Card
{
    // 1. 공통 데이터 
    // 모든 카드가 공통적으로 가지는 속성
    public string CardName { get; protected set; } // 카드 이름
    public Sprite CardImage { get; protected set; } // 카드 이미지
    public CardRarity Rarity { get; protected set; } // 카드 등급
    public float CooldownTime { get; protected set; } // 카드의 기본 스킬 쿨타임 (초)
    public float CurrentCooldown { get; set; } // 현재 남은 쿨타임. 0이 되면 스킬 발동
    protected object m_Owner; // 이 카드를 소유하고 관리하는 플레이어 또는 몬스터
    public int SlotIndex { get; private set; } // 카드가 몇 번 슬롯에 있는지
    public int Durability { get; protected set; } = -1; // 내구도

    // 역할 UI - [기본]
    public float BaseDamage { get; protected set; } = 0;   // 기본 대미지
    public float BaseShield { get; protected set; } = 0;   // 기본 쉴드
    public float BaseHeal { get; protected set; } = 0;     // 기본 회복
    public int HealStacksToApply { get; protected set; } = 0; // 지속 회복
    public float BaseCritChance { get; protected set; } = 0f; // 치명타 확률

    public virtual float GetCurrentDamage() { return this.BaseDamage; } // 대미지
    public virtual float GetCurrentShield() { return this.BaseShield; } // 쉴드
    public virtual float GetCurrentHeal() { return this.BaseHeal; } // 회복
    public virtual int GetCurrentHealStacks() { return this.HealStacksToApply; } // 지속 회복
    public virtual float GetCurrentCritChance() { return this.BaseCritChance; } // 치명타 확률

    // 역할 UI - [상태 이상]
    public int BleedStacksToApply { get; protected set; } = 0;    // 출혈 적용
    public float FreezeDurationToApply { get; protected set; } = 0; // 빙결 적용
    public int PoisonStacksToApply { get; protected set; } = 0; // 중독 적용
    public int BurnStacksToApply { get; protected set; } = 0; // 화상 적용


    public virtual int GetCurrentBleedStacks() { return this.BleedStacksToApply; }
    public virtual float GetCurrentFreezeDuration() { return this.FreezeDurationToApply; }
    public virtual int GetCurrentPoisonStacks() { return this.PoisonStacksToApply; } // [신규!]
    public virtual int GetCurrentBurnStacks() { return this.BurnStacksToApply; }

    // --- [상태 이상 컨트롤] ---
    public List<StatusEffectType> Immunities { get; protected set; } = new List<StatusEffectType>(); // 면역
    private bool m_IsFrozen = false; // 빙결 여부
    private float m_FreezeTimer = 0f; // 빙결 타이머
    private bool m_IsHasted = false; // 가속 여부
    private float m_HasteTimer = 0f; // 가속 타이머
    private bool m_IsSlowed = false; // 감속 여부
    private float m_SlowTimer = 0f; // 감속 타이머

    public bool IsFrozen()
    {
        return m_IsFrozen;
    }
    public bool IsHasted()
    {
        return m_IsHasted;
    }
    public bool IsSlowed()
    {
        return m_IsSlowed;
    }

    public virtual void ClearBattleFrozen() // 어디서 쓰는 지 몰라서 일단 남겨둠
    {
        // 빙결 상태를 강제로 해제합니다.
        m_IsFrozen = false;
        m_FreezeTimer = 0f;
    }

    public virtual void ClearStatusEffects() // 전투 종료 시 외부 상태 이상 초기화
    {
        m_IsFrozen = false;
        m_FreezeTimer = 0f;
        m_IsHasted = false;
        m_HasteTimer = 0f;
        m_IsSlowed = false;
        m_SlowTimer = 0f;
    }

    public virtual void ClearBattleStatBuffs()
    {
    }

    // ----- [태그] -----
    public List<string> Tags { get; protected set; } = new List<string>();
    // -------------------

    // --- 2. 생성자 (카드 처음 생성 시) ---

    /// 새 카드를 생성할 때 호출됩니다.
    /// <param name="owner">이 카드를 소유할 컨트롤러 (PlayerController 또는 MonsterController)</param>
    /// <param name="cooldown">이 카드의 기본 쿨타임 </param>
    /// <param name="index">이 카드의 위치 </param>

    public Card(object owner, int index, float cooldown)
    {
        this.m_Owner = owner;
        this.SlotIndex = index;
        this.CooldownTime = cooldown;
        this.CurrentCooldown = cooldown; // 전투 시작 시 쿨타임이 가득 찬 상태로 시작
        this.CardName = "Default Card Name"; 
    }

    /// 카드가 특정 Tag를 가지고 있는지 확인
    public bool HasTag(string tag)
    {
        return Tags.Contains(tag);
    }

    // --- 3. 핵심 함수 ---
    // 카드의 고유 스킬 로직 : 이 클래스를 상속받는 모든 카드는, 이 함수의 내용물을 반드시' 자신만의 로직으로 override 돼야 함.
    public abstract void ExecuteSkill();

    // 크리티컬 확인
    protected float CheckForCrit()
    {
        float currentCritChance = GetCurrentCritChance();
        if (currentCritChance <= 0) return 1.0f;

        if (Random.Range(0f, 1.0f) < currentCritChance)
        {
            Debug.Log($"[{this.CardName}] 치명타 발동!");
            return 2.0f; // 2배
        }
        return 1.0f; // 1배
    }

    /// BattleManager가 매 프레임 호출하여 쿨타임을 줄여주는 함수
    public virtual void UpdateCooldown(float deltaTime)
    {
        // 1. 빙결 체크
        if (m_IsFrozen)
        {
            m_FreezeTimer -= deltaTime;
            if (m_FreezeTimer <= 0) m_IsFrozen = false;
            return; // 쿨타임 감소 X
        }

        // 2. 가속/감속 적용된 
        float modifiedDeltaTime = deltaTime;

        if (m_IsHasted)
        {
            modifiedDeltaTime *= 2.0f; // 2배 가속
            m_HasteTimer -= deltaTime;
            if (m_HasteTimer <= 0) m_IsHasted = false;
        }
        else if (m_IsSlowed) // (가속과 감속은 중복 안 됨)
        {
            modifiedDeltaTime *= 0.5f; // 2배 느리게!
            m_SlowTimer -= deltaTime;
            if (m_SlowTimer <= 0) m_IsSlowed = false;
        }

        // 3. (기존 로직) 쿨타임 감소
        if (CurrentCooldown > 0)
        {
            CurrentCooldown -= modifiedDeltaTime; // '수정된 시간'으로 쿨타임 감소
        }
    }

    /// 외부에서 이 카드에게 상태 이상을 적용하는 함수 : <returns>면역이면 false, 적용되면 true를 반환 
    public virtual bool ApplyStatusEffect(StatusEffectType effectType, float duration)
    {
        // 면역 체크
        if (Immunities.Contains(effectType))
        {
            Debug.Log($"[{this.CardName}] (은)는 '{effectType}' 효과에 면역입니다!");
            return false; // 적용 실패!
        }

        // 2. 면역이 아니면, 실제 효과 적용
        switch (effectType)
        {
            case StatusEffectType.Freeze:
                m_IsFrozen = true;
                m_FreezeTimer = Mathf.Max(m_FreezeTimer, duration); // 더 긴 시간으로 갱신
                Debug.Log($"[{this.CardName}] (이)가 {duration}초간 빙결되었습니다!");
                break;

            case StatusEffectType.Haste:
                m_IsHasted = true;
                m_HasteTimer = Mathf.Max(m_HasteTimer, duration);
                m_IsSlowed = false; // 감속 해제
                m_SlowTimer = 0f;
                Debug.Log($"[{this.CardName}] (이)가 {duration}초간 가속되었습니다!");
                break;

            case StatusEffectType.Slow:
                m_IsSlowed = true;
                m_SlowTimer = Mathf.Max(m_SlowTimer, duration);
                m_IsHasted = false; // 가속 해제
                m_HasteTimer = 0f;
                Debug.Log($"[{this.CardName}] (이)가 {duration}초간 감속되었습니다!");
                break;
        }
        return true;
    }

    /// 쿨타임 즉시 감소 (초과분 적용)
    public virtual void ReduceCooldown(float amount)
    {
        CurrentCooldown -= amount;

        if (CurrentCooldown <= 0f)
        {
            float excessAmount = -CurrentCooldown; // 초과분 

            ExecuteSkill(); // 스킬 즉시 발동

            // 쿨타임을 100%로 채우고, 초과분만큼 다시 감소
            CurrentCooldown = CooldownTime - excessAmount;
        }
    }

    //쿨타임 즉시 증가
    public virtual void IncreaseCooldown(float amount)
    {
        if (!m_IsFrozen)
        {
            CurrentCooldown += amount;
        }
    }
}