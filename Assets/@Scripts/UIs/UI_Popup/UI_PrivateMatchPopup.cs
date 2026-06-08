using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 친구랑 하기 메인 팝업
/// - 방 만들기 / 코드 입력 후 참가
/// </summary>
public class UI_PrivateMatchPopup : UI_Popup
{
    #region Enum
    enum GameObjects
    {
        ContentObject,
        BtnCreateRoom,
        BtnJoinRoom,
        InputField,
    }

    enum Texts
    {
        TitleText,
    }
    #endregion

    private TMP_InputField _inputRoomCode;

    // ─── 초기화 ───────────────────────────────────────────────────────────

    private void Awake()
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));

        _inputRoomCode = GetObject((int)GameObjects.InputField)?.GetComponent<TMP_InputField>();

        GetObject((int)GameObjects.BtnCreateRoom).BindEvent(OnClickCreateRoom);
        GetObject((int)GameObjects.BtnJoinRoom).BindEvent(OnClickJoinRoom);

        return true;
    }

    private void OnEnable()
    {
        if (_inputRoomCode != null)
            _inputRoomCode.text = "";

        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
    }

    // ─── 버튼 이벤트 ──────────────────────────────────────────────────────

    private void OnClickCreateRoom()
    {
        Managers.Sound.PlayButtonClick();
        gameObject.SetActive(false);
        // null 전달 → 방 생성
        LobbyMatchMakingManager.Instance.StartPrivateMatch(null);
    }

    private void OnClickJoinRoom()
    {
        Managers.Sound.PlayButtonClick();

        string roomCode = _inputRoomCode != null ? _inputRoomCode.text.Trim() : "";

        if (string.IsNullOrEmpty(roomCode))
        {
            Managers.UI.ShowToast("방 코드를 입력해주세요!");
            return;
        }

        if (roomCode.Length != 4)
        {
            Managers.UI.ShowToast("방 코드는 4자리 숫자입니다!");
            return;
        }

        if (!int.TryParse(roomCode, out _))
        {
            Managers.UI.ShowToast("방 코드는 숫자만 입력 가능합니다!");
            return;
        }

        gameObject.SetActive(false);
        LobbyMatchMakingManager.Instance.StartPrivateMatch(roomCode);
    }

    public void OnClickClose()
    {
        Managers.Sound.PlayButtonClick();
        gameObject.SetActive(false);
    }
}
