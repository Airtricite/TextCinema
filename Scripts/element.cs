using Godot;
using System;

public partial class element : Button
{
	// Called when the node enters the scene tree for the first time.
	long euid;
	public override void _Ready()
	{
		euid=(long)GetNode<Marker2D>("euid").Position.X;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
	void OnPressed()
	{
		editor.select_element(euid);
	}
}
