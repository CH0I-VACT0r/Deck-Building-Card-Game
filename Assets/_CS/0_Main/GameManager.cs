using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// 요일 정의
public enum Weekday
{
    Monday_Hire = 0,    // 고용의 월요일
    Tuesday_Labor = 1,  // 노동의 화요일
    Wednesday_Train = 2,// 훈련의 수요일
    Thursday_Scout = 3, // 의뢰의 목요일
    Friday_Craft = 4,   // 제작의 금요일
    Saturday_Survey = 5,// 정찰의 토요일
    Sunday_Raid = 6     // 공략의 일요일
}


// 게임의 전체 흐름을 관리하는 최상위 매니저 : 요일, 라운드, 단계 등
public class GameManager : MonoBehaviour
{
    // --- 참조 ---
    public BattleManager battleManager;

    // --- 게임 상태 ---
    public int CurrentDay { get; private set; } = 1;
    public enum GamePhase { Preparation, Battle, Reward, DayEnd }
    public GamePhase currentPhase = GamePhase.Preparation;

    void Start()
    {
        if (battleManager == null)
        {
            battleManager = FindFirstObjectByType<BattleManager>();
        }

        // 첫 단계 시작 (CurrentDay는 1이므로 월요일 Preparation부터 시작)
        SetPhase(GamePhase.Preparation);
        TriggerDayEvent(GetCurrentWeekday()); // 첫날 이벤트 트리거
    }

    // --- 헬퍼 함수 ---
    private Weekday GetCurrentWeekday()
    {
        // CurrentDay(1부터 시작)를 0부터 6까지의 요일 인덱스로 변환
        int dayIndex = (CurrentDay - 1) % 7;
        return (Weekday)dayIndex;
    }

    void Update()
    {
        // [테스트용] 스페이스바를 누르면 '전투 단계'로 강제 전환
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentPhase == GamePhase.Preparation)
            {
                Debug.Log("Test] 스페이스바 입력: 전투 시작!");
                SetPhase(GamePhase.Battle);
            }
            else if (currentPhase == GamePhase.Reward)
            {
                // (나중에 턴 넘기기 버튼 기능)
                Debug.Log("[Test] 다음 날로 진행");
                StartNextDay();
            }
        }
    }

    // --- 핵심 로직 ---
    // 게임 단계 설정 / 필요한 액션 수행
    public void SetPhase(GamePhase newPhase)
    {
        currentPhase = newPhase;

        switch (newPhase)
        {
            case GamePhase.Preparation:
                // 덱 편집 허용: D&D 활성화!
                battleManager.IsDeckEditingAllowed = true;
                Debug.Log($"--- Day {CurrentDay} ({GetCurrentWeekday()}) 정비 단계 시작: 덱 편집 허용 ---");
                // TODO: 여기에 Inventory UI 등을 켜는 로직 추가
                break;

            case GamePhase.Battle:
                // 덱 편집 잠금: D&D 비활성화
                battleManager.IsDeckEditingAllowed = false;
                Debug.Log("--- 전투 시작! D&D 잠금 ---");
                // TODO: 여기에 BattleManager에게 '전투 시작' 명령을 내리는 로직 추가
                break;

            case GamePhase.Reward:
                // 전투 보상 지급 (BattleManager가 EndBattle을 호출한 후)
                Debug.Log("--- 전투 종료: 보상 지급 단계 ---");
                // TODO: 여기에 보상 UI를 띄우는 로직 추가
                // (보상 지급 후, SetPhase(GamePhase.DayEnd) 호출)
                break;

            case GamePhase.DayEnd:
                Debug.Log("--- 하루 종료: 다음 날 시작 대기 ---");
                // TODO: 여기에 End of Day UI/Summary를 띄우는 로직 추가
                break;
        }
    }

    // 다음 날을 시작하고 요일별 이벤트 설정 (DayEnd 단계에서 호출됨)
    public void StartNextDay()
    {
        CurrentDay++;
        Weekday today = GetCurrentWeekday();
        Debug.Log($"--- Day {CurrentDay} ({today}) 시작 ---");

        // Preparation 단계로 전환 및 이벤트 트리거
        SetPhase(GamePhase.Preparation);
        TriggerDayEvent(today);
    }

    // 요일별로 특정 이벤트 및 UI 활성화
    private void TriggerDayEvent(Weekday day)
    {
        // TODO: UIManager 통합 시, UIManager.OpenWindow(day) 등으로 대체
        switch (day)
        {
            case Weekday.Monday_Hire:
                Debug.Log("고용의 월요일: 용병 모집 UI 활성화");
                // TODO: UIManager.ShowHiringWindow();
                break;
            case Weekday.Tuesday_Labor:
                Debug.Log("노동의 화요일: 게임 재화 획득 이벤트 시작");
                // TODO: StartLaborMiniGame();
                break;
            case Weekday.Wednesday_Train:
                Debug.Log("훈련의 수요일: 용병 훈련/강화 이벤트 시작");
                // TODO: UIManager.ShowTrainingWindow();
                break;
            case Weekday.Thursday_Scout:
                Debug.Log("의뢰의 목요일: 세미 전투 이벤트(경험치 획득) 시작");
                // TODO: StartSemiBattleEvent();
                break;
            case Weekday.Friday_Craft:
                Debug.Log("제작의 금요일: 장비/음식 제작 UI 활성화");
                // TODO: UIManager.ShowCraftingWindow(CraftingType.Equipment | CraftingType.Food);
                break;
            case Weekday.Saturday_Survey:
                Debug.Log("정찰의 토요일: 던전 정보 획득 이벤트 시작");
                // TODO: StartScoutingEvent();
                break;
            case Weekday.Sunday_Raid:
                Debug.Log("공략의 일요일: 최종 던전 공략 준비 단계.");
                // 일요일은 D&D 정비 외에는 별도의 이벤트 UI 없이, 전투 시작 유도
                break;
        }
    }
}
