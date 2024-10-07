using UnityEngine;
using System;

/// <summary>
/// 움직임 처리 이벤트를 관리하는 이벤트 채널.
/// 어빌리티가 움직임 데이터를 수정할 수 있도록 MovementData를 전달.
/// </summary>
[CreateAssetMenu(fileName = "InputAbilityEventChannel", menuName = "InputAbility/Event/InputAbilityEventChannel")]
public class InputAbilityEventChannelSO : ScriptableObject
{
    /// <summary>
    /// 움직임 처리 이벤트. MovementData를 인자로 전달.
    /// </summary>
    public event Action<MovementData> OnProcessMovement;

    /// <summary>
    /// 움직임 처리 이벤트를 발생시킴.
    /// </summary>
    /// <param name="movementData">수정 가능한 움직임 데이터.</param>
    public void RaiseProcessMovement(MovementData movementData)
    {
        if (movementData == null)
        {
            Debug.LogError("MovementData is null. Cannot raise ProcessMovement event.");
            return;
        }

        OnProcessMovement?.Invoke(movementData);
    }

    /// <summary>
    /// 모든 리스너를 제거함. (주의: 예상치 못한 동작을 초래할 수 있음)
    /// </summary>
    public void RemoveAllListeners()
    {
        OnProcessMovement = null;
    }
}
