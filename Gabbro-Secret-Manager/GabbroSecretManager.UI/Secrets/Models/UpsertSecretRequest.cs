using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GabbroSecretManager.UI.Secrets.Models
{
    public class UpsertSecretRequest
    {
        [BindProperty(Name = "key"), Required(AllowEmptyStrings = false)]
        public required string Key { get; set; }

        [BindProperty(Name = "value")]
        public string? Value { get; set; }

        [BindProperty(Name = "comments")]
        public string? Comments { get; set; }

        [BindProperty(Name = "tags")]
        public List<string> Tags { get; set; } = [];

        [BindProperty(Name = "is-create")]
        public required bool IsCreate { get; set; }
    }
}
