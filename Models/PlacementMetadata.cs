using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Google.Api.Ads.AdManager.v202308;

namespace DotNetCoreSqlDb.Models
{
    public class PlacementMetadata
    {
        public long Id { get; set; }
        public string? Status { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Code { get; set; }
        public string[]? AdUnits { get; set; }
    }
}
