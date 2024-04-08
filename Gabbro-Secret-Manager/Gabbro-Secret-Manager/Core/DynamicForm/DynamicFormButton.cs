namespace Gabbro_Secret_Manager.Core.DynamicForm
{
	public class DynamicFormButton
	{
		public DynamicFormButtonType Type { get; set; } = DynamicFormButtonType.Button;
		public string? Text { get; set; }
		public string GetText() => Text ?? Type.ToString();
		public string? HxGet { get; set; }
		public string? HyperTrigger { get; set; }
	}
}
