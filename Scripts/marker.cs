using Godot;
using System;

public partial class marker : Control
{
	// Called when the node enters the scene tree for the first time.
	Node2D node;
	public override void _Ready()
	{
		node=GetParent<Node2D>();
	}
	bool is_dragging=false;
	bool is_entered=false;
	Vector2 pos;
	Vector2 offset=new Vector2(0,-5);
    public override void _Input(InputEvent @event)
    {
		
			
		
		if (@event is InputEventMouseButton)
		{if (is_entered)
		{

			InputEventMouseButton _e=(InputEventMouseButton)@event;
			if (_e.Pressed&&_e.ButtonIndex==MouseButton.Left)
			{
				is_dragging=true;
			}
			else if(!_e.Pressed&&_e.ButtonIndex==MouseButton.Left)
			{
				is_dragging=false;
			}
			if (_e.ButtonIndex==MouseButton.WheelUp)
			{
				node.GlobalRotationDegrees+=5;
			}else if(_e.ButtonIndex==MouseButton.WheelDown)
			{
				node.GlobalRotationDegrees+=-5;
			}

		}
		}
         if (@event is InputEventMouseMotion)
		{
			InputEventMouseMotion _e=(InputEventMouseMotion)@event;
			 pos=_e.GlobalPosition;
			if (is_dragging)
			{
			//	Input.MouseMode=Input.MouseModeEnum.Visible;
				node.GlobalPosition=pos+offset+editor.camera.GlobalPosition-GetWindow().Size/2;
			}else
			{
				//Input.MouseMode=Input.MouseModeEnum.Visible;
			}
			
		}

	}
	void entered(){is_entered=true;}
	void exited(){is_entered=false;}
}
