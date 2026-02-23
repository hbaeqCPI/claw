using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace R10.Web.Services
{
    public class CustomReportService : ICustomReportService
    {
        private IDataQueryService _dataQueryService;
        private readonly UserManager<CPiUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISystemSettings<DefaultSetting> _defaultSettings;
        private readonly ExportHelper _exportHelper;
        private readonly IReportDeployService _reportDeployService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public CustomReportService(
            IDataQueryService dataQueryService, 
            UserManager<CPiUser> userManager, 
            IHttpContextAccessor httpContextAccessor,
            ISystemSettings<DefaultSetting> defaultSettings,
            ExportHelper exportHelper,
            IReportDeployService reportDeployService,
            IStringLocalizer<SharedResource> localizer)
        {
            _dataQueryService = dataQueryService;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _defaultSettings = defaultSettings;
            _exportHelper = exportHelper;
            _reportDeployService = reportDeployService;
            _localizer = localizer;
        }
        public async Task<string> GetSQLExpr(int id, string? userEmail = null)
        {
            var dataQuery = await _dataQueryService.GetByIdAsync(id);
            if (dataQuery != null)
            {
                var user = _userManager.Users.Where(c => c.Email.ToLower() == (userEmail ?? _httpContextAccessor.HttpContext.User.Identity.Name.ToLower())).First();
                if (user != null)
                    if ((user.UserType == CPiUserType.Administrator || user.UserType == CPiUserType.SuperAdministrator) || dataQuery.IsShared || dataQuery.OwnedBy.ToLower() == (userEmail ?? _httpContextAccessor.HttpContext.User.Identity.Name.ToLower()))
                        return dataQuery.SQLExpr;
                throw new InvalidOperationException(_localizer["You don't have permission for the report data source."]);
            }
            return null;
        }

        public async Task<DataTable> RunQuery(string userId, int queryId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;
            string SQLExpr = await GetSQLExpr(queryId, user.Email.ToLower());
            if (SQLExpr == null)
                return null;
            var hasEntityFilterOn = user.EntityFilterType != CPiEntityType.None;
            var hasRespOfficeOn = false;
            return _dataQueryService.RunCRQuery(SQLExpr, userId, hasEntityFilterOn, hasRespOfficeOn);
        }

        public async Task<MemoryStream> GetCustomReport(int id)
        {

            string SQLExpr = await GetSQLExpr(id);
            if (SQLExpr == null)
                return null;
            var contents = GetTemplateAsString(SQLExpr, true);

            //set parameters.
            //p1 UserId
            //contents = ReplaceDefaultParameter(contents, "p1", _httpContextAccessor.HttpContext.User.GetUserIdentifier());
            ////p2 APIKey
            //contents = ReplaceDefaultParameter(contents, "p2", (await _defaultSettings.GetSetting()).CustomReportAPIKey);
            //p3 queryId
            contents = ReplaceDefaultParameter(contents, "p3", id.ToString());
            ////p4 root url
            //contents = ReplaceDefaultParameter(contents, "p4", _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase);

            byte[] byteArray = Encoding.ASCII.GetBytes(contents);
            MemoryStream streamResult = new MemoryStream(byteArray);
            return streamResult;
        }

        public async Task<MemoryStream> GetCustomReport(string queryName)
        {
            var id = (await GetDataQuery(queryName)).QueryId;
            var contents = GetTemplateAsString(queryName);
            //p3 queryId
            contents = ReplaceDefaultParameter(contents, "p3", id.ToString());

            //Adding local Data
            string SQLExpr = await GetSQLExpr(id);
            if (SQLExpr == null)
                return null;
            var dt = _dataQueryService.RunCRQuery(SQLExpr, _httpContextAccessor.HttpContext.User.GetUserIdentifier(), false, false);
            contents = InsertTemplate(contents, GetXMLDataTemplate(dt), "<CommandText>");
            byte[] byteArray = Encoding.ASCII.GetBytes(contents);
            MemoryStream streamResult = new MemoryStream(byteArray);
            return streamResult;
        }

        public async Task<MemoryStream> ConvertServerRDLToLocalRDL(byte[] reportDefinition)
        {
            var contents = System.Text.Encoding.UTF8.GetString(reportDefinition);
            contents = ReplaceTemplate(contents, "", "<ConnectString>", "</ConnectString>");

            string SQLExpr = await GetSQLExpr(GetCustomQueryIdInRDL(contents));
            if (SQLExpr == null)
                throw new InvalidOperationException(_localizer["Can not find the data source. Please make sure the data source is working correctly."]);
            var dt = _dataQueryService.RunCRQuery(SQLExpr, _httpContextAccessor.HttpContext.User.GetUserIdentifier(), false, false);
            if(dt.Rows.Count == 0)
            {
                throw new InvalidOperationException(_localizer["Please check if you have permission to the data source and there is data in the data source."]);
            }
            var data = GetXMLDataTemplate(dt);
            contents = ReplaceTemplate(contents, data, "<CommandText>", "</CommandText>");

            byte[] byteArray = Encoding.ASCII.GetBytes(contents);
            MemoryStream streamResult = new MemoryStream(byteArray);
            return streamResult;
        }

        public async Task<string> GetDataPermissionTextByReportId(int reportId)
        {
            var reportDefinition = await _reportDeployService.GetReportDefinition(reportId.ToString());
            var contents = System.Text.Encoding.UTF8.GetString(reportDefinition);
            contents = ReplaceTemplate(contents, "", "<ConnectString>", "</ConnectString>");
            int queryId = GetCustomQueryIdInRDL(contents);
            var dataQuery = await _dataQueryService.GetByIdAsync(queryId);
            if (dataQuery != null)
            {
                if (_httpContextAccessor.HttpContext.User.IsAdmin() || dataQuery.IsShared || dataQuery.OwnedBy.ToLower() == _httpContextAccessor.HttpContext.User.Identity.Name.ToLower())
                    return "";
                return _localizer["You don't have permission to access the report data source."];
            }

            return _localizer["The report data source had been deleted."];
        }

        private int GetCustomQueryIdInRDL(string contents)
        {
            var searchPosition = "<ReportParameter Name=\"p3\">";
            var postfix = contents.Substring(contents.IndexOf(searchPosition) + searchPosition.Length);
            var valuePosition = "<Value>";
            var endValuePostion = "</Value>";
            var result = postfix.Substring(postfix.IndexOf(valuePosition) + valuePosition.Length, postfix.IndexOf(endValuePostion) - postfix.IndexOf(valuePosition) - valuePosition.Length);

            return int.Parse(result);
        }

        public async Task<string> ConvertLocalRDLToServerRDL(string rdlContents)
        {
            var connectString = "= Parameters!p4.Value &amp;\"/api/customReport/get?type=XML&amp;lang=us&amp;p1=\" &amp; Parameters!p1.Value &amp; \"&amp;p2=\" &amp; Parameters!p2.Value &amp; \"&amp;p3=\" &amp; Parameters!p3.Value";
            rdlContents = rdlContents.Replace("<ConnectString />", "<ConnectString></ConnectString>").Replace("<CommandText />", "<CommandText></CommandText>");
            var queryId = GetQueryId(rdlContents);
            var queryCommand = await GetQueryCommand(queryId);
            //Add connect string
            var contents = ReplaceTemplate(rdlContents, connectString, "<ConnectString>", "</ConnectString>");
            contents = ReplaceTemplate(contents, queryCommand, "<CommandText>", "</CommandText>");

            //p1 UserId
            contents = ReplaceDefaultParameter(contents, "p1", "p1");
            //p2 APIKey
            contents = ReplaceDefaultParameter(contents, "p2", "p2");
            //p4 root url
            contents = ReplaceDefaultParameter(contents, "p4", "p4");

            return contents;
        }

        private async Task<string> GetQueryCommand(int queryId)
        {
            string result = "&lt;Query&gt;&lt;ElementPath IgnoreNamespaces=\"true\"&gt;DocumentElement/CPiReport{";
            string SQLExpr = await GetSQLExpr(queryId);
            if (SQLExpr == null)
                return null;
            var dt = _dataQueryService.RunCRQuery(SQLExpr, _httpContextAccessor.HttpContext.User.GetUserIdentifier(), false, false);

            foreach (DataColumn column in dt.Columns)
            {
                result += column.ColumnName+",";
            }

            result = result.Substring(0, result.Length - 1);

            result += "}&lt;/ElementPath&gt;&lt;/Query&gt;";
            return result;
        }

        public int GetQueryId (string rdlContents)
        {
            var stringValue = GetParameterValue(rdlContents, "p3");
            return int.Parse(stringValue);
        }

        private string GetParameterValue(string text, string parameterName)
        {
            var searchPosition = "<ReportParameter Name=\"" + parameterName + "\">";
            var postfix = text.Substring(text.IndexOf(searchPosition) + searchPosition.Length);
            var valuePosition = "<Value>";
            var endValuePostion = "</Value>";
            var value = postfix.Substring(postfix.IndexOf(valuePosition) + valuePosition.Length, postfix.IndexOf(endValuePostion) - postfix.IndexOf(valuePosition) - valuePosition.Length);
            return value;
        }

        private string ReplaceDefaultParameter(string text, string parameterName, string value)
        {
            var searchPosition = "<ReportParameter Name=\""+ parameterName + "\">";
            var prefix = text.Substring(0, text.IndexOf(searchPosition) + searchPosition.Length);
            var postfix = text.Substring(text.IndexOf(searchPosition) + searchPosition.Length);
            var valuePosition = "<Value>";
            var endValuePostion = "</Value>";
            prefix += postfix.Substring(0, postfix.IndexOf(valuePosition) + valuePosition.Length);
            postfix = postfix.Substring(postfix.IndexOf(endValuePostion));
            return prefix + value + postfix;
        }

        private string ReplaceTemplate(string text, string value, string startPosition, string endPosition)
        {
            var prefix = text.Substring(0, text.IndexOf(startPosition) + startPosition.Length);
            var postfix = text.Substring(text.IndexOf(endPosition));
            return prefix + value + postfix;
        }

        private string GetTemplateAsString(string SQLExpr, bool isLandscape)
        {
            var fileName = isLandscape ? "CustomReportTemplate.rdl" : "CustomReportTemplate_Protrait.rdl";
            var filePath= "wwwroot/src/template/" + fileName;
            StreamReader sr = new StreamReader(File.OpenRead(filePath));
            var contents = sr.ReadToEnd();
            contents = contents.Replace("<ConnectString />", "<ConnectString></ConnectString>").Replace("<CommandText />", "<CommandText></CommandText>");

            var dt = _dataQueryService.RunCRQuery(SQLExpr, _httpContextAccessor.HttpContext.User.GetUserIdentifier(), false, false);

            //Adding Fields
            contents = InsertTemplate(contents, GetFieldsTemplate(dt), "</Query>");

            //Adding Table
            contents = InsertTemplate(contents, GetTableTemplate(dt), "<Body>");

            //Adding local Data
            contents = InsertTemplate(contents, GetXMLDataTemplate(dt), "<CommandText>");

            return contents;
        }

        private string GetXMLDataTemplate(DataTable dt)
        {
            var fileStream = _exportHelper.DataTableToXMLMemoryStream(dt, "CPICustomReportTemplate");
            var t = fileStream.ToArray();
            fileStream = new MemoryStream(t);
            StreamReader sr = new StreamReader(fileStream);
            var originalXML = sr.ReadToEnd();
            var result = "&lt;Query&gt;&lt;XmlData&gt;&lt;CPICustomReportTemplate&gt;";
            if(dt.Columns.Count == 0)
            {
                return "&lt;Query&gt;&lt;/Query&gt;";
            }
            result += GetXMLDataSourceForCR(originalXML);
            result += "&lt;/CPICustomReportTemplate&gt;&lt;/XmlData&gt;";
            result += GetElementPath(dt);
            result += "&lt;/Query&gt;";
            return result;
        }

        private string GetXMLDataSourceForCR(string originalXML)
        {
            var startPosition = "<CPICustomReportTemplate>";
            var endPosition = "</CPICustomReportTemplate>";
            var result = "";
            if (originalXML.Contains("<CPICustomReportTemplate>"))
                result = originalXML.Substring(originalXML.IndexOf(startPosition), originalXML.LastIndexOf(endPosition) - originalXML.IndexOf(startPosition) + endPosition.Length).Replace(startPosition,"<Row>").Replace(endPosition,"</Row>").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;");
            return result;
        }

        private string GetElementPath(DataTable dt)
        {
            var result = "&lt;ElementPath&gt;CPICustomReportTemplate{}/Row{";
            foreach (DataColumn column in dt.Columns)
            {
                //result += column.ColumnName + "("+ column.DataType.Name + "),";
                result += column.ColumnName + "(String),";
            }
            result = result.Substring(0, result.Length - 1);
            result += "}&lt;/ElementPath&gt;";
            return result;
        }

        private string GetTemplateAsString(string queryName)
        {
            var fileName = queryName + ".rdl";
            var filePath = "wwwroot/src/template/" + fileName;
            StreamReader sr = new StreamReader(File.OpenRead(filePath));
            var contents = sr.ReadToEnd();
            contents = contents.Replace("<ConnectString />", "<ConnectString></ConnectString>").Replace("<CommandText />", "<CommandText></CommandText>");

            return contents;
        }

        private string InsertTemplate(string text, string insertedText, string insertedPostion)
        {
            var prefix = text.Substring(0, text.IndexOf(insertedPostion) + insertedPostion.Length);
            var postfix = text.Substring(text.IndexOf(insertedPostion) + insertedPostion.Length);
            return prefix + insertedText + postfix;
        }

        private string GetTableTemplate(DataTable dt)
        {
            //init
            var result = "\r\n        <ReportItems>\r\n          <Tablix Name=\"Tablix1\">\r\n            <TablixBody>\r\n              <TablixColumns>";
            //add columns infor
            var columnsTemplate = "\r\n                <TablixColumn>\r\n                  <Width>1in</Width>\r\n                </TablixColumn>";
            foreach (DataColumn column in dt.Columns)
            {
                result += columnsTemplate;
            }
            //finish cloumns info
            result += "\r\n              </TablixColumns>\r\n              <TablixRows>\r\n                <TablixRow>\r\n                  <Height>0.25in</Height>\r\n                  <TablixCells>";
            //add header rows
            foreach (DataColumn column in dt.Columns)
            {
                result += TableHeaderTemplate(column.ColumnName);
            }
            result += "\r\n                  </TablixCells>\r\n                </TablixRow>\r\n                <TablixRow>\r\n                  <Height>0.25in</Height>\r\n                  <TablixCells>";
            //add field rows
            foreach (DataColumn column in dt.Columns)
            {
                if (column.ColumnName.ToLower().EndsWith("image"))
                {
                    result += TableItemTemplateForImage(column.ColumnName);
                }
                else
                {
                    result += TableItemTemplate(column.ColumnName, column.DataType);
                }
                
            }
            result += "\r\n                  </TablixCells>\r\n                </TablixRow>\r\n              </TablixRows>\r\n            </TablixBody>\r\n            <TablixColumnHierarchy>\r\n              <TablixMembers>"; 
            //columns Member
            var tablixTemplate = "\r\n                <TablixMember />";
            foreach (DataColumn column in dt.Columns)
            {
                result += tablixTemplate;
            }
            result += "\r\n              </TablixMembers>\r\n            </TablixColumnHierarchy>\r\n            <TablixRowHierarchy>\r\n              <TablixMembers>\r\n                <TablixMember>\r\n                  <KeepWithGroup>After</KeepWithGroup>\r\n<RepeatOnNewPage>true</RepeatOnNewPage>\r\n                </TablixMember>\r\n                <TablixMember>\r\n                  <Group Name=\"Details\" />\r\n                </TablixMember>\r\n              </TablixMembers>\r\n            </TablixRowHierarchy>\r\n        <KeepTogether>true</KeepTogether>\r\n            <DataSetName>QueryDataSet</DataSetName>\r\n            <Height>0.5in</Height>\r\n            <Width>1in</Width>\r\n            <Style>\r\n              <Border>\r\n                <Style>None</Style>\r\n              </Border>\r\n            </Style>\r\n          </Tablix>\r\n        </ReportItems>";

            return result;
        }

        private string TableHeaderTemplate(string fieldName)
        {
            var s1 = "\r\n                    <TablixCell>\r\n                      <CellContents>\r\n                        <Textbox Name=\"";
            var s2 = "\">\r\n                          <CanGrow>true</CanGrow>\r\n                          <KeepTogether>true</KeepTogether>\r\n                          <Paragraphs>\r\n                            <Paragraph>\r\n                              <TextRuns>\r\n                                <TextRun>\r\n                                  <Value>";
            var s3 = "</Value>\r\n                                  <Style />\r\n                                </TextRun>\r\n                              </TextRuns>\r\n                              <Style />\r\n                            </Paragraph>\r\n                          </Paragraphs>\r\n                          <rd:DefaultName>";
            var s4 = "</rd:DefaultName>\r\n                          <Style>\r\n                            <Border>\r\n                              <Style>None</Style>\r\n                            </Border>\r\n                            <PaddingLeft>2pt</PaddingLeft>\r\n                            <PaddingRight>2pt</PaddingRight>\r\n                            <PaddingTop>2pt</PaddingTop>\r\n                            <PaddingBottom>2pt</PaddingBottom>\r\n                          </Style>\r\n                        </Textbox>\r\n                      </CellContents>\r\n                    </TablixCell>";
            return s1 + fieldName + "HeaderText" + s2 + fieldName  + s3 + fieldName + s4;
        }

        private string TableItemTemplate(string fieldName, Type type)
        {
            var result = TableStringItemTemplate(fieldName);
            return result;
        }

        private string TableItemTemplateForImage(string fieldName)
        {
            var s1 = "\r\n                    <TablixCell>\r\n                      <CellContents>\r\n                        <Image Name=\"";
            var s2 = "\">\r\n                          <Source>External</Source>\r\n                          <Value>=Parameters!p5.Value + Fields!";
            var s3 = ".Value</Value>\r\n                          <Sizing>Fit</Sizing>\r\n                          <Visibility>\r\n                            <Hidden>=IsNothing(Fields!";
            var s4 = ".Value) OrElse Fields!";
            var s5 = ".Value = \"\" </Hidden>\r\n                          </Visibility>\r\n                          <Style>\r\n                            <Border>\r\n                              <Style>None</Style>\r\n                            </Border>\r\n                          </Style>\r\n                        </Image>\r\n                      </CellContents>\r\n                    </TablixCell>";
            return s1 + fieldName + s2 + fieldName + s3 + fieldName + s4 + fieldName + s5;
        }

        private string TableStringItemTemplate(string fieldName)
        {
            var s1 = "\r\n                    <TablixCell>\r\n                      <CellContents>\r\n                        <Textbox Name=\"";
            var s2 = "\">\r\n                          <CanGrow>true</CanGrow>\r\n                          <KeepTogether>true</KeepTogether>\r\n                          <Paragraphs>\r\n                            <Paragraph>\r\n                              <TextRuns>\r\n                                <TextRun>\r\n                                  <Value>=Fields!";
            var s3 = ".Value</Value>\r\n                                  <Style />\r\n                                </TextRun>\r\n                              </TextRuns>\r\n                              <Style />\r\n                            </Paragraph>\r\n                          </Paragraphs>\r\n                          <rd:DefaultName>";
            var s4 = "</rd:DefaultName>\r\n                          <Style>\r\n                            <Border>\r\n                              <Style>None</Style>\r\n                            </Border>\r\n                            <PaddingLeft>2pt</PaddingLeft>\r\n                            <PaddingRight>2pt</PaddingRight>\r\n                            <PaddingTop>2pt</PaddingTop>\r\n                            <PaddingBottom>2pt</PaddingBottom>\r\n                          </Style>\r\n                        </Textbox>\r\n                      </CellContents>\r\n                    </TablixCell>";
            return s1 + fieldName + s2 + fieldName + s3 + fieldName + s4;
        }

        private string GetFieldsTemplate(DataTable dt)
        {

            var result = "\r\n                    <Fields>";
            foreach(DataColumn column in dt.Columns)
            {
                result += FieldTemplate(column.ColumnName, column.DataType);
            }
            result += "\r\n                    </Fields>";
            return result;
        }

        private string FieldTemplate(string fieldName, Type type)
        {
            var s1 = "\r\n        <Field Name=\"";
            var s2 = "\">\r\n          <DataField>";
            var s3 = "</DataField>\r\n <rd:TypeName>";
            var s4 = "</rd:TypeName>\r\n </Field> ";
            return s1 + fieldName + s2 + fieldName + s3 + type.FullName + s4;
        }

        public string PrepareFileName(string name)
        {
            List<string> list = invalidFileCharacters();
            foreach (string s in list)
            {
                name = name.Replace(s, "");
            }
            return name;
        }

        private List<string> invalidFileCharacters()
        {
            List<string> list = new List<string>();
            list.Add("#");
            list.Add("%");
            list.Add("&");
            list.Add("{");
            list.Add("}");
            list.Add("\\");
            list.Add("<");
            list.Add(">");
            list.Add("*");
            list.Add("?");
            list.Add("/");
            //list.Add(" ");
            list.Add("$");
            list.Add("!");
            list.Add("'");
            list.Add("\"");
            list.Add(":");
            list.Add("@");
            list.Add("+");
            list.Add("`");
            list.Add("|");
            list.Add("=");

            return list;
        }

        public async Task AddDataQuery(DataQueryMain query)
        {
            await _dataQueryService.Add(query);
        }

        public async Task<DataQueryMain> GetDataQuery(string queryName)
        {
            var dataQuery = await _dataQueryService.GetByNameAsync(queryName);
            return dataQuery;
        }

        //public List<T> ConvertDataTable<T>(DataTable dt)
        //{
        //    List<T> data = new List<T>();
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        T item = GetItem<T>(row);
        //        data.Add(item);
        //    }
        //    return data;
        //}
        //private T GetItem<T>(DataRow dr)
        //{
        //    Type temp = typeof(T);
        //    T obj = Activator.CreateInstance<T>();

        //    foreach (DataColumn column in dr.Table.Columns)
        //    {
        //        foreach (PropertyInfo pro in temp.GetProperties())
        //        {
        //            if (pro.Name == column.ColumnName)
        //                pro.SetValue(obj, dr[column.ColumnName], null);
        //            else
        //                continue;
        //        }
        //    }
        //    return obj;
        //}
    }
}
