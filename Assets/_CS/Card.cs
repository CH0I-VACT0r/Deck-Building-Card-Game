
/// 모든 카드(용병, 몬스터, 장비, 건축물 등등)의 공통 설계 추상 클래스
/// 이 클래스를 상속받아 실제 카드 제작하면 됨

public abstract class Card
{
    // 1. 공통 데이터 
    // 모든 카드가 공통적으로 가지는 속성

    /// 카드 이름
    public string CardName { get; protected set; }

    /// 카드의 기본 스킬 쿨타임 (초)
    public float CooldownTime { get; protected set; }

    /// 현재 남은 쿨타임. 0이 되면 스킬 발동
    public float CurrentCooldown { get; set; }

    /// 이 카드를 소유하고 관리하는 '주인' (플레이어 또는 몬스터)
    protected PlayerController m_Owner;


    // --- 2. 생성자 (카드가 처음 만들어질 때) ---

    /// 새 카드를 생성할 때 호출됩니다.
    /// <param name="owner">이 카드를 소유할 컨트롤러(PlayerController 또는 MonsterController)</param>
    /// <param name="cooldown">이 카드의 기본 쿨타임</param>
    /// 
    public Card(PlayerController owner, float cooldown)
    {
        this.m_Owner = owner;
        this.CooldownTime = cooldown;
        this.CurrentCooldown = cooldown; // 전투 시작 시 쿨타임이 가득 찬 상태로 시작
        this.CardName = "Default Card Name"; 
    }


    // --- 3. 핵심 함수 ---

    /// [핵심 추상 함수]
    /// 카드의 고유 스킬 로직
    /// 이 클래스를 상속받는 모든 카드는, 이 함수의 내용물을 반드시' 자신만의 로직으로 덮어써야(override) 합니다.

    public abstract void ExecuteSkill();

    /// [공통 함수]
    /// BattleManager가 매 프레임 호출하여 쿨타임을 줄여주는 함수입니다.
    /// virtual: 자식 클래스에서 이 함수를 수정(override)할 수도 있습니다.
    /// <param name="deltaTime">Time.deltaTime (프레임당 시간)</param>
 
    public virtual void UpdateCooldown(float deltaTime)
    {
        if (CurrentCooldown > 0)
        {
            CurrentCooldown -= deltaTime;
        }
    }
}
