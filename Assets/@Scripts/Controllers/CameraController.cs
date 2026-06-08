using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : BaseController
{
    public Creature creature;
    Camera _cam;
    float startOrtographic;
    public GameObject _background;

    public UI_Joystick _Joystick;
    // Use this for initialization
    void Start()
    {

        UI_GameScene scene = Managers.UI.SceneUI as UI_GameScene;
        _Joystick = scene._joystickUI;
        creature = Managers.Object.Player.parent;

        Application.targetFrameRate = 60;
        _cam = GetComponent<Camera>();
        startOrtographic = _cam.orthographicSize;
    }

    private void Update()
    {
      
    }
    // Update is called once per frame
    void LateUpdate()
    {

        if (_Joystick.gameObject.activeInHierarchy)
        {
            Follow();
            Zoom();
        }
      
    }


    void Follow()
    {
        Vector3 playerPosition = creature._head.transform.position;
        playerPosition.y = transform.position.y;
        transform.position = Vector3.Lerp(transform.position, playerPosition, 10 * Time.deltaTime);

    }

    private Vector3 _bgTargetScale = Vector3.one;

    void Zoom()
    {
        float scale = creature.referenceScale;
        // 카메라 줌
        float targetSize = startOrtographic + scale * 20;
        _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, targetSize, 1f);
        // 배경 스케일 공식
        float bgScale = (_cam.orthographicSize * 0.1f) * 2;
        // new를 쓰지 않고 값만 직접 수정
        _bgTargetScale.x = bgScale + 3f;
        _bgTargetScale.y = bgScale;
        _bgTargetScale.z = bgScale;
        _background.transform.localScale =
            Vector3.Lerp(_background.transform.localScale, _bgTargetScale, 10f * Time.deltaTime);
    }




    public IEnumerator Co_ZoomToFixedSize(float targetSize, float duration)
    {
        float startSize = _cam.orthographicSize;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            //  점점 가속: Quadratic ease-in
            t = t * t;

            _cam.orthographicSize = Mathf.Lerp(startSize, targetSize, t);

            yield return null;
        }

        _cam.orthographicSize = targetSize;
    }


}
