using UnityEngine;


public class Managers : MonoBehaviour
{


    static Managers s_instance;
    static Managers Instance { get { Init(); return s_instance; } }

    #region Contents
    GameManager _game = new GameManager();
    ObjectManager _object = new ObjectManager();

    
    public static GameManager Game { get { return Instance?._game; } }
    public static ObjectManager Object { get { return Instance?._object; } }

    #endregion

    #region Core

    PoolManager _pool = new PoolManager();
    ResourceManager _resource = new ResourceManager();
    MySceneManager _scene = new MySceneManager();
    UIManager _ui = new UIManager();

    DataManager _data = new DataManager();
    SoundManager _sound = new SoundManager();

    public static DataManager Data { get { return Instance?._data; } }

    public static SoundManager Sound { get { return Instance?._sound; } }

    public static PoolManager Pool { get { return Instance?._pool; } }
    public static ResourceManager Resource { get { return Instance?._resource; } }
    public static MySceneManager Scene { get { return Instance?._scene; } }

    public static UIManager UI { get { return Instance?._ui; } }
    #endregion


    public static void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();

            s_instance._sound.Init();

        }
    }

    public static void Clear()
    {
        Sound.Clear();
        Scene.Clear();
        UI.Clear();
        Pool.Clear();
        Object.Clear();
    }


}
