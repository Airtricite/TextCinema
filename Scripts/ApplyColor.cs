using Godot;
using System;

public partial class ApplyColor : Node
{
	// Called when the node enters the scene tree for the first time.
	static ColorRect color;
	public override void _Ready()
	{
		color=GetParent<ColorRect>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	 void Apply()
	{
		ElementExplorer.ApplyColor(GetParent<ColorRect>());
	
	}
}
