using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Helpers;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;

namespace R10.Web.Services.MailDownload
{
    public static class MailDownloadHelper
    {
        public static string GetDownloadFileName(this Message message)
        {
            return $"{message.From?.EmailAddress.Name} - {(message.Subject ?? "").Left(20)} - {((DateTimeOffset)message.ReceivedDateTime).DateTime.ToString("yyyyddMMHHmmss")}.eml";
        }

        public static IQueryable<Invention> AddCriteria(this IQueryable<Invention> queryable, List<QueryFilterViewModel> filters)
        {
            var caseNumber = filters.FirstOrDefault(f => f.Property == "CaseNumber");
            if (caseNumber != null)
            {
                var values = caseNumber.GetValueList();

                if (values.Count > 0)
                {
                    queryable = queryable.Where(q => values.Contains(q.CaseNumber));

                    filters.Remove(caseNumber);
                }
            }

            var clientRef = filters.FirstOrDefault(f => f.Property == "ClientRef");
            if (clientRef != null)
            {
                var values = clientRef.GetValueList();

                if (values.Count > 0)
                {
                    queryable = queryable.Where(q => !string.IsNullOrEmpty(q.ClientRef) && values.Contains(q.ClientRef));

                    filters.Remove(clientRef);
                }
            }

            if (filters.Any())
                queryable = QueryHelper.BuildCriteria<Invention>(queryable, filters);

            return queryable;
        }

        public static IQueryable<CountryApplication> AddCriteria(this IQueryable<CountryApplication> queryable, List<QueryFilterViewModel> filters, string[] separators)
        {
            //combined casenumber/country/subcase
            var caseNoCtrySub = filters.FirstOrDefault(f => f.Property == "CaseNumberCountrySubCase");
            if (caseNoCtrySub != null)
            {
                var values = caseNoCtrySub.GetCaseNumberCountrySubCase(separators);

                if (values.CaseNumbers.Any())
                    queryable = queryable.Where(q => values.CaseNumbers.Contains(q.CaseNumber));

                if (values.Countries.Any())
                    queryable = queryable.Where(q => values.Countries.Contains(q.Country));

                if (values.SubCases.Any())
                    queryable = queryable.Where(q => values.SubCases.Contains(q.SubCase ?? ""));

                filters.Remove(caseNoCtrySub);
            }

            var caseNumber = filters.FirstOrDefault(f => f.Property == "CaseNumber");
            if (caseNumber != null)
            {
                var values = caseNumber.GetValueList();

                if (values.Count > 0)
                {
                    queryable = queryable.Where(q => values.Contains(q.CaseNumber));

                    filters.Remove(caseNumber);
                }
            }

            var country = filters.FirstOrDefault(f => f.Property == "Country");
            if (country != null)
            {
                var values = country.GetValueList();

                if (values.Count > 0)
                {
                    queryable = queryable.Where(q => values.Contains(q.Country));

                    filters.Remove(country);
                }
            }

            var subCase = filters.FirstOrDefault(f => f.Property == "SubCase");
            if (subCase != null)
            {
                var values = subCase.GetValueList();

                if (values.Count > 0)
                {
                    queryable = queryable.Where(q => values.Contains(q.SubCase ?? ""));

                    filters.Remove(subCase);
                }
            }

            var caseType = filters.FirstOrDefault(f => f.Property == "CaseType");
            if (caseType != null)
            {
                var values = caseType.GetValueList();

                if (values.Count > 0)
                {
                    queryable = queryable.Where(q => values.Contains(q.CaseType ?? ""));

                    filters.Remove(caseType);
                }
            }

            var clientRef = filters.FirstOrDefault(f => f.Property == "AppClientRef");
            if (clientRef != null)
            {
                var values = clientRef.GetValueList();

                if (values.Count > 0)
                {
                    queryable = queryable.Where(q => !string.IsNullOrEmpty(q.AppClientRef) && values.Contains(q.AppClientRef));

                    filters.Remove(clientRef);
                }
            }

            if (filters.Any())
                queryable = QueryHelper.BuildCriteria<CountryApplication>(queryable, filters);

            return queryable;
        }

        public static IQueryable<TmkTrademark> AddCriteria(this IQueryable<TmkTrademark> queryable, List<QueryFilterViewModel> filters, string[] separators)
        {
            //combined casenumber/country/subcase
            var caseNoCtrySub = filters.FirstOrDefault(f => f.Property == "CaseNumberCountrySubCase");
            if (caseNoCtrySub != null)
            {
                var values = caseNoCtrySub.GetCaseNumberCountrySubCase(separators);

                if (values.CaseNumbers.Any())
                    queryable = queryable.Where(q => values.CaseNumbers.Contains(q.CaseNumber));

                if (values.Countries.Any())
                    queryable = queryable.Where(q => values.Countries.Contains(q.Country));

                if (values.SubCases.Any())
                    queryable = queryable.Where(q => values.SubCases.Contains(q.SubCase ?? ""));

                filters.Remove(caseNoCtrySub);
            }

            var caseNumber = filters.FirstOrDefault(f => f.Property == "CaseNumber");
            if (caseNumber != null)
            {
                var values = caseNumber.GetValueList();

                if (values.Count > 0)
                {
                    queryable = queryable.Where(q => values.Contains(q.CaseNumber));

                    filters.Remove(caseNumber);
                }
            }

            var country = filters.FirstOrDefault(f => f.Property == "Country");
            if (country != null)
            {
                var values = country.GetValueList();

                if (values.Count > 0)
                {
                    queryable = queryable.Where(q => values.Contains(q.Country));

                    filters.Remove(country);
                }
            }

            var subCase = filters.FirstOrDefault(f => f.Property == "SubCase");
            if (subCase != null)
            {
                var values = subCase.GetValueList();

                if (values.Count > 0)
                {
                    queryable = queryable.Where(q => values.Contains(q.SubCase ?? ""));

                    filters.Remove(subCase);
                }
            }

            var caseType = filters.FirstOrDefault(f => f.Property == "CaseType");
            if (caseType != null)
            {
                var values = caseType.GetValueList();

                if (values.Count > 0)
                {
                    queryable = queryable.Where(q => values.Contains(q.CaseType ?? ""));

                    filters.Remove(caseType);
                }
            }

            var clientRef = filters.FirstOrDefault(f => f.Property == "ClientRef");
            if (clientRef != null)
            {
                var values = clientRef.GetValueList();

                if (values.Count > 0)
                {
                    queryable = queryable.Where(q => !string.IsNullOrEmpty(q.ClientRef) && values.Contains(q.ClientRef));

                    filters.Remove(clientRef);
                }
            }

            if (filters.Any())
                queryable = QueryHelper.BuildCriteria<TmkTrademark>(queryable, filters);

            return queryable;
        }

        public static (List<string> CaseNumbers, List<string> Countries, List<string> SubCases) GetCaseNumberCountrySubCase(this QueryFilterViewModel caseNumberCountrySubCases, string[] separators)
        {
            var data = caseNumberCountrySubCases.GetValueList(); 
            var caseNumbers = new List<string>();
            var countries = new List<string>();
            var subCases = new List<string>();

            //GetValueList() returns an empty list if there is only one value
            //because "[" characters in QueryFilterViewModel.Value is escaped
            //causing an error when deserializing to List<string>
            if (data.Count == 0)
                data.Add(caseNumberCountrySubCases.Value);

            foreach (var caseNumberCountrySubCase in data)
            {
                foreach (var separator in separators)
                {
                    var values = caseNumberCountrySubCase.Trim().Split(separator);

                    if (values.Length > 1)
                    {
                        caseNumbers.Add(values[0]);

                        for (var i = 1; i < values.Length; i++)
                        {
                            if (!separators.Any(s => s.Trim() == values[i].Trim()))
                            {
                                countries.Add(values[i]);
                                break;
                            }
                        }

                        if (values.Length < 3)
                        {
                            foreach (var countrySubCaseSeparator in separators)
                            {
                                var countrySubCase = values[1].Trim().Split(countrySubCaseSeparator);
                                if (countrySubCase.Length > 1 && !separators.Any(s => s.Trim() == countrySubCase[0].Trim()))
                                {
                                    countries.Add(countrySubCase[0]);

                                    for(var i = 1; i < countrySubCase.Length; i++)
                                    {
                                        if (!separators.Any(s => s.Trim() == countrySubCase[i].Trim()))
                                        {
                                            subCases.Add(countrySubCase[i]);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else if (!separators.Any(s => s.Trim() == values[1].Trim()))
                            subCases.Add(String.Join(separator, values.Skip(2).ToArray()));
                    }
                    else
                        caseNumbers.Add(values[0]);
                }
            }

            //include empty subcase in IN list
            //if (subCases.Count > 0)
            //    subCases.Add("");

            return (
                caseNumbers.Select(s => s.Trim()).Distinct().ToList(),
                countries.Select(s => s.Trim()).Distinct().ToList(),
                subCases.Select(s => s.Trim()).Distinct().ToList()
                );
        }
    }
}
