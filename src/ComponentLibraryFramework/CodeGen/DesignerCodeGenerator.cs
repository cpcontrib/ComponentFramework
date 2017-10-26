using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentLibraryFramework.CodeGen
{
	public class DesignerCodeGenerator
	{

		public DesignerCodeGenerator()
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

			this.WriteFieldDeclarations(writer, componentInfo.Fields);
			writer.WriteLine();

			this.WriteConstructor(writer, componentInfo);

			this.WriteInputBaseMethod(writer, componentInfo.Fields);
			writer.WriteLine();

			this.WritePostInputBaseMethod(writer, componentInfo.Fields);
			writer.WriteLine();

			this.WriteOutputBaseMethod(writer, componentInfo.Fields);
			writer.WriteLine();

			writer.Indent--;
			writer.WriteLine("}");//finish class

			writer.Indent--;
			writer.WriteLine("}");//finish namespace

			return sw.ToString();
		}

		public void WriteFieldDeclarations(IndentedTextWriter writer, IEnumerable<FieldInfo> fields)
		{
			foreach(var field in fields)
			{
				string identifier = field.Label.ToLower();
				writer.WriteLine("public {0} {1} {{ get; set; }}", field.TypeRef, field.Identifier);
			}
		}

		public void WriteConstructor(IndentedTextWriter writer, GeneratorInfo info)
		{
			writer.WriteLine("public {0}", info.ClassName);
			writer.WriteLine("{");
			writer.Indent++;

			foreach(var field in info.Fields)
			{
				writer.WriteLine("{0} = new {1};", field.Identifier, field.TypeRef);
			}

			writer.WriteLine(@"ComponentMarkup = @""{0}"";", info.ComponentMarkup.Replace("\"","\"\""));

			writer.Indent--;
			writer.WriteLine("}");
		}


		public void WriteInputBaseMethod(IndentedTextWriter writer, IEnumerable<FieldInfo> fields)
		{
			writer.WriteLine("public void InputBase(Asset asset, InputContext context, string label, string name)");
			writer.WriteLine("{");
			writer.Indent++;

			foreach(var field in fields)
			{
				writer.WriteLine(@"this.{0}.ComponentInput(asset, context, label + "" {1}"", name + ""_{2}"");", field.Identifier, field.Label, field.Identifier);
			}

			writer.Indent--;
			writer.WriteLine("}");
		}


		private void WritePostInputBaseMethod(IndentedTextWriter writer, List<FieldInfo> fields)
		{
			writer.WriteLine("public void PostInputBase(Asset asset, InputContext context, string label, string name)");
			writer.WriteLine("{");
			writer.Indent++;

			foreach(var field in fields)
			{
				writer.WriteLine(@"this.{0}.ComponentPostInput(asset, context, label + "" {1}"", name + ""_{2}"");", field.Identifier, field.Label, field.Identifier);
			}

			writer.Indent--;
			writer.WriteLine("}");
		}

		private void WriteOutputBaseMethod(IndentedTextWriter writer, List<FieldInfo> fields)
		{
			writer.WriteLine(@"public string OutputBase(Asset asset, OutputContext context, string name, string index = """", bool isDrag = false)");
			writer.WriteLine("{");
			writer.Indent++;

			writer.WriteLine(@"if(isDrag && !context.IsPublishing) return ComponentFramework.ParseMarkupForDrag(ComponentMarkup);");
			writer.WriteLine();

			writer.WriteLine(@"StringBuilder sbContent = new StringBuilder();");
			writer.WriteLine(@"sbContent.Append(ComponentMarkup);");

			foreach(var field in fields)
			{
				writer.WriteLine(@"this.{0}.ComponentOutput(asset, context, label + "" {1}"", name + ""_{2}"");", field.Identifier, field.Label, field.Identifier);
			}

			writer.WriteLine(@"string markup = ComponentFramework.UpdateMarkupForPreviewPublish(context, sbContent.ToString());");
			writer.WriteLine(@"return markup;");

			writer.Indent--;
			writer.WriteLine("}");
		}
	}
}
