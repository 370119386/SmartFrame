using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

using System;
using System.IO;

using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.Linq;

namespace Smart.Editor
{   
    public static class ExcelHelper
    {
        [MenuItem ("Smart/TableConvert/OpenExcel")]
        public static void OpenExcel()
        {
            var excel = new ExcelReader();
            var tableRoot = System.IO.Path.GetFullPath(Application.dataPath + "/../Table/");
            var files = System.IO.Directory.GetFiles(tableRoot,"*.xls");
            ExcelDataConvert convert = new ExcelDataConvert();
            foreach (var file in files)
            {
                if(excel.Open(file))
                    convert.Convert(excel.SheetData,(int)ExcelDataConvert.ConvertFlag.CF_ReGenerate);
                excel.Close();
            }
        }
    }
    //head content
    public class HeadData
    {
        public string head;
        public string type;
        public string name;
        public string server;
        public string description;
        public enum VarType
        {
            VT_STRING = 0,
            VT_ENUM,
            VT_FLOAT,
            VT_INT,
            VT_BOOLEAN,
            VT_COUNT,
        }
        public static string[] unit_implementations = new string[(int)VarType.VT_COUNT]
        {
            @"public string {0};",
            @"public {0} e{0};",
            @"public float {0};",
            @"public int {0};",
            @"public bool {0};",
        };
        public static string[] array_implementations = new string[(int)VarType.VT_COUNT]
        {
            @"public string[] {0};",
            @"public {0}[] e{0};",
            @"public float[] {0};",
            @"public int[] {0};",
            @"public bool[] {0};",
        };
        public static Type[] array_element_types = new Type[(int)VarType.VT_COUNT]
        {
            typeof(string),
            typeof(int),
            typeof(float),
            typeof(int),
            typeof(bool),
        };
        public VarType eType;
        public enum CollectionType
        {
            CT_UNIT = 0,
            CT_ARRAY,
        }
        public CollectionType eCollectionType;
        public Dictionary<int,int> enumValueSet;
    }
    //one-row-data
    public class RowData
    {
        public string[] datas;
    }
    //sheet content
    public class ExcelSheetData
    {
        public string sheetName = string.Empty;
        public HeadData[] declares = new HeadData[0];
        public RowData[] datas =  new RowData[0];
    }

    public class ExcelReader
    {
        protected const string moduleName = @"ExcelReader";
        protected const int headRowNums = 5;
        protected const int optionRowInHead = 3;

        protected void LogFormat(string fmt,params object[] args)
        {
            try
            {
                var content = string.Format("[<color=#ff00ff>{0}</color>]:[<color=#00ffff>{1}</color>]",moduleName,string.Format(fmt,args));
                Debug.Log(content);
            }
            catch(Exception e)
            {
                var content = string.Format("[<color=#ff00ff>{0}</color>]:[Exception]:[<color=#00ffff>{1}</color>]",moduleName,e.Message);
                Debug.Log(content);
            }
        }

        protected enum ResultCode
        {
            RC_SUCCEED = 0,
            RC_FILE_NOT_EXIST,
            RC_READ_FILE_EXCEPTION,
            RC_FAILED,
            RC_NO_SHEET,
            EC_SHEET_NAME_INVALID,
            EC_UNVALID_CELL_TYPE,
            EC_ROW_HEAD_ERROR,
            RC_COUNT,
        }
        protected static string[] errExpress = new string[(int)ResultCode.RC_COUNT]
        {
            @"Read <{0}> Succeed ...",
            @"<{0}> does not exist",
            @"Read <{0}> Exception : {1}",
            @"Read <{0}> Failed",
            @"[{0}] Has No Sheet Index = [{1}]",
            @"[{0}] SheetName Is Empty",
            @"[{0}] UNVALID CELL TYPE [{1}]",
            @"SHEET ROWHEAD WHICH RANGE IN [0-2][4] MUST EXIST [3] CAN BE EMPTY",
        };

        FileStream m_fileStream;
        HSSFWorkbook m_workbook;
        ISheet m_sheet;
        string mFileName;
        ExcelSheetData m_sheetData;

        public bool Open(string path,int sheetIndex = 0)
        {
            m_sheetData = new ExcelSheetData();
            mFileName = System.IO.Path.GetFileNameWithoutExtension(path);

            if(!File.Exists(path))
            {
                LogFormat(errExpress[(int)ResultCode.RC_FILE_NOT_EXIST],mFileName);
                return false;
            }
            
            try
            {
                m_fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch(Exception e)
            {
                LogFormat(errExpress[(int)ResultCode.RC_READ_FILE_EXCEPTION],mFileName,e.Message);
                return false;
            }

            if(null == m_fileStream)
            {
                LogFormat(errExpress[(int)ResultCode.RC_FAILED],mFileName);
                return false;
            }

            m_workbook = new HSSFWorkbook(m_fileStream);
            m_sheet = m_workbook.GetSheetAt(sheetIndex);
            if(null == m_sheet)
            {
                LogFormat(errExpress[(int)ResultCode.RC_NO_SHEET],mFileName,sheetIndex);
                return false;
            }

            if (string.IsNullOrEmpty(m_sheet.SheetName))
            {
               LogFormat(errExpress[(int)ResultCode.EC_SHEET_NAME_INVALID],mFileName);
               return false;
            }

            //LogFormat("Load [{0}] Sheet Succeed ...",mFileName);
            for(int i = 0; i < headRowNums ; ++i)
            {
                var row = m_sheet.GetRow(i);
                if(null == row && i != optionRowInHead)
                {
                    LogFormat(errExpress[(int)ResultCode.EC_ROW_HEAD_ERROR],mFileName);
                    return false;
                }
            }

            var firstRow = m_sheet.GetRow(0);
            int coloumValue = getColoumsValue(firstRow);

            m_sheetData.sheetName = m_sheet.SheetName;
            m_sheetData.declares = new HeadData[coloumValue];

            for(int i = 0 ; i < coloumValue ; ++i)
            {
                m_sheetData.declares[i] = new HeadData();
            }

            for(int i = 0; i < headRowNums ; ++i)
            {
                var row = m_sheet.GetRow(i);
                if(null != row)
                {
                    for(int j = 0 ; j < row.Cells.Count ; ++j)
                    {
                        var cell = row.Cells[j];
                        if(cell.ColumnIndex < coloumValue)
                        {
                            if(i == 0)
                                m_sheetData.declares[cell.ColumnIndex].head = getCellValue(cell);
                            else if(i == 1)
                                m_sheetData.declares[cell.ColumnIndex].type = getCellValue(cell);
                            else if(i == 2)
                                m_sheetData.declares[cell.ColumnIndex].name = getCellValue(cell);
                            else if(i == 3)
                                m_sheetData.declares[cell.ColumnIndex].server = getCellValue(cell);
                            else if(i == 4)
                                m_sheetData.declares[cell.ColumnIndex].description = getCellValue(cell);
                        }
                    }
                }
            }

            List<RowData> datas = new List<RowData>(m_sheet.LastRowNum + 1);
            for(int i = headRowNums ; i <= m_sheet.LastRowNum ; ++i)
            {
                var row = m_sheet.GetRow(i);
                if(null == row)
                {
                    continue;
                }

                RowData data = new RowData();
                data.datas = new string[coloumValue];
                bool isEmpty = false;

                for(int j = 0 ; j < row.Cells.Count; ++j)
                {
                    var cell = row.Cells[j];
                    if(null == cell || cell.ColumnIndex >= coloumValue)
                    {
                        continue;
                    }
                    data.datas[cell.ColumnIndex] = getCellValue(cell);
                    if(string.IsNullOrEmpty(data.datas[cell.ColumnIndex]))
                    {
                        isEmpty = true;
                        break;
                    }
                }

                if(!isEmpty)
                    datas.Add(data);
            }
            m_sheetData.datas = datas.ToArray();

            LogFormat("Load TABLE=[<color=#00ff00>{0}</color>] SHEET=[<color=#00ff00>{1}</color>] succeed ROW=[{2}] COL=[{3}]",mFileName,m_sheet.SheetName,m_sheetData.datas.Length,m_sheetData.declares.Length);
            //printSheetContent(m_sheetData);

            return true;
        }

        public ExcelSheetData SheetData
        {
            get
            {
                return m_sheetData;
            }
        }

        public void Close()
        {
            m_sheetData = null;
            if(null != m_workbook)
            {
                m_workbook.Close();
                m_workbook = null;
            }
            if(null != m_fileStream)
            {
                m_fileStream.Close();
                m_fileStream.Dispose();
                m_fileStream = null;
            }          
        }

        void printSheetContent(ExcelSheetData data)
        {
            if(null != data)
            {
                var append = "\t\t";
                var content = @"[1]:";
                
                for(int i = 0 ; i < data.declares.Length ; ++i)
                {
                    if(i != 0)
                        content += append;
                    content += data.declares[i].head;
                }
                LogFormat(content);

                content = @"[2]:";
                for(int i = 0 ; i < data.declares.Length ; ++i)
                {
                    if(i != 0)
                        content += append;
                    content += data.declares[i].type;
                }
                LogFormat(content);

                content = @"[3]:";
                for(int i = 0 ; i < data.declares.Length ; ++i)
                {
                    if(i != 0)
                        content += append;
                    content += data.declares[i].name;
                }
                LogFormat(content);

                content = @"[4]:";
                for(int i = 0 ; i < data.declares.Length ; ++i)
                {
                    if(i != 0)
                        content += append;
                    content += data.declares[i].server;
                }
                LogFormat(content);

                content = @"[5]:";
                for(int i = 0 ; i < data.declares.Length ; ++i)
                {
                    if(i != 0)
                        content += append;
                    content += data.declares[i].description;
                }
                LogFormat(content);
                
                for(int i = 0 ; i < data.datas.Length ; ++i)
                {
                    content = "[" + (i + 6).ToString() + "]:";
                    for(int j = 0 ; j < data.datas[i].datas.Length ; ++j)
                    {
                        if(j != 0)
                            content += append;
                        content += data.datas[i].datas[j];
                    }
                    LogFormat(content);
                }
            }
        }

        string getCellValue(ICell cell)
        {
            if(null == cell)
                return string.Empty;
            if(cell.CellType == CellType.String)
                return cell.StringCellValue.Trim();
            else if(cell.CellType == CellType.Numeric)
                return cell.NumericCellValue.ToString();
            else if(cell.CellType == CellType.Boolean)
                return cell.BooleanCellValue ? 1.ToString() : 0.ToString();
            else if(cell.CellType == CellType.Blank)
                return string.Empty;
            else
                return string.Empty;
        }

        int getColoumsValue(IRow row)
		{
			int iRet = 0;
			if (null != row) 
			{
				for (int j = 0; j < row.Cells.Count; ++j) 
				{
					if (row.Cells[j].CellType == CellType.String && !string.IsNullOrEmpty (row.Cells[j].StringCellValue)) 
					{
						iRet = Math.Max (iRet, row.Cells [j].ColumnIndex + 1);
					}
				}
			}
			return iRet;
		}
    }
}