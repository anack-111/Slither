using CnControls;
using UnityEngine.EventSystems;
using UnityEngine;

public class UI_Joystick : UI_Scene
{
    [Header("Joystick UI")]
    public RectTransform joystickRoot;
    public RectTransform joystickHandle;

    private float _joystickRadius;
    private Vector2 _joystickOriginalPos;
    private Vector2 _moveDir;
    private Vector2 _joystickTouchPos;
    private bool _pointerDown = false;
    private int _touchId = -1;

    private VirtualAxis _horizontalAxis;
    private VirtualAxis _verticalAxis;

    public override bool Init()
    {
        if (!base.Init())
            return false;

        _joystickRadius = joystickRoot.GetComponent<RectTransform>().sizeDelta.y / 5;
        _joystickOriginalPos = joystickRoot.transform.position;

        GameObject touchArea = gameObject;
        touchArea.BindEvent(OnPointerDown, null, Define.EUIEvent.PointerDown);
        touchArea.BindEvent(OnPointerUp, null, Define.EUIEvent.PointerUp);
        touchArea.BindEvent(null, OnDrag, Define.EUIEvent.Drag);

        _horizontalAxis = new VirtualAxis("Horizontal");
        _verticalAxis = new VirtualAxis("Vertical");
        CnInputManager.RegisterVirtualAxis(_horizontalAxis);
        CnInputManager.RegisterVirtualAxis(_verticalAxis);

        return true;
    }

    public void OnPointerDown()
    {
        _pointerDown = true;

        Vector2 touchPos = Vector2.zero;

        // 멀티터치 처리
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.touches[i];

                // 화면 왼쪽 60%만 조이스틱으로 인식
                if (_touchId == -1 && touch.position.x < Screen.width * 0.6f)
                {
                    _touchId = touch.fingerId;
                    touchPos = touch.position;
                    break;
                }
            }
        }
        else // 에디터 테스트용
        {
            touchPos = Input.mousePosition;
        }

        if (touchPos != Vector2.zero)
        {
            _joystickTouchPos = touchPos;

            // 조이스틱 타입에 따라 처리
            if (Managers.Game.JoystickType == Define.EJoystickType.Flexible)
            {
                // 자유형: 터치한 곳에 조이스틱 생성
                joystickHandle.transform.position = touchPos;
                joystickRoot.transform.position = touchPos;
            }
            else
            {
                // 고정형: 조이스틱은 원래 위치 유지, 터치 위치만 기록
                _joystickTouchPos = _joystickOriginalPos;
            }
        }
    }

    public void OnPointerUp()
    {
        _pointerDown = false;
        _touchId = -1;
        _moveDir = Vector2.zero;

        joystickHandle.position = _joystickOriginalPos;
        joystickRoot.position = _joystickOriginalPos;

        _horizontalAxis.Value = 0;
        _verticalAxis.Value = 0;

        Managers.Game.MoveDir = Vector2.zero;
    }

    public void OnDrag(BaseEventData baseEventData)
    {
        if (!_pointerDown)
            return;

        Vector2 dragPos = Vector2.zero;

        // 멀티터치 처리
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.touches[i];
                if (touch.fingerId == _touchId)
                {
                    dragPos = touch.position;
                    break;
                }
            }
        }
        else // 에디터 테스트용
        {
            PointerEventData pointerEventData = baseEventData as PointerEventData;
            dragPos = pointerEventData.position;
        }

        if (dragPos == Vector2.zero)
            return;

        _moveDir = (dragPos - _joystickTouchPos).normalized;

        float joystickDist = Vector2.Distance(dragPos, _joystickTouchPos);
        Vector3 newPos;

        if (joystickDist < _joystickRadius)
        {
            newPos = _joystickTouchPos + _moveDir * joystickDist;
        }
        else
        {
            newPos = _joystickTouchPos + _moveDir * _joystickRadius;
        }

        joystickHandle.transform.position = newPos;

        Managers.Game.MoveDir = _moveDir;
        _horizontalAxis.Value = _moveDir.x;
        _verticalAxis.Value = _moveDir.y;
    }

    public void OnDestroy()
    {
        CnInputManager.UnregisterVirtualAxis(_horizontalAxis);
        CnInputManager.UnregisterVirtualAxis(_verticalAxis);
    }
}