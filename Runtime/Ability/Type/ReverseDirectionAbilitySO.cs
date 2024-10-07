using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// 움직임 방향을 반전시키는 어빌리티.
/// </summary>
[CreateAssetMenu(fileName = "ReverseDirectionAbility", menuName = "InputAbility/ReverseDirection")]
public class ReverseDirectionAbilitySO : InputAbilitySO
{
    // 중복 구독을 방지하기 위한 플래그
    private bool isSubscribed = false;

    /// <summary>
    /// 어빌리티를 활성화할 때 호출됨.
    /// 움직임 처리 이벤트에 ReverseMovement 메서드를 구독.
    /// </summary>
    public override void OnActivate(InputAbilityEventChannelSO eventChannel)
    {
        if (eventChannel == null)
        {
            Debug.LogError("InputAbilityEventChannelSO가 null입니다. ReverseDirectionAbility를 활성화할 수 없습니다.");
            return;
        }

        if (!isSubscribed)
        {
            eventChannel.OnProcessMovement += ReverseMovement;
            isSubscribed = true;
            Debug.Log($"{this.name}이 활성화되어 OnProcessMovement에 구독되었습니다.");
        }
    }

    /// <summary>
    /// 어빌리티를 비활성화할 때 호출됨.
    /// 움직임 처리 이벤트에서 ReverseMovement 메서드 구독을 해제.
    /// </summary>
    public override void OnDeactivate(InputAbilityEventChannelSO eventChannel)
    {
        if (eventChannel == null)
        {
            Debug.LogError("InputAbilityEventChannelSO가 null입니다. ReverseDirectionAbility를 비활성화할 수 없습니다.");
            return;
        }

        if (isSubscribed)
        {
            eventChannel.OnProcessMovement -= ReverseMovement;
            isSubscribed = false;
            Debug.Log($"{this.name}이 비활성화되어 OnProcessMovement에서 구독 해제되었습니다.");
        }
    }

    /// <summary>
    /// 움직임 데이터를 반전시키는 이벤트 핸들러.
    /// </summary>
    /// <param name="movementData">수정 가능한 움직임 데이터.</param>
    private void ReverseMovement(MovementData movementData)
    {
        if (movementData == null)
        {
            Debug.LogError("ReverseMovement: MovementData가 null입니다.");
            return;
        }

        Debug.Log("ReverseDirectionAbility: 움직임 방향을 반전시킵니다.");
        movementData.Movement = -movementData.Movement;
    }

    /// <summary>
    /// 어빌리티를 활성화하는 비동기 메서드.
    /// </summary>
    public override UniTask ActivateAsync(InputAbilityEventChannelSO eventChannel, CancellationToken cancellationToken)
    {
        // 어빌리티 활성화 로직은 OnActivate에서 처리되므로 별도의 비동기 작업이 필요 없습니다.
        OnActivate(eventChannel);
        return UniTask.CompletedTask;
    }

    /// <summary>
    /// 어빌리티를 비활성화하는 비동기 메서드.
    /// </summary>
    public override UniTask DeactivateAsync(InputAbilityEventChannelSO eventChannel)
    {
        // 어빌리티 비활성화 로직은 OnDeactivate에서 처리되므로 별도의 비동기 작업이 필요 없습니다.
        OnDeactivate(eventChannel);
        return UniTask.CompletedTask;
    }
}
