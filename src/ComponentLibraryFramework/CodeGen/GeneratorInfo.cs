using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentLibraryFramework.CodeGen
{
	public class GeneratorInfo
	{

		public GeneratorInfo()
		{
			this._fields = new List<FieldInfo>();
		}

		private List<FieldInfo> _fields;
		public List<FieldInfo> Fields
		{
			get { return _fields; }
		}

		public string Namespace { get; set; }
		public string ClassName { get; set; }

		public string ComponentMarkup { get; set; }


	}

	public class FieldInfo
	{
		public string Label { get; set; }
		public string TypeRef { get; set; }
		public string Identifier { get; set; }
	}
}
