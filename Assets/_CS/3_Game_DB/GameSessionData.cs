using UnityEngine;

// 게임의 씬(Scene)이 바뀌어도 유지되어야 하는 핵심 세션 데이터(예: 플레이어의 영주 선택)를 저장하는 정적 클래스
public static class GameSessionData
{
    // --- 1. 플레이어 선택 ---

    /// 플레이어가 '전략 씬' 또는 '영주 선택 씬'에서 선택한 영주의 고유 ID 문자열입니다. (예: "SevereCold", "Imperial")
    public static string SelectedLordType { get; set; } = "SevereCold"; // 프로토타입을 위해 기본값 설정


    // --- 2. 몬스터 정보 ---
    /// 플레이어가 전투를 시작할 때 상대할 몬스터 덱의 고유 ID 문자열입니다. (예: "Tutorial_Monsters", "Boss_Alcyoneus")
    public static string SelectedMonsterDeck { get; set; } = "Tutorial_Monsters";


    // (나중에 여기에 플레이어의 '명성', '골드' 등 씬을 넘나들며 유지되어야 하는 데이터 추가)

    // public static int PlayerFame = 0;
    // public static int PlayerGold = 15;



    // ------- 기타 -------
    // '카드 이름'을 Key로, '영구 보너스 스탯'을 Value로 저장
    // public static Dictionary<string, float> PermanentCardBuffs = new Dictionary<string, float>();
}