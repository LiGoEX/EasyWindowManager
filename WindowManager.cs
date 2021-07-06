using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WindowLayer
{
    BackgroudLayer,
    NormalLayer,
    TopLayer,
}

public class WindowObject
{
    public WindowConfig Config;
    public GameObject Go;
};

[System.Serializable]
public struct WindowConfig
{
    public string Name;
    public string LoadPath;
    public WindowLayer Layer;
    public bool DestroyWhenClose;
}

public class WindowManager : MonoSingleton<WindowManager>
{
    Dictionary<string, WindowConfig> windowConfigDict = new Dictionary<string, WindowConfig>();
    Stack<WindowObject> windowObjectStack = new Stack<WindowObject>();
    Dictionary<string, WindowObject> windowObjectDict = new Dictionary<string, WindowObject>();
    Dictionary<string, WindowObject> windowObjectPool = new Dictionary<string, WindowObject>();


    protected override void Init()
    {
        InitConfigs();
        OpenWindow("UIMain");
    }

    private void InitConfigs()
    {
        AddWindowConfig("UIMain", "Windows/UIMain", WindowLayer.BackgroudLayer);
        AddWindowConfig("UITask", "Windows/UITask", WindowLayer.NormalLayer);
        AddWindowConfig("UITips", "Windows/UITips", WindowLayer.TopLayer);
    }

    //添加窗口属性
    private void AddWindowConfig(string windowName, string loadPath, WindowLayer windowLayer, bool destroyWhenClose = false)
    {
        WindowConfig windowConfig = new WindowConfig();
        windowConfig.Name = windowName;
        windowConfig.LoadPath = loadPath;
        windowConfig.Layer = windowLayer;
        windowConfig.DestroyWhenClose = destroyWhenClose;
        windowConfigDict.Add(windowName, windowConfig);
    }

    //打开窗口
    public void OpenWindow(string windowName)
    {
        if (!windowConfigDict.ContainsKey(windowName))
        {
            Debug.LogError("windowConfigDict doesn't ContainsKey: " + windowName);
            return;
        }

        if (GetWindowObject(windowName) != null)
        {
            Debug.LogError("Already opened!");
            return;
        }

        InnerCreateWindow(windowConfigDict[windowName]);
    }

    //关闭窗口
    public void CloseWindow(string windowName)
    {
        InnerCloseWindow(GetWindowObject(windowName));
    }

    //关闭窗口
    public void CloseWindow(WindowObject windowObject)
    {
        InnerCloseWindow(windowObject);
    }

    //关闭当前窗口(回退)
    public void CloseCurrWindow()
    {
        if (windowObjectStack.Count == 1)
        {
            return;
        }

        InnerCloseWindow(windowObjectStack.Pop());
    }

    //关闭所有窗口
    public void CloseAllWindows()
    {
        foreach (var window in windowObjectStack)
        {
            InnerCloseWindow(window);
        }

        windowObjectStack.Clear();
        windowObjectDict.Clear();
    }

    //关闭层级所有窗口
    public void CloseAllWindowsByLayer(WindowLayer layer)
    {
        foreach (var item in windowObjectStack)
        {
            if (item.Config.Layer.Equals(layer))
            {
                InnerCloseWindow(item);
            }
        }
    }

    //关闭其它层级窗口
    public void CloseAllWindowsExceptLayer(WindowLayer layer)
    {
        foreach (var item in windowObjectStack)
        {
            if (!item.Config.Layer.Equals(layer))
            {
                InnerCloseWindow(item);
            }
        }
    }

    public WindowObject GetCurrWindowObject()
    {
        return windowObjectStack.Peek();
    }

    public WindowObject GetWindowObject(string windowName)
    {
        if (windowObjectDict.ContainsKey(windowName))
        {
            return windowObjectDict[windowName];
        }
        return null;
    }

    private void InnerCreateWindow(WindowConfig config)
    {
        WindowObject windowObject;
        if (windowObjectPool.ContainsKey(config.Name))
        {
            windowObject = windowObjectPool[config.Name];
            windowObject.Go.SetActive(true);
        }
        else
        {
            windowObject = new WindowObject();
            windowObject.Config = config;
            var ob = Resources.Load(config.LoadPath);
            windowObject.Go = Instantiate(ob) as GameObject;
        }

        if (!windowObject.Config.DestroyWhenClose && !windowObjectPool.ContainsKey(windowObject.Config.Name))
        {
            windowObjectPool.Add(windowObject.Config.Name, windowObject);
        }

        windowObjectStack.Push(windowObject);
        windowObjectDict[config.Name] = windowObject;
    }

    private void InnerCloseWindow(WindowObject windowObject)
    {
        if (windowObject != null)
        {
            if (windowObject.Config.DestroyWhenClose)
            {
                if (windowObjectPool.ContainsKey(windowObject.Config.Name))
                {
                    windowObjectPool.Remove(windowObject.Config.Name);
                }

                GameObject.Destroy(windowObject.Go);
            }
            else
            {
                windowObject.Go.SetActive(false);
            }
            windowObjectDict.Remove(windowObject.Config.Name);
        }
    }

    void Update()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Escape)) // 返回键
        {
            CloseCurrWindow();
        }
#endif

        if (Input.GetKeyDown(KeyCode.Z))
        {
            OpenWindow("UIMain");
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            OpenWindow("UITask");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            OpenWindow("UITips");
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            CloseCurrWindow();
        }

        if (Input.GetKeyUp(KeyCode.V))
        {
            CloseAllWindows();
        }
    }
}