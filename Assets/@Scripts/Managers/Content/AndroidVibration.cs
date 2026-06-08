using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 게임 전체에서 진동을 관리하는 매니저. (강도 조절 버전)
/// </summary>
public class VibrationManager : MonoBehaviour
{
    public static VibrationManager Inst { get; private set; }

    [Header("전역 On/Off")]
    public bool vibrationEnabled = true;

    [Header("진동 강도 (0~255, 안드로이드 전용)")]
    [Range(0, 255)]
    public int vibrationIntensity = 100; // 기본 강도 (중간보다 약간 약함)

    Coroutine _patternRoutine;
    Coroutine _longRoutine;

#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidJavaObject _vibrator;
    bool _supportsAmplitude = false;
#endif

    [SerializeField]
    float _minPulseInterval = 0.05f;
    float _lastPulseTime = -10f;

    private void Awake()
    {
        if (Inst != null && Inst != this)
        {
            Destroy(gameObject);
            return;
        }

        Inst = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using (var contextClass = new AndroidJavaClass("android.content.Context"))
                {
                    string vibService = contextClass.GetStatic<string>("VIBRATOR_SERVICE");
                    _vibrator = activity.Call<AndroidJavaObject>("getSystemService", vibService);
                }

                // 진동 강도 지원 여부 체크 (API 26+)
                if (_vibrator != null)
                {
                    try
                    {
                        _supportsAmplitude = _vibrator.Call<bool>("hasAmplitudeControl");
                    }
                    catch
                    {
                        _supportsAmplitude = false;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[VibrationManager] Android vibrator init failed: " + e.Message);
            _vibrator = null;
        }
#endif
    }

    // ===== 외부 API =====

    /// <summary>충돌 시 (매우 약하게, 짧게)</summary>
    public void VibrateHit()
    {
        if (!vibrationEnabled) return;
        Pulse(20, 0.4f); // 20ms, 강도 40%
    }

    /// <summary>아이템 획득 시 (약하게)</summary>
    public void VibrateItem()
    {
        if (!vibrationEnabled) return;
        Pulse(30, 0.5f); // 30ms, 강도 50%
    }

    /// <summary>콤보 진동 (리듬감 있게)</summary>
    public void VibrateCombo()
    {
        if (!vibrationEnabled) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (_vibrator == null)
        {
            if (_patternRoutine != null) return;
            _patternRoutine = StartCoroutine(Co_PatternCombo());
            return;
        }

        VibrateComboNative();
#else
        if (_patternRoutine != null)
            return;

        _patternRoutine = StartCoroutine(Co_PatternCombo());
#endif
    }

    /// <summary>죽었을 때 (중간 강도, 짧게)</summary>
    public void VibrateDeath()
    {
        if (!vibrationEnabled) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        VibrateForSecondsNative(0.25f, 0.1f); // 0.25초, 강도 60%
#else
        if (_longRoutine != null)
            StopCoroutine(_longRoutine);

        _longRoutine = StartCoroutine(Co_VibrateForSeconds(0.25f, 0.1f));
#endif
    }

    /// <summary>마그넷 등 (약하게)</summary>
    public void Vibrate1Sec()
    {
        if (!vibrationEnabled) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        VibrateForSecondsNative(0.3f, 0.1f); // 0.3초, 강도 50%
#else
        if (_longRoutine != null)
            StopCoroutine(_longRoutine);

        _longRoutine = StartCoroutine(Co_VibrateForSeconds(0.3f));
#endif
    }

    /// <summary>특수 이벤트용</summary>
    public void Vibrate2Sec()
    {
        if (!vibrationEnabled) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        VibrateForSecondsNative(0.5f, 0.7f); // 0.5초, 강도 70%
#else
        if (_longRoutine != null)
            StopCoroutine(_longRoutine);

        _longRoutine = StartCoroutine(Co_VibrateForSeconds(0.5f));
#endif
    }

    // ===== 내부 구현 =====

    /// <summary>짧은 진동 1번 (강도 조절 가능)</summary>
    void Pulse(long durationMs, float intensityMultiplier = 1f)
    {
        if (Time.realtimeSinceStartup - _lastPulseTime < _minPulseInterval)
            return;

        _lastPulseTime = Time.realtimeSinceStartup;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (_vibrator == null)
        {
            try { Handheld.Vibrate(); } catch { }
            return;
        }

        try
        {
            // 강도 조절 지원 시
            if (_supportsAmplitude)
            {
                int amplitude = Mathf.Clamp((int)(vibrationIntensity * intensityMultiplier), 1, 255);

                using (var veClass = new AndroidJavaClass("android.os.VibrationEffect"))
                {
                    AndroidJavaObject effect = veClass.CallStatic<AndroidJavaObject>(
                        "createOneShot",
                        durationMs,
                        amplitude
                    );
                    _vibrator.Call("vibrate", effect);
                }
            }
            else
            {
                // 구형 API (강도 조절 불가)
                _vibrator.Call("vibrate", durationMs);
            }
        }
        catch (Exception e)
        {
            try { Handheld.Vibrate(); } catch { }
        }
#else
        try { Handheld.Vibrate(); } catch { }
#endif
    }

    /// <summary>안드로이드에서 seconds 동안 진동 (강도 조절)</summary>
    void VibrateForSecondsNative(float seconds, float intensityMultiplier = 1f)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_vibrator == null)
        {
            if (_longRoutine != null)
                StopCoroutine(_longRoutine);
            _longRoutine = StartCoroutine(Co_VibrateForSeconds(seconds));
            return;
        }

        long ms = (long)(seconds * 1000f);

        try
        {
            if (_supportsAmplitude)
            {
                int amplitude = Mathf.Clamp((int)(vibrationIntensity * intensityMultiplier), 1, 255);

                using (var veClass = new AndroidJavaClass("android.os.VibrationEffect"))
                {
                    AndroidJavaObject effect = veClass.CallStatic<AndroidJavaObject>(
                        "createOneShot",
                        ms,
                        amplitude
                    );
                    _vibrator.Call("vibrate", effect);
                }
            }
            else
            {
                _vibrator.Call("vibrate", ms);
            }
        }
        catch (Exception ex)
        {
            if (_longRoutine != null)
                StopCoroutine(_longRoutine);
            _longRoutine = StartCoroutine(Co_VibrateForSeconds(seconds));
        }
#else
        if (_longRoutine != null)
            StopCoroutine(_longRoutine);

        _longRoutine = StartCoroutine(Co_VibrateForSeconds(seconds));
#endif
    }

    /// <summary>seconds 동안 짧은 진동 여러 번</summary>
    IEnumerator Co_VibrateForSeconds(float seconds, float interval = 0.1f)
    {
        float t = 0f;
        while (t < seconds)
        {
            Pulse(20, 0.5f); // 약하게
            yield return new WaitForSeconds(interval);
            t += interval;
        }

        _longRoutine = null;
    }

    /// <summary>안드로이드 콤보 패턴 (강도 조절)</summary>
    void VibrateComboNative()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_vibrator == null)
        {
            if (_patternRoutine != null) return;
            _patternRoutine = StartCoroutine(Co_PatternCombo());
            return;
        }

        try
        {
            if (_supportsAmplitude)
            {
                // 패턴: [대기, 짧게-약, 쉬고, 짧게-약, 쉬고, 중간-강]
                long[] timings = new long[] { 0, 20, 50, 20, 50, 30 };
                int baseAmp = Mathf.Clamp(vibrationIntensity, 1, 255);
                int[] amplitudes = new int[] { 
                    0, 
                    (int)(baseAmp * 0.4f), // 40%
                    0, 
                    (int)(baseAmp * 0.4f), // 40%
                    0, 
                    (int)(baseAmp * 0.6f)  // 60%
                };

                using (var veClass = new AndroidJavaClass("android.os.VibrationEffect"))
                {
                    AndroidJavaObject effect = veClass.CallStatic<AndroidJavaObject>(
                        "createWaveform",
                        timings,
                        amplitudes,
                        -1
                    );
                    _vibrator.Call("vibrate", effect);
                }
            }
            else
            {
                long[] timings = new long[] { 0, 20, 50, 20, 50, 30 };
                _vibrator.Call("vibrate", timings, -1);
            }
        }
        catch (Exception ex)
        {
            if (_patternRoutine != null) return;
            _patternRoutine = StartCoroutine(Co_PatternCombo());
        }
#endif
    }

    /// <summary>콤보 패턴 코루틴</summary>
    IEnumerator Co_PatternCombo()
    {
        Pulse(20, 0.4f);
        yield return new WaitForSeconds(0.08f);

        Pulse(20, 0.4f);
        yield return new WaitForSeconds(0.12f);

        Pulse(30, 0.6f);

        _patternRoutine = null;
    }

    public void StopAllVibration()
    {
        if (_patternRoutine != null)
        {
            StopCoroutine(_patternRoutine);
            _patternRoutine = null;
        }

        if (_longRoutine != null)
        {
            StopCoroutine(_longRoutine);
            _longRoutine = null;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        if (_vibrator != null)
        {
            try
            {
                _vibrator.Call("cancel");
            }
            catch { }
        }
#endif
    }
}