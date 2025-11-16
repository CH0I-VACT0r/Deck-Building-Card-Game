using System.Collections.Generic;
using UnityEngine;

// (프로토타입용 간단한 번역기)
/// 텍스트 키(Key)를 받아서, 현재 언어에 맞는 실제 텍스트 반환

public static class LocalizationManager
{
    // TODO: 나중에 이 값을 "en", "jp" 등으로 바꾸면 언어가 바뀝니다.
    private static string m_CurrentLanguage = "ko";

    // 한국어 사전 (ko)
    private static Dictionary<string, string> m_KoreanDict = new Dictionary<string, string>()
    {
        // TODO: 여기에 모든 카드 텍스트 키 추가
        { "quest_status_complete", "완료" },
        { "quest_status_incomplete", "진행 중" },
        { "stat_crit_chance", "치명타 확률: {0}%" },

        { "tag_mercenary", "용병" },
        { "tag_barbarian", "야만전사" },
        { "tag_dealer", "딜러" },
        { "tag_monster", "몬스터" },
        { "tag_beast", "야수" },

        // 바바리안 전사
        { "card_barbarian_warrior_name", "바바리안 전사" },
        { "card_barbarian_warrior_skill_desc", "쿨타임마다 {0}의 피해를 줍니다." },
        { "card_barbarian_warrior_quest_title", "[이기자!]" },
        { "card_barbarian_warrior_quest_desc", "전투에서 3회 승리" },
        { "card_barbarian_warrior_flavor", "\"우어어어어어!!\"" },

        { "card_goblin_name", "고블린" },
        { "card_goblin_skill_desc", "쿨타임마다 맞은편의 적을 공격하여 {0}의 피해를 줍니다." },
        { "card_goblin_flavor", "\"키 작다고 얕보지 마라!\"" },

        

        
    };

    // 영어 사전 (en)
    private static Dictionary<string, string> m_EnglishDict = new Dictionary<string, string>()
    {
        { "quest_status_complete", "Complete" },
        { "quest_status_incomplete", "In Progress" },
        { "stat_crit_chance", "Crit Chance: {0}%" },
        
        { "tag_mercenary", "Mercenary" },
        { "tag_barbarian", "Barbarian" },
        { "tag_dealer", "Dealer" },
        { "tag_monster", "Monster" },
        { "tag_beast", "Beast" },

        // 바바리안 전사
        { "card_barbarian_warrior_name", "Barbarian Warrior" },
        { "card_barbarian_warrior_skill_desc", "Deals {0} damage every cooldown." },
        { "card_barbarian_warrior_quest_title", "[Let's Win!]" },
        { "card_barbarian_warrior_quest_desc", "Win 3 battles" },
        { "card_barbarian_warrior_flavor", "\"Waaaaaaaaagh!!\"" },

        { "card_goblin_name", "Goblin" },
        { "card_goblin_skill_desc", "Attacks the opposite enemy for {0} damage every cooldown." },
        { "card_goblin_flavor", "\"Don't look down on me just because I'm short!\"" },
    };

    /// <summary>
    /// "키"를 주면 현재 언어에 맞는 "텍스트"를 반환합니다. (기본)
    /// </summary>
    public static string GetText(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return ""; // 키가 비어있으면 빈 문자열 반환
        }

        Dictionary<string, string> targetDict = null;

        if (m_CurrentLanguage == "ko")
        {
            targetDict = m_KoreanDict;
        }
        else if (m_CurrentLanguage == "en")
        {
            targetDict = m_EnglishDict;
        }
        // (나중에 다른 언어 추가)

        if (targetDict == null) return $"[NO LANG: {key}]";

        // 사전에서 키를 찾아 텍스트를 반환
        if (targetDict.TryGetValue(key, out string value))
        {
            return value;
        }

        // 사전에 키가 없으면 경고용 텍스트 반환
        return $"[MISSING: {key}]";
    }

    // 언어 설정
    public static void SetLanguage(string languageCode) // "en", "ko", "jp" ...
    {
        // TODO: 실제로 그 언어 사전이 존재하는지 확인하는 로직이 있으면 더 좋습니다.
        m_CurrentLanguage = languageCode;
        Debug.Log($"[Localization] 언어가 {m_CurrentLanguage}(으)로 변경되었습니다.");
    }

    // 현재 언어에 맞는 "텍스트"를 반환
    public static string GetText(string key, params object[] args)
    {
        // 1. 기본 번역 텍스트를 가져옵니다. (예: "...{0}의 피해...")
        string baseText = GetText(key);

        if (string.IsNullOrEmpty(baseText) || baseText.StartsWith("["))
        {
            return baseText; // 키가 없거나 에러가 난 텍스트는 포맷팅하지 않음
        }

        // 2. C#의 string.Format 기능을 사용해 {0} 자리에 args[0] (피해량)을 끼워넣습니다.
        try
        {
            return string.Format(baseText, args);
        }
        catch (System.Exception)
        {
            // 포맷팅에 실패하면 (예: {0}은 있는데 args가 없으면) 경고용 텍스트 반환
            return $"[FORMAT ERR: {key}]";
        }
    }
}
