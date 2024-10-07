using UnityEngine;

/// <summary>
/// 움직임 데이터를 참조 타입으로 캡슐화한 클래스.
/// 어빌리티가 움직임 데이터를 직접 수정할 수 있도록 함.
/// </summary>
public class MovementData
{
    /// <summary>
    /// 현재 움직임 벡터.
    /// </summary>
    public Vector2 Movement { get; set; }

    /// <summary>
    /// 생성자. 초기 움직임 벡터를 설정.
    /// </summary>
    /// <param name="movement">초기 움직임 벡터.</param>
    public MovementData(Vector2 movement)
    {
        Movement = movement;
    }
}
