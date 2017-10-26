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
	public partial class ReferenceClass : CrownPeak.CMSAPI.ComponentBase
	{
		public override void ComponentInput(Asset asset, InputContext context, string label, string name)
		{
			//The below function refers the automatically generated input functionality for this component.
			//If you wish to alter the the input functionality write your own input code here.
			//NOTE: It is important to handle cases where the asset AND OR the context will be null in your custom code
			InputBase(asset, context, label, name);
		}

		public override void ComponentPostInput(Asset asset, PostInputContext context, string name, string index = "")
		{
			//The below function refers the automatically generated post_input functionality for this component.
			//If you wish to alter the the post_input functionality write your own post_input code here.
			//NOTE: It is important to handle cases where the asset AND OR the context will be null in your custom code
			PostInputBase(asset, context, name, index);
		}

		public override string ComponentOutput(Asset asset, OutputContext context, string name, string index = "", bool isDrag = false)
		{
			//The below function refers the automatically generated output functionality for this component.
			//If you wish to alter the the output functionality write your own output code here.
			//NOTE: It is important to handle cases where the asset AND OR the context will be null in your custom code
			return OutputBase(asset, context, name, index, isDrag);
		}

	}
}