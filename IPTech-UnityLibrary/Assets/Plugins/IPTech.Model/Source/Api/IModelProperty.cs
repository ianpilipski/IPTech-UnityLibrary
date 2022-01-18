using IPTech.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.Model.Api
{
	public interface IModelProperty
	{
		IModel Owner { get; }
		RName PropertyName { get; }
		object PreviousValue { get; }
		object CurrentValue { get; set; }
		bool HasChanged { get; }

		bool IsArray { get; }
		IModelProperties EnumerableElements { get; }
	}

	public class ModelPropertyIsNotEnumerableException : Exception
	{
		const string ERRORMESSAGE = "The property {0} on object {1} is not an IEnumerable property type.";

		public ModelPropertyIsNotEnumerableException(IModelProperty property) : base(FormatErrorMessage(property)) {
			
		}

		private static string FormatErrorMessage(IModelProperty property) {
			return string.Format(ERRORMESSAGE, property.Owner.TargetType().Name, property.PropertyName.ToString());
		}
	}
}
