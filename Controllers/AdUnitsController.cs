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
        /// Get all ad units for STEP Network. Mainly Publisher Group Ad Units.
        /// </summary>
        [HttpGet]
        public IActionResult GetAdUnits()
        {
            AdManagerAppConfig appConfig = new AdManagerAppConfig();
            appConfig.MaskCredentials = true;
            appConfig.EnableGzipCompression = true;
            appConfig.IncludeUtilitiesInUserAgent = true;
            appConfig.ApplicationName = "GamApi";
            appConfig.NetworkCode = "21809957681";
            appConfig.OAuth2Mode = OAuth2Flow.SERVICE_ACCOUNT;
            appConfig.OAuth2SecretsJsonPath = "gamapi.json";
            
            AdManagerUser user = new AdManagerUser(appConfig);
            using (InventoryService inventoryService = user.GetService<InventoryService>())
            using (NetworkService networkService = user.GetService<NetworkService>())
            {
                // Get the effective root ad unit ID of the network.
                string effectiveRootAdUnitId =
                    networkService.getCurrentNetwork().effectiveRootAdUnitId;

                // Create a statement to select the children of the effective root ad
                // unit.
                StatementBuilder statementBuilder = new StatementBuilder()
                    .Where("parentId = :parentId").OrderBy("id ASC")
                    .Limit(StatementBuilder.SUGGESTED_PAGE_LIMIT)
                    .AddValue("parentId", 23194054406)
                    .AddValue("status", InventoryStatus.ACTIVE.ToString()); // 23194054406 - API Sandbox

                // Set default for page.
                AdUnitPage page = new AdUnitPage();

                try
                {
                    List<Models.AdUnit> adUnits = new List<Models.AdUnit>();

                    do
                    {
                        // Get ad units by statement.
                        page = inventoryService.getAdUnitsByStatement(statementBuilder
                            .ToStatement());

                        if (page.results != null)
                        {
                            foreach (Google.Api.Ads.AdManager.v202405.AdUnit adUnit in page.results)
                            {
                                Models.AdUnit newAdUnit = new Models.AdUnit
                                {
                                    Id = adUnit.id,
                                    Name = adUnit.name,
                                    Status = adUnit.status.ToString(),
                                    ParentId = adUnit.parentId,
                                    Code = adUnit.adUnitCode,
                                    ParentPath = adUnit.parentPath.Select(p => new ParentPath
                                    {
                                        Id = p.id,
                                        Name = p.name,
                                        Code = p.adUnitCode,
                                        CreatedDate = System.DateTime.Now
                                    }).ToArray(),
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


        // Example run of printing all ad units
        // /// <summary>
        // /// Get all ad units for STEP Network. Mainly Publisher Group Ad Units.
        // /// </summary>
        // public void GetRootAdUnits()
        // {
        //     AdManagerUser user = new AdManagerUser();
        //     using (InventoryService inventoryService = user.GetService<InventoryService>())
        //     using (NetworkService networkService = user.GetService<NetworkService>())
        //     {
        //         // Get the effective root ad unit ID of the network.
        //         string effectiveRootAdUnitId =
        //             networkService.getCurrentNetwork().effectiveRootAdUnitId;

        //         // Create a statement to select the children of the effective root ad
        //         // unit.
        //         StatementBuilder statementBuilder = new StatementBuilder()
        //             .Where("parentId = :parentId").OrderBy("id ASC")
        //             .Limit(StatementBuilder.SUGGESTED_PAGE_LIMIT)
        //             .AddValue("parentId", 23149994656)
        //             .AddValue("status", InventoryStatus.ACTIVE.ToString()); // 23149994656 - 123mc

        //         // Set default for page.
        //         AdUnitPage page = new AdUnitPage();

        //         try
        //         {
        //             do
        //             {
        //                 // Get ad units by statement.
        //                 page = inventoryService.getAdUnitsByStatement(statementBuilder
        //                     .ToStatement());

        //                 if (page.results != null)
        //                 {
        //                     int i = page.startIndex;
        //                     foreach (Google.Api.Ads.AdManager.v202405.AdUnit adUnit in page.results)
        //                     {
        //                         Console.WriteLine(
        //                             "{0}) Ad unit with ID = '{1}', name = '{2}', " +
        //                             "status = '{3}', parent ID = '{4}', code = '{5}' " +
        //                             "parent path name = '{6}', parent path code = '{7}', parent path id = '{8}' was found.", i, adUnit.id, adUnit.name,
        //                             adUnit.status, adUnit.parentId, adUnit.adUnitCode, adUnit.parentPath[0].name, adUnit.parentPath[0].adUnitCode, adUnit.parentPath[0].id);
        //                         foreach (AdUnitParent parent in adUnit.parentPath)
        //                         {
        //                             Console.WriteLine(
        //                                 "Parent ad unit with ID = '{0}', name = '{1}', " +
        //                                 "and code = '{2}' was found.", parent.id, parent.name,
        //                                 parent.adUnitCode);
        //                         }
        //                         i++;
        //                     }
        //                 }

        //                 statementBuilder.IncreaseOffsetBy(StatementBuilder
        //                     .SUGGESTED_PAGE_LIMIT);
        //             } while (statementBuilder.GetOffset() < page.totalResultSetSize);

        //             Console.WriteLine("Number of results found: {0}", page.totalResultSetSize);
        //         }
        //         catch (Exception e)
        //         {
        //             Console.WriteLine("Failed to get ad unit. Exception says \"{0}\"",
        //                 e.Message);
        //         }
        //     }
        // }

        // For connecting with a database:
        //     private readonly ILogger<AdUnitsController> _logger;
        //     private readonly MyDatabaseContext _context;
        //     private readonly IDistributedCache _cache;
        //     private readonly string _AdUnitsCacheKey = "AdUnitsList";

        //     public AdUnitsController(MyDatabaseContext context, IDistributedCache cache, ILogger<AdUnitsController> logger)
        //     {
        //         _context = context;
        //         _cache = cache;
        //         _logger = logger;
        //     }

        //     // GET: api/AdUnits
        //     // The cache logic is added with the help of GitHub Copilot
        //     /// <summary>
        //     /// Get all ad units
        //     /// </summary>
        //     /// <returns> A list of ad units </returns>
        //     [HttpGet]
        //     public async Task<IActionResult> GetAdUnits()
        //     {
        //         var adUnits = await _cache.GetAsync(_AdUnitsCacheKey);
        //         if (adUnits != null)
        //         {
        //             _logger.LogInformation("Data from cache.");
        //             var adUnitList = JsonConvert.DeserializeObject<List<AdUnit>>(Encoding.UTF8.GetString(adUnits));
        //             return Ok(adUnitList);
        //         }
        //         else
        //         {
        //             _logger.LogInformation("Data from database.");
        //             var adUnitList = await _context.AdUnit.ToListAsync();
        //             var serializedAdUnitList = JsonConvert.SerializeObject(adUnitList);
        //             await _cache.SetAsync(_AdUnitsCacheKey, Encoding.UTF8.GetBytes(serializedAdUnitList));
        //             return Ok(adUnitList);
        //         }
        //     }

        //     // GET: api/AdUnits/5
        //     // The cache logic is added with the help of GitHub Copilot
        //     [HttpGet("{id}")]
        //     public async Task<IActionResult> GetAdUnit(int id)
        //     {
        //         var adUnit = await _cache.GetAsync(GetAdUnitCacheKey(id));
        //         if (adUnit != null)
        //         {
        //             _logger.LogInformation("Data from cache.");
        //             var adUnitItem = JsonConvert.DeserializeObject<AdUnit>(Encoding.UTF8.GetString(adUnit));
        //             return Ok(adUnitItem);
        //         }
        //         else
        //         {
        //             _logger.LogInformation("Data from database.");
        //             var adUnitItem = await _context.AdUnit.FindAsync(id);
        //             if (adUnitItem == null)
        //             {
        //                 return NotFound();
        //             }

        //             var serializedAdUnit = JsonConvert.SerializeObject(adUnitItem);
        //             await _cache.SetAsync(GetAdUnitCacheKey(id), Encoding.UTF8.GetBytes(serializedAdUnit));
        //             return Ok(adUnitItem);
        //         }
        //     }

        //     // POST: api/AdUnits
        //     // The cache logic is added with the help of GitHub Copilot
        //     [HttpPost]
        //     public async Task<IActionResult> CreateAdUnit([FromBody] AdUnit adUnit)
        //     {
        //         if (ModelState.IsValid)
        //         {
        //             _context.Add(adUnit);
        //             await _context.SaveChangesAsync();

        //             // Clear the ad units cache
        //             await _cache.RemoveAsync(_AdUnitsCacheKey);

        //             return CreatedAtAction(nameof(GetAdUnit), new { id = adUnit.ID }, adUnit);
        //         }
        //         return BadRequest(ModelState);
        //     }

        //     // PUT: api/AdUnits/5
        //     // The cache logic is added with the help of GitHub Copilot
        //     [HttpPut("{id}")]
        //     public async Task<IActionResult> UpdateAdUnit(int id, [FromBody] AdUnit adUnit)
        //     {
        //         if (id != adUnit.ID)
        //         {
        //             return BadRequest();
        //         }

        //         if (ModelState.IsValid)
        //         {
        //             try
        //             {
        //                 _context.Update(adUnit);
        //                 await _context.SaveChangesAsync();

        //                 // Clear the ad unit and ad units list from the cache
        //                 await _cache.RemoveAsync(GetAdUnitCacheKey(id));
        //                 await _cache.RemoveAsync(_AdUnitsCacheKey);
        //             }
        //             catch (DbUpdateConcurrencyException)
        //             {
        //                 if (!AdUnitExists(adUnit.ID))
        //                 {
        //                     return NotFound();
        //                 }
        //                 else
        //                 {
        //                     throw;
        //                 }
        //             }
        //             return NoContent();
        //         }
        //         return BadRequest(ModelState);
        //     }

        //     // DELETE: api/AdUnits/5
        //     // The cache logic is added with the help of GitHub Copilot
        //     [HttpDelete("{id}")]
        //     public async Task<IActionResult> DeleteAdUnit(int id)
        //     {
        //         var adUnit = await _context.AdUnit.FindAsync(id);
        //         if (adUnit != null)
        //         {
        //             _context.AdUnit.Remove(adUnit);
        //         }

        //         await _context.SaveChangesAsync();

        //         // Clear the ad unit and ad units list from the cache
        //         await _cache.RemoveAsync(GetAdUnitCacheKey(id));
        //         await _cache.RemoveAsync(_AdUnitsCacheKey);

        //         return NoContent();
        //     }

        //     private bool AdUnitExists(int id)
        //     {
        //         return _context.AdUnit.Any(e => e.ID == id);
        //     }

        //     private string GetAdUnitCacheKey(int id)
        //     {
        //         return $"{_AdUnitsCacheKey}_{id}";
        //     }
        // }
    }
}