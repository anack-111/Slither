using DG.Tweening;
using UnityEngine;

public class Lotus : MonoBehaviour
{
    public float rotateSpeed = 3f;  // 회전 속도
    public float stopSpeed = 0.5f;  // 회전 멈추는 속도

    private Quaternion _currentRot;
    private bool _isRotating = false;

    private void Start()
    {
        _currentRot = transform.rotation;  // 연꽃의 원래 회전
    }

    private void OnTriggerEnter(Collider other)
    {
        // Trigger에 들어온 객체로 회전
        LookAtObject(other.gameObject);
    }

    void LookAtObject(GameObject obj)
    {
        // 연꽃의 현재 위치와 트리거된 객체의 위치 차이를 구함
        Vector3 direction = (obj.transform.position - transform.position).normalized;

        // X축은 90도 고정, Y축과 Z축만 회전하도록 설정
        // Y축과 Z축 회전만 처리하고 X축은 고정
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // 현재 회전 상태에서 목표 회전까지 부드럽게 회전
        if (!_isRotating)
        {
            transform.DOKill();  // 기존 애니메이션 종료
            _isRotating = true;

            // 회전 (X축 90도 고정, Y축과 Z축 회전만 적용)
            transform.DORotate(new Vector3(90f, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z), rotateSpeed)
                .SetEase(Ease.OutQuad)
                .OnKill(() => {
                    _isRotating = false;  // 회전 완료 후 다시 회전 가능 상태로 전환
                });
        }
    }
}
