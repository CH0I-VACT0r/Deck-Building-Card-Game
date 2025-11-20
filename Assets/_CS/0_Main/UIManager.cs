using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [Header("UXML Files")]
    public VisualTreeAsset mainLayoutAsset;    // "MainLayout"
    public VisualTreeAsset fixedPlayerAsset;   // "Fixed_Player"
    public VisualTreeAsset battlePageAsset;    // "Battle_Page"

    private PlayerController playerController;
    private MonsterController monsterController;

    // 내부 변수
    private UIDocument _uiDocument;
    private VisualElement _root;            // 전체 화면 루트 (MainLayout)
    private VisualElement _topContainer;    // 교체 영역 (위)
    private VisualElement _bottomContainer; // 고정 영역 (아래)

    void Start()
    {
        BattleManager bm = FindFirstObjectByType<BattleManager>();
        if (bm != null)
        {
            this.playerController = bm.playerController;
            this.monsterController = bm.monsterController;
        }
        else
        {
            Debug.LogError("[UIManager] BattleManager를 찾을 수 없습니다!");
        }

        // 1. 뼈대(MainLayout) 로드
        _uiDocument = GetComponent<UIDocument>();
        _uiDocument.visualTreeAsset = mainLayoutAsset;
        _root = _uiDocument.rootVisualElement;

        // 2. 컨테이너 찾기
        _topContainer = _root.Q<VisualElement>("TopContentContainer");
        _bottomContainer = _root.Q<VisualElement>("BottomFixedContainer");


        // [이름 확인용 로그]
        if (_topContainer == null) Debug.LogError("MainLayout.uxml에서 'TopContentContainer'를 찾을 수 없습니다.");
        if (_bottomContainer == null) Debug.LogError("MainLayout.uxml에서 'BottomFixedContainer'를 찾을 수 없습니다.");

        if (_topContainer == null || _bottomContainer == null) return; // 여기서 중단

        // 3. [고정] 플레이어 UI 생성 및 부착
        VisualElement playerUI = fixedPlayerAsset.Instantiate();
        playerUI.style.flexGrow = 1;
        _bottomContainer.Add(playerUI);

        // 4. PlayerController 초기화
        if (playerController != null)
        {
            // 여기서 비로소 UI가 연결됩니다.
            playerController.InitializeUI(playerUI, _root);
        }

        // 5. [초기 상태] 전투 페이지 로드
        SwitchToBattlePage();
    }

    // --- 페이지 교체 함수 ---

    /// <summary>
    /// 전투 화면(몬스터 파티)으로 전환합니다.
    /// </summary>
    public void SwitchToBattlePage()
    {
        _topContainer.Clear(); // 기존 내용 비우기

        if (battlePageAsset != null)
        {
            VisualElement battleUI = battlePageAsset.Instantiate();
            battleUI.style.flexGrow = 1;
            _topContainer.Add(battleUI);

            if (monsterController != null)
            {
                // [수정!] 여기서 _root를 두 번째 인자로 넘겨줘야 에러가 해결됩니다.
                monsterController.InitializeUI(battleUI, _root);
            }
        }
    }
}
