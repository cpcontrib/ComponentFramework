using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace CrownPeak.CMSAPI
{
	/// <summary>
	/// This is the base class from which all component classes inhereit. Every component must inheret from this class in order for the component
	/// to work as expected within the component library. 
	/// </summary>
	[DataContract]
	[Serializable]
	public abstract class ComponentBase : MarshalByRefObject
	{
		/// <summary>
		/// This is the raw markup for the component.
		/// </summary>
		public string ComponentMarkup
		{
			get;
			set;
		}

		/// <summary>
		/// Default Constructor for the ComponentBase class.
		/// </summary>
		public ComponentBase()
		{
		}

		/// <summary>
		/// The input function which is meant to be overridden by the child class
		/// </summary>
		/// <param name="asset">The current asset</param>
		/// <param name="context">The current context</param>
		/// <param name="label">The label for the current field or component</param>
		/// <param name="name">The content field name for the current field or component</param>
		public abstract void ComponentInput(Asset asset, InputContext context, string label, string name);

		/// <summary>
		/// The output function which is meant to be overridden by the child class
		/// </summary>
		/// <param name="asset">The current asset</param>
		/// <param name="context">The current OutputContext context</param>
		/// <param name="name">The content field name for the current field or component</param>
		/// <param name="index">If this is in a list panel then the current panel index</param>
		/// <param name="isDrag">Indicates if the output needs to be rendered for drag and drop or for publishing and previewing</param>
		/// <returns>The markup for the current component with all the content placeholders replaced with their specified content</returns>
		public abstract string ComponentOutput(Asset asset, OutputContext context, string name, string index = "", bool isDrag = false);

		/// <summary>
		/// The postInput function which is meant to be overridden by the child class
		/// </summary>
		/// <param name="asset">The current asset</param>
		/// <param name="context">The current PostInputContext context</param>
		/// <param name="name">The content field name for the current field or component</param>
		/// <param name="index">If this is in a list panel then the current panel index</param>
		public abstract void ComponentPostInput(Asset asset, PostInputContext context, string name, string index = "");

		/// <summary>
		/// A helper function to return the current integer as a string with ":" prepended to it
		/// </summary>
		/// <param name="i">the current panel index</param>
		/// <returns>a string of the current panel index with ":" prepended to it</returns>
		public static string Index(int i)
		{
			return string.Concat(":", i.ToString());
		}

		/// <summary>
		/// A helper function which takes the provided index string and return the number of the shallowest index.
		/// </summary>
		/// <param name="i">The string representation of the index</param>
		/// <returns>an integer representation of the shallowest index within the provided string.</returns>
		public static int Index(string i)
		{
			Match m = (new Regex("^(:)*[0-9]+")).Match(i);
			Out.DebugWriteLine(string.Concat("i: ", i));
			Out.DebugWriteLine(string.Concat("index match value: ", m.Value));
			if(m == null || string.IsNullOrWhiteSpace(m.Value))
			{
				return -1;
			}
			return Convert.ToInt32(m.Value.Substring(1));
		}

		/// <summary>
		/// A helper function used to generate the output when list panels are used. 
		/// </summary>
		/// <param name="asset">The current asset</param>
		/// <param name="context">The current context</param>
		/// <param name="label">The label for the current field or component</param>
		/// <param name="name">The content field name for the current field or component</param>
		/// <param name="fields">A Dictionary where each key value pair consist of a list of string parameters and the corresponding content field's input function</param>
		/// <param name="minPanels">The minimum number of panels for this input panel</param>
		/// <param name="maxPanels">The maximum number of panels for this input panel</param>
		public void PanelInput(Asset asset, InputContext context, string label, string name, Dictionary<List<string>, Action<Asset, InputContext, string, string>> fields, int? minPanels = null, int? maxPanels = null)
		{
			string str;
			while(Input.NextPanel(name, ListPanelType.Regular, minPanels, maxPanels, null, label, true))
			{
				foreach(KeyValuePair<List<string>, Action<Asset, InputContext, string, string>> entry in fields)
				{
					string fieldName = entry.Key[0];
					string fieldLabel = (entry.Key.Count > 1 ? entry.Key[1] : string.Empty);
					str = (entry.Key.Count > 2 ? entry.Key[2] : string.Empty);
					Action<Asset, InputContext, string, string> method = entry.Value;
					string messageContent = (entry.Key.Count > 3 ? entry.Key[3] : string.Empty);
					string messageType = (entry.Key.Count > 4 ? entry.Key[4] : string.Empty);
					Out.DebugWriteLine(string.Concat("messageType: ", messageType));
					if(str != "control_group")
					{
						if(!string.IsNullOrWhiteSpace(messageContent))
						{
							Input.ShowMessage(messageContent, (messageType == "basic" ? MessageType.Basic : MessageType.Warning));
						}
						method(asset, context, fieldLabel, string.Concat(name, fieldName));
					}
					else
					{
						Input.StartControlPanel(fieldLabel);
						if(!string.IsNullOrWhiteSpace(messageContent))
						{
							Input.ShowMessage(messageContent, (messageType == "basic" ? MessageType.Basic : MessageType.Warning));
						}
						method(asset, context, "", string.Concat(name, fieldName));
						Input.EndControlPanel();
					}
				}
			}
		}

		/// <summary>
		/// The default helper function used to iterate through each panel during the output process when list panels are used. 
		/// </summary>
		/// <param name="asset">The current asset</param>
		/// <param name="context">The current OutputContext context</param>
		/// <param name="name">The current content field name for the current content field or component</param>
		/// <param name="markup">The markup that should be repeated within the panel</param>
		/// <param name="listName">the cp-list name attribute value</param>
		/// <param name="fields">A Dictionary where each key value pair consist of a list of string parameters and the corresponding content field's output function</param>
		/// <param name="index">If this is in a list panel then the current panel index</param>
		/// <returns>The markup for the current list panel with all the content placeholders replaced with their specified content for each panel</returns>
		public string PanelOutput(Asset asset, OutputContext context, string name, string markup, string listName, Dictionary<List<string>, Func<Asset, OutputContext, string, string, bool, string>> fields, string index = "")
		{
			Out.DebugWriteLine("Entered panel_output with name: {0}, markup: {1}, listName: {2}", new object[] { name, Util.HtmlEncode(markup), listName });
			string output = "";
			int i = 1;
			Out.DebugWriteLine(string.Concat(new string[] { "checking asset[", name, ComponentBase.Index(i), index, "]..." }));
			if(!string.IsNullOrEmpty(asset[string.Concat(name, ComponentBase.Index(i), index)]))
			{
				while(!string.IsNullOrEmpty(asset[string.Concat(name, ComponentBase.Index(i), index)]))
				{
					string pattern = markup;
					Out.DebugWriteLine(string.Concat(new string[] { "asset[", name, ComponentBase.Index(i), index, "] = ", asset[string.Concat(name, ComponentBase.Index(i), index)] }));
					foreach(KeyValuePair<List<string>, Func<Asset, OutputContext, string, string, bool, string>> entry in fields)
					{
						List<string> fieldParams = entry.Key;
						Func<Asset, OutputContext, string, string, bool, string> method = entry.Value;
						pattern = pattern.Replace(fieldParams[0], method(asset, context, string.Concat(name, fieldParams[1]), string.Concat(ComponentBase.Index(i), index), false));
					}
					i++;
					output = string.Concat(output, pattern);
				}
			}
			else if(!string.IsNullOrEmpty(asset[string.Concat(name, index)]))
			{
				string pattern = markup;
				Out.DebugWriteLine(string.Concat(new string[] { "asset[", name, index, "] = ", asset[string.Concat(name, index)] }));
				foreach(KeyValuePair<List<string>, Func<Asset, OutputContext, string, string, bool, string>> entry in fields)
				{
					List<string> fieldParams = entry.Key;
					Func<Asset, OutputContext, string, string, bool, string> method = entry.Value;
					pattern = pattern.Replace(fieldParams[0], method(asset, context, string.Concat(name, fieldParams[1]), index, false));
				}
				i++;
				output = string.Concat(output, pattern);
			}
			else if(!string.IsNullOrEmpty(asset[string.Concat(name, ComponentBase.Index(i), ":1")]))
			{
				while(!string.IsNullOrEmpty(asset[string.Concat(name, ComponentBase.Index(i), ":1")]))
				{
					string pattern = markup;
					Out.DebugWriteLine(string.Concat(new string[] { "asset[", name, ComponentBase.Index(i), ":1] = ", asset[string.Concat(name, ComponentBase.Index(i), ":1")] }));
					foreach(KeyValuePair<List<string>, Func<Asset, OutputContext, string, string, bool, string>> entry in fields)
					{
						List<string> fieldParams = entry.Key;
						Func<Asset, OutputContext, string, string, bool, string> method = entry.Value;
						pattern = pattern.Replace(fieldParams[0], method(asset, context, string.Concat(name, fieldParams[1]), string.Concat(ComponentBase.Index(i), ":1"), false));
					}
					i++;
					output = string.Concat(output, pattern);
				}
			}
			return output;
		}

		/// <summary>
		/// A different helper function used to iterate through a list of assets to provide content for each panel. Use this option to generate the output if your panels correspond 
		/// to a list of external assets rather then content within the current asset
		/// </summary>
		/// <param name="asset">The current asset</param>
		/// <param name="context">The current OutputContext context</param>
		/// <param name="markup">The markup that should be repeated within the panel</param>
		/// <param name="fields">A Dictionary where each key value pair consist of a list of string parameters and the corresponding content field's output function. Note that in this case it will only take the first kvp within the dictionary</param>
		/// <param name="index">If this is in a list panel then the current panel index</param>
		/// <param name="overrideAssetList">The list of assets from which the content will be pulled from instead of the current asset</param>
		/// <returns>The markup for the current list panel with all the content placeholders replaced with their specified content for each panel</returns>
		public string PanelOutput(Asset asset, OutputContext context, string markup, Dictionary<List<string>, Func<Asset, OutputContext, string, string, bool, string>> fields, string index = "", List<Asset> overrideAssetList = null)
		{
			string contentToReplace = fields.First<KeyValuePair<List<string>, Func<Asset, OutputContext, string, string, bool, string>>>().Key[0];
			Func<Asset, OutputContext, string, string, bool, string> method = fields.First<KeyValuePair<List<string>, Func<Asset, OutputContext, string, string, bool, string>>>().Value;
			return this.PanelOutput(asset, context, markup, contentToReplace, method, "", overrideAssetList);
		}

		/// <summary>
		/// A different helper function used to iterate through a list of assets to provide content for each panel. Use this option to generate the output if your panels correspond 
		/// to a list of external assets rather then content within the current asset
		/// </summary>
		/// <param name="asset">The current asset</param>
		/// <param name="context">The current OutputContext context</param>
		/// <param name="markup">The markup that should be repeated within the panel</param>
		/// <param name="contentToReplace">The markup within the &lt;cp-list&gt; tags that should be updated with the content from the external assets</param>
		/// <param name="method">The method of the output function that should be used to replace the content placeholders within the &lt;cp-list&gt; tags</param>
		/// <param name="index">If this is in a list panel then the current panel index</param>
		/// <param name="overrideAssetList">The list of assets from which the content will be pulled from instead of the current asset</param>
		/// <returns>The markup for the current list panel with all the content placeholders replaced with their specified content for each panel</returns>
		public string PanelOutput(Asset asset, OutputContext context, string markup, string contentToReplace, Func<Asset, OutputContext, string, string, bool, string> method, string index = "", List<Asset> overrideAssetList = null)
		{
			string output = "";
			int i = 1;
			if(overrideAssetList != null)
			{
				foreach(Asset overrideAsset in overrideAssetList)
				{
					string pattern = markup;
					pattern = pattern.Replace(contentToReplace, method(overrideAsset, context, "", ComponentBase.Index(i), false));
					i++;
					output = string.Concat(output, pattern);
				}
			}
			return output;
		}

		/// <summary>
		/// A helper function used to iterate through each panel during the post input process when list panels are used. 
		/// </summary>
		/// <param name="context">The current PostInputContext context</param>
		/// <param name="name">The content field name for the current field or component</param>
		/// <param name="fields">A Dictionary where each key value pair consist of a list of string parameters and the corresponding content field's postInput function</param>
		/// <param name="index">If this is in a list panel then the current panel index</param>
		/// <param name="asset">The current asset</param>
		public void PanelPostInput(Asset asset, PostInputContext context, string name, Dictionary<List<string>, Action<Asset, PostInputContext, string, string>> fields, string index = "")
		{
			int i = 1;
			if(string.IsNullOrEmpty(context.InputForm[string.Concat(name, ComponentBase.Index(i), index)]))
			{
				if(string.IsNullOrEmpty(context.InputForm[string.Concat(name, index)]))
				{
					if(!string.IsNullOrEmpty(context.InputForm[string.Concat(name, ComponentBase.Index(i), ":1")]))
					{
						while(!string.IsNullOrEmpty(context.InputForm[string.Concat(name, ComponentBase.Index(i), ":1")]))
						{
							Out.DebugWriteLine(string.Concat(new string[] { "context.InputForm[", name, ComponentBase.Index(i), ":1] = ", context.InputForm[string.Concat(name, ComponentBase.Index(i), ":1")] }));
							foreach(KeyValuePair<List<string>, Action<Asset, PostInputContext, string, string>> entry in fields)
							{
								List<string> fieldParams = entry.Key;
								entry.Value(asset, context, string.Concat(name, fieldParams[0]), string.Concat(ComponentBase.Index(i), ":1"));
							}
							i++;
						}
					}
					return;
				}
				Out.DebugWriteLine(string.Concat(new string[] { "context.InputForm[", name, index, "] = ", context.InputForm[string.Concat(name, index)] }));
				foreach(KeyValuePair<List<string>, Action<Asset, PostInputContext, string, string>> entry in fields)
				{
					List<string> fieldParams = entry.Key;
					string contentName = (fieldParams.Count > 0 ? fieldParams[0] : "");
					string contentType = (fieldParams.Count > 1 ? fieldParams[1] : "");
					string contentValidationMessage = (fieldParams.Count > 1 ? fieldParams[2] : "");
					Action<Asset, PostInputContext, string, string> method = entry.Value;
					if(contentType == "")
					{
						method(asset, context, string.Concat(name, fieldParams[0]), index);
					}
					else if(contentType == "Href")
					{
						if(context.InputForm[string.Concat(name, contentName, "_link_type", index)] != "Internal")
						{
							if(!(context.InputForm[string.Concat(name, contentName, "_link_type", index)] == "External") || !string.IsNullOrWhiteSpace(context.InputForm[string.Concat(name, contentName, "_link_external", index)]))
							{
								continue;
							}
							context.ValidationErrorFields.Add(string.Concat(name, contentName, "_link_external", index), contentValidationMessage);
						}
						else
						{
							if(!string.IsNullOrWhiteSpace(context.InputForm[string.Concat(name, contentName, "_link_internal", index)]))
							{
								continue;
							}
							context.ValidationErrorFields.Add(string.Concat(name, contentName, "_link_internal", index), contentValidationMessage);
						}
					}
					else if(contentType != "Src")
					{
						if(!string.IsNullOrWhiteSpace(context.InputForm[string.Concat(name, contentName, index)]))
						{
							continue;
						}
						context.ValidationErrorFields.Add(string.Concat(name, contentName, index), contentValidationMessage);
					}
					else
					{
						if(!string.IsNullOrWhiteSpace(context.InputForm[string.Concat(name, contentName, "_image", index)]))
						{
							continue;
						}
						context.ValidationErrorFields.Add(string.Concat(name, contentName, "_image", index), contentValidationMessage);
					}
				}
				i++;
				return;
			}
			while(!string.IsNullOrEmpty(context.InputForm[string.Concat(name, ComponentBase.Index(i), index)]))
			{
				Out.DebugWriteLine(string.Concat(new string[] { "context.InputForm[", name, ComponentBase.Index(i), index, "] = ", context.InputForm[string.Concat(name, ComponentBase.Index(i), index)] }));
				foreach(KeyValuePair<List<string>, Action<Asset, PostInputContext, string, string>> entry in fields)
				{
					List<string> fieldParams = entry.Key;
					string contentName = (fieldParams.Count > 0 ? fieldParams[0] : "");
					string contentType = (fieldParams.Count > 1 ? fieldParams[1] : "");
					string contentValidationMessage = (fieldParams.Count > 1 ? fieldParams[2] : "");
					Action<Asset, PostInputContext, string, string> method = entry.Value;
					if(contentType == "")
					{
						method(asset, context, string.Concat(name, fieldParams[0]), string.Concat(ComponentBase.Index(i), index));
					}
					else if(contentType == "Href")
					{
						if(context.InputForm[string.Concat(new string[] { name, contentName, "_link_type", ComponentBase.Index(i), index })] != "Internal")
						{
							if(context.InputForm[string.Concat(new string[] { name, contentName, "_link_type", ComponentBase.Index(i), index })] != "External")
							{
								continue;
							}
							if(!string.IsNullOrWhiteSpace(context.InputForm[string.Concat(new string[] { name, contentName, "_link_external", ComponentBase.Index(i), index })]))
							{
								continue;
							}
							context.ValidationErrorFields.Add(string.Concat(new string[] { name, contentName, "_link_external", ComponentBase.Index(i), index }), contentValidationMessage);
						}
						else
						{
							if(!string.IsNullOrWhiteSpace(context.InputForm[string.Concat(new string[] { name, contentName, "_link_internal", ComponentBase.Index(i), index })]))
							{
								continue;
							}
							context.ValidationErrorFields.Add(string.Concat(new string[] { name, contentName, "_link_internal", ComponentBase.Index(i), index }), contentValidationMessage);
						}
					}
					else if(contentType != "Src")
					{
						if(!string.IsNullOrWhiteSpace(context.InputForm[string.Concat(name, contentName, ComponentBase.Index(i), index)]))
						{
							continue;
						}
						context.ValidationErrorFields.Add(string.Concat(name, contentName, ComponentBase.Index(i), index), contentValidationMessage);
					}
					else
					{
						if(!string.IsNullOrWhiteSpace(context.InputForm[string.Concat(new string[] { name, contentName, "_image", ComponentBase.Index(i), index })]))
						{
							continue;
						}
						context.ValidationErrorFields.Add(string.Concat(new string[] { name, contentName, "_image", ComponentBase.Index(i), index }), contentValidationMessage);
					}
				}
				i++;
			}
		}
	}
}