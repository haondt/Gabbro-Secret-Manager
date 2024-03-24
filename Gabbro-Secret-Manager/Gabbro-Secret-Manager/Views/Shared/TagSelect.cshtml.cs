using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Gabbro_Secret_Manager.Views.Shared
{
    public class TagSelectModel
    {
        public Dictionary<string, bool> Options { get; set; } = [];
        public Dictionary<string, bool> DummyOptions { get; set; } = new Dictionary<string, bool>
        {
            {"foo", false},
            {"bar", true},
            {"really long tag name that could cause issues", false},
            {"qux", false},
        };

        public bool HasActiveTags() => DummyOptions.Any(o => o.Value);
    }
}