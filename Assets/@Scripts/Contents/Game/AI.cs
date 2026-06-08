using UnityEngine;
using static Define;


// 소규모 게임에 Enter, Tick, Exit 함수가 있는 상태 머신을 도입하는 것은 오버엔지니어링일 수 있으니 일단 이렇게 구현
public class AI : MonoBehaviour
{
    public Vector3 direction;   
    public bool sprint;
    private Creature _creature;
    // 현재 상태
    EState _state = EState.Roam;

    //Roam
    [SerializeField] float _roamRadius = 250f;       // 랜덤으로 돌아다닐 범위
    [SerializeField] float _roamChangeInterval = 5f; // 몇 초마다 방향 바꿀지
    float _roamTimer;

    // 스프린트
    [SerializeField] float _sprintDecisionInterval = 3f; // 몇 초마다 스프린트 여부 재결정
    float _sprintTimer;

    // 추적/도망 타겟 
    Transform _chaseTarget;    
    Transform _escapeTarget;   

    void Start()
    {
        _creature = transform.root.GetComponent<Creature>();
        _roamTimer = _roamChangeInterval;
        _sprintTimer = _sprintDecisionInterval;
    }

    void Update()
    {
        // Escape 상태가 아닐 때만 
        if (_state != EState.Escape)
        {
            _sprintTimer += Time.deltaTime;
            if (_sprintTimer >= _sprintDecisionInterval)
            {
                _sprintTimer = 0f;
                
                int random = UnityEngine.Random.Range(0, 4); // 0,1,2,3
                sprint = (random == 0);
            }
        }

        switch (_state)
        {
            case EState.Roam:
                UpdateRoam();
                break;

            case EState.Chase:
                UpdateChase();
                break;

            case EState.Escape:
                UpdateEscape();
                break;
        }
    }

    #region State Updates

    // 배회 상태
    void UpdateRoam()
    {
        _roamTimer += Time.deltaTime;
        if (_roamTimer >= _roamChangeInterval)
        {
            _roamTimer = 0f;

            // 중심에서 랜덤한 위치를 뽑고 그 방향으로 이동
            Vector3 circle = UnityEngine.Random.insideUnitSphere * _roamRadius;
            circle.y = transform.position.y;

            direction = circle - transform.position;
        }
        // Roam 때는 sprint 값은 위에서 랜덤하게 갱신되고 있음
    }

    
    void UpdateChase()
    {
        if (_chaseTarget == null)
        {
            // 타겟을 잃어버리면 다시 배회로
            _state = EState.Roam;
            return;
        }

        Vector3 dir = _chaseTarget.position - transform.position;
        dir.y = transform.position.y;
        direction = dir;
        // Chase 중에도 sprint는 랜덤 로직에 의해 on/off
    }

    // 도망 상태
    void UpdateEscape()
    {
        // 도망 대상이 없으면 다시 배회
        if (_escapeTarget == null)
        {
            sprint = false;
            _state = EState.Roam;
            return;
        }

        float dist = Vector3.Distance(transform.position, _escapeTarget.position);

        // 일정 거리 멀어지면 도망 종료
        if (dist > _roamRadius * 0.7f)
        {
            _escapeTarget = null;
            sprint = false;
            _state = EState.Roam;
            return;
        }

        // 도망 방향
        Vector3 dir = transform.position - _escapeTarget.position;
        dir.y = transform.position.y;

        // 회전 둔화 & 스프린트 조건
        if (_creature.points <= 1000)
        {
            // 회전 매우 둔함 (가장 잘 잡힘)
            direction = Vector3.Lerp(direction, dir, Time.deltaTime * 3);
            sprint = false;
        }
        else if (_creature.points <= 2000)
        {
            // 회전 약간 둔함 (도망 잘함)
            direction = Vector3.Lerp(direction, dir, Time.deltaTime * 7);
            sprint = true;
        }
        else
        {
            // 회전 빠름 (고레벨 보스 느낌)
            direction = dir;
            sprint = true;
        }
    }


    #endregion

    #region Triggers

    void OnTriggerEnter(Collider other)
    {
        // 먹이(포인트) 감지 → 추적 상태로
        if (other.CompareTag("Point"))
        {
            _chaseTarget = other.transform;
            _state = EState.Chase;
        }
    }

    void OnTriggerStay(Collider other)
    {
        // 다른 크리쳐 감지 → 도망 
        if (other.CompareTag("Creature") && other.transform.root != transform.root)
        {
            _escapeTarget = other.transform;
            _state = EState.Escape;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // 도망 대상이 나간 경우
        if (_escapeTarget != null && other.transform == _escapeTarget)
        {
            _escapeTarget = null;
        }

        // 추적 대상이 나간 경우
        if (_chaseTarget != null && other.transform == _chaseTarget)
        {
            _chaseTarget = null;
        }
    }

    #endregion
}
