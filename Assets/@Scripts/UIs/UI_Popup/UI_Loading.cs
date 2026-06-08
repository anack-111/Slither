using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 매치메이킹 로딩 화면
/// - MatchMakingManager에서 Show/Hide 호출
/// - 씬에 미리 배치해두고 비활성화 상태로 시작
/// </summary>
public class UI_Loading : UI_Base
{
    #region Enum
    enum Images
    {
        LoadingImage,
    }
    #endregion

    private void Awake()
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        return true;
    }

    private void OnEnable()
    {
        var img = GetImage((int)Images.LoadingImage);
        if (img != null)
        {
            img.rectTransform.DOKill();
            img.rectTransform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);
        }
    }

    private void OnDisable()
    {
        var img = GetImage((int)Images.LoadingImage);
        if (img != null)
            img.rectTransform.DOKill();
    }
}
