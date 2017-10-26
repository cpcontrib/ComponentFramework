using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentLibraryFramework.CodeGen
{
	public class OverrideCodeGenerator
	{

		public OverrideCodeGenerator()
		{
		}


		public string GenerateCodeString(GeneratorInfo componentInfo)
		{
			StringWriter sw = new StringWriter();
			IndentedTextWriter writer = new IndentedTextWriter(sw, "\t");


			writer.WriteLine(@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrownPeak.CMSAPI;
using CrownPeak.CMSAPI.Services;");

			writer.WriteLine("using {0};", componentInfo.Namespace);

			writer.WriteLine("/* Some Namespaces are not allowed. */");

			writer.WriteLine("namespace {0}", componentInfo.Namespace);
			writer.WriteLine("{");

			writer.Indent++;

			//writer.Indent();
			writer.WriteLine("public partial class {0} : CrownPeak.CMSAPI.ComponentBase", componentInfo.ClassName);
			writer.WriteLine("{");

			writer.Indent++;

			this.WriteOverrideMethod(writer, "Input");
			writer.WriteLine();
			this.WriteOverrideMethod(writer, "PostInput");
			writer.WriteLine();
			this.WriteOverrideMethod(writer, "Output");
			writer.WriteLine();

			writer.Indent--;
			writer.WriteLine("}");//finish class

			writer.Indent--;
			writer.WriteLine("}");//finish namespace

			return sw.ToString();
		}

		public void WriteOverrideMethod(IndentedTextWriter writer, string phase)
		{
			writer.WriteLine("public override void Component{0}(Asset asset, {0}Context context, string label, string name)", phase);
			writer.WriteLine("{");

			writer.Indent++;

			writer.WriteLine("//The below function refers the automatically generated input functionality for this component.");
			writer.WriteLine("//If you wish to alter the the input functionality write your own input code here.");
			writer.WriteLine("//NOTE: It is important to handle cases where the asset AND OR the context will be null in your custom code");
			writer.WriteLine("{0}Base(asset, context, label, name);", phase);

			writer.Indent--;
			writer.WriteLine("}");
		}




	}
}
