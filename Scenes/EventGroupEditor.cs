using Godot;
using System;

public partial class EventGroupEditor : Window
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	 // 你的字符串变量
    private string _data = "这是我的初始数据";

    /// <summary>
    /// 保存函数：呼出对话框并保存
    /// </summary>
    public void SaveData()
    {
        FileDialog dialog = CreateBasicDialog(FileDialog.FileModeEnum.SaveFile);
        
        // 当用户点击“保存”并确认路径时触发
        dialog.FileSelected += (path) =>
        {
            SaveToFile(path);
            dialog.QueueFree(); // 使用完后释放内存
        };

        // 当用户点击取消时触发
        dialog.Canceled += () => dialog.QueueFree();

        AddChild(dialog);
        dialog.PopupCentered();
    }

    /// <summary>
    /// 加载函数：呼出对话框并读取
    /// </summary>
    public void LoadData()
    {
        FileDialog dialog = CreateBasicDialog(FileDialog.FileModeEnum.OpenFile);

        // 当用户选中文件并点击“打开”时触发
        dialog.FileSelected += (path) =>
        {
            ReadFromFile(path);
            dialog.QueueFree();
        };

        dialog.Canceled += () => dialog.QueueFree();

        AddChild(dialog);
        dialog.PopupCentered();
    }

    // --- 内部处理逻辑 ---

    /// <summary>
    /// 初始化通用对话框设置
    /// </summary>
    private FileDialog CreateBasicDialog(FileDialog.FileModeEnum mode)
    {
        var dialog = new FileDialog();
		dialog.UseNativeDialog=true;
        dialog.Mode = Window.ModeEnum.Windowed; // 窗口模式
        dialog.FileMode = mode;
        dialog.Access = FileDialog.AccessEnum.Filesystem; // 允许访问全盘系统文件
        
        // 设置后缀过滤器
        dialog.AddFilter("*.tceg", "TCEG Data File");
        
        // 如果你使用的是 Godot 4.2+，可以开启这行来调用系统原生对话框
        // dialog.UseNativeDialog = true; 

        dialog.Title = mode == FileDialog.FileModeEnum.SaveFile ? "保存数据" : "加载数据";
        dialog.Size = new Vector2I(800, 600);
        
        return dialog;
    }

    /// <summary>
    /// 实际的写入逻辑
    /// </summary>
    private void SaveToFile(string path)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(editor.SerializeEvents(EventPanel.SelectedGroup.Stream));
            GD.Print($"保存成功: {path}");
        }
        else
        {
            GD.PrintErr($"无法保存文件，错误代码: {FileAccess.GetOpenError()}");
        }
    }

    /// <summary>
    /// 实际的读取逻辑
    /// </summary>
    private void ReadFromFile(string path)
    {
        if (!FileAccess.FileExists(path)) return;

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file != null)
        {
			var window=editor.root.GetNode<Window>("EventGroupEditor");
			var stream=window.GetNode<Node2D>("EventStream");
			foreach (var item in stream.GetChildren())
			{
				if (!(item is ColorRect))
				{
					stream.RemoveChild(item);
				}
				
			}
			EventPanel.SelectedGroup.Stream.Clear();

            _data = file.GetAsText();
			var _stream=editor.DeserializeEvents(_data);

			EventPanel.SelectedGroup.Stream=_stream;
          GD.Print(_data);
		foreach (var item in _stream)
		{
			GD.Print(item.Entity);
			stream.AddChild(item.Entity);
		}
		EventPanel.UpdateStream();
            // 这里可以添加一个自定义信号来通知 UI 更新，例如：
            // EmitSignal(SignalName.DataLoaded, _data);
        }
    }
}
