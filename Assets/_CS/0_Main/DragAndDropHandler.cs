using UnityEngine;
using UnityEngine.UIElements;

public class DragAndDropHandler : PointerManipulator
{
    private bool m_IsDragging = false;
    private VisualElement m_Root; // 전체 화면 루트 (고스트를 띄울 공간)
    private VisualElement m_GhostIcon; // 마우스 따라다닐 가짜 아이콘

    public int StartSlotIndex { get; private set; } = -1;
    private object m_OwnerController;

    // 드래그 시작 시점의 오프셋 (마우스가 카드의 어디를 잡았는지)
    private Vector2 m_PointerOffset;

    public DragAndDropHandler(VisualElement target, VisualElement root, object controller)
    {
        this.target = target;
        m_Root = root;
        m_OwnerController = controller;
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
        target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
    }

    private void PointerDownHandler(PointerDownEvent evt)
    {
        if (m_OwnerController is PlayerController playerOwner)
        {
            // 1. 잠금 확인
            if (!playerOwner.GetBattleManager().IsDeckEditingAllowed) return;
            if (m_IsDragging || m_Root == null) return;

            // 2. 데이터 확인
            StartSlotIndex = playerOwner.GetSlotIndexFromTarget(target);
            Card card = playerOwner.GetCardAtIndex(StartSlotIndex);
            if (card == null) return;

            // 3. 드래그 시작
            m_IsDragging = true;
            target.CapturePointer(evt.pointerId);
            playerOwner.ClearTooltipScheduler();

            // 4. [핵심] 고스트 아이콘 생성 (Root에 붙임)
            CreateGhostIcon(card, evt.position);

            // 5. 원본 슬롯은 흐릿하게 처리 (데이터는 아직 그대로 있음!)
            target.style.opacity = 0.3f;

            evt.StopPropagation();
        }
    }

    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (!m_IsDragging || !target.HasPointerCapture(evt.pointerId)) return;

        // 6. 고스트 아이콘 이동
        if (m_GhostIcon != null)
        {
            // 마우스 위치(World)를 Root 기준 로컬 좌표로 변환
            Vector2 mousePos = evt.position; // 명시적 Vector2
            Vector2 localPos = m_Root.WorldToLocal(mousePos);

            // 오프셋 적용하여 위치 설정
            m_GhostIcon.style.left = localPos.x - m_PointerOffset.x;
            m_GhostIcon.style.top = localPos.y - m_PointerOffset.y;
        }
    }

    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (!m_IsDragging || !target.HasPointerCapture(evt.pointerId)) return;

        m_IsDragging = false;
        target.ReleasePointer(evt.pointerId);

        // 7. 드롭 위치 계산
        if (m_OwnerController is PlayerController playerOwner)
        {
            // 마우스 아래에 있는 요소 찾기
            Vector2 mousePos = evt.position;
            VisualElement dropTarget = m_Root.panel.Pick(mousePos);

            // 그 요소가 속한 슬롯 찾기 (부모 타고 올라가며 검색)
            int dropIndex = -1;
            VisualElement current = dropTarget;

            // (루프 안전장치 추가: 10번까지만 상위 검색)
            int depth = 0;
            while (current != null && depth < 10)
            {
                dropIndex = playerOwner.GetSlotIndexFromTarget(current);
                if (dropIndex != -1) break;
                current = current.parent;
                depth++;
            }

            // 8. [핵심] 데이터 이동 (MoveCard)
            if (dropIndex != -1 && dropIndex != StartSlotIndex)
            {
                Debug.Log($"[D&D] {StartSlotIndex} -> {dropIndex} 이동");
                playerOwner.MoveCard(StartSlotIndex, dropIndex);
            }
            else
            {
                Debug.Log("[D&D] 원래 위치로 복귀");
            }

            // 9. 뒷정리
            target.style.opacity = 1f; // 원본 불투명도 복구

            if (m_GhostIcon != null)
            {
                if (m_Root.Contains(m_GhostIcon))
                {
                    m_Root.Remove(m_GhostIcon);
                }
                m_GhostIcon = null;
            }
        }

        evt.StopPropagation();
    }

    // [헬퍼] 고스트 아이콘 생성 함수
    private void CreateGhostIcon(Card card, Vector2 mousePosition)
    {
        m_GhostIcon = new VisualElement();

        // 스타일 복사 (카드 이미지처럼 보이게)
        if (card.CardImage != null)
        {
            m_GhostIcon.style.backgroundImage = new StyleBackground(card.CardImage);
        }

        // 크기 설정 (원본 슬롯 크기 따라감)
        m_GhostIcon.style.width = target.resolvedStyle.width;
        m_GhostIcon.style.height = target.resolvedStyle.height;
        m_GhostIcon.style.position = Position.Absolute;

        // 마우스 커서가 카드의 중앙에 오도록 오프셋 설정
        m_PointerOffset = new Vector2(target.resolvedStyle.width / 2, target.resolvedStyle.height / 2);

        // 초기 위치 설정
        Vector2 localPos = m_Root.WorldToLocal(mousePosition);
        m_GhostIcon.style.left = localPos.x - m_PointerOffset.x;
        m_GhostIcon.style.top = localPos.y - m_PointerOffset.y;

        // 터치 무시 (드롭 시 밑에 있는 슬롯을 감지해야 하므로 필수!)
        m_GhostIcon.pickingMode = PickingMode.Ignore;

        m_Root.Add(m_GhostIcon);
    }
}