using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace I18NTool
{
    public static class I18NHelper
    {
        public static void Start()
        {
            try
            {
                if (File.Exists("Language.xlsx"))
                {
                    Dictionary<int, LangData> LangDict = new Dictionary<int, LangData>();

                    IWorkbook wk = null;
                    FileStream fs = File.OpenRead("Language.xlsx");
                    wk = new XSSFWorkbook(fs);
                    fs.Close();
                    var sheet = wk.GetSheetAt(0);

                    // 获取所有语言名称
                    Console.WriteLine("Search language name...");
                    IRow rowHead = sheet.GetRow(0);
                    for (int i = 1; i < 100; i++)
                    {
                        ICell cell = rowHead.GetCell(i);
                        if (cell != null && !string.IsNullOrWhiteSpace(cell.StringCellValue))
                        {
                            Console.WriteLine(cell.StringCellValue);
                            LangDict[i] = new LangData(cell.StringCellValue);
                        }
                        else
                        {
                            break;
                        }
                    }

                    Console.WriteLine("Search key...");
                    // 填充数据
                    for (int rowIndex = 1; rowIndex < sheet.LastRowNum; rowIndex++)
                    {
                        IRow row = sheet.GetRow(rowIndex);
                        if (row != null)
                        {
                            ICell keyCell = row.GetCell(0);
                            if (keyCell != null && !string.IsNullOrWhiteSpace(keyCell.StringCellValue))
                            {
                                string key = keyCell.StringCellValue;
                                Console.WriteLine(key);
                                for (int colIndex = 1; colIndex <= LangDict.Count; colIndex++)
                                {
                                    ICell valueCell = row.GetCell(colIndex);
                                    if (valueCell != null && !string.IsNullOrWhiteSpace(valueCell.StringCellValue))
                                    {
                                        LangDict[colIndex].Dict[key] = valueCell.StringCellValue;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"please check row {rowIndex} col {colIndex}.");
                                    }
                                }
                            }
                        }
                    }

                    Console.WriteLine("Create txt...");
                    // 创建txt
                    foreach (var langData in LangDict.Values)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var kv in langData.Dict)
                        {
                            sb.AppendLine($"{kv.Key}={kv.Value}");
                        }
                        File.WriteAllText($"{langData.LanguageName}.txt", sb.ToString());
                        Console.WriteLine($"{langData.LanguageName}.txt");
                    }
                    Console.WriteLine("Done!");
                }
                else
                {
                    Console.WriteLine("This tool needs to be placed next to the Language.xlsx file ");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    public class LangData
    {
        public string LanguageName;
        public Dictionary<string, string> Dict;

        public LangData(string name)
        {
            LanguageName = name;
            Dict = new Dictionary<string, string>();
        }
    }
}
