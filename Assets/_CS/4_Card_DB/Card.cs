/// 모든 카드(용병, 몬스터, 장비, 건축물 등등)의 공통 설계 추상 클래스
/// 이 클래스를 상속받아 실제 카드 제작하면 됨
using UnityEngine;
using System.Collections.Generic;

public abstract class Card
{
    // 1. 공통 데이터 
    // 모든 카드가 공통적으로 가지는 속성
    public string CardNameKey { get; protected set; }                         // 카드 이름
    public Sprite CardImage { get; protected set; }                           // 카드 이미지
    public List<string> TagKeys { get; protected set; } = new List<string>(); // 카드 태그 키
    public CardRarity Rarity { get; protected set; }                          // 카드 등급
    public int CardPrice { get; protected set; } = 0;                         // 카드 가격 (변동 가능)
    public int BasePrice { get; protected set; } = 0;                         // 원래 가격 (고정)
    public LordType OwnerLord { get; protected set; } = LordType.Common;      // 소속 영주
    public float BaseCooldownTime { get; protected set; }                     // 카드의 기본 스킬 쿨타임 (초)
    public float CurrentCooldown { get; set; }                                // 현재 남은 쿨타임. 0이 되면 스킬 발동
    protected object m_Owner;                                                 // 이 카드를 소유하고 관리하는 플레이어 또는 몬스터
    public int OriginalSlotIndex { get; private set; }                        // 전투 시작 시 슬롯 인덱스 (불변)
    public int SlotIndex { get; private set; }                                // 현재 카드 슬롯 인덱스
    public int Durability { get; protected set; } = -1;                       // 내구도
    protected int InnateEchoCount { get; set; } = 1;                          // 기본 스킬 시전 횟수
    private int m_BonusEchoStacks = 0;                                        // 추가 시전 횟수 변수
    public bool ShowCooldownUI { get; protected set; } = true;                // 쿨타운 UI 표기 여부 : 패시브 스킬만 있는 카드는 표기 안 함
    public int PriceInflateAmount { get; protected set; } = 0;                // 인상량
    public int PriceExtortAmount { get; protected set; } = 0;                 // 인하량
    public int TriggersChainCount { get; protected set; } = 0;                // 연쇄

    public string SummonCardNameKey { get; protected set; } = "";             // 소환할 카드 이름
    public int SummonCount { get; protected set; } = 0;                       // 소환 개체 수
    public string DeathrattleDescKey { get; protected set; } = "";            // 유언 설명

    public Card OriginalForm { get; set; } = null;                            // 원본 카드 저장
    public float PolymorphTimer { get; set; } = 0f;                           // 변이 지속 시간
    public bool IsPolymorphed => OriginalForm != null;                        // 변이 상태 여부 확인
    public float PolymorphDurationToApply { get; protected set; } = 0f;       // 변이 툴팁용 시간
    public string PolymorphTargetNameKey { get; protected set; } = "";        // 변이 대상의 이름 키 
    public float TriggersTargetShuffle { get; protected set; } = 0f;          // 교란

    // 툴팁
    public string CardSkillDescriptionKey { get; protected set; } = "";  // 카드 스킬 설명
    public bool HasQuest { get; protected set; } = false;                // 퀘스트 여부
    public string QuestTitleKey { get; protected set; } = "";            // 퀘스트 이름
    public string QuestDescriptionKey { get; protected set; } = "";      // 퀘스트 설명
    public bool IsQuestComplete { get; protected set; } = false;         // 퀘스트 달성 여부
    public string FlavorTextKey { get; protected set; } = "";            // 카드 플레이버 텍스트

    // 역할 UI - [기본]
    public float BaseDamage { get; protected set; } = 0;      // 기본 대미지
    public float BaseShield { get; protected set; } = 0;      // 기본 쉴드
    public float BaseHeal { get; protected set; } = 0;        // 기본 회복
    public int HealStacksToApply { get; protected set; } = 0; // 지속 회복
    public float BaseCritChance { get; protected set; } = 0f; // 치명타 확률

    public virtual float GetCurrentDamage() { return this.BaseDamage; }          // 대미지
    public virtual float GetCurrentShield() { return this.BaseShield; }          // 쉴드
    public virtual float GetCurrentHeal() { return this.BaseHeal; }              // 회복
    public virtual int GetCurrentHealStacks() { return this.HealStacksToApply; } // 지속 회복
    public virtual float GetCurrentCritChance() { return this.BaseCritChance; }  // 치명타 확률

    // 역할 UI - [상태 이상]
    public int BleedStacksToApply { get; protected set; } = 0;    // 출혈 적용
    public float FreezeDurationToApply { get; protected set; } = 0; // 빙결 적용
    public int PoisonStacksToApply { get; protected set; } = 0; // 중독 적용
    public int BurnStacksToApply { get; protected set; } = 0; // 화상 적용
    public float HasteDurationToApply { get; protected set; } = 0f; // 가속 적용 (지속시간)
    public float SlowDurationToApply { get; protected set; } = 0f;  // 감속 적용 (지속시간)
    public float BaseCooldownReduction { get; protected set; } = 0f; // 촉진 스탯 (지속시간)
    public float BaseCooldownIncrease { get; protected set; } = 0f; // 방해 스탯 (지속시간)
    public int EchoStacksToApply { get; protected set; } = 0;     // 메아리 적용 (횟수)
    public float ShockDurationToApply { get; protected set; } = 0f;    // 충격 적용 (지속시간)
    public float SturdyDurationToApply { get; protected set; } = 0f;   // 견고 적용 (지속시간)


    public virtual int GetCurrentBleedStacks() { return this.BleedStacksToApply; }
    public virtual float GetCurrentFreezeDuration() { return this.FreezeDurationToApply; }
    public virtual int GetCurrentPoisonStacks() { return this.PoisonStacksToApply; }
    public virtual int GetCurrentBurnStacks() { return this.BurnStacksToApply; }
    public virtual int GetCurrentEchoStacks() { return this.EchoStacksToApply; }
    public virtual float GetCurrentShockDuration() { return this.ShockDurationToApply; }
    public virtual float GetCurrentSturdyDuration() { return this.SturdyDurationToApply; }
    public virtual float GetCurrentPolymorphDuration() { return this.PolymorphDurationToApply; }
    public virtual float GetCurrentTargetShuffle() { return this.TriggersTargetShuffle; }

    // --- [쿨타임 컨트롤] ---

    public virtual float GetCurrentCooldownTime()
    {
        return this.BaseCooldownTime;
    }
    public virtual float GetCurrentHasteDuration()
    {
        return this.HasteDurationToApply;
    }
    public virtual float GetCurrentSlowDuration()
    {
        return this.SlowDurationToApply;
    }
    public virtual float GetCurrentCooldownReduction() { return this.BaseCooldownReduction; }
    public virtual float GetCurrentCooldownIncrease() { return this.BaseCooldownIncrease; }

    private bool m_IsHasted = false; // 가속 여부
    private float m_HasteTimer = 0f; // 가속 타이머
    private bool m_IsSlowed = false; // 감속 여부
    private float m_SlowTimer = 0f; // 감속 타이머

    public bool IsHasted()
    {
        return m_IsHasted;
    }
    public bool IsSlowed()
    {
        return m_IsSlowed;
    }


    // --- [상태 이상 컨트롤] ---
    public List<StatusEffectType> Immunities { get; protected set; } = new List<StatusEffectType>(); // 면역
    private bool m_IsFrozen = false; // 빙결 여부
    private float m_FreezeTimer = 0f; // 빙결 타이머
    

    public bool IsFrozen()
    {
        return m_IsFrozen;
    }
   
    public virtual void ClearBattleFrozen() // 어디서 쓰는지 몰라서 일단 남겨둠
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

        PriceInflateAmount = 0;
        PriceExtortAmount = 0;
    }

    public virtual void ClearBattleStatBuffs()
    {
    }

    // --- [가격 컨트롤] ---
    // 초기 가격 설정
    protected void SetInitPrice(int price)
    {
        this.CardPrice = price;
        this.BasePrice = price;
    }

    // 카드의 가격을 변경합니다.
    public void ChangePrice(int amount)
    {
        this.CardPrice += amount;

        // 가격은 0보다 작아질 수 없게 막음
        if (this.CardPrice < 0) this.CardPrice = 0;
    }

    // 카드 가격 리셋
    public void ResetPrice()
    {
        this.CardPrice = this.BasePrice;
    }

    // 현재 가격 반환
    public virtual int GetCurrentPrice()
    {
        int finalPrice = this.CardPrice;

        finalPrice += PriceInflateAmount;
        finalPrice -= PriceExtortAmount;

        // 0 이하 방지
        return Mathf.Max(0, finalPrice);
    }

    public virtual int GetCurrentPriceInflate() { return this.PriceInflateAmount; }
    public virtual int GetCurrentPriceExtort() { return this.PriceExtortAmount; }

    // --- 2. 생성자 (카드 처음 생성 시) ---
    /// 새 카드를 생성할 때 호출됩니다.
    public Card(object owner, int index, float cooldown)
    {
        this.m_Owner = owner;
        this.SlotIndex = index;
        this.BaseCooldownTime = cooldown;
        this.CurrentCooldown = GetCurrentCooldownTime(); 
        this.CardNameKey = "Default Card Name";
        this.OriginalSlotIndex = index;
    }
    // 현재 위치 업데이트
    public void SetSlotIndex(int index)
    {
        this.SlotIndex = index;
    }
    // 카드가 특정 Tag를 가지고 있는지 확인
    public bool HasTagKey(string tagKey)
    {
        return TagKeys.Contains(tagKey);
    }

    // --- 3. 핵심 함수 ---
    // 카드의 고유 스킬 로직 : 이 클래스를 상속받는 모든 카드는, 이 함수의 내용물을 반드시' 자신만의 로직으로 override 돼야 함.
    public abstract void ExecuteSkill();

    // 스킬 반복 횟수 계산
    public virtual void TriggerSkill()
    {
        int totalCasts = this.InnateEchoCount + this.m_BonusEchoStacks; // 1.시전 횟수 체크

        // 보너스 스택 즉시 소모
        this.m_BonusEchoStacks = 0;

        // 횟수만큼 ExecuteSkill() 호출
        for (int i = 0; i < totalCasts; i++)
        {
            ExecuteSkill();
        }

        // 양쪽 연쇄 발동 로직 : TriggersChainCount가 1이면 양쪽 1칸씩
        if (TriggersChainCount > 0)
        {
            if (TriggersChainCount > 0)
            {
                Card neighborRight = null;
                Card neighborLeft = null;

                if (m_Owner is PlayerController playerOwner)
                {
                    // PlayerController의 GetRight/LeftNeighbor 호출
                    neighborRight = playerOwner.GetRightNeighbor(this.SlotIndex);
                    neighborLeft = playerOwner.GetLeftNeighbor(this.SlotIndex);
                }
                else if (m_Owner is MonsterController monsterOwner)
                {
                    // MonsterController의 GetRight/LeftNeighbor 호출
                    neighborRight = monsterOwner.GetRightNeighbor(this.SlotIndex);
                    neighborLeft = monsterOwner.GetLeftNeighbor(this.SlotIndex);
                }

                // 연쇄 시도
                TryChainTrigger(neighborRight);
                TryChainTrigger(neighborLeft);
            }
        }

        float excessAmount = (CurrentCooldown <= 0f) ? -CurrentCooldown : 0f; // 남은 쿨타임 초과분 체크
        this.CurrentCooldown = this.GetCurrentCooldownTime() - excessAmount; // 쿨타임 초기화
        
        ConsumeDurability(); // 내구도 소모
    }

    private void TryChainTrigger(Card neighbor)
    {
        if (neighbor != null && neighbor.CurrentCooldown <= 0f)
        {
            neighbor.TriggerSkill(); // 즉발
        }
    }

    // 크리티컬 확인
    protected float CheckForCrit()
    {
        float currentCritChance = GetCurrentCritChance();
        if (currentCritChance <= 0) return 1.0f;

        if (Random.Range(0f, 1.0f) < currentCritChance)
        {
            Debug.Log($"[{this.CardNameKey}] 치명타 발동!");
            return 2.0f; // 2배
        }
        return 1.0f; // 1배
    }

    // 인접 버프 (Aura) 확인
    public virtual float GetAuraBuffTo(Card recipient, string buffType)
    {
        // 기본적으로, 모든 카드는 아무 아우라도 주지 않는다.
        return 0f;
    }

    // 죽음의 메아리 확인
    public virtual void OnDestroyed()
    {
        // 기본적으로, 대부분의 카드는 파괴될 때 아무 일도 하지 않는다.
    }

    // 쿨타임 줄여주는 함수 : 매 프레임 호출
    public virtual void UpdateCooldown(float deltaTime)
    {
        // 0. 변이 체크
        if (IsPolymorphed)
        {
            PolymorphTimer -= deltaTime;
            if (PolymorphTimer <= 0f)
            {
                // 시간 다 되면 복구
                if (m_Owner is PlayerController player) player.RevertMutation(this.SlotIndex);
                else if (m_Owner is MonsterController monster) monster.RevertMutation(this.SlotIndex);

                return;
            }
        }

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
    public virtual bool ApplyStatusEffect(StatusEffectType effectType, float duration, string extraData = "")
    {
        // 면역 체크
        if (Immunities.Contains(effectType))
        {
            Debug.Log($"[{this.CardNameKey}] (은)는 '{effectType}' 효과에 면역입니다!");
            return false; // 적용 실패!
        }

        // 2. 면역이 아니면, 실제 효과 적용
        switch (effectType)
        {
            case StatusEffectType.Freeze:
                m_IsFrozen = true;
                m_FreezeTimer = Mathf.Max(m_FreezeTimer, duration); // 더 긴 시간으로 갱신
                Debug.Log($"[{this.CardNameKey}] (이)가 {duration}초간 빙결되었습니다!");
                break;

            case StatusEffectType.Haste:
                m_IsHasted = true;
                m_HasteTimer = Mathf.Max(m_HasteTimer, duration);
                m_IsSlowed = false; // 감속 해제
                m_SlowTimer = 0f;
                Debug.Log($"[{this.CardNameKey}] (이)가 {duration}초간 가속되었습니다!");
                break;

            case StatusEffectType.Slow:
                m_IsSlowed = true;
                m_SlowTimer = Mathf.Max(m_SlowTimer, duration);
                m_IsHasted = false; // 가속 해제
                m_HasteTimer = 0f;
                Debug.Log($"[{this.CardNameKey}] (이)가 {duration}초간 감속되었습니다!");
                break;

            case StatusEffectType.Echo:
                // duration을 추가 시전 횟수로 사용
                m_BonusEchoStacks += (int)duration;
                Debug.Log($"[{this.CardNameKey}] (이)가 '메아리' {duration}스택을 얻었습니다!");
                break;

            case StatusEffectType.PriceInflate:
                PriceInflateAmount += (int)duration;
                Debug.Log($"[{CardNameKey}] 가격 인상! (+{(int)duration})");
                break;

            case StatusEffectType.PriceExtort:
                PriceExtortAmount += (int)duration;
                Debug.Log($"[{CardNameKey}] 가격 인하! (-{(int)duration})");
                break;

            case StatusEffectType.Polymorph:

                // 변이할 대상 ID 결정 : extraData가 있으면 그걸 쓰고, 없으면 기본값 "card_sheep" 사용
                string targetID = string.IsNullOrEmpty(extraData) ? "card_sheep" : extraData;

                // Controller에게 구체적인 ID로 변이 요청
                if (m_Owner is PlayerController player)
                {
                    player.MutateCard(this.SlotIndex, targetID, duration);
                }
                else if (m_Owner is MonsterController monster)
                {
                    monster.MutateCard(this.SlotIndex, targetID, duration);
                }
                break;
        }
        return true;
    }

    /// 쿨타임 즉시 감소 (초과분 적용)
    public virtual void ReduceCooldown(float amount)
    {
        CurrentCooldown -= amount;
        //if (CurrentCooldown <= 0f)
        //{
        //    float excessAmount = -CurrentCooldown; // 초과분 (예: 1.5f)

        //    // TriggerSkill()이 아니라 ExecuteSkill()을 직접 호출해야
        //    ExecuteSkill();
        //    CurrentCooldown = GetCurrentCooldownTime() - excessAmount;
        //}
    }

    //쿨타임 즉시 증가
    public virtual void IncreaseCooldown(float amount)
    {
        if (!m_IsFrozen)
        {
            CurrentCooldown += amount;
        }
    }

    // 내구도 시스템
    protected virtual void ConsumeDurability()
    {
        // 내구도 무한
        if (this.Durability == -1) return;

        this.Durability--;

        if (this.Durability <= 0)
        {
            // 파괴
            if (m_Owner is PlayerController playerOwner)
            {
                playerOwner.DestroyCard(this.SlotIndex);
            }
            else if (m_Owner is MonsterController monsterOwner)
            {
                monsterOwner.DestroyCard(this.SlotIndex);
            }
        }
    }
}