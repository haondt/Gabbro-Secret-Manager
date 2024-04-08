namespace Gabbro_Secret_Manager.Core.DynamicForm
{
	public class DynamicFormInput : DynamicFormItem
	{ 
		public string? Label { get; set; }
		public string? Error { get; set; }
		public string? PlaceHolder { get; set; }
		public string? Autocomplete { get; set; }
		public string? Value { get; set; }
		public required string Name { get; set; }
		public required DynamicFormInputType Type { get; set; }
	}
}
