using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PhysicsCenter : Window
{
	// Called when the node enters the scene tree for the first time
	public static Marker2D id_instance;
	public static Node2D origin;
	public static Node2D hitbox_prev;
	public static Panel circle_edit;
	public static Panel rect_edit;
	public static Panel capsule_edit;
	public static Panel polygon_edit;
	public static int hitbox_type=0;
	public static List<Vector2> polygon;
	public static long selected_pid=0;
	public static Node2D point;
	public override void _Ready()
	{
		
		//GD.Print(point);
	}
	static editor.PhysicsElementEvent _event;
	public static void edit()
	{
		
		var uid=(long)id_instance.Position.X;
		if (editor.EventStream.Count<=0)
		{
			return;
		}
		_event= (editor.PhysicsElementEvent)editor._select_event(uid);
		var element=_event.element;
		if (element==null)
		{
			return;
		}
		var node=element.Node;
		origin.GetNode("Preview").GetChildren().All(c=>{c.QueueFree();return true;});
		

		//load
		int type=0;
		_event.Hitbox.GetType();
		
		if (_event.Hitbox.Shape is CircleShape2D)
		{
			type=0;
			circle_edit.GetNode<SpinBox>("X").Value=_event.Hitbox.Position.X;
			circle_edit.GetNode<SpinBox>("Y").Value=_event.Hitbox.Position.Y;
			circle_edit.GetNode<SpinBox>("R").Value=((CircleShape2D)_event.Hitbox.Shape).Radius;
		}
		else if (_event.Hitbox.Shape is RectangleShape2D)
		{
			type=1;
			rect_edit.GetNode<SpinBox>("X").Value=_event.Hitbox.Position.X;
			rect_edit.GetNode<SpinBox>("Y").Value=_event.Hitbox.Position.Y;
			rect_edit.GetNode<SpinBox>("SizeX").Value=((RectangleShape2D)_event.Hitbox.Shape).Size.X;
			rect_edit.GetNode<SpinBox>("SizeY").Value=((RectangleShape2D)_event.Hitbox.Shape).Size.Y;
		}
		else if (_event.Hitbox.Shape is CapsuleShape2D)
		{
			type=2;
			capsule_edit.GetNode<SpinBox>("X").Value=_event.Hitbox.Position.X;
			capsule_edit.GetNode<SpinBox>("Y").Value=_event.Hitbox.Position.Y;
			capsule_edit.GetNode<SpinBox>("R").Value=((CapsuleShape2D)_event.Hitbox.Shape).Radius;
			capsule_edit.GetNode<SpinBox>("L").Value=((CapsuleShape2D)_event.Hitbox.Shape).Height-2*((CapsuleShape2D)_event.Hitbox.Shape).Radius;
			capsule_edit.GetNode<SpinBox>("Rotation").Value=_event.Hitbox.Rotation;
		}
		else if (_event.Hitbox.Shape is ConvexPolygonShape2D)
		{
			type=3;
			polygon_edit.GetNode<SpinBox>("X").Value=_event.Hitbox.Position.X;
			polygon_edit.GetNode<SpinBox>("Y").Value=_event.Hitbox.Position.Y;
			polygon_edit.GetNode<SpinBox>("Rotation").Value=_event.Hitbox.Rotation;
			polygon=((ConvexPolygonShape2D)_event.Hitbox.Shape).Points.ToList();
		}
		origin.GetNode<OptionButton>("../Type").Select(type);
		HitBoxType(type);
		_event.collision_layer.All(n=>
		{editor.root.GetNode("EventPanel/PhysicsCenter/Panel/Layer").GetNode<CheckBox>(n.ToString()).ButtonPressed=true;
		return true;});
		
		_event.collision_mask.All(n=>
		{editor.root.GetNode("EventPanel/PhysicsCenter/Panel/Mask").GetNode<CheckBox>(n.ToString()).ButtonPressed=true;
		return true;});
		var prev=(Node2D)node.Duplicate();
		origin.GetNode("Preview").AddChild(prev);
		prev.Position=Vector2.Zero;
	}
	

	static void Update()
	{
		hitbox_prev.QueueRedraw();
	}
	public static float scale=1;
	void ScaleView(float _scale)
	{
		origin.Scale=Vector2.One*_scale;
		scale=_scale;
	}
	static void HitBoxType(int type)
	{
		circle_edit	.Visible=false;
		rect_edit	.Visible=false;
		capsule_edit.Visible=false;
		polygon_edit.Visible=false;
		switch (type)
		{
			
			default:
			case 0://circle
			{
				circle_edit.Visible=true;
			}
			break;
			case 1://rect
			{rect_edit.Visible=true;

			}
			break;
			case 2://capsule
			{capsule_edit.Visible=true;

			}
			break;
			case 3://polygon
			{polygon_edit.Visible=true;

			}
			break;
		}
		hitbox_type=type;
		Update();
	}

	void AddPoint()
	{

		var p=(Node2D)point.Duplicate();
		var rng=new RandomNumberGenerator();
		var points=origin.GetNode("Points");
		
		p.Visible=true;

		selected_pid+=1;
		p.Name=(-1).ToString();

		var resort=points.GetChildren().Where(p=>long.Parse(p.Name)>=selected_pid).OrderByDescending(p=>long.Parse(p.Name)).ToHashSet();
		foreach (var _p in resort)
		{
			_p.Name=(long.Parse(_p.Name)+1).ToString();
			//break;
		}
		
		p.Name=selected_pid.ToString();
		points.AddChild(p);
		points.GetChildren().All(p=>{p.GetNode<Label>("id").Text=p.Name;return true;});
		
		p.Position=new Vector2(rng.RandiRange(-100,100),rng.RandiRange(-100,100));
		//Node prev=null;
		
		
		
		Update();
	}
	void RemovePoint()
	{
		var points=origin.GetNode("Points");
		
		var del=points.GetChildren().OrderBy(p=>int.Parse(p.Name)).ElementAt((int)selected_pid-1);
		del.Free();

		var i=1;
		var resort=points.GetChildren().OrderBy(p=>int.Parse(p.Name)).ToArray();
		
		foreach (var _p in resort)
		{
			_p.Name=i.ToString();i++;
			//_p.Name=(long.Parse(_p.Name)+1).ToString();
			//break;
		}
	
	
		points.GetChildren().All(p=>{p.GetNode<Label>("id").Text=p.Name;return true;});
	}
	void Apply()
	{
		var uid=(long)id_instance.Position.X;
		if (editor.EventStream.Count<=0)
		{
			return;
		}
		_event= (editor.PhysicsElementEvent)editor._select_event(uid);
		var element=_event.element;
		if (element==null)
		{
			return;
		}
		var cshape=new CollisionShape2D();
		switch (hitbox_type)
		{
			
			default:
			case 0://circle
			{
				var shape=new CircleShape2D();
				shape.Radius=(float)circle_edit.GetNode<SpinBox>("R").Value;
				cshape.Shape=shape;
				cshape.Position=new Vector2((float)circle_edit.GetNode<SpinBox>("X").Value, (float)circle_edit.GetNode<SpinBox>("Y").Value);

			}
			break;
			case 1://rect
			{
				var shape=new RectangleShape2D();
				shape.Size=new Vector2((float)rect_edit.GetNode<SpinBox>("SizeX").Value,(float)rect_edit.GetNode<SpinBox>("SizeY").Value);
				cshape.Shape=shape;
				cshape.Position=new Vector2((float)rect_edit.GetNode<SpinBox>("X").Value, (float)rect_edit.GetNode<SpinBox>("Y").Value);

			}
			break;
			case 2://capsule
			{
				var shape=new CapsuleShape2D();
				shape.Radius=(float)capsule_edit.GetNode<SpinBox>("R").Value;
				shape.Height=(float)capsule_edit.GetNode<SpinBox>("L").Value+2*(float)capsule_edit.GetNode<SpinBox>("R").Value;
				cshape.Shape=shape;
				cshape.Position=new Vector2((float)capsule_edit.GetNode<SpinBox>("X").Value, (float)capsule_edit.GetNode<SpinBox>("Y").Value);
				cshape.Rotation=(float)capsule_edit.GetNode<SpinBox>("Rotation").Value;
			}
			break;
			case 3://polygon
			{
				var shape=new ConvexPolygonShape2D();
				shape.Points=polygon.ToArray();
				cshape.Shape=shape;
				cshape.Position=new Vector2((float)polygon_edit.GetNode<SpinBox>("X").Value, (float)polygon_edit.GetNode<SpinBox>("Y").Value);
				cshape.Rotation=(float)polygon_edit.GetNode<SpinBox>("Rotation").Value;
			}
			break;
		}
		_event.Hitbox=cshape;
		_event.collision_layer=GetTree().Root.GetNode("Editor/EventPanel/PhysicsCenter/Panel/Layer").GetChildren()
		.Where(n=>((CheckBox)n).ButtonPressed).Select(n=>int.Parse(n.Name)).ToHashSet();
		_event.collision_mask=GetTree().Root.GetNode("Editor/EventPanel/PhysicsCenter/Panel/Mask").GetChildren()
		.Where(n=>((CheckBox)n).ButtonPressed).Select(n=>int.Parse(n.Name)).ToHashSet();
		Close();
	}
	void Close(){Visible=false;
	if (polygon!=null)
	{
		polygon.Clear();
	}
	
	}
	//on type selected
}
