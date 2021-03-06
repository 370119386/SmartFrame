﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Smart.Table
{
    ///<summary>
    ///this code was generated by tools,do not edit this code
    ///</summary>
    [System.Serializable]
    public class TableTemplateItem : ITable
    {
        //<enum>
        public int getId()
        {
            return 0;//<id>
        }
        //<content>
    }
    //[CreateAssetMenu]
    public class TableTemplate : ScriptableObject
    {
        public TableTemplateItem[] datas = new TableTemplateItem[0];
        protected Dictionary<int,TableTemplateItem> table = null;
        public void MakeTable()
        {
            table = new Dictionary<int, TableTemplateItem>(datas.Length);
            for(int i = 0 ; i < datas.Length ; ++i)
            {
                table.Add(datas[i].getId(),datas[i]);
            }
        }
        public TableTemplateItem GetTableItem(int iId)
        {
            if(null != table && table.ContainsKey(iId))
            {
                return table[iId];
            }
            return null;
        }
    }
}