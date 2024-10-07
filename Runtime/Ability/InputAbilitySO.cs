using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

/// <summary>
/// 모든 입력 어빌리티의 기본 클래스.
/// 어빌리티의 활성화 및 비활성화 메서드를 정의.
/// </summary>
public abstract class InputAbilitySO : ScriptableObject, IAbilityBehavior
{
    [Tooltip("어빌리티가 활성화된 상태로 유지되는 시간 (초 단위).")]
    public float duration = 3.0f;

    /// <summary>
    /// 이 어빌리티가 지속적인 움직임 처리를 요구하는지 여부.
    /// </summary>
    public bool requiresContinuousMovement = false;
    
    /// <summary>
    /// 이 어빌리티가 재사용될 경우 기존 어빌리티 시간을 갱신 할지 여부.
    /// </summary>
    public bool refreshAbility = false;

    /// <summary>
    /// 어빌리티를 활성화할 때 호출되는 메서드.
    /// 이벤트에 구독하여 어빌리티 효과를 적용.
    /// </summary>
    /// <param name="eventChannel">이벤트 채널.</param>
    public abstract void OnActivate(InputAbilityEventChannelSO eventChannel);

    /// <summary>
    /// 어빌리티를 비활성화할 때 호출되는 메서드.
    /// 이벤트 구독을 해제하여 어빌리티 효과를 제거.
    /// </summary>
    /// <param name="eventChannel">이벤트 채널.</param>
    public abstract void OnDeactivate(InputAbilityEventChannelSO eventChannel);

    /// <summary>
    /// 어빌리티를 활성화하는 비동기 메서드.
    /// 기본 구현은 동기적으로 처리.
    /// </summary>
    public virtual UniTask ActivateAsync(InputAbilityEventChannelSO eventChannel, CancellationToken cancellationToken)
    {
        OnActivate(eventChannel);
        return UniTask.CompletedTask;
    }

    /// <summary>
    /// 어빌리티를 비활성화하는 비동기 메서드.
    /// 기본 구현은 동기적으로 처리.
    /// </summary>
    public virtual UniTask DeactivateAsync(InputAbilityEventChannelSO eventChannel)
    {
        OnDeactivate(eventChannel);
        return UniTask.CompletedTask;
    }
}
