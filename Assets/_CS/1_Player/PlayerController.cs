using UnityEngine;
using UnityEngine.UIElements; // UI Toolkit 사용
using System.Collections.Generic; // List 사용

// 모든 플레이어 영주(Lord)의 공통 부모가 되는 기본 클래스 : 체력, 슬롯, 카드 덱 등 공통 기능만 관리
// 영주만의 특수한 로직은 포함하지 않고, 따로 관리

public class PlayerController
{
    // --- 1. 참조 변수 ---
    protected BattleManager m_BattleManager; // 전투의 규칙을 관리
    protected MonsterController m_Target; // 공격해야 할 타겟 (몬스터 컨트롤러)

    // --- 2. UI 요소 ---
    protected VisualElement m_LordPortrait; // 초상화 패널
    protected VisualElement m_PlayerParty; // 7칸 카드 슬롯 패널
    protected VisualElement m_StatusPanel; // 상태 패널 UI
    public List<VisualElement> Slots { get; protected set; } = new List<VisualElement>(7); // 7개의 카드 슬롯 UI 요소 리스트

    // 툴팁 UI 요소
    private VisualElement m_TooltipRoot;
    private Label m_TooltipName;
    private VisualElement m_TooltipTagContainer;
    private Label m_TooltipCooldown;
    private Label m_TooltipSkillDesc;
    private VisualElement m_TooltipStatContainer;
    private VisualElement m_TooltipQuestContainer;
    private Label m_TooltipQuestTitle;
    private Label m_TooltipQuestDesc;
    private Label m_TooltipQuestStatus;

    private VisualElement m_TooltipCritContainer;
    private Label m_TooltipCritChance;
    private VisualElement m_TooltipDurabilityContainer;
    private Label m_TooltipDurability;
    private Label m_TooltipFlavorText;
    private VisualElement m_TooltipDivider1;
    private VisualElement m_TooltipDivider2;

    // 툴팁 스케줄러 변수
    private IVisualElementScheduledItem m_TooltipScheduler;
    private const long TOOLTIP_DELAY_MS = 300; // 0.4초

    // 상태 패널 요소
    private VisualElement m_HealthBarFill;
    private VisualElement m_ShieldBarFill;
    private Label m_HealthLabel;
    private Label m_ShieldLabel;
    private Label m_LevelLabel;
    private List<VisualElement> m_XPTicks = new List<VisualElement>(10); // 10칸 네모 XP 바를 위한 리스트

    // DoT 도트 대미지 아이콘 UI 라벨
    private Label m_BleedStatusLabel;
    private Label m_PoisonStatusLabel;
    private Label m_BurnStatusLabel;
    private Label m_HealStatusLabel;

    // 역할 UI 컨테이너 리스트
    private List<VisualElement> m_RoleUIContainers = new List<VisualElement>(7);
    private List<VisualElement> m_CooldownOverlays = new List<VisualElement>(7);
    private List<VisualElement> m_CardImageLayers = new List<VisualElement>(7);
    private List<VisualElement> m_CostContainers = new List<VisualElement>(7);
    private List<Label> m_CostLabels = new List<Label>(7);

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
    public int HealStacks { get; protected set; } = 0;

    // DoT 데미지 타이머 (개별 관리)
    private float m_BleedTickTimer = 1.5f;   // 출혈 : 1.5초 
    private float m_PoisonTickTimer = 3.0f;  // 중독 : 3초 
    private float m_BurnTickTimer = 0.5f;    // 화상 : 0.5초
    private float m_HealTickTimer = 2.0f; // 회복 : 2초

    // 충격/견고 상태 변수
    private bool m_IsShocked = false;
    private float m_ShockTimer = 0f;
    private bool m_IsSturdy = false;
    private float m_SturdyTimer = 0f;

    // 충격/견고 배율 (상수)
    private const float SHOCK_MULTIPLIER = 1.2f; // 20% 추가 피해
    private const float STURDY_MULTIPLIER = 0.8f; // 20% 피해 감소

    // --- 4. 카드 덱 관리 ---
    /// 이 영주가 현재 전투에서 사용하는 7칸의 카드 배열
    protected Card[] m_Cards = new Card[7];


    // --- 5. 생성자 ---
    /// PlayerController가 처음 생성될 때 호출
    public PlayerController(BattleManager manager, VisualElement VisualElement, float maxHP)
    {
        this.m_BattleManager = manager;
        this.MaxHP = maxHP;
        this.CurrentHP = maxHP;
        this.CurrentShield = 0;

        this.m_PlayerParty = VisualElement.Q<VisualElement>("PlayerParty");
        this.m_StatusPanel = VisualElement.Q<VisualElement>("PlayerStatus");

        // TooltipRoot와 그 자식들 쿼리
        m_TooltipRoot = VisualElement.Q<VisualElement>("TooltipRoot");
        if (m_TooltipRoot != null)
        {
            // UXML의 Name 확인
            m_TooltipName = m_TooltipRoot.Q<Label>("TooltipName");
            m_TooltipTagContainer = m_TooltipRoot.Q<VisualElement>("TooltipTagContainer");
            m_TooltipSkillDesc = m_TooltipRoot.Q<Label>("TooltipSkillDesc");
            m_TooltipCooldown = m_TooltipRoot.Q<Label>("TooltipCooldown");
            m_TooltipQuestContainer = m_TooltipRoot.Q<VisualElement>("TooltipQuestContainer");
            m_TooltipQuestTitle = m_TooltipRoot.Q<Label>("QuestName");
            m_TooltipQuestDesc = m_TooltipRoot.Q<Label>("TooltipQuestDesc");
            m_TooltipQuestStatus = m_TooltipRoot.Q<Label>("TooltipQuestStatus");
            m_TooltipStatContainer = m_TooltipRoot.Q<VisualElement>("TooltipStatContainer");
            m_TooltipCritContainer = m_TooltipRoot.Q<VisualElement>("TooltipCritContainer");
            m_TooltipCritChance = m_TooltipRoot.Q<Label>("TooltipCritChance");
            m_TooltipDurabilityContainer = m_TooltipRoot.Q<VisualElement>("TooltipDurabilityContainer");
            m_TooltipDurability = m_TooltipRoot.Q<Label>("TooltipDurability");
            m_TooltipFlavorText = m_TooltipRoot.Q<Label>("TooltipFlavorText");
            m_TooltipDivider1 = m_TooltipRoot.Q<VisualElement>("TooltipDivider1");
            m_TooltipDivider2 = m_TooltipRoot.Q<VisualElement>("TooltipDivider2");
            m_TooltipRoot.style.display = DisplayStyle.None;
        }
        else
        {
            Debug.LogError("[PlayerController] 'TooltipRoot'를 UXML에서 찾을 수 없습니다!");
        }

        // 2. 카드 슬롯(CardSlot1 ~ CardSlot7) 찾기
        Slots.Clear();
        m_RoleUIContainers.Clear();
        m_CooldownOverlays.Clear();
        m_CardImageLayers.Clear();
        m_CostContainers.Clear();
        m_CostLabels.Clear();
        for (int i = 0; i < 7; i++)
        {
            // UXML에 정의된 이름 (UXML 이름 확인!)
            VisualElement slot = m_PlayerParty.Q<VisualElement>("CardSlot" + (i + 1));
            Slots.Add(slot);

            if (slot != null)
            {
                m_CardImageLayers.Add(slot.Q<VisualElement>("CardImage")); 
                m_RoleUIContainers.Add(slot.Q<VisualElement>("RoleUIContatiner"));
                m_CooldownOverlays.Add(slot.Q<VisualElement>("CooldownOverlay"));
                m_CostContainers.Add(slot.Q<VisualElement>("CostContainer"));
                m_CostLabels.Add(slot.Q<Label>("CostLabel"));

                // 마우스 이벤트 등록
                int currentIndex = i;
                slot.RegisterCallback<PointerEnterEvent>(evt => OnPointerEnterSlot(currentIndex, evt));
                slot.RegisterCallback<PointerLeaveEvent>(evt => OnPointerLeaveSlot());
            }
            else
            {
                Debug.LogError($"[PlayerController] 'CardSlot{i + 1}'을 UXML에서 찾을 수 없습니다!");
                m_RoleUIContainers.Add(null);
                m_CooldownOverlays.Add(null);
                m_CostContainers.Add(null);
                m_CostLabels.Add(null);
            }
        }

        // 상태 UI
        if (m_StatusPanel != null)
        {
            m_LordPortrait = m_StatusPanel.Q<VisualElement>("Portrait");
            m_HealthBarFill = m_StatusPanel.Q<VisualElement>("HP-Bar-Fill");
            m_HealthLabel = m_StatusPanel.Q<Label>("HP-label");
            m_ShieldBarFill = m_StatusPanel.Q<VisualElement>("Shield-Bar_Fill");
            m_ShieldLabel = m_StatusPanel.Q<Label>("Shield-label");
            m_LevelLabel = m_StatusPanel.Q<Label>("LV-label");

            m_XPTicks.Clear();
            for (int i = 0; i < 10; i++)
            {
                m_XPTicks.Add(m_StatusPanel.Q<VisualElement>("XPTick" + i));
            }

            //DoT 아이콘
            m_BleedStatusLabel = m_StatusPanel.Q<Label>("BleedStatus");
            m_PoisonStatusLabel = m_StatusPanel.Q<Label>("PoisonStatus");
            m_BurnStatusLabel = m_StatusPanel.Q<Label>("BurnStatus");
            m_HealStatusLabel = m_StatusPanel.Q<Label>("HealStatus");
        }
        else
        {
            Debug.LogError("[PlayerController] 'PlayerStatusPanel'을 UXML에서 찾을 수 없습니다!");
        }

        // UI 업데이트
        UpdateHealthUI();
        UpdateXPUI();
        UpdateDoTUI();
    }

    // --- 6. 핵심 함수 ---
    public virtual void BattleUpdate(float deltaTime)
    {
        // 내 카드들의 쿨타임 회전 및 스킬 발동
        for (int i = 0; i < 7; i++)
        {
            if (m_Cards[i] != null)
            {
                m_Cards[i].UpdateCooldown(deltaTime);

                // 스킬 발동
                if (m_Cards[i].CurrentCooldown <= 0f)
                {
                    m_Cards[i].TriggerSkill();
                }
                UpdateCardSlotUI(i);
            }
        }
        // DoT 데미지 타이머
        ProcessDoTs(deltaTime);
    }

    // [이벤트] 카드 슬롯에 마우스가 들어왔을 때 호출
    private void OnPointerEnterSlot(int slotIndex, PointerEnterEvent evt)
    {
        // 해당 슬롯에 카드가 있는지 확인
        Card card = GetCardAtIndex(slotIndex);
        if (card == null) return; // 카드가 없으면 무시

        // 이전에 예약된 툴팁 스케줄이 있다면 취소
        m_TooltipScheduler?.Pause();

        // 'TOOLTIP_DELAY_MS' 밀리초 후 ShowTooltip 함수 실행
        VisualElement slotElement = evt.currentTarget as VisualElement;
        m_TooltipScheduler = slotElement.schedule
            .Execute(() => ShowTooltip(card, slotElement))
            .StartingIn(TOOLTIP_DELAY_MS);
    }

    // [이벤트] 카드 슬롯에서 마우스가 나갔을 때 호출
    private void OnPointerLeaveSlot()
    {
        // 예약되어 있던 툴팁 스케줄 즉시 취소
        m_TooltipScheduler?.Pause();

        // 2. 툴팁 UI 즉시 숨김
        if (m_TooltipRoot != null)
        {
            m_TooltipRoot.style.display = DisplayStyle.None;
        }
    }

    // [실행] 스케줄러에 의해 딜레이 이후 실제 툴팁을 띄우는 함수
    private void ShowTooltip(Card card, VisualElement slotElement)
    {
        // null 체크
        if (m_TooltipRoot == null || card == null || slotElement == null) return;

        // --- 카드 데이터로 툴팁 UI 내용 채우기 ---
        // 이름 (Key -> 번역)
        m_TooltipName.text = LocalizationManager.GetText(card.CardNameKey);
        // 태그 (동적 Label 생성)
        m_TooltipTagContainer.Clear();
        m_TooltipTagContainer.style.display = DisplayStyle.None;
        if (card.TagKeys != null && card.TagKeys.Count > 0)
        {
            foreach (string tagKey in card.TagKeys)
            {
                string translatedTag = LocalizationManager.GetText(tagKey);
                Label tagLabel = new Label(translatedTag);
                tagLabel.AddToClassList("tooltip-tag-label");
                m_TooltipTagContainer.Add(tagLabel);
            }
            m_TooltipTagContainer.style.display = DisplayStyle.Flex;
        }

        // --- 쿨타임 표시 (별도 라벨) ---
        if (card.ShowCooldownUI && card.BaseCooldownTime > 0)
        {
            m_TooltipCooldown.text = LocalizationManager.GetText("stat_cooldown", card.BaseCooldownTime.ToString());
            m_TooltipCooldown.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_TooltipCooldown.style.display = DisplayStyle.None;
        }
        //  메인 스킬 설명
        m_TooltipSkillDesc.text = LocalizationManager.GetText(card.CardSkillDescriptionKey);
        // 부가 스탯/효과 목록
        m_TooltipStatContainer.Clear();

        System.Action<string, float> AddStatLine = (string key, float value) =>
        {
            if (value > 0)
            {
                Label line = new Label(LocalizationManager.GetText(key, value.ToString()));
                line.AddToClassList("tooltip-stat-line");
                m_TooltipStatContainer.Add(line);
            }
        };

        System.Action<string> AddTextLine = (string text) =>
        {
            Label line = new Label(text);
            line.AddToClassList("tooltip-stat-line");
            m_TooltipStatContainer.Add(line);
        };

        // 모든 스탯 추가
        AddStatLine("stat_damage", card.GetCurrentDamage());
        AddStatLine("stat_shield", card.GetCurrentShield());
        AddStatLine("stat_heal", card.GetCurrentHeal());
        AddStatLine("stat_crit_chance", card.GetCurrentCritChance() * 100f);
        AddStatLine("stat_apply_bleed", card.GetCurrentBleedStacks());
        AddStatLine("stat_apply_poison", card.GetCurrentPoisonStacks());
        AddStatLine("stat_apply_burn", card.GetCurrentBurnStacks());
        AddStatLine("stat_apply_heal_dot", card.GetCurrentHealStacks());
        AddStatLine("stat_apply_freeze", card.GetCurrentFreezeDuration());
        AddStatLine("stat_apply_haste", card.GetCurrentHasteDuration());
        AddStatLine("stat_apply_slow", card.GetCurrentSlowDuration());
        AddStatLine("stat_apply_cooldown_reduction", card.GetCurrentCooldownReduction());
        AddStatLine("stat_apply_cooldown_increase", card.GetCurrentCooldownIncrease());
        AddStatLine("stat_apply_echo", card.GetCurrentEchoStacks());
        AddStatLine("stat_apply_shock", card.GetCurrentShockDuration());
        AddStatLine("stat_apply_sturdy", card.GetCurrentSturdyDuration());
        AddStatLine("stat_price_inflate", card.GetCurrentPriceInflate());
        AddStatLine("stat_price_extort", card.GetCurrentPriceExtort());

        // 소환
        if (card.SummonCount > 0 && !string.IsNullOrEmpty(card.SummonCardNameKey))
        {
            // 소환할 카드 이름 번역
            string summonName = LocalizationManager.GetText(card.SummonCardNameKey);
            // 최종 문장 완성 ("소환: "카드 이름" x 2")
            string text = LocalizationManager.GetText("stat_summon", summonName, card.SummonCount);
            AddTextLine(text);
        }

        // 유언
        if (!string.IsNullOrEmpty(card.DeathrattleDescKey))
        {
            // 유언 효과 설명 번역 
            string effectDesc = LocalizationManager.GetText(card.DeathrattleDescKey);
            // 최종 문장 완성 ("유언: ~~~")
            string text = LocalizationManager.GetText("stat_deathrattle", effectDesc);
            AddTextLine(text);
        }

        // 컨테이너 보이기/숨기기
        if (m_TooltipStatContainer.childCount > 0)
        {
            m_TooltipStatContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_TooltipStatContainer.style.display = DisplayStyle.None;
        }

        // --- [이하 동일] 퀘스트, 내구도, 플레이버 텍스트 ---

        // 퀘스트 (조건부 표시)
        if (card.HasQuest)
        {
            m_TooltipQuestTitle.text = LocalizationManager.GetText(card.QuestTitleKey);
            m_TooltipQuestDesc.text = LocalizationManager.GetText(card.QuestDescriptionKey);
            bool isDone = card.IsQuestComplete;
            string statusKey = isDone ? "quest_status_complete" : "quest_status_incomplete";
            m_TooltipQuestStatus.text = LocalizationManager.GetText(statusKey);
            m_TooltipQuestStatus.EnableInClassList("quest-status-complete", isDone);
            m_TooltipQuestStatus.EnableInClassList("quest-status-incomplete", !isDone);
            m_TooltipQuestContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_TooltipQuestContainer.style.display = DisplayStyle.None;
        }

        // 내구도 (조건부 표시)
        if (card.Durability > -1)
        {
            m_TooltipDurability.text = LocalizationManager.GetText("stat_durability", card.Durability.ToString());
            m_TooltipDurabilityContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_TooltipDurabilityContainer.style.display = DisplayStyle.None;
        }

        // 플레이버 텍스트 (Key -> 번역)
        string flavor = LocalizationManager.GetText(card.FlavorTextKey);
        if (!string.IsNullOrEmpty(flavor) && !flavor.StartsWith("["))
        {
            m_TooltipFlavorText.text = $"\"{flavor}\"";
            m_TooltipFlavorText.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_TooltipFlavorText.style.display = DisplayStyle.None;
        }

        //Divider1 (태그/쿨타임과 스킬설명 사이)
        bool isTopSectionVisible = (m_TooltipTagContainer.style.display == DisplayStyle.Flex ||
                                    m_TooltipCooldown.style.display == DisplayStyle.Flex);

        m_TooltipDivider1.style.display = isTopSectionVisible ? DisplayStyle.Flex : DisplayStyle.None;

        // Divider2 (부가 스탯/퀘스트/내구도와 플레이버 텍스트 사이)
        bool isMidSectionVisible = (
                                    m_TooltipQuestContainer.style.display == DisplayStyle.Flex ||
                                    m_TooltipDurabilityContainer.style.display == DisplayStyle.Flex);

        bool isFlavorTextVisible = (m_TooltipFlavorText.style.display == DisplayStyle.Flex);

        m_TooltipDivider2.style.display = (isMidSectionVisible && isFlavorTextVisible) ? DisplayStyle.Flex : DisplayStyle.None;

        // --- 툴팁 위치 조절 (슬롯의 오른쪽 상단) ---
        Rect slotRect = slotElement.worldBound;
        float offsetX = 10f;
        m_TooltipRoot.style.left = slotRect.xMax + offsetX;
        m_TooltipRoot.style.top = slotRect.yMin;

        // ---  툴팁 UI---
        m_TooltipRoot.style.display = DisplayStyle.Flex;
    }

    /// (공통) 몬스터가 나를 공격할 때 호출하는 함수
    public virtual void TakeDamage(float amount)
    {
        // 최종 피해량 계산
        float finalDamage = amount;
        if (m_IsShocked)
        {
            finalDamage *= SHOCK_MULTIPLIER; // 20% 증폭
        }
        else if (m_IsSturdy) // (중첩 안 됨)
        {
            finalDamage *= STURDY_MULTIPLIER; // 20% 감소
        }
        finalDamage = Mathf.Round(finalDamage);
        float damageRemaining = finalDamage;

        // 쉴드 깎음
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

        // 체력 깎음
        if (damageRemaining > 0)
        {
            CurrentHP -= damageRemaining;
        }

        Debug.Log($"[플레이어] {finalDamage} 피해 받음! (쉴드 {CurrentShield} 남음, 체력 {CurrentHP} 남음)");

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            m_BattleManager.EndBattle("Monster"); // 패배!
        }

        UpdateHealthUI();
    }

    public virtual void IncreaseMaxHP(float amount)
    {
        if (amount <= 0) return;

        MaxHP += amount;
        CurrentHP = MaxHP; // 현재 체력 최대로 회복

        Debug.Log($"[플레이어] 최대 체력 영구 증가! (새로운 최대 HP: {MaxHP})");

        // UI 즉시 업데이트
        UpdateHealthUI();
    }

    public virtual void DecreaseMaxHP(float amount)
    {
        if (amount <= 0) return;

        MaxHP -= amount;

        // 최대 체력이 1 미만 방지
        if (MaxHP < 1)
        {
            MaxHP = 1;
        }

        if (CurrentHP > MaxHP)
        {
            CurrentHP = MaxHP;
        }

        Debug.LogWarning($"[플레이어] 최대 체력을 담보로 지불! (새로운 최대 HP: {MaxHP})");

        // UI 즉시 업데이트
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

            MaxHP += 10; // 레벨업 시 최대 체력 증가 (추후 조정)
            CurrentHP = MaxHP; // 레벨업 시 체력 회복)

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
    /// 내 덱(m_Cards)의 특정 인덱스에 있는 카드 반환 : (범위를 벗어나면 null을 반환)
    public Card GetCardAtIndex(int index)
    {
        if (index >= 0 && index < 7 && m_Cards[index] != null)
        {
            return m_Cards[index];
        }
        return null;
    }

    /// [인접-왼쪽] 왼쪽에 있는 카드를 반환
    public Card GetLeftNeighbor(int myIndex)
    {
        return GetCardAtIndex(myIndex - 1);
    }

    /// [인접-오른쪽] 오른쪽에 있는 카드를 반환
    public Card GetRightNeighbor(int myIndex)
    {
        return GetCardAtIndex(myIndex + 1);
    }

    /// [상대 위치] 맞은편에 있는 몬스터 카드 반환
    public Card GetOppositeCard(int myIndex)
    {
        if (m_Target != null)
        {
            return m_Target.GetCardAtIndex(myIndex);
        }
        return null;
    }

    // --- 8. 상태 이상 헬퍼 함수 ---
    /// 내 카드 중 '면역이 아닌' 무작위 카드 N개에 상태 이상을 적용
    public void ApplyStatusToRandomCards(int count, StatusEffectType effectType, float duration)
    {
        if (m_BattleManager.IsBattleEnded) return;
        // 0~6번 슬롯 인덱스가 담긴 리스트 생성
        List<int> slotIndices = new List<int> { 0, 1, 2, 3, 4, 5, 6 };

        // 리스트 무작위 셔플
        for (int i = 0; i < slotIndices.Count; i++)
        {
            int temp = slotIndices[i];
            int randomIndex = Random.Range(i, slotIndices.Count);
            slotIndices[i] = slotIndices[randomIndex];
            slotIndices[randomIndex] = temp;
        }

        // 적용에 성공한 횟수를 카운팅
        int successCount = 0;

        // 무작위로 섞인 슬롯 순서대로 확인
        foreach (int index in slotIndices)
        {
            Card card = GetCardAtIndex(index); // 

            // 슬롯이 비어있지 않은지 확인
            if (card != null)
            {
                // 카드에게 효과 적용 시도
                // (이 코드가 작동하려면 Card.cs에 ApplyStatusEffect 함수가 있어야 됨)
                if (card.ApplyStatusEffect(effectType, duration))
                {
                    // 적용에 성공했으면, 카운팅
                    successCount++;
                }
            }

            // 목표한 횟수(count)만큼 성공했으면, 즉시 종료
            if (successCount >= count)
            {
                break;
            }
        }
    }

    /// 이 영주에게 스탯 중첩 추가
    public virtual void ApplyLordStatus(StatusEffectType effectType, int stacks)
    {
        if (m_BattleManager.IsBattleEnded) return;
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
            case StatusEffectType.Heal:
                HealStacks += stacks;
                break;

            // --- Buff/Debuff (지속시간) ---
            case StatusEffectType.Shock: // 충격
                m_IsShocked = true;
                m_ShockTimer = Mathf.Max(m_ShockTimer, stacks); // 더 긴 시간으로 갱신
                m_IsSturdy = false; // 견고 해제
                m_SturdyTimer = 0f;
                Debug.Log($"[플레이어] '감전' 효과! ({stacks}초)");
                break;

            case StatusEffectType.Sturdy: // 견고
                m_IsSturdy = true;
                m_SturdyTimer = Mathf.Max(m_SturdyTimer, stacks); // 더 긴 시간으로 갱신
                m_IsShocked = false; // 감전 해제
                m_ShockTimer = 0f;
                Debug.Log($"[플레이어] '견고' 효과! ({stacks}초)");
                break;
        }
        UpdateDoTUI(); // UI 업데이트
    }


    /// 특정 상태 이상 중첩을 '일정 수치(정수)'만큼 감소
    public virtual void ReduceLordStatus(StatusEffectType effectType, int amount)
    {
        if (m_BattleManager.IsBattleEnded) return;
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
            case StatusEffectType.Heal:
                HealStacks = Mathf.Max(0, HealStacks - amount);
                break;
        }
        UpdateDoTUI(); // UI 업데이트
    }

    /// 특정 상태 이상 중첩을 '일정 퍼센트(%)'만큼 감소
    public virtual void ReduceLordStatusPercent(StatusEffectType effectType, float percent)
    {
        if (m_BattleManager.IsBattleEnded) return;
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
            case StatusEffectType.Heal:
                HealStacks = Mathf.FloorToInt(HealStacks * (1.0f - clampedPercent));
                break;
        }
        UpdateDoTUI(); // UI 업데이트
    }

    // --- 9. DoT 데미지 처리---
    // '개별' 타이머로 DoT 데미지를 계산 및 적용
    private void ProcessDoTs(float deltaTime)
    {
        if (m_BattleManager.IsBattleEnded) return;
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

        // (4) 지속 회복 (2초 틱, 체력 회복)
        if (HealStacks > 0)
        {
            m_HealTickTimer -= deltaTime;
            if (m_HealTickTimer <= 0f)
            {
                float healAmount = HealStacks * 1; // (스택당 1 회복)
                AddHealth(healAmount); // 체력 추가 함수 호출
                Debug.Log($"[HoT] 지속 회복으로 {healAmount} 회복!");
                m_HealTickTimer = 2.0f; // 2초 타이머 초기화
            }
        }

        // (5) 충격 타이머
        if (m_IsShocked)
        {
            m_ShockTimer -= deltaTime;
            if (m_ShockTimer <= 0f)
            {
                m_IsShocked = false;
                Debug.Log("[플레이어] '감전' 상태 해제!");
                // (나중에 UI가 있다면 UpdateStatusEffectUI() 호출)
            }
        }

        // (6) 견고 타이머
        if (m_IsSturdy)
        {
            m_SturdyTimer -= deltaTime;
            if (m_SturdyTimer <= 0f)
            {
                m_IsSturdy = false;
                Debug.Log("[플레이어] '견고' 상태 해제!");
                // (나중에 UI가 있다면 UpdateStatusEffectUI() 호출)
            }
        }
    }

    public virtual void AddHealth(float amount)
    {
        if (m_BattleManager.IsBattleEnded) return;

        CurrentHP += amount;
        CurrentHP = Mathf.Clamp(CurrentHP, 0, MaxHP);
        UpdateHealthUI();
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
            m_HealthLabel.text = $"{Mathf.CeilToInt(CurrentHP)}";
        }

        // 쉴드 텍스트(Label)
        if (m_ShieldLabel != null)
        {
            m_ShieldLabel.text = $"{Mathf.CeilToInt(CurrentShield)}";
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
                m_BleedStatusLabel.text = $"{BleedStacks}";
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
                m_PoisonStatusLabel.text = $"{PoisonStacks}";
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
                m_BurnStatusLabel.text = $"{BurnStacks}";
                m_BurnStatusLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_BurnStatusLabel.style.display = DisplayStyle.None;
            }
        }

        // 회복 UI
        if (m_HealStatusLabel != null)
        {
            if (HealStacks > 0)
            {
                m_HealStatusLabel.text = $"{HealStacks}";
                m_HealStatusLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_HealStatusLabel.style.display = DisplayStyle.None;
            }
        }
    }

    public virtual void UpdateCardSlotUI(int index)
    {
        // 인덱스 유효 여부 확인
        if (index < 0 || index >= 7) return;

        // C# 데이터 & UI 슬롯 호출
        Card cardData = m_Cards[index];
        VisualElement slotUI = Slots[index];

        if (slotUI == null) return;

        // UI 내부 요소 호출
        VisualElement cooldownOverlay = m_CooldownOverlays[index];
        VisualElement roleUIContainer = m_RoleUIContainers[index];
        VisualElement cardImageLayer = m_CardImageLayers[index];
        VisualElement costContainer = m_CostContainers[index];
        Label costLabel = m_CostLabels[index];

        // 카드 데이터에 따라 UI를 업데이트
        if (cardData != null) 
        {
            // 카드 이미지 적용
            if (cardImageLayer != null)
            {
                cardImageLayer.style.backgroundImage = new StyleBackground(cardData.CardImage);
            }
            if (costContainer != null && costLabel != null)
            {
                // Card.cs의 CardPrice가 0보다 클 때만 UI를 켭니다.
                if (cardData.CardPrice >= 0)
                {
                    costContainer.style.display = DisplayStyle.Flex;
                    costLabel.text = cardData.CardPrice.ToString();
                }
                else
                {
                    costContainer.style.display = DisplayStyle.None;
                }
            }

            // 쿨타임 UI 업데이트
            if (cooldownOverlay != null)
            {
                // 패시브 카드인지 확인
                if (cardData.ShowCooldownUI)
                {
                    //카드가 '빙결' 상태인지 먼저 확인
                    if (cardData.IsFrozen())
                    {
                        cooldownOverlay.style.display = DisplayStyle.Flex;
                        cooldownOverlay.style.height = Length.Percent(100f);
                        cooldownOverlay.AddToClassList("cooldown-overlay-frozen");
                        cooldownOverlay.RemoveFromClassList("cooldown-overlay-hasted");
                        cooldownOverlay.RemoveFromClassList("cooldown-overlay-slowed");
                    }
                    // 일반 쿨타임 상태인지 확인
                    
                    else if (cardData.CurrentCooldown > 0.01f)
                    {
                        cooldownOverlay.style.display = DisplayStyle.Flex; // 오버레이
                        float maxCooldown = cardData.GetCurrentCooldownTime(); // 
                        float percent = (maxCooldown > 0) ? (cardData.CurrentCooldown / maxCooldown) : 1f; 
                        cooldownOverlay.style.height = Length.Percent(percent * 100f); // '남은 %'만큼 높이를 조절
                        cooldownOverlay.RemoveFromClassList("cooldown-overlay-frozen"); // 빙결 제거

                        // 가속 상태 확인
                        if (cardData.IsHasted())
                        {
                            cooldownOverlay.AddToClassList("cooldown-overlay-hasted");
                            cooldownOverlay.RemoveFromClassList("cooldown-overlay-slowed");
                        }
                        // 감속 상태 확인
                        else if (cardData.IsSlowed())
                        {
                            cooldownOverlay.AddToClassList("cooldown-overlay-slowed");
                            cooldownOverlay.RemoveFromClassList("cooldown-overlay-hasted");
                        }
                        // 일반' 쿨타임 상태입니다.
                        else
                        {
                            // 모든 특수 색상 제거
                            cooldownOverlay.RemoveFromClassList("cooldown-overlay-hasted");
                            cooldownOverlay.RemoveFromClassList("cooldown-overlay-slowed");
                        }
                    }
                    // 기본 준비 상태
                    else
                    {
                        cooldownOverlay.style.display = DisplayStyle.None;
                        cooldownOverlay.RemoveFromClassList("cooldown-overlay-frozen");
                        cooldownOverlay.RemoveFromClassList("cooldown-overlay-hasted");
                        cooldownOverlay.RemoveFromClassList("cooldown-overlay-slowed");
                        cooldownOverlay.style.height = Length.Percent(100f);
                    }
                }
                else
                {
                    cooldownOverlay.style.display = DisplayStyle.None;
                }
            }
                // 역할 UI 업데이트
            if (roleUIContainer != null)
            {   
                roleUIContainer.Clear();
                // 우선순위에 따라 아이콘을 동적 생성
                // 우선순위 1: 대미지
                float currentDamage = cardData.GetCurrentDamage();
                if (currentDamage > 0) { CreateRoleIcon(roleUIContainer, "role-attacker", currentDamage.ToString()); }

                // 우선순위 2: 상태이상
                // 출혈
                int currentBleed = cardData.GetCurrentBleedStacks();
                if (currentBleed > 0) { CreateRoleIcon(roleUIContainer, "role-bleed", currentBleed.ToString()); }
                //화상
                int currentBurn = cardData.GetCurrentBurnStacks();
                if (currentBurn > 0) { CreateRoleIcon(roleUIContainer, "role-burn", currentBurn.ToString()); }
                // 중독
                int currentPoison = cardData.GetCurrentPoisonStacks();
                if (currentPoison > 0) { CreateRoleIcon(roleUIContainer, "role-poison", currentPoison.ToString()); }
                // 빙결
                float currentFreeze = cardData.GetCurrentFreezeDuration();
                if (currentFreeze > 0) { CreateRoleIcon(roleUIContainer, "role-freeze", currentFreeze.ToString()); }

                // 우선순위 3: 쉴드
                float currentShield = cardData.GetCurrentShield();
                if (currentShield > 0) { CreateRoleIcon(roleUIContainer, "role-tanker", currentShield.ToString()); }

                // 우선순위 4: 힐
                float currentHeal = cardData.GetCurrentHeal();
                if (currentHeal > 0) { CreateRoleIcon(roleUIContainer, "role-healer", currentHeal.ToString()); }

                // 우선순위 5: 지속 힐
                int currentHealStacks = cardData.GetCurrentHealStacks();
                if (currentHealStacks > 0) { CreateRoleIcon(roleUIContainer, "role-heal-dot", currentHealStacks.ToString()); }
            }
        }
        else // 슬롯이 비어있다면
        {
            // 배경 이미지를 null로 설정
            if (cardImageLayer != null)
            {
                cardImageLayer.style.backgroundImage = null;
            }

            // 쿨타임/역할 UI도 모두 숨김
            if (cooldownOverlay != null) cooldownOverlay.style.display = DisplayStyle.None;
            if (roleUIContainer != null) roleUIContainer.Clear();
        }
    }

    // 역할 아이콘 동적 생성
    private void CreateRoleIcon(VisualElement container, string roleClass, string valueText)
    {
        // 1) 아이콘 생성
        VisualElement icon = new VisualElement();
        icon.AddToClassList("card-role-icon"); // 공통 스타일 
        icon.AddToClassList(roleClass); // 개별 색상 스타일

        // 2) 텍스트 라벨(Label) 생성
        Label label = new Label(valueText);
        label.AddToClassList("card-role-label"); 

        // 3) 네모 안에 텍스트를 넣고, 컨테이너에 네모를 추가
        icon.Add(label); // 텍스트 추가
        container.Add(icon); // 아이콘 추가
    }

    public virtual void CleanupBattleUI()
    {
        for (int i = 0; i < 7; i++)
        {
            if (m_Cards[i] != null) // 슬롯에 카드가 있다면
            {
                m_Cards[i].ClearBattleStatBuffs(); // 스탯 초기화
                m_Cards[i].ClearBattleFrozen(); // 빙결 초기화
                m_Cards[i].CurrentCooldown = 0f; // 쿨타임 초기화
                UpdateCardSlotUI(i); // UI 갱신
            }
        }
    }

    // --- 11. 카드 파괴 함수 ---
    // 이 함수는 전투 중 파괴만 담당 : 다음 획득 전까지 영구적으로 사라지는 로직은 이곳이 아닌 메인 덱 리스트에서 이 카드를 제거함으로써 구현
    public virtual void DestroyCard(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 7) return;

        Card cardToDestroy = m_Cards[slotIndex];
        if (cardToDestroy != null)
        {
            Debug.Log($"[{cardToDestroy.CardNameKey}] (이)가 파괴되었습니다!");
            cardToDestroy.OnDestroyed();
            // 1. C# 배열에서 카드를 제거 (null로 만듦)
            m_Cards[slotIndex] = null;

            // 2. UI를 빈 슬롯 상태로 즉시 업데이트 (이미지/UI 모두 지움)
            UpdateCardSlotUI(slotIndex);
        }
    }

    // --- 12. 카드 소환 함수 ---
    public virtual bool SpawnCardToRandomEmptySlot(string cardID)
    {
        // 비어있는 슬롯 찾기
        int emptySlotIndex = -1;
        for (int i = 0; i < 7; i++)
        {
            if (m_Cards[i] == null)
            {
                emptySlotIndex = i;
                break;
            }
        }

        if (emptySlotIndex == -1)
        {
            Debug.Log($"[{cardID}] 소환 실패: 빈 슬롯이 없습니다.");
            return false;
        }

        // CardFactory 카드 생성 요청
        Card newCard = CardFactory.CreateCard(cardID, this, emptySlotIndex);

        if (newCard == null)
        {
            Debug.LogError($"[SpawnCard] CardFactory가 {cardID} 카드 생성을 실패했습니다.");
            return false;
        }

        //카드 배치
        m_Cards[emptySlotIndex] = newCard;

        //UI 업데이트
        UpdateCardSlotUI(emptySlotIndex);

        Debug.Log($"[{newCard.CardNameKey}] (이)가 {emptySlotIndex}번 슬롯에 성공적으로 소환되었습니다!");
        return true;
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