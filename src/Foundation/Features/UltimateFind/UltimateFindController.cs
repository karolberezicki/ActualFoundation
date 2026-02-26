namespace Foundation.Features.UltimateFind;

[ApiController]
[Route("api/[controller]")]
public class UltimateFindController : ControllerBase
{
    private readonly IUltimateFindService _service;

    public UltimateFindController(IUltimateFindService service)
    {
        _service = service;
    }

    [HttpGet("basic-search")]
    public async Task<IActionResult> BasicTypedSearch([FromQuery] string q = "europe") =>
        Ok(await _service.BasicTypedSearchAsync(q));

    [HttpGet("field-search")]
    public async Task<IActionResult> FieldSpecificSearch([FromQuery] string q = "stockholm") =>
        Ok(await _service.FieldSpecificSearchAsync(q));

    [HttpGet("filter-basics")]
    public async Task<IActionResult> FilterBasics(
        [FromQuery] string continent = "Europe",
        [FromQuery] string country = "Sweden") =>
        Ok(await _service.FilterBasicsAsync(continent, country));

    [HttpGet("filter-boolean-operators")]
    public async Task<IActionResult> FilterBooleanOperators(
        [FromQuery] string c1 = "Europe",
        [FromQuery] string c2 = "Asia") =>
        Ok(await _service.FilterBooleanOperatorsAsync(c1, c2));

    [HttpGet("filter-advanced")]
    public async Task<IActionResult> FilterAdvanced(
        [FromQuery] string prefix = "sw",
        [FromQuery] int min = 1000000,
        [FromQuery] int max = 50000000) =>
        Ok(await _service.FilterAdvancedAsync(prefix, min, max));

    [HttpGet("filter-datetime")]
    public async Task<IActionResult> FilterDateTime(
        [FromQuery] DateTime? after = null,
        [FromQuery] DateTime? before = null) =>
        Ok(await _service.FilterDateTimeAsync(after, before));

    [HttpGet("filter-type-hierarchy")]
    public async Task<IActionResult> FilterTypeHierarchy() =>
        Ok(await _service.FilterTypeHierarchyAsync());

    [HttpGet("facets-terms")]
    public async Task<IActionResult> FacetsTerms([FromQuery] string q = "") =>
        Ok(await _service.FacetsTermsAsync(q));

    [HttpGet("facets-range")]
    public async Task<IActionResult> FacetsRange() =>
        Ok(await _service.FacetsRangeAsync());

    [HttpGet("facets-geo-distance")]
    public async Task<IActionResult> FacetsGeoDistance(
        [FromQuery] double lat = 59.33,
        [FromQuery] double lon = 18.07) =>
        Ok(await _service.FacetsGeoDistanceAsync(lat, lon));

    [HttpGet("boosting")]
    public async Task<IActionResult> Boosting([FromQuery] string q = "beach") =>
        Ok(await _service.BoostingAsync(q));

    [HttpGet("sorting")]
    public async Task<IActionResult> Sorting() =>
        Ok(await _service.SortingAsync());

    [HttpGet("pagination")]
    public async Task<IActionResult> Pagination(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5,
        [FromQuery] int cacheMinutes = 1) =>
        Ok(await _service.PaginationAsync(page, pageSize, cacheMinutes));

    [HttpGet("cms-filters")]
    public async Task<IActionResult> CmsFilters([FromQuery] string q = "") =>
        Ok(await _service.CmsFiltersAsync(q));

    [HttpGet("highlighting")]
    public async Task<IActionResult> Highlighting([FromQuery] string q = "beach") =>
        Ok(await _service.HighlightingAsync(q));

    [HttpGet("more-like-this")]
    public async Task<IActionResult> MoreLikeThis([FromQuery] string text = "sunny beach resort tropical vacation") =>
        Ok(await _service.MoreLikeThisAsync(text));

    [HttpGet("best-bets")]
    public async Task<IActionResult> BestBets([FromQuery] string q = "travel") =>
        Ok(await _service.BestBetsAsync(q));

    [HttpGet("unified-search")]
    public async Task<IActionResult> UnifiedSearch([FromQuery] string q = "travel") =>
        Ok(await _service.UnifiedSearchAsync(q));

    [HttpGet("statistics")]
    public async Task<IActionResult> StatisticsAndTracking([FromQuery] string q = "stokholm") =>
        Ok(await _service.StatisticsAndTrackingAsync(q));

    [HttpGet("geo-search")]
    public async Task<IActionResult> GeoSearch(
        [FromQuery] double lat = 59.33,
        [FromQuery] double lon = 18.07,
        [FromQuery] int distKm = 2000,
        [FromQuery] int minKm = 500,
        [FromQuery] int maxKm = 3000) =>
        Ok(await _service.GeoSearchAsync(lat, lon, distKm, minKm, maxKm));

    [HttpGet("wildcard")]
    public async Task<IActionResult> WildcardSearch([FromQuery] string pattern = "stock*") =>
        Ok(await _service.WildcardSearchAsync(pattern));

    [HttpGet("nested-filter")]
    public async Task<IActionResult> NestedFilter([FromQuery] string skuPrefix = "SKU") =>
        Ok(await _service.NestedFilterAsync(skuPrefix));

    [HttpGet("multi-search")]
    public async Task<IActionResult> MultiSearch([FromQuery] string q = "foundation") =>
        Ok(await _service.MultiSearchAsync(q));

    [HttpGet("run-all")]
    public async Task<IActionResult> RunAll() =>
        Ok(await _service.RunAllAsync());
}
