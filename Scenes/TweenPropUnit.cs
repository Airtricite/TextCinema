using Godot;
using System;
using System.Collections;

public partial class TweenPropUnit : Panel
{
	// Called when the node enters the scene tree for the first time.
	 public override void _Ready()
    {
        // 从当前节点（this）开始，递归连接所有子节点的信号
        ConnectSignalsRecursive(this);
    }

    /// <summary>
    /// 递归遍历并连接信号
    /// </summary>
	
    private void ConnectSignalsRecursive(Node parent)
    {
        foreach (Node child in parent.GetChildren())
        {
            // 根据节点类型连接不同的信号
            switch (child)
            {
                case CheckBox checkBox:
                    // Toggled 信号返回 bool，我们用 Lambda 丢弃参数并调用 Changed
                    checkBox.Toggled += (toggledOn) => Changed(toggledOn,0,checkBox.Name);
                    break;

                case OptionButton optionButton:
                    // ItemSelected 信号返回 long (index)
                    optionButton.ItemSelected += (index) => Changed(index,1,optionButton.Name);
                    break;

                case SpinBox range: // SpinBox, HSlider, VSlider 都继承自 Range
                    // ValueChanged 信号返回 double
                    range.ValueChanged += (value) => Changed(value,2,range.Name);
                    break;

            }

        }
    }

    /// <summary>
    /// 统一的触发函数
    /// </summary>
    private void Changed(dynamic value,int type,string name)
    {
		
		foreach (DictionaryEntry _item in TweenPanel.selected_units)
		{
			
			Node item=(Node)_item.Value;
			switch (type)
			{
				
				case 0:
				item.GetNode<CheckBox>((string)name).ButtonPressed=value;
				break;
				case 1:
				item.GetNode<OptionButton>((string)name).Selected=(int)value;
				break;
				case 2:
				item.GetNode<SpinBox>((string)name).Value=(float)value;
				break;
			}
		}
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	 public override void _Input(InputEvent @event)
    {
		if (@event is InputEventMouseButton ||@event is  InputEventMouseMotion )//if A is Type Instance
    {
		  Vector2 localMousePos = GetLocalMousePosition();
            Rect2 controlRect = new Rect2(Vector2.Zero, Size);
            
            if (!controlRect.HasPoint(localMousePos))
            {
                // 鼠标在Control节点内
				return;
            }
        // 获取外部变量drag_type，is_dragging
        if (TweenPanel.is_dragging)
        {
            // 根据drag_type选上或取消选
            if (TweenPanel.drag_type == "ADD")
            {
                // 选上逻辑
                // 外部全局Hashtable处理
				TweenPanel.selected_units[this]=this;
                // 高亮效果开
				GetNode<ReferenceRect>("Box").Visible=true;
            }
            else if (TweenPanel.drag_type == "REMOVE")
            {
                // 取消选逻辑
                // 外部全局Hashtable处理
				TweenPanel.selected_units[this]=null;
				GetNode<ReferenceRect>("Box").Visible=false;
                // 高亮效果关
            }
        }
    }
	}
}
