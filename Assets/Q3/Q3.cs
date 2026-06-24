using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

/**
**该题目校招岗位可以不作答，社招需要作答。**

按照要求在 {@link Q3.onStartBtnClick} 中编写一段异步任务处理逻辑，具体执行步骤如下：
1. 调用 {@link Q3.loadConfig} 加载配置文件，获取资源列表
2. 根据资源列表调用 {@link Q3.loadFile} 加载资源文件
3. 资源列表中的所有文件加载完毕后，调用 {@link Q3.initSystem} 进行系统初始化
4. 系统初始化完成后，打印日志

附加要求
1. 加载文件时，需要做并发控制，最多并发 3 个文件
2. 加载文件时，需要添加超时控制，超时时间为 3 秒
3. 加载文件失败时，需要对单文件做 backoff retry 处理，重试次数为 3 次
4. 对错误进行捕获并打印输出
*/

public class Q3 : MonoBehaviour
{
    private CancellationTokenSource _cts;

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public async void OnStartBtnClick()
    {
        // 取消之前的任务
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;

        try
        {
            // 1. 加载配置文件，获取资源列表
            string[] files = await LoadConfig();

            // 检查是否已取消
            if (token.IsCancellationRequested) return;

            // 2. 并发加载文件，最多3个并发，超时3秒，失败重试3次
            SemaphoreSlim semaphore = new SemaphoreSlim(3);
            List<Task> loadTasks = new List<Task>();

            foreach (string file in files)
            {
                if (token.IsCancellationRequested) break;
                await semaphore.WaitAsync(token);
                loadTasks.Add(LoadFileWithRetry(file, token).ContinueWith(t => semaphore.Release()));
            }

            await Task.WhenAll(loadTasks);

            if (token.IsCancellationRequested) return;

            // 3. 所有文件加载完毕，初始化系统
            await InitSystem();

            // 4. 打印完成日志
            Debug.Log("All tasks completed successfully");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Task was cancelled");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error: {e.Message}");
        }
    }

    private async Task LoadFileWithRetry(string file, CancellationToken token)
    {
        int maxRetries = 3; // 重试次数
        int retryDelay = 1000; // 初始延迟1秒

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                using (CancellationTokenSource timeoutCts = new CancellationTokenSource(3000))
                using (CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutCts.Token))
                {
                    await LoadFileWithTimeout(file, linkedCts.Token);
                    return; // 成功则返回
                }
            }
            catch (OperationCanceledException) when (!token.IsCancellationRequested)
            {
                // 超时，不是外部取消
                throw new TimeoutException($"Load file timeout: {file}");
            }
            catch (Exception e)
            {
                if (attempt == maxRetries)
                {
                    Debug.LogError($"Failed to load {file} after {maxRetries} retries: {e.Message}");
                    throw;
                }

                Debug.LogWarning($"Retry {attempt + 1}/{maxRetries} for {file}: {e.Message}");
                await Task.Delay(retryDelay, token);
                retryDelay *= 2; // backoff: 延迟时间翻倍
            }
        }
    }

    private async Task LoadFileWithTimeout(string file, CancellationToken token)
    {
        Task loadTask = LoadFile(file);
        Task timeoutTask = Task.Delay(Timeout.Infinite, token);

        Task completedTask = await Task.WhenAny(loadTask, timeoutTask);

        if (completedTask == timeoutTask)
        {
            throw new TimeoutException($"Load file timeout: {file}");
        }

        await loadTask; // 确保异常被抛出
    }

    // #region 以下是辅助测试题而写的一些 mock 函数，请勿修改

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <returns>文件列表</returns>
    public async Task<string[]> LoadConfig()
    {
        Debug.Log("load config start");
        await Task.Delay(1000);
        if (Random.value > 0.01f)
        {
            Debug.Log("load config success");
            string[] files = new string[100];
            for (int i = 0; i < 100; i++)
            {
                files[i] = $"file-{i}";
            }
            return files;
        }
        else
        {
            Debug.Log("load config failed");
            throw new System.Exception("Load config failed");
        }
    }

    /// <summary>
    /// 加载文件
    /// </summary>
    /// <param name="file">文件名</param>
    /// <returns></returns>
    public async Task LoadFile(string file)
    {
        Debug.Log($"load file start: {file}");
        await Task.Delay(Random.Range(1000, 5000));
        if (Random.value > 0.01f)
        {
            Debug.Log($"load file success: {file}");
        }
        else
        {
            Debug.Log($"load file failed: {file}");
            throw new System.Exception($"Load file failed: {file}");
        }
    }

    /// <summary>
    /// 初始化系统
    /// </summary>
    /// <returns></returns>
    public async Task InitSystem()
    {
        Debug.Log("init system start");
        await Task.Delay(1000);
        Debug.Log("init system success");
    }

    // #endregion
}
