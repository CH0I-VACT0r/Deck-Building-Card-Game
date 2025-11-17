using UnityEngine;

/// <summary>
/// 게임에 존재하는 모든 상태 이상을 정의합니다.
/// </summary>
public enum StatusEffectType
{
    None,
    Bleed,  // 출혈
    Poison,  // 중독
    Heal, // 회복
    Burn, // 화상
    Freeze, // 빙결
    Haste, // 가속
    Slow, // 감속
    Echo, // 스킬 반복 시전
    Shock, // 충격 : 받는 피해 증가
    Sturdy, // 견고 : 받는 피해 감소
    PriceInflate, // 가격 인상
    PriceExtort,   // 가격 인하
    // (나중에 추가...)
}