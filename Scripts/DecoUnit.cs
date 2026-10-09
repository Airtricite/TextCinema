using Godot;
using System;

public partial class DecoUnit : Button
{
	void Select()
	{
		 CustomTheme.DeselectAll();
		CustomTheme.selected_deco= (Control)((Control)CustomTheme.Themes[CustomTheme.selected_tuid]).GetNode((NodePath)("Decos/"+(string)GetMeta("UName")));
		GetNode<ReferenceRect>("Highlight").Visible=true;
		CustomTheme.DecoSelected();
	}
}
