using UnityEngine;
using static Define;

public class ItemAI : MonoBehaviour
{
    public Vector3 direction;
    private EState _state = EState.Roam;

    [SerializeField] float _roamRadius = 200f;
    [SerializeField] float _roamChangeInterval = 4f;
    float _maxDistanceFromCenter = 450; // 중심에서 최대 거리

    float _roamTimer;
    public GameObject _head;
    Transform _escapeTarget;

    void Start()
    {
        PickRandomDirection();
    }

    void Update()
    {
        //  경계 체크 (최우선)
        CheckBoundary();

        switch (_state)
        {
            case EState.Roam:
                UpdateRoam();
                break;
            case EState.Escape:
                UpdateEscape();
                break;
        }
    }

    //  경계 체크 함수
    void CheckBoundary()
    {
        float distance = Vector3.Distance(_head.transform.position, Vector3.zero);

        if (distance >= _maxDistanceFromCenter)
        {
            // 중심을 향하는 방향으로 강제 회전
            Vector3 toCenter = (Vector3.zero - _head.transform.position);
            toCenter.y = 0f;
            toCenter.Normalize();

            direction = toCenter;

        }
    }

    // ----------------------------
    // 배회
    // ----------------------------
    void UpdateRoam()
    {
        _roamTimer += Time.deltaTime;
        if (_roamTimer >= _roamChangeInterval)
        {
            _roamTimer = 0f;
            PickRandomDirection();
        }

        _head.transform.position += direction * (20f * Time.deltaTime);
    }

    // ----------------------------
    // 도망
    // ----------------------------
    void UpdateEscape()
    {
        if (_escapeTarget == null)
        {
            _state = EState.Roam;
            return;
        }

        Vector3 dir = _head.transform.position - _escapeTarget.position;
        dir.y = 0;
        direction = dir.normalized;

        float escapeSpeed = 12f;
        _head.transform.position += direction * (escapeSpeed * Time.deltaTime);
    }

    // ----------------------------
    // Trigger 감지
    // ----------------------------
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Creature"))
        {
            _escapeTarget = other.transform;
            _state = EState.Escape;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Creature"))
        {
            _escapeTarget = other.transform;
            _state = EState.Escape;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (_escapeTarget != null && other.transform == _escapeTarget)
        {
            _escapeTarget = null;
            _state = EState.Roam;
        }
    }

    void PickRandomDirection()
    {
        Vector2 r = Random.insideUnitCircle.normalized;
        direction = new Vector3(r.x, 0, r.y);
    }
}