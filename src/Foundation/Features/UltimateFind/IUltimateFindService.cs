namespace Foundation.Features.UltimateFind;

public interface IUltimateFindService
{
    Task<object> BasicTypedSearchAsync(string q);
    Task<object> FieldSpecificSearchAsync(string q);
    Task<object> FilterBasicsAsync(string continent, string country);
    Task<object> FilterBooleanOperatorsAsync(string c1, string c2);
    Task<object> FilterAdvancedAsync(string prefix, int min, int max);
    Task<object> FilterDateTimeAsync(DateTime? after, DateTime? before);
    Task<object> FilterTypeHierarchyAsync();
    Task<object> FacetsTermsAsync(string q);
    Task<object> FacetsRangeAsync();
    Task<object> FacetsGeoDistanceAsync(double lat, double lon);
    Task<object> BoostingAsync(string q);
    Task<object> SortingAsync();
    Task<object> PaginationAsync(int page, int pageSize, int cacheMinutes);
    Task<object> CmsFiltersAsync(string q);
    Task<object> HighlightingAsync(string q);
    Task<object> MoreLikeThisAsync(string text);
    Task<object> BestBetsAsync(string q);
    Task<object> UnifiedSearchAsync(string q);
    Task<object> StatisticsAndTrackingAsync(string q);
    Task<object> GeoSearchAsync(double lat, double lon, int distKm, int minKm, int maxKm);
    Task<object> WildcardSearchAsync(string pattern);
    Task<object> NestedFilterAsync(string skuPrefix);
    Task<object> MultiSearchAsync(string q);
    Task<object> RunAllAsync();
}
