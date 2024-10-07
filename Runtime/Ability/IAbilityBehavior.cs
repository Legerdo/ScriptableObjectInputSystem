using Cysharp.Threading.Tasks;
using System.Threading;

public interface IAbilityBehavior
{
    /// <summary>
    /// 어빌리티를 활성화하는 비동기 메서드.
    /// </summary>
    /// <param name="eventChannel">이벤트 채널.</param>
    /// <param name="cancellationToken">취소 토큰.</param>
    UniTask ActivateAsync(InputAbilityEventChannelSO eventChannel, CancellationToken cancellationToken);

    /// <summary>
    /// 어빌리티를 비활성화하는 비동기 메서드.
    /// </summary>
    /// <param name="eventChannel">이벤트 채널.</param>
    UniTask DeactivateAsync(InputAbilityEventChannelSO eventChannel);
}
