using UnityEngine;

/// <summary>
/// 게임에 존재하는 모든 상태 이상을 정의합니다.
/// </summary>
public enum StatusEffectType
{
    None,
    Bleed,  // 출혈
    Poison,  // 중독
    Burn, // 화상
    Freeze // 빙결
    // (나중에 '기절' 등 추가...)
}