using System;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace Smart.Editor
{
    public class ExcelDataConvert
    {
        protected const string moduleName = @"ExcelConvert";
        public enum ConvertFlag
        {
            CF_CSharpCode = (1 << 0),
            CF_TableAsset = (1 << 1),
            CF_ReGenerate = CF_CSharpCode | CF_TableAsset,
        }

        public enum ErrorCode
        {
            EC_SUCCEED = 0,
            EC_INVALID_SHEETDATA,
            EC_HEAD_INVALID,
            EC_TYPE_INVALID,
            EC_NAME_INVALID,
            EC_TEMPLATE_FILE_NOT_EXIST,
        }

        public string[] errorDescriptions = new string[]
        {
            @"convert [<color=#00ff00>{0}</color>] succeed ...",
            @"sheet data is invalid ...",
            @"[{0}] match head content failed coloum = [{1}] ...",
            @"[{0}] match type content failed value is [{1}] coloum = [{2}] ...",
            @"[{0}] match name content failed value is [{1}] coloum = [{2}] ...",
            @"[{0}] template file does not exist",
        };

        protected void LogFormat(string fmt,params object[] args)
        {
            try
            {
                var content = string.Format("[<color=#ff00ff>{0}</color>]:[<color=#00ffff>{1}</color>]",moduleName,string.Format(fmt,args));
                Debug.Log(content);
            }
            catch(Exception e)
            {
                var content = string.Format("[<color=#ff00ff>{0}</color>]:[<color=#ff0000>{1}</color>]",moduleName,e.Message);
                Debug.Log(content);
            }
        }

        protected void LogErrorFormat(string fmt,params object[] args)
        {
            try
            {
                var content = string.Format("[<color=#ff00ff>{0}</color>]:[<color=#ff0000>{1}</color>]",moduleName,string.Format(fmt,args));
                Debug.Log(content);
            }
            catch(Exception e)
            {
                var content = string.Format("[<color=#ff00ff>{0}</color>]:[<color=#ff0000>{1}</color>]",moduleName,e.Message);
                Debug.Log(content);
            }
        }

        static Regex ms_proto_head_reg = new Regex(@"(^\s*required\s*$|^\s*repeated\s*$)",RegexOptions.Singleline);
        static Regex ms_proto_var_type_reg = new Regex(@"(sint32|string|enum|bool|float)", RegexOptions.Singleline);
        static Regex ms_proto_var_name_reg = new Regex(@"(^\s*[a-zA-Z_][a-zA-Z_0-9]*\s*$|^\s*[a-zA-Z_][a-zA-Z_0-9]*:[0-9]*\s*$)", RegexOptions.Singleline);
        static Regex ms_enum_prop_reg = new Regex(@"([a-zA-Z_][0-9a-zA-Z_]*):(\-*\d+):\s*([^\s]+)\s*", RegexOptions.Singleline);

        protected bool verifyHeadContent(ExcelSheetData data)
        {
            if(null == data)
            {
                LogErrorFormat(errorDescriptions[(int)ErrorCode.EC_INVALID_SHEETDATA]);
                return false;
            }
            //verify head contents
            for(int i = 0 ; i < data.declares.Length;++i)
            {
                var match = ms_proto_head_reg.Match(data.declares[i].head);
                if (!match.Success)
                {
                    LogErrorFormat(errorDescriptions[(int)ErrorCode.EC_HEAD_INVALID],data.sheetName,i + 1);
                    return false;
                }

                match = ms_proto_var_type_reg.Match(data.declares[i].type);
                if (!match.Success)
                {
                    LogErrorFormat(errorDescriptions[(int)ErrorCode.EC_TYPE_INVALID],data.sheetName,data.declares[i].type,i + 1);
                    return false;
                }

                match = ms_proto_var_name_reg.Match(data.declares[i].name);
                if (!match.Success)
                {
                    LogErrorFormat(errorDescriptions[(int)ErrorCode.EC_NAME_INVALID],data.sheetName,data.declares[i].name,i + 1);
                    return false;
                }
            }
            return true;
        }

        protected void markVarType(ExcelSheetData data)
        {
            for(int i = 0 ; i < data.declares.Length;++i)
            {
                if(string.Equals(data.declares[i].type,@"string"))
                {
                    data.declares[i].eType = HeadData.VarType.VT_STRING;
                    continue;
                }

                if(string.Equals(data.declares[i].type,@"sint32"))
                {
                    data.declares[i].eType = HeadData.VarType.VT_INT;
                    continue;
                }

                if(string.Equals(data.declares[i].type,@"bool"))
                {
                    data.declares[i].eType = HeadData.VarType.VT_BOOLEAN;
                    continue;
                }

                if(string.Equals(data.declares[i].type,@"float"))
                {
                    data.declares[i].eType = HeadData.VarType.VT_FLOAT;
                    continue;
                }

                if(string.Equals(data.declares[i].type,@"enum"))
                {
                    data.declares[i].eType = HeadData.VarType.VT_ENUM;
                    continue;
                }
            }
        }
        protected void markCollectionType(ExcelSheetData data)
        {
            for(int i = 0 ; i < data.declares.Length;++i)
            {
                if(string.Equals(data.declares[i].head,@"required"))
                {
                    data.declares[i].eCollectionType = HeadData.CollectionType.CT_UNIT;
                    continue;
                }

                if(string.Equals(data.declares[i].head,@"repeated"))
                {
                    data.declares[i].eCollectionType = HeadData.CollectionType.CT_ARRAY;
                }
            }
        }

        protected string generateEnumDeclartions(ExcelSheetData data)
        {
            string enumContent = string.Empty;
            for(int i = 0 ; i < data.declares.Length;++i)
            {
                var declare = data.declares[i];
                if(declare.eType != HeadData.VarType.VT_ENUM)
                {
                    continue;
                }

                string enumText = string.Format("public enum {0}\n",declare.name);
                enumText += "\t\t{\n";

                var lines = declare.description.Split('\r','\n');
                for(int j = 0 ; j < lines.Length ; ++j)
                {
                    var match = ms_enum_prop_reg.Match(lines[j]);
                    if(!match.Success)
                    {
                        continue;
                    }
                    enumText += string.Format("\t\t\t{0} = {1},\n",match.Groups[1].Value,match.Groups[2].Value);
                }

                enumText += "\t\t}\n";

                enumContent += enumText;
            }
            return enumContent;
        }

        protected bool generateCSharpCode(ExcelSheetData data)
        {
            var template_file = System.IO.Path.GetFullPath(Application.dataPath + "/SmartFrame/Tools/TableConvert/Editor/TableTemplate.cs");
            if(!System.IO.File.Exists(template_file))
            {
                LogErrorFormat(errorDescriptions[(int)ErrorCode.EC_TEMPLATE_FILE_NOT_EXIST],template_file);
                return false;
            }

            var content = System.IO.File.ReadAllText(template_file);
            content = content.Replace("TableTemplate",data.sheetName);
            content = content.Replace("return 0;//<id>","return ID;");
            content = content.Replace("//<enum>",generateEnumDeclartions(data));
            string varFields = string.Empty;
            for(int i = 0 ; i < data.declares.Length;++i)
            {
                var declare = data.declares[i];
                string varField = string.Empty;
                if(declare.eCollectionType == HeadData.CollectionType.CT_UNIT)
                {
                    varField = HeadData.unit_implementations[(int)declare.eType];
                }
                else if(data.declares[i].eCollectionType == HeadData.CollectionType.CT_ARRAY)
                {
                    varField = HeadData.array_implementations[(int)declare.eType];
                }

                if(declare.eType == HeadData.VarType.VT_ENUM)
                {
                    varField = string.Format(varField,data.declares[i].name);
                }
                else
                {
                    varField = string.Format(varField,data.declares[i].name);
                }

                if(string.IsNullOrEmpty(varFields))
                {
                    varFields = varField;
                }
                else
                {
                    varFields += "\n\t\t" + varField;
                }
            }
            content = content.Replace("//<content>",varFields);
            var code_file = System.IO.Path.GetFullPath(Application.dataPath + string.Format("/SmartFrame/TableScripts/{0}.cs",data.sheetName));
            System.IO.File.WriteAllText(code_file,content);

            LogFormat("Convert [{0}.cs]: Succeed ...",data.sheetName);
            return true;
        }

        protected void makeEnumImpl(ExcelSheetData data)
        {
            for(int i = 0 ; i < data.declares.Length ; ++i)
            {
                var declare = data.declares[i];
                if(declare.eType != HeadData.VarType.VT_ENUM)
                {
                    continue;
                }

                if(null == declare.enumValueSet)
                {
                    declare.enumValueSet = new Dictionary<int, int>();
                }
                else
                {
                    declare.enumValueSet.Clear();
                }

                var lines = declare.description.Split('\r','\n');
                for(int j = 0 ; j < lines.Length ; ++j)
                {
                    var line = lines[j].Trim();
                    var match = ms_enum_prop_reg.Match(line);
                    if(!match.Success)
                    {
                        continue;
                    }
                    int enumValue = int.Parse(match.Groups[2].Value);
                    if(!declare.enumValueSet.ContainsKey(enumValue))
                    {
                        declare.enumValueSet.Add(enumValue,enumValue);
                    }
                }
            }
        }
        
        protected object generateData(ref string sheetName,int coloum,HeadData declare,ref string content,out bool succeed)
        {
            if(declare.eType == HeadData.VarType.VT_INT)
            {
                int result = 0;
                if(!string.IsNullOrEmpty(content) && !int.TryParse(content,out result))
                {
                    LogErrorFormat("[{0}]:parse int failed coloumName = [{1}] row = [{2}] data = [{3}]",sheetName,declare.name,coloum,content);
                    succeed = false;
                    return null;
                }

                succeed = true;
                return result;
            }
            if(declare.eType == HeadData.VarType.VT_BOOLEAN)
            {
                int result = 0;
                if(!string.IsNullOrEmpty(content) && !int.TryParse(content,out result))
                {
                    LogErrorFormat("[{0}]:parse int failed coloumName = [{1}] row = [{2}]",sheetName,declare.name,coloum);
                    succeed = false;
                    return null;
                }

                bool bValue = result == 1 ? true : false;

                succeed = true;
                return bValue;
            }
            if(declare.eType == HeadData.VarType.VT_FLOAT)
            {
                float result = 0.0f;
                if(!string.IsNullOrEmpty(content) && !float.TryParse(content,out result))
                {
                    LogErrorFormat("[{0}]:parse float failed coloumName = [{1}] row = [{2}]",sheetName,declare.name,coloum);
                    succeed = false;
                    return null;
                }

                succeed = true;
                return result;
            }
            if(declare.eType == HeadData.VarType.VT_STRING)
            {
                if(string.IsNullOrEmpty(content))
                {
                    succeed = true;
                    return string.Empty;
                }

                succeed = true;
                return content;
            }
            if(declare.eType == HeadData.VarType.VT_ENUM)
            {
                int result = 0;
                if(string.IsNullOrEmpty(content))
                {
                    LogErrorFormat("[{0}]: parse enum failed content is empty coloumName = [{1}] row = [{2}]",sheetName,declare.name,coloum);
                    succeed = false;
                    return null;
                }

                if(!int.TryParse(content,out result))
                {
                    LogErrorFormat("[{0}]:parse int failed coloumName = [{1}] row = [{2}]",sheetName,declare.name,coloum);
                    succeed = false;
                    return null;
                }

                if(!declare.enumValueSet.ContainsKey(result))
                {
                    LogErrorFormat("[{0}]:enum value error enumName = [{1}] implValue = [{2}]",sheetName,declare.name,result);
                    succeed = false;
                    return null;
                }

                succeed = true;
                return result;
            }

            succeed = false;
            return null;
        }

        protected bool generateTableAsset(ExcelSheetData data)
        {
            //var file = UnityEditor.AssetDatabase.
            var so = ScriptableObject.CreateInstance(string.Format("Smart.Table.{0}",data.sheetName));
            if(null == so)
            {
                LogErrorFormat("create [{0}] so failed ...",data.sheetName);
                return false;
            }

            var type = so.GetType();
            var fieldInfo = type.GetField("datas",BindingFlags.Public | BindingFlags.Instance);
            if(null == fieldInfo)
            {
                LogErrorFormat("[{0}] get field datas failed ...",data.sheetName);
                return false;
            }

            var asset_file = string.Format("Assets/SmartFrame/DataAssets/Table/{0}.asset",data.sheetName);
            UnityEditor.AssetDatabase.CreateAsset(so,asset_file);

            //make enum implementation
            makeEnumImpl(data);

            var tableItems = Array.CreateInstance(type.Assembly.GetType(string.Format("Smart.Table.{0}Item",data.sheetName))
            ,data.datas.Length);

            for(int i = 0 ; i < data.datas.Length ; ++i)
            {
                var rowData = data.datas[i];
                var tableItem = type.Assembly.CreateInstance(string.Format("Smart.Table.{0}Item",data.sheetName));
                if(null == tableItem)
                {
                    LogErrorFormat("[0]:create tableItem failed ...",data.sheetName);
                    return false;
                }
                var itemType = tableItem.GetType();

                for(int j = 0 ; j < rowData.datas.Length ; ++j)
                {
                    var declare = data.declares[j];
                    if(declare.eCollectionType == HeadData.CollectionType.CT_UNIT)
                    {
                        var fieldName = declare.eType == HeadData.VarType.VT_ENUM ? "e" + declare.name : declare.name;
                        var tableField = itemType.GetField(fieldName,BindingFlags.Public|BindingFlags.Instance);
                        bool succeed = false;
                        var result = generateData(ref data.sheetName,j + 6, declare,ref rowData.datas[j],out succeed);
                        if(!succeed)
                        {
                            return false;
                        }
                        tableField.SetValue(tableItem,result);
                    }
                    else if(declare.eCollectionType == HeadData.CollectionType.CT_ARRAY)
                    {
                        var fieldName = declare.eType == HeadData.VarType.VT_ENUM ? "e" + declare.name : declare.name;
                        var tableField = itemType.GetField(fieldName,BindingFlags.Public|BindingFlags.Instance);
                        bool succeed = false;
                        var contents = null == rowData.datas[j] ? new string[0] : rowData.datas[j].Split('|');
                        var arrayItems = Array.CreateInstance(HeadData.array_element_types[(int)declare.eType],contents.Length);
                        for(int k = 0 ; k < contents.Length ; ++k)
                        {
                            var result = generateData(ref data.sheetName,j + 6, declare,ref contents[k],out succeed);
                            if(!succeed)
                            {
                                return false;
                            }
                            arrayItems.SetValue(result,k);
                        }
                        tableField.SetValue(tableItem,arrayItems);
                    }
                }

                tableItems.SetValue(tableItem,i);
            }

            fieldInfo.SetValue(so,tableItems);
            UnityEditor.EditorUtility.SetDirty(so);
            UnityEditor.AssetDatabase.SaveAssets();
            return true;
        }

        public bool Convert(ExcelSheetData data,int flag)
        {
            if(!verifyHeadContent(data))
            {
                return false;
            }
            markVarType(data);
            markCollectionType(data);
            //generate csharp code
            if((flag & (int)ConvertFlag.CF_CSharpCode) == (int)ConvertFlag.CF_CSharpCode)
            {
                if(!generateCSharpCode(data))
                {
                    return false;
                }
            }
            //generate table asset
            if((flag & (int)ConvertFlag.CF_TableAsset) == (int)ConvertFlag.CF_TableAsset)
            {
                if(!generateTableAsset(data))
                {
                    return false;
                }
            }

            return true;
        }
    }
}