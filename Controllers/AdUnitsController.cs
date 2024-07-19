using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Data;
using DotNetCoreSqlDb.Models;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Text;
using Google.Api.Ads.AdManager.Lib;
using Google.Api.Ads.AdManager.v202405;
using Google.Api.Ads.AdManager.Util.v202405;
using Google.Api.Ads.Common.Lib;

namespace DotNetCoreSqlDb.Controllers
{
    /// <summary>
    /// API controller for ad units
    /// </summary>
    [ActionTimerFilter]
    [ApiController]
    [Route("api/[controller]")]
    public class AdUnitsController : ControllerBase
    {
        /// <summary>
        /// Ad Manager user to make requests
        /// </summary>
        AdManagerUser user;
        public static StringBuilder sb = new StringBuilder();
        public static string excludeAdUnits = "21808883114, 21808959844, 21809118501, 22452709038, 21842098369, 22569845256, 22894638438, 22911316057, 21838210805, 21877986668, 22119756265";

        /// <summary>
        /// Constructor for AdUnitsController
        /// </summary>
        public AdUnitsController()
        {
            AdManagerAppConfig appConfig = new AdManagerAppConfig();
            appConfig.MaskCredentials = true;
            appConfig.EnableGzipCompression = true;
            appConfig.IncludeUtilitiesInUserAgent = true;
            appConfig.ApplicationName = "GamApi";
            appConfig.NetworkCode = "21809957681";
            appConfig.OAuth2Mode = OAuth2Flow.SERVICE_ACCOUNT;
            appConfig.OAuth2SecretsJsonPath = "gamapi.json";

            user = new AdManagerUser(appConfig);
        }

        /// <summary>
        /// Get all publisher sites.
        /// </summary>
        /// <returns> List of all publisher sites in STEP Network. </returns>
        [HttpGet("publishers")]
        public IActionResult GetChildPublishers()
        {
            using (SiteService siteService = user.GetService<SiteService>())
            {
                try
                {
                    // Create a statement to select sites.
                    int pageSize = StatementBuilder.SUGGESTED_PAGE_LIMIT;
                    StatementBuilder statementBuilder =
                        new StatementBuilder().OrderBy("url ASC").Limit(pageSize);
                    List<SiteMetadata> sites = new List<SiteMetadata>();

                    // Retrieve a small amount of sites at a time, paging through until all
                    // sites have been retrieved.
                    int totalResultSetSize = 0;
                    do
                    {
                        SitePage page =
                            siteService.getSitesByStatement(statementBuilder.ToStatement());

                        // Print out some information for each site.
                        if (page.results != null)
                        {
                            totalResultSetSize = page.totalResultSetSize;
                            int i = page.startIndex;
                            foreach (Site site in page.results)
                            {
                                SiteMetadata newSite = new SiteMetadata
                                {
                                    Id = site.id,
                                    Url = site.url,
                                    Active = site.active.ToString(),
                                    ApprovalStatus = site.approvalStatus.ToString(),
                                    ChildNetworkCode = site.childNetworkCode,
                                    DisapprovalReasons = site.disapprovalReasons
                                };
                                sites.Add(newSite);
                            }
                        }

                        statementBuilder.IncreaseOffsetBy(pageSize);
                    } while (statementBuilder.GetOffset() < totalResultSetSize);

                    return Ok(sites);
                }
                catch (Exception e)
                {
                    return BadRequest($"Failed to get child publishers. Exception says \"{e.Message}\"");
                }
            }
        }

        /// <summary>
        /// Get all ad units for STEP Network (top level ad units). Mainly Publisher Group Ad Units.
        /// </summary>
        [HttpGet]
        public IActionResult GetAdUnits()
        {
            using (InventoryService inventoryService = user.GetService<InventoryService>())
            using (NetworkService networkService = user.GetService<NetworkService>())
            {
                // Get the effective root ad unit ID of the network.
                string effectiveRootAdUnitId =
                    networkService.getCurrentNetwork().effectiveRootAdUnitId;

                // Create a statement to select the children of the effective root ad
                // unit.
                StatementBuilder statementBuilder = new StatementBuilder()
                    .Where("NOT id IN (" + excludeAdUnits + ") AND parentId = :parentId AND status = :status")
                    .OrderBy("name ASC")
                    .Limit(StatementBuilder.SUGGESTED_PAGE_LIMIT)
                    .AddValue("parentId", effectiveRootAdUnitId) // 23194054406 - API Sandbox
                    .AddValue("status", InventoryStatus.ACTIVE.ToString());

                // Set default for page.
                AdUnitPage page = new AdUnitPage();

                try
                {
                    List<AdUnitMetadata> adUnits = new List<AdUnitMetadata>();

                    do
                    {
                        // Get ad units by statement.
                        page = inventoryService.getAdUnitsByStatement(statementBuilder
                            .ToStatement());

                        if (page.results != null)
                        {
                            foreach (Google.Api.Ads.AdManager.v202405.AdUnit adUnit in page.results)
                            {
                                AdUnitMetadata newAdUnit = new AdUnitMetadata
                                {
                                    Id = adUnit.id,
                                    Name = adUnit.name,
                                    Status = adUnit.status.ToString(),
                                    ParentId = adUnit.parentId,
                                    Code = adUnit.adUnitCode,
                                    ParentPath = adUnit.parentPath?.Select(p => new ParentPathMetadata
                                    (
                                        p.id,
                                        p.name,
                                        p.adUnitCode
                                    )).ToArray(),
                                    Sizes = adUnit.adUnitSizes?.Select(s => new AdUnitSizeMetadata(
                                        s.fullDisplayString,
                                        s.environmentType.ToString(),
                                        s.size.width,
                                        s.size.height
                                    )).ToArray(),
                                    LastModifiedDate = new System.DateTime(
                                        adUnit.lastModifiedDateTime.date.year,
                                        adUnit.lastModifiedDateTime.date.month,
                                        adUnit.lastModifiedDateTime.date.day,
                                        adUnit.lastModifiedDateTime.hour,
                                        adUnit.lastModifiedDateTime.minute,
                                        adUnit.lastModifiedDateTime.second
                                    ).AddHours(9),
                                    ApplicationId = adUnit.applicationId,
                                    AppliedLabelFrequencyCaps = adUnit.appliedLabelFrequencyCaps,
                                    AppliedLabels = adUnit.appliedLabels,
                                    AppliedTeamIds = adUnit.appliedTeamIds,
                                    EffectiveAppliedLabels = adUnit.effectiveAppliedLabels,
                                    EffectiveTeamIds = adUnit.effectiveTeamIds,
                                    EffectiveLabelFrequencyCaps = adUnit.effectiveLabelFrequencyCaps,
                                    ExplicitlyTargeted = adUnit.explicitlyTargeted,
                                    IsFluid = adUnit.isFluid,
                                    IsInterstitial = adUnit.isInterstitial,
                                    IsNative = adUnit.isNative,
                                    RefreshRate = adUnit.refreshRate,
                                    SmartSizeMode = adUnit.smartSizeMode,
                                    TargetWindow = adUnit.targetWindow
                                };

                                adUnits.Add(newAdUnit);
                            }
                        }

                        statementBuilder.IncreaseOffsetBy(StatementBuilder
                            .SUGGESTED_PAGE_LIMIT);
                    } while (statementBuilder.GetOffset() < page.totalResultSetSize);

                    return Ok(adUnits);
                }
                catch (Exception e)
                {
                    return BadRequest($"Failed to get ad units. Exception says \"{e.Message}\"");
                }
            }
        }

        /// <summary>
        /// Get all ad units for a given parent ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns> List of ad units </returns>
        [HttpGet("{id}")]
        public IActionResult GetAdUnits(string? id)
        {
            using (InventoryService inventoryService = user.GetService<InventoryService>())
            using (NetworkService networkService = user.GetService<NetworkService>())
            {
                // Get the effective root ad unit ID of the network.
                string effectiveRootAdUnitId =
                    networkService.getCurrentNetwork().effectiveRootAdUnitId;

                // Create a statement to select the children of the effective root ad
                // unit.
                StatementBuilder statementBuilder = new StatementBuilder()
                    .Where("parentId = :parentId AND status = :status")
                    .OrderBy("name ASC")
                    .Limit(StatementBuilder.SUGGESTED_PAGE_LIMIT)
                    .AddValue("parentId", id)    // 23194054406 - API Sandbox
                    .AddValue("status", InventoryStatus.ACTIVE.ToString());

                // Set default for page.
                AdUnitPage page = new AdUnitPage();

                try
                {
                    List<AdUnitMetadata> adUnits = new List<AdUnitMetadata>();

                    do
                    {
                        // Get ad units by statement.
                        page = inventoryService.getAdUnitsByStatement(statementBuilder
                            .ToStatement());

                        if (page.results != null)
                        {
                            foreach (Google.Api.Ads.AdManager.v202405.AdUnit adUnit in page.results)
                            {
                                System.DateTime lastModifiedDateTime = new System.DateTime(
                                    adUnit.lastModifiedDateTime.date.year,
                                    adUnit.lastModifiedDateTime.date.month,
                                    adUnit.lastModifiedDateTime.date.day,
                                    adUnit.lastModifiedDateTime.hour,
                                    adUnit.lastModifiedDateTime.minute,
                                    adUnit.lastModifiedDateTime.second
                                ).AddHours(9);

                                AdUnitMetadata newAdUnit = new AdUnitMetadata
                                {
                                    Id = adUnit.id,
                                    Name = adUnit.name,
                                    Status = adUnit.status.ToString(),
                                    ParentId = adUnit.parentId,
                                    Code = adUnit.adUnitCode,
                                    ParentPath = adUnit.parentPath?.Select(p => new ParentPathMetadata
                                    (
                                        p.id,
                                        p.name,
                                        p.adUnitCode
                                    )).ToArray(),
                                    Sizes = adUnit.adUnitSizes?.Select(s => new AdUnitSizeMetadata(
                                        s.fullDisplayString,
                                        s.environmentType.ToString(),
                                        s.size.width,
                                        s.size.height
                                    )).ToArray(),
                                    LastModifiedDate = new System.DateTime(
                                        adUnit.lastModifiedDateTime.date.year,
                                        adUnit.lastModifiedDateTime.date.month,
                                        adUnit.lastModifiedDateTime.date.day,
                                        adUnit.lastModifiedDateTime.hour,
                                        adUnit.lastModifiedDateTime.minute,
                                        adUnit.lastModifiedDateTime.second
                                    ).AddHours(9),
                                    ApplicationId = adUnit.applicationId,
                                    AppliedLabelFrequencyCaps = adUnit.appliedLabelFrequencyCaps,
                                    AppliedLabels = adUnit.appliedLabels,
                                    AppliedTeamIds = adUnit.appliedTeamIds,
                                    EffectiveAppliedLabels = adUnit.effectiveAppliedLabels,
                                    EffectiveTeamIds = adUnit.effectiveTeamIds,
                                    EffectiveLabelFrequencyCaps = adUnit.effectiveLabelFrequencyCaps,
                                    ExplicitlyTargeted = adUnit.explicitlyTargeted,
                                    IsFluid = adUnit.isFluid,
                                    IsInterstitial = adUnit.isInterstitial,
                                    IsNative = adUnit.isNative,
                                    RefreshRate = adUnit.refreshRate,
                                    SmartSizeMode = adUnit.smartSizeMode,
                                    TargetWindow = adUnit.targetWindow
                                };

                                adUnits.Add(newAdUnit);
                            }
                        }

                        statementBuilder.IncreaseOffsetBy(StatementBuilder
                            .SUGGESTED_PAGE_LIMIT);
                    } while (statementBuilder.GetOffset() < page.totalResultSetSize);

                    return Ok(adUnits);
                }
                catch (Exception e)
                {
                    return BadRequest($"Failed to get ad units. Exception says \"{e.Message}\"");
                }
            }
        }

        /// <summary>
        /// Get all ad unit sizes.
        /// </summary>
        /// <returns> List of all ad unit sizes </returns>
        [HttpGet("sizes")]
        public IActionResult GetAdUnitSizes()
        {
            using (InventoryService inventoryService = user.GetService<InventoryService>())
            {
                try
                {
                    // Create a statement to select ad unit sizes.
                    StatementBuilder statementBuilder = new StatementBuilder();

                    Google.Api.Ads.AdManager.v202405.AdUnitSize[] adUnitSizes =
                    inventoryService.getAdUnitSizesByStatement(statementBuilder.ToStatement());

                    List<AdUnitSizeMetadata> adUnitSizesList = new List<AdUnitSizeMetadata>();

                    foreach (Google.Api.Ads.AdManager.v202405.AdUnitSize adUnitSize in adUnitSizes)
                    {
                        adUnitSizesList.Add(new AdUnitSizeMetadata(adUnitSize.fullDisplayString, adUnitSize.environmentType.ToString(), adUnitSize.size.width, adUnitSize.size.height));
                    }

                    return Ok(adUnitSizesList);
                }
                catch (Exception e)
                {
                    return BadRequest($"Failed to get ad unit sizes. Exception says \"{e.Message}\"");
                }
            }
        }

        /// <summary>
        /// Get the whole Ad Unit tree for STEP Network.
        /// </summary>
        /// <returns> All Active Ad Units in a tree stack. </returns>
        [HttpGet("tree")]
        public IActionResult AdUnitsTree()
        {
            try
            {
                // Get all ad units.
                Google.Api.Ads.AdManager.v202405.AdUnit[] allAdUnits = GetAllAdUnits(user);

                // Find the root ad unit.
                Google.Api.Ads.AdManager.v202405.AdUnit? rootAdUnit = allAdUnits.FirstOrDefault(a => a.parentId == "21808957681");

                // Build the ad unit tree.
                AdUnitMetadata root = BuildAdUnitTree(rootAdUnit, allAdUnits);

                return Ok(root);
            }
            catch (Exception e)
            {
                return BadRequest($"Failed to get ad unit tree. Exception says \"{e.Message}\"");
            }
        }

        /// <summary>
        /// Build the Ad Unit tree.
        /// </summary>
        /// <param name="rootAdUnit"></param>
        /// <param name="allAdUnits"></param>
        /// <returns> Ad Unit tree </returns>
        private AdUnitMetadata BuildAdUnitTree(Google.Api.Ads.AdManager.v202405.AdUnit? rootAdUnit, Google.Api.Ads.AdManager.v202405.AdUnit[] allAdUnits)
        {
            if (rootAdUnit == null)
            {
                return null;
            }

            AdUnitMetadata root = new AdUnitMetadata
            {
                Id = rootAdUnit.id,
                Name = rootAdUnit.name,
                Status = rootAdUnit.status.ToString(),
                ParentId = rootAdUnit.parentId,
                Code = rootAdUnit.adUnitCode,
                Sizes = rootAdUnit.adUnitSizes?.Select(s => new AdUnitSizeMetadata(s.fullDisplayString, s.environmentType.ToString(), s.size.width, s.size.height)).ToArray(),
                Children = new List<AdUnitMetadata>()
            };

            foreach (Google.Api.Ads.AdManager.v202405.AdUnit adUnit in allAdUnits)
            {
                if (adUnit.parentId == rootAdUnit.id)
                {
                    AdUnitMetadata child = BuildAdUnitTree(adUnit, allAdUnits);
                    root.Children.Add(child);
                }
            }

            return root;
        }

        /// <summary>
        /// Get all ad units for a given parent ID.
        /// </summary>
        /// <param name="user"></param>
        /// <returns> List of ad units </returns>
        static Google.Api.Ads.AdManager.v202405.AdUnit[] GetAllAdUnits(AdManagerUser user)
        {
            using (InventoryService inventoryService = user.GetService<InventoryService>())
            {
                // Create list to hold all ad units.
                List<Google.Api.Ads.AdManager.v202405.AdUnit> adUnits = new List<Google.Api.Ads.AdManager.v202405.AdUnit>();

                // Create a statement to get all ad units.
                StatementBuilder statementBuilder = new StatementBuilder()
                    .Where("status = :status AND NOT id IN (" + excludeAdUnits + ")")
                    .OrderBy("name ASC")
                    .Limit(StatementBuilder.SUGGESTED_PAGE_LIMIT)
                    .AddValue("status", InventoryStatus.ACTIVE.ToString());

                // Set default for page.
                AdUnitPage page = new AdUnitPage();

                do
                {
                    // Get ad units by statement.
                    page = inventoryService.getAdUnitsByStatement(statementBuilder.ToStatement());

                    if (page.results != null)
                    {
                        adUnits.AddRange(page.results);
                    }

                    statementBuilder.IncreaseOffsetBy(StatementBuilder.SUGGESTED_PAGE_LIMIT);
                } while (statementBuilder.GetOffset() < page.totalResultSetSize);

                return adUnits.ToArray();
            }
        }

    }
}