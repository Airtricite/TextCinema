using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public partial class EventBlock : Button
{
	bool isDragging=false;bool isEntered=false;
	Vector2 initialMousePos=Vector2.Zero;
	float offset=0;
	float ioffset=0;
	float fram=0;
	long ifram=0;
	long uid=0;
	editor.Event _event;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	uid = (long)GetParent().GetNode<Marker2D>("uid").Position.X;
		_event=editor._select_event(uid);
		
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
	int click=0;
	float timer=0;
	void OpenGroup()
	{
		GD.PrintErr("Group ",uid);
		if (_event is editor.EventGroup)
		{
		var window=editor.root.GetNode<Window>("EventGroupEditor");
		var stream=window.GetNode<Node2D>("EventStream");
		var group=(editor.EventGroup)_event;
		EventPanel.SelectedGroup=group;
		// add entity
		foreach (var item in stream.GetChildren())
		{
			if (!(item is ColorRect))
			{
				stream.RemoveChild(item);
			}
			
		}
		foreach (var item in group.Stream)
		{
			GD.Print(item.Entity);
			if (item.Entity==null)
			{
				item.Entity = EventPanel.event_block.Instantiate<Node2D>();
				if (item is editor.Mark||item is editor.BPMMark)
			{
				// add marker 
				var mark = editor._Mark.Instantiate<Node2D>();
				item.Entity.GetNode("Marks").AddChild(mark);
			}
			item.Entity.GetNode<Marker2D>("uid").Position = Vector2.Right * item.uid;
			item.button = item.Entity.GetNode<Button>("EventBlock");
			}
			stream.AddChild(item.Entity);
		}
		window.Popup();
		EventPanel.UpdateStream();
		}
	}
   
    void entered(){isEntered=true;} void exited(){isEntered=false;}
	 public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            if (mouseMotion.ButtonMask == MouseButtonMask.Left&&isEntered&&!editor.rect_selecting)
            {
                if (!isDragging)
                {
                    // Start dragging
					uid = (long)GetParent().GetNode<Marker2D>("uid").Position.X;
					editor.select_event(uid);
                    initialMousePos = mouseMotion.Position;
					//ioffset=_event.posX;
					//ifram=_event.frame_stamp;
                    isDragging = true;
					foreach (editor.Event _e in editor.SelectedEvents)
					{
						_e.Entity.GetChild<Marker2D>(4).Position=new Vector2(_e.posX,_e.frame_stamp);
					}
                }
				float dx=initialMousePos.X-mouseMotion.Position.X;
				float dy=initialMousePos.Y-mouseMotion.Position.Y;
				foreach (editor.Event _e in editor.SelectedEvents)
					{
						_e.posX=(long)(_e.Entity.GetChild<Marker2D>(4).Position.X-dx);
						if (!editor.pressing_alt)
						{
							_e.frame_stamp=(long)(_e.Entity.GetChild<Marker2D>(4).Position.Y-dy/EventPanel.scale);
						}
						
					}
				//_event.posX=(long)(ioffset-dx);
				//_event.frame_stamp=(long)(ifram-dy);
				//GetParent<Node2D>().Position=GetParent<Node2D>().Position.Y*Vector2.Down+Vector2.Left*offset;
				EventPanel.UpdateStream();
            }
            else
            {
                // Stop dragging
				
                isDragging = false;
            }
        }
    }
	void Edit(string PropName)
	{
		/*Type t = _E.GetType(); PropertyInfo prop = t.GetProperty(GetNode<Label>("PropName").Text);
		prop.SetValue(_E,text);
		EventPanel.UpdateStream();*/
		var text_edit=GetTree().Root.GetNode<Window>("Editor/EventPanel/TextEditor");
		text_edit.GetNode<Label>("Panel/TargetPath").Text=GetPath();
		text_edit.GetNode<Label>("Panel/PropName").Text=PropName;
		Type t = _event.GetType();
		PropertyInfo prop = t.GetProperty(PropName);
		var str=(string)prop.GetValue(_event);
		//GetNode<LineEdit>("Input").Text=str;
		
		text_edit.GetNode<TextEdit>("Panel/TextEdit").Text=str;
		
		if (_event is editor.GeneralTextEvent)
		{
			var _GTE=(editor.GeneralTextEvent)_event;
			var check=text_edit.GetNode<CheckBox>("Centered");
			if (prop == t.GetProperty("Text"))
			{
				
				check.Visible=true;
				check.ButtonPressed=_GTE.Centered;
			}else{
				check.Visible=false;
			}
		}
		text_edit.Popup();
		}
	private double _lastClickTime = 0;
	double DoubleClickThreshold = 0.3;

	void BtnDown(){//DoubleClick To Open EventGroup
	
		CinemaPreview.Apply();
	
		
		uid = (long)GetParent().GetNode<Marker2D>("uid").Position.X;
		
		editor.select_event(uid);
		GD.PrintErr("Event: ",_event);
		void edit()
		{
			if(_event is editor.GeneralTextEvent)
		{
			Edit("Text");
		}else if(_event is editor.PlotEvent)
		{
			Edit("Text");
		}else if(_event is editor.ElementEvent)
		{
			Edit("PlaceHolderInfo");
		}else
		{
			Edit("name");
		}

		}

		void DoubleClick(Action func){double currentTime = Time.GetUnixTimeFromSystem();
                double timeSinceLastClick = currentTime - _lastClickTime;

                if (timeSinceLastClick < DoubleClickThreshold)
                {
                    // 触发双击函数
                    func();
                    
                    // 成功触发后，重置时间防止“三连击”触发两次双击
                    _lastClickTime = 0;
                }
                else
                {
                    // 记录当前点击时间
                    _lastClickTime = currentTime;
                }}
				
		if (_event is editor.EventGroup)
		{
			DoubleClick(OpenGroup);
		}else 
		{
			DoubleClick(edit);
		}

	}
}
