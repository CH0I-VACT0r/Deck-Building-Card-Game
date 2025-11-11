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

    /// 이 컨트롤러가 관리하는 UXML의 최상위 패널 (UI Toolkit : "PlayerParty")
    protected VisualElement m_PlayerParty;

    /// <summary>
    /// UXML에서 찾아온 7개의 카드 슬롯 UI 요소 리스트
    /// </summary>
    public List<VisualElement> Slots { get; protected set; } = new List<VisualElement>(7);

    // --- 3. 핵심 상태 (공통) ---

    /// 영주의 현재 체력
    /// protected set: 나 자신과 내 자식 클래스(영주)만 체력을 수정할 수 있음
    public float CurrentHP { get; protected set; }

    /// 영주의 최대 체력
    public float MaxHP { get; protected set; }

    // (나중에 레벨, 명성 등 공통 자원 여기에 추가)
    // public int Level { get; protected set; }

    // --- 4. 카드 덱 관리 ---
    /// 이 영주가 현재 전투에서 사용하는 7칸의 카드 배열
    protected Card[] m_Cards = new Card[7];


    // --- 5. 생성자 ---
    /// PlayerController가 처음 생성될 때 호출됩니다.
    /// <param name="manager">나를 관리할 BattleManager</param>
    /// <param name="panel">내가 제어할 UXML의 'PlayerParty' 패널</param>
    /// <param name="maxHP">이 영주의 최대 체력</param>
    /// 
    public PlayerController(BattleManager manager, VisualElement panel, float maxHP)
    {
        this.m_BattleManager = manager;
        this.m_PlayerParty = panel;
        this.MaxHP = maxHP;
        this.CurrentHP = maxHP;

        // UXML 슬롯(CardSlot1 ~ CardSlot7)을 이름으로 찾아 리스트에 추가합니다.
        Slots.Clear();
        for (int i = 0; i < 7; i++)
        {
            // UXML에 정의된 이름 (예: "CardSlot1")
            string slotName = "CardSlot" + (i + 1);
            Slots.Add(m_PlayerParty.Q<VisualElement>(slotName));
        }
    }

    // --- 6. 핵심 함수 ---
    /// BattleManager가 호출해주는, 이 컨트롤러의 매 프레임 업데이트 함수
    /// <param name="deltaTime">Time.deltaTime (프레임당 시간)</param>
    /// 
    public virtual void BattleUpdate(float deltaTime)
    {
        // 1. (공통) 내 카드들의 쿨타임 회전 및 스킬을 발동
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
    }

    /// (공통) 몬스터가 나를 공격할 때 호출하는 함수
    /// <param name="amount">받는 피해량</param>
    /// 
    public virtual void TakeDamage(float amount)
    {
        // (나중에 보호막(Shield) 변수가 있다면, 여기서 먼저 피해를 흡수하는 로직 추가)

        CurrentHP -= amount;
        // 일단 로그로 확인
        Debug.Log($"[플레이어] {amount} 피해 받음! 남은 체력: {CurrentHP}");

        // 체력이 0 이하가 되면 패배를 전달
        if (CurrentHP <= 0)
        {
            m_BattleManager.EndBattle("Monster"); // 패배!
        }
    }

    /// (공통) 프로토타입용 덱 설정 함수입니다.
    /// 'virtual'이므로 자식 클래스(영주)가 이 함수를 덮어써서 자신만의 덱을 구성 가능.
    /// </summary>
    /// <param name="cardNames">덱에 넣을 카드 이름 목록 (예: "이름")</param>
    /// 
    public virtual void SetupDeck(string[] cardNames)
    {
        // 기본 PlayerController는 덱이 비어있습니다.
        // 자식 클래스에서 이 부분을 채워야 합니다.
        Debug.Log("기본 PlayerController는 덱을 설정할 수 없습니다.");
    }

    /// (공통) 나의 '타겟'(몬스터)이 누구인지 알려주는 함수.
    public MonsterController GetTarget()
    {
        return m_Target;
    }

    /// (공통) BattleManager가 나의 '타겟'을 지정해주는 함수.
    public void SetTarget(MonsterController target)
    {
        this.m_Target = target;
    }
}
