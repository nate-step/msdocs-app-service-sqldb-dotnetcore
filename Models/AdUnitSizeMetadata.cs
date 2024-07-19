using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreSqlDb.Models
{
    public class AdUnitSizeMetadata
    {
        public string Size { get; set; }
        public string? EnvironmentType { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }

        public AdUnitSizeMetadata(string size, string environmentType, int width, int height)
        {
            Size = size;
            EnvironmentType = environmentType;
            Width = width;
            Height = height;
        }
    }
}
