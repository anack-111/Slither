using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

namespace Data
{

 

    #region CustomData
    [Serializable]
    public class CustomData
    {
     
        public int CustomIndex = 1;
        public string SpriteName;
    }


    public class CustomDataLoader : ILoader<int, CustomData>
    {
        public List<CustomData> customs = new List<CustomData>();

        public Dictionary<int, CustomData> MakeDict()
        {
            Dictionary<int, CustomData> dict = new Dictionary<int, CustomData>();
            foreach (CustomData custom in customs)
                dict.Add(custom.CustomIndex, custom);
            return dict;
        }
    }
    #endregion



    #region RankData
    [Serializable]
    public class RankData
    {

        public int CustomIndex = 1;
        public string name;
        public int KillPoint;
    }


    public class RankDataLoader : ILoader<int, RankData>
    {
        public List<RankData> ranks = new List<RankData>();

        public Dictionary<int, RankData> MakeDict()
        {
            Dictionary<int, RankData> dict = new Dictionary<int, RankData>();
            foreach (RankData custom in ranks)
                dict.Add(custom.CustomIndex, custom);
            return dict;
        }
    }
    #endregion



    #region CustomChildData
    [Serializable]
    public class CustomChildData
    {

        public int CustomIndex = 1;
        public string SpriteName;
    }


    public class CustomChildDataLoader : ILoader<int, CustomChildData>
    {
        public List<CustomChildData> customchilds = new List<CustomChildData>();

        public Dictionary<int, CustomChildData> MakeDict()
        {
            Dictionary<int, CustomChildData> dict = new Dictionary<int, CustomChildData>();
            foreach (CustomChildData custom in customchilds)
                dict.Add(custom.CustomIndex, custom);
            return dict;
        }
    }
    #endregion



    #region AccessoryData
    [Serializable]
    public class AccessoryData
    {

        public int CustomIndex = 1;
        public string Name;
        public string Description;
        public string SpriteName;
        public int Cost;
    }


    public class AccessoryDataLoader : ILoader<int, AccessoryData>
    {
        public List<AccessoryData> accessorys = new List<AccessoryData>();

        public Dictionary<int, AccessoryData> MakeDict()
        {
            Dictionary<int, AccessoryData> dict = new Dictionary<int, AccessoryData>();
            foreach (AccessoryData accessory in accessorys)
                dict.Add(accessory.CustomIndex, accessory);
            return dict;
        }
    }
    #endregion
}