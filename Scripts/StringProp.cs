using Godot;
using System;
using System.Linq;
using System.Reflection;
public partial class StringProp : Panel
{
	// Called when the node enters the scene tree for the first time.
	long uid;
	Control panel;
	editor.Event _E;
	PropertyInfo prop;
	public override void _Ready()
	{
		panel=editor.prop_panel;
		uid= (long)editor.prop_list_uid.Position.X;
		_E=editor.SelectedEvents.Where(e=>e.uid==uid).Select(e=>e).First();

		Type t = _E.GetType(); //
		//PropertyInfo prop = t.GetProperties().Where(p=>p.Name==GetNode<Label>("PropName").Text).Select(p=>p).First();
		PropertyInfo prop = t.GetProperty(GetNode<Label>("PropName").Text);
		var str=(string)prop.GetValue(_E);
		GD.Print(GetNode<Label>("PropName").Text);
		GetNode<Label>("Preview").Text=str;
		
		
		
	}
	void Edit()
	{
		/*Type t = _E.GetType(); PropertyInfo prop = t.GetProperty(GetNode<Label>("PropName").Text);
		prop.SetValue(_E,text);
		EventPanel.UpdateStream();*/
		var text_edit=GetTree().Root.GetNode<Window>("Editor/EventPanel/TextEditor");
		text_edit.GetNode<Label>("Panel/TargetPath").Text=GetPath();
		text_edit.GetNode<Label>("Panel/PropName").Text=GetNode<Label>("PropName").Text;
		
		Type t = _E.GetType();
		PropertyInfo prop = t.GetProperty(GetNode<Label>("PropName").Text);
		var str=(string)prop.GetValue(_E);
		//GetNode<LineEdit>("Input").Text=str;
		
		text_edit.GetNode<TextEdit>("Panel/TextEdit").Text=str;
		
		if (_E is editor.GeneralTextEvent)
		{
			var _GTE=(editor.GeneralTextEvent)_E;
			var check=text_edit.GetNode<CheckBox>("Centered");
			if (prop == t.GetProperty("Text"))
			{
				
				check.Visible=true;
				check.ButtonPressed=_GTE.Centered;
			}else{
				check.Visible=false;
			}
		}
		text_edit.Popup();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
}
