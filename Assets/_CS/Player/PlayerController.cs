using UnityEngine;
using UnityEngine.UIElements; // UI Toolkit 사용
using System.Collections.Generic; // List 사용

// 모든 플레이어 영주(Lord)의 공통 부모가 되는 기본 클래스
// 체력, 슬롯, 카드 덱 등 공통 기능만 관리
// 영주만의 특수한 로직은 포함하지 않고, 따로 관리

public class PlayerController
{
    // --- 1. 참조 변수 ---

    /// 전투의 규칙을 관리
    /// protected: 이 클래스를 상속받는 자식 클래스(영주)도 접근 가능
    protected BattleManager m_BattleManager;

    /// 내가 공격해야 할 '타겟' (몬스터 컨트롤러)
    protected MonsterController m_Target;

    // --- 2. UI 요소 ---
    protected VisualElement m_PlayerParty; // 7칸 카드 슬롯 패널
    protected VisualElement m_StatusPanel; // 상태 패널 UI
    // 7개의 카드 슬롯 UI 요소 리스트
    public List<VisualElement> Slots { get; protected set; } = new List<VisualElement>(7);

    // 상태 패널 요소
    private VisualElement m_HealthBarFill;
    private VisualElement m_ShieldBarFill;
    private Label m_HealthLabel;
    private Label m_ShieldLabel;
    private Label m_LevelLabel;
    // 10칸 네모 XP 바를 위한 리스트
    private List<VisualElement> m_XPTicks = new List<VisualElement>(10);

    // DoT 도트 대미지 아이콘 UI 라벨
    private Label m_BleedStatusLabel;
    private Label m_PoisonStatusLabel;
    private Label m_BurnStatusLabel;

    // --- 3. 핵심 상태 (공통) ---
    public float CurrentHP { get; protected set; } /// 영주의 현재 체력
    public float MaxHP { get; protected set; } /// 영주의 현재 체력
    public float CurrentShield { get; protected set; } /// 영주의 현재 쉴드
    
    public int CurrentLevel { get; protected set; } = 1; /// 현재 레벨
    public int CurrentXP { get; protected set; } = 0; /// 현재 경험치 
    public int MaxXP { get; protected set; } = 10; /// 최대 경험치

    // DoT 중첩 변수
    public int BleedStacks { get; protected set; } = 0;
    public int PoisonStacks { get; protected set; } = 0;
    public int BurnStacks { get; protected set; } = 0;

    // DoT 데미지 타이머 (개별 관리)
    private float m_BleedTickTimer = 1.5f;   // 출혈 : 1.5초 
    private float m_PoisonTickTimer = 3.0f;  // 중독 : 3초 
    private float m_BurnTickTimer = 0.5f;    // 화상 : 0.5초


    // --- 4. 카드 덱 관리 ---
    /// 이 영주가 현재 전투에서 사용하는 7칸의 카드 배열
    protected Card[] m_Cards = new Card[7];


    // --- 5. 생성자 ---
    /// PlayerController가 처음 생성될 때 호출됩니다.
    /// <param name="manager">나를 관리할 BattleManager</param>
    /// <param name="VisualElement">내가 제어할 UXML의 'PlayerParty' 패널</param>
    /// <param name="maxHP">이 영주의 최대 체력</param>
    /// 
    public PlayerController(BattleManager manager, VisualElement VisualElement, float maxHP)
    {
        this.m_BattleManager = manager;
        this.MaxHP = maxHP;
        this.CurrentHP = maxHP;
        this.CurrentShield = 0;

        this.m_PlayerParty = VisualElement.Q<VisualElement>("PlayerParty");
        this.m_StatusPanel = VisualElement.Q<VisualElement>("PlayerStatus");

        // 2. 카드 슬롯(CardSlot1 ~ CardSlot7) 찾기
        Slots.Clear();
        for (int i = 0; i < 7; i++)
        {
            string slotName = "CardSlot" + (i + 1);
            Slots.Add(m_PlayerParty.Q<VisualElement>(slotName));
        }

        // 3. 상태 UI 요소들 찾기
        if (m_StatusPanel != null)
        {
            m_HealthBarFill = m_StatusPanel.Q<VisualElement>("HP-Bar-Fill"); // UXML 이름 확인!
            m_HealthLabel = m_StatusPanel.Q<Label>("HP-label"); // UXML 이름 확인!
            m_ShieldBarFill = m_StatusPanel.Q<VisualElement>("Shield-Bar-Fill"); // UXML 이름 확인!
            m_ShieldLabel = m_StatusPanel.Q<Label>("Shield-label"); // UXML 이름 확인!
            m_LevelLabel = m_StatusPanel.Q<Label>("LV-label"); // UXML 이름 확인!

            m_XPTicks.Clear();
            for (int i = 0; i < 10; i++)
            {
                m_XPTicks.Add(m_StatusPanel.Q<VisualElement>("XPTick" + i));
            }

            //DoT 아이콘 라벨 찾기
            m_BleedStatusLabel = m_StatusPanel.Q<Label>("BleedStatus");
            m_PoisonStatusLabel = m_StatusPanel.Q<Label>("PoisonStatus");
            m_BurnStatusLabel = m_StatusPanel.Q<Label>("BurnStatus");
        }
        else
        {
            Debug.LogError("[PlayerController] 'PlayerStatusPanel'을 UXML에서 찾을 수 없습니다!");
        }

        // 4. UI를 현재 상태로 즉시 업데이트
        UpdateHealthUI();
        UpdateXPUI();
        UpdateDoTUI();
    }

    // --- 6. 핵심 함수 ---
    /// BattleManager가 호출해주는, 이 컨트롤러의 매 프레임 업데이트 함수
    /// <param name="deltaTime">Time.deltaTime (프레임당 시간)</param>

    public virtual void BattleUpdate(float deltaTime)
    {
        // (공통) 내 카드들의 쿨타임 회전 및 스킬을 발동
        for (int i = 0; i < 7; i++)
        {
            if (m_Cards[i] != null) // 해당 슬롯에 카드가 있다면
            {
                // 카드가 스스로 쿨타임을 줄이도록 함
                m_Cards[i].UpdateCooldown(deltaTime);

                // 쿨타임이 0 이하가 되면 스킬 발동
                if (m_Cards[i].CurrentCooldown <= 0f)
                {
                    m_Cards[i].ExecuteSkill(); // 카드가 알아서 스킬을 씁니다.
                    m_Cards[i].CurrentCooldown = m_Cards[i].CooldownTime; // 쿨타임 초기화
                }
            }
        }
        // DoT 데미지 타이머 돌리기
        ProcessDoTs(deltaTime);
    }

    /// (공통) 몬스터가 나를 공격할 때 호출하는 함수
    /// <param name="amount">받는 피해량</param>

    public virtual void TakeDamage(float amount)
    {
        float damageRemaining = amount;

        if (CurrentShield > 0)
        {
            if (damageRemaining >= CurrentShield)
            {
                damageRemaining -= CurrentShield;
                CurrentShield = 0;
            }
            else
            {
                CurrentShield -= damageRemaining;
                damageRemaining = 0;
            }
        }

        if (damageRemaining > 0)
        {
            CurrentHP -= damageRemaining;
        }

        Debug.Log($"[플레이어] {amount} 피해 받음! (쉴드 {CurrentShield} 남음, 체력 {CurrentHP} 남음)");

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            m_BattleManager.EndBattle("Monster"); // 패배!
        }

        UpdateHealthUI();
    }

    // 쉴드 추가
    public virtual void AddShield(float amount)
    {
        CurrentShield += amount;
        Debug.Log($"[플레이어] 쉴드 {amount} 획득! (총 쉴드: {CurrentShield})");
        UpdateHealthUI();
    }

    // 명성/경험치 획득 함수
    public virtual void AddExperience(int amount)
    {
        CurrentXP += amount;
        Debug.Log($"[플레이어] 경험치 {amount} 획득! (총 XP: {CurrentXP}/{MaxXP})");

        //10칸이 다 차면 레벨업 
        while (CurrentXP >= MaxXP)
        {
            CurrentLevel++;
            CurrentXP -= MaxXP; // 초과분

            MaxHP += 10; // 레벨업 시 최대 체력 증가
            CurrentHP = MaxHP; // 레벨업 시 체력 회복)
                               // MaxXP = 10; 

            Debug.LogWarning($"[플레이어] 레벨 업! {CurrentLevel} 레벨 달성! (남은 XP: {CurrentXP})");
        }

        UpdateHealthUI(); // 체력 회복 UI 업데이트
        UpdateXPUI();
    }

    // (공통) 나의 '타겟'(몬스터)이 누구인지 알려주는 함수.
    public MonsterController GetTarget()
    {
        return m_Target;
    }

    // (공통) BattleManager가 나의 '타겟'을 지정해주는 함수.
    public void SetTarget(MonsterController target)
    {
        this.m_Target = target;
    }

    // --- 7. 위치 호출 함수 ---
    /// 내 덱(m_Cards)의 특정 인덱스에 있는 카드 반환
    /// (범위를 벗어나면 null을 반환)
    
    public Card GetCardAtIndex(int index)
    {
        if (index >= 0 && index < 7 && m_Cards[index] != null)
        {
            return m_Cards[index];
        }
        return null;
    }

    /// [인접-왼쪽] "나의 왼쪽"에 있는 카드를 반환
    public Card GetLeftNeighbor(int myIndex)
    {
        return GetCardAtIndex(myIndex - 1);
    }

    /// [인접-오른쪽] "나의 오른쪽"에 있는 카드를 반환
    public Card GetRightNeighbor(int myIndex)
    {
        return GetCardAtIndex(myIndex + 1);
    }

    /// [상대 위치] "나의 맞은편"에 있는 몬스터 카드 반환
    public Card GetOppositeCard(int myIndex)
    {
        if (m_Target != null)
        {
            return m_Target.GetCardAtIndex(myIndex);
        }
        return null;
    }

    // --- 8. 상태 이상 헬퍼 함수 ---
    /// [핵심] 내 카드 중 '면역이 아닌' 무작위 카드 N개에 상태 이상을 적용
    public void ApplyStatusToRandomCards(int count, StatusEffectType effectType, float duration)
    {
        // 1) 0~6번 슬롯 인덱스가 담긴 리스트 생성
        List<int> slotIndices = new List<int> { 0, 1, 2, 3, 4, 5, 6 };

        // 2) 리스트 무작위 셔플
        for (int i = 0; i < slotIndices.Count; i++)
        {
            int temp = slotIndices[i];
            int randomIndex = Random.Range(i, slotIndices.Count);
            slotIndices[i] = slotIndices[randomIndex];
            slotIndices[randomIndex] = temp;
        }

        // 3) 적용에 성공한 횟수를 카운팅
        int successCount = 0;

        // 4) 무작위로 섞인 슬롯 순서대로 확인
        foreach (int index in slotIndices)
        {
            Card card = GetCardAtIndex(index); // 

            // 5) 슬롯이 비어있지 않은지 확인
            if (card != null)
            {
                // 6) 카드에게 효과 적용을 시도
                // (이 코드가 작동하려면 Card.cs에 ApplyStatusEffect 함수가 있어야 합니다!)
                if (card.ApplyStatusEffect(effectType, duration))
                {
                    // 7) 적용에 성공했으면, 카운트를 1 올립니다.
                    successCount++;
                }
            }

            // 8) 목표한 횟수(count)만큼 성공했으면, 즉시 종료합니다.
            if (successCount >= count)
            {
                break;
            }
        }
    }

    /// <summary>
    /// [신규!] (카드들이 호출) 이 영주에게 DoT 중첩을 추가합니다.
    /// </summary>
    public virtual void ApplyLordDoT(StatusEffectType effectType, int stacks)
    {
        switch (effectType)
        {
            case StatusEffectType.Bleed:
                BleedStacks += stacks;
                break;
            case StatusEffectType.Poison:
                PoisonStacks += stacks;
                break;
            case StatusEffectType.Burn:
                BurnStacks += stacks;
                break;
        }
        UpdateDoTUI(); // UI 업데이트
    }

    /// <summary>
    /// [신규!] 특정 DoT 중첩을 '일정 수치(정수)'만큼 감소시킵니다.
    /// </summary>
    public virtual void ReduceLordDoT(StatusEffectType effectType, int amount)
    {
        switch (effectType)
        {
            case StatusEffectType.Bleed:
                BleedStacks = Mathf.Max(0, BleedStacks - amount);
                break;
            case StatusEffectType.Poison:
                PoisonStacks = Mathf.Max(0, PoisonStacks - amount);
                break;
            case StatusEffectType.Burn:
                BurnStacks = Mathf.Max(0, BurnStacks - amount);
                break;
        }
        UpdateDoTUI(); // UI 업데이트
    }

    /// <summary>
    /// [신규!] 특정 DoT 중첩을 '일정 퍼센트(%)'만큼 감소시킵니다.
    /// </summary>
    public virtual void ReduceLordDoTPercent(StatusEffectType effectType, float percent)
    {
        float clampedPercent = Mathf.Clamp01(percent);
        switch (effectType)
        {
            case StatusEffectType.Bleed:
                BleedStacks = Mathf.FloorToInt(BleedStacks * (1.0f - clampedPercent));
                break;
            case StatusEffectType.Poison:
                PoisonStacks = Mathf.FloorToInt(PoisonStacks * (1.0f - clampedPercent));
                break;
            case StatusEffectType.Burn:
                BurnStacks = Mathf.FloorToInt(BurnStacks * (1.0f - clampedPercent));
                break;
        }
        UpdateDoTUI(); // UI 업데이트
    }

    // --- 9. DoT 데미지 처리---
    // '개별' 타이머로 DoT 데미지를 계산 및 적용
    private void ProcessDoTs(float deltaTime)
    {
        // (1) 출혈 (1.5초 틱, 쉴드부터 깎음)
        if (BleedStacks > 0)
        {
            m_BleedTickTimer -= deltaTime;
            if (m_BleedTickTimer <= 0f)
            {
                float damage = BleedStacks * 1; // (스택당 1데미지)
                TakeDamage(damage);
                Debug.Log($"[DoT] 출혈로 {damage} 피해!");
                m_BleedTickTimer = 1.5f; // 1.5초 타이머 초기화
            }
        }

        // (2) 중독 (3초 틱, 쉴드 무시)
        if (PoisonStacks > 0)
        {
            m_PoisonTickTimer -= deltaTime;
            if (m_PoisonTickTimer <= 0f)
            {
                float damage = PoisonStacks * 1;
                CurrentHP -= damage; // 쉴드를 무시하고 체력에 직접 피해
                Debug.Log($"[DoT] 중독으로 {damage} 피해! (쉴드 무시)");
                UpdateHealthUI(); // 체력이 바로 바뀌었으니 UI 업데이트
                m_PoisonTickTimer = 3.0f; // 3초 타이머 초기화
            }
        }

        // (3) 화상 (0.5초 틱, 쉴드부터 깎음, 1스택 '고갈')
        if (BurnStacks > 0)
        {
            m_BurnTickTimer -= deltaTime;
            if (m_BurnTickTimer <= 0f)
            {
                float damage = BurnStacks * 1; // (예: 틱당 1데미지)
                TakeDamage(damage);
                Debug.Log($"[DoT] 화상으로 {damage} 피해!");

                // 피해를 입힌 후 1스택 감소
                BurnStacks -= 1;
                UpdateDoTUI(); // 스택이 바뀌었으니 UI 업데이트

                m_BurnTickTimer = 0.5f; // 0.5초 타이머 초기화
            }
        }
    }

    // --- 10. UI 업데이트 함수 ---
    protected virtual void UpdateHealthUI()
    {
        // 체력 바
        if (m_HealthBarFill != null)
        {
            float healthPercent = (MaxHP > 0) ? (CurrentHP / MaxHP) : 0f;
            m_HealthBarFill.style.width = Length.Percent(healthPercent * 100f);
        }

        // 체력 텍스트(Label)
        if (m_HealthLabel != null)
        {
            m_HealthLabel.text = $"HP: {Mathf.CeilToInt(CurrentHP)} / {MaxHP}";
        }

        // 쉴드 텍스트(Label)
        if (m_ShieldLabel != null)
        {
            m_ShieldLabel.text = $"SHIELD: {Mathf.CeilToInt(CurrentShield)}";
        }

        // 쉴드 바(Fill)
        if (m_ShieldBarFill != null)
        {
            if (CurrentShield > 0)
            {
                m_ShieldBarFill.style.display = DisplayStyle.Flex;
                float shieldPercent = Mathf.Clamp(CurrentShield / MaxHP, 0, 1f);
                m_ShieldBarFill.style.width = Length.Percent(shieldPercent * 100f);
            }
            else
            {
                m_ShieldBarFill.style.display = DisplayStyle.None;
            }
        }
    }

    // 레벨/경험치(10칸 네모) UI를 업데이트
    protected virtual void UpdateXPUI()
    {
        // 레벨 텍스트(Label)
        if (m_LevelLabel != null)
        {
            m_LevelLabel.text = $"LV. {CurrentLevel}";
        }

        // 10칸 네모(Ticks) UI 업데이트
        int filledTicks = CurrentXP;

        for (int i = 0; i < 10; i++)
        {
            if (m_XPTicks[i] != null)
            {
                if (i < filledTicks)
                {
                    // (i)번 칸이 채워져야 함 (예: 0, 1, 2)
                    m_XPTicks[i].AddToClassList("xp-tick-filled");
                }
                else
                {
                    // (i)번 칸이 비어있어야 함
                    m_XPTicks[i].RemoveFromClassList("xp-tick-filled");
                }
            }
        }
    }

    // DoT 중첩을 UI 라벨에 업데이트하고, 0이면 숨김.
    protected virtual void UpdateDoTUI()
    {
        // 출혈 UI
        if (m_BleedStatusLabel != null)
        {
            if (BleedStacks > 0)
            {
                m_BleedStatusLabel.text = $"BLEED : {BleedStacks}";
                m_BleedStatusLabel.style.display = DisplayStyle.Flex; // 보이기
            }
            else
            {
                m_BleedStatusLabel.style.display = DisplayStyle.None; // 숨기기
            }
        }

        // 중독 UI
        if (m_PoisonStatusLabel != null)
        {
            if (PoisonStacks > 0)
            {
                m_PoisonStatusLabel.text = $"POISON : {PoisonStacks}";
                m_PoisonStatusLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_PoisonStatusLabel.style.display = DisplayStyle.None;
            }
        }

        // 화상 UI
        if (m_BurnStatusLabel != null)
        {
            if (BurnStacks > 0)
            {
                m_BurnStatusLabel.text = $"BURN : {BurnStacks}";
                m_BurnStatusLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_BurnStatusLabel.style.display = DisplayStyle.None;
            }
        }
    }

    // -------------------------- 프로토타입용 덱 설정 함수 ---------------------------------
    // --------------------------------------------------------------------------------------

    /// <param name="cardNames">덱에 넣을 카드 이름 목록 (예: "이름")</param>

    public virtual void SetupDeck(string[] cardNames)
    {
        // 기본 PlayerController는 덱이 비어있습니다.
        // 자식 클래스에서 이 부분을 채워야 합니다.
        Debug.Log("기본 PlayerController는 덱을 설정할 수 없습니다.");
    }
}