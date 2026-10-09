using Godot;
using System;

public partial class MoveablePoint : Control
{
Node2D node;
	public override void _Ready()
	{
		node=GetParent<Node2D>();
	}
	bool is_dragging=false;
	bool is_entered=false;
	Vector2 pos;
    public override void _Input(InputEvent @event)
    {
		
			
		
		if (@event is InputEventMouseButton)
		{if (is_entered)
		{

			InputEventMouseButton _e=(InputEventMouseButton)@event;
			if (_e.Pressed&&_e.ButtonIndex==MouseButton.Left)
			{
				is_dragging=true;
				PhysicsCenter.selected_pid=long.Parse(GetParent().Name);
				GD.Print("selected_pid:"+PhysicsCenter.selected_pid);
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
			 pos=_e.Position;
			if (is_dragging)
			{
			//	Input.MouseMode=Input.MouseModeEnum.Visible;
				node.GlobalPosition=pos;//*PhysicsCenter.scale;
			}else
			{
				//Input.MouseMode=Input.MouseModeEnum.Visible;
			}
			
		}

	}
	void entered(){is_entered=true;}
	void exited(){is_entered=false;}
}
