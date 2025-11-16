using UnityEngine;
using UnityEngine.UIElements;

// 데이터 기반 실제 컨트롤러 객체 생성 클래스
public static class ControllerFactory
{

    // [플레이어]
    public static PlayerController CreatePlayerController(BattleManager battleManager, VisualElement playerPanel)
    {
        string lordType = GameSessionData.SelectedLordType; // 플레이어가 선택한 영주 타입

        // 2. 'switch' 문을 사용해, 타입에 맞는 컨트롤러를 생성
        // (프로토타입 단계에서는 영주의 기본 체력을 여기에 하드코딩)
        switch (lordType)
        {
            case "SevereCold":
                // "혹한의 성주"를 선택했다면, '격노'가 포함된 자식 클래스를 생성합니다.
                Debug.Log("ControllerFactory: 'Lord_SevereCold_Controller'를 생성합니다.");
                return new Lord_SevereCold_Controller(battleManager, playerPanel, 100f); // 최대 체력 100

            // --- 나중에 다른 영주를 추가할 공간 ---
            // case "Imperial":
            //    return new Lord_Imperial_Controller(battleManager, playerPanel, 120f);

            // case "Progress":
            //    return new Lord_Progress_Controller(battleManager, playerPanel, 80f);

            //값이 없을 때, 오류 방지용
            default:
                Debug.LogError($"[ControllerFactory] 알 수 없는 영주 타입({lordType})입니다! " +
                               $"기본 PlayerController를 반환합니다.");
                return new PlayerController(battleManager, playerPanel, 100f);
        }
    }

    // [몬스터]
    public static MonsterController CreateMonsterController(BattleManager battleManager, VisualElement monsterPanel)
    {
        string monsterDeckID = GameSessionData.SelectedMonsterDeck; // 1. 상대할 몬스터 덱 ID

        // 2. 'switch' 문을 사용해, 덱에 맞는 컨트롤러와 체력을 설정합니다.
        switch (monsterDeckID)
        {
            case "Tutorial_Monsters":
                // 튜토리얼 몬스터(프로토타입)를 생성
                Debug.Log("ControllerFactory: 기본 'MonsterController'를 생성합니다.");
                return new MonsterController(battleManager, monsterPanel, 80f); // 최대 체력 80

            // --- 나중에 보스 몬스터를 추가할 공간 ---
            // case "Boss_Alcyoneus":
            //    return new Boss_Alcyoneus_Controller(battleManager, monsterPanel, 1000f); // 보스 체력 1000

            default:
                Debug.LogError($"[ControllerFactory] 알 수 없는 몬스터 덱 ID({monsterDeckID})입니다! " +
                               $"기본 MonsterController를 반환합니다.");
                return new MonsterController(battleManager, monsterPanel, 80f);
        }
    }
}
