using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentLibraryFramework.CodeGen
{
	[TestFixture]
	public class CodeGenerator_Tests
	{

		private GeneratorInfo CreateComponentInfo()
		{
			var info = new GeneratorInfo();

			info.ClassName = "ReferenceClass";
			info.Namespace = "TestNamespace";
			info.ComponentMarkup = @"{image}{text}{link}";

			info.Fields.Add(new FieldInfo() { Label = "image", TypeRef = "Image", Identifier = "image" });
			info.Fields.Add(new FieldInfo() { Label = "text", TypeRef = "Text", Identifier = "text" });
			info.Fields.Add(new FieldInfo() { Label = "link", TypeRef = "LinkAndTarget", Identifier = "link" });

			return info;
		}
		private DesignerCodeGenerator CreateDesignerCodeGenerator()
		{
			return new DesignerCodeGenerator();
		}
		private OverrideCodeGenerator CreateOverrideCodeGenerator()
		{
			return new OverrideCodeGenerator();
		}

		[Test]
		public void GenerateBase()
		{
			//arrange
			GeneratorInfo info = CreateComponentInfo();

			//act
			OverrideCodeGenerator gen = CreateOverrideCodeGenerator();

			string basecode = gen.GenerateCodeString(info);

			//assert
			Assert.Pass("passing for now.  todo code analysis to compare important parts against crownpeak base");
		}

		[Test]
		public void GenerateDesignerCode()
		{
			//arrange
			GeneratorInfo info = CreateComponentInfo();

			//act
			DesignerCodeGenerator gen = CreateDesignerCodeGenerator();

			string basecode = gen.GenerateCodeString(info);

			//assert
			Assert.Pass("passing for now.  todo code analysis to compare important parts against crownpeak base");
		}
	}
}
