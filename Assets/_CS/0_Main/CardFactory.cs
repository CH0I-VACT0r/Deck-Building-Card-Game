// 파일명: CardFactory.cs
using UnityEngine;

// '카드 ID' 문자열을 기반 실제 Card 객체 생성 데이터베이스
public static class CardFactory
{
    //카드 ID, 주인, 슬롯을 받아 카드 생성
    public static Card CreateCard(string cardID, object owner, int index)
    {
        PlayerController playerOwner = owner as PlayerController;
        MonsterController monsterOwner = owner as MonsterController;

        switch (cardID)
        {
            // --- '혹한의 성주' 카드들 ---
            case "BarbarianWarrior":
                if (playerOwner != null)
                    return new Card_BarbarianWarrior(playerOwner, index);
                break;

            case "Shieldbearer":
                if (playerOwner != null)
                    return new Card_Shieldbearer(playerOwner, index);
                break;

            // (나중에 추가...)
            // case "":
            //    if (playerOwner != null) return new ~~~ (playerOwner, index);
            //    break;

            // --- '몬스터' 카드들 ---
            case "Goblin":
                if (monsterOwner != null)
                    return new Card_Goblin(monsterOwner, index);
                break;
        }

        // 덱 설정이 잘못되었거나, 알 수 없는 ID일 경우
        Debug.LogError($"[CardFactory] 알 수 없는 cardID({cardID})이거나, " +
                       $"잘못된 주인(Owner) 타입입니다! (Owner: {owner.GetType()})");
        return null;
    }
}
