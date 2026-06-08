using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{

    public Dictionary<int, Data.CustomData> CustomDic { get; private set; } = new Dictionary<int, Data.CustomData>();
    public Dictionary<int, Data.CustomChildData> CustomChildDic { get; private set; } = new Dictionary<int, Data.CustomChildData>();
    public Dictionary<int, Data.AccessoryData> AccessoryDic { get; private set; } = new Dictionary<int, Data.AccessoryData>();
    public Dictionary<int, Data.RankData> RankDic { get; private set; } = new Dictionary<int, Data.RankData>();
    public void Init()
    {
        CustomDic = LoadJson<Data.CustomDataLoader, int, Data.CustomData>("CustomData").MakeDict();
        CustomChildDic = LoadJson<Data.CustomChildDataLoader, int, Data.CustomChildData>("CustomChildData").MakeDict();
        AccessoryDic = LoadJson<Data.AccessoryDataLoader, int, Data.AccessoryData>("AccessoryData").MakeDict();
        RankDic = LoadJson<Data.RankDataLoader, int, Data.RankData>("RankData").MakeDict();
    }

    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"{path}");
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
}
