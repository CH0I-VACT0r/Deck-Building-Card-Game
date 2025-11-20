// --- 5. 생성자 ---
/// PlayerController가 처음 생성될 때 호출
/*
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

*/