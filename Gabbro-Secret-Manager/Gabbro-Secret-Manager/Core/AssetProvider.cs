using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Core
{
    public class AssetProvider(IWebHostEnvironment env, IOptions<AssetsSettings> options)
    {
        public bool TryGetAsset(string path, out byte[] content)
        {
            var pathsToCheck = new List<string>
            {
                Path.Combine(env.ContentRootPath, "wwwroot", path),
            };

            if (!string.IsNullOrEmpty(options.Value.BasePath))
                pathsToCheck.Add(Path.Combine(options.Value.BasePath, path));


            foreach (var pathToCheck in pathsToCheck)
            {
                if (System.IO.File.Exists(pathToCheck))
                {
                    content = System.IO.File.ReadAllBytes(pathToCheck);
                    return true;
                }
            }

            content = [];
            return false;
        }
    }
}
