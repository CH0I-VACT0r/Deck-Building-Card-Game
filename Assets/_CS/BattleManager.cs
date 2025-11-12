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

    // --- 3. 전투 상태 ---
    private bool m_IsBattleEnded = false;


    // --- 4. Unity 수명 주기 함수 ---
    /// 이 스크립트가 활성화될 때 (게임 시작 시) 호출됩니다
    void OnEnable()
    {
        // 1) UXML 루트 요소 찾기
        m_Root = GetComponent<UIDocument>().rootVisualElement; 

        if (m_Root == null)
        {
            Debug.LogError("### UIDocument 컴포넌트나 Source Asset(UXML)이 없습니다! ###");
            return;
        }

        // 2) Root 바로 전달
        playerController = ControllerFactory.CreatePlayerController(this, m_Root);
        monsterController = ControllerFactory.CreateMonsterController(this, m_Root);

        // 3) 타겟 지정 (서로가 누굴 공격할지 알려줌)
        playerController.SetTarget(monsterController);
        monsterController.SetTarget(playerController);

        // 4) 덱 설정 (프로토타입: 컨트롤러가 스스로 자신의 덱을 설정하도록 함)
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
    /// 전투 종료를 선언하는 함수
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