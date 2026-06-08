using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using Data;

public class LobbySimpleMover : MonoBehaviour
{
    [Header("이동 속도")]
    public float moveSpeed = 5f;

    [Header("X 범위 (-4 ~ 8)")]
    public float minX = -4f;
    public float maxX = 8f;

    [Header("Z 범위 (-15.5 ~ 0)")]
    public float minZ = -15.5f;
    public float maxZ = 0f;

    [Header("Y 고정값 (현재 Y 그대로 쓰려면 -999 유지)")]
    public float fixedY = -999f;

    [Header("지렁이 움직임 설정")]
    public Transform[] pieces; // 4개의 새끼들
    public float swayAmplitude = 0.3f; // 좌우 흔들림 크기
    public float swaySpeed = 3f; // 흔들림 속도
    public float phaseGap = 0.5f; // 각 piece 간 딜레이

    // 내부 상태
    bool _isWaiting = false;
    Vector3 _moveDir = Vector3.right;
    float _outMargin = 13f;

    public GameObject _head;

    [Header("커스텀 ")]
    public SpriteRenderer _headRenderer;
    public SpriteRenderer _acRenderer;
    public SpriteRenderer _bodyRenderer;
    public SpriteRenderer[] _childrenderers;

    private Vector3[] _pieceOriginPositions; // 각 piece의 원래 로컬 위치

    private void Awake()
    {
        Refresh();
        InitializePieces();
    }

    void InitializePieces()
    {
        if (pieces == null || pieces.Length == 0) return;

        _pieceOriginPositions = new Vector3[pieces.Length];

        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] != null)
            {
                _pieceOriginPositions[i] = pieces[i].localPosition;
            }
        }
    }

    void OnEnable()
    {
        _isWaiting = false;
        ResetPositionAndDirection();
    }

    void Update()
    {
        if (_isWaiting)
            return;

        Vector3 pos = transform.position;
        pos += _moveDir * moveSpeed * Time.deltaTime;

        if (fixedY != -999f)
            pos.y = fixedY;

        transform.position = pos;

        // Piece 지렁이 움직임
        UpdatePieceSway();

        if (IsOutOfBounds(pos))
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    void UpdatePieceSway()
    {
        if (pieces == null || pieces.Length == 0) return;

        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null) continue;

            // 각 piece마다 phase를 다르게 줘서 물결 효과
            float phase = i * phaseGap;
            float sway = Mathf.Sin(Time.time * swaySpeed - phase) * swayAmplitude;

            // 로컬 X축으로만 움직임 (좌우)
            Vector3 newPos = _pieceOriginPositions[i];
            newPos.x += sway;

            pieces[i].localPosition = newPos;
        }
    }

    public void Refresh()
    {
        string spriteName = Managers.Game.PlayerSpriteNames[0].Replace("_Head", "");
        _headRenderer.sprite = Managers.Resource.Load<Sprite>(spriteName);

        foreach (var child in _childrenderers)
        {
            child.sprite = Managers.Resource.Load<Sprite>(Managers.Game.PlayerSpriteNames[1]);
        }

        int equippedID = Managers.Game.EquippedAccessoryIndex;
        AccessoryData data = Managers.Data.AccessoryDic[equippedID];
        _acRenderer.sprite = Managers.Resource.Load<Sprite>(data.SpriteName);
    }

    bool IsOutOfBounds(Vector3 p)
    {
        return
            p.x < minX - _outMargin ||
            p.x > maxX + _outMargin ||
            p.z < minZ - _outMargin ||
            p.z > maxZ + _outMargin;
    }

    IEnumerator RespawnRoutine()
    {
        _isWaiting = true;

        transform.position += _moveDir * 5f;

        float wait = Random.Range(1f, 2f);
        yield return new WaitForSeconds(wait);

        _head.SetActive(false);
        ResetPositionAndDirection();

        _isWaiting = false;
    }

    void ResetPositionAndDirection()
    {
        int dirType = Random.Range(0, 8);

        float x = 0f;
        float z = 0f;

        switch (dirType)
        {
            case 0:
                _moveDir = Vector3.right;
                x = minX - 1f;
                z = Random.Range(minZ, maxZ);
                break;

            case 1:
                _moveDir = Vector3.left;
                x = maxX + 1f;
                z = Random.Range(minZ, maxZ);
                break;

            case 2:
                _moveDir = Vector3.forward;
                x = Random.Range(minX, maxX);
                z = minZ - 1f;
                break;

            case 3:
                _moveDir = Vector3.back;
                x = Random.Range(minX, maxX);
                z = maxZ + 1f;
                break;

            case 4:
                _moveDir = new Vector3(1f, 0f, 1f).normalized;
                x = minX - 1f;
                z = minZ - 1f;
                break;

            case 5:
                _moveDir = new Vector3(1f, 0f, -1f).normalized;
                x = minX - 1f;
                z = maxZ + 1f;
                break;

            case 6:
                _moveDir = new Vector3(-1f, 0f, 1f).normalized;
                x = maxX + 1f;
                z = minZ - 1f;
                break;

            case 7:
                _moveDir = new Vector3(-1f, 0f, -1f).normalized;
                x = maxX + 1f;
                z = maxZ + 1f;
                break;
        }

        float y = (fixedY == -999f) ? transform.position.y : fixedY;
        transform.position = new Vector3(x, y, z);

        if (_moveDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(_moveDir, Vector3.up);

        _head.SetActive(true);
    }
}