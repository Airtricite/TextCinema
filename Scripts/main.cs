using Godot;
using System;

public partial class main : Control
{

	public static bool MovieMakerMode=false;
	string ProjectPath="";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
	void Edit()
	{
		GetTree().ChangeSceneToFile("res://Scenes/editor.tscn");
	}
	void Play()
	{
		GetNode<FileDialog>("PlayMenu").Popup();
	}
	void FileChosen(string path)
	{
		cinema.tmov_path=path;
		GetTree().ChangeSceneToFile("res://Scenes/cinema.tscn");
	}
}
