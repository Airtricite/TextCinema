using Godot;
using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Reflection;
using System.Text;

public partial class ElementExplorer : Panel
{
	// Called when the node enters the scene tree for the first time.
	public static PackedScene element_block;
	public static Control Grid;
	FileDialog Dialog;
	public static bool WriteTransformOnly=false;
	public override void _Ready()
	{
		element_block = ResourceLoader.Load<PackedScene>("res://Scenes/element.tscn");
		Grid = GetNode<Control>("Scroll/GridContainer");
		edit_win = GetNode<Window>("EditWindow");
		Dialog = GetNode<FileDialog>("FileDialog");
		picker = GetTree().Root.GetNode<Window>("Editor/ColorPicker");
		GetViewport();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
	public static void Update()
	{
		foreach (var e in editor.Elements)
		{
			e.entity.GetNode<ReferenceRect>("ReferenceRect").Visible = false;
			var typename = "";
			switch (e.type)
			{

				default:
				case ElementType.Image: typename = "🖼Image"; break;
				case ElementType.Animation: typename = "🎞Animation"; break;
				case ElementType.Video: typename = "📽Video"; break;
				case ElementType.Text: typename = "📃Text"; break;
				case ElementType.Custom: typename = "Custom"; break;
				case ElementType.Progress: typename = "ProgressBar"; break;
			}
			e.entity.Text = "["+e.euid+"]"+"[" + typename + "]\n" + e.Name;
		}
		foreach (var e in editor.SelectedElements)
		{
			e.entity.GetNode<ReferenceRect>("ReferenceRect").Visible = true;
		}
	}
	public enum ElementType
	{
		Image,
		Animation,
		Video,
		Text,
		Custom,
		Progress,
	}
	void NewElementPressed(int id)
	{
		var type = ElementType.Image;
		switch (id)
		{

			default:
			case 0: type = ElementType.Image; break;
			case 1: type = ElementType.Animation; break;
			case 2: type = ElementType.Video; break;
			case 3: type = ElementType.Text; break;
			case 4: type = ElementType.Custom; break;
			case 5: type = ElementType.Progress; break;
		}
		NewElement(editor.assign_euid(), type);

	}
	editor.Element NewElement(long euid, ElementType type)
	{
		//editor.auto_set("elements");
		//HashSet<Hashtable> elements=(HashSet<Hashtable>)editor.ProjectData["elements"];
		//case
		//
		editor.Element new_element = new editor.Element
		{
			type = type,
			euid = euid,
			entity = element_block.Instantiate<Button>(),
			Name = ""
		};
		new_element.entity.GetNode<Marker2D>("euid").Position = Vector2.Right * new_element.euid;
		switch (new_element.type)
		{
			default:
			case ElementType.Image:
				new_element.Node = ResourceLoader.Load<PackedScene>("res://Scenes/image_node.tscn").Instantiate<Node2D>();
				break;
			case ElementType.Animation:
				new_element.Node = ResourceLoader.Load<PackedScene>("res://Scenes/anim_node.tscn").Instantiate<Node2D>();
				break;
			case ElementType.Video:
				new_element.Node = ResourceLoader.Load<PackedScene>("res://Scenes/video_node.tscn").Instantiate<Node2D>();
				break;
			case ElementType.Text:
				new_element.Node = ResourceLoader.Load<PackedScene>("res://Scenes/text_node.tscn").Instantiate<Node2D>();
				break;
			case ElementType.Custom://new_element.Node=ResourceLoader.Load<PackedScene>("res://Scenes/custom_node.tscn").Instantiate<Node2D>();
				break;
			case ElementType.Progress:
				new_element.Node = ResourceLoader.Load<PackedScene>("res://Scenes/progress_node.tscn").Instantiate<Node2D>();
				break;
		}


		editor.Elements.Add(new_element);
		Grid.AddChild(new_element.entity);
		//GD.Print(editor.ToDictionary(new_event));
		//elements.Add(new_element.ToDict());
		new_element.PrewriteNodeData();
		editor.select_element(euid);
		EditElement();
		Update();
		//UpdatePreview();
		return new_element;
	}
	static Window edit_win;
	public static void WriteFrame()
	{
		if (editor.Elements.Count <= 0 || editor.EventStream.Count <= 0) return;

		var _Event =editor._select_event((long)CinemaPreview.id_instance.Position.X);
		editor.ElementEvent Event=null;
		if (!(_Event is editor.ElementEvent))
		{
			return;
		}else{
			Event=(editor.ElementEvent)_Event;
		}
		long euid = Event.element.euid;

		//element events

		var element = editor.Elements.Where(e => e.euid == euid).First();

		//Event.element_props["pos"]=element.Node.Position;
		//Event.element_props["rot"]=;
		//Event.element_props["scale"]=;
		CinemaPreview.ApplyTransform(true);
		if (!WriteTransformOnly)
		{
			
		
		Event.element_props["z_index"] = element.Node.ZIndex;
		switch (element.type)//Z INDEX
		{

			default:
			case ElementType.Image://no
				break;
			case ElementType.Animation://speed
				Event.element_props["speed_scale"] = ((AnimatedSprite2D)element.Node).SpeedScale;
				break;
			case ElementType.Video://no
				break;
			case ElementType.Text://visible ratio
				Event.element_props["visible_ratio"] = ((RichTextLabel)element.Node.GetChild(0)).VisibleRatio;
				break;
			case ElementType.Custom://no
				break;
			case ElementType.Progress://namepos/progress hint pos/progress/prog on/name on/
									  //boarder color/fill color//boarder Width/
									  //
				Event.element_props["name_pos"] = element.Node.GetNode<Control>("Name").Position;
				Event.element_props["prog_pos"] = element.Node.GetNode<Control>("Progress").Position;
				Event.element_props["progress"] = (float)element.Node.GetMeta("Progress");
				Event.element_props["prog_on"] = element.Node.GetNode<Control>("Name").Visible;
				Event.element_props["name_on"] = element.Node.GetNode<Control>("Progress").Visible;
				//GD.PrintErr("uid: "+Event.uid);
				var _t = element.Node.GetMeta("Type");
				switch ((int)_t)
				{
					default:
					case 0://Event.element_props["border_color"];
						{
							var progress_node = element.Node.GetNode<Control>("Bar");
							Event.element_props["border_color"] = progress_node.SelfModulate;
							Event.element_props["fill_color"] = progress_node.GetNode<Control>("Fill").SelfModulate;
							StyleBoxFlat style = (StyleBoxFlat)((Panel)progress_node).GetThemeStylebox("Panel");
							Event.element_props["border_width"] = style.GetBorderWidthMin();
						}
						break;
					case 1:
						{
							var progress_node = element.Node.GetNode<Control>("Ring");


							var shader = (ShaderMaterial)progress_node.Material;
							Event.element_props["radius"] = (float)shader.GetShaderParameter("radius");
							Event.element_props["hollow_radius"] = (float)shader.GetShaderParameter("hollow_radius");
							Event.element_props["segments"] = (float)shader.GetShaderParameter("segments");
							Event.element_props["margin"] = (float)shader.GetShaderParameter("margin");
							Event.element_props["rotation"] = (float)shader.GetShaderParameter("rotation");
							Event.element_props["color"] = (Color)shader.GetShaderParameter("color");
						}
						break;
					case 2:
						{
							var progress_node = element.Node.GetNode<Control>("Wave");
							var shader = (ShaderMaterial)progress_node.Material;
							Event.element_props["back_wave_color"] = (Color)shader.GetShaderParameter("backFillColour");
							Event.element_props["front_wave_color"] = (Color)shader.GetShaderParameter("frontFillOuterColour");
							Event.element_props["fill_color"] = (Color)shader.GetShaderParameter("fillcolour");
							Event.element_props["ring_color"] = (Color)shader.GetShaderParameter("ringColour");
							Event.element_props["ring_width"] = (float)shader.GetShaderParameter("ringWidth");
							Event.element_props["inner_offset"] = (float)shader.GetShaderParameter("innerCircleRadiusOffset");
							Event.element_props["amp_mul"] = (float)shader.GetShaderParameter("amp_mul");
							Event.element_props["freq_mul"] = (float)shader.GetShaderParameter("freq_mul");

							break;
						}
				}

				break;

		}
		}
		CloseAll();

		//
	}
	static void CloseAll()
	{
			edit_win.GetNode<Panel>("ImageEdit").Visible = false;
			edit_win.GetNode<Panel>("AnimEdit").Visible = false;
			edit_win.GetNode<Panel>("VideoEdit").Visible = false;
			edit_win.GetNode<Panel>("TextEdit").Visible = false;
			edit_win.GetNode<Panel>("CustomEdit").Visible = false;
			edit_win.GetNode<Panel>("ProgressEdit").Visible = false;
	}
	void EditElementDirectly()
	{
		if (editor.SelectedElements.Count > 0)
		{
		editor.SelectedElements.Clear();
		}
		if (editor.SelectedEvents.Count>0)
		{
			var _e=editor.SelectedEvents.First();
			if (_e is editor.ElementEvent && ((editor.ElementEvent)_e).element!=null)
			{
				editor.SelectedElements.Add(((editor.ElementEvent)_e).element);
				EditElement();
			}
		
		}
	}
	void EditElement()
	{

		if (editor.SelectedElements.Count > 0)
		{
			edit_win.Popup();
			var selected = editor.SelectedElements.First();

			edit_win.GetNode<LineEdit>("Name").Text = selected.Name;

			if (selected.Node != null)
			{ edit_win.GetNode<SpinBox>("ZIndex").Value = selected.Node.ZIndex; }

			CloseAll();

			int mode2option(TextureRect.StretchModeEnum stretchMode)
			{
				switch (stretchMode)
				{

					default:
					case TextureRect.StretchModeEnum.Scale:
						return 0;
					case TextureRect.StretchModeEnum.KeepAspectCentered:
						return 1;
					case TextureRect.StretchModeEnum.Tile:
						return 2;
				}
			}
			int blend2option(CanvasItemMaterial.BlendModeEnum Mode)
			{
				switch (Mode)
				{

					default:
					case CanvasItemMaterial.BlendModeEnum.Add:
						return 1;
					case CanvasItemMaterial.BlendModeEnum.Mix:
						return 0;
					case CanvasItemMaterial.BlendModeEnum.Mul:
						return 3;
						case CanvasItemMaterial.BlendModeEnum.PremultAlpha:
						return 4;
						case CanvasItemMaterial.BlendModeEnum.Sub:
						return 2;
				}
			}
			switch (selected.type)
			{
				default:
				case ElementType.Image:
					{
						var edit = edit_win.GetNode<Panel>("ImageEdit"); edit.Visible = true;
						var prev = edit.GetNode<TextureRect>("Preview");
						GD.Print("[selected.FrameData.Count]:"+selected.FrameData.Count);
						if (selected.FrameData.Count>0)
						{
							var tex = new PortableCompressedTexture2D();
							tex._Data = (byte[])selected.FrameData.ElementAt(0).Clone();
							
							tex.KeepCompressedBuffer = false;
							prev.Texture=tex;
						}
							


						var img = selected.Node.GetChild<TextureRect>(0);
						LineEdit OffsetX = edit_win.GetNode<LineEdit>("ImageEdit/OffsetX");
						LineEdit OffsetY = edit_win.GetNode<LineEdit>("ImageEdit/OffsetY");
						LineEdit SizeX = edit_win.GetNode<LineEdit>("ImageEdit/SizeX");
						LineEdit SizeY = edit_win.GetNode<LineEdit>("ImageEdit/SizeY");
						LineEdit ScaleX = edit_win.GetNode<LineEdit>("ImageEdit/ScaleX");
						LineEdit ScaleY = edit_win.GetNode<LineEdit>("ImageEdit/ScaleY");
						OptionButton stretch = edit_win.GetNode<OptionButton>("ImageEdit/StretchMode");
						OptionButton blend= edit_win.GetNode<OptionButton>("ImageEdit/BlendMode");
						CheckBox pixel = edit_win.GetNode<CheckBox>("ImageEdit/PixelMode");
						OffsetX.Text = img.Position.X.ToString();
						OffsetY.Text = img.Position.Y.ToString();
						SizeX.Text = img.Size.X.ToString();
						SizeY.Text = img.Size.Y.ToString();
						ScaleX.Text = selected.Node.Scale.X.ToString();
						ScaleY.Text = selected.Node.Scale.Y.ToString();
						stretch.Selected = mode2option(img.StretchMode);

						pixel.ButtonPressed = img.TextureFilter == TextureFilterEnum.Nearest;
						blend.Selected = blend2option(((CanvasItemMaterial)selected.Node.Material).BlendMode);
					}
					break;
				case ElementType.Animation:
					{
						var edit = edit_win.GetNode<Panel>("AnimEdit"); edit.Visible = true;
						var prev = edit.GetNode<AnimatedSprite2D>("Preview");
						prev.SpriteFrames = (SpriteFrames)((AnimatedSprite2D)selected.Node).SpriteFrames.Duplicate();
						prev.SpriteFrames.Clear("default");
						foreach (var frame in selected.FrameData)
						{
							var tex = new PortableCompressedTexture2D();
							tex._Data = (byte[])frame.Clone();
							GD.Print(tex);
							tex.KeepCompressedBuffer = false;
							prev.SpriteFrames.AddFrame("default", tex);
						}
						LineEdit spd = edit_win.GetNode<LineEdit>("AnimEdit/Speed");
						LineEdit fps = edit_win.GetNode<LineEdit>("AnimEdit/FPS");
						LineEdit scale_x = edit_win.GetNode<LineEdit>("AnimEdit/ScaleX");
						LineEdit scale_y = edit_win.GetNode<LineEdit>("AnimEdit/ScaleY");
						CheckBox pixel = edit_win.GetNode<CheckBox>("AnimEdit/PixelMode");
						OptionButton blend= edit_win.GetNode<OptionButton>("ImageEdit/BlendMode");
						pixel.ButtonPressed = ((AnimatedSprite2D)selected.Node).TextureFilter == TextureFilterEnum.Nearest;
						blend.Selected = blend2option(((CanvasItemMaterial)((AnimatedSprite2D)selected.Node).Material).BlendMode);
						spd.Text = ((AnimatedSprite2D)selected.Node).SpeedScale.ToString();
						fps.Text = ((AnimatedSprite2D)selected.Node).SpriteFrames.GetAnimationSpeed("default").ToString();
						scale_x.Text = ((AnimatedSprite2D)selected.Node).Scale.X.ToString();
						scale_y.Text = ((AnimatedSprite2D)selected.Node).Scale.Y.ToString();
						prev.Play();
					}
					break;
				case ElementType.Video:
					{
						var edit = edit_win.GetNode<Panel>("VideoEdit"); edit.Visible = true;
						var prev = edit.GetNode<VideoStreamPlayer>("Preview");
						var vid_player = selected.Node.GetNode<VideoStreamPlayer>("Video");
						prev.Stream = vid_player.Stream;
						LineEdit OffsetX = edit_win.GetNode<LineEdit>("VideoEdit/OffsetX");
						LineEdit OffsetY = edit_win.GetNode<LineEdit>("VideoEdit/OffsetY");
						LineEdit SizeX = edit_win.GetNode<LineEdit>("VideoEdit/SizeX");
						LineEdit SizeY = edit_win.GetNode<LineEdit>("VideoEdit/SizeY");
						LineEdit ScaleX = edit_win.GetNode<LineEdit>("VideoEdit/ScaleX");
						LineEdit ScaleY = edit_win.GetNode<LineEdit>("VideoEdit/ScaleY");

						CheckBox loop = edit_win.GetNode<CheckBox>("VideoEdit/Loop");
						CheckBox expand = edit_win.GetNode<CheckBox>("VideoEdit/Expand");

						OffsetX.Text = vid_player.Position.X.ToString();
						OffsetY.Text = vid_player.Position.Y.ToString();
						SizeX.Text = vid_player.Size.X.ToString();
						SizeY.Text = vid_player.Size.Y.ToString();
						ScaleX.Text = selected.Node.Scale.X.ToString();
						ScaleY.Text = selected.Node.Scale.Y.ToString();

						loop.ButtonPressed = vid_player.Loop;
						expand.ButtonPressed = vid_player.Expand;

						prev.Play();
					}
					break;
				case ElementType.Text:
					{
						var edit = edit_win.GetNode<Panel>("TextEdit"); edit.Visible = true;
						var prev = edit.GetNode<TextEdit>("Preview");
						prev.Text = selected.Node.GetNode<RichTextLabel>("Text").Text;

					}
					break;
				case ElementType.Custom:
					edit_win.GetNode<Panel>("CustomEdit").Visible = true;
					break;
				case ElementType.Progress:
					{
						var edit = edit_win.GetNode<Panel>("ProgressEdit"); edit.Visible = true;
						var prev = edit.GetNode<Node2D>("PreviewWindow/Preview");

						var node = selected.Node;
						//instantiate
						Control progress_node;
						var _t = (int)node.GetMeta("Type");
						node.GetNode<Control>("Bar").Visible = false;
						node.GetNode<Control>("Ring").Visible = false;
						node.GetNode<Control>("Wave").Visible = false;

						switch (_t)
						{

							default:
							case 0:
								progress_node = node.GetNode<Control>("Bar");
								break;
							case 1:
								progress_node = node.GetNode<Control>("Ring");
								break;
							case 2:
								progress_node = node.GetNode<Control>("Wave");
								break;
						}
						progress_node.Visible = true;
						var name_label = node.GetNode<Label>("Name");
						var prog_label = node.GetNode<Label>("Progress");

						SpinBox OffsetX = edit.GetNode<SpinBox>("OffsetX");
						SpinBox OffsetY = edit.GetNode<SpinBox>("OffsetY");
						SpinBox SizeX = edit.GetNode<SpinBox>("SizeX");
						SpinBox SizeY = edit.GetNode<SpinBox>("SizeY");
						SpinBox ScaleX = edit.GetNode<SpinBox>("ScaleX");
						SpinBox ScaleY = edit.GetNode<SpinBox>("ScaleY");
						SpinBox NamePosX = edit.GetNode<SpinBox>("NamePosX");
						SpinBox NamePosY = edit.GetNode<SpinBox>("NamePosY");
						SpinBox ProgPosX = edit.GetNode<SpinBox>("ProgPosX");
						SpinBox ProgPosY = edit.GetNode<SpinBox>("ProgPosY");
						SpinBox prog_size = edit.GetNode<SpinBox>("ProgSize");
						SpinBox name_size = edit.GetNode<SpinBox>("NameSize");
						CheckBox prog_on = edit.GetNode<CheckBox>("ProgOn");
						CheckBox name_on = edit.GetNode<CheckBox>("NameOn");

						OptionButton type = edit.GetNode<OptionButton>("Type");
						Slider progress = edit.GetNode<Slider>("Progress");//0~100

						OffsetX.Value = progress_node.Position.X;
						OffsetY.Value = progress_node.Position.Y;
						SizeX.Value = progress_node.Size.X;
						SizeY.Value = progress_node.Size.Y;
						ScaleX.Value = node.Scale.X;
						ScaleY.Value = node.Scale.Y;
						NamePosX.Value = name_label.Position.X;
						NamePosY.Value = name_label.Position.Y;
						ProgPosX.Value = prog_label.Position.X;
						ProgPosY.Value = prog_label.Position.Y;
						prog_on.ButtonPressed = prog_label.Visible;
						name_on.ButtonPressed = name_label.Visible;
						prog_size.Value=prog_label.GetThemeFontSize("font_size");
						name_size.Value=name_label.GetThemeFontSize("font_size");
						type.Selected = _t;
						progress.Value = (float)node.GetMeta("Progress");//0~100
						node.GetNode<Marker2D>("Value").Position=Vector2.Right*(float)progress.Value;
						switch (_t)
						{
							default:
							case 0:
								{
									OptionButton Behavior = edit.GetNode<OptionButton>("Bar/Behavior");
									Behavior.Selected = (int)progress_node.GetMeta("Behavior");
									ColorRect BorderColor = edit.GetNode<ColorRect>("Bar/BorderColor");
									BorderColor.Color = progress_node.SelfModulate;
									ColorRect FillColor = edit.GetNode<ColorRect>("Bar/FillColor");
									FillColor.Color = progress_node.GetNode<Control>("Fill").SelfModulate;
									StyleBoxFlat style = (StyleBoxFlat)((Panel)progress_node).GetThemeStylebox("Panel");
									SpinBox BorderWidth = edit.GetNode<SpinBox>("Bar/BorderWidth");
									BorderWidth.Value = style.GetBorderWidthMin();
								}
								break;
							case 1:
								{
									OptionButton Behavior = edit.GetNode<OptionButton>("Ring/Behavior");
									Behavior.Selected = (int)progress_node.GetMeta("Behavior");
									SpinBox Radius = edit.GetNode<SpinBox>("Ring/Radius");
									SpinBox HollowRadius = edit.GetNode<SpinBox>("Ring/Radius");
									SpinBox Segments = edit.GetNode<SpinBox>("Ring/Radius");
									var shader = (ShaderMaterial)progress_node.Material;
									Radius.Value = (float)shader.GetShaderParameter("radius");
									HollowRadius.Value = (float)shader.GetShaderParameter("hollow_radius");
									Segments.Value = (float)shader.GetShaderParameter("segments");
									Slider Margin = edit.GetNode<Slider>("Ring/Margin");
									Slider Rotation = edit.GetNode<Slider>("Ring/Rotation");
									Margin.Value = (float)shader.GetShaderParameter("margin");
									Rotation.Value = (float)shader.GetShaderParameter("rotation");
									ColorRect Color = edit.GetNode<ColorRect>("Ring/Color");
									Color.Color = (Color)shader.GetShaderParameter("color");
								}
								break;
							case 2:
								{
									var shader = (ShaderMaterial)progress_node.Material;
									ColorRect BackWave = edit.GetNode<ColorRect>("Wave/BackWave");
									BackWave.Color = (Color)shader.GetShaderParameter("backFillColour");
									ColorRect FrontWave = edit.GetNode<ColorRect>("Wave/FrontWave");
									FrontWave.Color = (Color)shader.GetShaderParameter("frontFillOuterColour");
									ColorRect FillMod = edit.GetNode<ColorRect>("Wave/FillMod");
									FillMod.Color = (Color)shader.GetShaderParameter("fillcolour");
									ColorRect RingColor = edit.GetNode<ColorRect>("Wave/RingColor");
									RingColor.Color = (Color)shader.GetShaderParameter("ringColour");
									SpinBox RingWidth = edit.GetNode<SpinBox>("Wave/RingWidth");
									SpinBox InnerOffset = edit.GetNode<SpinBox>("Wave/InnerOffset");
									RingWidth.Value = (float)shader.GetShaderParameter("ringWidth");
									InnerOffset.Value = (float)shader.GetShaderParameter("innerCircleRadiusOffset");
									Slider AmpMul = edit.GetNode<Slider>("Wave/AmpMul");
									Slider FreqMul = edit.GetNode<Slider>("Wave/FreqMul");
									AmpMul.Value = (float)shader.GetShaderParameter("amp_mul");
									FreqMul.Value = (float)shader.GetShaderParameter("freq_mul");

								}
								break;
						}
					}
					break;
			}
			edit_win.GetNode<Marker2D>("curr_euid").Position = Vector2.Right * selected.euid;
		}

		UpdatePreview();

	}
	void TransformOnly(bool on)
	{
		WriteTransformOnly=on;
	}
	void SelectFiles()
	{
		Dialog.Popup();
	}
	string[] file_paths;
	void FilesSelected(string[] paths)
	{
		file_paths = paths;
		foreach (var item in paths)
		{
			GD.Print("file_path:" + item);
		}
		_FilesSelected(paths);
	}

	void _on_file_dialog_confirmed()
	{

		Dialog.Visible = false;
		_FilesSelected(file_paths);
	}
	static PortableCompressedTexture2D preview_comp_tex;
	static bool Changed = false;
	void _FilesSelected(string[] paths)
	{
		var euid = (long)edit_win.GetNode<Marker2D>("curr_euid").Position.X;

		if (editor.Elements.Count <= 0)
		{
			return;
		}
		var _e = editor.Elements.Where(e => e.euid == euid).Select(e => e).First();
		var ext = Path.GetExtension(paths[0]).ToLower();
		switch (_e.type)
		{
			default:
			case ElementType.Image:
				if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
				{

					var file = Godot.FileAccess.Open(paths[0], Godot.FileAccess.ModeFlags.Read);
					if (file != null)
					{   //GD.Print(1111111);
						//var data=file.GetBuffer((long)file.GetLength());
						if (preview_comp_tex != null)
						{
							preview_comp_tex.KeepCompressedBuffer = false;
						}

						preview_comp_tex = new PortableCompressedTexture2D();
						preview_comp_tex.KeepCompressedBuffer = true;
						preview_comp_tex.CreateFromImage(Image.LoadFromFile(paths[0]), PortableCompressedTexture2D.CompressionMode.Lossless);
						edit_win.GetNode<TextureRect>("ImageEdit/Preview").Texture = preview_comp_tex;
						Changed = true;
					}
				}
				break;
			case ElementType.Animation:
				if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
				{
					var anim = edit_win.GetNode<AnimatedSprite2D>("AnimEdit/Preview");
					var sprf = anim.SpriteFrames;

					sprf.Clear("default");

					foreach (var _p in paths)
					{
						var file = Godot.FileAccess.Open(_p, Godot.FileAccess.ModeFlags.Read);
						if (file != null)
						{
							var image = new PortableCompressedTexture2D();
							image.KeepCompressedBuffer = true;
							image.CreateFromImage(Image.LoadFromFile(_p), PortableCompressedTexture2D.CompressionMode.Lossless);
							sprf.AddFrame("default", image);
						}
					}
					Changed = true;
					anim.Play("default");
				}
				break;
			case ElementType.Video:
				if (ext == ".ogv")
				{
					var edit = edit_win.GetNode<Panel>("VideoEdit");
					var prev = edit.GetNode<VideoStreamPlayer>("Preview");

					var file = Godot.FileAccess.Open(paths[0], Godot.FileAccess.ModeFlags.Read);
					if (file != null)
					{
						var stream = new VideoStreamTheora();
						stream.File = paths[0];
						edit_win.GetNode<VideoStreamPlayer>("VideoEdit/Preview").Stream = stream;
						prev.Play();
					}
				}

				break;
			case ElementType.Text:

				break;
			case ElementType.Custom:
				if (ext == ".tscn")
				{
					var edit = edit_win.GetNode<Panel>("CustomEdit");
					var prev = edit.GetNode<Node2D>("Preview");
					var file = Godot.FileAccess.Open(paths[0], Godot.FileAccess.ModeFlags.Read);
					if (file != null)
					{
						//clear all children
						var custom = ResourceLoader.Load<PackedScene>(paths[0]).Instantiate();
						Node2D n2d;
						try
						{
							n2d = (Node2D)custom;
						}
						catch (System.Exception)
						{

							throw;
						}
						if (n2d != null)
						{
							_e.Node = n2d;
						}

					}
				}
				break;
		}
		UpdatePreview();
	}
	//.GetNode<>();
	void _UpdatePreview(string str) { UpdatePreview(); }
	void _UpdatePreview(float num) { UpdatePreview(); }
	void _UpdatePreview(int num) { UpdatePreview(); }
	void _UpdatePreview() { UpdatePreview(); }

	void UpdatePreview()
	{
		var euid = (long)edit_win.GetNode<Marker2D>("curr_euid").Position.X;

		editor.Element e = null;//editor.Elements.Where(e=>e.euid==euid).First();
		foreach (var _e in editor.Elements)
		{
			if (_e.euid == euid)
			{
				e = _e;
			}
		}
		if (e == null || e.Node == null)
		{
			return;
		}
		e.Node.ZIndex = (int)edit_win.GetNode<SpinBox>("ZIndex").Value;
		CanvasItemMaterial.BlendModeEnum option2blend(int selected)
			{
				switch (selected)
				{

					default:
					case 1:
						return CanvasItemMaterial.BlendModeEnum.Add;
					case 0 :
						return CanvasItemMaterial.BlendModeEnum.Mix;
					case 3:
						return CanvasItemMaterial.BlendModeEnum.Mul;
						case 4:
						return CanvasItemMaterial.BlendModeEnum.PremultAlpha;
						case 2:
						return CanvasItemMaterial.BlendModeEnum.Sub;
				}
			}
		switch (e.type)
		{
			default:
			case ElementType.Image:
				{

					LineEdit OffsetX = edit_win.GetNode<LineEdit>("ImageEdit/OffsetX");
					LineEdit OffsetY = edit_win.GetNode<LineEdit>("ImageEdit/OffsetY");
					LineEdit SizeX = edit_win.GetNode<LineEdit>("ImageEdit/SizeX");
					LineEdit SizeY = edit_win.GetNode<LineEdit>("ImageEdit/SizeY");
					LineEdit ScaleX = edit_win.GetNode<LineEdit>("ImageEdit/ScaleX");
					LineEdit ScaleY = edit_win.GetNode<LineEdit>("ImageEdit/ScaleY");
					OptionButton stretch = edit_win.GetNode<OptionButton>("ImageEdit/StretchMode");
					OptionButton blend=edit_win.GetNode<OptionButton>("ImageEdit/BlendMode");
					CheckBox pixel = edit_win.GetNode<CheckBox>("ImageEdit/PixelMode");
					var prev = edit_win.GetNode<TextureRect>("ImageEdit/Preview");
					var node = e.Node.GetNode<TextureRect>("Image");

					var ox = 0f; float.TryParse(OffsetX.Text, out ox);
					var oy = 0f; float.TryParse(OffsetY.Text, out oy);
					var sx = 500f; float.TryParse(SizeX.Text, out sx);
					var sy = 500f; float.TryParse(SizeY.Text, out sy);
					var scx = 1f; float.TryParse(ScaleX.Text, out scx);
					var scy = 1f; float.TryParse(ScaleY.Text, out scy);
					var mode = stretch.Selected;

					prev.Position = new Vector2(ox, oy);
					prev.Size = new Vector2(sx, sy);
					prev.Scale = new Vector2(scx, scy);
					if (pixel.ButtonPressed)
					{
						prev.TextureFilter = TextureFilterEnum.Nearest;
						node.TextureFilter = TextureFilterEnum.Nearest;
					}
					else
					{
						prev.TextureFilter = TextureFilterEnum.Linear;
						node.TextureFilter = TextureFilterEnum.Linear;
					}
					if (mode == 0)
					{
						prev.StretchMode = TextureRect.StretchModeEnum.Scale;
						node.StretchMode = TextureRect.StretchModeEnum.Scale;
					}
					else if (mode == 1)
					{
						prev.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
						node.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
					}
					else if (mode == 2)
					{
						prev.StretchMode = TextureRect.StretchModeEnum.Tile;
						node.StretchMode = TextureRect.StretchModeEnum.Tile;
					}
					node.Position = new Vector2(ox, oy);
					node.Size = new Vector2(sx, sy);
					e.Node.Scale = new Vector2(scx, scy);
					((CanvasItemMaterial)e.Node.Material).BlendMode=option2blend(blend.Selected);
					((CanvasItemMaterial)prev.Material).BlendMode=option2blend(blend.Selected);
					node.UseParentMaterial=true;
				}
				break;
			case ElementType.Animation:
				{
					LineEdit spd = edit_win.GetNode<LineEdit>("AnimEdit/Speed");
					LineEdit fps = edit_win.GetNode<LineEdit>("AnimEdit/FPS");
					LineEdit scale_x = edit_win.GetNode<LineEdit>("AnimEdit/ScaleX");
					LineEdit scale_y = edit_win.GetNode<LineEdit>("AnimEdit/ScaleY");
					CheckBox pixel = edit_win.GetNode<CheckBox>("AnimEdit/PixelMode");
					OptionButton blend=edit_win.GetNode<OptionButton>("ImageEdit/BlendMode");
					var anim = edit_win.GetNode<AnimatedSprite2D>("AnimEdit/Preview");
					var node = (AnimatedSprite2D)e.Node;

					var _s = 1f; float.TryParse(spd.Text, out _s);
					var _f = 24; int.TryParse(fps.Text, out _f);
					var _scx = 1f; float.TryParse(scale_x.Text, out _scx);
					var _scy = 1f; float.TryParse(scale_y.Text, out _scy);

					if (pixel.ButtonPressed)
					{
						anim.TextureFilter = TextureFilterEnum.Nearest;
						node.TextureFilter = TextureFilterEnum.Nearest;
					}
					else
					{
						anim.TextureFilter = TextureFilterEnum.Linear;
						node.TextureFilter = TextureFilterEnum.Linear;
					}
					anim.SpeedScale = _s;
					anim.SpriteFrames.SetAnimationSpeed("default", _f);
					anim.Scale = new Vector2(_scx, _scy);

					node.SpeedScale = _s;
					node.SpriteFrames.SetAnimationSpeed("default", _f);
					e.Node.Scale = new Vector2(_scx, _scy);
					((CanvasItemMaterial)e.Node.Material).BlendMode=option2blend(blend.Selected);
				}
				break;
			case ElementType.Video:
				{
					var prev = edit_win.GetNode<VideoStreamPlayer>("VideoEdit/Preview");
					var node = e.Node.GetNode<VideoStreamPlayer>("Video");

					LineEdit OffsetX = edit_win.GetNode<LineEdit>("VideoEdit/OffsetX");
					LineEdit OffsetY = edit_win.GetNode<LineEdit>("VideoEdit/OffsetY");
					LineEdit SizeX = edit_win.GetNode<LineEdit>("VideoEdit/SizeX");
					LineEdit SizeY = edit_win.GetNode<LineEdit>("VideoEdit/SizeY");
					LineEdit ScaleX = edit_win.GetNode<LineEdit>("VideoEdit/ScaleX");
					LineEdit ScaleY = edit_win.GetNode<LineEdit>("VideoEdit/ScaleY");

					CheckBox loop = edit_win.GetNode<CheckBox>("VideoEdit/Loop");
					CheckBox expand = edit_win.GetNode<CheckBox>("VideoEdit/Expand");

					var ox = 0f; float.TryParse(OffsetX.Text, out ox);
					var oy = 0f; float.TryParse(OffsetY.Text, out oy);
					var sx = 500f; float.TryParse(SizeX.Text, out sx);
					var sy = 500f; float.TryParse(SizeY.Text, out sy);
					var scx = 1f; float.TryParse(ScaleX.Text, out scx);
					var scy = 1f; float.TryParse(ScaleY.Text, out scy);
					var lp = loop.ButtonPressed;
					var exp = expand.ButtonPressed;

					prev.Position = new Vector2(ox, oy);
					prev.Size = new Vector2(sx, sy);
					prev.Scale = new Vector2(scx, scy);
					prev.Loop = lp; prev.Expand = exp;

					node.Position = new Vector2(ox, oy);
					node.Size = new Vector2(sx, sy);
					e.Node.Scale = new Vector2(scx, scy);
					node.Loop = lp; node.Expand = exp;

				}
				break;
			case ElementType.Text:
				{
					var prev = edit_win.GetNode<TextEdit>("TextEdit/Preview");
					var node = e.Node.GetNode<RichTextLabel>("Text");

					LineEdit OffsetX = edit_win.GetNode<LineEdit>("TextEdit/OffsetX");
					LineEdit OffsetY = edit_win.GetNode<LineEdit>("TextEdit/OffsetY");
					LineEdit SizeX = edit_win.GetNode<LineEdit>("TextEdit/SizeX");
					LineEdit SizeY = edit_win.GetNode<LineEdit>("TextEdit/SizeY");
					LineEdit ScaleX = edit_win.GetNode<LineEdit>("TextEdit/ScaleX");
					LineEdit ScaleY = edit_win.GetNode<LineEdit>("TextEdit/ScaleY");

					var ox = 0f; float.TryParse(OffsetX.Text, out ox);
					var oy = 0f; float.TryParse(OffsetY.Text, out oy);
					var sx = 500f; float.TryParse(SizeX.Text, out sx);
					var sy = 500f; float.TryParse(SizeY.Text, out sy);
					var scx = 1f; float.TryParse(ScaleX.Text, out scx);
					var scy = 1f; float.TryParse(ScaleY.Text, out scy);

					prev.Position = new Vector2(ox, oy);
					prev.Size = new Vector2(sx, sy);
					prev.Scale = new Vector2(scx, scy);

					node.Position = new Vector2(ox, oy);
					node.Size = new Vector2(sx, sy);
					e.Node.Scale = new Vector2(scx, scy);
				}
				break;
			case ElementType.Custom:
				{
					var prev = edit_win.GetNode<Node2D>("CustomEdit/Preview");
					prev.GetChildren().All(c => { c.QueueFree(); return true; });
					var custom = e.Node;
					if (custom != null)
					{
						prev.AddChild(custom.Duplicate());
					}

				}
				break;
			case ElementType.Progress:
				{
					var edit = edit_win.GetNode<Panel>("ProgressEdit"); edit.Visible = true;
					var prev = edit.GetNode<Node2D>("PreviewWindow/Preview");

					var node = e.Node;
					Control progress_node;


					var name_label = node.GetNode<Label>("Name");
					var prog_label = node.GetNode<Label>("Progress");

					SpinBox OffsetX = edit.GetNode<SpinBox>("OffsetX");
					SpinBox OffsetY = edit.GetNode<SpinBox>("OffsetY");
					SpinBox SizeX = edit.GetNode<SpinBox>("SizeX");
					SpinBox SizeY = edit.GetNode<SpinBox>("SizeY");
					SpinBox ScaleX = edit.GetNode<SpinBox>("ScaleX");
					SpinBox ScaleY = edit.GetNode<SpinBox>("ScaleY");
					SpinBox NamePosX = edit.GetNode<SpinBox>("NamePosX");
					SpinBox NamePosY = edit.GetNode<SpinBox>("NamePosY");
					SpinBox ProgPosX = edit.GetNode<SpinBox>("ProgPosX");
					SpinBox ProgPosY = edit.GetNode<SpinBox>("ProgPosY");

					CheckBox prog_on = edit.GetNode<CheckBox>("ProgOn");
					CheckBox name_on = edit.GetNode<CheckBox>("NameOn");
					SpinBox prog_size = edit.GetNode<SpinBox>("ProgSize");
					SpinBox name_size = edit.GetNode<SpinBox>("NameSize");
					OptionButton type = edit.GetNode<OptionButton>("Type");
					Slider progress = edit.GetNode<Slider>("Progress");//0~100

					var _t = type.Selected;
					node.SetMeta("Type", _t);
					switch (_t)
					{
						default:
						case 0:
							progress_node = node.GetNode<Control>("Bar");
							break;
						case 1:
							progress_node = node.GetNode<Control>("Ring");
							break;
						case 2:
							progress_node = node.GetNode<Control>("Wave");
							break;
					}
					if (prev_t != _t)
					{
						node.GetNode<Control>("Bar").Visible = false;
						node.GetNode<Control>("Ring").Visible = false;
						node.GetNode<Control>("Wave").Visible = false;
						edit.GetNode<Control>("Bar").Visible = false;
						edit.GetNode<Control>("Ring").Visible = false;
						edit.GetNode<Control>("Wave").Visible = false;
						progress_node.Visible = true;
						edit.GetNode<Control>((string)progress_node.Name).Visible = true;

					}
					prev_t = _t;
					progress_node.Position = new Vector2((float)OffsetX.Value, (float)OffsetY.Value);
					progress_node.Size = new Vector2((float)SizeX.Value, (float)SizeY.Value);
					node.Scale = new Vector2((float)ScaleX.Value, (float)ScaleY.Value);
					name_label.Position = new Vector2((float)NamePosX.Value, (float)NamePosY.Value);
					prog_label.Position = new Vector2((float)ProgPosX.Value, (float)ProgPosY.Value);
					prog_label.Visible = prog_on.ButtonPressed;
					name_label.Visible = name_on.ButtonPressed;
					name_label.Text = e.Name;
					prog_label.AddThemeFontSizeOverride("font_size", (int)prog_size.Value);
					name_label.AddThemeFontSizeOverride("font_size", (int)name_size.Value);
					node.SetMeta("Progress", progress.Value);//0~100
					node.GetNode<Marker2D>("Value").Position=Vector2.Right*(float)progress.Value;
					switch (_t)
					{
						default:
						case 0:
							{
								OptionButton Behavior = edit.GetNode<OptionButton>("Bar/Behavior");
								progress_node.SetMeta("Behavior", Behavior.Selected);
								ColorRect BorderColor = edit.GetNode<ColorRect>("Bar/BorderColor");
								progress_node.SelfModulate = BorderColor.Color;
								ColorRect FillColor = edit.GetNode<ColorRect>("Bar/FillColor");
								var fill = progress_node.GetNode<Control>("Fill"); fill.SelfModulate = FillColor.Color;
								StyleBoxFlat style = (StyleBoxFlat)((Panel)progress_node).GetThemeStylebox("panel");
								SpinBox BorderWidth = edit.GetNode<SpinBox>("Bar/BorderWidth");
								style.SetBorderWidthAll((int)BorderWidth.Value);//
								
							}
							break;
						case 1:
							{
								OptionButton Behavior = edit.GetNode<OptionButton>("Ring/Behavior");
								progress_node.SetMeta("Behavior", Behavior.Selected);
								SpinBox Radius = edit.GetNode<SpinBox>("Ring/Radius");
								SpinBox HollowRadius = edit.GetNode<SpinBox>("Ring/HollowRadius");
								SpinBox Segments = edit.GetNode<SpinBox>("Ring/Segments");
								var shader = (ShaderMaterial)progress_node.Material;
								shader.SetShaderParameter("radius", Radius.Value);
								shader.SetShaderParameter("hollow_radius", HollowRadius.Value);
								shader.SetShaderParameter("segments", Segments.Value);
								Slider Margin = edit.GetNode<Slider>("Ring/Margin");
								Slider Rotation = edit.GetNode<Slider>("Ring/Rotation");
								shader.SetShaderParameter("margin", Margin.Value);
								shader.SetShaderParameter("rotation", Rotation.Value);
								ColorRect Color = edit.GetNode<ColorRect>("Ring/Color");
								shader.SetShaderParameter("color", Color.Color);
								
								
							}
							break;
						case 2:
							{
								var shader = (ShaderMaterial)progress_node.Material;
								ColorRect BackWave = edit.GetNode<ColorRect>("Wave/BackFill");
								shader.SetShaderParameter("backFillColour", BackWave.Color);
								ColorRect FrontWave = edit.GetNode<ColorRect>("Wave/FrontFill");
								shader.SetShaderParameter("frontFillOuterColour", FrontWave.Color);
								ColorRect FillMod = edit.GetNode<ColorRect>("Wave/FillModulate");
								shader.SetShaderParameter("fillcolour", FillMod.Color);
								ColorRect RingColor = edit.GetNode<ColorRect>("Wave/RingColor");
								shader.SetShaderParameter("ringColour", RingColor.Color);
								SpinBox RingWidth = edit.GetNode<SpinBox>("Wave/RingWidth");
								SpinBox InnerOffset = edit.GetNode<SpinBox>("Wave/InnerOffset");
								shader.SetShaderParameter("ringWidth", RingWidth.Value);
								shader.SetShaderParameter("innerCircleRadiusOffset", InnerOffset.Value);
								Slider AmpMul = edit.GetNode<Slider>("Wave/AmpMul");
								Slider FreqMul = edit.GetNode<Slider>("Wave/FreqMul");
								shader.SetShaderParameter("amp_mul", AmpMul.Value);
								shader.SetShaderParameter("freq_mul", FreqMul.Value);
								
							}
							break;
					}
					if (prev.GetChildren().Count > 0)
					{
						prev.GetChildren().All(c => { c.QueueFree(); return true; });
					}
					prev.AddChild(node.Duplicate());
					
				}
				break;
		}
	}
	int prev_t = 0;


	void ApplyEdit()
	{
		var euid = (long)edit_win.GetNode<Marker2D>("curr_euid").Position.X;
		if (editor.Elements.Count <= 0)
		{
			return;
		}
		var _e = editor.Elements.Where(e => e.euid == euid).Select(e => e).First();
		_e.Name = edit_win.GetNode<LineEdit>("Name").Text;
		UpdatePreview();
		switch (_e.type)
		{
			default:
			case ElementType.Image:

				{
					if (Changed == true)
					{
						Changed = false;
					var edit = edit_win.GetNode<Panel>("ImageEdit");
					_e.FrameData.Clear();
					var tex = new PortableCompressedTexture2D();
					tex.KeepCompressedBuffer = true;
					tex._Data = (byte[])preview_comp_tex._Data.Clone();
					_e.FrameData.Add(tex._Data);
					var sprite = (TextureRect)_e.Node.GetChild(0);
					if (sprite.Texture!=null)
					{
						sprite.Texture=null;
					}
					
					}
				}
				break;
			case ElementType.Animation:
				{
					if (Changed == true)
					{
						Changed = false;
						var edit = edit_win.GetNode<Panel>("AnimEdit");
						var prev_sprite = edit.GetNode<AnimatedSprite2D>("Preview");
						var sprite = (AnimatedSprite2D)_e.Node;
						sprite.SpriteFrames.SetAnimationSpeed("default", prev_sprite.SpriteFrames.GetAnimationSpeed("default"));
						sprite.SpriteFrames.Clear("default");
						_e.FrameData.Clear();
						for (int i = 0; i < prev_sprite.SpriteFrames.GetFrameCount("default"); i++)
						{
							var _prev_tex = (PortableCompressedTexture2D)prev_sprite.SpriteFrames.GetFrameTexture("default", i);
							_prev_tex.KeepCompressedBuffer = true;

							var tex = new PortableCompressedTexture2D();
							tex.KeepCompressedBuffer = true;
							tex._Data = (byte[])_prev_tex._Data.Clone();


							_e.FrameData.Add(tex._Data);
							GD.Print(_e.FrameData.Count);
							//sprite.SpriteFrames.AddFrame("default", tex);


						}
					}

				}
				break;
			case ElementType.Video:
				{
					var edit = edit_win.GetNode<Panel>("VideoEdit");
					_e.Node.GetChild<VideoStreamPlayer>(0).Stream = (VideoStream)edit.GetNode<VideoStreamPlayer>("Preview").Stream.Duplicate();

				}
				break;
			case ElementType.Text:
				{
					var edit = edit_win.GetNode<Panel>("TextEdit");
					_e.Node.GetChild<RichTextLabel>(0).Text = edit.GetNode<TextEdit>("Preview").Text;
					_e.Node.GetChild<RichTextLabel>(0).AddThemeFontSizeOverride("normal_font_size", (int)edit.GetNode<SpinBox>("FontSize").Value);
				}
				break;
			case ElementType.Custom:


				break;
			case ElementType.Progress:
				break;
		}
		_e.PrewriteNodeData();
		edit_win.Visible = false;
		Update();
	}
	public static void ColorPicker()
	{
		var picker = editor.root.GetNode<Window>("ColorPicker");
		picker.Popup();
	}
	public static void ClosePicker()
	{
		var picker = editor.root.GetNode<Window>("ColorPicker");
		picker.Visible = false;
	}
	static Window picker;
	public static void ApplyColor(ColorRect color)
	{

		picker.Visible = false;
		color.Color = picker.GetNode<ColorPicker>("ColorPicker").Color;
	}
	void CloseWindow() { edit_win.Visible = false; UpdatePreview(); }
	void Delete()
	{
		foreach (var e in editor.SelectedElements)
		{
			if (editor.Elements.Count <= 0)
			{
				break;
			}
			e.entity.Visible = false;
			//e.Node.QueueFree();
			editor.Elements.Where(_e => _e.euid == e.euid).Select(e => e).All(e => { editor.RecycledElements.Add(e); return true; });
			editor.Elements.RemoveWhere(_e => _e.euid == e.euid);


		}
		editor.SelectedElements.Clear(); editor.UpdateBin();
		Update();
	}
	void CopyE()
	{
		editor.CopiedElements.Clear();
		foreach (var e in editor.SelectedElements)
		{
			editor.CopiedElements.Add(e);
		}
	}
	void PasteE()
	{
		if (editor.CopiedElements.Count <= 0)
			return;

		HashSet<editor.Element> cache = new HashSet<editor.Element>();
		//var event_stream=(HashSet<Hashtable>)editor.ProjectData["event_stream"];
		foreach (var _e in editor.CopiedElements)
		{
			var new_e = NewElement(editor.assign_euid(), _e.type);
			//CopyProperties(_e,new_e);//new_e.uid=editor.assign_uid();
			Type t = _e.GetType(); PropertyInfo[] properties = t.GetProperties();
			foreach (var property in properties)
			{

				if (property.CanRead && property.CanWrite)
				{
					object value = property.GetValue(_e);
					if (property.Name != "euid")
					{
						GD.Print(property.Name);
						if (value is Node)
						{
							new_e.GetType().GetProperty(property.Name).SetValue(new_e, ((Node)value).Duplicate());
						}
						else
						{
							new_e.GetType().GetProperty(property.Name).SetValue(new_e, value);
						}

					}
				}
			}
			cache.Add(new_e);
		}
		editor.SelectedElements.Clear();
		foreach (var item in cache)
		{
			editor.SelectedElements.Add(item);
		}

		Update();
	}
}
