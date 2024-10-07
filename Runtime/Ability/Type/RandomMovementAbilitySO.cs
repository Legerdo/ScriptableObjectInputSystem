using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// 일정 간격마다 랜덤한 방향으로 이동하며 플레이어 입력을 막는 어빌리티.
/// </summary>
[CreateAssetMenu(fileName = "RandomMovementAbility", menuName = "InputAbility/RandomMovement")]
public class RandomMovementAbilitySO : InputAbilitySO
{
    // 중복 구독을 방지하기 위한 플래그
    private bool isSubscribed = false;

    // 랜덤 움직임 변경을 위한 취소 토큰 소스
    private CancellationTokenSource movementCancellationTokenSource;

    // 현재 랜덤한 움직임 벡터
    private Vector2 currentRandomMovement = Vector2.zero;

    [Tooltip("움직임 방향을 변경하는 간격 (초 단위).")]
    public float changeInterval = 1.0f;

    /// <summary>
    /// 어빌리티를 활성화할 때 호출됨.
    /// 움직임 처리 이벤트에 ProcessMovement 메서드를 구독하고 랜덤 움직임을 시작.
    /// </summary>
    public override void OnActivate(InputAbilityEventChannelSO eventChannel)
    {
        if (eventChannel == null)
        {
            Debug.LogError("InputAbilityEventChannelSO가 null입니다. RandomMovementAbility를 활성화할 수 없습니다.");
            return;
        }

        if (!isSubscribed)
        {
            eventChannel.OnProcessMovement += ProcessMovement;
            isSubscribed = true;
            Debug.Log($"{this.name}이 활성화되어 OnProcessMovement에 구독되었습니다.");

            // 랜덤 움직임 변경을 위한 작업 시작
            movementCancellationTokenSource = new CancellationTokenSource();
            ChangeDirectionPeriodicallyAsync(movementCancellationTokenSource.Token).Forget();
        }
    }

    /// <summary>
    /// 어빌리티를 비활성화할 때 호출됨.
    /// 움직임 처리 이벤트에서 ProcessMovement 메서드 구독을 해제하고 랜덤 움직임을 중지.
    /// </summary>
    public override void OnDeactivate(InputAbilityEventChannelSO eventChannel)
    {
        if (eventChannel == null)
        {
            Debug.LogError("InputAbilityEventChannelSO가 null입니다. RandomMovementAbility를 비활성화할 수 없습니다.");
            return;
        }

        if (isSubscribed)
        {
            eventChannel.OnProcessMovement -= ProcessMovement;
            isSubscribed = false;
            Debug.Log($"{this.name}이 비활성화되어 OnProcessMovement에서 구독 해제되었습니다.");

            // 랜덤 움직임 변경 작업 취소
            if (movementCancellationTokenSource != null)
            {
                movementCancellationTokenSource.Cancel();
                movementCancellationTokenSource.Dispose();
                movementCancellationTokenSource = null;
            }
        }
    }

    /// <summary>
    /// 움직임 데이터를 현재 랜덤한 움직임으로 설정하여 플레이어 입력을 막음.
    /// </summary>
    /// <param name="movementData">수정 가능한 움직임 데이터.</param>
    private void ProcessMovement(MovementData movementData)
    {
        if (movementData == null)
        {
            Debug.LogError("ProcessMovement: MovementData가 null입니다.");
            return;
        }

        // 플레이어 입력을 무시하고 랜덤한 움직임으로 설정
        movementData.Movement = currentRandomMovement;
    }

    /// <summary>
    /// 일정 간격마다 움직임 방향을 랜덤하게 변경하는 비동기 메서드.
    /// </summary>
    /// <param name="cancellationToken">취소 토큰.</param>
    private async UniTaskVoid ChangeDirectionPeriodicallyAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // 상, 하, 좌, 우 중 하나의 방향을 랜덤하게 선택
            int randomIndex = Random.Range(0, 4);
            switch (randomIndex)
            {
                case 0:
                    currentRandomMovement = Vector2.up;    // 위 방향 (0, 1)
                    break;
                case 1:
                    currentRandomMovement = Vector2.down;  // 아래 방향 (0, -1)
                    break;
                case 2:
                    currentRandomMovement = Vector2.left;  // 왼쪽 방향 (-1, 0)
                    break;
                case 3:
                    currentRandomMovement = Vector2.right; // 오른쪽 방향 (1, 0)
                    break;
            }

            Debug.Log($"{this.name}: 방향이 {currentRandomMovement}로 변경되었습니다.");

            // 지정된 간격만큼 대기하거나 취소 시 중단
            try
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(changeInterval), cancellationToken: cancellationToken);
            }
            catch (System.OperationCanceledException)
            {
                // 작업이 취소됨
                break;
            }
        }
    }

    /// <summary>
    /// 어빌리티를 활성화하는 비동기 메서드.
    /// </summary>
    public override UniTask ActivateAsync(InputAbilityEventChannelSO eventChannel, CancellationToken cancellationToken)
    {
        // 어빌리티 활성화 로직은 OnActivate에서 처리
        OnActivate(eventChannel);
        return UniTask.CompletedTask;
    }

    /// <summary>
    /// 어빌리티를 비활성화하는 비동기 메서드.
    /// </summary>
    public override UniTask DeactivateAsync(InputAbilityEventChannelSO eventChannel)
    {
        // 어빌리티 비활성화 로직은 OnDeactivate에서 처리
        OnDeactivate(eventChannel);
        return UniTask.CompletedTask;
    }
}
