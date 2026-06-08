using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
//using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Define;

public class ObjectManager
{
    public PlayerController Player { get; private set; }

    public HashSet<PointController> Points { get; } = new HashSet<PointController>();

    public HashSet<ItemController> Items { get; } = new HashSet<ItemController>();
    public HashSet<Piece> Pieces { get; } = new HashSet<Piece>();

    public List<Creature> Creatures { get; } = new List<Creature>();


    public T Spawn<T>(Vector3 position, int templateID = 0, string prefabName = "") where T : BaseController
    {
        System.Type type = typeof(T);

        if (type == typeof(PlayerController))
        {
            GameObject go = Managers.Resource.Instantiate("Player");
            go.transform.position = new Vector3(0, 0, 0);
            PlayerController pc = go.GetOrAddComponent<PlayerController>();

            Player = pc; 

            return pc as T;
        }
        else if(type == typeof(PointController))
        {
            GameObject go = Managers.Resource.Instantiate("Point");
            go.transform.position = new Vector3(0, 0, 0);
           

        }


            return null;
    }
    public T Spawn<T>(Vector3 position, string prefabName = "") where T : BaseController
    {
        System.Type type = typeof(T);

        if (type == typeof(PlayerController))
        {
            GameObject go = Managers.Resource.Instantiate("Player");
            go.transform.position = new Vector3(0, 0, 0);
            PlayerController pc = go.transform.GetChild(0).gameObject.GetOrAddComponent<PlayerController>();

            Player = pc;

            Managers.Object.RegisterCreature(pc.parent); 

            return pc as T;
        }
        if (type == typeof(PointController))
        {
            GameObject go = Managers.Resource.Instantiate("Point", pooling : true);
            PointController ptc = go.GetOrAddComponent<PointController>();
            go.transform.position = position;  // position 파라미터 사용

            Points.Add(ptc);

            return ptc as T;
        }

        if (type == typeof(ItemController))
        {
            GameObject go = Managers.Resource.Instantiate(prefabName, pooling: true);
            ItemController ic = go.GetOrAddComponent<ItemController>();
            go.transform.position = position;

            Items.Add(ic);

            return ic as T;
        }
          
        if (type == typeof(BodyController))
        {
            GameObject go = Managers.Resource.Instantiate(prefabName, pooling: true);
            BodyController bc = go.GetOrAddComponent<BodyController>();
            bc.transform.position = position;
              
            return bc as T;
        }


        if (type == typeof(Piece))
        {
            GameObject go = Managers.Resource.Instantiate("Piece", pooling: true);
            Piece piece = go.GetOrAddComponent<Piece>();
            piece.transform.position = position;

            Pieces.Add(piece);

            return piece as T;
        }

            return null;
    }


    public void Despawn<T>(T obj) where T : BaseController
    {
        System.Type type = typeof(T);

        if (type == typeof(PointController))
        {
            Points.Remove(obj as PointController);
            Managers.Resource.Destroy(obj.gameObject);
        }



        if (type == typeof(ItemController))
        {
            Items.Remove(obj as ItemController);
            Managers.Resource.Destroy(obj.gameObject);
        }
        if (type == typeof(Piece))
        {
            Pieces.Remove(obj as Piece);
            Managers.Resource.Destroy(obj.gameObject);
        }
        if (type == typeof(BodyController))
        {
            Managers.Resource.Destroy(obj.gameObject);
        }

    }

    // Creature 등록
    public void RegisterCreature(Creature creature)
    {
        if (!Creatures.Contains(creature))
            Creatures.Add(creature);
    }

    // Creature 제거
    public void UnregisterCreature(Creature creature)
    {
        Creatures.Remove(creature);
    }



    public void MagnetCollect(PlayerController controller, float radius)
    {
        if (Player == null)
            return;

        Transform playerTarget = controller.transform;
        Vector3 playerPos = controller.transform.position;

        float radiusSqr = radius * radius;



        foreach (PointController point in Points.ToList())
        {
            if (point == null || !point.gameObject.activeSelf)
                continue;

            if (point.IsEating)
                continue;

            // 거리 체크
            float distSqr = (point.transform.position - playerPos).sqrMagnitude;

            if (distSqr <= radiusSqr)
            {
                // PointController 내부 코루틴 실행
                point.IsMagnetLocked = true;
                point.LockToMagnet(controller);
                point.ScaleBounceThenMagnet(playerTarget);
                point._trailObject.SetActive(true);
            }
        }
    }

    public UI_Noti ShowSign(Vector3 pos ,int sign)
    {

        GameObject go = Managers.Resource.Instantiate("Sign", pooling: true);
        UI_Noti signText = go.GetOrAddComponent<UI_Noti>();
        signText.SetInfo(pos, sign);

        return signText;
    }
    public void ShowCombo(Vector3 pos)
    {

        GameObject go = Managers.Resource.Instantiate("Combo", pooling: true);
        UI_Combo combo = go.GetOrAddComponent<UI_Combo>();
        combo.SetInfo(pos);

    }

    public void Clear()
    {
        Points.Clear();
        Pieces.Clear();
    }

}
