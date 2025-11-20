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

    //  [밤 시스템]
    public const float NIGHT_START_TIME = 60.0f; // 60초 후 밤 시작
    private float m_BattleTimer = 0f;            // 현재 전투 시간

    public bool IsNight { get; private set; } = false; // 현재 밤인지 여부 (외부에서 읽기 가능)

    // 밤 대미지 관련
    private float m_NightTickTimer = 0f;
    private const float NIGHT_TICK_INTERVAL = 0.5f; // 1초마다 대미지
    private float m_CurrentNightDamage = 1f;        // 초기 대미지 5
    private float m_NightDamageIncrease = 1f;       // 틱당 대미지 증가량 2

    // (선택사항) 밤이 시작될 때 알림을 줄 이벤트
    public System.Action OnNightStarted;


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
        if (IsDeckEditingAllowed || m_IsBattleEnded) return;

        // 전투 시간 흐름
        float dt = Time.deltaTime;
        m_BattleTimer += dt;

        // 밤 시작 체크
        if (!IsNight && m_BattleTimer >= NIGHT_START_TIME)
        {
            StartNightPhase();
        }

        //밤 페이즈 로직 (지속 피해)
        if (IsNight)
        {
            HandleNightPhase(dt);
        }

        // dt time을 컨트롤러들에게 전달
        if (playerController != null) playerController.BattleUpdate(dt);
        if (monsterController != null) monsterController.BattleUpdate(dt);

        // 4. (선택사항) UI에 남은 시간 표시용
        // UIManager.Instance.UpdateTimerUI(m_BattleTimer);
    }

    private void StartNightPhase()
    {
        IsNight = true;
        Debug.LogWarning("[밤이 찾아왔습니다! 지속 피해가 시작됩니다.]");

        // 1. 비주얼 변경 (예: 배경 어둡게) -> UIManager나 Controller를 통해 호출
        // ChangeBackgroundToNight(); 

        // 2. 밤 시작 이벤트 발동 (나중에 늑대인간 카드 변신 등에 사용)
        OnNightStarted?.Invoke();
    }

    // 밤 동안 매 프레임 호출되어 지속 피해를 입힘
    private void HandleNightPhase(float dt)
    {
        m_NightTickTimer += dt;

        if (m_NightTickTimer >= NIGHT_TICK_INTERVAL)
        {
            m_NightTickTimer = 0f;

            // 양쪽 본체에 피해 주기 (TakeDamage 함수는 쉴드 먼저 깎도록 설계되어 있음)
            if (playerController != null)
            {
                playerController.TakeDamage(m_CurrentNightDamage);
            }

            if (monsterController != null)
            {
                monsterController.TakeDamage(m_CurrentNightDamage);
            }

            Debug.Log($"밤의 저주! 양쪽에 {m_CurrentNightDamage} 피해!");
            CheckConcurrentDeath();
            // 다음 틱을 위해 대미지 증가 (점점 아파짐)
            m_CurrentNightDamage += m_NightDamageIncrease;
        }
    }

    private void CheckConcurrentDeath()
    {
        if (m_IsBattleEnded) return; // 이미 끝났으면 무시

        bool playerDead = playerController.CurrentHP <= 0;
        bool monsterDead = monsterController.CurrentHP <= 0;

        if (playerDead && monsterDead)
        {
            // --- 동시 사망 발생! ---
            float playerHP = playerController.CurrentHP;   // 예: -5
            float monsterHP = monsterController.CurrentHP; // 예: -12 (더 많이 죽음)

            Debug.LogWarning($"동시 사망! PlayerHP: {playerHP} vs MonsterHP: {monsterHP}");

            if (playerHP > monsterHP)
            {
                EndBattle("Player");
            }
            else if (monsterHP > playerHP)
            {
                EndBattle("Monster");
            }
            else // 진짜 완벽하게 똑같은 체력으로 죽었다면?
            {
                // (기획에 따라 무승부 혹은 플레이어 패배 처리)
                Debug.LogWarning("체력까지 완벽하게 동일함. 플레이어 승리로 판정.");
                EndBattle("Player");
            }
        }
        else if (playerDead)
        {
            EndBattle("Monster");
        }
        else if (monsterDead)
        {
            EndBattle("Player");
        }
    }

    public void CheckBattleStatus()
    {
        // 밤 페이즈 로직과 동일한 로직 사용 (동시 사망 체크 포함)
        CheckConcurrentDeath();
    }

    // 다음 전투를 위해 상태 초기화 (필수)
    public void ResetBattleState()
    {
        m_BattleTimer = 0f;
        IsNight = false;
        m_CurrentNightDamage = 5f;
        m_NightTickTimer = 0f;
        // m_IsBattleEnded = false; // 이건 게임 흐름에 따라 처리
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