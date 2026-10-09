using Godot;
using System;

public partial class LFOEditor : Window
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
	void Edit()
	{
		//popup
		Popup();
	}
	void Update()
	{
		//update oscilliscope frame
		//update modifier list
		//update modifier parameter{name,default value,range,step}
	}
	void Apply()
	{
		//if lfo has parent then Edit(parent) else close the window
	}
	void WaveTypeSelected(int type)
	{
		Update();
	}
	void AddModifier(int index)
	{
		Update();
	}
	
}
