using DG.Tweening;
using System.Collections;
using UnityEngine;

public class BodyController : ItemController
{
    public int _soudCount = 5;
    Coroutine _coMoveToPlayer;

    public void Start()
    {
       
    }

    public void OnEnable()
    {
        if (_coMoveToPlayer != null)
        {
            StopCoroutine(_coMoveToPlayer);
            _coMoveToPlayer = null;
        }
    }

    public override bool Init()
    {
        base.Init();
        return true;
    }

    public void SetInfo(GameObject obj)
    {
        transform.localScale = obj.transform.localScale;
    }

    public void GetItem()
    {

        if (_coMoveToPlayer == null && this.IsValid())
        {
            _coMoveToPlayer = StartCoroutine(CoMoveToPlayer());
        }
    }


    Vector3 targetScale = new Vector3(5.5f, 5.5f, 5.5f);
    public IEnumerator CoMoveToPlayer()
    {
        float duration = 0.5f;  // 목표까지 이동 시간
        float t = 0f;

        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;

        Vector3 targetPos;

        while (this.IsValid())
        {
            targetPos = Managers.Game.BodyDestination;

            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / duration);

            // 위치 보간 (정확히 duration에 맞춰 도달)
            transform.position = Vector3.Lerp(startPos, targetPos, ratio);

            // 크기 보간
            transform.localScale = Vector3.Lerp(startScale, targetScale, ratio);

            // 0.5초 지나고 도착 처리
            if (ratio >= 1f)
            {
                Managers.Game.Kill++;
                Managers.Object.Despawn(this);
                yield break;
            }

            yield return null;
        }
    }

}
