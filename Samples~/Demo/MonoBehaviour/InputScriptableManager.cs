using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using ScriptableObjectArchitecture;

/// <summary>
/// 입력과 어빌리티를 관리하는 매니저 클래스.
/// 입력 이벤트를 처리하고, 어빌리티의 활성화 및 비활성화를 관리.
/// </summary>
public class InputScriptableManager : MonoBehaviour
{
    [Header("Input and Movement")]
    [Tooltip("사용자의 입력을 처리하는 InputReader.")]
    public InputReader inputReader;

    [Tooltip("현재 움직임 방향을 저장하는 변수.")]
    public Vector2Variable moveDirection;

    [Header("Event Channels")]
    [Tooltip("움직임 처리를 위한 이벤트 채널.")]
    public InputAbilityEventChannelSO eventChannel;

    [Tooltip("어빌리티 활성화를 위한 이벤트 채널.")]
    public InputAbilityActivationEventSO abilityActivationEvent;

    [Header("Abilities")]
    [Tooltip("활성화할 수 있는 어빌리티 목록.")]
    [SerializeField]
    private List<InputAbilitySO> availableAbilities;

    // 활성화된 어빌리티와 해당 어빌리티의 취소 토큰 소스를 저장하는 딕셔너리
    private Dictionary<InputAbilitySO, CancellationTokenSource> activeAbilities = new Dictionary<InputAbilitySO, CancellationTokenSource>();

    // 마지막으로 입력된 움직임을 저장하여 어빌리티 활성화/비활성화 시 재처리
    private Vector2 lastMovementInput = Vector2.zero;

    // 지속적인 움직임 처리가 필요한지 여부를 나타내는 플래그
    private bool requiresContinuousMovement = false;

    // 재진입 방지 플래그
    private bool isReprocessingMovement = false;

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
        // 모든 활성 어빌리티를 비활성화하여 정리
        foreach (var ability in activeAbilities.Keys)
        {
            DeactivateAbility(ability);
        }
        activeAbilities.Clear();
    }

    private void Update()
    {
        // 지속적인 움직임 처리가 필요한 경우 매 프레임 움직임 처리
        if (requiresContinuousMovement)
        {
            ProcessContinuousMovement();
        }
    }

    /// <summary>
    /// 지속적인 움직임 처리를 수행하는 메서드.
    /// </summary>
    private void ProcessContinuousMovement()
    {
        // 마지막 입력된 움직임을 사용하거나, 없으면 Vector2.zero 사용
        Vector2 movementInput = lastMovementInput;

        MovementData movementData = new MovementData(movementInput);

        eventChannel.RaiseProcessMovement(movementData);

        ProcessMovement(movementData.Movement);
    }

    /// <summary>
    /// 입력과 어빌리티 활성화 이벤트에 구독.
    /// </summary>
    private void SubscribeEvents()
    {
        if (inputReader != null)
        {
            inputReader.MovePerformed += OnMovePerformed;
        }
        else
        {
            Debug.LogError("InputReader가 InputScriptableManager에 할당되지 않았습니다.");
        }

        if (abilityActivationEvent != null)
        {
            abilityActivationEvent.OnAbilityActivation += ActivateAbility;
        }
        else
        {
            Debug.LogError("InputAbilityActivationEventSO가 InputScriptableManager에 할당되지 않았습니다.");
        }
    }

    /// <summary>
    /// 입력과 어빌리티 활성화 이벤트에서 구독 해제.
    /// </summary>
    private void UnsubscribeEvents()
    {
        if (inputReader != null)
        {
            inputReader.MovePerformed -= OnMovePerformed;
        }

        if (abilityActivationEvent != null)
        {
            abilityActivationEvent.OnAbilityActivation -= ActivateAbility;
        }
    }

    /// <summary>
    /// 움직임 입력이 수행되었을 때 호출되는 콜백.
    /// 움직임 데이터를 처리하고, 움직임 방향을 업데이트.
    /// </summary>
    /// <param name="movement">입력된 움직임 벡터.</param>
    private void OnMovePerformed(Vector2 movement)
    {
        lastMovementInput = movement; // 마지막 입력 저장

        MovementData movementData = new MovementData(movement);

        eventChannel.RaiseProcessMovement(movementData);

        ProcessMovement(movementData.Movement);
    }

    /// <summary>
    /// 처리된 움직임을 moveDirection 변수에 적용.
    /// </summary>
    /// <param name="processedMovement">어빌리티에 의해 처리된 움직임 벡터.</param>
    private void ProcessMovement(Vector2 processedMovement)
    {
        moveDirection.Value = processedMovement;
    }

    /// <summary>
    /// 어빌리티를 활성화하고 관리하는 메서드.
    /// </summary>
    /// <param name="ability">활성화할 어빌리티.</param>
    public void ActivateAbility(InputAbilitySO ability)
    {
        if (ability == null)
        {
            Debug.LogError("Null 어빌리티를 활성화하려고 시도했습니다.");
            return;
        }

        if (!availableAbilities.Contains(ability))
        {
            Debug.LogWarning($"어빌리티 {ability.name}가 이 InputScriptableManager에 등록되어 있지 않습니다.");
            return;
        }

        if (activeAbilities.ContainsKey(ability))
        {
            if (ability.refreshAbility)
            {
                // 기존 CancellationTokenSource를 취소하고 새로 생성하여 지속 시간을 갱신
                CancellationTokenSource existingCts = activeAbilities[ability];
                existingCts.Cancel();
                existingCts.Dispose();

                CancellationTokenSource newCts = new CancellationTokenSource();
                activeAbilities[ability] = newCts;

                // 지속 시간 갱신을 위해 ActivateAbilityAsync를 다시 호출
                ActivateAbilityAsync(ability, newCts.Token).Forget();

                Debug.Log($"어빌리티 {ability.name}의 지속 시간이 갱신되었습니다.");
            }
            else
            {
                Debug.LogWarning($"어빌리티 {ability.name}이 이미 활성화되어 있습니다.");
            }
            return;
        }

        // 어빌리티 활성화 및 취소 토큰 생성
        CancellationTokenSource ctsNew = new CancellationTokenSource();
        activeAbilities[ability] = ctsNew;

        if (ability.requiresContinuousMovement)
        {
            requiresContinuousMovement = true;
        }

        // 어빌리티 활성화 비동기 메서드 호출
        ActivateAbilityAsync(ability, ctsNew.Token).Forget();
    }

    /// <summary>
    /// 어빌리티 활성화를 비동기적으로 처리하는 메서드.
    /// </summary>
    /// <param name="ability">활성화할 어빌리티.</param>
    /// <param name="cancellationToken">취소 토큰.</param>
    private async UniTaskVoid ActivateAbilityAsync(InputAbilitySO ability, CancellationToken cancellationToken)
    {
        Debug.Log($"어빌리티 {ability.name}을 활성화합니다.");
        
        await ability.ActivateAsync(eventChannel, cancellationToken);

        // 어빌리티의 활성화가 완료된 후 움직임을 재처리하여 효과를 즉시 반영 ( 기존에 움직임을 재처리 하기 때문에 OnProcessMovement 구독 이벤트가 2번 발생 할 수 있음 )
        // TODO : 아직 별 문제 없지만 추후 어빌리티를 추가로 구현시 문제가 발생하면 1번만 체크하도록 수정.
        ReprocessMovement();

        // 어빌리티의 지속 시간 동안 대기
        try
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(ability.duration), cancellationToken: cancellationToken);
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log($"어빌리티 {ability.name}의 활성화가 취소되었습니다.");
            return;
        }

        // 어빌리티 비활성화
        await DeactivateAbilityAsync(ability);

        // 어빌리티를 활성화 목록에서 제거
        activeAbilities.Remove(ability);

        Debug.Log($"어빌리티 {ability.name}이 자동으로 비활성화되었습니다.");
    }

    /// <summary>
    /// 어빌리티를 비활성화하는 메서드.
    /// </summary>
    /// <param name="ability">비활성화할 어빌리티.</param>
    public void DeactivateAbility(InputAbilitySO ability)
    {
        if (ability == null)
        {
            Debug.LogError("Null 어빌리티를 비활성화하려고 시도했습니다.");
            return;
        }

        if (!activeAbilities.ContainsKey(ability))
        {
            Debug.LogWarning($"어빌리티 {ability.name}이 활성화되어 있지 않습니다.");
            return;
        }

        // 취소 토큰을 통해 어빌리티의 비동기 작업을 취소
        CancellationTokenSource cts = activeAbilities[ability];
        cts.Cancel();
        cts.Dispose();
        activeAbilities.Remove(ability);

        // 어빌리티 비활성화 비동기 메서드 호출
        DeactivateAbilityAsync(ability).Forget();
    }

    /// <summary>
    /// 어빌리티 비활성화를 비동기적으로 처리하는 메서드.
    /// </summary>
    /// <param name="ability">비활성화할 어빌리티.</param>
    private async UniTask DeactivateAbilityAsync(InputAbilitySO ability)
    {
        // 활성화된 다른 어빌리티들이 지속적인 움직임을 요구하는지 확인
        requiresContinuousMovement = false;
        foreach (var activeAbility in activeAbilities.Keys)
        {
            if (activeAbility.requiresContinuousMovement)
            {
                requiresContinuousMovement = true;
                break;
            }
        }

        Debug.Log($"어빌리티 {ability.name}을 비활성화합니다.");
        await ability.DeactivateAsync(eventChannel);

        // 어빌리티 비활성화가 완료된 후 움직임을 재처리하여 효과를 즉시 반영
        ReprocessMovement();
    }

    /// <summary>
    /// 어빌리티 목록에 새로운 어빌리티를 추가.
    /// </summary>
    /// <param name="ability">추가할 어빌리티.</param>
    public void AddAbility(InputAbilitySO ability)
    {
        if (ability == null)
        {
            Debug.LogError("Null 어빌리티를 추가하려고 시도했습니다.");
            return;
        }

        if (!availableAbilities.Contains(ability))
        {
            availableAbilities.Add(ability);
            Debug.Log($"어빌리티 {ability.name}을 availableAbilities 목록에 추가했습니다.");
        }
        else
        {
            Debug.LogWarning($"어빌리티 {ability.name}이 이미 availableAbilities 목록에 존재합니다.");
        }
    }

    /// <summary>
    /// 어빌리티 목록에서 특정 어빌리티를 제거하고, 활성화되어 있다면 비활성화.
    /// </summary>
    /// <param name="ability">제거할 어빌리티.</param>
    public void RemoveAbility(InputAbilitySO ability)
    {
        if (ability == null)
        {
            Debug.LogError("Null 어빌리티를 제거하려고 시도했습니다.");
            return;
        }

        if (availableAbilities.Contains(ability))
        {
            availableAbilities.Remove(ability);
            Debug.Log($"어빌리티 {ability.name}을 availableAbilities 목록에서 제거했습니다.");

            // 어빌리티가 활성화되어 있다면 비활성화
            if (activeAbilities.ContainsKey(ability))
            {
                DeactivateAbility(ability);
            }
        }
        else
        {
            Debug.LogWarning($"어빌리티 {ability.name}이 availableAbilities 목록에 존재하지 않습니다.");
        }
    }

    /// <summary>
    /// 어빌리티 목록에 있는 모든 어빌리티를 비활성화하고, 활성화를 재처리합니다.
    /// </summary>
    private void ReprocessMovement()
    {
        if (isReprocessingMovement)
            return;

        isReprocessingMovement = true;

        if (lastMovementInput == Vector2.zero)
        {
            isReprocessingMovement = false;
            return;
        }

        MovementData movementData = new MovementData(lastMovementInput);

        eventChannel.RaiseProcessMovement(movementData);

        ProcessMovement(movementData.Movement);

        Debug.Log("ReprocessMovement: 움직임을 재처리했습니다.");

        isReprocessingMovement = false;
    }
}
