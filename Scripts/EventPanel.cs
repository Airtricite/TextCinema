using Godot;
using Godot.Collections;
using System;
using Array = Godot.Collections.Array;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Collections;
using System.Numerics;
using Vector2 = Godot.Vector2;
public partial class EventPanel : Panel
{
	private Vector2 initialMousePos;
	private bool isDragging = false;
	private bool isEntered = false;
	public static long frame = 0;
	public static long iframe = 0;
	static int PhysicsTicksPerSecond = 60;
	LineEdit min;
	LineEdit sec;
	LineEdit fra;
	OptionButton event_selecting;
	public static PackedScene event_block;
	public static Node2D EventStream;
	static Window EventGroupWindow;
	Label scale_display;
	AudioStreamPlayer sfx;
	static PackedScene Mark;
	public override void _Ready()
	{
		PhysicsTicksPerSecond = Engine.PhysicsTicksPerSecond;
		min = GetNode<LineEdit>("min");
		sec = GetNode<LineEdit>("sec");
		fra = GetNode<LineEdit>("frame");
		event_selecting = GetNode<OptionButton>("new/EventSelecting");
		event_block = ResourceLoader.Load<PackedScene>("res://Scenes/event_block.tscn");
		EventStream = GetNode<Node2D>("../EventStream");
		scale_display = GetNode<Label>("../EventPanel/scale");
		sfx = GetNode<AudioStreamPlayer>("../SFX");
		physics_center = GetTree().Root.GetNode<Window>("Editor/EventPanel/PhysicsCenter");
		Mark = ResourceLoader.Load<PackedScene>("res://Scenes/Mark.tscn");
		editor.auto_set("event_stream");
		EventGroupWindow = GetTree().Root.GetNode<Window>("Editor/EventGroupEditor");
		ConvertFramesToTime(frame);
	}
	void Entered() { isEntered = true; }
	void Exited() { isEntered = false; }
	static float scale_index = 0;
	void ChangeScale(float amount)
	{

		scale = Mathf.Pow(2, scale_index + amount);

		if (scale < 0.001f)
		{
			scale = Mathf.Pow(2, scale_index);
		}
		else if (scale > 10)
		{
			scale = Mathf.Pow(2, scale_index);
		}
		else
		{
			scale_index += amount;
		}
		GD.Print("scale:" + scale);
		scale_display.Text = "scale:" + scale;
		UpdateStream();
	}
	public override void _PhysicsProcess(double delta)
	{
		if (editor.camera != null)
		{
			MousePos = DisplayServer.MouseGetPosition() + editor.camera.Position - ((Vector2)GetWindow().Size) / 2;
			if (editor.rect_selecting)
			{
				editor.selection_rect.Visible = true;
				editor.selection_rect.Position = initialMousePos;
				var dp = MousePos - initialMousePos;
				if (dp.X < 0)
				{
					editor.selection_rect.Position = new Vector2(MousePos.X, editor.selection_rect.Position.Y);

				}
				if (dp.Y < 0)
				{
					editor.selection_rect.Position = new Vector2(editor.selection_rect.Position.X, MousePos.Y);

				}
				editor.selection_rect.Size = dp.Abs();
			}

			if (playing)
			{
				frame += 1;
				if (editor.EventStream.Count > 0)
					editor.EventStream.Where(e => e.frame_stamp == frame).Select(e => e).All(e => { sfx.Play(); return true; });
				ConvertFramesToTime(frame);
			}
		}
	}
	Vector2 MousePos = Vector2.Zero;
	public override void _Input(InputEvent @event)
	{
		var pos = Vector2.Zero;
		if (@event is InputEventMouseMotion mouseMotion)
		{

			if (mouseMotion.ButtonMask == MouseButtonMask.Left && isEntered)
			{
				pos = mouseMotion.Position + editor.camera.Position - ((Vector2)GetWindow().Size) / 2;
				//MousePos = pos;
				if (!isDragging)
				{
					// Start dragging


					isDragging = true;
				}
				if (editor.pressing_ctrl)
				{


				}
				else //if(editor.SelectedEvents.Count<=0)
				{
					frame = iframe + (long)((initialMousePos.Y - pos.Y) / scale);
					//GD.Print(frame);
					ConvertFramesToTime(frame);
				}
			}
			else
			{
				// Stop dragging
				//iframe=frame;
				//isDragging = false;
			}
		}
		if (@event is InputEventMouseButton && isEntered)
		{
			InputEventMouseButton emb = (InputEventMouseButton)@event;
			if (emb.IsPressed())
			{
				if (editor.pressing_alt)
				{
					if (emb.ButtonIndex == MouseButton.WheelUp)
					{
						ChangeScale(0.15f);
					}
					if (emb.ButtonIndex == MouseButton.WheelDown)
					{
						ChangeScale(-0.15f);
					}
				}
				else
				{
					if (emb.ButtonIndex == MouseButton.WheelUp)
					{
						frame -= (long)(PhysicsTicksPerSecond / scale / 4);
						ConvertFramesToTime(frame);
					}
					if (emb.ButtonIndex == MouseButton.WheelDown)
					{
						frame += (long)(PhysicsTicksPerSecond / scale / 4);
						ConvertFramesToTime(frame);
					}
				}
				if (emb.ButtonIndex == MouseButton.Left)
				{
					EventGroupUnfocused();
					iframe = frame;
					isDragging = true;
					initialMousePos = MousePos;
				}
				if (editor.pressing_ctrl)
				{
					editor.rect_selecting = true;
				}
			}
			else
			{
				initialMousePos = MousePos;
				editor.selection_rect.Visible = false;
				isDragging = false;
				editor.rect_selecting = false;
			}
		}
		if (@event is InputEventKey)
		{
			InputEventKey key_event = (InputEventKey)@event;
			if (key_event.KeyLabel == Key.Ctrl && key_event.IsPressed())
			{
				editor.pressing_ctrl = true;
			}
			else if (key_event.KeyLabel == Key.Ctrl) { editor.pressing_ctrl = false; }
			if (key_event.KeyLabel == Key.Alt && key_event.IsPressed())
			{
				editor.pressing_alt = true;
			}
			else if (key_event.KeyLabel == Key.Alt) { editor.pressing_alt = false; }


			if (editor.pressing_ctrl)
			{
				//GD.Print(1);
				if (key_event.KeyLabel == Key.C)
				{//GD.Print(2);
					Copy();
				}
				if (key_event.KeyLabel == Key.V)
				{
					Paste();
				}
				if (key_event.KeyLabel == Key.S)
				{//GD.Print(2);
					editor.CallSavingDialog();
					
				}
			}
			else
			{
				editor.selection_rect.Visible = false;
				editor.rect_selecting = false;
			}
			
			/*if (isEntered)
			{
				if (key_event.KeyLabel==Key.Space)
				{
					Rhythm();
				}
			}*/

		}
	}
public static editor.Event FindEvent(int uid)
{
dynamic Event=null; 
		
		var main_list = editor.EventStream.Where(e => e.uid == (long)uid);
		if (main_list.Count()>0)
		{
			return main_list.First();
		}else if(EventPanel.SelectedGroup.Stream!=null){
			foreach (var item in EventPanel.SelectedGroup.Stream)
			{
				if (item.uid== (long)uid)
				{
					
					return item;
				}
			}
			}
			return null;

}
	public void ConvertTimeToFrames(string str)
	{
		min.ReleaseFocus();
		sec.ReleaseFocus();
		fra.ReleaseFocus();
		long m = 0; long s = 0; long f = 0;
		bool ParseSuccess = long.TryParse(min.Text, out m) && long.TryParse(sec.Text, out s) && long.TryParse(fra.Text, out f);
		if (ParseSuccess)
		{
			frame = 60 * m * PhysicsTicksPerSecond + s * PhysicsTicksPerSecond + f;
		}
		UpdateStream();
		//long m=
	}
	public static string TruncateString(string input, int maxLength)
	{
		// 检查输入是否为null或空
		if (string.IsNullOrEmpty(input))
			return input;

		// 如果字符串长度小于等于最大长度，直接返回原字符串
		if (input.Length <= maxLength)
			return input;

		// 否则截取前maxLength个字符并添加省略号
		return input.Substring(0, maxLength) + "...";
	}
	public static float scale = 1;
	public static editor.EventGroup SelectedGroup;
	static Hashtable GroupEntityMap = new Hashtable();
	static void UpdateEvent(editor.Event _e)
	{
		var name = _e.name;
		if (_e is editor.GeneralTextEvent && (_e.name == string.Empty || _e.name == ""))
		{
			name = ((editor.GeneralTextEvent)_e).Text;
		}
		if (_e is editor.PlotEvent && (_e.name == string.Empty || _e.name == ""))
		{
			name = ((editor.PlotEvent)_e).Text;
		}
		if (_e is editor.ElementEvent && (_e.name == string.Empty || _e.name == ""))
		{
			name = ((editor.ElementEvent)_e).uuid;
			if (((editor.ElementEvent)_e).PlaceHolderInfo!="")
			{
				name = "["+name+"]"+((editor.ElementEvent)_e).PlaceHolderInfo;
			}
		}
		_e.button.GetChild<RichTextLabel>(0).Text = "[center][" + _e.GetType().Name + "]\n" + name + "[/center]";
		//editor.Event _event=editor.FromDictionary<editor.Event>((Dictionary)_e);

		_e.Entity.Position = Vector2.Down * scale * _e.frame_stamp + Vector2.Right * _e.posX;
		// 计算总秒数
		decimal totalSeconds = (decimal)_e.frame_stamp / PhysicsTicksPerSecond;
		// 分解总秒数
		long minutes = (long)totalSeconds / 60;
		long remainingSeconds = (long)totalSeconds % 60;
		long remainingFrames = (long)(_e.frame_stamp % PhysicsTicksPerSecond);
		_e.Entity.GetChild<Label>(5).Text = minutes.ToString() + "'' " + remainingSeconds.ToString() + "' " + remainingFrames.ToString() + "f uid=" + _e.uid;
		_e.Entity.Modulate = _e.color;
		if (_e is editor.BPMMark)
		{
			var _em = (editor.BPMMark)_e;
			var marks = _em.Entity.GetNode("Marks").GetChildren();
			//.All(c=>{c.QueueFree(); return true;});

			for (int i = 0; i < _em.Amount; i++)
			{
				if (_em.Amount < 1)
				{
					_em.Amount = 1;
				}
				else if (_em.Amount > 65535)
				{
					_em.Amount = 65535;
				}
				if (_em.div < 1)
				{
					_em.div = 1;
				}
				else if (_em.div > 32)
				{
					_em.div = 32;
				}
				if (_em.BPM < 1)
				{
					_em.BPM = 1;
				}
				else if (_em.BPM > 1024)
				{
					_em.BPM = 1024;
				}
				var second_per_bar = 1 / (_em.BPM / 60) * 4;
				var sec_per_mark = second_per_bar / _em.div;
				var m_framestamp = (long)(sec_per_mark * 60 * i);

				if (i > marks.Count - 1)
				{
					var _m = Mark.Instantiate<Node2D>();
					marks.Add(_m);
					_e.Entity.GetNode("Marks").AddChild(_m);
				}
				if (_em.Amount < marks.Count - 1)
				{
					//	GD.Print("_em.Amount<marks.Count-1");
					for (int j = marks.Count - 1; j >= _em.Amount; j--)
					{
						_em.Entity.GetNode("Marks").GetChild(j).QueueFree();
					}

				}
				var m = (Node2D)marks.ElementAt(i);
				if (i % _em.div != 0)
				{
					m.Modulate = new Color(m.Modulate.R, m.Modulate.G, m.Modulate.B, 0.3f);
				}
				else
				{
					m.Modulate = new Color(m.Modulate.R, m.Modulate.G, m.Modulate.B, 1);
				}
				//Mark.Instantiate<Node2D>();
				m.Position = Vector2.Down * scale * m_framestamp;
				m.Scale = Vector2.One * _em.scale;


			}
		}
	}
	public static void UpdateStream()//turn visibility
	{
		EventStream.GlobalPosition = Vector2.Down * scale * -frame;
		
		foreach (editor.Event _e in CollectionsMarshal.AsSpan(editor.EventStream.ToList()))
		{
			
			UpdateEvent(_e);
		
		}
		if (SelectedGroup != null && EventGroupWindow.Visible)
		{
			// display events	
			foreach (var item in SelectedGroup.Stream)
			{

				if (!editor.SelectedEvents.Contains(item))
				{
					item.Entity.GetChild<ReferenceRect>(0).Visible = false;
				}
				item.frame_stamp = Math.Clamp(item.frame_stamp, 0, 600);
				item.posX = Math.Clamp(item.posX, -450, 450);
				UpdateEvent(item);
				//update props,clamp time and posx
			}
		}
		foreach (editor.Event _e in editor.SelectedEvents)
		{
			//GD.PrintErr(_e);
			//if (_e!=null)
			//{
				_e.Entity.GetChild<ReferenceRect>(0).Visible = true;
			//}
			

		}
		if (editor.EventStream.Count > 0)
			editor.EventStream.Where(e => !editor.SelectedEvents.Contains(e)).Select(e => e)
			.All(e => { e.Entity.GetChild<ReferenceRect>(0).Visible = false; return true; });



		if (frame <= -3600)
		{
			frame = -3600;
		}
	}
	public void ConvertFramesToTime(long nFrames)
	{
		// 计算总秒数
		decimal totalSeconds = (decimal)nFrames / PhysicsTicksPerSecond;

		// 分解总秒数
		long minutes = (long)totalSeconds / 60;
		long remainingSeconds = (long)totalSeconds % 60;
		long remainingFrames = (long)(nFrames % PhysicsTicksPerSecond);
		min.Text = minutes.ToString();
		sec.Text = remainingSeconds.ToString();
		fra.Text = remainingFrames.ToString();
		UpdateStream();
		//GD.Print($"转换结果：{minutes} 分钟 {remainingSeconds} 秒 {remainingFrames} 帧");
	}
	void OnSelectNewEvent(int item_index)
	{
		var _e = NewEvent(item_index);
		ControlWindow._e = _e;
		ControlWindow.Edit();
		ControlWindow.Apply();
		editor.select_event(_e.uid);
	}

	static bool isGroupSelected()
	{
		//var window=editor.root.GetNode<Window>("EventGroupEditor");
		return focused && SelectedGroup != null && EventGroupWindow.Visible;
	}

	editor.Event NewEvent(int item_index)
	{
		HashSet<editor.Event> dest = null;
		if (isGroupSelected())
		{
			dest = SelectedGroup.Stream;
		}
		//GD.Print(dest);
		return _NewEvent(item_index, dest);
	}
	editor.Event _NewEvent(int item_index, HashSet<editor.Event> stream)
	{
		//HashSet<Hashtable> event_stream = (HashSet<Hashtable>)editor.ProjectData["event_stream"];
		//case
		//
		editor.Event new_event = new editor.Event();
		
		
		
		switch (item_index)
		{
			case 0:
				new_event = new editor.GeneralTextEvent();
				((editor.GeneralTextEvent)new_event).Text = "";
				((editor.GeneralTextEvent)new_event).TextSize = 44;
				((editor.GeneralTextEvent)new_event).PopupDuration = 0.25f;
				((editor.GeneralTextEvent)new_event).HorizontalPadding = 0;
				break;
			case 1:
				new_event = new editor.PlotEvent();
				((editor.PlotEvent)new_event).Text = "";
				((editor.PlotEvent)new_event).TextSize = 36;
				((editor.PlotEvent)new_event).WriteSpeed = 10;
				((editor.PlotEvent)new_event).UseDefaultStyle = false;
				break;
			case 2:
				{
					new_event = new editor.ElementEvent();
					var props = new Hashtable();
					props["pos"] = Vector2.Zero;
					props["rot"] = 0f;
					props["scale"] = Vector2.One;
					props["offset"] = Vector2.Zero;
					props["size"] = Vector2.One * 250;
					props["z_index"] = 0;
					((editor.ElementEvent)new_event).PlaceHolderInfo="";
					((editor.ElementEvent)new_event).element_props = props;
					((editor.ElementEvent)new_event).belongs_to = editor.Where.General;
					((editor.ElementEvent)new_event).BackgroundColor=new Color(0.9f,0.9f,0.9f,1);
					((editor.ElementEvent)new_event).DeleteMode = false;
					((editor.ElementEvent)new_event).ContentColor = new Color(1, 1, 1, 1);
				}
				break;
			case 3:
				new_event = new editor.PhysicsEvent();
				((editor.PhysicsEvent)new_event).GravityDirection = 90;
				((editor.PhysicsEvent)new_event).Magnitude = 980;
				break;
			case 4:
				{
					new_event = new editor.PhysicsElementEvent();
					var props = new Hashtable();
					props["pos"] = Vector2.Zero;
					props["rot"] = 0f;
					props["scale"] = Vector2.One;
					props["offset"] = Vector2.Zero;
					props["size"] = Vector2.One * 250;
					props["z_index"] = 0;
					((editor.ElementEvent)new_event).PlaceHolderInfo="";
					((editor.ElementEvent)new_event).element_props = props;
					((editor.ElementEvent)new_event).belongs_to = editor.Where.General;
					((editor.ElementEvent)new_event).BackgroundColor=new Color(0.9f,0.9f,0.9f,1);
					((editor.ElementEvent)new_event).DeleteMode = false;
					((editor.PhysicsElementEvent)new_event).Hitbox = new CollisionShape2D();
					((editor.PhysicsElementEvent)new_event).Mass = 1;
					((editor.PhysicsElementEvent)new_event).Friction = 1;
					((editor.PhysicsElementEvent)new_event).Bounce = 0.2f;
					((editor.PhysicsElementEvent)new_event).GravityScale = 1;
					((editor.PhysicsElementEvent)new_event).collision_layer = new HashSet<int>();
					((editor.PhysicsElementEvent)new_event).collision_mask = new HashSet<int>();
				}
				break;
			case 5:
				new_event = new editor.GUIEvent();
				((editor.GUIEvent)new_event).BasePosition = new Vector2(0, 160);
				((editor.GUIEvent)new_event).BaseRotation = 0;
				//
				((editor.GUIEvent)new_event).GeneralPos = new Vector2(-720, -2040);
				((editor.GUIEvent)new_event).GeneralRot = 0;
				((editor.GUIEvent)new_event).GeneralSize = new Vector2(1440, 2000);
				((editor.GUIEvent)new_event).GeneralPivotOffset = new Vector2(0, 0);
				//
				((editor.GUIEvent)new_event).PlotPos = new Vector2(-780, 40);
				((editor.GUIEvent)new_event).PlotRot = 0;
				((editor.GUIEvent)new_event).PlotSize = new Vector2(1560, 190);
				((editor.GUIEvent)new_event).PlotPivotOffset = new Vector2(0, 0);
				//
				((editor.GUIEvent)new_event).CameraPosition = new Vector2(0, 0);
				((editor.GUIEvent)new_event).CameraRotation = 0;
				((editor.GUIEvent)new_event).CameraZoom = new Vector2(1, 1);
				break;
			case 6:
				new_event = new editor.GeneralTextRemoveEvent();
				((editor.GeneralTextRemoveEvent)new_event).index_from_the_latest_one = 1;
				((editor.GeneralTextRemoveEvent)new_event).amount = 1;
				((editor.GeneralTextRemoveEvent)new_event).ease_duration = 0.25f;
				break;
			case 7:
				new_event = new editor.Mark();
				((editor.Mark)new_event).Shape = editor._shape.Diamond;
				((editor.Mark)new_event).scale = 1;
				break;
			case 8:
				new_event = new editor.BPMMark();
				((editor.BPMMark)new_event).BPM = 120;
				((editor.BPMMark)new_event).div = 4;
				((editor.BPMMark)new_event).Amount = 4;
				((editor.BPMMark)new_event).scale = 1;
				break;
			case 9:
				new_event = new editor.ArchiveEvent();
				((editor.ArchiveEvent)new_event).ArchiveName = "";
				((editor.ArchiveEvent)new_event).Mode = editor.ArchiveEventMode.Save;
				break;
			case 10:
				new_event = new editor.SpecialEvent();
				break;
			case 11:
				new_event = new editor.CameraShakeEvent();
				{
					((editor.CameraShakeEvent)new_event).Amplitude = 30;
					((editor.CameraShakeEvent)new_event).RotationAmp = 1;
					((editor.CameraShakeEvent)new_event).Frequency = 10;
					((editor.CameraShakeEvent)new_event).MultiplierX=1;
					((editor.CameraShakeEvent)new_event).MultiplierY=1;
				}
				break;
			case 12:
				new_event = new editor.EventGroup();
				{
					((editor.EventGroup)new_event).VarString="";
				}
				break;
		}
		new_event.uid = editor.assign_uid();
		new_event.frame_stamp = frame;
		new_event.posX = 0;
		new_event.type = item_index;
		new_event.color = new Color(1, 1, 1, 1);
		switch (item_index)
		{
			case 2:
			case 4:
				{((editor.ElementEvent)new_event).uuid = "u"+new_event.uid;
				
				}
				break;
		}
		editor.GlobalEventStream.Add(new_event);

		new_event.Entity = event_block.Instantiate<Node2D>();
		if (new_event is editor.Mark)
		{
			// add marker 
			var mark = Mark.Instantiate<Node2D>();
			new_event.Entity.GetNode("Marks").AddChild(mark);
		}
		new_event.Entity.GetNode<Marker2D>("uid").Position = Vector2.Right * new_event.uid;
		new_event.button = new_event.Entity.GetNode<Button>("EventBlock");
		new_event.name = "";
		if (stream != null && stream != editor.EventStream)
		{
			stream.Add(new_event);
			if (SelectedGroup!=null &&stream == SelectedGroup.Stream)
			{
				EventGroupWindow.GetNode("EventStream").AddChild(new_event.Entity);
			}
		}
		else
		{
			editor.EventStream.Add(new_event);
			EventStream.AddChild(new_event.Entity);
		}

		//GD.Print(editor.ToDictionary(new_event));
		//event_stream.Add(new_event.ToDict());
		
		
		UpdateStream();
		return new_event;
	}
	static bool focused = false;
	void EventGroupFocused()
	{
		editor.SelectedEvents.Clear();
		var rect = editor.root.GetNode<ReferenceRect>("EventGroupEditor/ReferenceRect");
		rect.Visible = true;
		focused = true;
	}
	void CloseGroupEditor()
	{
		editor.SelectedEvents.Clear();
		EventGroupWindow.Visible = false;
	}
	void EventGroupUnfocused()
	{
		var rect = editor.root.GetNode<ReferenceRect>("EventGroupEditor/ReferenceRect");
		rect.Visible = false;
		focused = false;
	}
	void Delete()
	{
		foreach (editor.Event _e in editor.SelectedEvents)
		{
			//var event_stream = (HashSet<Hashtable>)editor.ProjectData["event_stream"];
			//if (event_stream.Count > 0)
			//	event_stream.Where(n => (long)n["uid"] == _e.uid).Select(n => n).All(n => { event_stream.Remove(n); return true; });

			_e.Entity.Visible = false;
			editor.RecycledEvents.Add(_e);

			void _delete_event(HashSet<editor.Event> stream)
			{
				foreach (var e in stream)
				{
					if (e == _e)
					{
						stream.Remove(_e);
					}
					else if (e is editor.EventGroup)
					{
						_delete_event(((editor.EventGroup)e).Stream);
					}
				}

			}
			editor.UpdateBin();
			//editor.EventStream.Remove(_e);
			_delete_event(editor.EventStream);
		}

		editor.SelectedEvents.Clear(); editor.UpdateBin();
	}
	void Copy()
	{
		editor.CopiedEvents.Clear();
		foreach (editor.Event _e in editor.SelectedEvents)
		{
			editor.CopiedEvents.Add(_e);
		}
	}
	editor.Event CloneEvent(editor.Event Event, HashSet<editor.Event> stream)
	{

		var new_e = _NewEvent(Event.type, stream);
		//CopyProperties(_e,new_e);//new_e.uid=editor.assign_uid();
		Type t = Event.GetType(); PropertyInfo[] properties = t.GetProperties();
		if (Event is editor.EventGroup)
		{
			foreach (var item in ((editor.EventGroup)Event).Stream)
			{
				var _new_e=CloneEvent(item, ((editor.EventGroup)new_e).Stream);
				_new_e.frame_stamp = item.frame_stamp;
			}
		}
		foreach (var property in properties)
		{

			if (property.CanRead && property.CanWrite)
			{
				object value = property.GetValue(Event);
				if (property.Name != "uid" && property.Name != "Entity" && property.Name != "button" && property.Name != "Stream")
				{
					GD.Print(property.Name);
					new_e.GetType().GetProperty(property.Name).SetValue(new_e, value);
				}
				
			}
		}
		return new_e;
	}
	void Paste()
	{
		if (editor.CopiedEvents.Count <= 0)
			return;
		var first = editor.CopiedEvents.OrderBy(_e => _e.frame_stamp).First();
		if (first != null)
		{
			HashSet<editor.Event> cache = new HashSet<editor.Event>();
			//var event_stream = (HashSet<Hashtable>)editor.ProjectData["event_stream"];
			foreach (var _e in editor.CopiedEvents)
			{
				dynamic dest = null;
				if (isGroupSelected())
				{
					dest = SelectedGroup.Stream;
				}
				var new_e = CloneEvent(_e, dest);//if sub stream window then copy to the sub stream
				new_e.frame_stamp = frame + _e.frame_stamp - first.frame_stamp;
				cache.Add(new_e);
			}
			editor.SelectedEvents.Clear();
			foreach (var item in cache)
			{
				editor.SelectedEvents.Add(item);
			}
		}
		UpdateStream();
	}
	bool playing = false;
	void Rhythm()
	{
		//GD.Print(1);
		playing = !playing;
	}
	void TestPlay()
	{

		if (editor.path_cache != null && editor.path_cache != string.Empty && editor.path_cache != "")
		{
			editor.Save(editor.path_cache + "_test");
			cinema.tmov_path = editor.path_cache + "_test";
			cinema.frame = frame;
			cinema.is_test_playing = true;
			cinema.placeholder_on=GetNode<CheckBox>("TestPlay/PlaceHolder").ButtonPressed;
			cinema.show_back_button = !GetNode<CheckBox>("TestPlay/Hide").ButtonPressed;
			editor._Clear();
			
			GetTree().ChangeSceneToFile("res://Scenes/cinema.tscn");
		}
		else
		{
			editor.CallSavingDialog();
		}

	}
	static Window physics_center;
	void EditPhysics()
	{
		physics_center.Popup();
		PhysicsCenter.edit();
	}
	void EditSpecialEvent(string path)
	{
		if (path.GetExtension() == ".tscn")
		{
			AnimationPlayer anim;
			try
			{
				anim = ResourceLoader.Load<PackedScene>(path).Instantiate<AnimationPlayer>();
			}
			catch (System.Exception)
			{
				return;
			}

			var uid = (long)GetParent().GetNode<Marker2D>("PropertyPanel/current_uid").Position.X;
			if (editor.EventStream.Count <= 0)
			{
				return;
			}
			var _event = editor._select_event(uid);
			if (_event is editor.SpecialEvent)
			{
				((editor.SpecialEvent)_event).custom_action = anim;
			}
		}
	}
}

