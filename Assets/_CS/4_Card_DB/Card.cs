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

    // 역할 UI - [기본]
    public float BaseDamage { get; protected set; } = 0;   // 
    public float BaseShield { get; protected set; } = 0;   // 
    public float BaseHeal { get; protected set; } = 0;     // 
    public int HealStacksToApply { get; protected set; } = 0;

    public virtual float GetCurrentDamage() { return this.BaseDamage; }
    public virtual float GetCurrentShield() { return this.BaseShield; }
    public virtual float GetCurrentHeal() { return this.BaseHeal; }

    // 역할 UI - [상태 이상]
    public int BleedStacksToApply { get; protected set; } = 0;    // 
    public float FreezeDurationToApply { get; protected set; } = 0; // 
    

    public virtual int GetCurrentBleedStacks() { return this.BleedStacksToApply; }
    public virtual float GetCurrentFreezeDuration() { return this.FreezeDurationToApply; }
    public virtual int GetCurrentHealStacks() { return this.HealStacksToApply; }

    // -------------------
    // --- [상태 이상] ---
    public List<StatusEffectType> Immunities { get; protected set; } = new List<StatusEffectType>();
    private bool m_IsFrozen = false;
    private float m_FreezeTimer = 0f;

    public virtual void ClearBattleStatBuffs()
    {
    }

    public bool IsFrozen()
    {
        return m_IsFrozen;
    }
    public virtual void ClearBattleFrozen()
    {
        // 빙결 상태를 강제로 해제합니다.
        m_IsFrozen = false;
        m_FreezeTimer = 0f;
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
    /// 카드의 고유 스킬 로직
    /// 이 클래스를 상속받는 모든 카드는, 이 함수의 내용물을 반드시' 자신만의 로직으로 override 돼야 함.
    public abstract void ExecuteSkill();

    /// BattleManager가 매 프레임 호출하여 쿨타임을 줄여주는 함수
    /// <param name="deltaTime">Time.deltaTime (프레임당 시간)</param>
    public virtual void UpdateCooldown(float deltaTime)
    {
        // '빙결' 상태라면, 쿨타임을 줄이지 않고 빙결 시간만 줄입니다.
        if (m_IsFrozen)
        {
            m_FreezeTimer -= deltaTime;
            if (m_FreezeTimer <= 0)
            {
                m_IsFrozen = false;
                Debug.Log($"[{this.CardName}] (이)가 빙결에서 풀려났습니다!");
            }
            return; // [핵심!] 쿨타임 감소 로직을 실행하지 않고 건너뜀!
        }

        if (CurrentCooldown > 0)
        {
            CurrentCooldown -= deltaTime;
        }
    }

    /// 외부에서 이 카드에게 상태 이상을 적용하는 함수
    /// <returns>면역이면 false, 적용되면 true를 반환합니다.</returns>
    public virtual bool ApplyStatusEffect(StatusEffectType effectType, float duration)
    {
        // 1. 면역인지 체크
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
                m_FreezeTimer = duration;
                Debug.Log($"[{this.CardName}] (이)가 {duration}초간 빙결되었습니다!");
                break;
        }
        return true; 
    }
}