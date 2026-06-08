using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using static Define;

/*
 * 자주쓰이는 범용적인 함수들 
 */

public static class Util
{

    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }


    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }


    public static Color HexToColor(string color)
    {
        Color parsedColor;
        ColorUtility.TryParseHtmlString("#" + color, out parsedColor);

        return parsedColor;
    }



    public static void PlayUIEnter(GameObject obj, Vector2 fromOffset, float duration = 0.4f, bool useScale = true, bool useFade = false, float delay = 0f)
    {
        if (obj == null)
            return;

        RectTransform target = obj.GetComponent<RectTransform>();

        target.DOKill(); // 중복 애니메이션 제거

        CanvasGroup cg = target.GetComponent<CanvasGroup>();
        if (useFade && cg == null)
            cg = target.gameObject.AddComponent<CanvasGroup>();

        Vector2 endPos = target.anchoredPosition;

        // 시작 위치
        target.anchoredPosition = endPos + fromOffset;

        // 스케일 초기 설정
        if (useScale)
            target.localScale = new Vector3(0.8f, 0.8f, 1f);

        // 페이드 초기 설정
        if (useFade && cg != null)
            cg.alpha = 0f;

        // DOTween Sequence 생성
        Sequence seq = DOTween.Sequence();

        //  딜레이 먼저 삽입
        if (delay > 0f)
            seq.AppendInterval(delay);

        // 위치 이동
        seq.Join(target.DOAnchorPos(endPos, duration).SetEase(Ease.OutCubic));

        // 스케일 복귀
        if (useScale)
            seq.Join(target.DOScale(1f, duration).SetEase(Ease.OutCubic));

        // 페이드인
        if (useFade && cg != null)
            seq.Join(cg.DOFade(1f, duration * 0.8f));

        seq.SetUpdate(true);
    }


    //public static void PlayUIExit(GameObject uiObject, Vector2 exitDir, float duration = 0.3f)
    //{
    //    RectTransform rt = uiObject.GetComponent<RectTransform>();
    //    if (rt == null) return;

    //    rt.DOKill();

    //    Vector2 endPos = rt.anchoredPosition + exitDir;

    //    rt.DOAnchorPos(endPos, duration)
    //        .SetEase(Ease.InBack)
    //        .OnComplete(() => uiObject.SetActive(false));

    //}

    public static void ShowCanvasGroup(GameObject obj, float duration = 1f)
    {
        if (obj == null) return;

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();

        obj.SetActive(true);

        cg.alpha = 0f;
        cg.DOFade(1f, duration)
          .SetEase(Ease.OutCubic);
    }

    public static void HideCanvasGroup(GameObject obj, float duration = 1f)
    {
        if (obj == null) return;

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();

        cg.DOFade(0f, duration)
          .SetEase(Ease.InCubic)
          .OnComplete(() =>
          {
              obj.SetActive(false);
          });
    }
    public static string FormatTime(float time)
    {
        int totalSec = Mathf.FloorToInt(time);

        int hours = totalSec / 3600;
        int minutes = (totalSec % 3600) / 60;
        int seconds = totalSec % 60;

        if (hours > 0)
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        else
            return $"{minutes:D2}:{seconds:D2}";
    }

    //나중에 공용으로 뺴야할듯
    public static void ShowUI(GameObject obj, Vector2 offset, float duration = 0.6f)
    {
        if (obj == null) return;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.DOKill();


        Vector2 target = rt.anchoredPosition;             // 최종 목표 = 현재 위치
        Vector2 start = target - offset;                  // offset의 반대 방향에서 시작

        rt.anchoredPosition = start;

        obj.SetActive(true);

        rt.DOAnchorPos(target, duration)
          .SetEase(Ease.OutCubic);
    }


    public static void ShowAndHideUI(GameObject obj, Vector2 offset, float duration = 1f)
    {
        if (obj == null) return;

        RectTransform rt = obj.GetComponent<RectTransform>();
        obj.SetActive(true);

        Vector2 start = rt.anchoredPosition;
        Vector2 target = start + offset;

        rt.DOKill();
        rt.DOAnchorPos(target, duration)
          .SetEase(Ease.InCubic);
    }
}
