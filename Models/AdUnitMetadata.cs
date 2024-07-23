using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Google.Api.Ads.AdManager.v202308;

namespace DotNetCoreSqlDb.Models
{
    public class AdUnitMetadata
    {
        public required string Id { get; set; }
        public string? ParentId { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Status { get; set; }
        public long? ApplicationId { get; set; }
        public Google.Api.Ads.AdManager.v202405.LabelFrequencyCap[]? AppliedLabelFrequencyCaps { get; set; }
        public Google.Api.Ads.AdManager.v202405.AppliedLabel[]? AppliedLabels { get; set; }
        public long[]? AppliedTeamIds { get; set; }
        public Google.Api.Ads.AdManager.v202405.AppliedLabel[]? EffectiveAppliedLabels { get; set; }
        public long[]? EffectiveTeamIds { get; set; }
        public Google.Api.Ads.AdManager.v202405.LabelFrequencyCap[]? EffectiveLabelFrequencyCaps { get; set; }
        public bool? ExplicitlyTargeted { get; set; }
        public bool? IsFluid { get; set; }
        public bool? IsInterstitial { get; set; }
        public bool? IsNative { get; set; }
        public int? RefreshRate { get; set; }
        public Google.Api.Ads.AdManager.v202405.SmartSizeMode? SmartSizeMode { get; set; }
        public Google.Api.Ads.AdManager.v202405.AdUnitTargetWindow? TargetWindow { get; set; }
        public ParentPathMetadata[]? ParentPath { get; set; }
        public AdUnitSizeMetadata[]? Sizes { get; set; }
        public List<AdUnitMetadata>? Children { get; set; }
        public SiteMetadata? Site { get; set; }
        public string? Path 
        { 
            get 
            { 
                if (Site != null && ParentPath != null)
                    return $"/21809957681,{Site?.ChildNetworkCode}/{ParentPath?.Last()?.Code}/{Name}"; 
                else return null;
            } 
            set { } 
        }
        [DisplayName("Last Modified Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public System.DateTime LastModifiedDate { get; set; }
    }
}
