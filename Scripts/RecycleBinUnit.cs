using Godot;
using System;

public partial class RecycleBinUnit : Panel
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	void Delete(){editor.DeleteBinItem((long)GetNode<Marker2D>("id").Position.X,GetParent()==editor.event_bin_list);}
	void Restore(){editor.Restore((long)GetNode<Marker2D>("id").Position.X,GetParent()==editor.event_bin_list);}
}
