using UnityEngine;

/// <summary>
/// 개발 모드 매치메이킹 설정 브릿지
/// Resources/DevMatchmakingSettings ScriptableObject를 통해 설정값을 읽어옴
/// </summary>
public static class DevMatchmakingBridge
{
    private static ScriptableObject _settings;

    private static ScriptableObject Settings
    {
        get
        {
            if (_settings == null)
                _settings = Resources.Load<ScriptableObject>("DevMatchmakingSettings");
            return _settings;
        }
    }

    public static bool IsAvailable => Settings != null;

    public static bool EnableDevMode
    {
        get => GetBool("EnableDevMode");
        set => SetBool("EnableDevMode", value);
    }

    public static int MinMatchmakingPlayerCount
    {
        get => GetInt("MinMatchmakingPlayerCount");
        set => SetInt("MinMatchmakingPlayerCount", value);
    }

    public static int MaxMatchmakingPlayerCount
    {
        get => GetInt("MaxMatchmakingPlayerCount");
        set => SetInt("MaxMatchmakingPlayerCount", value);
    }

    private static bool GetBool(string fieldName)
    {
        var field = Settings?.GetType().GetField(fieldName);
        return field != null && field.GetValue(Settings) is bool v && v;
    }

    private static void SetBool(string fieldName, bool value)
    {
        Settings?.GetType().GetField(fieldName)?.SetValue(Settings, value);
    }

    private static int GetInt(string fieldName)
    {
        var field = Settings?.GetType().GetField(fieldName);
        return field != null && field.GetValue(Settings) is int v ? v : 0;
    }

    private static void SetInt(string fieldName, int value)
    {
        Settings?.GetType().GetField(fieldName)?.SetValue(Settings, value);
    }
}
