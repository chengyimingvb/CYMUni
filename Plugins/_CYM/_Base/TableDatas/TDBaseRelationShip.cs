//------------------------------------------------------------------------------
// TDRelationShip.cs
// Copyright 2020 2020/6/13 
// Created by CYM on 2020/6/13
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace CYM
{
    [Serializable]
    public class TDBaseRelationShipData : TDBaseData
    {
        public static int MaxRelationShip { get; set; } = 0;
        public static int MinRelationShip { get; set; } = 0;
        public Range Range { get; set; }
    }

    public class TDBaseRelationShip<TData> : TDBaseConfig<TData>
        where TData : TDBaseRelationShipData, new()
    {
        Dictionary<int, TData> RangeData = new Dictionary<int, TData>();
        public override void OnLuaParseEnd()
        {
            base.OnLuaParseEnd();
            foreach (var item in Data)
            {
                for (int i = (int)item.Value.Range.Min; i < item.Value.Range.Max; ++i)
                {
                    if (!RangeData.ContainsKey(i))
                        RangeData.Add(i, item.Value);
                }
                if (item.Value.Range.Max > TDBaseRelationShipData.MaxRelationShip)
                    TDBaseRelationShipData.MaxRelationShip = (int)item.Value.Range.Max;
                if (item.Value.Range.Min < TDBaseRelationShipData.MinRelationShip)
                    TDBaseRelationShipData.MinRelationShip = (int)item.Value.Range.Min;
            }
        }

        public TData GetByRange(int i)
        {
            if (!RangeData.ContainsKey(i))
                return Data.Values.First();
            return RangeData[i];
        }
    }
}