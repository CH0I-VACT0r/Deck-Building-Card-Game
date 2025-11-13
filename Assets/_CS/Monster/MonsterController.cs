using UnityEngine;
using UnityEngine.UIElements; // UI Toolkit 사용
using System.Collections.Generic; // List 사용


/// 모든 몬스터(일반, 보스)의 공통 부모가 되는 기본 클래스
/// 몬스터의 체력, 슬롯, 카드 덱 등 공통 기능만 관리

public class MonsterController
{
    // --- 1. 참조 변수 ---
    /// 전투 규칙 관리
    protected BattleManager m_BattleManager;
    /// 내가 공격해야 할 타겟 (플레이어)
    protected PlayerController m_Target;

    // --- 2. UI 요소 ---
    protected VisualElement m_MonsterParty; // UXML의 MonsterParty 패널
    protected VisualElement m_StatusPanel; // 몬스터 상태 패널 UI
    public List<VisualElement> Slots { get; protected set; } = new List<VisualElement>(7);

    // 체력 및 쉴드 UI 요소
    private VisualElement m_HealthBarFill;
    private VisualElement m_ShieldBarFill;
    private Label m_HealthLabel;
    private Label m_ShieldLabel;
    private Label m_NameLabel; // (몬스터 이름 라벨)

    // DoT 도트 대미지 아이콘 UI 라벨
    private Label m_BleedStatusLabel;
    private Label m_PoisonStatusLabel;
    private Label m_BurnStatusLabel;

    // --- 3. 핵심 상태 (공통) ---
    public float CurrentHP { get; protected set; } // 몬스터 현재 체력
    public float MaxHP { get; protected set; } // 몬스터 전체 체력
    public float CurrentShield { get; protected set; } // 몬스터 쉴드

    // DoT 중첩 변수
    public int BleedStacks { get; protected set; } = 0;
    public int PoisonStacks { get; protected set; } = 0;
    public int BurnStacks { get; protected set; } = 0;

    // DoT 데미지 타이머 (개별 관리)
    private float m_BleedTickTimer = 1.5f;   // 출혈 :1.5초
    private float m_PoisonTickTimer = 3.0f;  // 중독 : 3초
    private float m_BurnTickTimer = 0.5f;    // 화상 : 0.5초

    // --- 4. 카드 덱 관리 ---
    /// 이 몬스터가 현재 전투에서 사용하는 7칸의 카드 배열
    protected Card[] m_Cards = new Card[7];


    // --- 5. 생성자 ---
    /// MonsterController가 처음 생성될 때 호출됩니다.
    /// <param name="manager">나를 관리할 BattleManager</param>
    /// <param name="VisualElement">내가 제어할 UXML의 'MonsterParty' 패널</param>
    /// <param name="maxHP">이 몬스터의 최대 체력</param>
    /// 
    public MonsterController(BattleManager manager, VisualElement VisualElement, float maxHP)
    {
        this.m_BattleManager = manager;
        this.MaxHP = maxHP;
        this.CurrentHP = maxHP;
        this.CurrentShield = 0;

        // 1) UXML 패널 이름
        this.m_MonsterParty = VisualElement.Q<VisualElement>("MonsterParty");
        this.m_StatusPanel = VisualElement.Q<VisualElement>("MonsterStatus"); // UXML Name 확인!

        // 2) 카드 슬롯(MonSlot1 ~ MonSlot7)을 찾아 리스트에 추가합니다.
        Slots.Clear();
        for (int i = 0; i < 7; i++)
        {
            string slotName = "MonSlot" + (i + 1);
            Slots.Add(m_MonsterParty.Q<VisualElement>(slotName));
        }

        // 3. [신규!] 상태 UI 요소들을 찾아서 변수에 연결합니다.
        if (m_StatusPanel != null)
        {
            // (UXML에 정의된 'Name'과 일치해야 합니다)
            m_HealthBarFill = m_StatusPanel.Q<VisualElement>("Monster-HP-fill");
            m_HealthLabel = m_StatusPanel.Q<Label>("Monster-HP-label");
            m_ShieldBarFill = m_StatusPanel.Q<VisualElement>("Monster-ShieldBar-fill");
            m_ShieldLabel = m_StatusPanel.Q<Label>("Monster-ShieldBar-label");
            m_NameLabel = m_StatusPanel.Q<Label>("MonsterName-label");

            m_BleedStatusLabel = m_StatusPanel.Q<Label>("MonsterBleedStatus");
            m_PoisonStatusLabel = m_StatusPanel.Q<Label>("MonsterPoisonStatus");
            m_BurnStatusLabel = m_StatusPanel.Q<Label>("MonsterBurnStatus");
        }
        else
        {
            Debug.LogError("[MonsterController] 'MonsterStatusPanel'을 UXML에서 찾을 수 없습니다!");
        }

        // 4. [신규!] UI를 현재 상태로 즉시 업데이트합니다.
        UpdateHealthUI();
        if (m_NameLabel != null)
        {
            m_NameLabel.text = "Tutorial (Lv.1)"; // (임시 이름)
        }
    }

    // --- 6. 핵심 함수 ---
    /// BattleManager가 호출해주는, 이 컨트롤러의 매 프레임 업데이트 함수
    /// <param name="deltaTime">Time.deltaTime (프레임당 시간)</param>
  
    public virtual void BattleUpdate(float deltaTime)
    {
        // 1. (공통) 몬스터 카드들의 쿨타임 회전 및 스킬 발동
        for (int i = 0; i < 7; i++)
        {
            if (m_Cards[i] != null) // 해당 슬롯에 카드가 있다면
            {
                m_Cards[i].UpdateCooldown(deltaTime);

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

    /// (공통) 플레이어가 나를 공격할 때 호출하는 함수
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

        Debug.LogWarning($"[몬스터] {amount} 피해 받음! (쉴드 {CurrentShield} 남음, 체력 {CurrentHP} 남음)");

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            m_BattleManager.EndBattle("Player"); // 승리!
        }

        UpdateHealthUI();
    }

    // 몬스터 쉴드 생성 함수
    public virtual void AddShield(float amount)
    {
        CurrentShield += amount;
        Debug.LogWarning($"[몬스터] 쉴드 {amount} 획득! (총 쉴드: {CurrentShield})");
        UpdateHealthUI();
    }

    /// (공통) 나의 '타겟'(플레이어)이 누구인지 알려주는 함수.
    /// 몬스터 카드들이 이 함수를 호출하여 플레이어 공격
    public PlayerController GetTarget()
    {
        return m_Target;
    }

    /// (공통) BattleManager가 나의 '타겟'을 지정해주는 함수.
    public void SetTarget(PlayerController target)
    {
        this.m_Target = target;
    }

    // --- 7. 위치 기반 헬퍼 함수  ---
    /// [헬퍼] 내 덱(m_Cards)의 특정 인덱스에 있는 카드를 반환
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

    /// [상대 위치] "나의 맞은편"에 있는 플레이어 카드를 반환합니다.
    public Card GetOppositeCard(int myIndex)
    {
        if (m_Target != null)
        {
            return m_Target.GetCardAtIndex(myIndex);
        }
        return null;
    }

    // --- 8. 상태 이상 헬퍼 함수 ---
    /// 내 카드 중 '면역이 아닌' 무작위 카드 N개에 상태 이상을 적용합니다.
    /// (플레이어의 '빙결' 스킬 등이 이 함수를 호출합니다.)
    public void ApplyStatusToRandomCards(int count, StatusEffectType effectType, float duration)
    {
        // 1) 0~6번 슬롯 인덱스가 담긴 리스트를 만듭니다.
        List<int> slotIndices = new List<int> { 0, 1, 2, 3, 4, 5, 6 };

        // 2) 리스트를 무작위로 섞습니다 (Fisher-Yates Shuffle).
        for (int i = 0; i < slotIndices.Count; i++)
        {
            int temp = slotIndices[i];
            int randomIndex = Random.Range(i, slotIndices.Count);
            slotIndices[i] = slotIndices[randomIndex];
            slotIndices[randomIndex] = temp;
        }

        // 3) 적용에 성공한 횟수를 셉니다.
        int successCount = 0;

        // 4) 무작위로 섞인 슬롯 순서대로 확인합니다.
        foreach (int index in slotIndices)
        {
            Card card = GetCardAtIndex(index); // (방금 위에 추가한 헬퍼 함수)

            // 5) 슬롯이 비어있지 않은지 확인
            if (card != null)
            {
                // 6) 카드에게 효과 적용 시도
                // (이 코드가 작동하려면 Card.cs에 ApplyStatusEffect 함수가 있어야 합니다!)
                if (card.ApplyStatusEffect(effectType, duration))
                {
                    // 7) 적용에 성공했으면, 카운트 1 증가
                    successCount++;
                }
            }

            // 8) 목표한 횟수(count)만큼 성공했으면, 즉시 종료
            if (successCount >= count)
            {
                break;
            }
        }
    }

    // (카드들이 호출) 이 몬스터에게 DoT 중첩을 추가합니다.
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

    // 특정 DoT 중첩을 '일정 수치(정수)'만큼 감소
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

    // 특정 DoT 중첩을 '일정 퍼센트(%)'만큼 감소
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

    // --- 9. DoT 데미지 처리 ---
    // 개별 타이머로 DoT 데미지를 계산하고 적용합니다.
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
                Debug.LogWarning($"[DoT - 몬스터] 출혈로 {damage} 피해!");
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
                Debug.LogWarning($"[DoT - 몬스터] 중독으로 {damage} 피해! (쉴드 무시)");
                UpdateHealthUI(); // 체력 UI 업데이트
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
                Debug.LogWarning($"[DoT - 몬스터] 화상으로 {damage} 피해!");

                // 피해를 입힌 후 1스택 감소
                BurnStacks -= 1;
                UpdateDoTUI(); // 스택이 바뀌었으니 UI 업데이트

                m_BurnTickTimer = 0.5f; // 0.5초 타이머 초기화
            }
        }
    }

    // --- 10. UI 업데이트 함수 ---
    // 현재 체력(HP)과 쉴드(Shield) 변수를 실제 UI 바(Bar)와 텍스트(Label)에 적용
    protected virtual void UpdateHealthUI()
    {
        if (m_HealthBarFill != null)
        {
            float healthPercent = (MaxHP > 0) ? (CurrentHP / MaxHP) : 0f;
            m_HealthBarFill.style.width = Length.Percent(healthPercent * 100f);
        }

        if (m_HealthLabel != null)
        {
            m_HealthLabel.text = $"HP: {Mathf.CeilToInt(CurrentHP)} / {MaxHP}";
        }

        if (m_ShieldLabel != null)
        {
            m_ShieldLabel.text = $"SHIELD: {Mathf.CeilToInt(CurrentShield)}";
        }

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

    // DoT 중첩을 UI 라벨에 업데이트하고, 0이면 숨김
    protected virtual void UpdateDoTUI()
    {
        // 출혈 UI
        if (m_BleedStatusLabel != null)
        {
            if (BleedStacks > 0)
            {
                m_BleedStatusLabel.text = $"MON-BLEED : {BleedStacks}";
                m_BleedStatusLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_BleedStatusLabel.style.display = DisplayStyle.None;
            }
        }

        // 중독 UI
        if (m_PoisonStatusLabel != null)
        {
            if (PoisonStacks > 0)
            {
                m_PoisonStatusLabel.text = $"MON-POISON : {PoisonStacks}";
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
                m_BurnStatusLabel.text = $"MON-BURN : {BurnStacks}";
                m_BurnStatusLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_BurnStatusLabel.style.display = DisplayStyle.None;
            }
        }
    }

    public virtual void UpdateCardSlotUI(int index)
    {
        // 1. 인덱스가 유효한지 확인
        if (index < 0 || index >= 7) return;

        // 2. C# 데이터와 UI 슬롯을 가져옵니다.
        Card cardData = m_Cards[index];
        VisualElement slotUI = Slots[index];

        if (slotUI == null) return; // UI 슬롯이 없으면 종료

        // 3. 카드 데이터에 따라 UI를 업데이트
        if (cardData != null) // 슬롯에 카드가 있다면
        {
            // [신규!] UXML 슬롯의 배경 이미지를 카드의 이미지로 설정
            slotUI.style.backgroundImage = new StyleBackground(cardData.CardImage);

            // (나중에 여기에 쿨타임 UI, 상단 네모 UI를 업데이트하는 코드 추가)
            // (예: slotUI.Q<Label>("DamageLabel").text = cardData.BaseDamage.ToString();)
        }
        else // 슬롯이 비어있다면
        {
            // 배경 이미지를 '없음(null)'으로 설정하여 투명하게
            slotUI.style.backgroundImage = null;

            // (나중에 쿨타임 UI, 상단 네모 UI를 숨기는 코드 추가)
        }
    }


    // -------------------------- 프로토타입용 덱 설정 함수 ---------------------------------
    // --------------------------------------------------------------------------------------

    /// <param name="cardNames">덱에 넣을 카드 이름 목록 (예: "Monster")</param>

    public virtual void SetupDeck(string[] cardNames)
    {
        // (프로토타입용 하드코딩)
        // 'this'는 "MonsterController 자신"을 의미합니다.
        // Card.cs를 상속받은 Card_Monster.cs가 있다고 가정

        // m_Cards[1] = new Card_Monster(this); // 2번 슬롯에 몬스터 생성

        // (쿨타임 초기화 로직...)
    }
}
