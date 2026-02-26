using EPiServer.Find;
using EPiServer.Find.Api.Facets;
using EPiServer.Find.Cms;
using EPiServer.Find.Framework.Statistics;
using EPiServer.Find.Statistics;
using EPiServer.Find.UnifiedSearch;
using EPiServer.Globalization;
using Foundation.Features.Blog.BlogItemPage;
using Foundation.Features.CatalogContent.Product;
using Foundation.Features.Events.CalendarEvent;
using Foundation.Features.Locations.LocationItemPage;
using Foundation.Features.People.PersonItemPage;
using Foundation.Infrastructure.Find;
using System.Diagnostics.CodeAnalysis;

namespace Foundation.Features.UltimateFind;

[SuppressMessage("ReSharper", "RedundantAnonymousTypePropertyName")]
[SuppressMessage("ReSharper", "ArrangeObjectCreationWhenTypeNotEvident")]
public class UltimateFindService : IUltimateFindService
{
    private readonly IClient _client;

    public UltimateFindService(IClient client)
    {
        _client = client;
    }

    public async Task<object> BasicTypedSearchAsync(string q)
    {
        var result = await _client.Search<LocationItemPage>()
            .For(q)
            .Take(10)
            .GetContentResultAsync();

        return new
        {
            Feature = "Basic typed search: Search<T>(), .For(), GetContentResultAsync(), TotalMatching",
            TotalMatching = result.TotalMatching,
            Hits = result.Select(p => new
            {
                p.Name,
                p.Continent,
                p.Country,
            }),
        };
    }

    public async Task<object> FieldSpecificSearchAsync(string q)
    {
        var multiFieldResult = await _client.Search<LocationItemPage>()
            .For(q)
            .InField(x => x.Country)
            .InField(x => x.Continent)
            .InField(x => x.AirportInitials)
            .Take(5)
            .GetContentResultAsync();

        var singleFieldResult = await _client.Search<LocationItemPage>()
            .For(q)
            .InField(x => x.Country)
            .Take(5)
            .GetContentResultAsync();

        return new
        {
            Feature = "Field-specific search: .InField() with multiple fields vs single field",
            MultiFieldHits = multiFieldResult.Select(p => new
            {
                p.Name,
                p.Country,
                p.Continent,
                p.AirportInitials,
            }),
            SingleFieldHits = singleFieldResult.Select(p => new
            {
                p.Name,
                p.Country,
                p.Continent,
            }),
        };
    }

    public async Task<object> FilterBasicsAsync(string continent, string country)
    {
        var result = await _client.Search<LocationItemPage>()
            .Filter(x => x.Continent.MatchCaseInsensitive(continent))
            .Filter(x => x.Country.Match(country))
            .Filter(x => x.New.Match(false))
            .Take(20)
            .GetContentResultAsync();

        return new
        {
            Feature = "Filter basics: .Match(), .MatchCaseInsensitive(), multiple .Filter() = AND, bool .Match()",
            TotalMatching = result.TotalMatching,
            Locations = result.Select(p => new
            {
                p.Name,
                p.Continent,
                p.Country,
                p.New,
            }),
        };
    }

    public async Task<object> FilterBooleanOperatorsAsync(string c1, string c2)
    {
        var inlineResult = await _client.Search<LocationItemPage>()
            .Filter(x => x.Continent.Match(c1) | x.Continent.Match(c2))
            .Filter(x => x.Promoted.Match(true) | x.New.Match(true))
            .OrderBy(x => x.PageName)
            .Take(20)
            .GetContentResultAsync();

        var dynamicFilter = _client.BuildFilter<LocationItemPage>();
        dynamicFilter = dynamicFilter.Or(x => x.Continent.Match(c1));
        dynamicFilter = dynamicFilter.Or(x => x.Continent.Match(c2));

        var dynamicResult = await _client.Search<LocationItemPage>()
            .Filter(dynamicFilter)
            .Take(20)
            .GetContentResultAsync();

        return new
        {
            Feature = "Boolean operators: | OR, & AND, grouping, BuildFilter<T>().Or()",
            InlineOrResult = new
            {
                TotalMatching = inlineResult.TotalMatching,
                Locations = inlineResult.Select(p => new { p.Name, p.Continent, p.Promoted, p.New }),
            },
            DynamicFilterResult = new
            {
                TotalMatching = dynamicResult.TotalMatching,
                Locations = dynamicResult.Select(p => new { p.Name, p.Continent }),
            },
        };
    }

    public async Task<object> FilterAdvancedAsync(string prefix, int min, int max)
    {
        var result = await _client.Search<LocationItemPage>()
            .Filter(x => x.Country.PrefixCaseInsensitive(prefix))
            .Filter(x => x.YearlyPassengers.InRange(min, max))
            .Filter(x => x.AirportInitials.Exists())
            .Take(20)
            .GetContentResultAsync();

        return new
        {
            Feature = "Advanced filters: .PrefixCaseInsensitive(), .InRange(), .Exists()",
            TotalMatching = result.TotalMatching,
            Locations = result.Select(p => new
            {
                p.Name,
                p.Country,
                p.YearlyPassengers,
                p.AirportInitials,
            }),
        };
    }

    public async Task<object> FilterDateTimeAsync(DateTime? after, DateTime? before)
    {
        var afterDate = after ?? DateTime.UtcNow;
        var beforeDate = before ?? DateTime.UtcNow.AddYears(1);

        var result = await _client.Search<CalendarEventPage>()
            .Filter(x => x.EventStartDate.GreaterThan(afterDate))
            .Filter(x => x.EventEndDate.LessThan(beforeDate))
            .OrderBy(x => x.EventStartDate)
            .Take(20)
            .GetContentResultAsync();

        return new
        {
            Feature = "DateTime filters: .GreaterThan(), .LessThan() on CalendarEventPage dates",
            AfterDate = afterDate,
            BeforeDate = beforeDate,
            TotalMatching = result.TotalMatching,
            Events = result.Select(e => new
            {
                e.Name,
                e.EventStartDate,
                e.EventEndDate,
                e.Location,
            }),
        };
    }

    public async Task<object> FilterTypeHierarchyAsync()
    {
        var filterResult = await _client.Search<LocationItemPage>()
            .Filter(x => x.MatchTypeHierarchy(typeof(LocationItemPage)))
            .TermsFacetFor(x => x.Continent)
            .Select(x => new { x.Name, x.Continent })
            .Take(5)
            .GetResultAsync();

        var filterHitsResult = await _client.Search<FoundationPageData>()
            .TermsFacetFor(x => x.MetaTitle)
            .FilterHits(x => x.MatchTypeHierarchy(typeof(BlogItemPage)))
            .Select(x => new { x.Name, x.MetaTitle })
            .Take(5)
            .GetResultAsync();

        return new
        {
            Feature = "Type hierarchy: .MatchTypeHierarchy(), .FilterHits() vs .Filter() — facet scope difference",
            FilterExample = new
            {
                Description = ".Filter() narrows both hits AND facets to LocationItemPage",
                TotalMatching = filterResult.TotalMatching,
                ContinentFacets = filterResult.TermsFacetFor(x => x.Continent).Terms
                    .Select(t => new { t.Term, t.Count }),
            },
            FilterHitsExample = new
            {
                Description = ".FilterHits() narrows hits to BlogItemPage, but facets still count all FoundationPageData",
                TotalMatching = filterHitsResult.TotalMatching,
                MetaTitleFacets = filterHitsResult.TermsFacetFor(x => x.MetaTitle).Terms
                    .Take(10)
                    .Select(t => new { t.Term, t.Count }),
            },
        };
    }

    public async Task<object> FacetsTermsAsync(string q)
    {
        var search = _client.Search<LocationItemPage>();

        if (!string.IsNullOrEmpty(q))
        {
            search = search.For(q);
        }

        var result = await search
            .TermsFacetFor(x => x.Continent)
            .TermsFacetFor(x => x.Country)
            .FilterFacet("PromotedLocations", x => x.Promoted.Match(true))
            .FilterFacet("NewLocations", x => x.New.Match(true))
            .Select(x => new { x.Name, x.Continent, x.Country })
            .Take(5)
            .GetResultAsync();

        return new
        {
            Feature = "Facets: .TermsFacetFor(), .FilterFacet(), reading facet results",
            TotalMatching = result.TotalMatching,
            ContinentFacet = result.TermsFacetFor(x => x.Continent).Terms
                .Select(t => new { t.Term, t.Count }),
            CountryFacet = result.TermsFacetFor(x => x.Country).Terms
                .Take(15)
                .Select(t => new { t.Term, t.Count }),
            PromotedCount = result.FilterFacet("PromotedLocations").Count,
            NewCount = result.FilterFacet("NewLocations").Count,
        };
    }

    public async Task<object> FacetsRangeAsync()
    {
        var result = await _client.Search<LocationItemPage>()
            .RangeFacetFor(x => x.YearlyPassengers,
                new NumericRange(0, 1000000),
                new NumericRange(1000000, 10000000),
                new NumericRange(10000000, 50000000),
                new NumericRange(50000000, 100000000))
            .RangeFacetFor(x => x.AvgTemp,
                new NumericRange(-10, 0),
                new NumericRange(0, 15),
                new NumericRange(15, 25),
                new NumericRange(25, 40))
            .Select(x => new { x.Name, x.YearlyPassengers, x.AvgTemp })
            .Take(0)
            .GetResultAsync();

        return new
        {
            Feature = "Range facets: .RangeFacetFor() with NumericRange buckets",
            PassengerRanges = result.RangeFacetFor(x => x.YearlyPassengers)
                .Ranges.Select(r => new { r.From, r.To, r.TotalCount }),
            TemperatureRanges = result.RangeFacetFor(x => x.AvgTemp)
                .Ranges.Select(r => new { r.From, r.To, r.TotalCount }),
        };
    }

    public async Task<object> FacetsGeoDistanceAsync(double lat, double lon)
    {
        var origin = new GeoLocation(lat, lon);

        var result = await _client.Search<LocationItemPage>()
            .GeoDistanceFacetFor(x => x.Coordinates, origin,
                new NumericRange { From = 0, To = 500 },
                new NumericRange { From = 500, To = 1000 },
                new NumericRange { From = 1000, To = 2500 },
                new NumericRange { From = 2500, To = 5000 },
                new NumericRange { From = 5000, To = 10000 },
                new NumericRange { From = 10000, To = 25000 })
            .Select(x => new { x.Name, x.Coordinates })
            .Take(0)
            .GetResultAsync();

        return new
        {
            Feature = "Geo distance facets: .GeoDistanceFacetFor() with km ranges",
            Origin = new { Latitude = lat, Longitude = lon },
            DistanceRanges = result.GeoDistanceFacetFor(x => x.Coordinates)
                .Ranges.Select(r => new
                {
                    FromKm = r.From,
                    ToKm = r.To,
                    Count = r.TotalCount,
                }),
        };
    }

    public async Task<object> BoostingAsync(string q)
    {
        var result = await _client.Search<LocationItemPage>()
            .For(q)
            .BoostMatching(x => x.Promoted.Match(true), 3)
            .BoostMatching(x => x.Continent.Match("Europe"), 1.5)
            .BoostMatching(x => x.New.Match(true) & x.Continent.Match("Asia"), 2)
            .BoostMatching(x => x.Coordinates
                .WithinDistanceFrom(new GeoLocation(59.33, 18.07), 2000.Kilometers()), 2.5)
            .UsingAutoBoost(TimeSpan.FromDays(30))
            .Take(10)
            .GetContentResultAsync();

        return new
        {
            Feature = "Boosting: .BoostMatching() single/multi/complex, geo boost, .UsingAutoBoost()",
            Note = "Boosted items appear first — ordering demonstrates boost effect",
            Hits = result.Select(p => new
            {
                p.Name,
                p.Continent,
                p.Country,
                p.Promoted,
                p.New,
            }),
        };
    }

    public async Task<object> SortingAsync()
    {
        var alphabeticResult = await _client.Search<LocationItemPage>()
            .OrderBy(x => x.Country)
            .ThenByDescending(x => x.YearlyPassengers)
            .Take(10)
            .GetContentResultAsync();

        var geoResult = await _client.Search<LocationItemPage>()
            .Filter(x => x.Continent.Match("Europe"))
            .OrderBy(x => x.Coordinates)
            .DistanceFrom(new GeoLocation(59.33, 18.07))
            .Take(10)
            .GetContentResultAsync();

        return new
        {
            Feature = "Sorting: .OrderBy(), .OrderByDescending(), .ThenByDescending(), .DistanceFrom() geo sort",
            AlphabeticSort = alphabeticResult.Select(p => new
            {
                p.Name,
                p.Country,
                p.YearlyPassengers,
            }),
            GeoSort = geoResult.Select(p => new
            {
                p.Name,
                p.Country,
                p.Latitude,
                p.Longitude,
            }),
        };
    }

    public async Task<object> PaginationAsync(int page, int pageSize, int cacheMinutes)
    {
        var skip = (page - 1) * pageSize;

        var result = await _client.Search<LocationItemPage>()
            .OrderBy(x => x.PageName)
            .Skip(skip)
            .Take(pageSize)
            .StaticallyCacheFor(TimeSpan.FromMinutes(cacheMinutes))
            .GetContentResultAsync();

        var totalPages = (int)Math.Ceiling(result.TotalMatching / (double)pageSize);

        return new
        {
            Feature = "Pagination & caching: .Skip(), .Take(), .StaticallyCacheFor()",
            Page = page,
            PageSize = pageSize,
            TotalMatching = result.TotalMatching,
            TotalPages = totalPages,
            CacheMinutes = cacheMinutes,
            Locations = result.Select(p => new { p.Name, p.Country }),
        };
    }

    public async Task<object> CmsFiltersAsync(string q)
    {
        var search = _client.Search<LocationItemPage>();

        if (!string.IsNullOrEmpty(q))
        {
            search = search.For(q);
        }

        var result = await search
            .PublishedInCurrentLanguage()
            .FilterForVisitor()
            .FilterOnReadAccess()
            .ExcludeDeleted()
            .OrderBy(x => x.PageName)
            .Take(10)
            .GetContentResultAsync();

        return new
        {
            Feature = "CMS filters: .PublishedInCurrentLanguage(), .FilterForVisitor(), .FilterOnReadAccess(), .ExcludeDeleted()",
            TotalMatching = result.TotalMatching,
            Locations = result.Select(p => new
            {
                p.Name,
                p.Continent,
                p.Country,
                Language = p.Language?.Name,
            }),
        };
    }

    public async Task<object> HighlightingAsync(string q)
    {
        var projectedResult = await _client.Search<LocationItemPage>()
            .For(q)
            .Select(x => new
            {
                x.Name,
                x.Continent,
                x.Country,
                x.AvgTemp,
                x.YearlyPassengers,
            })
            .Take(5)
            .GetResultAsync();

        var hitSpec = new HitSpecification
        {
            HighlightTitle = true,
            HighlightExcerpt = true,
            ExcerptLength = 200,
        };

        var unifiedResult = await _client.UnifiedSearchFor(q)
            .Filter(x => x.MatchTypeHierarchy(typeof(LocationItemPage)))
            .Take(5)
            .GetResultAsync(hitSpec);

        return new
        {
            Feature = "Projections & highlighting: .Select() projection, HitSpecification (HighlightTitle, HighlightExcerpt, ExcerptLength)",
            ProjectedResults = projectedResult.Hits.Select(h => new
            {
                h.Document,
                h.Score,
            }),
            HighlightedResults = unifiedResult.Hits.Select(h => new
            {
                h.Document.Title,
                h.Document.Excerpt,
                h.Document.Url,
                h.Score,
            }),
        };
    }

    public async Task<object> MoreLikeThisAsync(string text)
    {
        var result = await _client.Search<LocationItemPage>()
            .MoreLike(text)
            .MinimumDocumentFrequency(1)
            .MaximumQueryTerms(25)
            .BoostMatching(x => x.Continent.Match("Europe"), 1.5)
            .BoostMatching(x => x.Coordinates
                .WithinDistanceFrom(new GeoLocation(40.0, 25.0), 1000.Kilometers()), 2.5)
            .PublishedInCurrentLanguage()
            .FilterForVisitor()
            .Take(10)
            .GetContentResultAsync();

        return new
        {
            Feature = "More Like This: .MoreLike(), .MinimumDocumentFrequency(), .MaximumQueryTerms(), combined with geo boost",
            InputText = text,
            TotalMatching = result.TotalMatching,
            Hits = result.Select(p => new
            {
                p.Name,
                p.Continent,
                p.Country,
            }),
        };
    }

    public async Task<object> BestBetsAsync(string q)
    {
        var result = await _client.Search<FoundationPageData>()
            .For(q)
            .ApplyBestBets()
            .PublishedInCurrentLanguage()
            .FilterForVisitor()
            .Take(10)
            .GetContentResultAsync();

        return new
        {
            Feature = "Best Bets: .ApplyBestBets() — editorial pins on typed search",
            Query = q,
            TotalMatching = result.TotalMatching,
            Pages = result.Select(p => new
            {
                p.Name,
                Type = p.GetOriginalType().Name,
            }),
        };
    }

    public async Task<object> UnifiedSearchAsync(string q)
    {
        var hitSpec = new HitSpecification
        {
            HighlightTitle = true,
            HighlightExcerpt = true,
        };

        var result = await _client.UnifiedSearchFor(q,
                _client.Settings.Languages.GetSupportedLanguage(ContentLanguage.PreferredCulture)
                ?? Language.None)
            .UsingSynonyms()
            .TermsFacetFor(x => x.SearchSection)
            .FilterFacet("AllSections", x => x.SearchSection.Exists())
            .ApplyBestBets()
            .Skip(0)
            .Take(10)
            .GetResultAsync(hitSpec);

        return new
        {
            Feature = "Unified search: .UnifiedSearchFor(), .UsingSynonyms(), HitSpecification, section facets, .ApplyBestBets()",
            TotalMatching = result.TotalMatching,
            SectionFacets = result.TermsFacetFor(x => x.SearchSection).Terms
                .Select(t => new { t.Term, t.Count }),
            AllSectionsCount = result.FilterFacet("AllSections").Count,
            Hits = result.Hits.Select(h => new
            {
                h.Document.Title,
                h.Document.Url,
                h.Document.Excerpt,
                h.Score,
            }),
        };
    }

    public async Task<object> StatisticsAndTrackingAsync(string q)
    {
        var result = await _client.Search<LocationItemPage>()
            .For(q)
            .Track()
            .Take(5)
            .GetContentResultAsync();

        object didYouMean = null;
        if (result.TotalMatching == 0)
        {
            var didYouMeanResult = await _client.Statistics().GetDidYouMeanAsync(q);
            didYouMean = didYouMeanResult;
        }

        return new
        {
            Feature = "Statistics & tracking: .Track(), Statistics().GetDidYouMeanAsync()",
            Query = q,
            TotalMatching = result.TotalMatching,
            DidYouMean = didYouMean,
            Hits = result.Select(p => new
            {
                p.Name,
                p.Country,
            }),
        };
    }

    public async Task<object> GeoSearchAsync(double lat, double lon, int distKm, int minKm, int maxKm)
    {
        var origin = new GeoLocation(lat, lon);

        var withinResult = await _client.Search<LocationItemPage>()
            .Filter(x => x.Coordinates.WithinDistanceFrom(origin, distKm.Kilometers()))
            .OrderBy(x => x.Coordinates)
            .DistanceFrom(origin)
            .Take(20)
            .GetContentResultAsync();

        var rangeResult = await _client.Search<LocationItemPage>()
            .Filter(x => x.Coordinates.WithinDistanceFrom(origin, minKm.Kilometers(), maxKm.Kilometers()))
            .OrderBy(x => x.Coordinates)
            .DistanceFrom(origin)
            .GeoDistanceFacetFor(x => x.Coordinates, origin,
                new NumericRange { From = 0, To = 1000 },
                new NumericRange { From = 1000, To = 2500 },
                new NumericRange { From = 2500, To = 5000 })
            .Select(x => new { x.Name, x.Country, x.Coordinates })
            .Take(20)
            .GetResultAsync();

        return new
        {
            Feature = "Geo search: .WithinDistanceFrom() single + range, .DistanceFrom() sort, .GeoDistanceFacetFor()",
            Origin = new { Latitude = lat, Longitude = lon },
            WithinDistance = new
            {
                RadiusKm = distKm,
                TotalMatching = withinResult.TotalMatching,
                Locations = withinResult.Select(p => new
                {
                    p.Name,
                    p.Country,
                    p.Latitude,
                    p.Longitude,
                }),
            },
            DistanceRange = new
            {
                MinKm = minKm,
                MaxKm = maxKm,
                TotalMatching = rangeResult.TotalMatching,
                DistanceFacets = rangeResult.GeoDistanceFacetFor(x => x.Coordinates)
                    .Ranges.Select(r => new { r.From, r.To, r.TotalCount }),
                Locations = rangeResult.Hits.Select(h => new
                {
                    h.Document.Name,
                    h.Document.Country,
                }),
            },
        };
    }

    public async Task<object> WildcardSearchAsync(string pattern)
    {
        var result = await _client.Search<LocationItemPage>()
            .AddWildCardQuery(pattern, x => x.Name)
            .Take(10)
            .GetContentResultAsync();

        return new
        {
            Feature = "Wildcard search: .AddWildCardQuery() — custom extension from SearchExtensions",
            Pattern = pattern,
            TotalMatching = result.TotalMatching,
            Hits = result.Select(p => new
            {
                p.Name,
                p.Country,
            }),
        };
    }

    public async Task<object> NestedFilterAsync(string skuPrefix)
    {
        var result = await _client.Search<GenericProduct>()
            .Filter(p => p.VariationModels(), x => x.Code.PrefixCaseInsensitive(skuPrefix))
            .Select(p => p.VariationModels())
            .Take(10)
            .GetResultAsync();

        return new
        {
            Feature = "Nested queries: .Filter(nested lambda) on VariationModels(), .PrefixCaseInsensitive() on nested Code",
            SkuPrefix = skuPrefix,
            TotalMatching = result.TotalMatching,
            Products = result.Hits.Select(h => new
            {
                Variations = h.Document?.Select(v => new
                {
                    v.Code,
                    v.Name,
                }),
            }),
        };
    }

    public async Task<object> MultiSearchAsync(string q)
    {
        var result = await _client.Search<FoundationPageData>()
            .For(q)
            .FilterFacet("Locations", x => x.MatchTypeHierarchy(typeof(LocationItemPage)))
            .FilterFacet("BlogPosts", x => x.MatchTypeHierarchy(typeof(BlogItemPage)))
            .FilterFacet("People", x => x.MatchTypeHierarchy(typeof(PersonPage)))
            .FilterFacet("Events", x => x.MatchTypeHierarchy(typeof(CalendarEventPage)))
            .PublishedInCurrentLanguage()
            .FilterForVisitor()
            .Take(10)
            .GetContentResultAsync();

        var sr = result.SearchResult;
        return new
        {
            Feature = "Multi-search / cross-type: Search<FoundationPageData> + .MatchTypeHierarchy() counting per subtype",
            Query = q,
            TotalMatching = result.TotalMatching,
            TypeCounts = new
            {
                Locations = sr.FilterFacet("Locations").Count,
                BlogPosts = sr.FilterFacet("BlogPosts").Count,
                People = sr.FilterFacet("People").Count,
                Events = sr.FilterFacet("Events").Count,
            },
            Hits = result.Select(p => new
            {
                p.Name,
                Type = p.GetOriginalType().Name,
            }),
        };
    }

    public async Task<object> RunAllAsync()
    {
        var examples = new Dictionary<string, Func<Task<object>>>
        {
            ["basic-search"] = () => BasicTypedSearchAsync("europe"),
            ["field-search"] = () => FieldSpecificSearchAsync("stockholm"),
            ["filter-basics"] = () => FilterBasicsAsync("Europe", "Sweden"),
            ["filter-boolean-operators"] = () => FilterBooleanOperatorsAsync("Europe", "Asia"),
            ["filter-advanced"] = () => FilterAdvancedAsync("sw", 1000000, 50000000),
            ["filter-datetime"] = () => FilterDateTimeAsync(null, null),
            ["filter-type-hierarchy"] = FilterTypeHierarchyAsync,
            ["facets-terms"] = () => FacetsTermsAsync(""),
            ["facets-range"] = FacetsRangeAsync,
            ["facets-geo-distance"] = () => FacetsGeoDistanceAsync(59.33, 18.07),
            ["boosting"] = () => BoostingAsync("beach"),
            ["sorting"] = SortingAsync,
            ["pagination"] = () => PaginationAsync(1, 5, 1),
            ["cms-filters"] = () => CmsFiltersAsync(""),
            ["highlighting"] = () => HighlightingAsync("beach"),
            ["more-like-this"] = () => MoreLikeThisAsync("sunny beach resort tropical vacation"),
            ["best-bets"] = () => BestBetsAsync("travel"),
            ["unified-search"] = () => UnifiedSearchAsync("travel"),
            ["statistics"] = () => StatisticsAndTrackingAsync("stokholm"),
            ["geo-search"] = () => GeoSearchAsync(59.33, 18.07, 2000, 500, 3000),
            ["wildcard"] = () => WildcardSearchAsync("stock*"),
            ["nested-filter"] = () => NestedFilterAsync("SKU"),
            ["multi-search"] = () => MultiSearchAsync("foundation"),
        };

        var results = new Dictionary<string, object>();
        var tasks = examples.Select(async kvp =>
        {
            try
            {
                var result = await kvp.Value();
                lock (results)
                {
                    results[kvp.Key] = result;
                }
            }
            catch (Exception ex)
            {
                lock (results)
                {
                    results[kvp.Key] = new { Error = ex.Message };
                }
            }
        });

        await Task.WhenAll(tasks);
        return results;
    }
}
