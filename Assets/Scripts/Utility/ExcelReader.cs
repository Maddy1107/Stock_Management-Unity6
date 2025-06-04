using MiniExcelLibs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

    public static void CopyExcelFile(string sourcePath, string destinationPath, bool overwrite = true)
    {
        if (!File.Exists(sourcePath))
        {
            Debug.LogError("❌ Source Excel file not found at: " + sourcePath);
            return;
        }

        try
        {
            File.Copy(sourcePath, destinationPath, overwrite);
            Debug.Log($"✅ Excel file copied to: {destinationPath}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"❌ Failed to copy Excel file: {ex.Message}");
        }
    }
    
    public static void UpdateExcelProductValues(string excelPath, Dictionary<string, string> productData)
    {
        if (!File.Exists(excelPath))
        {
            Debug.LogError("❌ Excel file not found at: " + excelPath);
            return;
        }

        try
        {
            var rows = ReadExcelRows(excelPath);
            var finalData = new List<List<object>>();

            int rowIndex = 1;

            foreach (var row in rows)
            {
                var values = row.Values.ToList();
                // Pad to 4 columns if short
                while (values.Count < 4)
                    values.Add(null);

                // Start updates from row 5 (MiniExcel is 1-based)
                if (rowIndex >= 5)
                {
                    string product = values[2]?.ToString()?.Trim();

                    if (!string.IsNullOrEmpty(product) && productData.ContainsKey(product))
                    {
                        values[3] = productData[product];
                        Debug.Log($"✅ Updated '{product}' to '{productData[product]}'");
                    }
                }

                finalData.Add(values);
                rowIndex++;
            }

            if(File.Exists(excelPath))
            {
                File.Delete(excelPath);
                Debug.Log($"🗑️ Original Excel file deleted: {excelPath}");
            }

            // Overwrite the same Excel file with updated values
            MiniExcel.SaveAs(excelPath, finalData);
            Debug.Log("✅ Excel file successfully updated.");
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ Error while updating Excel: " + ex.Message);
        }
    }

    public static string UpdateMonthInFileName(string originalFilePath)
    {
        if (string.IsNullOrWhiteSpace(originalFilePath))
            return originalFilePath;

        string currentMonth = DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture); // e.g., "June"

        // Match any month name from the list and replace it
        string[] monthNames = CultureInfo.InvariantCulture.DateTimeFormat.MonthNames;
        foreach (var month in monthNames)
        {
            if (!string.IsNullOrEmpty(month) && originalFilePath.Contains(month, StringComparison.OrdinalIgnoreCase))
            {
                return originalFilePath.Replace(month, currentMonth, StringComparison.OrdinalIgnoreCase);
            }
        }

        // If no month was found, just return as-is
        return originalFilePath;
    }

    public static void ExportExcel(string originalFilePath, Dictionary<string, string> productData)
    {
        // Create a temp file with updated month in filename
        string tempFileName = UpdateMonthInFileName(Path.GetFileNameWithoutExtension(originalFilePath));
        string tempFilePath = Path.Combine(Application.persistentDataPath, tempFileName + ".xlsx");

        // Copy and edit
        CopyExcelFile(originalFilePath, tempFilePath, true);

        UpdateExcelProductValues(tempFilePath, productData);

#if UNITY_ANDROID && !UNITY_EDITOR
            // Android: save to Downloads
            string downloadsPath = "/storage/emulated/0/Download"; // common location
            string destFile = Path.Combine(downloadsPath, tempFileName);

            try
            {
                File.Copy(tempFilePath, destFile, overwrite: true);
                Debug.Log("✅ Exported to Android Downloads: " + destFile);
            }
            catch (Exception e)
            {
                Debug.LogError("❌ Failed to export on Android: " + e.Message);
            }

#elif UNITY_EDITOR || UNITY_STANDALONE
#if UNITY_EDITOR
        // Unity Editor: show Save File Panel
        string savePath = UnityEditor.EditorUtility.SaveFilePanel(
            "Save Exported Excel File",
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            tempFileName,
            "xlsx"
        );

        if (!string.IsNullOrEmpty(savePath))
        {
            try
            {
                File.Copy(tempFilePath, savePath, overwrite: true);
                Debug.Log("✅ Exported to Editor/Desktop: " + savePath);
            }
            catch (Exception e)
            {
                Debug.LogError("❌ Failed to export on Editor: " + e.Message);
            }
        }
        else
        {
            Debug.Log("⚠️ Export cancelled by user.");
        }
#endif
#else
            Debug.LogWarning("❌ Export not supported on this platform.");
#endif
        
        try
        {
            if (File.Exists(tempFilePath))
            {
            File.Delete(tempFilePath);
            Debug.Log($"🗑️ Temporary file deleted: {tempFilePath}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"⚠️ Could not delete temporary file: {ex.Message}");
        }
    }

}
