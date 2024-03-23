using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Core
{
    public class StylesProvider(
        IOptions<ColorSettings> colorOptions,
        IWebHostEnvironment webHostEnvironment)
    {
        private readonly ColorSettings _colorSettings = colorOptions.Value;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

        public string GetStyles()
        {
            var colorsCss = ":root {\n";
            colorsCss += string.Join('\n', new List<string>
            {
                $"    --color-dark-bg: {_colorSettings.DarkBackground};",
                $"    --color-bright-bg: {_colorSettings.BrightBackground};",
                $"    --color-dark-fg: {_colorSettings.DarkForeground};",
                $"    --color-bright-fg: {_colorSettings.BrightForeground};",
                $"    --color-accent: {_colorSettings.Accent};",
                $"    --color-negative: {_colorSettings.Negative};",
                $"    --color-positive: {_colorSettings.Positive};",
            });
            colorsCss += "\n}\n";

            var baseCss = LoadFile("base.css");
            var customCss = LoadFile("style.css");

            return string.Join('\n', new List<string>
            {
                colorsCss,
                baseCss,
                customCss
            });
        }

        private string LoadFile(string fileName)
        {
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot", fileName);
            return System.IO.File.ReadAllText(filePath);
        }
    }
}
