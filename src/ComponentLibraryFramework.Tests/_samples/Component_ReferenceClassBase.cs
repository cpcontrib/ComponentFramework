using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrownPeak.CMSAPI;
using CrownPeak.CMSAPI.Services;
using TestNamespace;
/* Some Namespaces are not allowed. */
namespace TestNamespace
{
	[Serializable]
	public partial class ReferenceClass : CrownPeak.CMSAPI.ComponentBase
	{
		public Image image { get; set; }
		public Text text { get; set; }
		public LinkAndTarget link { get; set; }

		public ReferenceClass()
		{
			image = new Image();
			text = new Text();
			link = new LinkAndTarget();
			ComponentMarkup = @"{image}{text}{link}";
		}

		public void InputBase(Asset asset, InputContext context, string label, string name)
		{
			this.image.ComponentInput(asset, context, label + " Image", name + "_image");
			this.text.ComponentInput(asset, context, label + " Text", name + "_text");
			this.link.ComponentInput(asset, context, label + " Link", name + "_link");
		}

		public void PostInputBase(Asset asset, PostInputContext context, string name, string index = "")
		{
			if(String.IsNullOrWhiteSpace(context.InputForm[name + "_image" + index])) { context.ValidationErrorFields.Add(name + "_image" + index, ""); }
		}

		public string OutputBase(Asset asset, OutputContext context, string name, string index = "", bool isDrag = false)
		{
			if(isDrag && !context.IsPublishing) return ComponentFramework.ParseMarkupForDrag(ComponentMarkup);

			StringBuilder sbContent = new StringBuilder();
			sbContent.Append(ComponentMarkup);
			sbContent.Replace(@"{image}", this.image.ComponentOutput(asset, context, name + "_image", index));
			sbContent.Replace(@"{text}", this.text.ComponentOutput(asset, context, name + "_text", index));
			sbContent.Replace(@"{link}", this.link.ComponentOutput(asset, context, name + "_link", index));
			string markup = ComponentFramework.UpdateMarkupForPreviewPublish(context, sbContent.ToString());
			return markup;
		}

	}
}