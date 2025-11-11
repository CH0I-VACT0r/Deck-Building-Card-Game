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


    // --- 2. 생성자 ---
    /// '혹한의 성주' 컨트롤러가 생성될 때 호출됩니다.
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

        // (참고) 격노 상태일 때 UI 이펙트를 추가하고 싶다면, 이곳에서 제어 가능. 추후 논의
        // if (IsEnraged) { m_PartyPanel.style.backgroundColor = Color.red; }
        // else { m_PartyPanel.style.backgroundColor = Color.clear; }
    }

    /// PlayerController의 '프로토타입용 덱 설정' 함수를 override
    /// <param name="cardNames">BattleManager로부터 전달받은 덱 정보 (지금은 null)</param>
   
    public override void SetupDeck(string[] cardNames)
    {
        // (나중에는 cardNames 배열이나 GameSessionData를 기반으로 덱을 생성해야 합니다)

        // [프로토타입용 하드코딩]
        // '혹한의 성주' 덱을 생성
        // 'this'는 "Lord_SevereCold_Controller 자신"을 의미

        // 2번 슬롯(m_Cards[1])에 카드1 생성
        // (Card1_"Name".cs가 'PlayerController'를 받도록 수정해야 합니다.)
        /// 예시) m_Cards[1] = new Card1_"Name"(this);

        // 3번 슬롯(m_Cards[2])에 카드2 생성
        // (Card_Shieldbearer.cs가 'PlayerController'를 받도록 수정해야 합니다.)
        /// 예시) m_Cards[2] = new Card2_"Name"(this);

        UnityEngine.Debug.Log("[Lord_SevereCold_Controller] 혹한의 성주 전용 덱 설정 완료.");

        // 쿨타임 초기화
        for (int i = 0; i < 7; i++)
        {
            if (m_Cards[i] != null)
                m_Cards[i].CurrentCooldown = m_Cards[i].CooldownTime;
        }
    }

    // (TakeDamage, GetTarget 등 다른 함수들은 부모의 것을 그대로 사용하므로 여기에 다시 작성할 필요가 없다.)
}
