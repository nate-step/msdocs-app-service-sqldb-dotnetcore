using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Google.Api.Ads.AdManager.v202308;

namespace DotNetCoreSqlDb.Models
{
    public class AdUnit
    {
        public required string Id { get; set; }
        public string? ParentId { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Status { get; set; }
        public bool? IsInterstitial { get; set; }
        public ParentPath[]? ParentPath { get; set; }
        public AdUnitSize[]? Sizes { get; set; }
        public List<AdUnit>? Children { get; set; }
    }
}
