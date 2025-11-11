using UnityEngine;
using UnityEngine.UIElements; // UI Toolkit 사용

// 전투 씬의 메인 컨트롤러입니다.
/// 이 스크립트는 공통 로직만 가지며, 어떤 영주나 몬스터가 선택되었는지에 대해서는 다루지 않음
/// </summary>
public class BattleManager : MonoBehaviour
{
    // --- 1. 컨트롤러 참조 ---
    //플레이어 진영을 관리하는 컨트롤러의 '기본' 형태
    /// (실제로는 컨트롤러팩토리 에서 생성된 영주가 담김.)
    public PlayerController playerController;

    /// 몬스터 진영을 관리하는 컨트롤러의 '기본' 형태입니다.
    public MonsterController monsterController;

    // --- 2. UI 참조 ---
    private VisualElement m_Root;
    private VisualElement m_PlayerPartyPanel;
    private VisualElement m_MonsterPartyPanel;

    // --- 3. 전투 상태 ---
    private bool m_IsBattleEnded = false;


    // --- 4. Unity 수명 주기 함수 ---
    /// 이 스크립트가 활성화될 때 (게임 시작 시) 호출됩니다
    void OnEnable()
    {
        // 1. UXML 루트 요소 찾기
        m_Root = GetComponent<UIDocument>().rootVisualElement;

        // 2. UXML에서 이름으로 파티 패널 찾기
        m_PlayerPartyPanel = m_Root.Q<VisualElement>("PlayerParty");
        m_MonsterPartyPanel = m_Root.Q<VisualElement>("MonsterParty");

        if (m_PlayerPartyPanel == null || m_MonsterPartyPanel == null)
        {
            Debug.LogError("### UXML에서 'PlayerParty' 또는 'MonsterParty' 패널을 찾을 수 없습니다! UXML의 'Name'을 확인하세요. ###");
            return;
        }

        // 3. 컨트롤러를 직접 생성하지 않고, '공장(Factory)'에 요청
        //    '공장'이 GameSessionData를 읽어와서 알맞은 컨트롤러 생성.
        playerController = ControllerFactory.CreatePlayerController(this, m_PlayerPartyPanel);
        monsterController = ControllerFactory.CreateMonsterController(this, m_MonsterPartyPanel);

        // 4. 타겟 지정 (서로가 누굴 공격할지 알려줌)
        playerController.SetTarget(monsterController);
        monsterController.SetTarget(playerController);

        // 5. 덱 설정 (프로토타입: 컨트롤러가 스스로 자신의 덱을 설정하도록 함)
        //    (SetupDeck 함수는 이제 각 컨트롤러가 알아서 본인 덱 설정)
        playerController.SetupDeck(null); // (나중에는 GameSessionData에서 덱 정보를 받아와 넘겨줄 수 있다)
        monsterController.SetupDeck(null);
    }

    /// 매 프레임마다 Unity에 의해 호출됩니다.
    void Update()
    {
        // 전투 종료
        if (m_IsBattleEnded) return;

        // dt time을 컨트롤러들에게 전달
        float dt = Time.deltaTime;
        playerController.BattleUpdate(dt);
        monsterController.BattleUpdate(dt);
    }


    // --- 5. 공용 함수 ---
    /// 전투 종료를 선언하는 함수 (컨트롤러들이 호출)
    /// <param name="winner">승자 ("Player" 또는 "Monster")</param>

    public void EndBattle(string winner)
    {
        m_IsBattleEnded = true; // Update 루프 정지

        if (winner == "Player")
        {
            Debug.Log("--- 전투 종료! 승자: 플레이어 ---");
            // (여기에 승리 보상 로직 호출)
        }
        else
        {
            Debug.LogError("--- 전투 종료! 승자: 몬스터 ---");
            // (여기에 패배 처리 로직 호출)
        }
    }
}