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

    /// 내가 공격해야 할 '타겟' (플레이어 컨트롤러)
    protected PlayerController m_Target;

    // --- 2. UI 요소 ---

    /// 이 컨트롤러가 관리하는 UXML의 최상위 패널 (UI Toolkit: "MonsterParty")
    protected VisualElement m_MonsterParty;

    /// UXML에서 찾아온 7개의 몬스터 슬롯 UI 요소 리스트
    public List<VisualElement> Slots { get; protected set; } = new List<VisualElement>(7);

    // --- 3. 핵심 상태 (공통) ---
    /// 몬스터(지휘관)의 현재 체력
    public float CurrentHP { get; protected set; }

    /// 몬스터(지휘관)의 최대 체력
    public float MaxHP { get; protected set; }

    // --- 4. 카드 덱 관리 ---

    /// 이 몬스터가 현재 전투에서 사용하는 7칸의 카드 배열
    protected Card[] m_Cards = new Card[7];


    // --- 5. 생성자 ---
    /// MonsterController가 처음 생성될 때 호출됩니다.
    /// <param name="manager">나를 관리할 BattleManager</param>
    /// <param name="panel">내가 제어할 UXML의 'MonsterParty' 패널</param>
    /// <param name="maxHP">이 몬스터의 최대 체력</param>
    /// 
    public MonsterController(BattleManager manager, VisualElement panel, float maxHP)
    {
        this.m_BattleManager = manager;
        this.m_MonsterParty = panel;
        this.MaxHP = maxHP;
        this.CurrentHP = maxHP;

        // UXML 슬롯(MonSlot1 ~ MonSlot7)을 이름으로 찾아 리스트에 추가합니다.
        Slots.Clear();
        for (int i = 0; i < 7; i++)
        {
            // UXML에 정의된 이름 (예: "MonSlot1")
            string slotName = "MonSlot" + (i + 1);
            Slots.Add(m_MonsterParty.Q<VisualElement>(slotName));
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
    }

    /// (공통) 플레이어가 나를 공격할 때 호출하는 함수입니다.
    /// <param name="amount">받는 피해량</param>
   
    public virtual void TakeDamage(float amount)
    {
        CurrentHP -= amount;
        Debug.LogWarning($"[몬스터] {amount} 피해 받음! 남은 체력: {CurrentHP}");

        // 체력이 0 이하가 되면 플레이어의 승리를 알림
        if (CurrentHP <= 0)
        {
            m_BattleManager.EndBattle("Player"); // 승리!
        }
    }

    /// (공통) 프로토타입용 덱 설정 함수입니다.
    /// <param name="cardNames">덱에 넣을 카드 이름 목록 (예: "Monster")</param>
    
    public virtual void SetupDeck(string[] cardNames)
    {
        // (프로토타입용 하드코딩)
        // 'this'는 "MonsterController 자신"을 의미합니다.
        // Card.cs를 상속받은 Card_Monster.cs가 있다고 가정합니다.

        // m_Cards[1] = new Card_Monster(this); // 2번 슬롯에 몬스터 생성

        // (쿨타임 초기화 로직...)
    }

    /// (공통) 나의 '타겟'(플레이어)이 누구인지 알려주는 함수.
    /// 몬스터 카드들이 이 함수를 호출하여 플레이어를 공격합니다.

    public PlayerController GetTarget()
    {
        return m_Target;
    }

    /// (공통) BattleManager가 나의 '타겟'을 지정해주는 함수.
    public void SetTarget(PlayerController target)
    {
        this.m_Target = target;
    }

    // --- 7. 위치 기반 헬퍼 함수 (신규 추가!) ---
    /// [헬퍼] 내 덱(m_Cards)의 특정 인덱스에 있는 카드를 반환
    public Card GetCardAtIndex(int index)
    {
        if (index >= 0 && index < 7 && m_Cards[index] != null)
        {
            return m_Cards[index];
        }
        return null;
    }

    /// [인접-왼쪽] "나의 왼쪽"에 있는 카드를 반환합니다.
    public Card GetLeftNeighbor(int myIndex)
    {
        return GetCardAtIndex(myIndex - 1);
    }

    /// [인접-오른쪽] "나의 오른쪽"에 있는 카드를 반환합니다.
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

    // --- 8. 상태 이상 헬퍼 함수 (신규 추가!) ---
    /// 내 카드 중 '면역이 아닌' 무작위 카드 N개에 상태 이상을 적용합니다.
    /// (플레이어의 '빙결' 스킬 등이 이 함수를 호출합니다.)
    public void ApplyStatusToRandomCards(int count, StatusEffectType effectType, float duration)
    {
        // 1. 0~6번 슬롯 인덱스가 담긴 리스트를 만듭니다.
        List<int> slotIndices = new List<int> { 0, 1, 2, 3, 4, 5, 6 };

        // 2. 리스트를 무작위로 섞습니다 (Fisher-Yates Shuffle).
        for (int i = 0; i < slotIndices.Count; i++)
        {
            int temp = slotIndices[i];
            int randomIndex = Random.Range(i, slotIndices.Count);
            slotIndices[i] = slotIndices[randomIndex];
            slotIndices[randomIndex] = temp;
        }

        // 3. 적용에 성공한 횟수를 셉니다.
        int successCount = 0;

        // 4. 무작위로 섞인 슬롯 순서대로 확인합니다.
        foreach (int index in slotIndices)
        {
            Card card = GetCardAtIndex(index); // (방금 위에 추가한 헬퍼 함수)

            // 5. 슬롯이 비어있지 않은지 확인
            if (card != null)
            {
                // 6. [핵심!] 카드에게 효과 적용을 '시도'합니다.
                // (이 코드가 작동하려면 Card.cs에 ApplyStatusEffect 함수가 있어야 합니다!)
                if (card.ApplyStatusEffect(effectType, duration))
                {
                    // 7. 적용에 성공했으면, 카운트를 1 올립니다.
                    successCount++;
                }
            }

            // 8. 목표한 횟수(count)만큼 성공했으면, 즉시 종료합니다.
            if (successCount >= count)
            {
                break;
            }
        }
    }
}
