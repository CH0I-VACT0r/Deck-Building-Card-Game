using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    // --- 자원 ---
    public int Gold { get; private set; } = 15; // 초기 자금

    // --- 인벤토리 (카드 ID 문자열 리스트) ---
    // 예: ["barbarian", "potion", "potion", "iron_ore"]
    public List<string> OwnedCardIDs { get; private set; } = new List<string>();

    // --- 싱글톤 (어디서든 접근 가능하게) ---
    public static InventoryManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // [테스트] 시작 아이템 지급 (CardFactory에 있는 case 이름이어야 함!)
        // AddCard("merc_barbarian"); // 바바리안
    }

    // --- 기능 구현 ---

    // 아이템 획득
    public void AddCard(string cardID)
    {
        // 실제 존재하는 카드인지 확인 (Factory 이용)
        // 주인이 없는(null) 더미 카드를 살짝 만들어서 확인만 하고 버림
        Card tempCard = CardFactory.CreateCard(cardID, null, -1);

        if (tempCard == null)
        {
            Debug.LogError($"[Inventory] 존재하지 않는 카드 ID: {cardID}");
            return;
        }

        OwnedCardIDs.Add(cardID);
        Debug.Log($"[Inventory] 획득: {cardID}");

        // UI 갱신 요청 (나중에 연결)
        // UIManager.Instance.RefreshInventoryUI(); 
    }

    public void RemoveCard(string cardID)
    {
        if (OwnedCardIDs.Contains(cardID))
        {
            OwnedCardIDs.Remove(cardID); // 리스트에서 하나만 제거됨
            Debug.Log($"[Inventory] 소모/판매: {cardID}");
        }
    }

    // 골드 변경 (획득/소모)
    public bool ModifyGold(int amount)
    {
        if (Gold + amount < 0) return false;
        Gold += amount;
        return true;
    }
}
