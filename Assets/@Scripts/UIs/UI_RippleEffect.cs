using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UIRippleEffect : MonoBehaviour, IPointerClickHandler
{
    [Header("Ripple Setup")]
    public Sprite m_EffectSprite;
    public Color RippleColor;
    public Color[] GradientColor;
    public float MaxPower = 0.25f; // 1
    public float Duration = 0.25f; // 1
    public Vector2 rippleSize = Vector2.one; // 2000, 2000
    public bool type; // gradient, linear
    public TextMeshProUGUI btnText;
    public Color tempColor;

    private bool m_IsInitialized = false;
    private RectMask2D m_RectMask;

    void Awake()
    {
        if (m_EffectSprite == null)
        {
            Debug.LogWarning("Failed to add ripple graphics. Not Ripple found.");
            return;
        }
        SetupRippleContainer();
    }
    void Start()
    {
        tempColor = btnText.color;
    }
    private void SetupRippleContainer()
    {
        m_RectMask = gameObject.AddComponent<RectMask2D>();
        m_RectMask.padding = new Vector4(0, 0, 0, 0); // 5, 5, 5, 5
        m_RectMask.softness = new Vector2Int(0, 0); // 20, 20
        m_IsInitialized = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!m_IsInitialized) return;
        if (type)
        {
            GameObject rippleObject = new GameObject("_ripple_");
            LayoutElement crl = rippleObject.AddComponent<LayoutElement>();
            crl.ignoreLayout = true;

            Image currentRippleImage = rippleObject.AddComponent<Image>();
            currentRippleImage.sprite = m_EffectSprite;
            currentRippleImage.transform.SetAsLastSibling();
            currentRippleImage.transform.SetPositionAndRotation(eventData.position, Quaternion.identity);
            currentRippleImage.transform.SetParent(transform);
            currentRippleImage.color = new Color(RippleColor.r, RippleColor.g, RippleColor.b, 0f);
            currentRippleImage.raycastTarget = false;

            //hsv color < 30이면 텍스트 하얀색 > 30이면 텍스트 검은색
            float h, s, v;
            Color.RGBToHSV(currentRippleImage.color, out h, out s, out v);
            if (v < 0.3f)
            {
                btnText.color = Color.white;
            }
            else
            {
                btnText.color = Color.black;
            }

            StartCoroutine(AnimateRipple(rippleObject.GetComponent<RectTransform>(), currentRippleImage, () =>
            {
                currentRippleImage = null;
                Destroy(rippleObject);

                if (transform.childCount <= 1) // 텍스트 원래 색으로 돌아오기
                {
                    btnText.color = tempColor;
                }
                StopCoroutine(nameof(AnimateRipple));
            }));
        }
        else
        {
            StartCoroutine(GradientRippleEffect(eventData));
        }
    }
    private IEnumerator GradientRippleEffect(PointerEventData eventData)
    {
        List<GameObject> rippleObjects = new List<GameObject>();

        for (int i = 0; i < 3; i++)
        {
            GameObject rippleObject = new GameObject("_ripple_");
            LayoutElement crl = rippleObject.AddComponent<LayoutElement>();
            crl.ignoreLayout = true;

            Image rippleImage = rippleObject.AddComponent<Image>();
            rippleImage.sprite = m_EffectSprite;
            rippleImage.transform.SetAsLastSibling();
            rippleImage.transform.SetPositionAndRotation(eventData.position, Quaternion.identity);
            rippleImage.transform.SetParent(transform);
            rippleImage.color = new Color(GradientColor[i].r, GradientColor[i].g, GradientColor[i].b, 0f);
            rippleImage.raycastTarget = false;

            rippleObjects.Add(rippleObject);
        }

        int foreachIndex = 0;
        foreach (GameObject rippleObject in rippleObjects)
        {
            Image rippleImage = rippleObject.GetComponent<Image>();
            if (foreachIndex == 0) //0 or rippleObjects.Count-1
            {
                float h, s, v;
                Color.RGBToHSV(rippleImage.color, out h, out s, out v);
                if (v < 0.3f)
                {
                    btnText.color = Color.white;
                }
                else
                {
                    btnText.color = Color.black;
                }
            }

            StartCoroutine(AnimateRipple(rippleObject.GetComponent<RectTransform>(), rippleImage, GradientColor[foreachIndex], () =>
            {
                Destroy(rippleObject);
                if (transform.childCount <= 1) // 텍스트 원래 색으로 돌아오기
                {
                    btnText.color = tempColor;
                }
            }));
            foreachIndex++;
            yield return new WaitForSeconds(0.075f);
        }

    }
    private IEnumerator AnimateRipple(RectTransform rippleTransform, Image rippleImage, Action onComplete)
    {
        Vector2 initialSize = Vector2.zero;
        Vector2 targetSize = rippleSize; // 2000, 2000
        Color initialColor = new Color(RippleColor.r, RippleColor.g, RippleColor.b, MaxPower);
        Color targetColor = new Color(RippleColor.r, RippleColor.g, RippleColor.b, 0f);
        float elapsedTIme = 0f;

        while (elapsedTIme < Duration)
        {
            elapsedTIme += Time.deltaTime;
            rippleTransform.sizeDelta = Vector2.Lerp(initialSize, targetSize, elapsedTIme / Duration);
            rippleImage.color = Color.Lerp(initialColor, targetColor, elapsedTIme / Duration);
            yield return null;

        }
        onComplete?.Invoke();

    }
    private IEnumerator AnimateRipple(RectTransform rippleTransform, Image rippleImage, Color selectColor, Action onComplete)
    { // 그라데이션에서 사용하기 위해 Color를 직접 지정
        Vector2 initialSize = Vector2.zero;
        Vector2 targetSize = rippleSize; // 2000, 2000
        Color initialColor = new Color(selectColor.r, selectColor.g, selectColor.b, MaxPower);
        Color targetColor = new Color(selectColor.r, selectColor.g, selectColor.b, 0f);
        float elapsedTIme = 0f;

        while (elapsedTIme < Duration)
        {
            elapsedTIme += Time.deltaTime;
            rippleTransform.sizeDelta = Vector2.Lerp(initialSize, targetSize, elapsedTIme / Duration);
            rippleImage.color = Color.Lerp(initialColor, targetColor, elapsedTIme / Duration);
            yield return null;

        }
        onComplete?.Invoke();
    }
}