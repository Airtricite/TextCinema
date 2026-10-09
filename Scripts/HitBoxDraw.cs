using Godot;
using System;
using System.Linq;
using static PhysicsCenter;
public partial class HitBoxDraw : Node2D
{
	// Called when the node enters the scene tree for the first time.
    public override void _PhysicsProcess(double delta)
    {
       QueueRedraw();
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    void DrawCapsule(Vector2 pos,float radius,float height,float rot,Color color)//a convertion is needed
	{
	Vector2 upper_circle_position  = pos + new Vector2(0, height * 0.5f);
	DrawCircle(upper_circle_position, radius, color);

	Vector2 lower_circle_position = pos - new Vector2(0, height * 0.5f);
	DrawCircle(lower_circle_position, radius, color);

	Vector2 rect_position = pos - new Vector2(radius, height * 0.5f);
	Rect2 rect = new Rect2(rect_position, new Vector2(radius * 2, height));
	DrawRect(rect, color);
	Rotation=rot;
	}
    public override void _Draw()
    {
		var color=new Color(0.5f,0.5f,1,0.5f);
	   switch (hitbox_type)
		{
			
			default:
			case 0://circle
				Position=new Vector2((float)circle_edit.GetNode<SpinBox>("X").Value, (float)circle_edit.GetNode<SpinBox>("Y").Value);
				DrawCircle(
					Vector2.Zero,
					(float)circle_edit.GetNode<SpinBox>("R").Value,color
					);
			
			break;
			case 1://rect
				Position=new Vector2((float)rect_edit.GetNode<SpinBox>("X").Value,(float)rect_edit.GetNode<SpinBox>("Y").Value);
				Rotation=0;
				DrawRect(
					new Rect2(
						Vector2.Zero,
						new Vector2((float)rect_edit.GetNode<SpinBox>("SizeX").Value,(float)rect_edit.GetNode<SpinBox>("SizeY").Value)),
						color
				);
			
			break;
			case 2://capsule
			Position=new Vector2((float)capsule_edit.GetNode<SpinBox>("X").Value,(float)capsule_edit.GetNode<SpinBox>("Y").Value);
				DrawCapsule(
					Vector2.Zero,
					(float)capsule_edit.GetNode<SpinBox>("R").Value,(float)capsule_edit.GetNode<SpinBox>("L").Value,
					(float)capsule_edit.GetNode<SpinBox>("Rotation").Value,color
				);
			
			break;
			case 3://polygon
			Rotation=(float)polygon_edit.GetNode<SpinBox>("Rotation").Value;
			GetNode<Node2D>("../Points").Rotation=Rotation;
			Position=new Vector2((float)polygon_edit.GetNode<SpinBox>("X").Value,(float)polygon_edit.GetNode<SpinBox>("Y").Value);
			GetNode<Node2D>("../Points").Position=Position;
				var points=origin.GetNode("Points");
				if (points.GetChildren().Count<=0)
				{
					break;
				}
				polygon=points.GetChildren().OrderBy(p=>long.Parse(p.Name)).Select(p=>(Node2D)p).Select(p=>p.Position).ToList();
				if (polygon.Count()<3)
				{
					break;
				}
				try
				{
					DrawColoredPolygon(polygon.ToArray(),color);
				}
				catch (System.Exception)
				{
					throw;
					
				}
				
			
			break;
		}
    }
	
}
