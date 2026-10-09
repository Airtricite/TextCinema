using Godot;
using System;

public partial class DragEdit : Button
{
	// Called when the node enters the scene tree for the first time.
	bool is_dragging=false;
	Vector2 lock_pos=Vector2.Zero;
	Vector2 prev_pos=Vector2.Zero;
	SpinBox X;
	SpinBox Y;
	public override void _Ready()
	{
		 X=GetNodeOrNull<SpinBox>("X");
		 Y=GetNodeOrNull<SpinBox>("Y");
	}
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion)
		{
			InputEventMouseMotion _e=(InputEventMouseMotion)@event;
			var pos=DisplayServer.MouseGetPosition();
			var dp=Vector2.Zero;
			if (is_dragging)
			{
			//	Input.MouseMode=Input.MouseModeEnum.Visible;
				
				if (pos.X+10>=GetWindow().Size.X || pos.X<=0 ||pos.Y>=GetWindow().Size.Y || pos.Y<=0 )
				{
					Input.WarpMouse(pos-(Vector2)(pos-GetWindow().Size/2)*1.8f);
					pos=DisplayServer.MouseGetPosition();
					prev_pos=DisplayServer.MouseGetPosition();
					dp=Vector2.Zero;
				}else
				{
					dp=pos-prev_pos;
				}

				if (X!=null)
				X.Value+=dp.X;
				if (Y!=null)
				Y.Value+=dp.Y;
				

				CinemaPreview.UpdateEdit();
			}else
			{
				//Input.MouseMode=Input.MouseModeEnum.Visible;
			}
			prev_pos=pos;
		}
    }
    void OnDrag()
	{
		lock_pos=DisplayServer.MouseGetPosition();
		is_dragging=true;

	}
	void StopDragging()
	{
		is_dragging=false;
		//Input.MouseMode=Input.MouseModeEnum.Visible;
	}
	void ValueChanged(float Value)
	{
		CinemaPreview.UpdateEdit();
	}
}
