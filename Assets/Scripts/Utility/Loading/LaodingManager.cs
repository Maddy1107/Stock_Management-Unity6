using System;
using System.Threading.Tasks;
using UnityEngine;

public static class LoadingManager
{
    public static async Task ShowWhile(Func<Task> taskFunc, string loadingText = "Loading...", string tip = null)
    {
        if (LoadingScreen.Instance == null)
        {
            Debug.LogError("LoadingScreen is missing in the scene!");
            return;
        }

        LoadingScreen.Instance.Show();
        LoadingScreen.Instance.SetLoadingText(loadingText);

        try
        {
            await taskFunc.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during async task: {ex}");
        }

        await Task.Delay(200); // smooth UI experience
        LoadingScreen.Instance.Hide();
    }

    public static async Task<T> ShowWhile<T>(Func<Task<T>> taskFunc, string loadingText = "Loading...", string tip = null)
    {
        T result = default;

        if (LoadingScreen.Instance == null)
        {
            Debug.LogError("LoadingScreen is missing in the scene!");
            return result;
        }

        LoadingScreen.Instance.Show();
        LoadingScreen.Instance.SetLoadingText(loadingText);

        try
        {
            result = await taskFunc.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during async task: {ex}");
        }

        await Task.Delay(200);
        LoadingScreen.Instance.Hide();

        return result;
    }
}
