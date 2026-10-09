using Godot;
using Godot.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;
using static ElementExplorer;
using Newtonsoft.Json;
using K4os.Compression.LZ4.Streams;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

public partial class editor : Node2D
{
	public static Node root;
	public static PackedScene _Mark = ResourceLoader.Load<PackedScene>("res://Scenes/Mark.tscn");//mark node

	public enum ArchiveEventMode
	{
		Save,
		Load,
		New,
		Duplicate
	}
	public enum Where
	{
		General,
		BaseLine,
		Camera
	}
	public enum _shape
	{
		Circle,
		Square,
		Diamond,
	}
	public class Event
	{
		public long frame_stamp{get;set;}
		public float posX{get;set;}
        public Node2D Entity{get;set;}//可不用参与序列化
		public Button button{get;set;}//同上
		public long uid{get;set;}
		public Color color{get;set;}//==event entity color
		public string name{get;set;}//==
		public int type{get;set;}
		public Hashtable tweens=new Hashtable();//{tweened_property=[tween_curve_id,easetype,duration,is elementprop,enabled]}
		public Hashtable ToDict(){
			Hashtable dict=new Hashtable();
			_ToDict(dict);
			return dict;
		}
		internal Hashtable _ToDict(Hashtable dict){
			dict["frame_stamp"]=frame_stamp;
			dict["posX"]=posX;
			dict["uid"]=uid;
			dict["color"]=color.ToHtml();
			dict["name"]=name;
			dict["type"]=type;
			var tprops=new Hashtable();
			foreach (var key in tweens.Keys)
			{
				tprops[key]=tweens[key]; //Convert.ToBase64String(GD.VarToBytesWithObjects((Variant)tweens[key]));//??
			}
			dict["tweens"]=tprops;//JsonConvert.SerializeObject(tprops);
			return dict;
		}
		public void FromDict(Hashtable dict)
		{
			_FromDict(dict);
		}
		internal void _FromDict(Hashtable dict)
		{
			frame_stamp=long.Parse((string)dict["frame_stamp"]);
			posX= float.Parse((string)dict["posX"]);
			uid= long.Parse((string)dict["uid"]);
			color= Color.FromHtml((string)dict["color"]);
			GD.Print("color:"+color);
			name= (string)dict["name"];
            type = int.Parse((string)dict["type"]);
			var tprops=new Hashtable();
			foreach (DictionaryEntry pair in (Hashtable)dict["tweens"])
			{
				debug("tprops:"+pair.Value);
				var tprop=JsonConvert.DeserializeObject<List<object>>((string)pair.Value);
				tprops[pair.Key]=tprop;//GD.BytesToVarWithObjects(Convert.FromBase64String((string)pair.Value));
			}
			tweens=tprops;
        }
    }
	public class GeneralTextEvent:Event
	{
		public string Text{get;set;}
		public int TextSize{get;set;}//44
		public float PopupDuration{get;set;}//2.5
		public float HorizontalPadding{get;set;}//positive=Left Padding,Negative = Right Padding, range from -1 to 1 
		public long ui_override_tuid=-1;
		public Color UIColorMod{get;set;}
		public bool Centered=true;
		public new Hashtable ToDict(){
			Hashtable dict=new Hashtable();
			_ToDict(dict);
			Encoding custom=new CustomEncoder();
			
				dict["UIOverride"]=ui_override_tuid;
			dict["ui_color_mod"]=color.ToHtml();
			dict["HorizontalPadding"]=HorizontalPadding;
			dict["Centered"]=Centered;
			dict["Text"]=Text;
			dict["TextSize"]=TextSize;dict["PopupDuration"]=PopupDuration;
			return dict;
		}
		public new void FromDict(Hashtable dict)
		{
			_FromDict(dict);
			Encoding custom=new CustomEncoder();
			GD.Print("uio:"+dict["UIOverride"].GetType());
			if ((string)dict["UIOverride"]!="")
			{				
				
				ui_override_tuid= long.Parse((string)dict["UIOverride"]);
			}
			HorizontalPadding=float.Parse((string)dict["HorizontalPadding"]);
			UIColorMod= Color.FromHtml((string)dict["ui_color_mod"]);
			Text= (string)dict["Text"];
			TextSize= int.Parse((string)dict["TextSize"]);
			PopupDuration= float.Parse((string)dict["PopupDuration"]);
				Centered= bool.Parse((string)dict["Centered"]);
		}
    }
	public class PlotEvent:Event
	{
		public string Text{get;set;}
		public int TextSize{get;set;}//36
		public float WriteSpeed{get;set;}//10(character/sec)or(alphabet or number or punctuation mark/half sec)
		public long ui_override_tuid=-1;//-1 = Not Changing
		public Color UIColorMod{get;set;}
		public bool UseDefaultStyle{get;set;}
	public new Hashtable ToDict(){
		Encoding custom=new CustomEncoder();
			Hashtable dict=new Hashtable();
			_ToDict(dict);
			
				
				dict["UIOverride"]=ui_override_tuid;
			dict["ui_color_mod"]=color.ToHtml();
				
			
				dict["Text"]=Text;
			dict["TextSize"]=TextSize;dict["WriteSpeed"]=WriteSpeed;
			dict["UseDefaultStyle"]=UseDefaultStyle;
			return dict;
		}
		public new void FromDict(Hashtable dict)
		{
			Encoding custom=new CustomEncoder(); 
			_FromDict(dict);
			if ((string)dict["UIOverride"]!="")
			{				
				ui_override_tuid= long.Parse((string)dict["UIOverride"]);
			}
			UIColorMod= Color.FromHtml((string)dict["ui_color_mod"]);
			Text= (string)dict["Text"];
			TextSize= int.Parse((string)dict["TextSize"]);
			WriteSpeed= float.Parse((string)dict["WriteSpeed"]);
			UseDefaultStyle= bool.Parse((string)dict["UseDefaultStyle"]);
		}
	}
	public class ArchiveEvent:Event
	{
		public ArchiveEventMode Mode;
		public string ArchiveName{get;set;}
		public new Hashtable ToDict(){
			Hashtable dict=new Hashtable();
			_ToDict(dict);
			
			
				dict["Mode"]=(int)Mode;
				dict["ArchiveName"]=ArchiveName;
			
			return dict;
		}
		public new void FromDict(Hashtable dict)
		{
			_FromDict(dict);
			Mode= (ArchiveEventMode)int.Parse((string)dict["Mode"]);
			ArchiveName= (string)dict["ArchiveName"];
        }
	}
	public class ElementEvent:Event
	{
		public Element element{get;set;}
		public Color ContentColor{get;set;}
		public Color BackgroundColor{get;set;}
		public string PlaceHolderInfo{get;set;}
		public Hashtable element_props{get;set;}//{"Position"=0,0,"Rotation"=...}
		public Where belongs_to{get;set;}//if this is physicselement then BaseLine=>GeneralSpace
		public bool DeleteMode{get;set;}
		public string uuid{get;set;}
		public new Hashtable ToDict(){
			Hashtable dict=new Hashtable();
			_ToDict(dict);
			E_ToDict(dict);
			return dict;
		}
		internal void E_ToDict(Hashtable dict)
		{
			if (element!=null)
			{
				dict["element"]=element.euid;
				debug(name+"]"+"[ElementEvent.element.euid]="+dict["element"].ToString());
			}else{
				dict["element"]=null;
			}
			//   EUID
			dict["belongs_to"]=(int)belongs_to;
			var props=new Hashtable();
			foreach (DictionaryEntry pair in element_props)
			{
				props[pair.Key]=Convert.ToHexString(GD.VarToBytes((dynamic)pair.Value));//??
			}
			dict["element_props"]=props;//JsonConvert.SerializeObject(props);
			dict["uuid"]=uuid;
			dict["info"]=PlaceHolderInfo;
			dict["content color"]=ContentColor.ToHtml();
			dict["bgcolor"]=BackgroundColor.ToHtml();
			dict["DeleteMode"]=DeleteMode;
		}
		public new void FromDict(Hashtable dict)
		{
			_FromDict(dict);
			E_FromDict(dict);
        }
		internal void E_FromDict(Hashtable dict)
		{
			if ((string)dict["element"]!="")
			{
				var target_euid=long.Parse((string)dict["element"]);
				debug("[ElementEvent.target_euid]="+target_euid.ToString());
				if (Elements.Count>0){
				var ie=Elements.Where(e=>{return e.euid==target_euid;});
				debug("[Elements.Count]"+Elements.Count());
				debug("[ElementEvent.var_ie]="+ie.Count());
				if (ie.Count()>0)
				{
					element=ie.First();
					debug("[ElementEvent.element]="+element);
				}
				}
			}
			belongs_to= (Where)int.Parse((string)dict["belongs_to"]);
			if (dict.ContainsKey("DeleteMode"))
			{
				DeleteMode=bool.Parse((string)dict["DeleteMode"]);
			}
			
			var props=new Hashtable();
			foreach (DictionaryEntry pair in (Hashtable)dict["element_props"])
			{
				props[pair.Key]=GD.BytesToVar(Convert.FromHexString((string)pair.Value));//??
			}
			element_props=props;
			uuid= (string)dict["uuid"];
			PlaceHolderInfo=(string)dict["info"];
			ContentColor= Color.FromHtml((string)dict["content color"]);
			BackgroundColor=Color.FromHtml((string)dict["bgcolor"]);
		}
	}
	public class PhysicsEvent:Event
	{
		public float GravityDirection{get;set;}
		public float Magnitude{get;set;}
		public new Hashtable ToDict(){
			Hashtable dict=new Hashtable();
			_ToDict(dict);
			dict["GravityDirection"]=GravityDirection;dict["Magnitude"]=Magnitude;
			return dict;
		}
		public new void FromDict(Hashtable dict)
		{
			_FromDict(dict);
			GravityDirection= float.Parse((string)dict["GravityDirection"]);
			Magnitude= float.Parse((string)dict["Magnitude"]);

        }
	}
	public class PhysicsElementEvent:ElementEvent//spawn a body, no  tweening
	{
		//CollisionShape
		public RigidBody2D rigid_body;
		public CollisionShape2D Hitbox;//edit circle rect capsule polygon
		public Vector2 SpawnPos;	//global						//Global
		public Vector2 SpawnVelocity;
		public float SpawnAngularVelocity;//degree/s
		public float Mass{get;set;}
		public float Friction{get;set;}
		public float Bounce{get;set;}
		public float GravityScale{get;set;}
		public HashSet<int> collision_layer;
		public HashSet<int> collision_mask;
		public new Hashtable ToDict(){
			Hashtable dict=new Hashtable();
			_ToDict(dict);
			E_ToDict(dict);
			dict["Hitbox"]=Convert.ToHexString(GD.VarToBytesWithObjects(Hitbox));
			dict["SpawnPos"]=Convert.ToHexString(GD.VarToBytes(SpawnPos));
			dict["SpawnVelocity"]=Convert.ToHexString(GD.VarToBytes(SpawnVelocity));
			dict["SpawnAngularVelocity"]=SpawnAngularVelocity;
			dict["Mass"]=Mass;
			dict["Friction"]=Friction;
			dict["Bounce"]=Bounce;
			dict["GravityScale"]=GravityScale;
			var layer_data=new HashSet<byte>();
			var mask_data=new HashSet<byte>();
			collision_layer.All(i=>{layer_data.Add((byte)i);return true;});
			collision_mask.All(i=>{mask_data.Add((byte)i);return true;});
			dict["collision_layer"]=Convert.ToHexString(layer_data.ToArray());
			dict["collision_mask"]=Convert.ToHexString(mask_data.ToArray());
			return dict;
		}
		public new void FromDict(Hashtable dict)
		{
			_FromDict(dict);
			E_FromDict(dict);
			Hitbox = (CollisionShape2D)GD.BytesToVarWithObjects(Convert.FromHexString((string)dict["Hitbox"]));
			SpawnPos = (Vector2)GD.BytesToVar(Convert.FromHexString((string)dict["SpawnPos"]));
			SpawnVelocity=(Vector2)GD.BytesToVar(Convert.FromHexString((string)dict["SpawnVelocity"]));
			SpawnAngularVelocity= float.Parse((string)dict["SpawnAngularVelocity"]);
			Mass= float.Parse((string)dict["Mass"]);
			Friction= float.Parse((string)dict["Friction"]);
			Bounce= float.Parse((string)dict["Bounce"]);
            GravityScale = float.Parse((string)dict["GravityScale"]);
			HashSet<int> TableToSet(Hashtable table)
			{
				return (HashSet<int>)table.Values;

            }

            collision_layer = new HashSet<int>();
            collision_mask = new HashSet<int>();
			Convert.FromHexString((string)dict["collision_layer"]).All(bt=>{collision_layer.Add((int)bt);return true;});
			Convert.FromHexString((string)dict["collision_mask"]).All(bt=>{collision_mask.Add((int)bt);return true;});
        }
	}
	public class SpecialEvent:Event
	{
		public AnimationPlayer custom_action;
		public new Hashtable ToDict(){
			Hashtable dict=new Hashtable();
			_ToDict(dict);
			Encoding custom=new CustomEncoder();
			if (custom_action!=null)
			{
				dict["custom_action"]=custom.GetString(GD.VarToBytesWithObjects(PackUp(custom_action)));
			}else
			{
				dict["custom_action"]=null;
			}
			
			return dict;
		}
		public new void FromDict(Hashtable dict)
		{
			_FromDict(dict);//20241117继续往下写 FromDict函数
			Encoding custom=new CustomEncoder();
			if ((string)dict["custom_action"]!="")
			{
				var cus_a=((PackedScene)GD.BytesToVarWithObjects(custom.GetBytes((string)dict["custom_action"]))).Instantiate();
				if (cus_a is AnimationPlayer)
				{
					custom_action=(AnimationPlayer)cus_a;
				}else
				{
					cus_a.QueueFree();
				}
			}
			
	
		}
	}
	public class GeneralTextRemoveEvent:Event
	{
		public int index_from_the_latest_one{get;set;}//1=latest 2=second latest
		public int amount{get;set;}//for example 3, will delete the latest, the 2nd latest and the 3rd latest at once until clear
		public float ease_duration{get;set;}
		public new Hashtable ToDict(){
			Hashtable dict=new Hashtable();
			_ToDict(dict);
			dict["index"]=index_from_the_latest_one;
			dict["amount"]=amount;
			dict["ease_duration"]=ease_duration;
			return dict;
		}
		public new void FromDict(Hashtable dict)
		{
			_FromDict(dict);
			index_from_the_latest_one= int.Parse((string)dict["index"]);
			amount= int.Parse((string)dict["amount"]);
			ease_duration= float.Parse((string)dict["ease_duration"]);

        }
	}
	public class GUIEvent:Event
	{
		public Vector2 BasePosition{get;set;}
		public float BaseRotation{get;set;}
		public Color LineColor{get;set;}
		public Vector2 GeneralPos{get;set;}
		public float GeneralRot{get;set;}
		public Vector2 GeneralSize{get;set;}
		public Vector2 GeneralPivotOffset{get;set;}
		public Vector2 PlotPos{get;set;}
		public float PlotRot{get;set;}
		public Vector2 PlotSize{get;set;}
		public Vector2 PlotPivotOffset{get;set;}
		public bool ClearGeneralUnits{get;set;}//clear anim:{fade, stream away,}
		public bool ClearPlotTexts{get;set;}
		public Vector2 CameraPosition{get;set;}
		public float CameraRotation{get;set;}
		public Vector2 CameraZoom{get;set;}
		public new Hashtable ToDict(){
			Hashtable dict=new Hashtable();
			_ToDict(dict);
			dict["BasePosition"]=Convert.ToHexString(GD.VarToBytes(BasePosition));
			dict["BaseRotation"]=BaseRotation;
			dict["GeneralPos"]=Convert.ToHexString(GD.VarToBytes(GeneralPos));
			dict["GeneralRot"]=GeneralRot;
			dict["GeneralSize"]=Convert.ToHexString(GD.VarToBytes(GeneralSize));
			dict["GeneralPivotOffset"]=Convert.ToHexString(GD.VarToBytes(GeneralPivotOffset));
			dict["PlotPos"]=Convert.ToHexString(GD.VarToBytes(PlotPos));
			dict["PlotRot"]=PlotRot;
			dict["PlotSize"]=Convert.ToHexString(GD.VarToBytes(PlotSize));
			dict["PlotPivotOffset"]=Convert.ToHexString(GD.VarToBytes(PlotPivotOffset));
			dict["ClearGeneralUnits"]=ClearGeneralUnits;
			dict["ClearPlotUnits"]=ClearPlotTexts;
			dict["CameraPosition"]=Convert.ToHexString(GD.VarToBytes(CameraPosition));
			dict["CameraRotation"]=CameraRotation;
			dict["CameraZoom"]=Convert.ToHexString(GD.VarToBytes(CameraZoom));
			dict["LineColor"]=Convert.ToHexString(GD.VarToBytes(LineColor));
			return dict;
		}
		public new void FromDict(Hashtable dict)
		{
			_FromDict(dict);
			BasePosition		= (Vector2)GD.BytesToVar(Convert.FromHexString((string)dict["BasePosition"]));

            BaseRotation		= float.Parse((string)dict["BaseRotation"]);

            GeneralPos			=	(Vector2)GD.BytesToVar(Convert.FromHexString((string)dict["GeneralPos"]));
			GeneralRot			=	float.Parse((string)dict["GeneralRot"]);
			GeneralSize			=	(Vector2)GD.BytesToVar(Convert.FromHexString((string)dict["GeneralSize"]));
			GeneralPivotOffset	=	(Vector2)GD.BytesToVar(Convert.FromHexString((string)dict["GeneralPivotOffset"]));		
			PlotPos				=	(Vector2)GD.BytesToVar(Convert.FromHexString((string)dict["PlotPos"]));		
			PlotRot				=	float.Parse((string)dict["PlotRot"]);
			PlotSize			=	(Vector2)GD.BytesToVar(Convert.FromHexString((string)dict["PlotSize"]));		
			PlotPivotOffset		=	(Vector2)GD.BytesToVar(Convert.FromHexString((string)dict["PlotPivotOffset"]));			
			ClearGeneralUnits	= bool.Parse((string)dict["ClearGeneralUnits"]);		
			ClearPlotTexts		=	 bool.Parse((string)dict["ClearPlotUnits"]);	
			CameraPosition		=			(Vector2)GD.BytesToVar(Convert.FromHexString((string)dict["CameraPosition"]));	
			CameraRotation		=		float.Parse((string)dict["CameraRotation"]);
			CameraZoom			=		(Vector2)GD.BytesToVar(Convert.FromHexString((string)dict["CameraZoom"]));	
			LineColor			=	(Color)GD.BytesToVar(Convert.FromHexString((string)dict["LineColor"]));	
		}
	}
	public class Mark:Event
	{
		public _shape Shape;
		public float scale{get;set;}
		public new Hashtable ToDict(){
			Hashtable dict=new Hashtable();
			_ToDict(dict);
			dict["shape"]=(int)Shape;
			dict["scale"]=scale;
			return dict;
		}
		public new void FromDict(Hashtable dict)
		{
			_FromDict(dict);
			Shape= (_shape)int.Parse((string)dict["shape"]);
			scale= float.Parse((string)dict["scale"]);
        }
    }
	public class BPMMark:Mark
	{
		public int Amount{get;set;}
		public float BPM{get;set;}
		public int div{get;set;}// 4/4 default
		public new Hashtable ToDict(){
			Hashtable dict=new Hashtable();
			_ToDict(dict);
			dict["shape"]=(int)Shape;
			dict["scale"]=scale;
			dict["Amount"]=Amount;
			dict["BPM"]=BPM;
			dict["div"]=div;
			return dict;
		}
		public new void FromDict(Hashtable dict)
		{
			_FromDict(dict);
			Shape= (_shape)int.Parse((string)dict["shape"]);
			scale= float.Parse((string)dict["scale"]);
			Amount= int.Parse((string)dict["Amount"]);
			BPM= float.Parse((string)dict["BPM"]);
			div= int.Parse((string)dict["div"]);

		}
    }
	
	static float range(float y,float[] range_in,float[] range_out)
	{
		if (range_in[1]==0)
		{
			return range_out[0];
		}
		return (y-range_in[0])*(range_out[1]-range_out[0])/range_in[1]+range_out[0];
	}
	
	public class CameraShakeEvent:Event
	{
		//shake state LFO 
		public float Amplitude{get;set;}//leave blank as you want it to keep itself going on, otherwise it will replace the running lfo instantly 
		
		public float RotationAmp{get;set;}
		public float MultiplierX{get;set;}
		public float MultiplierY{get;set;}
		public float Frequency{get;set;}
		public new Hashtable ToDict(){
			Hashtable dict=new Hashtable();
			_ToDict(dict);
			dict["Amplitude"]=Amplitude;
			dict["RotationAmp"]=RotationAmp;
			dict["Frequency"]=Frequency;
			dict["MultiplierX"]=MultiplierX;
			dict["MultiplierY"]=MultiplierY;
			return dict;
		}
		public new void FromDict(Hashtable dict)
		{
			_FromDict(dict);
			Amplitude=float.Parse((string)dict["Amplitude"]);
			RotationAmp=float.Parse((string)dict["RotationAmp"]);
			Frequency=float.Parse((string)dict["Frequency"]);
			MultiplierX=float.Parse((string)dict["MultiplierX"]);
			MultiplierY=float.Parse((string)dict["MultiplierY"]);
		}
	}

	public class EventGroup:Event
	{
		//shake state LFO 
		public HashSet<Event> Stream=new HashSet<Event>();
		public string VarString {get;set;}
		public new Hashtable ToDict(){
			Hashtable dict=new Hashtable();
			_ToDict(dict);

			/*var stream=new Hashtable();
			foreach (DictionaryEntry pair in Stream)
			{
				stream[pair.Key]=Convert.ToHexString(GD.VarToBytes((dynamic)pair.Value));//??
			}*/
			dict["stream"]=SerializeEvents(Stream);//JsonConvert.SerializeObject(props);
			dict["VarString"]=VarString;
			return dict;
		}
		public new void FromDict(Hashtable dict)
		{
			_FromDict(dict);
		//var stream=new Hashtable();
		    Stream=DeserializeEvents((string)dict["stream"]);
			if (dict.ContainsKey("VarString"))
			{
				VarString=(string)dict["VarString"];
			}
			
			/*foreach (DictionaryEntry pair in (Hashtable)dict["element_props"])
			{
				stream[pair.Key]=GD.BytesToVar(Convert.FromHexString((string)pair.Value));//??
			}
			Stream=stream;*/
		}
	}
	
	public static Script ProgressNodeScript = ResourceLoader.Load<Script>("res://Scripts/ProgressNode.cs");
	public class Element//一个element是一个单独的个体，被events所复制引用
	{
		//public Element[] SubElements{get;set;}
		public string Name{get;set;}
		public Node2D Node{get;set;}
		public long euid{get;set;}
		//public bool instantiated=false;
		public Button entity;
		public ElementExplorer.ElementType type{get;set;}
		public byte[] NodeData;
		public List<byte[]> FrameData=new List<byte[]>();
		public void PrewriteNodeData()
		{//8+2+1,no “+4” means no script
			NodeData=GD.VarToBytesWithObjects(PackUp(Node));
		}
		public Hashtable ToDict(){
			Hashtable dict=new Hashtable();
			Encoding custom = new CustomEncoder();
		
			dict["Name"]=Name;dict["euid"]=euid;dict["type"]=(int)type;
			debug("euid:"+euid);
			if (Node!=null)
			{
				dict["Node"]= custom.GetString(NodeData);//packed node
				//ResourceSaver.Save(PackUp(Node),"res://Projects/test.res");
			}else
			{
				dict["Node"]=null;
			}
			if (FrameData!=null)
			{
				var data=new List<string>();
				foreach (var item in FrameData)
				{
					var _data=custom.GetString(item);
					data.Add(_data);
					GD.Print("[data]:  "+_data);
				}
				GD.PrintErr("[data.Count]:  "+data.Count);
				dict["AnimFrames"]=data;
			}else
			{
				dict["AnimFrames"]=null;
			}
			GD.Print("packed node: ===============",dict["Node"]);
			return dict;
		}
		public void FromDict(Hashtable dict)
		{
			Encoding custom = new CustomEncoder();
			Name= (string)dict["Name"];
			euid= long.Parse((string)dict["euid"]);
			type= (ElementType)int.Parse((string)dict["type"]);
			if ((string)dict["Node"]!="")
			{
				GD.Print("str:"+(string)dict["Node"]);
				//GD.Print("str:"+(string)dict["Node"]);
				NodeData=custom.GetBytes((string)dict["Node"]);
				var scene=(PackedScene)GD.BytesToVarWithObjects(NodeData);
				var node=scene.Instantiate();
				if (type==ElementType.Progress)
				{
					ulong objId = node.GetInstanceId();
				node.SetScript(ProgressNodeScript);
				node = (Node)InstanceFromId(objId);
				}
				
			if (node is Node2D)
			{
				Node=(Node2D)node;
			}else
			{
				node.QueueFree();
			}
			}
			
				if (type==ElementType.Animation||type==ElementType.Image)
				{
					if (dict["AnimFrames"]!=null)
					{
						foreach (var item in JsonConvert.DeserializeObject<List<string>>((string)dict["AnimFrames"]))
						{
							FrameData.Add(custom.GetBytes(item)); 
						}
					}
					if (Node.Material!=null&&Node.Material is CanvasItemMaterial)
					{}else
					{
						Node.Material=new CanvasItemMaterial();
					}
                }
			

			debug("euid:"+euid);

        }
	}
	
	public static byte[] Compress(byte[] data,K4os.Compression.LZ4.LZ4Level level)
    {
        using (var outputStream = new System.IO.MemoryStream())
        using (var compressionStream = LZ4Stream.Encode(outputStream,level,0,true))
        {
            compressionStream.Write(data, 0, data.Length);
            compressionStream.Close();
            return outputStream.ToArray();
        }
    }

   public static byte[] Decompress(byte[] data)
    {	
        using (var inputStream = new System.IO.MemoryStream(data))
        using (var decompressionStream = LZ4Stream.Decode(inputStream,0,true))
        using (var outputStream = new System.IO.MemoryStream())
        {
            decompressionStream.CopyTo(outputStream);
            return outputStream.ToArray();
        }
    }


	static PackedScene PackUp(Node node)
	{
		if (node==null)
		{
			return null;
		}
			var pack=new PackedScene();
			HandleOwner(node);
			pack.Pack(node);
			
		return pack;
			
			
	}
	public static void HandleOwner(Node node)
	{
		void _HandleOwner(Node _node)
		{
			_node.Owner=node;
			foreach (var _child in _node.GetChildren())
			{
				_HandleOwner(_child);
			}
		}
		foreach (var child in node.GetChildren())
		{
			 _HandleOwner(child);
		}
	}
	// Called when the node enters the scene tree for the first time.
	FileDialog SaveDialog;
	FileDialog LoadDialog;
	public static Camera2D camera;
	public static Hashtable ProjectData=new Hashtable(); 
	public static HashSet<Event> EventStream=new HashSet<Event>();
	public static HashSet<Event> GlobalEventStream=new HashSet<Event>();
	public static HashSet<Element> Elements=new HashSet<Element>();

	public static HashSet<Event> SelectedEvents=new HashSet<Event>();
	public static HashSet<Event> CopiedEvents=new HashSet<Event>();
	public static HashSet<Element> SelectedElements=new HashSet<Element>();
	public static HashSet<Element> CopiedElements=new HashSet<Element>();
	public static HashSet<Event> RecycledEvents=new HashSet<Event>();
	public static HashSet<Element> RecycledElements=new HashSet<Element>();
	public static long id_assigning_index=0;
	public static long euid_assigning_index=0;
	static Control str_prop;
	static Control num_prop;
	static Control elem_prop;
	static Control color_prop;
	static Control content_color_prop;
	static Control trans_prop;
	static Control belongs_2_prop;
	static Control physicsbody_prop;
	static Control bool_prop;
	static Control int_prop;
	static Control tween_ctrl;
	static Window popup;
static Control archive_event_mode_prop;
static Button theme_override;
static Label double_click_tip;
	public static Control prop_panel;
	static Control prop_list;
	public static Marker2D prop_list_uid;
	public static ReferenceRect selection_rect;
	public static Window physics_center;
	public override void _Ready()
	{
		root=this;	
		SaveDialog = GetNode<FileDialog>("SaveDialog");
		LoadDialog = GetNode<FileDialog>("LoadDialog");
		camera = GetNode<Camera2D>("Camera2D");
		GD.Print("Ready");
		str_prop= GetNode<Control>("PropUnits/StringProp");
		num_prop= GetNode<Control>("PropUnits/NumberProp");
		elem_prop= GetNode<Control>("PropUnits/ElementProp");
		color_prop= GetNode<Control>("PropUnits/ColorProp");
		content_color_prop=GetNode<Control>("PropUnits/ContentColorProp");
		trans_prop= GetNode<Control>("PropUnits/TransformProp");
		belongs_2_prop= GetNode<Control>("PropUnits/BelongsToProp");
		physicsbody_prop=GetNode<Control>("PropUnits/PhysicsBodyProp");
		bool_prop=GetNode<Control>("PropUnits/BoolProp");
		int_prop=GetNode<Control>("PropUnits/IntProp");
		tween_ctrl=GetNode<Control>("PropUnits/TweenControls");
		archive_event_mode_prop= GetNode<Control>("PropUnits/ArchiveEventModeProp");
		theme_override=GetNode<Button>("PropUnits/ThemeOverride");
double_click_tip=GetNode<Label>("PropUnits/DoubleClick");
		prop_panel=GetNode<Control>("PropertyPanel");
		prop_list=GetNode<Control>("PropertyPanel/Scroll/List");
		prop_list_uid=GetNode<Marker2D>("PropertyPanel/current_uid");
		selection_rect=GetNode<ReferenceRect>("SelectionRect");
		element_bin_list=GetNode<VBoxContainer>("RecycleElements/Scr/List");
		event_bin_list=GetNode<VBoxContainer>("RecycleEvents/Scr/List");
		bin_unit=GetNode<Panel>("RecycleBinUnit");

		save_button=GetNode<Button>("EventPanel/Save");
		physics_center=GetNode<Window>("EventPanel/PhysicsCenter");
		PhysicsCenter.id_instance=root.GetNode<Marker2D>("PropertyPanel/current_uid");
		PhysicsCenter.origin=physics_center.GetNode<Node2D>("Panel/HitBoxEdit/Origin");
		PhysicsCenter.hitbox_prev = PhysicsCenter.origin.GetNode<Node2D>("HitBox");
		PhysicsCenter.circle_edit	= physics_center.GetNode<Panel>("Panel/HitBoxEdit/CircleEdit");
		PhysicsCenter.rect_edit	= physics_center.GetNode<Panel>("Panel/HitBoxEdit/RectEdit");
		PhysicsCenter.capsule_edit = physics_center.GetNode<Panel>("Panel/HitBoxEdit/CapsuleEdit");
		PhysicsCenter.polygon_edit = physics_center.GetNode<Panel>("Panel/HitBoxEdit/PolygonEdit");
		PhysicsCenter.point = physics_center.GetNode<Node2D>("point");
		foreach (var e in RecycledEvents)
		{
			if (IsValid(e.Entity))
				{
			e.Entity.QueueFree();
				}
		}
		RecycledEvents.Clear();
		foreach (var e in RecycledElements)
		{
			if (IsValid(e.entity)&&IsValid(e.Node))
				{
					e.entity.QueueFree();
					e.Node.QueueFree();
				}
		}
		RecycledElements.Clear();
		if (GlobalEventStream.Count>0)
		{
			GlobalEventStream.All(e=>{
				if (IsValid(e.Entity))
				{
				e.Entity.QueueFree();
				}return true;
				});
		}
		//EventStream.Clear();
		if (Elements.Count>0)
		{
			Elements.All(e=>{
				if (IsValid(e.entity))
				{
				e.entity.QueueFree();};return true;
				});
		}
		//Elements.Clear();
		
		
		SelectedElements=new HashSet<Element>();
		SelectedEvents=new HashSet<Event>();
		EventStream=new HashSet<Event>();
		GlobalEventStream=new HashSet<Event>();
		Elements=new HashSet<Element>();
		ProjectData=new Hashtable();
		if (!cinema.is_test_playing)
		{
			path_cache=string.Empty;
		}else{
			var _path_cache=path_cache;
			FileConfirmed(cinema.tmov_path);
			path_cache=_path_cache;
			cinema.is_test_playing=false;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	Vector2 cam_range=new Vector2(1600,1200);
	public static bool IsValid(Node n)
	{
		return IsInstanceValid(n) && n!=null;
	}
	public override void _PhysicsProcess(double delta)
	{
		if (!focused&&camera!=null)
		{
			if (cam_move_x!=0||cam_move_y!=0)
			{
				
			
				camera.Position+=new Vector2(cam_move_x,cam_move_y)*cam_speed;
				
				if (camera.Position.X<=-cam_range.X)
				{
					camera.Position=new Vector2(-cam_range.X,camera.Position.Y);
				}else if (camera.Position.X>cam_range.X)
				{
					camera.Position=new Vector2(cam_range.X,camera.Position.Y);
				}
				if (camera.Position.Y<=-cam_range.Y)
				{
					camera.Position=new Vector2(camera.Position.X,-cam_range.Y);
				}else if (camera.Position.Y>cam_range.Y)
				{
					camera.Position=new Vector2(camera.Position.X,cam_range.Y);
				}
			}
		}
		
		if (rect_selecting)
		{
			GD.Print(rect_selecting);
			SelectedEvents.Clear();
		//	var size=selection_rect.Size;
			EventStream.Where(e=>selection_rect.GetRect().HasPoint(e.Entity.GlobalPosition)).Select(e=>e)
			.All(e=>{SelectedEvents.Add(e);return true;});
			EventPanel.UpdateStream();
		}
	}
	int cam_move_x = 0;
	int cam_move_y=0;
	float cam_speed=10;

	public static bool focused=false;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
		{
		InputEventKey key_event=(InputEventKey)@event;
			if (key_event.KeyLabel==Key.W&&key_event.IsPressed())		{cam_move_y=-1;}
			else if(key_event.KeyLabel==Key.W&&!key_event.IsPressed())								{cam_move_y=0;}

			if (key_event.KeyLabel==Key.A&&key_event.IsPressed())		{cam_move_x=-1;}
			else if(key_event.KeyLabel==Key.A&&!key_event.IsPressed())								{cam_move_x=0;}
			//GD.Print(pressing_ctrl);
			if (key_event.KeyLabel==Key.S&&key_event.IsPressed()&&!pressing_ctrl)		{cam_move_y=1;}
			else if(key_event.KeyLabel==Key.S&&!key_event.IsPressed())								{cam_move_y=0;}

			if (key_event.KeyLabel==Key.D&&key_event.IsPressed())		{cam_move_x=1;}
			else if(key_event.KeyLabel==Key.D&&!key_event.IsPressed())								{cam_move_x=0;}
		}
    }
	public static void auto_set(string name)
	{
		if (!ProjectData.ContainsKey(name))
		{
			ProjectData.Add(name,new HashSet<Hashtable>());
		}
	}
	public class CustomEncoder : Encoding
{
	
	private char[] Page=new char[]
	{'0','1','2','3','4','5','6','7','8','9',	//10 
	'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
	'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y','Z',//62
	'\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007', '\u0008', '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u000E', '\u000F',
    '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019', '\u001A', '\u001B', '\u001C', '\u001D', '\u001E', '\u001F',//94
	' ', '!', '#', '$', '%', '&', '(', ')', '*', '+', '-', '.', '/',';', '<', '=', '>', '?','@','^', '_', '`','|','~','\u03C6',//119
	'\u00A1', '\u00A2', '\u00A3', '\u00A4', '\u00A5', '\u00A6', '\u00A7', '\u00A8', '\u00A9', '\u00AA', '\u00AB', '\u00AC', '\u00AD', 
	'\u00AE', '\u00AF', '\u00B0', '\u00B1', '\u00B2', '\u00B3', '\u00B4', '\u00B5', '\u00B6', '\u00B7', '\u00B8', '\u00B9', '\u00BA', 
	'\u00BB', '\u00BC', '\u00BD', '\u00BE', '\u00BF', '\u00C0', '\u00C1', '\u00C2', '\u00C3', '\u00C4', '\u00C5', '\u00C6', '\u00C7', 
	'\u00C8', '\u00C9', '\u00CA', '\u00CB', '\u00CC', '\u00CD', '\u00CE', '\u00CF', '\u00D0', '\u00D1', '\u00D2', '\u00D3', '\u00D4', 
	'\u00D5', '\u00D6', '\u00D7', '\u00D8', '\u00D9', '\u00DA', '\u00DB', '\u00DC', '\u00DD', '\u00DE', '\u00DF', '\u00E0', '\u00E1', 
	'\u00E2', '\u00E3', '\u00E4', '\u00E5', '\u00E6', '\u00E7', '\u00E8', '\u00E9', '\u00EA', '\u00EB', '\u00EC', '\u00ED', '\u00EE', 
	'\u00EF', '\u00F0', '\u00F1', '\u00F2', '\u00F3', '\u00F4', '\u00F5', '\u00F6', '\u00F7', '\u00F8', '\u00F9', '\u00FA', '\u00FB', 
	'\u00FC', '\u00FD',//212
	 '\u0391', '\u0392', '\u0393', '\u0394', '\u0395', '\u0396', '\u0397', '\u0398', '\u0399', '\u039A', '\u039B', '\u039C', '\u039D',
	 '\u039E', '\u039F', '\u03A0', '\u03A1', '\u03A3', '\u03A4', '\u03A5', '\u03A6', '\u03A7', '\u03A8', '\u03A9', '\u03B1', '\u03B2',
	 '\u03B3', '\u03B4', '\u03B5', '\u03B6', '\u03B7', '\u03B8', '\u03B9', '\u03BA', '\u03BB', '\u03BC', '\u03BD', '\u03BE', '\u03BF',
	 '\u03C0', '\u03C1', '\u03C3', '\u03C4', '\u03C5'
	};
    public override byte[] GetBytes(string s)
    {
        byte[] bytes = new byte[s.Length];
		var i=0;
       s.All(ch=>{
		byte bt= (byte)System.Array.IndexOf(Page,ch);
		bytes[i]=bt;
		i++
		;return true;});
        return bytes;
    }

    public override string GetString(byte[] bytes)
    {
        char[] chars = new char[bytes.Length];
       var i=0;
       bytes.All(bt=>{
		char ch = Page[bt];
		chars[i]=ch;
		i++
		;return true;});
        return new string(chars);
    }

   
        public override int GetByteCount(char[] chars, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            throw new NotImplementedException();
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            throw new NotImplementedException();
        }

        public override int GetMaxByteCount(int charCount)
        {
            throw new NotImplementedException();
        }

        public override int GetMaxCharCount(int byteCount)
        {
            throw new NotImplementedException();
        }

    }

/*class Program
{
    static void Main()
    {
        string originalText = "Hello, ☆!";
        Encoding customEncoding = new CustomEncoder();

        byte[] encodedBytes = customEncoding.GetBytes(originalText);
        string decodedText = customEncoding.GetString(encodedBytes);

        Console.WriteLine("Original Text: " + originalText);
        Console.WriteLine("Encoded Bytes: " + BitConverter.ToString(encodedBytes));
        Console.WriteLine("Decoded Text: " + decodedText);
    }
}*/

	public static Type[] EventTypes={typeof(GeneralTextEvent),typeof(PlotEvent),typeof(ElementEvent),typeof(PhysicsEvent),
	typeof(PhysicsElementEvent),typeof(GUIEvent),typeof(GeneralTextRemoveEvent),typeof(Mark),typeof(BPMMark),typeof(ArchiveEvent),typeof(SpecialEvent),typeof(CameraShakeEvent),typeof(EventGroup)};

	public static string path_cache=string.Empty;
	static Button save_button;
	static void _Save(string path)
	{
		Save(path);
		path_cache=path;
	}
	public static string SerializeEvents(HashSet<Event> Events)
	{
		HashSet<Hashtable> event_stream_data=new HashSet<Hashtable>();
		foreach (var e in Events)
		{
			Type T=EventTypes[e.type];
				var data=(Hashtable)T.GetMethod("ToDict").Invoke(e,null);

			event_stream_data.Add(data);
		}
		var json=JsonConvert.SerializeObject(event_stream_data);
		return json;
	} 
	public static void Save(string path)//====================================A=R=C=H=I=V=I=N=G=================================================================
	{Encoding custom= new CustomEncoder();
		//{"event_stream":[],"elements":[]}
		//save events
		GD.Print("Save");
		save_button.Text="Saving...";
		HashSet<Hashtable> event_stream_data=new HashSet<Hashtable>();
		HashSet<Hashtable> element_data=new HashSet<Hashtable>();
		Hashtable theme_data= new Hashtable();

		foreach (var e in Elements)
		{
			var data=e.ToDict();
			element_data.Add(data);
		}
		foreach (var e in EventStream)
		{
			Type T=EventTypes[e.type];
				var data=(Hashtable)T.GetMethod("ToDict").Invoke(e,null);

			event_stream_data.Add(data);
		}
		foreach (DictionaryEntry theme in CustomTheme.Themes)
		{
			
			var data=custom.GetString(GD.VarToBytesWithObjects(PackUp((Node)theme.Value)));
			theme_data.Add(theme.Key,data);
		}
		
		
			
			ProjectData.Clear();
			ProjectData["id_assigning_index"]=id_assigning_index;
			ProjectData["euid_assigning_index"]=euid_assigning_index;
			ProjectData["tuid_assigning_index"]=CustomTheme.tuid_assigner;
			ProjectData["event_stream"]=event_stream_data;
			ProjectData["elements"]=element_data;
			ProjectData["themes"]=theme_data;
		var json=JsonConvert.SerializeObject(ProjectData);
		
		GD.Print(json);
		var bytes=Compress(Encoding.Unicode.GetBytes(json),K4os.Compression.LZ4.LZ4Level.L03_HC);//Compress(GD.VarToBytes(json),K4os.Compression.LZ4.LZ4Level.L03_HC);
		//var save_dialog=root.GetNode<FileDialog>("SaveDialog");
	//	var dir_path=save_dialog.CurrentDir;
	///	GD.Print(path+".tmov");
		System.IO.File.WriteAllBytesAsync(path,bytes);
		//json serialization
		//var(str) to byte 
		//save with extension:.tmov
		save_button.Text="Save";
		
		
	}
	public static void CallSavingDialog()
	{
		if (path_cache!=string.Empty&&path_cache!="")
		{
			_Save(path_cache);
		}else{
		root.GetNode<FileDialog>("SaveDialog").Popup();
		}
	}
	void CallLoadingDialog()
	{
		root.GetNode<FileDialog>("LoadDialog").Popup();
	}
	void CloseSavingDialog()
	{
		root.GetNode<FileDialog>("SaveDialog").Visible=false;
		
	}
	void CloseLoadingDialog()
	{
		root.GetNode<FileDialog>("SaveDialog").Visible=false;
	}
	void CallExitConfirmation()
	{
		
		GetNode<ConfirmationDialog>("EventPanel/ExitConfirmation").Popup();
		
	}
	void CloseExitConfirmation()
	{
		GetNode<ConfirmationDialog>("EventPanel/ExitConfirmation").Visible=false;
	}
	static bool changed=false;
	void ConfirmSavingBeforeExiting()
	{
		CallSavingDialog();
		
	}
	public static void _Clear()
	{
		//ProjectData.Clear();
		
		foreach (var e in RecycledEvents)
		{
			e.Entity.QueueFree();
		}
		RecycledEvents.Clear();
		foreach (var e in RecycledElements)
		{
			e.entity.QueueFree();
			e.Node.QueueFree();
		}
		
		//SelectedEvents.Clear();
		//SelectedElements.Clear();
		
		if (EventStream.Count>0)
		{
			EventStream.All(e=>{e.Entity.QueueFree();return true;});
		}
		//EventStream.Clear();
		if (Elements.Count>0)
		{
			Elements.All(e=>{e.entity.QueueFree();return true;});
		}
		//Elements.Clear();
		if (!cinema.is_test_playing)
		{
			path_cache=string.Empty;
		}
		
		SelectedElements=new HashSet<Element>();
		SelectedEvents=new HashSet<Event>();
		EventStream=new HashSet<Event>();
		GlobalEventStream=new HashSet<Event>();
		Elements=new HashSet<Element>();
		CustomTheme.Themes=new Hashtable();
		ProjectData=new Hashtable();
	
	}
	static void Exit()
	{
		_Clear();
		
		root.GetTree().ChangeSceneToFile("res://Scenes/main.tscn");
		
	}
	static void debug(string str)
	{
		GD.Print("[Debug]: "+str);
	}
	public static Hashtable JTokenToHashtable(Newtonsoft.Json.Linq.JToken token)
{
    Hashtable table = new Hashtable();

    foreach (Newtonsoft.Json.Linq.JProperty property in token.Children<Newtonsoft.Json.Linq.JProperty>())
    {
        table[property.Name] = property.Value.Type == Newtonsoft.Json.Linq.JTokenType.Object ? JTokenToHashtable(property.Value) : property.Value.ToString();
    }

    return table;
}
public static HashSet<Event> DeserializeEvents(string json)
{
	HashSet<Event> Stream=new HashSet<Event>();

	foreach (var token in (JArray)JsonConvert.DeserializeObject(json))
		{

			var _e=JTokenToHashtable(token);
			var type=int.Parse((string)_e["type"]);
			Type T=EventTypes[type];
			var _Event=Activator.CreateInstance(T);
			//debug("1-1. "+T);
			//debug("1-2. "+T.GetMethod("FromDict"));
			GlobalEventStream.Add((Event)_Event);
			T.GetMethod("FromDict").Invoke(_Event,new object[]{_e});
			if (cinema.is_in_cinema&&_Event is ElementEvent)
			{
				var target_euid=long.Parse((string)_e["element"]);
				
				var ie=cinema.Elements.Where(e=>{GD.PrintErr("Found "+e.euid+" for euid"+target_euid);return e.euid==target_euid;});
				if (ie.Count()>0)
				{
					((ElementEvent)_Event).element=ie.First();
					GD.PrintErr("Loaded Element: "+target_euid+" from "+((ElementEvent)_Event).name);
				}
			}
			((Event)_Event).uid=assign_uid();
			
			if (!cinema.is_test_playing){
			((Event)_Event).Entity = EventPanel.event_block.Instantiate<Node2D>();
			GD.Print(((Event)_Event).Entity);
			if (type ==7||type==8)
			{
				// add marker 
				var mark = _Mark.Instantiate<Node2D>();
				((Event)_Event).Entity.GetNode("Marks").AddChild(mark);
			}
			((Event)_Event).Entity.GetNode<Marker2D>("uid").Position = Vector2.Right * ((Event)_Event).uid;
			((Event)_Event).button = ((Event)_Event).Entity.GetNode<Button>("EventBlock");
			//EventPanel.EventStream.AddChild(((Event)_Event).Entity);
			}
			Stream.Add((Event)_Event);
			
		}
		return Stream;
}
    void FileConfirmed(string path)
	{
		Encoding custom=new CustomEncoder();
		GetNode<Button>("EventPanel/Load").Text="Loading...";
		_Clear();
		auto_set("event_stream");auto_set("elements");
		//var path=LoadDialog.CurrentPath;
		debug("0. "+path);
		//var project=FileAccess.Open(path,FileAccess.ModeFlags.Read);
		byte[] bytes=Decompress(System.IO.File.ReadAllBytes(path));
		var data=Encoding.Unicode.GetString(bytes);
		//debug(data);
	if (data!=null)
		{
			
		
			//try{
				var json=JsonConvert.DeserializeObject<Hashtable>(data);
				//debug("0-1. "+json);
				ProjectData["id_assigning_index"]=json["id_assigning_index"];
				ProjectData["euid_assigning_index"]=json["euid_assigning_index"];
				if (json.ContainsKey("tuid_assigning_index"))
				{
					ProjectData["tuid_assigning_index"]=json["tuid_assigning_index"];
				}else{
					ProjectData["tuid_assigning_index"]=0;
				}
				
				ProjectData["event_stream"]=json["event_stream"];
				ProjectData["elements"]=json["elements"];
				if (json.ContainsKey("themes")&&json["themes"]!=null)
				{
					ProjectData["themes"]=(JObject)json["themes"];
				}else
				{
					ProjectData["themes"]=new JObject();
				}
				
				//debug("themes:"+ProjectData["themes"]);
			//}catch(Exception){}
			
		}	
		
		debug("1. deserialized");
		id_assigning_index=(long)ProjectData["id_assigning_index"];
		euid_assigning_index=(long)ProjectData["euid_assigning_index"];
		CustomTheme.tuid_assigner=(long)ProjectData["tuid_assigning_index"];
		foreach (var token in (JArray)ProjectData["elements"])
		{
			/*foreach (KeyValuePair<string,object> prop in _e)
			{
				if (prop.Value is HashSet<Hashtable>)
				{
					
				}
			}*/
			var _e=JTokenToHashtable(token);
			var element=new Element();
			debug("2. JTokenToHashtable");
			element.FromDict(_e);
			debug("3. FromDict");
			element.entity=element_block.Instantiate<Button>();
			Elements.Add(element);
			element.entity.GetNode<Marker2D>("euid").Position=Vector2.Right*element.euid;
			Grid.AddChild(element.entity);
			debug("4. Grid.AddChild");
		}
		foreach (var token in (JArray)ProjectData["event_stream"])
		{
			var _e=JTokenToHashtable(token);
			var type=int.Parse((string)_e["type"]);
			Type T=EventTypes[type];
			var _Event=Activator.CreateInstance(T);
			//debug("1-1. "+T);
			//debug("1-2. "+T.GetMethod("FromDict"));
			GlobalEventStream.Add((Event)_Event);
			T.GetMethod("FromDict").Invoke(_Event,new object[]{_e});

			((Event)_Event).Entity = EventPanel.event_block.Instantiate<Node2D>();

			if (type ==7||type==8)
			{
				// add marker 
				var mark = _Mark.Instantiate<Node2D>();
				((Event)_Event).Entity.GetNode("Marks").AddChild(mark);
			}
			((Event)_Event).Entity.GetNode<Marker2D>("uid").Position = Vector2.Right * ((Event)_Event).uid;
			((Event)_Event).button = ((Event)_Event).Entity.GetNode<Button>("EventBlock");
			EventPanel.EventStream.AddChild(((Event)_Event).Entity);
			EventStream.Add((Event)_Event);
			
		}
		//debug(((JObject)ProjectData["themes"]).Count.ToString());

		foreach (KeyValuePair<string,JToken> kvp in (JObject)ProjectData["themes"])
		{
			var _t=(string)kvp.Value;
			var ImageRect=((PackedScene)GD.BytesToVarWithObjects(custom.GetBytes(_t))).Instantiate<Control>();
			CustomTheme.Themes[long.Parse(kvp.Key)]=ImageRect;
			//var patch_rect = ImageRect.GetNode<NinePatchRect>("Texture");
			
		}
		//debug(CustomTheme.Themes.Count.ToString());
	//	debug("2. "+EventStream.Count);
		

		//project.Close();
		path_cache=path;
		//popup.Visible=false;
		UpdateBin();
		EventPanel.frame=0;
		EventPanel.UpdateStream();
		Update();
		GetNode<Button>("EventPanel/Load").Text="Load";
		ProjectData.Clear();
		EventPanel.frame=0;
		EventPanel.iframe=0;
		EventPanel.UpdateStream();
	}
	public static void focus()
	{
		focused=true;
	}
	
	void release_all(Array<Node> nodes)
	{
		foreach (var n in CollectionsMarshal.AsSpan(nodes.ToList()))
		{
			if (n.GetType()==typeof(Control))
			{
				((Control)n).ReleaseFocus();
			}
			var c=n.GetChildren();
			if (c.Count>0)
			{
				release_all(c);
			}
		}
	}
	public void lose_focus_event(InputEvent @event)
	{
		if (@event is InputEventMouseButton)
		{
			lose_focus();
		}
	}
	public void lose_focus()
	{
		focused=false;
		release_all(GetChildren());
		
	}
	public void debug()
	{
		foreach (var item in GlobalEventStream)
		{
			GD.Print(item.uid);
		}
	}
	public static long assign_uid(){
	
	int i=0;
	Hashtable A=new Hashtable();
	foreach (var item in GlobalEventStream)
	{
		A[item.uid.ToString()]=item.uid;
	}
	while (true)
	{
		if (A.ContainsKey(i.ToString()))
		{
			i+=1;
		}else
		{
			break;
		}
	}
	 return i;}
	 public static long assign_euid(){long id = 0;
int i=0;
	Hashtable A=new Hashtable();
	foreach (var item in Elements)
	{
		A[item.euid.ToString()]=true;
	}
	while (true)
	{
		if (A.ContainsKey(i.ToString()))
		{
			i+=1;
		}else
		{
			id=i;
			break;
		}
	}
	 return id;}
	 public static bool pressing_ctrl=false;
	 public static bool pressing_alt=false;

public static bool rect_selecting=false;
	public static void select_element(long euid)
	{
		if (!pressing_ctrl)
		{
			SelectedElements.Clear();
		}
		Element _e=null;
		if (Elements.Count()>0)
		{
			 _e=Elements.Where(e=>{return e.euid==euid;}).Select(e=>e).First();
		}
		if (_e!=null)
		{
			SelectedElements.Add(_e);
		}
		ElementExplorer.Update();
	}
	public static Event _select_event(long uid)
	{	
	
		foreach (var e in GlobalEventStream)
		{
			if (e.uid==uid)
			{
				return e;
			}
		}
		return null;
	}
   public static void select_event(long uid)//=====================PROP EXPLORER
   {
		

		if (!(pressing_ctrl||rect_selecting))
		{
			SelectedEvents.Clear();
		}
		var _e=_select_event(uid);
		SelectedEvents.Add(_e);
		ControlWindow.sel_uid=uid;
		EventPanel.UpdateStream();
		prop_list.GetChildren().All(n=>{n.QueueFree(); return true;});
		prop_list_uid.Position=Vector2.Right*uid;
			Type t = _e.GetType(); PropertyInfo[] properties = t.GetProperties();
			GD.Print("type:",t.ToString());
			
			foreach (var prop in properties)
			{
				if (
					prop.Name=="uid"||
					prop.Name=="button"||
					prop.Name=="Entity"||
					prop.Name=="frame_stamp"||
					prop.Name=="posX"||
					prop.Name=="type"||
					prop.Name=="ContentColor"||
					(_e is GUIEvent &&(!(prop.Name=="ClearGeneralUnits"||prop.Name=="ClearPlotUnits"||prop.Name=="LineColor")))
					
					
					)
				{
					//do nothing
				}else
				{
					var prop_t = prop.PropertyType;
					Type[] Types = { typeof(string), typeof(float), typeof(Color), typeof(Element), typeof(Where),typeof(ArchiveEventMode), typeof(bool), typeof(int) };
					Control[] units = { str_prop, num_prop, color_prop, elem_prop,belongs_2_prop, archive_event_mode_prop, bool_prop, int_prop };
					var i = 0;
					foreach (var type in Types)
					{
						var _type=Types[i];
						if (prop_t == _type)
						{
							var _u = (Control)units[i].Duplicate();
							_u.GetNode<Label>("PropName").Text = prop.Name;
							prop_list.AddChild(_u);
							_u.Visible=true;
							if (type==typeof(Element))
							{

								_u.GetNode<CheckBox>("TransformOnly").ButtonPressed=WriteTransformOnly;
							}
							
						}
						i++;
					}
					
				}
			}
			if(_e is ElementEvent||_e is GUIEvent||_e is PhysicsEvent||_e is CameraShakeEvent)
			{

				var _u=(Control)trans_prop.Duplicate();
				if (_e is ElementEvent)
				{
					_u.GetNode<Control>("Write").Visible=false;
					var ccp=(Control)content_color_prop.Duplicate();
					prop_list.AddChild(ccp);
					ccp.Visible=true;
				}
				_u.GetNode<Label>("PropName").Text = "Transformation";
				
				prop_list.AddChild(_u);
				_u.Visible=true;
				if (_e is PhysicsElementEvent)
				{
						var _p=(Control)physicsbody_prop.Duplicate();
					_p.GetNode<Label>("PropName").Text = "Physics Body Properties";
					prop_list.AddChild(_p);
					_p.Visible=true;
				}
				var tc=(Control)tween_ctrl.Duplicate();
				prop_list.AddChild(tc);
				tc.Visible=true;
			}	
			if (_e is ArchiveEvent)
			{
				var _u=(Control)archive_event_mode_prop.Duplicate();
				_u.GetNode<Label>("PropName").Text = "ArchivingMode";
				prop_list.AddChild(_u);
				_u.Visible=true;
			}
			if (_e is GeneralTextEvent||_e is PlotEvent)
			{
				var _u=(Button)theme_override.Duplicate();
				prop_list.AddChild(_u);
				_u.Visible=true;
			}
		if (_e is EventGroup)
			{
				var _u=(Label)double_click_tip.Duplicate();
				prop_list.AddChild(_u);
				_u.Visible=true;
			}
   }
  static void ClearBinEvents()
   {
	foreach (var e in RecycledEvents)
	{
		e.Entity.QueueFree();
	}
	RecycledEvents.Clear();
	UpdateBin();
   }
static void ClearBinElements()
{
foreach (var e in RecycledElements)
	{
		e.entity.QueueFree();
		e.Node.QueueFree();
	}
	RecycledElements.Clear();
	UpdateBin();
}
public static Node event_bin_list;public static Node element_bin_list;static Node bin_unit;
public static void UpdateBin()
{
	if (event_bin_list!=null)
	{
			event_bin_list.GetChildren().All(e=>{e.QueueFree();return true;});
	}
	if (element_bin_list!=null)
	{
	element_bin_list.GetChildren().All(e=>{e.QueueFree();return true;});
	}
	foreach (var _e in RecycledEvents)
	{
		var unit=bin_unit.Duplicate();
		unit.GetNode<Label>("Name").Text="["+_e.GetType().Name+"]\n"+_e.name;
		unit.GetNode<Marker2D>("id").Position=Vector2.Right*_e.uid;
		event_bin_list.AddChild(unit);
		((Control)unit).Visible=true;
	}
	foreach (var _e in RecycledElements)
	{
		var unit=bin_unit.Duplicate();
		var typename="";
			switch (_e.type)
		{
			
			default:
			case ElementType.Image:typename="🖼Image";break;
			case ElementType.Animation:typename="🎞Animation";break;
			case ElementType.Video:typename="📽Video";break;
			case ElementType.Text:typename="📃Text";break;
			case ElementType.Custom:typename="Custom";break;
			case ElementType.Progress:typename="ProgressBar";break;
		}
		unit.GetNode<Label>("Name").Text="["+typename+"]\n"+_e.Name;
		unit.GetNode<Marker2D>("id").Position=Vector2.Right*_e.euid;
		element_bin_list.AddChild(unit);((Control)unit).Visible=true;
	}
}
public static void Restore(long id,bool is_event)
{
	if (is_event)
	{
		if (RecycledEvents.Count<=0)
		{
			return;
		}
		var e=RecycledEvents.Where(e=>e.uid==id).Select(e=>e).First();
		RecycledEvents.RemoveWhere(e=>e.uid==id);
		EventStream.Add(e);
		e.frame_stamp=EventPanel.frame;
		e.Entity.Visible=true;
		EventPanel.UpdateStream();
	}
	else
	{if (RecycledElements.Count<=0)
		{
			return;
		}
var e=RecycledElements.Where(e=>e.euid==id).Select(e=>e).First();
		RecycledElements.RemoveWhere(e=>e.euid==id);
		Elements.Add(e);
		e.entity.Visible=true;
		Update();
	}
	UpdateBin();
}
public static void DeleteBinItem(long id,bool is_event)
{
if (is_event)
	{if (RecycledEvents.Count<=0)
		{
			return;
		}
var e=RecycledEvents.Where(e=>e.uid==id).Select(e=>e).First();
e.Entity.QueueFree();
RecycledEvents.RemoveWhere(e=>e.uid==id);
	}
	else
	{if (RecycledElements.Count<=0)
		{
			return;
		}
var e=RecycledElements.Where(e=>e.euid==id).Select(e=>e).First();
		e.entity.QueueFree();
		e.Node.QueueFree();
		RecycledElements.RemoveWhere(e=>e.euid==id);
	}
UpdateBin();
}
}
