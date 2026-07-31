using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using JetBrains.Annotations;

public class QueryManagement
{
    public static string ToQueryString(QueryParams parameters)
    {
        var sb = new StringBuilder();
        bool isFirst = true;

        void Append(string key, string value)
        {
            if (isFirst)
            {
                sb.Append('?');
                isFirst = false;
            }
            else
            {
                sb.Append('&');
            }
            sb.Append(key).Append('=').Append(Uri.EscapeDataString(value));
        }

        if (parameters.page != null && parameters.page > 0 && parameters.page != 1)
            Append("page", parameters.page.ToString());

        if (parameters.per_page != null && parameters.per_page > 0 && parameters.per_page != 10)
            Append("per_page", parameters.per_page.ToString());

        if (!string.IsNullOrEmpty(parameters.search))
            Append("search", parameters.search);

        if (!string.IsNullOrEmpty(parameters.sort))
            Append("sort", parameters.sort);

        if (!string.IsNullOrEmpty(parameters.order))
        {
            string lowerOrder = parameters.order.ToLower();
            if (lowerOrder == "asc" || lowerOrder == "desc")
            {
                Append("order", lowerOrder);
            }
        }

        if (parameters.alteration_id != null)
        {
            foreach (var alteration in parameters.alteration_id)
            {
                Append("alteration_id", alteration.ToString());
            }
        }
        
        if (parameters.material_id != null)
        {
            foreach (var material in parameters.material_id)
            {
                Append("material_id", material.ToString());
            }
        }
        
        
        if (parameters.year_from != null)
            Append("year_from", parameters.year_from.ToString());
        
        if (parameters.year_to != null)
            Append("year_to", parameters.year_to.ToString());
        
        if (parameters.mime_type != null)
        {
            foreach (var mimeType in parameters.mime_type)
            {
                Append("mime_type", mimeType);
            }
        }
        
        if (!string.IsNullOrEmpty(parameters.reference_date_from))
            Append("reference_date_from", parameters.reference_date_from);
        
        if (!string.IsNullOrEmpty(parameters.reference_date_to))
            Append("reference_date_to", parameters.reference_date_to);
        
        if (!string.IsNullOrEmpty(parameters.scale))
            Append("scale", parameters.scale);
        
        if (!string.IsNullOrEmpty(parameters.category))
            Append("category", parameters.category);
        
        if (!string.IsNullOrEmpty(parameters.state_of_conservation))
            Append("state_of_conservation", parameters.state_of_conservation);
        
        if (parameters.country_id != null)
            Append("country_id", parameters.country_id.ToString());
        
        if (parameters.province_id != null)
            Append("province_id", parameters.province_id.ToString());
        
        if (parameters.municipality_id != null)
            Append("municipality_id", parameters.municipality_id.ToString());
        
        if (!string.IsNullOrEmpty(parameters.country))
            Append("country", parameters.country);
        
        if (!string.IsNullOrEmpty(parameters.admin1))
            Append("admin1", parameters.admin1);

        return sb.ToString(); // may return "" if no params are valid/different
    }
    
    public static QueryParams ParseUrl(string url)
    {
        var queryParams = new QueryParams();

        try
        {
            Uri uri = new Uri(url);
            var query = HttpUtility.ParseQueryString(uri.Query);

            if (int.TryParse(query.Get("page"), out int page))
                queryParams.page = page;

            if (int.TryParse(query.Get("per_page"), out int perPage))
                queryParams.per_page = perPage;

            if (query["search"] != null)
                queryParams.search = query["search"];

            if (query["sort"] != null)
                queryParams.sort = query["sort"];

            if (query["order"] != null)
                queryParams.order = query["order"];
            
            if (int.TryParse(query.Get("alteration_id"), out int alterationId))
                queryParams.alteration_id = new List<int> {alterationId};
            
            if (int.TryParse(query.Get("material_id"), out int materialId))
                queryParams.material_id = new List<int> {materialId};
            
            if (int.TryParse(query.Get("year_from"), out int yearFrom))
                queryParams.year_from = yearFrom;
            
            if (int.TryParse(query.Get("year_to"), out int yearTo))
                queryParams.year_to = yearTo;
            
            if (query["reference_date_from"] != null)
                queryParams.reference_date_from = query["reference_date_from"];
            
            if (query["reference_date_to"] != null)
                queryParams.reference_date_to = query["reference_date_to"];
            
            if (query["scale"] != null)
                queryParams.scale = query["scale"];
            
            if (query["category"] != null)
                queryParams.category = query["category"];
            
            if (query["state_of_conservation"] != null)
                queryParams.state_of_conservation = query["state_of_conservation"];
            
            if (int.TryParse(query.Get("country_id"), out int countryId))
                queryParams.country_id = countryId;
            
            if (int.TryParse(query.Get("province_id"), out int provinceId))
                queryParams.province_id = provinceId;
            
            if (int.TryParse(query.Get("municipality_id"), out int municipalityId))
                queryParams.municipality_id = municipalityId;
            
            if (query["country"] != null)
                queryParams.country = query["country"];
            
            if (query["admin1"] != null)
                queryParams.admin1 = query["admin1"];
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Error parsing URL: " + ex.Message);
        }

        return queryParams;
    }
}

[Serializable]
public class QueryParams
{
    public int? page = 1;
    public int? per_page = 10;
    public string search = "";
    public string sort = "";
    public string order = "asc";
    [CanBeNull] public List<int> alteration_id = null;
    [CanBeNull] public List<int> material_id = null;
    public int? year_from = null;
    public int? year_to = null;
    [CanBeNull] public List<string> mime_type = null;
    public string reference_date_from = null;
    public string reference_date_to = null;
    public string scale = null;
    public string category = null;
    public string state_of_conservation = null;
    public int? country_id = null;
    public int? province_id = null;
    public int? municipality_id = null;
    public string country = null;
    public string admin1 = null;

    public string QueryToString()
    {
        return "page=" + page + "&per_page=" + per_page + "&search=" + search + "&sort="
               + sort + "&order=" + order + "&alteration_id=" + alteration_id + "&material_id="
               + material_id + "&year_from=" + year_from + "&year_to=" + year_to + "&mime_type="
               + mime_type + "&reference_date_from=" + reference_date_from + "&reference_date_to="
               + reference_date_to + "&scale=" + scale + "&category" + category + "&state_of_conservation="
               + state_of_conservation + "&country_id=" + country_id + "&province_id=" + province_id
               + "&municipality_id=" + municipality_id + "&country=" + country + "&admin1=" + admin1;
    }
}