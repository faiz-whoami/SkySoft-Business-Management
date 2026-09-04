using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Web;

namespace business_managment_system.Services
{
    public class CrystalPdfService
    {
        public const string EngineAssemblyName =
            "CrystalDecisions.CrystalReports.Engine, Version=13.0.4000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304";
        public const string SharedAssemblyName =
            "CrystalDecisions.Shared, Version=13.0.4000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304";

        public bool IsAvailable()
        {
            return TryLoad(EngineAssemblyName) != null && TryLoad(SharedAssemblyName) != null;
        }

        public byte[] ExportTable(DataTable table, string title, string templateName)
        {
            if (table == null || table.Rows.Count == 0)
            {
                throw new InvalidOperationException("There is no data for this report.");
            }

            var engine = TryLoad(EngineAssemblyName);
            var shared = TryLoad(SharedAssemblyName);
            if (engine == null || shared == null)
            {
                throw new InvalidOperationException(MissingCrystalMessage());
            }

            var reportsFolder = ReportsFolder();
            Directory.CreateDirectory(reportsFolder);
            var rptPath = Path.Combine(reportsFolder, templateName + ".rpt");
            EnsureTemplate(rptPath, table, title, engine);

            var reportType = engine.GetType("CrystalDecisions.CrystalReports.Engine.ReportDocument", true);
            var formatType = shared.GetType("CrystalDecisions.Shared.ExportFormatType", true);
            var pdf = Enum.Parse(formatType, "PortableDocFormat");

            dynamic report = Activator.CreateInstance(reportType);
            try
            {
                report.Load(rptPath);
                report.SetDataSource(table);
                report.SummaryInfo.ReportTitle = title;
                using (var stream = (Stream)report.ExportToStream(pdf))
                using (var memory = new MemoryStream())
                {
                    stream.CopyTo(memory);
                    return memory.ToArray();
                }
            }
            finally
            {
                try { report.Close(); } catch { }
                try { report.Dispose(); } catch { }
            }
        }

        public static string MissingCrystalMessage()
        {
            return "SAP Crystal Reports for Visual Studio (v13 / 13.0.4000.0) is not installed. "
                + "Close Visual Studio, run the CRforVS installer as Administrator from "
                + "https://origin.softwaredownloads.sap.com/public/site/index.html "
                + "(product: SAP Crystal Reports, version for Visual Studio — Install Package), "
                + "then reopen the project and try again.";
        }

        private void EnsureTemplate(string rptPath, DataTable table, string title, Assembly engine)
        {
            if (File.Exists(rptPath))
            {
                return;
            }

            try
            {
                CrystalRptBuilder.Create(rptPath, table, title);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Crystal Reports is installed, but the .rpt template could not be created automatically ("
                    + ex.Message
                    + "). In Visual Studio, right-click Reports, Add → New Item → Crystal Reports, "
                    + "and bind it to a DataSet whose columns match this report.",
                    ex);
            }

            if (!File.Exists(rptPath))
            {
                throw new InvalidOperationException(
                    "Crystal template was not saved to " + rptPath + ".");
            }
        }

        private static string ReportsFolder()
        {
            var root = HttpContext.Current != null
                ? HttpContext.Current.Server.MapPath("~/Reports")
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
            return root;
        }

        private static Assembly TryLoad(string assemblyName)
        {
            try
            {
                return Assembly.Load(assemblyName);
            }
            catch
            {
                return null;
            }
        }
    }

    internal static class CrystalRptBuilder
    {
        public static void Create(string rptPath, DataTable table, string title)
        {
            var clientDoc = Assembly.Load(
                "CrystalDecisions.ReportAppServer.ClientDoc, Version=13.0.4000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304");
            var dataDef = Assembly.Load(
                "CrystalDecisions.ReportAppServer.DataDefModel, Version=13.0.4000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304");
            var reportDef = Assembly.Load(
                "CrystalDecisions.ReportAppServer.ReportDefModel, Version=13.0.4000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304");

            var rcdType = clientDoc.GetType("CrystalDecisions.ReportAppServer.ClientDoc.ReportClientDocumentClass", true);
            dynamic rcd = Activator.CreateInstance(rcdType);
            rcd.Create();

            var connectionInfoType = dataDef.GetType("CrystalDecisions.ReportAppServer.DataDefModel.ConnectionInfo", true);
            var propertyBagType = dataDef.GetType("CrystalDecisions.ReportAppServer.DataDefModel.PropertyBag", true);
            var tableType = dataDef.GetType("CrystalDecisions.ReportAppServer.DataDefModel.Table", true);
            var fieldType = dataDef.GetType("CrystalDecisions.ReportAppServer.DataDefModel.ISCRField")
                ?? dataDef.GetType("CrystalDecisions.ReportAppServer.DataDefModel.Field", false);

            dynamic logon = Activator.CreateInstance(propertyBagType);
            logon.Add("File Path", "");
            logon.Add("Internal Connection ID", Guid.NewGuid().ToString("B").ToUpperInvariant());

            dynamic attributes = Activator.CreateInstance(propertyBagType);
            attributes.Add("Database DLL", "crdb_adoplus.dll");
            attributes.Add("QE_DatabaseName", "");
            attributes.Add("QE_DatabaseType", "ADO.NET (XML)");
            attributes.Add("QE_LogonProperties", logon);
            attributes.Add("QE_ServerDescription", table.TableName);
            attributes.Add("QE_SQLDB", false);
            attributes.Add("SSO Enabled", false);

            dynamic connection = Activator.CreateInstance(connectionInfoType);
            connection.Attributes = attributes;
            connection.UserName = "";
            connection.Password = "";
            connection.Kind = 5;

            dynamic rasTable = Activator.CreateInstance(tableType);
            rasTable.ConnectionInfo = connection;
            rasTable.Name = table.TableName;
            rasTable.Alias = table.TableName;
            rasTable.QualifiedName = table.TableName;

            var crFieldValueType = dataDef.GetType("CrystalDecisions.ReportAppServer.DataDefModel.CrFieldValueTypeEnum", true);
            foreach (DataColumn column in table.Columns)
            {
                var dbFieldType = dataDef.GetType("CrystalDecisions.ReportAppServer.DataDefModel.DbField", true);
                dynamic dbField = Activator.CreateInstance(dbFieldType);
                dbField.Name = column.ColumnName;
                dbField.Description = column.ColumnName;
                dbField.Type = MapFieldType(column.DataType, crFieldValueType);
                rasTable.DataFields.Add(dbField);
            }

            rcd.DatabaseController.AddTable(rasTable, null);

            AddText(rcd, reportDef, title, 240, 200, 11000, 600, 1);
            var left = 200;
            var width = Math.Max(1400, 14500 / Math.Max(table.Columns.Count, 1));
            for (var i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];
                AddText(rcd, reportDef, column.ColumnName, 200, left, width, 400, 2);
                AddField(rcd, reportDef, dataDef, table.TableName, column.ColumnName, 200, left, width, 400);
                left += width;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(rptPath));
            rcd.SaveAs(rptPath, 0);
            rcd.Close();
        }

        private static object MapFieldType(Type dataType, Type enumType)
        {
            string name;
            if (dataType == typeof(int) || dataType == typeof(int?) || dataType == typeof(long))
            {
                name = "crFieldValueTypeInt32sType";
            }
            else if (dataType == typeof(decimal) || dataType == typeof(decimal?) || dataType == typeof(double))
            {
                name = "crFieldValueTypeNumberType";
            }
            else if (dataType == typeof(DateTime) || dataType == typeof(DateTime?))
            {
                name = "crFieldValueTypeDateTimeType";
            }
            else if (dataType == typeof(bool))
            {
                name = "crFieldValueTypeBooleanType";
            }
            else
            {
                name = "crFieldValueTypeStringType";
            }

            return Enum.Parse(enumType, name);
        }

        private static void AddText(dynamic rcd, Assembly reportDef, string text, int top, int left, int width, int height, int sectionKind)
        {
            var textType = reportDef.GetType("CrystalDecisions.ReportAppServer.ReportDefModel.TextObject", true);
            dynamic textObject = Activator.CreateInstance(textType);
            textObject.Name = "Text_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            textObject.Text = text;
            textObject.Top = top;
            textObject.Left = left;
            textObject.Width = width;
            textObject.Height = height;
            var section = FindSection(rcd, reportDef, sectionKind);
            if (section != null)
            {
                section.ReportObjects.Add(textObject);
            }
        }

        private static void AddField(dynamic rcd, Assembly reportDef, Assembly dataDef, string tableName, string fieldName, int top, int left, int width, int height)
        {
            var fieldObjectType = reportDef.GetType("CrystalDecisions.ReportAppServer.ReportDefModel.FieldObject", true);
            dynamic fieldObject = Activator.CreateInstance(fieldObjectType);
            fieldObject.Name = "Field_" + fieldName;
            fieldObject.DataSource = tableName + "." + fieldName;
            fieldObject.Top = top;
            fieldObject.Left = left;
            fieldObject.Width = width;
            fieldObject.Height = height;
            var section = FindSection(rcd, reportDef, 3);
            if (section != null)
            {
                section.ReportObjects.Add(fieldObject);
            }
        }

        private static dynamic FindSection(dynamic rcd, Assembly reportDef, int kind)
        {
            try
            {
                var areas = rcd.ReportDefController.ReportDefinition.Areas;
                foreach (var area in areas)
                {
                    foreach (var section in area.Sections)
                    {
                        return section;
                    }
                }
            }
            catch
            {
            }

            return null;
        }
    }
}
