using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CustomTheme : Window
{
	// Called when the node enters the scene tree for the first time.
	public static Control prev;
	public static Panel panel;
	public static TextureRect image_prev;
	public static ReferenceRect TextCaseRef;
	public static long tuid_assigner = 0;
	public static Hashtable Themes = new Hashtable();//Theme Controls
	public static long selected_tuid = 0;
	public static Control selected_deco;
	public static Control ThemeList;
	bool selecting = false;
	public override void _Ready()
	{
		panel = GetNode<Panel>("Panel");
		prev = GetNode<Control>("Panel/Preview");
		image_prev = prev.GetNode<TextureRect>("Preview2D/Image");
		TextCaseRef = image_prev.GetNode<ReferenceRect>("TextCase");
		ThemeList = GetNode<Control>("Panel/Explorer/Scr/VBox");

		texture_name = GetNode<Label>("Panel/ImageName");
		margins = GetNode<Control>("Panel/PatchMargins");
		offsets = GetNode<Control>("Panel/AnchorOffsets");
		deco_editor =GetNode<Window>("DecoEditor");
		buttons=new Button[]{GetNode<Button>("Panel/Select"),GetNode<Button>("DecoEditor/EditPanel/Image/Select"),GetNode<Button>("DecoEditor/EditPanel/Animation/Select")};
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	long assign_tuid()
	{
		tuid_assigner += 1;
		return tuid_assigner;
	}
		void ChooseTheme()
	{
		Popup();
		GetNode<Button>("Panel/Apply").Visible=true;
		GetNode<Button>("Panel/Done").Visible=false;
		UpdateList();
	}
	void Open()
	{
		GetNode<Button>("Panel/Apply").Visible=false;
		GetNode<Button>("Panel/Done").Visible=true;
		Popup();
		UpdateList();
	}
	static Window deco_editor;
	void SwitchType(bool value)
	{
		deco_editor.GetNode<Control>("EditPanel/Image").Visible=false;
		deco_editor.GetNode<Control>("EditPanel/Animation").Visible=false;
		if (!value)
		{//image
			deco_editor.GetNode<Control>("EditPanel/Image").Visible=true;
		}else
		{
			deco_editor.GetNode<Control>("EditPanel/Animation").Visible=true;
		}
		
	}
	void OpenDecoEdit()
	{
		if (not_exist())
		{
			return;
		}
		deco_editor.Popup();
		var type_switch=deco_editor.GetNode<CheckButton>("EditPanel/Type");
		SwitchType(type_switch.ButtonPressed);
		if (type_switch.ButtonPressed)
		{
			GetNode<SpinBox>("DecoEditor/EditPanel/Animation/SpdScale").Value=selected_deco.GetNode<AnimatedSprite2D>("Sprite").SpeedScale;
			GetNode<Label>("DecoEditor/EditPanel/Animation/AnimName").Text="[Animation]";
		}else
		{
			GetNode<Label>("DecoEditor/EditPanel/Image/ImageName").Text="[Image]";
		}
		
	}
	void CloseDecoEdit()
	{
		if (selecting)
		{
			return;
		}
		deco_editor.Visible=false;
	}
	void UpdateList()
	{

		foreach (var item in ThemeList.GetChildren())
		{
			item.QueueFree();
		}
		foreach (DictionaryEntry item in Themes)
		{
			var list = GetNode<Control>("Panel/Explorer/Scr/VBox");
			var button = (Button)GetNode<Button>("Panel/Explorer/Unit").Duplicate();
			
			var tuid = (long)item.Key;

			button.GetNode<Marker2D>("tuid").Position = tuid * Vector2.Right;
			list.AddChild(button);
			button.Visible=true;
		}
	}
	void AddTheme()
	{
		var list = GetNode<Control>("Panel/Explorer/Scr/VBox");
		var button = (Button)GetNode<Button>("Panel/Explorer/Unit").Duplicate();
		var theme = (Control)GetNode("ThemeUnit").Duplicate();
		var tuid = assign_tuid();
		Themes.Add(tuid, theme);
		button.GetNode<Marker2D>("tuid").Position = tuid * Vector2.Right;
		list.AddChild(button);
		button.Visible=true;
	}
	void Delete()
	{
		if (!Themes.ContainsKey(selected_tuid))
		{
			return;
		}
		if (ThemeList.GetChildCount()>0)
		{
			ThemeList.GetChildren().Where(t => t.GetNode<Marker2D>("tuid").Position == selected_tuid * Vector2.Right).First().QueueFree();
		}
		Themes.Remove(selected_tuid);
	}
	void Duplicate()
	{
		if (!Themes.ContainsKey(selected_tuid))
		{
			return;
		}
		var list = GetNode<Control>("Panel/Explorer/Scr/VBox");
		var button = (Button)GetNode<Button>("Panel/Explorer/Unit").Duplicate();
		var selected_theme = Themes[selected_tuid];
		var dup = ((Node)selected_theme).Duplicate();
		var tuid = assign_tuid();
		Themes.Add(tuid, dup);
		button.GetNode<Marker2D>("tuid").Position = tuid * Vector2.Right;
		list.AddChild(button);
		button.Visible=true;
	}
	public static void DeselectAll()
	{

		foreach (var t in ThemeList.GetChildren())
		{
			((Node)t).GetNode<Control>("Highlight").Visible = false;
		}
		foreach (var d in panel.GetNode<Control>("DecoExplorer/Scr/VBox").GetChildren())
		{
			((Node)d).GetNode<Control>("Highlight").Visible = false;
		}
		
	}

void ApplyThemeName(string text)
{
	((Control)Themes[selected_tuid]).SetMeta("name",text);
}
void ApplyDecoName(string text)
{
	selected_deco.SetMeta("name",text);
}
	void _Update(int v)
	{
		Update();
	}
	static bool loading=false;
	static void Update()//update the actual theme data
	{
		if (Themes.ContainsKey(selected_tuid)&&loading==false)
		{
			
		
		var theme = (Control)Themes[selected_tuid];
		var patch_rect = theme.GetNode<NinePatchRect>("Texture");
		var text_case = theme.GetNode<RichTextLabel>("TextCase");

		patch_rect.PatchMarginBottom = (int)margins.GetNode<SpinBox>("BMargin").Value;
		patch_rect.PatchMarginLeft = (int)margins.GetNode<SpinBox>("LMargin").Value;
		patch_rect.PatchMarginRight = (int)margins.GetNode<SpinBox>("RMargin").Value;
		patch_rect.PatchMarginTop = (int)margins.GetNode<SpinBox>("UMargin").Value;

		text_case.AnchorBottom = (float)offsets.GetNode<SpinBox>("BOffset").Value;
		text_case.AnchorLeft = (float)offsets.GetNode<SpinBox>("LOffset").Value;
		text_case.AnchorRight = (float)offsets.GetNode<SpinBox>("ROffset").Value;
		text_case.AnchorTop = (float)offsets.GetNode<SpinBox>("UOffset").Value;

		patch_rect.AxisStretchHorizontal = (NinePatchRect.AxisStretchMode)panel.GetNode<OptionButton>("HStretch").Selected;

		patch_rect.AxisStretchVertical = (NinePatchRect.AxisStretchMode)panel.GetNode<OptionButton>("VStretch").Selected;
		
		//var texture = patch_rect.Texture;
		if (editor.Elements.Count>0)
		{
			var image_id =(long)theme.GetMeta("image_euid");
			Texture2D texture;
			foreach (var e in editor.Elements)
			{
				GD.Print(e.euid,",",image_id);
				if (e.euid==image_id)
				{
					if (e.FrameData.Count>0)
					{
						var tex = new PortableCompressedTexture2D();
						tex._Data = (byte[])e.FrameData.ElementAt(0).Clone();
						
						tex.KeepCompressedBuffer = false;
						image_prev.Texture=tex;
					}
				}
			}
			 
			
			
		}
		if (image_prev.Texture!=null)
		{
			image_size = image_prev.Texture.GetSize();
		image_prev.Size=image_size;
		}else{image_prev.Size=new Vector2(100,100);}
		
		var deco_prev=image_prev.GetNode("Decos");

		var explorer = panel.GetNode<Panel>("DecoExplorer");
		var deco_list = explorer.GetNode<VBoxContainer>("Scr/VBox");
		foreach (Button button in ThemeList.GetChildren())
		{
			var _t=(Control)Themes[(long)button.GetNode<Marker2D>("tuid").Position.X];
			if (_t.HasMeta("name"))
			{
				button.Text= (StringName)(string)_t.GetMeta("name");
			}else{_t.SetMeta("name","ThemeOverride");}
			
		}
		foreach (Button button in deco_list.GetChildren())
		{
			var _d=theme.GetNode<Control>((NodePath)("Decos/"+(string)button.GetMeta("UName")));
			if (_d.HasMeta("name"))
			{
					button.Text= (StringName)(string)_d.GetMeta("name");
			}else
			{
				_d.SetMeta("name","ThemeDecoration");
			}
			button.Name= (StringName)(string)_d.GetMeta("name");
		}
		foreach (Control deco in theme.GetNode<Control>("Decos").GetChildren())
		{
			var uname=deco.GetMeta("prev_name");
			var prev=deco_prev.GetNode<Control>((NodePath)(string)uname);
			prev.Rotation=deco.Rotation;
			prev.Scale=deco.Scale;
			prev.Position=deco.Position;
			prev.SetAnchor(Side.Left,deco.AnchorLeft,true,true);
				prev.SetAnchor(Side.Right,deco.AnchorRight,true,true);
					prev.SetAnchor(Side.Top,deco.AnchorTop,true,true);
						prev.SetAnchor(Side.Bottom,deco.AnchorBottom,true,true);
			var prev_sprite=prev.GetNode<AnimatedSprite2D>("Sprite");
			var sprite=deco.GetNode<AnimatedSprite2D>("Sprite");
		if (editor.Elements.Count>0)
		{
		
			var image_euid=(long)deco.GetMeta("image_euid");
			if ((bool)deco.GetMeta("changed")&&image_euid>-1) 
			{
				editor.Element selected_image=null;
				foreach (var item in editor.Elements)
				{
						//GD.Print(deco.GetMeta("image_euid"),",",item.euid);
					if (item.euid==image_euid)
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
						prev_sprite.SpriteFrames = frames;
						
					}else if(selected_image.type == ElementExplorer.ElementType.Animation)
					{		
						prev_sprite.SpriteFrames = (SpriteFrames)((AnimatedSprite2D)selected_image.Node).SpriteFrames.Duplicate();
						prev_sprite.SpriteFrames.Clear("default");
							
						foreach (var item in selected_image.FrameData)
						{
							var tex=new PortableCompressedTexture2D();
							tex._Data= (byte[])item.Clone();
							GD.Print(tex);
							tex.KeepCompressedBuffer=true;
							
							prev_sprite.SpriteFrames.AddFrame("default",tex);
							
						}
						prev_sprite.Play();
						
					}
					deco.SetMeta("changed",false);
				}
				

			}
		}
			prev_sprite.SpeedScale=sprite.SpeedScale;

		}
		
		}
	}
	void AddDeco()
	{
		var theme = (Control)Themes[selected_tuid];
		var decos = theme.GetNode<Control>("Decos");
		var deco = (Control)GetNode("DecoEditor/Deco").Duplicate();
		var explorer = panel.GetNode<Panel>("DecoExplorer");
		var deco_list = explorer.GetNode<VBoxContainer>("Scr/VBox");

		decos.AddChild(deco);
		var deco_dup = deco.Duplicate();
		image_prev.GetNode("Decos").AddChild(deco_dup);
		LinkDeco(deco,deco_dup);
		var u_dup = explorer.GetNode("Unit").Duplicate();
		u_dup.SetMeta("UName", deco.Name);
		deco_list.AddChild(u_dup);
		((Control)u_dup).Visible=true;
		deco_dup.GetNode<AnimatedSprite2D>("Sprite").Visible=true;
	}
	static bool not_exist()
	{
		return selected_deco == null || (!Themes.ContainsKey(selected_tuid));
	}
	void DupDeco()
	{
		if (not_exist())
		{
			return;
		}
		var theme = (Control)Themes[selected_tuid];
		var dup = selected_deco.Duplicate();
		theme.GetNode("Decos").AddChild(dup);
		//update
		var explorer = panel.GetNode<Panel>("DecoExplorer");
		var deco_list = explorer.GetNode<VBoxContainer>("Scr/VBox");

		var deco_dup = dup.Duplicate();
		image_prev.GetNode("Decos").AddChild(deco_dup);
		LinkDeco(dup,deco_dup);
		var u_dup = explorer.GetNode("Unit").Duplicate();
		u_dup.SetMeta("UName", dup.Name);
		deco_list.AddChild(u_dup);
		((Control)u_dup).Visible=true;
		deco_dup.GetNode<AnimatedSprite2D>("Sprite").Visible=true;

	}
	void DeleteDeco()
	{
		if (not_exist())
		{
			return;
		}
		selected_deco.QueueFree();
		//update
		var explorer = panel.GetNode<Panel>("DecoExplorer");
		var deco_list = explorer.GetNode<VBoxContainer>("Scr/VBox");

		foreach (var deco_prev in image_prev.GetNode("Decos").GetChildren())
		{
			if ((string)deco_prev.GetMeta("UName") == selected_deco.Name)
			{
				deco_prev.QueueFree();
			}
		}
		foreach (var deco_prev in deco_list.GetChildren())
		{
			if ((string)deco_prev.GetMeta("UName") == selected_deco.Name)
			{
				deco_prev.QueueFree();
			}
		}
		selected_deco = null;
	}
	void _SelectImageForDeco()//
	{
		if (not_exist())
		{
			return;
		}
		SelectImage(GetNode<Button>("DecoEditor/EditPanel/Image/Select"), GetNode<Label>("DecoEditor/EditPanel/Image/ImageName"), selected_deco);
		//if (!selecting)
		//{
			
		//}
		selected_deco.SetMeta("changed",true);
		Update();

	}
	Button[] buttons;
	void HandleButtons(Button button)
	{
		if (button!=null)
		{
			foreach (var item in buttons)
			{
				if (item!=button)
				{
					item.Disabled=true;
				}
			}
		}else
		{
			foreach (var item in buttons)
			{
				item.Disabled=false;
			}
		}
	}
	void SelectAnimForDeco()
	{
		if (not_exist())
		{
			return;
		}
		var btn_select = GetNode<Button>("DecoEditor/EditPanel/Animation/Select");
		var label = GetNode<Label>("DecoEditor/EditPanel/Animation/AnimName");
		selecting = !selecting;
		if (selecting)
		{
			editor.camera.Position = Vector2.Left * 1400;
			btn_select.Text = "Apply";
			HandleButtons(btn_select);
		}
		else
		{
			btn_select.Text = "Select";

			// Apply image to 9 patch rect
			//duplicate the portable compressed texture
			//Update

			var selected_anim = editor.SelectedElements.First();
			if (selected_anim.type == ElementExplorer.ElementType.Animation)
			{
				label.Text = "[Anim]" + selected_anim.Name;
				var image_id = selected_anim.euid;
				selected_deco.SetMeta("image_euid", image_id);
				selected_deco.SetMeta("changed",true);
				//fit margin
				//selected_deco.GetNode<AnimatedSprite2D>("Sprite").SpriteFrames = (SpriteFrames)((AnimatedSprite2D)selected_anim.Node).SpriteFrames.Duplicate();
				selected_deco.GetNode<AnimatedSprite2D>("Sprite").SpeedScale= (float)GetNode<SpinBox>("DecoEditor/EditPanel/Animation/SpdScale").Value;

                //Update();
			}
			HandleButtons(null);


		}
		Update();
	}
	static Label texture_name;
	void _SelectImageForTheme()
	{
		var theme=(Control)Themes[selected_tuid];
		SelectImage(GetNode<Button>("Panel/Select"), GetNode<Label>("Panel/ImageName"), theme);
	}
	void SelectImage(Button btn_select, Label label, Control ImageRect)
	{
		selecting = !selecting;
		if (selecting)
		{
			editor.camera.Position = Vector2.Left * 1400;
			btn_select.Text = "Apply";
			HandleButtons(btn_select);
		}
		else
		{
			btn_select.Text = "Select";

			// Apply image to 9 patch rect
			//duplicate the portable compressed texture
			//Update

			var selected_image = editor.SelectedElements.First();
			if (selected_image!=null&&selected_image.type == ElementExplorer.ElementType.Image)
			{
				label.Text = "[Image]" + selected_image.Name;
				var image_id = selected_image.euid;
				ImageRect.SetMeta("image_euid", image_id);
				//var patch_rect = ImageRect.GetNode<NinePatchRect>("Texture");
				//patch_rect.Texture=selected_image.Node.GetChild<TextureRect>(0).Texture;
				//fit margin
				

				Update();
			}
			HandleButtons(null);


		}

	}
	void RotateDeco(float value)
	{
		if (selected_deco == null)
		{
			return;
		}
		selected_deco.RotationDegrees = value;
		Update();
	}
	void ScaleDeco(float value)
	{
		if (selected_deco == null)
		{
			return;
		}
		selected_deco.Scale = Vector2.One * value;
		Update();
	}
	static Vector2 prev_pos=Vector2.Zero;
	static Vector2 start_pos=Vector2.Zero;
	static bool dragging=false;
	void MoveDeco(InputEvent _event)
	{
		
		if (selected_deco == null)
		{
			return;
		}
		if (_event is InputEventMouseButton)
		{
			InputEventMouseButton _mouse_event=(InputEventMouseButton)_event;
			if (_mouse_event.Pressed)
			{	//GD.Print("pressed");
				start_pos=_mouse_event.Position;
				prev_pos=selected_deco.Position;
				dragging=true;
			}else{
				dragging=false;
			}
				//selected_deco.Position = pos;
			Update();
		}
		else if (_event is InputEventMouseMotion)
		{
			if (dragging)
			{
				var theme = (Control)Themes[selected_tuid];
				InputEventMouseMotion _mouse_event=(InputEventMouseMotion)_event;
				var pos=prev_pos+_mouse_event.Position-start_pos;
				
//				selected_deco.GetNode<Node2D>("Anchor").Position=pos/theme.Size;
				
				selected_deco.SetAnchor(Side.Left,pos.X/image_prev.Size.X,true,true);
				selected_deco.SetAnchor(Side.Top,pos.Y/image_prev.Size.Y,true,true);
				selected_deco.SetAnchor(Side.Right,selected_deco.AnchorLeft,true,true);
				selected_deco.SetAnchor(Side.Bottom,selected_deco.AnchorTop,true,true);
				selected_deco.Position=pos;
				selected_deco.Size=Vector2.Zero;
				
				Update();
			}
		}
	}
	static Vector2 image_size = new Vector2(440, 330);
	static Control margins;
	static Control offsets;
	void _AdjLinePos(float v) { AdjLinePos(); }
	void _AdjTextCase(float v) { AdjTextCase(); }
	static void AdjLinePos()
	{

		prev.GetNode<Line2D>("Lines/L").Position = 
		new Vector2(

			(float)margins.GetNode<SpinBox>("LMargin").Value, 0
			
			) * scale+Vector2.Up*56;
		
		prev.GetNode<Line2D>("Lines/R").Position = new Vector2(

			(float)(image_size.X - margins.GetNode<SpinBox>("RMargin").Value), 0
			
			) * scale+Vector2.Up*56;

		prev.GetNode<Line2D>("Lines/U").Position = new Vector2(
			
			0, (float)margins.GetNode<SpinBox>("UMargin").Value
			
			) * scale+Vector2.Left*107;

		prev.GetNode<Line2D>("Lines/B").Position = new Vector2(
			
			0, (float)(image_size.Y - margins.GetNode<SpinBox>("BMargin").Value)
			
			) * scale+Vector2.Left*107;

		Update();
	}
	static void AdjTextCase()
	{
		//
		var text_case = prev.GetNode<Control>("Preview2D/Image/TextCase");

		text_case.AnchorLeft = (float)offsets.GetNode<SpinBox>("LOffset").Value;
		text_case.AnchorRight = (float)offsets.GetNode<SpinBox>("ROffset").Value;
		text_case.AnchorTop = (float)offsets.GetNode<SpinBox>("UOffset").Value;
		text_case.AnchorBottom = (float)offsets.GetNode<SpinBox>("BOffset").Value;
		Update();
	}
	static float scale = 1;
	void Scale(float _scale)
	{
		var prev2d = prev.GetNode<Node2D>("Preview2D");
		prev2d.Scale = Vector2.One * _scale;
		scale = _scale;
		AdjLinePos();
	}
	static void LinkDeco(Node n,Node n_dup)
	{
			n_dup.SetMeta("UName", n.Name);
			n.SetMeta("prev_name",n_dup.Name);
	}
	public static void ThemeSelected()
	{
		if (!Themes.ContainsKey(selected_tuid))
		{
			return;
		}
		loading=true;
		var theme = (Control)Themes[selected_tuid];
		var patch_rect = theme.GetNode<NinePatchRect>("Texture");
		var text_case = theme.GetNode<RichTextLabel>("TextCase");
		
		var Decos = theme.GetNode("Decos").GetChildren();
		
		//display decos
		selected_deco = null;
		var explorer = panel.GetNode<Panel>("DecoExplorer");
		var deco_list = explorer.GetNode<VBoxContainer>("Scr/VBox");
		foreach (var deco in deco_list.GetChildren())
		{
			deco.QueueFree();
		}
		foreach (var deco in image_prev.GetNode("Decos").GetChildren())
		{
			deco.QueueFree();
		}
		foreach (var n in Decos)//list decos in the explorer
		{
			n.SetMeta("changed",true);
			
			var n_dup = n.Duplicate();
			
			image_prev.GetNode("Decos").AddChild(n_dup);

			LinkDeco(n,n_dup);

			n_dup.GetNode<AnimatedSprite2D>("Sprite").Visible=true;
			var u_dup = explorer.GetNode("Unit").Duplicate();
			u_dup.SetMeta("UName", n.Name);
			deco_list.AddChild(u_dup);
			((Control)u_dup).Visible=true;

		}
		//set params
		texture_name.Text = "[Image]";
		foreach (var e in editor.Elements)
		{
			if (e.euid == (int)theme.GetMeta("image_euid"))
			{
				texture_name.Text = e.Name;
			}
		}
		

		margins.GetNode<SpinBox>("LMargin").Value = patch_rect.PatchMarginLeft;
		//GD.Print(patch_rect.PatchMarginRight);
		margins.GetNode<SpinBox>("RMargin").Value = patch_rect.PatchMarginRight;
		//GD.Print(margins.GetNode<SpinBox>("RMargin").Value);
		margins.GetNode<SpinBox>("UMargin").Value = patch_rect.PatchMarginTop;
		margins.GetNode<SpinBox>("BMargin").Value = patch_rect.PatchMarginBottom;

		offsets.GetNode<SpinBox>("LOffset").Value = text_case.AnchorLeft;
		offsets.GetNode<SpinBox>("ROffset").Value = text_case.AnchorRight;
		offsets.GetNode<SpinBox>("UOffset").Value = text_case.AnchorTop;
		offsets.GetNode<SpinBox>("BOffset").Value = text_case.AnchorBottom;

		panel.GetNode<OptionButton>("HStretch").Selected = (int)patch_rect.AxisStretchHorizontal;
		panel.GetNode<OptionButton>("VStretch").Selected = (int)patch_rect.AxisStretchVertical;
		panel.GetNode<LineEdit>("Name").Text= (string)theme.GetMeta("name");

		
		loading=false;
		Update();
		AdjLinePos();
 		AdjTextCase();
		
	}
	public static void DecoSelected()
	{
		if (not_exist())
		{
			return;
		}
		panel.GetNode<SpinBox>("Rotation").Value=selected_deco.RotationDegrees;
		panel.GetNode<SpinBox>("Size").Value=selected_deco.Scale.X;
		deco_editor.GetNode<LineEdit>("EditPanel/Name").Text= (string)selected_deco.GetMeta("name");
	}

	void FilesSelected(string[] paths)
	{
		foreach (var path in paths)
		{


			var ext = path.GetExtension().ToLower();

			if (ext == ".tscn")
			{
				var list = GetNode<Control>("Panel/Explorer/Scr/VBox");
				var button = (Button)GetNode<Button>("Panel/Explorer/Unit").Duplicate();
				var tuid = assign_tuid();
				var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
				if (file != null)
				{
					//clear all children
					var custom = ResourceLoader.Load<PackedScene>(paths[0]).Instantiate();
					Control theme = null;
					try
					{
						theme = (Control)custom;
					}
					catch (Exception)
					{

					}
					if (theme != null)
					{
						Themes.Add(tuid, theme);
						button.GetChild<Marker2D>(0).Position = tuid * Vector2.Right;
						list.AddChild(button);
					}

				}
			}


		}
	}

	void Apply()
	{
		
		if (Themes.ContainsKey(selected_tuid))
		{
			var uid = (long)GetTree().Root.GetNode<Marker2D>("Editor/PropertyPanel/current_uid").Position.X;
			editor.Event _e=null;
			if (editor.EventStream.Count>0)
			{
				 _e = editor.EventStream.Where(_e => _e.uid == uid).First();
			}
			
			if (_e is editor.GeneralTextEvent)
			{
				((editor.GeneralTextEvent)_e).ui_override_tuid = selected_tuid;
			}
			else if (_e is editor.PlotEvent)
			{
				((editor.PlotEvent)_e).ui_override_tuid = selected_tuid;
			}
		}

		Visible = false;
	}
	void Close()
	{
		Visible = false;
	}
}
