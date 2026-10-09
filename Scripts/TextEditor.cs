using Godot;
using System;
using System.Linq;
using System.Reflection;
public partial class TextEditor : Window
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
	long uid;
	editor.Event _E;
	 void Done()
	{
		/*Type t = _E.GetType(); PropertyInfo prop = t.GetProperty(GetNode<Label>("PropName").Text);
		prop.SetValue(_E,text);
		EventPanel.UpdateStream();*/
	//	var prop_instance=editor.root.GetNode<Panel>((NodePath)editor.root.GetNode<Label>("EventPanel/TextEditor/Panel/TargetPath").Text);
		uid= (long)editor.prop_list_uid.Position.X;
		_E=editor.SelectedEvents.Where(e=>e.uid==uid).Select(e=>e).First();
		Type t = _E.GetType(); PropertyInfo prop = t.GetProperty(GetNode<Label>("Panel/PropName").Text);
		prop.SetValue(_E,GetNode<TextEdit>("Panel/TextEdit").Text);
		if (_E is editor.GeneralTextEvent)
		{
			var _GTE=(editor.GeneralTextEvent)_E;
			var check=GetNode<CheckBox>("Centered");
			if (prop == t.GetProperty("Text"))
			{
				
				_GTE.Centered=check.ButtonPressed;
				
			}
		}
		EventPanel.UpdateStream();
		editor.select_event(uid);
		Visible=false;
	}
	void Close()
	{
		Visible=false;
	}
}
