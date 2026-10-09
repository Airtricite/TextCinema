using Godot;
using System;
using System.Linq;
using static ElementExplorer;
using static ControlWindow;

public partial class CinemaPreview : Panel
{
	// Called when the node enters the scene tree for the first time.
	public static Marker2D id_instance;
	public static Node2D common_case;
	public static Node2D line_case;
	public static Node2D cam_case;
	public override void _Ready()
	{
		id_instance = GetParent().GetNode<Marker2D>("PropertyPanel/current_uid");
		common_case = GetNode<Node2D>("CommonCase");
		line_case = GetNode<Node2D>("Origin/Line/LineCase");
		cam_case = GetNode<Node2D>("Origin/Cam/CamCase");

		PosEdit = GetNode<Panel>("../TransformEditor/Scr/List/Pos");
		RotEdit = GetNode<Panel>("../TransformEditor/Scr/List/Rot");
		ScaleEdit = GetNode<Panel>("../TransformEditor/Scr/List/Scale");
		OffsetEdit = GetNode<Panel>("../TransformEditor/Scr/List/Offset");
		SizeEdit = GetNode<Panel>("../TransformEditor/Scr/List/Size");
		GravityEdit = GetNode<Panel>("../TransformEditor/Scr/List/Gravity");

		PhysicBodyEdit = GetNode<Panel>("../TransformEditor/Scr/List/PhysicBody");
		GUIEdit = GetNode<Panel>("../TransformEditor/Scr/List/GUI");
		cursor= GetNode<Node2D>("../TransformEditor/Indicator/Cursor");
		Camera = cam_case = GetNode<Node2D>("Origin/Cam");
		Line = GetNode<Node2D>("Origin/Line");
		Plot = GetNode<Control>("Origin/Line/Plot");
		General = GetNode<Control>("Origin/Line/General");
		Origin = GetNode<Node2D>("Origin").GlobalPosition;
		PreviewScale = Size / new Vector2(1920, 1080);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
	static Vector2 Origin;
	static Vector2 PreviewScale;
	static Panel PosEdit;
	static Panel RotEdit;
	static Panel ScaleEdit;
	static Panel SizeEdit;
	static Panel OffsetEdit;
	static Panel GravityEdit;
	static Panel PhysicBodyEdit;
	static Panel GUIEdit;
	static Node node;//element preview
	static editor.Element element;
	static editor.Event Event;

	static Node2D Camera;
	static Node2D cursor;
	static Node2D Line;
	static Control Plot;
	static Control General;

	public static void UpdateEdit()
	{
		if (element != null)
		{
			var prev = (Node2D)node;
			prev.GlobalPosition = Origin + new Vector2((float)PosEdit.GetNode<SpinBox>("Drag/X").Value, (float)PosEdit.GetNode<SpinBox>("Drag/Y").Value) * PreviewScale;
			prev.GlobalRotationDegrees = (float)RotEdit.GetNode<SpinBox>("Drag/X").Value;
			prev.Scale = PreviewScale * new Vector2((float)ScaleEdit.GetNode<SpinBox>("Drag/X").Value, (float)ScaleEdit.GetNode<SpinBox>("Drag/Y").Value) * 0.01f;
			switch (element.type)
			{

				default://pos, rot
				case ElementType.Image:
				case ElementType.Video:
				case ElementType.Text:
					{
						var ctrl = prev.GetChild<Control>(0);
						ctrl.Size = new Vector2((float)SizeEdit.GetNode<SpinBox>("Drag/X").Value, (float)SizeEdit.GetNode<SpinBox>("Drag/Y").Value);
						ctrl.Position = new Vector2((float)OffsetEdit.GetNode<SpinBox>("Drag/X").Value-ctrl.Size.X/2, (float)OffsetEdit.GetNode<SpinBox>("Drag/Y").Value-ctrl.Size.Y/2);
						if (Event is editor.ElementEvent)
						{
							ctrl.GetNode<ColorRect>("ColorRect").Color=((editor.ElementEvent)Event).BackgroundColor;
						}
					}
					break;
				case ElementType.Progress:
					//offset, size, scale
					{
						Control prog;
						switch ((int)prev.GetMeta("Type"))
						{
							default:
							case 0://Bar
								prog = prev.GetNode<Control>("Bar");
								var fill = prog.GetNode<Control>("Fill");
								fill.Size = new Vector2(fill.Size.X, prog.Size.Y);
								break;
							case 1://ring
								prog = prev.GetNode<Control>("Ring");
								break;
							case 2://wave
								prog = prev.GetNode<Control>("Wave");
								break;
						}
						var ctrl = prog;
						ctrl.Position = new Vector2((float)OffsetEdit.GetNode<SpinBox>("Drag/X").Value, (float)OffsetEdit.GetNode<SpinBox>("Drag/Y").Value);
						ctrl.Size = new Vector2((float)SizeEdit.GetNode<SpinBox>("Drag/X").Value, (float)SizeEdit.GetNode<SpinBox>("Drag/Y").Value);

					}
					break;
				case ElementType.Animation:
				case ElementType.Custom:
					//scale
					break;
			}
			if (Event is editor.PhysicsElementEvent)
		{
		 
			var camera_fix=Vector2.Zero;
			
			cursor.GlobalPosition=Origin+PreviewScale*new Vector2
			(
				(float)PhysicBodyEdit.GetNode<SpinBox>("SpawnPosition/X").Value,(float)PhysicBodyEdit.GetNode<SpinBox>("SpawnPosition/Y").Value
			);
			prev.GlobalPosition +=cursor.Position;
		}
		}
		//Event = editor.EventStream.Where(e => e.uid == (long)id_instance.Position.X).First();
		//GD.Print(Event.GetType());
		
		if (Event is editor.GUIEvent)
		{
			//GD.Print(111);
			Line.Position = PreviewScale * new Vector2
			(
				(float)GUIEdit.GetNode<SpinBox>("DragPosL/X").Value, (float)GUIEdit.GetNode<SpinBox>("DragPosL/Y").Value
			);
			Line.RotationDegrees = (float)GUIEdit.GetNode<SpinBox>("DragRotL/X").Value;
			//===================================================================================
			General.Position = PreviewScale * new Vector2
			(
				(float)GUIEdit.GetNode<SpinBox>("DragPosG/X").Value, (float)GUIEdit.GetNode<SpinBox>("DragPosG/Y").Value
			);
			General.Size = PreviewScale * new Vector2
			(
				(float)GUIEdit.GetNode<SpinBox>("DragSizeG/X").Value, (float)GUIEdit.GetNode<SpinBox>("DragSizeG/Y").Value
			);
			General.PivotOffset = PreviewScale * new Vector2
			(
				(float)GUIEdit.GetNode<SpinBox>("DragPivotG/X").Value, (float)GUIEdit.GetNode<SpinBox>("DragPivotG/Y").Value
			);
			General.RotationDegrees = (float)GUIEdit.GetNode<SpinBox>("DragRotG/X").Value;
			//===================================================================================
			Plot.Position = PreviewScale * new Vector2
			(
				(float)GUIEdit.GetNode<SpinBox>("DragPosP/X").Value, (float)GUIEdit.GetNode<SpinBox>("DragPosP/Y").Value
			);
			Plot.Size = PreviewScale * new Vector2
			(
				(float)GUIEdit.GetNode<SpinBox>("DragSizeP/X").Value, (float)GUIEdit.GetNode<SpinBox>("DragSizeP/Y").Value
			);
			Plot.PivotOffset = PreviewScale * new Vector2
			(
				(float)GUIEdit.GetNode<SpinBox>("DragPivotP/X").Value, (float)GUIEdit.GetNode<SpinBox>("DragPivotP/Y").Value
			);
			Plot.RotationDegrees = (float)GUIEdit.GetNode<SpinBox>("DragRotP/X").Value;
			//===================================================================================
			Camera.Position = PreviewScale * new Vector2
			(
				(float)GUIEdit.GetNode<SpinBox>("DragPosC/X").Value, (float)GUIEdit.GetNode<SpinBox>("DragPosC/Y").Value
			);
			Camera.Scale = Vector2.One / (new Vector2
			(
				(float)GUIEdit.GetNode<SpinBox>("DragZoomC/X").Value, (float)GUIEdit.GetNode<SpinBox>("DragZoomC/Y").Value
			));
			Camera.RotationDegrees = (float)GUIEdit.GetNode<SpinBox>("DragRotC/X").Value;
			//===================================================================================			
		}
	}
	public static void Apply()
	{
		GD.Print(editing);
		ApplyTransform(true);
	}
	void Cancel()
	{
		ApplyTransform(false);
	}
	
	public static void ApplyTransform(bool write)//write
	{
		if ((!editing)||(editor.EventStream.Count <= 0))
		{
			
		}else{
		 var _uid= (int)id_instance.Position.X;
		 Event=EventPanel.FindEvent(_uid);

			if (Event is editor.ElementEvent)
			{
				if (node == null)
				{
					return;
				}
				if (editor.Elements.Count <= 0)
				{
					return;
				}
				
				if (write==false)
				{
					node.QueueFree();
					node = null;

				}
				else
				{
					var e = (editor.ElementEvent)Event;
					var n = (Node2D)node;
					//GD.PrintErr(e,e.element_props,n);
					var px=(float)PosEdit.GetNode<SpinBox>("Drag/X").Value;
						var py=(float)PosEdit.GetNode<SpinBox>("Drag/Y").Value;
						//GD.PrintErr("pos:"+new Vector2(px,py));
					e.element_props["pos"] = new Vector2(px,py);//-Origin
					//GD.PrintErr("prop:"+e.element_props["pos"]);
					e.element_props["rot"] = n.Rotation;
					e.element_props["scale"] = n.Scale / PreviewScale;
					e.element.Node.Position = n.Position / PreviewScale;
					e.element.Node.Rotation = n.Rotation;
					e.element.Node.Scale = n.Scale / PreviewScale;
					if (Event is editor.PhysicsElementEvent)
					{
						editor.PhysicsElementEvent _e=(editor.PhysicsElementEvent)Event;
						PhysicBodyEdit.Visible=true;
						var x=(float)PhysicBodyEdit.GetNode<SpinBox>("SpawnPosition/X").Value;
						var y=(float)PhysicBodyEdit.GetNode<SpinBox>("SpawnPosition/Y").Value;
						var vx=(float)PhysicBodyEdit.GetNode<SpinBox>("SpawnVelocity/X").Value;
						var vy=(float)PhysicBodyEdit.GetNode<SpinBox>("SpawnVelocity/Y").Value;
						 _e.SpawnPos=new Vector2(x, y);
						 _e.SpawnVelocity=new Vector2(vx, vy);
						  _e.SpawnAngularVelocity= (float)PhysicBodyEdit.GetNode<SpinBox>("SpawnAngularVelocity/Y").Value;
				//	e.element_props["pos"] =(Vector2)e.element_props["pos"] - _e.SpawnPos/PreviewScale;//-Origin
						 
					}
					// offset/size/progress/namepos/progress hint pos...
					switch (element.type)
					{

						default://pos, rot
						case ElementType.Image:
						case ElementType.Video:
						case ElementType.Text:
							{
								var pos=n.GetChild<Control>(0).Position;
								e.element_props["offset"] = pos+n.GetChild<Control>(0).Size/2;
								e.element_props["size"] = n.GetChild<Control>(0).Size;
								e.element.Node.GetChild<Control>(0).Position = pos;
								e.element.Node.GetChild<Control>(0).Size = n.GetChild<Control>(0).Size;
							}
							break;
						case ElementType.Progress:
							//offset, size, scale
							{

								Control prog;
								Control _prog;
								switch ((int)n.GetMeta("Type"))
								{
									default:
									case 0://Bar
										prog = n.GetNode<Control>("Bar");
										_prog = e.element.Node.GetNode<Control>("Bar");
										break;
									case 1://ring
										prog = n.GetNode<Control>("Ring");
										_prog = e.element.Node.GetNode<Control>("Ring");
										break;
									case 2://wave
										prog = n.GetNode<Control>("Wave");
										_prog = e.element.Node.GetNode<Control>("Wave");
										break;
								}
								e.element_props["offset"] = prog.Position;
								e.element_props["size"] = prog.Size;
								_prog.Position = prog.Position;
								_prog.Size = prog.Size;
							}
							break;
						case ElementType.Animation:
						case ElementType.Custom:
							//scale
							break;
					}
					
				}
			}
			else if (Event is editor.GUIEvent)
			{
				if (write)
				{
				var e = (editor.GUIEvent)Event;
				GD.Print(e.uid,"|",e.BaseRotation);
				e.BasePosition = Line.Position / PreviewScale;
				e.BaseRotation = Line.RotationDegrees;
				e.GeneralPos = General.Position / PreviewScale;
				e.GeneralRot = General.RotationDegrees;
				e.GeneralSize = General.Size / PreviewScale;
				e.GeneralPivotOffset = General.PivotOffset / PreviewScale;
				e.PlotPos = Plot.Position / PreviewScale;
				e.PlotRot = Plot.RotationDegrees;
				e.PlotSize = Plot.Size / PreviewScale;
				e.PlotPivotOffset = Plot.PivotOffset / PreviewScale;
				e.CameraPosition = Camera.Position / PreviewScale;
				e.CameraRotation = Camera.RotationDegrees;
				e.CameraZoom = Vector2.One / Camera.Scale;
				}
			}
			else if (Event is editor.PhysicsEvent)
			{
				if (write)
				{
				var e = (editor.PhysicsEvent)Event;
				e.GravityDirection = (float)GravityEdit.GetNode<SpinBox>("DragDir/X").Value;
				e.Magnitude = (float)GravityEdit.GetNode<SpinBox>("DragMag/Y").Value;
				}
			}
		}
		element = null;
		Event = null;

		PosEdit.Visible = false;
		RotEdit.Visible = false;
		ScaleEdit.Visible = false;
		SizeEdit.Visible = false;
		OffsetEdit.Visible = false;
		GUIEdit.Visible = false;
		GravityEdit.Visible = false;
		PhysicBodyEdit.Visible=false;
		cursor.Visible=false;
		editing=false;
		if (node!=null)
		{
			node.QueueFree();
		}
		
		node = null;
	}
	 public void AddGoldenMarker(Control target)
    {
        if (target == null) return;


        // 金色定义
        Color gold = new Color(1.0f, 0.84f, 0.0f);
        float thickness = 3.0f;

        // 1. 创建 ReferenceRect 边框
        ReferenceRect rect = new ReferenceRect();
        rect.Name = "GoldenOutline";
        rect.BorderColor = gold;
        rect.BorderWidth = thickness+1;
        rect.EditorOnly = false; // 【关键】必须设为 false，否则运行时不可见
        rect.MouseFilter = Control.MouseFilterEnum.Ignore; // 不阻挡点击
        
        // 布局：覆盖全父节点
        target.AddChild(rect);
        rect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

        // 2. 创建十字架容器（用于定位）
        Control crossContainer = new Control();
        crossContainer.Name = "Crosshair";
        crossContainer.MouseFilter = Control.MouseFilterEnum.Ignore;
        
        // 布局：居中并保持在中心
        rect.AddChild(crossContainer);
        crossContainer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
		void cross(Color color,int width){
        // 3. 创建十字架 - 水平线
        ColorRect hLine = new ColorRect();
        hLine.Color = gold;
        hLine.MouseFilter = Control.MouseFilterEnum.Ignore;
        // 设置长 20px，高 2px (即厚度)
        hLine.Size = new Vector2(20, width); 
        // 居中对齐：偏移量为自身大小的一半
        hLine.Position = new Vector2(-hLine.Size.X / 2, -hLine.Size.Y / 2);
        crossContainer.AddChild(hLine);

        // 4. 创建十字架 - 垂直线
        ColorRect vLine = new ColorRect();
        vLine.Color = color;
        vLine.MouseFilter = Control.MouseFilterEnum.Ignore;
        // 设置宽 2px，高 20px
        vLine.Size = new Vector2(width, 20);
        // 居中对齐：偏移量为自身大小的一半
        vLine.Position = new Vector2(-vLine.Size.X / 2, -vLine.Size.Y / 2);
        crossContainer.AddChild(vLine);
		}
		cross(Color.Color8(0,0,0),8);
		cross(gold,3);

    }
	static bool editing=false;
	void EditTransform()
	{
	
		if (editing)
		{
			return;
		}
		editing=true;
		ApplyTransform(false);
		editor.camera.Position = cam_case.GlobalPosition;
		PosEdit.Visible = false;
		RotEdit.Visible = false;
		ScaleEdit.Visible = false;
		SizeEdit.Visible = false;
		OffsetEdit.Visible = false;
		GUIEdit.Visible = false;
		GravityEdit.Visible = false;
		PhysicBodyEdit.Visible=false;
		cursor.Visible=false;
		//get event//get element
		//if event is a element event
		var uid = (int)id_instance.Position.X;

		if (editor.EventStream.Count <= 0)
		{
			return;
		}
		 Event =  EventPanel.FindEvent(uid);;
		 GD.Print(uid);
		if (Event is editor.ElementEvent)
		{
			PosEdit.Visible = true;
			RotEdit.Visible = true;
			ScaleEdit.Visible = true;
			if (Event is editor.PhysicsElementEvent)
			{
				cursor.Visible=true;
				editor.PhysicsElementEvent e=(editor.PhysicsElementEvent)Event;
				PhysicBodyEdit.Visible=true;
				PhysicBodyEdit.GetNode<SpinBox>("SpawnPosition/X").Value = e.SpawnPos.X;//
				PhysicBodyEdit.GetNode<SpinBox>("SpawnPosition/Y").Value = e.SpawnPos.Y;//
				PhysicBodyEdit.GetNode<SpinBox>("SpawnVelocity/X").Value = e.SpawnVelocity.X;
				PhysicBodyEdit.GetNode<SpinBox>("SpawnVelocity/Y").Value = e.SpawnVelocity.Y;
				PhysicBodyEdit.GetNode<SpinBox>("SpawnAngularVelocity/Y").Value = e.SpawnAngularVelocity;
			}
			editor.ElementEvent _e = (editor.ElementEvent)Event;

			if (editor.Elements.Count <= 0) return;
			if (_e.element == null) return;

			var euid = _e.element.euid;
			var Element = editor.Elements.Where(e => e.euid == euid).First();
			if (Element == null)
			{
				return;
			}
			element = Element;
			var prev = (Node2D)Element.Node.Duplicate();
			//load texture
		switch (Element.type)
		{
			
			case ElementType.Image:{
				var Rect=prev.GetNode<TextureRect>("Image");
			if (Element.FrameData.Count>0)
					{
						var tex = new PortableCompressedTexture2D();
						tex._Data = (byte[])Element.FrameData.ElementAt(0).Clone();
						
						tex.KeepCompressedBuffer = false;
						
						Rect.Texture=tex;
						/*var full=prev.GetNode<Control>("Image/Full");
						if (full!=null)
						{
							
						}else
						{
							Control c=new Control();
							c.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
							c.Name="Full";
							Rect.AddChild(c);
							AddGoldenMarker(c);
						}*/
						
						
					}
					AddGoldenMarker(Rect);
			break;}
			case ElementType.Animation:{
			((AnimatedSprite2D)prev).SpriteFrames.Clear("default");
			foreach (var item in Element.FrameData)
			{
				var tex=new PortableCompressedTexture2D();
				tex._Data= (byte[])item.Clone();
				GD.Print(tex);
				tex.KeepCompressedBuffer=true;
				
				((AnimatedSprite2D)prev).SpriteFrames.AddFrame("default",tex);
				
			}((AnimatedSprite2D)prev).Play();}
			break;
		}
			node = prev;

			switch (_e.belongs_to)
			{

				default:
				case editor.Where.General:
					common_case.AddChild(prev);
					break;
				case editor.Where.BaseLine:
					line_case.AddChild(prev);
					break;
				case editor.Where.Camera:
					cam_case.AddChild(prev);
					break;
			}
			//
			//load position: 
			//GD.PrintErr("pos:"+_e.element_props["pos"]);
			var pos=VarToData(_e.element_props["pos"]);
			//GD.PrintErr("v2d:"+pos);
			prev.Position = pos * PreviewScale;
			prev.Rotation = VarToData(_e.element_props["rot"]);
			prev.Scale = VarToData(_e.element_props["scale"]) * PreviewScale;
			GD.Print("[1]",prev.Position);
			//load globalposition: 
			//var gpos = (prev.GlobalPosition - Origin) / PreviewScale;
			var scale = prev.Scale / PreviewScale;
			PosEdit.GetNode<SpinBox>("Drag/X").Value = pos.X;
			PosEdit.GetNode<SpinBox>("Drag/Y").Value = pos.Y;
			RotEdit.GetNode<SpinBox>("Drag/X").Value = Mathf.RadToDeg(prev.GlobalRotation);
			ScaleEdit.GetNode<SpinBox>("Drag/X").Value = scale.X * 100;
			ScaleEdit.GetNode<SpinBox>("Drag/Y").Value = scale.Y * 100;

		
			//Detect element type
			switch (Element.type)
			{

				default://pos, rot
				case ElementType.Image:
				case ElementType.Video:
				case ElementType.Text:

					//offset, size, scale
					{
						SizeEdit.Visible = true;
						OffsetEdit.Visible = true;
						var size = VarToData(_e.element_props["size"]);
						var off = VarToData(_e.element_props["offset"]);
						SizeEdit.GetNode<SpinBox>("Drag/X").Value = size.X;
						SizeEdit.GetNode<SpinBox>("Drag/Y").Value = size.Y;
						OffsetEdit.GetNode<SpinBox>("Drag/X").Value = off.X;
						OffsetEdit.GetNode<SpinBox>("Drag/Y").Value = off.Y;
					}
					break;
				case ElementType.Progress:
					{
						Control prog;
						switch ((int)prev.GetMeta("Type"))
						{
							default:
							case 0://Bar
								prog = prev.GetNode<Control>("Bar");
								break;
							case 1://ring
								prog = prev.GetNode<Control>("Ring");
								break;
							case 2://wave
								prog = prev.GetNode<Control>("Wave");
								break;
						}
						SizeEdit.Visible = true;
						OffsetEdit.Visible = true;
						var size = VarToData(_e.element_props["size"]);
						var off = VarToData(_e.element_props["offset"]);
						SizeEdit.GetNode<SpinBox>("Drag/X").Value = size.X;
						SizeEdit.GetNode<SpinBox>("Drag/Y").Value = size.Y;
						OffsetEdit.GetNode<SpinBox>("Drag/X").Value = off.X;
						OffsetEdit.GetNode<SpinBox>("Drag/Y").Value = off.Y;
					}
					break;
				case ElementType.Animation:
				case ElementType.Custom:
					//scale
					break;
			}

		}
		else if (Event is editor.GUIEvent)
		{
            static float purify(float v)
			{
				return Mathf.Round(v*100)/100;
			}
			editor.GUIEvent E = (editor.GUIEvent)Event;
			
			GUIEdit.Visible = true;
			GUIEdit.GetNode<SpinBox>("DragPosL/X").Value = purify(E.BasePosition.X);
			GUIEdit.GetNode<SpinBox>("DragPosL/Y").Value = purify(E.BasePosition.Y);
			GUIEdit.GetNode<SpinBox>("DragRotL/X").Value = purify(E.BaseRotation);

			GUIEdit.GetNode<SpinBox>("DragPosG/X").Value = purify(E.GeneralPos.X);
			GUIEdit.GetNode<SpinBox>("DragPosG/Y").Value = purify(E.GeneralPos.Y);
			GUIEdit.GetNode<SpinBox>("DragSizeG/X").Value = purify(E.GeneralSize.X);
			GUIEdit.GetNode<SpinBox>("DragSizeG/Y").Value = purify(E.GeneralSize.Y);
			GUIEdit.GetNode<SpinBox>("DragPivotG/X").Value = purify(E.GeneralPivotOffset.X);
			GUIEdit.GetNode<SpinBox>("DragPivotG/Y").Value = purify(E.GeneralPivotOffset.Y);
			GUIEdit.GetNode<SpinBox>("DragRotG/X").Value = purify(E.GeneralRot);

			GUIEdit.GetNode<SpinBox>("DragPosP/X").Value =purify( E.PlotPos.X);
			GUIEdit.GetNode<SpinBox>("DragPosP/Y").Value = purify(E.PlotPos.Y);
			GUIEdit.GetNode<SpinBox>("DragSizeP/X").Value =purify(E.PlotSize.X);
			GUIEdit.GetNode<SpinBox>("DragSizeP/Y").Value = purify(E.PlotSize.Y);
			GUIEdit.GetNode<SpinBox>("DragPivotP/X").Value = purify(E.PlotPivotOffset.X);
			GUIEdit.GetNode<SpinBox>("DragPivotP/Y").Value = purify(E.PlotPivotOffset.Y);
			GUIEdit.GetNode<SpinBox>("DragRotP/X").Value = purify(E.PlotRot);

			GUIEdit.GetNode<SpinBox>("DragPosC/X").Value = purify(E.CameraPosition.X);
			GUIEdit.GetNode<SpinBox>("DragPosC/Y").Value = purify(E.CameraPosition.Y);
			GUIEdit.GetNode<SpinBox>("DragZoomC/X").Value = purify(E.CameraZoom.X);
			GUIEdit.GetNode<SpinBox>("DragZoomC/Y").Value = purify(E.CameraZoom.Y);
			GUIEdit.GetNode<SpinBox>("DragRotC/X").Value =  purify(E.CameraRotation);

		}
		else if (Event is editor.PhysicsEvent)
		{
			editor.PhysicsEvent E = (editor.PhysicsEvent)Event;
			GravityEdit.Visible = true;
			GravityEdit.GetNode<SpinBox>("DragDir/X").Value = E.GravityDirection;
			GravityEdit.GetNode<SpinBox>("DragMag/Y").Value = E.Magnitude;
		}
		//GD.Print();
		UpdateEdit();
		editing=true;
		
	}
	void ToggleClipContents(bool clip)
	{
		ClipContents=clip;
	}
}
