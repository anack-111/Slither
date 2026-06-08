using System.Collections;
using TMPro;
using UnityEngine;

public class UI_Noti : MonoBehaviour
{
    public TextMeshProUGUI _sign;
    CanvasGroup _cg;

    void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        if (_cg == null)
            _cg = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetInfo(Vector3 pos, int sign)
    {
        transform.position = pos;

        _sign.text = (sign == 1 ? "+" : "-");

        PlayFloatingAnimation();
    }

    public void PlayFloatingAnimation()
    {
        StartCoroutine(CO_Floating());
    }

    IEnumerator CO_Floating()
    {
        _cg.alpha = 1f;

        float angle = Random.Range(0f, 360f);
        Vector2 dir2D = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Vector3 dir = new Vector3(dir2D.x, dir2D.y, 0f);

        float distance = Random.Range(2f, 3.5f);
        float waveAmount = Random.Range(0.3f, 0.6f);
        float time = 0f;
        float duration = 1.2f;

        float holdTime = 0.5f;
        float fadeTime = duration - holdTime;

        Vector3 startPos = transform.position;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // 이동 계산 (Transform만 변경)
            Vector3 target = startPos + dir * distance * t;
            Vector3 perp = new Vector3(-dir.y, dir.x, 0f);
            float wave = Mathf.Sin(t * Mathf.PI * 2f) * waveAmount;

            transform.position = target + perp * wave * (1f - t);

            // 알파 조절
            if (time < holdTime)
            {
                _cg.alpha = 1f;
            }
            else
            {
                float fadeT = (time - holdTime) / fadeTime;
                _cg.alpha = Mathf.Lerp(1f, 0f, fadeT);
            }

            yield return null;
        }

        _cg.alpha = 0f;
        Managers.Resource.Destroy(gameObject);
    }
}
