using Godot;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using static editor;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static ElementExplorer;
using System.Reflection;
//using System.Security.Cryptography.X509Certificates;


public partial class cinema : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public static string tmov_path = string.Empty;
	public static bool is_test_playing = false;
	public static bool is_in_cinema = false;
	public static bool placeholder_on=false;
	public static bool show_back_button = false;
	HashSet<Event> EventStream = new HashSet<Event>();
	public static HashSet<Element> Elements = new HashSet<Element>();

	Marker2D GravityValues;
	Hashtable Themes=new Hashtable();
	Control DefaultGeneralTextSection;
	Control DefaultPlotSection;
	public override void _Ready()
	{
		GeneralTextSection = GetNode<Control>("BaseLine/General");
		PlotSection = GetNode<RichTextLabel>("BaseLine/Plot/TextCase");
		PlotSection.Text = string.Empty;
		DefaultGeneralTextSection = (Control)GeneralTextSection.Duplicate();
		DefaultPlotSection = (Control)PlotSection.GetParent().Duplicate();
		BaseLine = GetNode<Node2D>("BaseLine");
		HeightMeasurer = GetNode<RichTextLabel>("HeightMeasurer");
		HeightMeasurerGroup = GetNode<Node2D>("HeightMeasurers");
		CameraShifter = GetNode<Node2D>("CameraShifter");
		Camera = CameraShifter.GetNode<Camera2D>("MicroOscillator/Camera2D");
		//ShakeValue = GetNode<Marker3D>("CameraShaker/Values");
		baseline_case = BaseLine.GetNode<Node2D>("Elements");
		camera_case = Camera.GetNode<Node2D>("Elements");
		general_case = GetNode<Node2D>("Elements");
		general_case_physic = GetNode<Node2D>("PhysicCase");
		camera_case_physic = Camera.GetNode<Node2D>("PhysicCaseCam");
		GravityValues=GetNode<Marker2D>("GravityValues");
		Gravity=GetNode<Area2D>("Gravity");
		//camera = editor.root.GetNode<Camera2D>("CameraShifter/MicroOscillator/Camera2D");
		// CameraShaker.Values=GetNode<Marker3D>("CameraShaker/Values");
		is_in_cinema=true;
		CameraShaker.Values.Position=Vector3.Zero;
		Load();
		if (is_test_playing)
		{
			Camera.GetNode<Button>("Back").Visible=show_back_button;
		}
		EventStream.Add(new GeneralTextEvent()
		{
			uid=-1,
			frame_stamp=0,
		}
		);
		playing = true;
		/*if (main.MovieMakerMode)
		{
			Engine.PhysicsTicksPerSecond=300;
			Engine.TimeScale=5;
		}*/
	}
	void Back()
	{
		frame=0;
		is_in_cinema=false;
		GetTree().ChangeSceneToFile("res://Scenes/editor.tscn");
	}
	void Load()
	{
		Hashtable ProjectData = new Hashtable();
		//GetNode<Button>("EventPanel/Load").Text="Loading...";

		//var path=LoadDialog.CurrentPath;
		//var project=FileAccess.Open(path,FileAccess.ModeFlags.Read);
		GD.Print(tmov_path);
		byte[] bytes = Decompress(System.IO.File.ReadAllBytes(tmov_path));
		var data = Encoding.Unicode.GetString(bytes);
		//debug(data);
		if (data != null)
		{
			try
			{
				var json = JsonConvert.DeserializeObject<Hashtable>(data);
				//debug("0-1. "+json);
				ProjectData["id_assigning_index"] = json["id_assigning_index"];
				ProjectData["euid_assigning_index"] = json["euid_assigning_index"];
				ProjectData["event_stream"] = json["event_stream"];
				ProjectData["elements"] = json["elements"];
				if (json.ContainsKey("themes")&&json["themes"]!=null)
				{
					ProjectData["themes"]=(JObject)json["themes"];
				}else
				{
					ProjectData["themes"]=new JObject();
				}
			}
			catch (Exception)
			{
			}
		}
		foreach (var token in (Newtonsoft.Json.Linq.JArray)ProjectData["elements"])
		{
			/*foreach (KeyValuePair<string,object> prop in _e)
			{
				if (prop.Value is HashSet<Hashtable>)
				{
					
				}
			}*/
			var _e = JTokenToHashtable(token);
			var element = new Element();
			element.FromDict(_e);
			GD.Print("ElementNode:"+element.Node);
			Elements.Add(element);
		}
		foreach (var token in (Newtonsoft.Json.Linq.JArray)ProjectData["event_stream"])
		{
			var _e = JTokenToHashtable(token);
			var type = int.Parse((string)_e["type"]);
			Type T = EventTypes[type];
			var _Event = Activator.CreateInstance(T);
			//debug("1-1. "+T);
			//debug("1-2. "+T.GetMethod("FromDict"));

			T.GetMethod("FromDict").Invoke(_Event, new object[] { _e });
			if (_Event is ElementEvent)
			{
				var target_euid=long.Parse((string)_e["element"]);
				
				var ie=Elements.Where(e=>{GD.PrintErr("Found "+e.euid+" for euid"+target_euid);return e.euid==target_euid;});
				if (ie.Count()>0)
				{
					((ElementEvent)_Event).element=ie.First();
					GD.PrintErr("Loaded Element: "+target_euid+" from "+((ElementEvent)_Event).name);
				}
			}
			EventStream.Add((Event)_Event);
		}
		foreach (KeyValuePair<string,JToken> kvp in (JObject)ProjectData["themes"])
		{
			var _t=(string)kvp.Value;
			Encoding custom=new CustomEncoder();
			Themes[long.Parse(kvp.Key)]=(PackedScene)GD.BytesToVarWithObjects(custom.GetBytes(_t));
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	Variant ObjToVar(object obj)
	{
		var T=obj.GetType();
		GD.Print("ObjToVar:"+T);
		if (T==typeof(float))
		{
			return Variant.From((float)obj);
		}else if(T==typeof(Variant))
		{
			return (Variant)obj;
		}
		else if(T==typeof(Vector2))
		{
			return Variant.From((Vector2)obj);
		}else if(T==typeof(Color))
		{
			return Variant.From((Color)obj);
		}
		return new Variant();
	}
void do_tween(List<object>[] props, object[] nodes, string[] paths, object[] values)
		{
			//.SetEase(Tween.EaseType.In).SetParallel().BindNode(this);
			//tween.Stop();
			var i = 0;
			foreach (List<object> prop in props)
			{
				
				if (prop != null)
				{
					if ((bool)prop[4])
					{
						
						var curve_id = (long)prop[0]-1;
						GD.Print(nodes[i],"==tween==> ",paths[i],"curve_id=",curve_id);
						if (curve_id == -1)
						{
							Tween tween = GetTree().CreateTween().BindNode(this);
							tween.TweenProperty((GodotObject)nodes[i], paths[i], ObjToVar(values[i]), 0.000f).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Linear);
							GD.Print("set_tween");
						}
						else
						{
							Tween tween = GetTree().CreateTween().BindNode(this);
							tween.TweenProperty((GodotObject)nodes[i], paths[i], ObjToVar(values[i]), (double)prop[2]).SetEase((Tween.EaseType)prop[1]).SetTrans((Tween.TransitionType)curve_id);
							GD.Print("set_tween");
						}
					}
				}
				i++;
			}
			//tween.Play();
		}
	void _Tween(Event _event)
	{
		var tween_props = _event.tweens;

		
		void TweenElement(ElementEvent Event)
		{
			var e = (ElementEvent)_event;
			var element = e.element;
			var uuid = e.uuid;
			if (!Instances.ContainsKey(uuid))
			{
				return;
			}
			var instance = (Node2D)Instances[uuid];
			var _p = Event.element_props;
			//element_prop
			{
				var posx = (List<object>)tween_props["pos.x"]; var posy = (List<object>)tween_props["pos.y"];
				var rot = (List<object>)tween_props["rot"];
				var scalex = (List<object>)tween_props["scale"]; var scaley = (List<object>)tween_props["scale.y"];
				var offx = (List<object>)tween_props["offset.x"]; var offy = (List<object>)tween_props["offset.y"];
				var sizex = (List<object>)tween_props["size.x"]; var sizey = (List<object>)tween_props["size.y"];
				
				var props = new List<object>[] { posx, posy, rot, scalex, scaley, offx, offy, sizex, sizey };
				var paths = new string[] { "position:x", "position:y", "rotation_degrees", "scale:x", "scale:y", "position:x", "position:y", "size:x", "size:y" };

				var Pos = VarTo<Vector2>(_p["pos"]);
				var Rot = _p["rot"];
				var Scale = VarTo<Vector2>(_p["scale"]);
				
				var Size = VarTo<Vector2>(_p["size"]);
var Offset = VarTo<Vector2>(_p["offset"])-Size/2;
				var vals = new object[] { Pos.X, Pos.Y, Rot, Scale.X, Scale.Y, Offset.X, Offset.Y, Size.X, Size.Y };
				GD.PrintErr(tween_props);
				if (tween_props.ContainsKey("ContentColor"))
				{
					var tween_c= (List<object>)tween_props["ContentColor"];
					//((Variant)).AsColor();
					var props_c = new List<object>[]{tween_c};
					var path_c=new string[] {"self_modulate"};
					var color =new object[]{Event.ContentColor};
					do_tween
					(
						props_c,
						new object[] {instance},
						path_c,
						color
					);
				}
				
				switch (element.type)
				{
					case ElementExplorer.ElementType.Image:
					case ElementExplorer.ElementType.Video:
					case ElementExplorer.ElementType.Text:
						{
							var control_node=instance.GetChild<Control>(0);
							var bgcolor=control_node.GetChild<ColorRect>(0);
							do_tween
							(
								props,
								new object[] { instance, instance, instance, instance, instance, control_node, control_node, control_node, control_node },
								paths,
								vals
							);
							if (tween_props.ContainsKey("BackgroundColor"))
							{
								var tween_c= (List<object>)tween_props["BackgroundColor"];
								//((Variant)).AsColor();
								var props_c = new List<object>[]{tween_c};
								var path_c=new string[] {"color"};
								var color =new object[]{Event.BackgroundColor};
								do_tween
								(
									props_c,
									new object[] {bgcolor},
									path_c,
									color
								);
							}
						}
						break;
					case ElementExplorer.ElementType.Progress:
						{
							Control prog;
							//Control _prog;
							switch ((int)instance.GetMeta("Type"))
							{
								default:
								case 0://Bar
									prog = instance.GetNode<Control>("Bar");
									//_prog = e.element.Node.GetNode<Control>("Bar");
									break;
								case 1://ring
									prog = instance.GetNode<Control>("Ring");
									//_prog = e.element.Node.GetNode<Control>("Ring");
									break;
								case 2://wave
									prog = instance.GetNode<Control>("Wave");
									//_prog = e.element.Node.GetNode<Control>("Wave");
									break;
							}
							do_tween
							(
								props,
								new object[] { instance, instance, instance, instance, instance, prog, prog, prog, prog },
								paths,
								vals
							);
						}
						break;
				}
			}
			{

				switch (element.type)
				{
					case ElementExplorer.ElementType.Animation:
						{
							do_tween
							(
								new List<object>[] { (List<object>)tween_props["speed_scale"] },
								new object[] { (AnimatedSprite2D)instance },
								new string[] { "speed_scale" },
								new object[] { _p["speed_scale"] }
							);
						}
						break;
					case ElementExplorer.ElementType.Text:
						{
							do_tween
							(
								new List<object>[] { (List<object>)tween_props["visible_ratio"] },
								new object[] { (RichTextLabel)instance.GetChild(0) },
								new string[] { "visible_ratio" },
								new object[] { _p["visible_ratio"] }
							);
						}
						break;
					case ElementExplorer.ElementType.Progress:
						{
							Control prog;
							var _t = instance.GetMeta("Type");
							switch ((int)_t)
							{
								default:
								case 0://Bar
									prog = instance.GetNode<Control>("Bar");
									//_prog = e.element.Node.GetNode<Control>("Bar");
									break;
								case 1://ring
									prog = instance.GetNode<Control>("Ring");
									//_prog = e.element.Node.GetNode<Control>("Ring");
									break;
								case 2://wave
									prog = instance.GetNode<Control>("Wave");
									//_prog = e.element.Node.GetNode<Control>("Wave");
									break;
							}
							var name_posx = (List<object>)tween_props["name_pos.x"]; var name_posy = (List<object>)tween_props["name_pos.y"];
							var prog_posx = (List<object>)tween_props["prog_pos.x"]; var prog_posy = (List<object>)tween_props["prog_pos.y"];
							var progress = (List<object>)tween_props["progress"];

							var name_node = instance.GetNode<Control>("Name");
							var prog_node = instance.GetNode<Control>("Progress");
							var prog_val = instance.GetNode<Marker2D>("Value");

							var nPos = VarTo<Vector2>(_p["name_pos"]);
							var pPos = VarTo<Vector2>(_p["prog_pos"]);
							do_tween
							(
								new List<object>[] { name_posx, name_posy, prog_posx, prog_posy, progress },
								new object[] { name_node, name_node, prog_node, prog_node, prog_val },
								new string[] { "position:x", "position:y", "position:x", "position:y", "position:x" },
								new object[] { nPos.X, nPos.Y, pPos.X, pPos.Y, _p["progress"] }
							);
							switch ((int)_t)
							{
								default:
								case 0://Event.element_props["border_color"];
									{

										do_tween
										(
											new List<object>[] { (List<object>)tween_props["border_color"], (List<object>)tween_props["fill_color"] },
											new object[] { prog, prog.GetNode<Control>("Fill") },
											new string[] { "self_modulate", "self_modulate" },
											new object[] { _p["border_color"], _p["fill_color"] }
										);
									}
									break;
								case 1:
									{
										var shader = (ShaderMaterial)prog.Material;
										var R = (List<object>)tween_props["radius"];
										var r = (List<object>)tween_props["hollow_radius"];
										var seg = (List<object>)tween_props["segments"];
										var margin = (List<object>)tween_props["margin"];
										var rot = (List<object>)tween_props["rotation"];
										var color = (List<object>)tween_props["color"];
										do_tween
										(
											new List<object>[] { R, r, seg, margin, rot, color },
											new object[] { shader, shader, shader, shader, shader, shader },
											new string[] { "radius", "hollow_radius", "segments", "margin", "rotation", "color" },
											new object[] { _p["radius"], _p["hollow_radius"], _p["segments"], _p["margin"], _p["rotation"], _p["color"] }
										);


									}
									break;
								case 2:
									{
										var shader = (ShaderMaterial)prog.Material;
										var bwc = (List<object>)tween_props["back_wave_color"];
										var fwc = (List<object>)tween_props["front_wave_color"];
										var fc = (List<object>)tween_props["fill_color"];
										var rc = (List<object>)tween_props["ring_color"];
										var rw = (List<object>)tween_props["ring_width"];
										var io = (List<object>)tween_props["inner_offset"];
										var amp = (List<object>)tween_props["amp_mul"];
										var freq = (List<object>)tween_props["freq_mul"];

										do_tween
										(
											new List<object>[] { bwc, fwc, fc, rc, rw, io, amp, freq },
											new object[] { shader, shader, shader, shader, shader, shader, shader, shader },
											new string[] { "backFillColour", "frontFillOuterColour", "fillcolour", "ringColour", "ringWidth", "innerCircleRadiusOffset", "amp_mul", "freq_mul" },
											new object[] { _p["back_wave_color"], _p["front_wave_color"], _p["fill_color"], _p["ring_color"], _p["ring_width"], _p["inner_offset"], _p["amp_mul"], _p["freq_mul"] }
										);
									}
									break;
							}
						}
						break;
				}
			}
			//Tween tween = GetTree().CreateTween().SetEase(Tween.EaseType.Out);
			//tween.TweenProperty(PlotSection, "theme_override_font_sizes/normal_font_size", new Color(1,1,1,1), dur).SetTrans(Tween.TransitionType.Linear);
			//tween.SetParallel();
			//tween.TweenCallback(Callable.From(()=>{text_case.FitContent=true;}));
		}
		switch (_event.type)
		{
			case 0:
				break;
			case 1:
				{
					Tween tween = GetTree().CreateTween().SetEase(Tween.EaseType.Out);
					tween.TweenProperty(PlotSection, "theme_override_font_sizes/normal_font_size", ((PlotEvent)_event).TextSize, 0.5).SetTrans(Tween.TransitionType.Linear);
					tween.SetParallel();
					//tween.TweenCallback(Callable.From(()=>{text_case.FitContent=true;}));
					//text_size
				}
				break;
			case 2:
				{
					TweenElement((ElementEvent)_event);
				}
				break;
			case 3:
				{

					var val = GetNode<Marker2D>("GravityValues");
					var dir = (List<object>)tween_props["GravityDirection"];
					var mag = (List<object>)tween_props["Magnitude"];
					do_tween
					(
						new List<object>[] { dir, mag },
						new object[] { val, val },
						new string[] { "position:x", "position:y" },
						new object[] { ((PhysicsEvent)_event).GravityDirection, ((PhysicsEvent)_event).Magnitude }
					);
					//text_size
				}
				break;
			case 4:
				{
					TweenElement((ElementEvent)_event);
				}
				break;
			case 5:
				{
					//element_prop
					var line_posx = (List<object>)tween_props["BasePosition.x"]; var line_posy = (List<object>)tween_props["BasePosition.y"];
					var line_rot = (List<object>)tween_props["BaseRotation"];var line_color = (List<object>)tween_props["LineColor"];
					var gposx = (List<object>)tween_props["GeneralPos.x"]; var gposy = (List<object>)tween_props["GeneralPos.y"];
					var grot = (List<object>)tween_props["GeneralRot"];
					var gsizex = (List<object>)tween_props["GeneralSize.x"]; var gsizey = (List<object>)tween_props["GeneralSize.y"];

					var goffsetx = (List<object>)tween_props["GeneralPivotOffset.x"]; var goffsety = (List<object>)tween_props["GeneralPivotOffset.y"];
					var pposx = (List<object>)tween_props["PlotPos.x"]; var pposy = (List<object>)tween_props["PlotPos.y"];
					var prot = (List<object>)tween_props["PlotRot"];
					var psizex = (List<object>)tween_props["PlotSize.x"]; var psizey = (List<object>)tween_props["PlotSize.y"];
					var poffsetx = (List<object>)tween_props["PlotPivotOffset.x"]; var poffsety = (List<object>)tween_props["PlotPivotOffset.y"];

					var cposx = (List<object>)tween_props["CameraPosition.x"]; var cposy = (List<object>)tween_props["CameraPosition.y"];
					var crot = (List<object>)tween_props["CameraRotation"];
					var czoomx = (List<object>)tween_props["CameraZoom.x"]; var czoomy = (List<object>)tween_props["CameraZoom.y"];

					var plot_node = PlotSection.GetParent();
					var e = (GUIEvent)_event;
					do_tween
					(
						new List<object>[]{
						line_posx,line_posy,line_rot,gposx,gposy,grot,gsizex,gsizey,goffsetx,goffsety,
						pposx,pposy,prot,psizex,psizey,poffsetx,poffsety,cposx,cposy,crot,czoomx,czoomy,line_color
						},
						new object[]
						{
						BaseLine,BaseLine,BaseLine,GeneralTextSection,GeneralTextSection,GeneralTextSection,GeneralTextSection,
						GeneralTextSection,GeneralTextSection,GeneralTextSection,
						plot_node,plot_node,plot_node,plot_node,plot_node,plot_node,plot_node,CameraShifter,CameraShifter,CameraShifter,
						camera,camera,BaseLine
						},
						new string[]{
						"position:x","position:y","rotation_degrees","position:x","position:y","rotation_degrees",
						"size:x","size:y","pivot_offset:x","pivot_offset:y","position:x","position:y","rotation_degrees",
						"size:x","size:y","pivot_offset:x","pivot_offset:y","position:x","position:y","rotation_degrees",
						"zoom:x","zoom:y","default_color"
						},
						new object[]{
						e.BasePosition.X,e.BasePosition.Y,e.BaseRotation,e.GeneralPos.X,e.GeneralPos.Y,e.GeneralRot,
						e.GeneralSize.X,e.GeneralSize.Y,e.GeneralPivotOffset.X,e.GeneralPivotOffset.Y,e.PlotPos.X,e.PlotPos.Y,
						e.PlotRot,e.PlotSize.X,e.PlotSize.Y,e.PlotPivotOffset.X,e.PlotPivotOffset.Y,
						e.CameraPosition.X,e.CameraPosition.Y,e.CameraRotation,e.CameraZoom.X,e.CameraZoom.Y,e.LineColor
						}
					);

					//tween.TweenCallback(Callable.From(()=>{text_case.FitContent=true;}));

				}
				break;

		}
	}
	bool playing = false;
	public static long frame = -60;
	public static Camera2D Camera;
	Node2D CameraShifter;
	Queue<Event> queue=new Queue<Event>();
	 public static void ReplaceAllVsProperties(object obj,EventGroup group)
    {
        if (obj == null) return;
        
        var properties = obj.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(string) && p.CanWrite && p.CanRead);
        
        foreach (var property in properties)
        {
            var value = property.GetValue(obj) as string;
            if (value == "$vs")
            {
                property.SetValue(obj, group.VarString);
            }
			 if (value == "$name")
            {
                property.SetValue(obj, group.name);
            }
        }
    }
	void InvokeEvent(Event _event)
	{//Invoke+Tween
		switch (_event.type)
		{
			case 0:
				PopupGeneralText((GeneralTextEvent)_event);
				break;
			case 1:
				WritePlot((PlotEvent)_event);
				break;
			case 2:
				DropElement((ElementEvent)_event);
				break;
			case 3: break;
			case 4:
				DropElement((PhysicsElementEvent)_event);
				break;
			case 5: break;
			case 6:
				RemoveGeneralUnit((GeneralTextRemoveEvent)_event);
				break;
			case 9:
				DoArchive((ArchiveEvent)_event);
				break;
			case 10:
				InvokeSpecialEvent((SpecialEvent)_event);
				break;
				case 11:
				ShakeCamera((CameraShakeEvent)_event);
				break;
				case 12:
				{
					var _e=(EventGroup)_event;
					foreach (var item in _e.Stream)
					{
						ReplaceAllVsProperties(item,_e);
						if (item.frame_stamp==0)
						{
							
							
							InvokeEvent(item);
						}
						item.frame_stamp=item.frame_stamp+_e.frame_stamp;
						
						Addition.Add(item);
					}
				}
			
				break;
				default:
				break;
		}
		_Tween(_event);
	}
	Area2D Gravity;
	HashSet<Event> RemovedEvents=new HashSet<Event> ();
HashSet<Event> Addition=new HashSet<Event> ();
	public override void _PhysicsProcess(double delta)
	{
		if (playing)
		{
			frame += 1;
				
				foreach (var e in EventStream)
				{
					if (e.frame_stamp >= frame&&e.frame_stamp <frame+10)
					{
						InvokeEvent(e); RemovedEvents.Add(e);
					}
				}
				foreach (var e in RemovedEvents)
				{
					EventStream.Remove(e);
				}
				foreach (var e in Addition)
				{
					EventStream.Add(e);
				}
				if (Gravity!=null&&GravityValues!=null)
				{
					Gravity.Gravity=GravityValues.Position.Y;
					var theta=GravityValues.Position.X;
					Gravity.GravityDirection=Vector2.FromAngle(Mathf.DegToRad(theta) );
				}
				
				
			
		}
	}

	PackedScene DefaultTextUnit = ResourceLoader.Load<PackedScene>("res://Scenes/default_text_unit.tscn");
	PackedScene DefaultPlotStyle = ResourceLoader.Load<PackedScene>("res://Scenes/default_plot_style.tscn");
	Script GTUScript = ResourceLoader.Load<Script>("res://Scripts/GeneralTextUnit.cs");
	RichTextLabel HeightMeasurer;
	Node2D HeightMeasurerGroup;
	public static Control GeneralTextSection;
	public static RichTextLabel PlotSection;
	Node2D BaseLine;
	RichTextLabel _text_case;
	string small_chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&'()*+,-./:;<=>?@[]^_`{|}~。？！，、；：“”‘’（）《》〈〉【】『』「」〔〕…—～﹏" + '"';
	Control UnpackUI(PackedScene ui)
	{
		var theme=ui.Instantiate<Control>();
		var patch_rect = theme.GetNode<NinePatchRect>("Texture");
		var image_euid=(long)theme.GetMeta("image_euid");
		//GD.Print("image_euid",image_euid);
		foreach (var e in Elements)
			{
				GD.Print(e.euid,",",image_euid);
				if (e.euid==image_euid)
				{
					if (e.FrameData.Count>0)
					{
						var tex = new PortableCompressedTexture2D();
						tex._Data = (byte[])e.FrameData.ElementAt(0).Clone();
						
						tex.KeepCompressedBuffer = false;
						patch_rect.Texture=tex;
					}
				}
			}
		theme.GetNode<Control>("Decos").SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		foreach (Control deco in theme.GetNode<Control>("Decos").GetChildren())
		{
			var deco_image_euid=(long)deco.GetMeta("image_euid");
			var sprite=deco.GetNode<AnimatedSprite2D>("Sprite");
			
			if (deco_image_euid>-1) 
			{
				editor.Element selected_image=null;
				foreach (var item in Elements)
				{
						//GD.Print(deco.GetMeta("image_euid"),",",item.euid);
					if (item.euid==deco_image_euid)
					{
						selected_image=item;
					}
				}
				if (selected_image!=null)
				{
					if (selected_image.type == ElementExplorer.ElementType.Image)
					{
						var frames = new SpriteFrames();
						if (selected_image.FrameData.Count>0)
						{
							var tex = new PortableCompressedTexture2D();
							tex._Data = (byte[])selected_image.FrameData.ElementAt(0).Clone();
							
							tex.KeepCompressedBuffer = false;
							frames.AddFrame("default", tex);
						}
						sprite.SpriteFrames = frames;
						sprite.TextureFilter=selected_image.Node.GetChild<TextureRect>(0).TextureFilter;
						
					}else if(selected_image.type == ElementExplorer.ElementType.Animation)
					{
						//sprite.SpriteFrames = (SpriteFrames)((AnimatedSprite2D)selected_image.Node).SpriteFrames.Duplicate();
						var frames = (SpriteFrames)((AnimatedSprite2D)selected_image.Node).SpriteFrames.Duplicate();

						frames.Clear("default");
						foreach (var item in selected_image.FrameData)
						{
							var tex=new PortableCompressedTexture2D();
							tex._Data= (byte[])item.Clone();
							GD.Print("[Cinema:] Anim");
							tex.KeepCompressedBuffer=false;
							
							frames.AddFrame("default",tex);
							
						}
							sprite.TextureFilter=selected_image.Node.TextureFilter;
						sprite.SpriteFrames = frames;
						sprite.Play();
					}
					deco.SetMeta("changed",false);
				}
				deco.SetAnchorAndOffset(Side.Left,deco.AnchorLeft,0);
				deco.SetAnchorAndOffset(Side.Top,deco.AnchorTop,0);
				deco.SetAnchorAndOffset(Side.Right,deco.AnchorLeft,0);
				deco.SetAnchorAndOffset(Side.Bottom,deco.AnchorTop,0);
				deco.Size=Vector2.Zero;

			
			}
			sprite.Visible=true;
		}
		return theme;
	}
	void PopupGeneralText(GeneralTextEvent Event)
	{
		var text = Event.Text;
		var size = Event.TextSize;
		var dur = Event.PopupDuration;
		var padder=new Control();
		Control ui = null;
		long tuid=Event.ui_override_tuid;
		//GD.Print()
		if (tuid != -1)
		{
			ui = UnpackUI((PackedScene)Themes[tuid]);


		}
		else
		{
			ui = DefaultTextUnit.Instantiate<Control>();
		}
		ui.Visible = false;
		var Val = new Marker2D();
		Val.Name = "_TweenedVal";
		Val.Position = new Vector2(0, size);
		ui.AddChild(Val);

		ulong objId = ui.GetInstanceId();
		ui.SetScript(GTUScript);
		ui = (Control)InstanceFromId(objId);

		var text_case = ui.GetNode<RichTextLabel>("TextCase");
		if (Event.Centered)
		{
			text="[center]"+text+"[/center]";
		}
		text_case.Text = text;
		text_case.AddThemeFontSizeOverride("normal_font_size", size);
		text_case.VisibleRatio = 0;
		text_case.BbcodeEnabled=true;
		ui.Modulate = new Color(1, 1, 1, 0);
		ui.Size = Vector2.Zero;
		ui.CustomMinimumSize *= Vector2.Right;
		ui.SelfModulate=Event.UIColorMod;
		ui.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		if (Event.HorizontalPadding>0)
		{
			ui.AnchorLeft=Mathf.Abs(Event.HorizontalPadding);
		}else
		{ui.AnchorRight=1-Mathf.Abs(Event.HorizontalPadding);
			
		}
		//HeightMeasurer.Text=text;
		//var h=HeightMeasurer.Size.Y;

padder.AddChild(ui);
		GeneralTextSection.AddChild(padder);
		//var rect=new ReferenceRect();
		//rect.EditorOnly=false;
		//rect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		//padder.AddChild(rect);
		ui.Visible = true;
		Tween tween = GetTree().CreateTween().SetEase(Tween.EaseType.Out);
		tween.TweenProperty(ui, "modulate", new Color(1, 1, 1, 1), dur).SetTrans(Tween.TransitionType.Linear);
		tween.SetParallel();
		tween.TweenProperty(Val, "position:x", 1, dur).SetDelay(0.02).SetTrans(Tween.TransitionType.Quad);
		tween.TweenProperty(text_case, "visible_ratio", 1, dur).SetDelay(0.1).SetTrans(Tween.TransitionType.Quad);
		tween.TweenCallback(Callable.From(() => { text_case.FitContent = true; }));
		//ui.SetScript(GTUScript);
	}

	async Task WritePlotAsync(string text, float spd)
	{
		foreach (char ch in text)
		{
			PlotSection.Text += ch;
			//GD.Print(spd);
			if (small_chars.Contains(ch))
			{

				await Task.Delay((int)Mathf.Ceil(500 / spd));
			}
			else
			{
				await Task.Delay((int)Mathf.Ceil(1000 / spd));
			}
		}
	}
	void WritePlot(PlotEvent Event)
	{
		var text = Event.Text;
		var size = Event.TextSize;
		var spd = Event.WriteSpeed;
		var use_def = Event.UseDefaultStyle;
		Control ui = null;
		long tuid=Event.ui_override_tuid;
		if (tuid != -1)
		{
			ui = UnpackUI((PackedScene)Themes[tuid]);

		}
		else if (use_def)
		{
			ui = DefaultPlotStyle.Instantiate<Control>();
		}


		if (ui != null)
		{
			ui.Name = "Plot";
			ui.Size = PlotSection.Size;
			ui.Position = PlotSection.GetParent<Control>().Position;
			PlotSection.GetParent<Control>().QueueFree();
			BaseLine.AddChild(ui);
			PlotSection = ui.GetNode<RichTextLabel>("TextCase");
		}
		PlotSection.AddThemeFontSizeOverride("normal_font_size", size);
		WritePlotAsync(text, Mathf.Clamp(spd, 0.01f, 1000));
	}
	Hashtable Instances = new Hashtable();
	Hashtable RegularInstances=new Hashtable();
	Hashtable PhysicInstances=new Hashtable();
	Node2D baseline_case;
	Node2D camera_case;
	Node2D general_case;
	Node2D general_case_physic;
	Node2D camera_case_physic;
	dynamic VarTo<[MustBeVariant] T>(object obj)
	{
		return ((Variant)obj).As<T>();
	}
	 public void CreateAutoFitText(Control target, string content)
    {
        // 1. 创建 Label
        Label label = new Label();
        label.Text = content;
        
        // 2. 设置布局：覆盖全父节点 (Full Rect)
        target.AddChild(label);
        label.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        
        // 3. 设置文字居中
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;

        // 4. 配置视觉样式：白边黑字 (LabelSettings)
        LabelSettings settings = new LabelSettings();
        settings.FontColor = new Color(0,0,0,0.3f);       // 黑字
        settings.OutlineColor = new Color(1,1,1,0.3f);    // 白边
        // 注意：描边宽度会在后面根据字号动态调整，否则字号大时描边显得太细
        
        label.LabelSettings = settings;
		label.ZIndex=-1;
        // 5. 核心逻辑：自动计算最大字号
        // 我们需要等待一帧或者在布局完成后计算，这里使用 CallDeferred 确保 Parent 尺寸已确定
        label.Ready += () => AdjustFontSizeToFit(label, target.Size);
        
        // 如果目标 Control 尺寸发生变化，自动重新调整
        target.Resized += () => AdjustFontSizeToFit(label, target.Size);
    }

    private void AdjustFontSizeToFit(Label label, Vector2 containerSize)
    {
        if (string.IsNullOrEmpty(label.Text) || containerSize.X <= 0 || containerSize.Y <= 0) return;

        var settings = label.LabelSettings;
        Font font = label.GetThemeFont("font"); // 获取当前主题字体

        // 初始尝试一个较大的字号
        int minSize = 2;
        int maxSize = 500; // 设一个合理的上限
        int bestSize = minSize;

        // 使用二分查找法快速找到最合适的字号
        while (minSize <= maxSize)
        {
            int mid = (minSize + maxSize) / 2;
            // 获取在 mid 字号下，文字所占的实际像素宽高
            Vector2 stringSize = font.GetStringSize(label.Text, HorizontalAlignment.Center, -1, mid);

            if (stringSize.X <= containerSize.X && stringSize.Y <= containerSize.Y)
            {
                bestSize = mid;
                minSize = mid + 1;
            }
            else
            {
                maxSize = mid - 1;
            }
        }

        // 应用计算出的字号
        settings.FontSize = bestSize;
        
        // 动态调整描边粗细（通常为字号的 10%-15% 视觉效果最好）
        settings.OutlineSize = Mathf.Max(1, bestSize / 8); 
    }
	void DropElement(ElementEvent Event)
	{

		//Physics or General

		//Load the Element Instantiate(Duplicate)
		//Set Props(UUID=...)

		var element = Event.element;

		var properties = Event.element_props;
		var tween = Event.tweens;
		var belongs_to = Event.belongs_to;
		var uuid = Event.uuid;
		
		Node2D instance;
		
		if (!Instances.ContainsKey(uuid)&&Event.DeleteMode==false)
		{
			GD.PrintErr("Element "+element+" from "+Event.uuid);
			instance = (Node2D)element.Node.Duplicate();
			Instances[uuid] = instance;
			
			instance.Position = ((Variant)properties["pos"]).AsVector2();
			instance.Rotation = ((Variant)properties["rot"]).As<float>();
			instance.Scale = ((Variant)properties["scale"]).AsVector2();
			instance.SelfModulate=Event.ContentColor;

			instance.ZIndex = ((Variant)properties["z_index"]).As<int>();
			switch (element.type)
		{
			
			case ElementType.Image:{
			if (element.FrameData.Count>0)
					{
						var tex = new PortableCompressedTexture2D();
						tex._Data = (byte[])element.FrameData.ElementAt(0).Clone();
						
						tex.KeepCompressedBuffer = false;
						instance.GetNode<TextureRect>("Image").Texture=tex;
						
					}
					if (placeholder_on&&Event.PlaceHolderInfo!="")
						{
							CreateAutoFitText(instance.GetNode<TextureRect>("Image"),Event.PlaceHolderInfo);
						}
			break;}
			case ElementType.Animation:{
			((AnimatedSprite2D)instance).SpriteFrames.Clear("default");
			foreach (var item in element.FrameData)
			{
				var tex=new PortableCompressedTexture2D();
				tex._Data= (byte[])item.Clone();
				GD.Print(tex);
				tex.KeepCompressedBuffer=true;
				
				((AnimatedSprite2D)instance).SpriteFrames.AddFrame("default",tex);
				
			}((AnimatedSprite2D)instance).Play();}
			break;
		}
			switch (element.type)
			{
				case ElementExplorer.ElementType.Image:
				case ElementExplorer.ElementType.Video:
				case ElementExplorer.ElementType.Text:
					{
						
						instance.GetChild<Control>(0).Size = ((Variant)properties["size"]).AsVector2();
						instance.GetChild<Control>(0).Position = ((Variant)properties["offset"]).AsVector2()-instance.GetChild<Control>(0).Size/2;
						instance.GetChild<Control>(0).GetChild<ColorRect>(0).Color=Event.BackgroundColor;
						instance.GetChild<Control>(0).GetChild<ColorRect>(0).ZIndex=-1;
					}
					break;
				case ElementExplorer.ElementType.Progress:
					{
						Control prog;
						//Control _prog;
						switch ((int)instance.GetMeta("Type"))
						{
							default:
							case 0://Bar
								prog = instance.GetNode<Control>("Bar");
								//_prog = e.element.Node.GetNode<Control>("Bar");
								break;
							case 1://ring
								prog = instance.GetNode<Control>("Ring");
								//_prog = e.element.Node.GetNode<Control>("Ring");
								break;
							case 2://wave
								prog = instance.GetNode<Control>("Wave");
								//_prog = e.element.Node.GetNode<Control>("Wave");
								break;
						}
						prog.Position = ((Variant)properties["offset"]).AsVector2();
						prog.Size = ((Variant)properties["size"]).AsVector2();
					}
					break;
			}

			switch (element.type)
			{
				case ElementExplorer.ElementType.Animation:
					{
						((AnimatedSprite2D)instance).SpeedScale = ((Variant)properties["speed_scale"]).AsSingle();

					}
					break;
				case ElementExplorer.ElementType.Text:
					{
						//((RichTextLabel)instance.GetChild(0)).VisibleRatio = ((Variant)properties["visible_ratio"]).AsSingle();
					}
					break;
				case ElementExplorer.ElementType.Progress:
					{
						Control prog;
						//Control _prog;
						var _t = instance.GetMeta("Type");
						switch ((int)_t)
						{
							default:
							case 0://Bar
								prog = instance.GetNode<Control>("Bar");
								//_prog = e.element.Node.GetNode<Control>("Bar");
								break;
							case 1://ring
								prog = instance.GetNode<Control>("Ring");
								//_prog = e.element.Node.GetNode<Control>("Ring");
								break;
							case 2://wave
								prog = instance.GetNode<Control>("Wave");
								//_prog = e.element.Node.GetNode<Control>("Wave");
								break;
						}

						instance.GetNode<Control>("Name").Position = ((Variant)properties["name_pos"]).As<Vector2>();
						instance.GetNode<Control>("Progress").Position = ((Variant)properties["prog_pos"]).As<Vector2>();
						var progress = ((Variant)properties["progress"]).As<float>();
						instance.GetNode<Control>("Name").Visible = ((Variant)properties["prog_on"]).As<bool>();
						instance.GetNode<Control>("Progress").Visible = ((Variant)properties["name_on"]).As<bool>();


						switch ((int)_t)
						{
							default:
							case 0://Event.element_props["border_color"];
								{
									var progress_node = element.Node.GetNode<Control>("Bar");
									progress_node.SelfModulate = (Color)VarTo<Color>(properties["border_color"]);
									progress_node.GetNode<Control>("Fill").SelfModulate = (Color)VarTo<Color>(properties["fill_color"]);
									StyleBoxFlat style = (StyleBoxFlat)((Panel)progress_node).GetThemeStylebox("Panel");
									style.SetBorderWidthAll((int)VarTo<int>(properties["border_width"]));

									var behavior = (int)prog.GetMeta("Behavior");
									var fill = prog.GetNode<Control>("Fill");
									switch (behavior)
									{
										case 0:
										default:
											fill.Position = Vector2.Zero;
											fill.Size = new Vector2((float)(prog.Size.X * progress * 0.01), prog.Size.Y);
											break;
										case 1:
											fill.Position = Vector2.Zero;
											fill.Size = new Vector2((float)(prog.Size.X * (1 - progress * 0.01)), prog.Size.Y);
											break;
										case 2:
											fill.Position = Vector2.Right * prog.Size.X * (float)(1 - progress * 0.01) / 2;
											fill.Size = new Vector2((float)(prog.Size.X * progress * 0.01), prog.Size.Y);
											break;
										case 3:
											fill.Position = Vector2.Right * prog.Size.X * (float)(progress * 0.01) / 2;
											fill.Size = new Vector2((float)(prog.Size.X * (1 - progress * 0.01)), prog.Size.Y);
											break;

									}
								}
								break;
							case 1:
								{
									var progress_node = element.Node.GetNode<Control>("Ring");


									var shader = (ShaderMaterial)progress_node.Material;
									shader.SetShaderParameter("radius", VarTo<float>(properties["radius"]));
									shader.SetShaderParameter("hollow_radius", VarTo<float>(properties["hollow_radius"]));
									shader.SetShaderParameter("segments", VarTo<float>(properties["segments"]));
									shader.SetShaderParameter("margin", VarTo<float>(properties["margin"]));
									shader.SetShaderParameter("rotation", VarTo<float>(properties["rotation"]));
									shader.SetShaderParameter("color", VarTo<Color>(properties["color"]));
									shader.SetShaderParameter("value", progress);

									var behavior = (int)prog.GetMeta("Behavior");
									switch (behavior)
									{
										case 0:
										default:
											shader.SetShaderParameter("progress_rotation", 0);
											break;
										case 1:
											shader.SetShaderParameter("progress_rotation", -progress * 0.01);
											break;

									}
								}
								break;
							case 2:
								{
									var progress_node = element.Node.GetNode<Control>("Wave");
									var shader = (ShaderMaterial)progress_node.Material;
									shader.SetShaderParameter("backFillColour", VarTo<Color>(properties["back_wave_color"]));
									shader.SetShaderParameter("frontFillOuterColour", VarTo<Color>(properties["front_wave_color"]));
									shader.SetShaderParameter("fillcolour", VarTo<Color>(properties["fill_color"]));
									shader.SetShaderParameter("ringColour", VarTo<Color>(properties["ring_color"]));
									shader.SetShaderParameter("ringWidth", VarTo<float>(properties["ring_width"]));
									shader.SetShaderParameter("innerCircleRadiusOffset", VarTo<float>(properties["inner_offset"]));
									shader.SetShaderParameter("amp_mul", VarTo<float>(properties["amp_mul"]));
									shader.SetShaderParameter("freq_mul", VarTo<float>(properties["freq_mul"]));
									shader.SetShaderParameter("fill_value", (progress - 50) * 2);
								}
								break;
						}
						ulong objId = instance.GetInstanceId();
						instance.SetScript(ProgressNodeScript);
						instance = (Node2D)InstanceFromId(objId);
						
					}
					break;
			}
			//detect whether physics or regular
			if (!(Event is editor.PhysicsElementEvent))
			{
				switch (belongs_to)//not physics body
				{

					default:
					case editor.Where.BaseLine:
						baseline_case.AddChild(instance);
						break;
					case editor.Where.Camera:
						camera_case.AddChild(instance);
						break;
					case editor.Where.General:
						general_case.AddChild(instance);
						break;
				}
				RegularInstances[uuid]=instance;
			}
			else
			{
				//turn instance into rigidbody
				var phy_e = (PhysicsElementEvent)Event;
				var body = new RigidBody2D();
				phy_e.rigid_body = body;
				if (phy_e.Hitbox.Shape is RectangleShape2D)
				{
					phy_e.Hitbox.Position+=((RectangleShape2D)phy_e.Hitbox.Shape).Size/2;
				}
				body.AddChild(phy_e.Hitbox);
				body.AddChild(instance);
				body.Position = phy_e.SpawnPos;
				body.Mass = phy_e.Mass;
				body.GravityScale = phy_e.GravityScale;
				body.LinearVelocity = phy_e.SpawnVelocity;
				body.AngularVelocity = phy_e.SpawnAngularVelocity;
				PhysicInstances[uuid]=body;
				
				foreach (int layer in phy_e.collision_layer)
				{
					body.SetCollisionLayerValue(layer,true);
				}

				foreach (int c_m in phy_e.collision_mask)
				{
					body.SetCollisionMaskValue(c_m,true);
				}
				var material = new PhysicsMaterial
				{
					Bounce = phy_e.Bounce,
					Friction = phy_e.Friction
				};
				body.PhysicsMaterialOverride = material;

				switch (belongs_to)
				{

					default:
					case editor.Where.BaseLine:
					case editor.Where.General:
						general_case.AddChild(body);
						break;
					case editor.Where.Camera:
						camera_case_physic.AddChild(body);
						break;
				}
			}
		}
		else
		{
			instance = (Node2D)Instances[uuid];
			if (Event.DeleteMode)
			{
				Instances[uuid]=null;
				if (instance!=null)
				{
					instance.QueueFree();
				}
				
			}
			if (element.type == ElementExplorer.ElementType.Progress)
			{
				instance.GetNode<Control>("Name").Visible = VarTo<bool>(properties["prog_on"]);
				instance.GetNode<Control>("Progress").Visible = VarTo<bool>(properties["name_on"]);

			}
		}
		var tween_props = Event.tweens;
		
			
		

	}
	void InvokeSpecialEvent(SpecialEvent Event)
	{
		var anim_player = Event.custom_action;
		AddChild(anim_player);
		anim_player.Play(anim_player.GetAnimationList()[0]);
	}
	void RemoveGeneralUnit(GeneralTextRemoveEvent Event)
	{
		var dur = Event.ease_duration;
		var amt = Event.amount;
		var units2remove = new HashSet<Control>();
		//tween the unit
		bool cond(int i)
		{
			if (Event.index_from_the_latest_one > 0)
			{
				return -Event.index_from_the_latest_one - i >= -GeneralTextSection.GetChildren().Count;
			}
			else
			{
				return Event.index_from_the_latest_one + i < GeneralTextSection.GetChildren().Count;
			}
		};
		int _i(int i)
		{
			if (Event.index_from_the_latest_one > 0)
			{
				return i;
			}
			else
			{
				return -i;
			}
		}

		for (int i = 0; i < amt && cond(i); i++)
		{
			var unit = GeneralTextSection.GetChild<Control>(-Event.index_from_the_latest_one - _i(i));
			GD.Print(unit);
			units2remove.Add(unit);
		}
		units2remove.All(unit =>
		{
			var ui=unit.GetChild<Control>(0);
			var Val = ui.GetNode<Marker2D>("_TweenedVal");
			var text_case = ui.GetNode<RichTextLabel>("TextCase");
			text_case.FitContent = false;
			Tween tween = GetTree().CreateTween().SetEase(Tween.EaseType.In);
			tween.TweenProperty(ui, "modulate", new Color(1, 1, 1, 0), dur).SetTrans(Tween.TransitionType.Linear);
			tween.SetParallel();
			tween.TweenProperty(Val, "position:x", 0, dur).SetDelay(0.02).SetTrans(Tween.TransitionType.Quad);
			tween.TweenProperty(text_case, "visible_ratio", 0, dur).SetDelay(0.1).SetTrans(Tween.TransitionType.Quad);
			tween.TweenCallback(Callable.From(() => { ui.QueueFree(); })).SetDelay(0.3);//queue free
			; return true;
		});

	}
	Hashtable Archive = new Hashtable();//{["archive name"]=tmovarchive}
	public class TMOVScene
	{
		public Control GeneralSection;//control general_section(duplicated)
		public Control PlotNode;//control plot_section(duplicated)
		public Hashtable Elements = new Hashtable();//hashtable all the elements  {id={instance,node path}}
	}
	void DoArchive(ArchiveEvent Event)
	{
		var mode = Event.Mode;
		var name = Event.ArchiveName;
		var scene = new TMOVScene();
		switch (mode)
		{

			default:
			case ArchiveEventMode.Save:
				{
					scene.GeneralSection = (Control)GeneralTextSection.Duplicate();
					scene.PlotNode = (Control)PlotSection.GetParent().Duplicate();
					foreach (DictionaryEntry item in RegularInstances)
					{
						DictionaryEntry instance = new DictionaryEntry { Key = item.Value, Value = ((Node)item.Value).GetParent().GetPath() };
						scene.Elements[(string)item.Key] = instance;
					}
					foreach (DictionaryEntry item in PhysicInstances)
					{
						DictionaryEntry instance = new DictionaryEntry { Key = item.Value, Value = ((Node)item.Value).GetParent().GetPath() };
						scene.Elements[(string)item.Key] = instance;
					}
					Archive[name] = scene;
				}
				break;
			case ArchiveEventMode.Load:
				{
					if (!Archive.ContainsKey(name))
					{
						break;
					}
					scene = (TMOVScene)Archive[name];
					GeneralTextSection.QueueFree();
					PlotSection.GetParent().QueueFree();
					HeightMeasurerGroup.GetChildren().All(n => { n.QueueFree(); return true; });
					GeneralTextSection = (Control)scene.GeneralSection.Duplicate();
					var plot_n = (Control)scene.PlotNode.Duplicate();
					PlotSection = plot_n.GetNode<RichTextLabel>("TextCase");
					BaseLine.AddChild(GeneralTextSection);
					BaseLine.AddChild(plot_n);
					foreach (DictionaryEntry n in RegularInstances)
					{
						((Node)n.Value).GetParent().RemoveChild(((Node)n.Value));
					}
					foreach (DictionaryEntry n in PhysicInstances)
					{
						((Node)n.Value).GetParent().RemoveChild(((Node)n.Value));
					}
					Instances.Clear();
					PhysicInstances.Clear();
					RegularInstances.Clear();
					foreach (DictionaryEntry item in scene.Elements)
					{
						var node = ((DictionaryEntry)item.Value).Key;
						Instances[item.Key] = node;
						GetTree().Root.GetNode((NodePath)((DictionaryEntry)item.Value).Value).AddChild((Node)node);
					}
				}
				break;
			case ArchiveEventMode.New:
				{
					foreach (DictionaryEntry n in RegularInstances)
					{
						((Node)n.Value).GetParent().RemoveChild(((Node)n.Value));
					}
					foreach (DictionaryEntry n in PhysicInstances)
					{
						((Node)n.Value).GetParent().RemoveChild(((Node)n.Value));
					}
					Instances.Clear();
					PhysicInstances.Clear();
					RegularInstances.Clear();
					GeneralTextSection.QueueFree();
					PlotSection.GetParent().QueueFree();
					HeightMeasurerGroup.GetChildren().All(n => { n.QueueFree(); return true; });
					GeneralTextSection = (Control)DefaultGeneralTextSection.Duplicate();
					var plot_n = (Control)DefaultPlotSection.Duplicate();
					PlotSection = plot_n.GetNode<RichTextLabel>("TextCase");
					BaseLine.AddChild(GeneralTextSection);
					BaseLine.AddChild(plot_n);
					Archive[name] = scene;
					PopupGeneralText(new GeneralTextEvent()
					{
						uid=-1,
					});
				}
				break;
		}
	}
	

	//Marker3D ShakeValue;
	void ShakeCamera(CameraShakeEvent Event)
	{
		var tween_props = Event.tweens;
		var amp=(List<object>)tween_props["Amplitude"];
		var amp_rot=(List<object>)tween_props["RotationAmp"];
		var freq=(List<object>)tween_props["Frequency"];
		var mul_x=(List<object>)tween_props["MultiplierX"];
		var mul_y=(List<object>)tween_props["MultiplierY"];
		var ShakeValue=CameraShaker.Values;
		var Mul=CameraShaker.Multipliers;
		//var props=new List<object>[]{ };
		
			if (amp==null || (bool)amp[4]==false)
			{
				ShakeValue.Position=new Vector3(Event.Amplitude,ShakeValue.Position.Y,ShakeValue.Position.Z);
			}
			if (amp_rot==null ||(bool)amp_rot[4]==false)
			{
				ShakeValue.Position=new Vector3(ShakeValue.Position.X,Event.RotationAmp,ShakeValue.Position.Z);
			}
			if (freq==null ||(bool)freq[4]==false)
			{
				ShakeValue.Position=new Vector3(ShakeValue.Position.X,ShakeValue.Position.Y,Event.Frequency);
			}
			if (mul_x==null ||(bool)mul_x[4]==false)
			{
				Mul.Position=new Vector2(Event.MultiplierX,Mul.Position.Y);
			}
			if (mul_y==null ||(bool)mul_y[4]==false)
			{
				Mul.Position=new Vector2(Mul.Position.X,Event.MultiplierY);
			}
		//GD.Print(Event.MultiplierX,Event.MultiplierY);
		do_tween(new List<object>[]{
			amp,amp_rot,freq,mul_x,mul_y
		},
		new object[] {
			ShakeValue,ShakeValue,ShakeValue,Mul,Mul
		},
		new string[] {"position:x","position:y","position:z","position:x","position:y"}
		,
		new object[] {
			Event.Amplitude,Event.RotationAmp,Event.Frequency,Event.MultiplierX,Event.MultiplierY
		});
	}
}
