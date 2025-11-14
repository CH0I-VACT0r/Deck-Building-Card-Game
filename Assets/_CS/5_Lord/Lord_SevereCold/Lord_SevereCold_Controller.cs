using UnityEngine;
using UnityEngine.UIElements; // UI Toolkit 사용
using System.Diagnostics; // Debug.Log 대신 사용 가능

/// '혹한의 성주' 전용 컨트롤러
/// 'PlayerController'의 모든 공통 기능 상속
/// '격노(Enrage)'고유 메커니즘을 추가

public class Lord_SevereCold_Controller : PlayerController
{
    // --- 1. '혹한의 성주'만의 고유 상태 ---

    /// '격노' 상태인지 여부 (체력 50% 미만)
    /// public { get; ... } : 다른 스크립트가 이 값을 읽을 수 있게 함
    /// private set; : 이 값은 오직 이 스크립트 안에서만 수정 가능
    public bool IsEnraged { get; private set; } = false;
    private bool m_WasEnragedLastFrame = false;


    // --- 2. 생성자 ---
    /// '혹한의 성주' 컨트롤러가 생성될 때 호출
    /// <param name="manager">나를 관리할 BattleManager</param>
    /// <param name="panel">내가 제어할 UXML의 'PlayerParty' 패널</param>
    /// <param name="maxHP">이 영주의 최대 체력</param>

    // ': base(manager, panel, maxHP)'
    //  받은 이 정보들을, PlayerController의 생성자에게 그대로 전달
    public Lord_SevereCold_Controller(BattleManager manager, VisualElement panel, float maxHP)
        : base(manager, panel, maxHP)
    {
        // (이곳은 '혹한의 성주'만의 초기화 코드를 위한 공간. 추후 로직 추가.)
        UnityEngine.Debug.Log("[Lord_SevereCold_Controller] 생성 완료. '격노' 시스템 활성화.");
        UpdateEnrageVisuals(false);
    }


    // --- 3. 핵심 함수 (기능 확장) ---
    /// [PlayerController의 BattleUpdate 함수를 override
    /// <param name="deltaTime">Time.deltaTime (프레임당 시간)</param>
    /// 
    public override void BattleUpdate(float deltaTime)
    {
        // 1. 부모의 공통 로직(카드 쿨타임 돌리기)을 먼저 실행
        // 이 코드가 없으면 카드 스킬이 발동되지 않음!!
        base.BattleUpdate(deltaTime);

        // 2. '혹한의 성주'만의 '격노' 상태를 매 프레임 검사
        IsEnraged = (CurrentHP < (MaxHP * 0.5f));

        // 격노 상태일 때 UI 이펙트를 추가하고 싶다면, 이곳에서 제어 가능. 추후 논의
        if (m_WasEnragedLastFrame != IsEnraged)
        {
            UpdateEnrageVisuals(IsEnraged);
            m_WasEnragedLastFrame = IsEnraged; // 현재 상태를 '이전 상태'로 저장
        }
    }

    private void UpdateEnrageVisuals(bool isEnraged)
    {
        // (부모의 m_LordPortrait 변수를 사용합니다)
        if (m_LordPortrait == null) return; // UI가 없으면 종료

        if (isEnraged)
        {
            // 격노 상태 켜기
            m_LordPortrait.AddToClassList("lord-portrait-enraged");
        }
        else
        {
            // 격노 상태 끄기
            m_LordPortrait.RemoveFromClassList("lord-portrait-enraged");
        }
    }

    public override void CleanupBattleUI()
    {
        base.CleanupBattleUI();

        // '혹한의 성주'만의 '격노' 상태 초기화
        IsEnraged = false;
        m_WasEnragedLastFrame = false; 
        UpdateEnrageVisuals(false);
    }




    // [프로토타입용 하드코딩]
    // '혹한의 성주' 덱을 생성
    public override void SetupDeck(string[] cardNames)
    {
        m_Cards[0] = new Card_BarbarianWarrior(this, 0);
        UpdateCardSlotUI(0);

        m_Cards[1] = new Card_Shieldbearer(this, 1);
        UpdateCardSlotUI(1);

        m_Cards[2] = new Card_Bloodletter(this, 2);     // 출혈 카드
        UpdateCardSlotUI(2);

        m_Cards[3] = new Card_Healer(this, 3);          // 즉발 힐 카드
        UpdateCardSlotUI(3);

        m_Cards[4] = new Card_Regenerator(this, 4);     // 지속 힐 카드
        UpdateCardSlotUI(4);

        m_Cards[5] = new Card_FrostFowl(this, 5);     // 빙결 카드
        UpdateCardSlotUI(5);

        m_Cards[6] = new Card_FuriousWarrior(this, 6);     // 빙결 카드
        UpdateCardSlotUI(6);

        UnityEngine.Debug.Log("[Lord_SevereCold_Controller] 혹한의 성주 전용 덱 설정 완료.");

        // 쿨타임 초기화
        for (int i = 0; i < 7; i++)
        {
            if (m_Cards[i] != null)
                m_Cards[i].CurrentCooldown = m_Cards[i].CooldownTime;
        }
    }
}
