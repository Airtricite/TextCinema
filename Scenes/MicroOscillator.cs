using Godot;
using System;

public partial class MicroOscillator : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		offset=new RandomNumberGenerator().RandiRange(0,2000);
	}
	double k=0.004f;
	float o=0.6f;//spd_mul
	int offset=0;
	float amp=0;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	double t=0;
	double _t=0;
	double start_time=Math.PI*4;
	public override void _PhysicsProcess(double delta)
	{
		var time=Time.GetTicksMsec();
		t+=delta;
		if (t<start_time)
		{
			amp= (float)(t / start_time);
		}else
		{
			amp=1;
		}
		_t+=delta*(0.2+(Math.Sin(time/5000)+1)*0.2);
		Rotation= (float)(Math.Sin(((long)time+offset)*o/1000.0)*k)*amp;
		Position= Vector2.Down*(float)Math.Sin(Math.PI*_t/3)*3*amp;
	}
}
