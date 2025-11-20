using UnityEngine;
// using UnityEngine.UIElements; // 이제 여기서 UI 요소는 필요 없습니다.

// 데이터 기반 실제 컨트롤러 객체 생성 클래스
public static class ControllerFactory
{
    // [플레이어]
    // [수정!] VisualElement 인자 제거
    public static PlayerController CreatePlayerController(BattleManager battleManager)
    {
        string lordType = GameSessionData.SelectedLordType; // 플레이어가 선택한 영주 타입

        // 2. 'switch' 문을 사용해, 타입에 맞는 컨트롤러를 생성
        switch (lordType)
        {
            case "SevereCold":
                Debug.Log("ControllerFactory: 'Lord_SevereCold_Controller'를 생성합니다.");
                // [수정!] 생성자에서 playerPanel 제거 (인자 2개만 전달)
                return new Lord_SevereCold_Controller(battleManager, 100f);

            // ... (다른 케이스들) ...

            default:
                Debug.LogError($"[ControllerFactory] 알 수 없는 영주 타입({lordType})입니다! 기본 PlayerController를 반환합니다.");
                // [수정!] 생성자에서 playerPanel 제거
                return new PlayerController(battleManager, 100f);
        }
    }

    // [몬스터]
    // [수정!] VisualElement 인자 제거
    public static MonsterController CreateMonsterController(BattleManager battleManager)
    {
        string monsterDeckID = GameSessionData.SelectedMonsterDeck;

        switch (monsterDeckID)
        {
            case "Tutorial_Monsters":
                Debug.Log("ControllerFactory: 기본 'MonsterController'를 생성합니다.");
                // [수정!] 생성자에서 monsterPanel 제거
                return new MonsterController(battleManager, 80f);

            default:
                Debug.LogError($"[ControllerFactory] 알 수 없는 몬스터 덱 ID({monsterDeckID})입니다! 기본 MonsterController를 반환합니다.");
                // [수정!] 생성자에서 monsterPanel 제거
                return new MonsterController(battleManager, 80f);
        }
    }
}