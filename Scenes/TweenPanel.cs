using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class TweenPanel : Panel
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	private Vector2 start_point; // 拖拽起点
    private Vector2 current_point; // 鼠标当前位置
    public static bool is_dragging = false; // 是否正在拖拽
    public static string drag_type; // "ADD" 或 "REMOVE"
    public static Hashtable selected_units = new Hashtable(); // 已选择单位的列表

    public override void _Input(InputEvent @event)
    {
        // 处理按下
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
            {
                drag_type = "ADD";
                is_dragging = true;
            }
            
            if (mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed)
            {
                drag_type = "REMOVE";
                is_dragging = true;
            }
            
            // 鼠标左键或右键松开
            if ((mouseButton.ButtonIndex == MouseButton.Left || mouseButton.ButtonIndex == MouseButton.Right) 
                && !mouseButton.Pressed)
            {
                is_dragging = false;
            }
        }
    }
}
