using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Google.Api.Ads.AdManager.v202308;

namespace DotNetCoreSqlDb.Models
{
    public class SiteMetadata
    {
        public long Id { get; set; }
        public string? Active { get; set; }
        public string? ApprovalStatus { get; set; }
        public string ChildNetworkCode { get; set; }
        public Google.Api.Ads.AdManager.v202405.DisapprovalReason[]? DisapprovalReasons { get; set; }
        public string Url { get; set; }
    }
}
