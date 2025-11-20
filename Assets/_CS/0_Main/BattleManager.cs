using UnityEngine;
using UnityEngine.UIElements;

// 전투 씬의 메인 컨트롤러입니다.
/// 이 스크립트는 공통 로직만 가지며, 어떤 영주나 몬스터가 선택되었는지에 대해서는 다루지 않음
public class BattleManager : MonoBehaviour
{
    // --- 1. 컨트롤러 참조 ---
    public PlayerController playerController; //플레이어 진영을 관리하는 컨트롤러의 기본 형태 : 컨트롤러팩토리 에서 생성
    public MonsterController monsterController; // 몬스터 진영을 관리하는 컨트롤러의 '기본' 형태
    
    // --- 2. 전투 상태 ---
    private bool m_IsBattleEnded = false;
    public bool IsBattleEnded { get { return m_IsBattleEnded; } }
    public bool IsDeckEditingAllowed { get; set; } = false; // 덱 편집(D&D) 허용 상태

    // --- 3. Unity 수명 주기 함수 ---
    void Awake()
    {
        Debug.Log("[BattleManager] 컨트롤러 객체 생성 중...");
        playerController = ControllerFactory.CreatePlayerController(this);
        monsterController = ControllerFactory.CreateMonsterController(this);

        // 타겟 지정
        playerController.SetTarget(monsterController);
        monsterController.SetTarget(playerController);

        // 덱 설정
        playerController.SetupDeck(null);
        monsterController.SetupDeck(null);
    }

    /// 매 프레임마다 Unity에 의해 호출됩니다.
    void Update()
    {
        // 전투 종료
        if (m_IsBattleEnded) return;

        // dt time을 컨트롤러들에게 전달
        float dt = Time.deltaTime;
        if (playerController != null) playerController.BattleUpdate(dt);
        if (monsterController != null) monsterController.BattleUpdate(dt);
    }


    // --- 5. 공용 함수 ---
    // 전투 종료 선언 함수
    public void EndBattle(string winner)
    {
        if (m_IsBattleEnded) return;
        m_IsBattleEnded = true;

        if (playerController != null) playerController.CleanupBattleUI();
        if (monsterController != null) monsterController.CleanupBattleUI();

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
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.SetPhase(GameManager.GamePhase.Reward); // GameManager의 Reward 단계로 진입
        }
    }
}