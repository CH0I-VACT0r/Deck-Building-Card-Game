// 파일명: Card_RallyingStandard.cs
using UnityEngine;

/// <summary>
/// [결집의 깃발] (아우라 카드)
/// '존재'하는 것만으로 양옆에 인접한 모든 아군 카드에게
/// 치명타 확률 +50% '아우라'를 부여합니다.
/// </summary>
public class Card_CritTest : Card // Card 뼈대를 상속
{
    // --- 1. 생성자 ---
    public Card_CritTest(PlayerController owner, int index)
        : base(owner, index, 99999f) // (쿨타임 99초, 사실상 패시브 아우라 전용)
    {
        this.CardName = "결집의 깃발";
        this.Rarity = CardRarity.Silver; // (실버 등급)

        // [역할 데이터] (아무런 스킬 수치가 없음)

        this.CardImage = Resources.Load<Sprite>("CardImages/Lord_SevereCold/Healer"); 

        this.Tags.Add("#건축물"); // (토템/깃발)
        this.Tags.Add("#아우라");
        this.ShowCooldownUI = false;
    }

    // --- 2. 핵심 스킬 로직 (아무것도 안 함) ---
    public override void ExecuteSkill()
    {
        // 이 카드는 쿨타임 스킬이 없습니다 (패시브 아우라 전용).
        Debug.Log($"[{this.CardName}] (이)가 전장에 서서 아군을 격려합니다.");
    }


    // --- 3. [핵심!] '아우라' 재정의 ---

    /// <summary>
    /// [재정의!] (Card.cs의 뼈대 함수)
    /// 다른 카드(recipient)가 나에게 버프를 '질문'하면, '대답'합니다.
    /// </summary>
    /// <param name="recipient">버프를 받으려는 카드 (예: 전투병)</param>
    /// <param name="buffType">요청하는 버프 종류 (예: "CritChance")</param>
    /// <returns>버프 수치 (50% = 0.5f)</returns>
    public override float GetAuraBuffTo(Card recipient, string buffType)
    {
        // 1. "혹시 '치명타(CritChance)' 버프를 찾으시나요?"
        if (buffType == "CritChance")
        {
            // 2. (조건 없음. 그냥 인접한 모든 아군에게 줍니다)
            Debug.Log($"[{this.CardName}] (이)가 {recipient.CardName}에게 50% 치명타 버프를 부여합니다.");

            // 3. "그렇다면 여기 +50% (0.5f)를 드립니다."
            return 0.5f;
        }

        // "그 외에는 드릴 버프가 없습니다."
        return 0f;
    }
}
