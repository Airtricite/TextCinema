using Godot;
using System;

public partial class ProgressNode : Node2D
{
	int behavior=0;
	int _t=0;
	Control prog;
	Marker2D val;
	Control fill;
	// Called when the node enters the scene tree for the first time.
	Label name_label;
	Label prog_label;
	public override void _Ready()
	{
		GD.PrintErr("SetScript");
		_t= (int)GetMeta("Type");
		switch (_t)
		{
			default:
			case 0://Bar
				prog = GetNode<Control>("Bar");
				//_prog = e.element.Node.GetNode<Control>("Bar");
				break;
			case 1://ring
				prog = GetNode<Control>("Ring");
				//_prog = e.element.Node.GetNode<Control>("Ring");
				break;
			case 2://wave
				prog = GetNode<Control>("Wave");
				//_prog = e.element.Node.GetNode<Control>("Wave");
				break;
		}
		behavior=(int)prog.GetMeta("Behavior");
		val=GetNode<Marker2D>("Value");
		 name_label = GetNode<Label>("Name");
		 prog_label = GetNode<Label>("Progress");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		var progress=val.Position.X;
		//GD.PrintErr(progress);
		switch (_t)
		{
			default:
			case 0://Event.element_props["border_color"];
			{	fill=prog.GetNode<Control>("Fill");
				switch (behavior)
				{
					case 0:
					default:
					fill.Position=Vector2.Zero;
					fill.Size=new Vector2((float)(prog.Size.X*progress*0.01), prog.Size.Y);
					break;
					case 1:
					fill.Position=Vector2.Zero;
					fill.Size=new Vector2((float)(prog.Size.X*(1-progress*0.01)), prog.Size.Y);
					break;
					case 2:
					fill.Position=Vector2.Right*prog.Size.X*(float)(1-progress*0.01)/2;
					fill.Size=new Vector2((float)(prog.Size.X*progress*0.01), prog.Size.Y);
					break;
					case 3:
					fill.Position=Vector2.Right*prog.Size.X*(float)(progress*0.01)/2;
					fill.Size=new Vector2((float)(prog.Size.X*(1-progress*0.01)), prog.Size.Y);
					break;
				}
			}
			break;
			case 1:
			{
				var shader=(ShaderMaterial)prog.Material;
				shader.SetShaderParameter("value",progress);
				switch (behavior)
				{
					case 0:
					default:shader.SetShaderParameter("progress_rotation",0);
					break;
					case 1:
					shader.SetShaderParameter("progress_rotation",-progress*0.01);
					break;

				}
			}
			break;
			case 2:
			{
				var shader=(ShaderMaterial)prog.Material;
				shader.SetShaderParameter("fill_value",(progress-50)*2);
			}
			break;
		}
		
		
					prog_label.Text = (((double)(long)progress)*100 / 100).ToString() + "%";
	}
}
