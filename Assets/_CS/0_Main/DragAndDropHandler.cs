using UnityEngine;
using UnityEngine.UIElements;

// Drag and Drop 처리용 핸들러 클래스
public class DragAndDropHandler : PointerManipulator
{
    /// D&D 상태 변수
    private bool m_IsDragging = false;
    private Vector2 m_StartMousePosition;
    private Vector2 m_StartOffset;
    private VisualElement m_Root;
    public int StartSlotIndex { get; private set; } = -1;

    // [수정!] object로 통일 (CS0656 에러 방지)
    private object m_OwnerController;

    private Card m_PickedUpCard;

    // --- 1. 생성자 ---
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
           
            if (!playerOwner.GetBattleManager().IsDeckEditingAllowed) return;
            if (m_IsDragging || evt.button != 0) return;

            // 툴팁 취소 및 카드 정보/인덱스 저장
            playerOwner.ClearTooltipScheduler();
            StartSlotIndex = playerOwner.GetSlotIndexFromTarget(target);
            m_PickedUpCard = playerOwner.GetCardAtIndex(StartSlotIndex);
            if (m_PickedUpCard == null) return;

            // 배열에서 제거 및 왼쪽으로 시프트
            playerOwner.RemoveCard(StartSlotIndex);

            // 드래그 상태 및 위치 저장
            m_IsDragging = true;
            m_StartMousePosition = evt.position;

            // 현재 transform offset을 Vector2로 저장
            StyleTranslate translateStyle = target.style.translate;
            Translate currentTranslate = translateStyle.value;

            m_StartOffset = new Vector2(currentTranslate.x.value,
                                        currentTranslate.y.value);

            // 시각적 변경 및 마우스 캡처
            target.BringToFront();
            target.style.position = new StyleEnum<Position>(Position.Absolute);
            target.CapturePointer(evt.pointerId);

            evt.StopPropagation();
        }
        else if (m_OwnerController is MonsterController monsterOwner)
        {
            if (!monsterOwner.GetBattleManager().IsDeckEditingAllowed) return;
            // (몬스터 카드는 드래그하지 않으므로, 여기서 그냥 return 할 수도 있다.)
        }
    }

    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (!m_IsDragging || !target.HasPointerCapture(evt.pointerId)) return;

        // 마우스가 움직인 만큼만 타겟을 이동
        Vector2 mousePos = evt.position;
        Vector2 startPos = m_StartMousePosition;

        Vector2 delta = mousePos - startPos;


        // 시작 오프셋에 델타를 더하여 최종 위치를 결정
        target.style.translate = new StyleTranslate(new Translate(
            delta.x + m_StartOffset.x,
            delta.y + m_StartOffset.y
        ));

        evt.StopPropagation();
    }


    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (!m_IsDragging || !target.HasPointerCapture(evt.pointerId)) return;

        m_IsDragging = false;
        target.ReleasePointer(evt.pointerId);

        if (m_OwnerController is PlayerController playerOwner)
        {
            // 마우스를 놓은 위치의 VisualElement 인덱스 확인
            VisualElement dropElement = evt.target as VisualElement;
            int dropIndex = playerOwner.GetSlotIndexFromTarget(dropElement);

            // 드롭 인덱스 결정
            if (dropIndex == -1 || dropIndex == StartSlotIndex) // 원래 자리 복귀 또는 유효하지 않음
            {
                dropIndex = StartSlotIndex;
            }

            // 배열에 삽입 및 시각적 복구
            playerOwner.InsertCard(dropIndex, m_PickedUpCard);
            m_PickedUpCard = null;
        }
        // (몬스터 컨트롤러는 드롭 로직 없음)

        SnapBackToOrigin();
        evt.StopPropagation();
    }

    private void SnapBackToOrigin()
    {
        target.style.position = new StyleEnum<Position>(Position.Relative);
        target.style.translate = new StyleTranslate(new Translate(0, 0));
    }
}