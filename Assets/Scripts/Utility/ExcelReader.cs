using MiniExcelLibs;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ExcelReader
{
    // 1. Reads and returns all rows from Excel
    public static List<IDictionary<string, object>> ReadExcelRows(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("❌ Excel file not found at: " + filePath);
            return null;
        }

        try
        {
            var rows = MiniExcel.Query(filePath);
            var rowList = new List<IDictionary<string, object>>();

            foreach (var row in rows)
            {
                if (row is IDictionary<string, object> dict)
                    rowList.Add(dict);
            }

            return rowList;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("❌ Failed to read Excel: " + ex.Message);
            return null;
        }
    }

    // 2. Finds the value from column D for a matching product name in column C
    public static string FindProductValue(List<IDictionary<string, object>> rows, string targetProductName)
    {
        if (rows == null || rows.Count == 0)
        {
            Debug.LogError("❌ No rows to search.");
            return null;
        }

        int startRow = 5;
        int rowIndex = 1;

        foreach (var dict in rows)
        {
            if (rowIndex >= startRow)
            {
                object[] values = new object[dict.Count];
                dict.Values.CopyTo(values, 0);

                if (values.Length >= 4)
                {
                    string product = values[2]?.ToString()?.Trim();
                    string value = values[3]?.ToString()?.Trim();

                    if (product == targetProductName)
                    {
                        Debug.Log($"✅ Match found for {product}: {value}");
                        return value;
                    }
                }
            }
            rowIndex++;
        }

        Debug.LogWarning($"❌ Product '{targetProductName}' not found.");
        return null;
    }
}
