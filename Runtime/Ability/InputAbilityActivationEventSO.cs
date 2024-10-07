using UnityEngine;
using System;

/// <summary>
/// 어빌리티 활성화 이벤트를 관리하는 이벤트 채널.
/// </summary>
[CreateAssetMenu(fileName = "AbilityActivationEvent", menuName = "InputAbility/AbilityActivationEvent")]
public class InputAbilityActivationEventSO : ScriptableObject
{
    /// <summary>
    /// 어빌리티 활성화 이벤트. InputAbilitySO를 인자로 전달.
    /// </summary>
    public event Action<InputAbilitySO> OnAbilityActivation;

    /// <summary>
    /// 어빌리티 활성화 이벤트를 발생시킴.
    /// </summary>
    /// <param name="ability">활성화할 어빌리티.</param>
    public void RaiseEvent(InputAbilitySO ability)
    {
        if (ability == null)
        {
            Debug.LogError("Ability is null. Cannot raise AbilityActivation event.");
            return;
        }

        OnAbilityActivation?.Invoke(ability);
    }

    /// <summary>
    /// 모든 리스너를 제거함. (주의: 예상치 못한 동작을 초래할 수 있음)
    /// </summary>
    public void RemoveAllListeners()
    {
        OnAbilityActivation = null;
    }
}
