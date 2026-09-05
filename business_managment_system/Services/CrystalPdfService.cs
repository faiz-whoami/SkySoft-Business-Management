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

            if (!IsAvailable())
            {
                return TablePdfWriter.FromTable(table, title);
            }

            var reportsFolder = ReportsFolder();
            Directory.CreateDirectory(reportsFolder);
            var rptPath = Path.Combine(reportsFolder, templateName + ".rpt");
            EnsureTemplate(rptPath, table, title);

            var engine = TryLoad(EngineAssemblyName);
            var shared = TryLoad(SharedAssemblyName);
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

        public static string ReportsFolder()
        {
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Server.MapPath("~/Reports");
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
        }

        private void EnsureTemplate(string rptPath, DataTable table, string title)
        {
            if (File.Exists(rptPath))
            {
                return;
            }

            CrystalRptBuilder.Create(rptPath, table, title);
            if (!File.Exists(rptPath))
            {
                throw new InvalidOperationException(
                    "Crystal Reports is installed, but the .rpt file could not be created at " + rptPath + ".");
            }
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
            dynamic document = Activator.CreateInstance(rcdType);
            try
            {
                document.CreateNew();
                document.DatabaseController.Init();

                var tableType = dataDef.GetType("CrystalDecisions.ReportAppServer.DataDefModel.Table", true);
                dynamic crTable = Activator.CreateInstance(tableType);
                crTable.Name = table.TableName;
                crTable.Alias = table.TableName;
                crTable.QualifiedName = table.TableName;

                var connectionInfoType = dataDef.GetType("CrystalDecisions.ReportAppServer.DataDefModel.ConnectionInfo", true);
                var propertyBagType = dataDef.GetType("CrystalDecisions.ReportAppServer.DataDefModel.PropertyBag", true);
                dynamic connection = Activator.CreateInstance(connectionInfoType);
                dynamic attributes = Activator.CreateInstance(propertyBagType);
                attributes.Add("Database DLL", "crdb_adoplus.dll");
                attributes.Add("QE_DatabaseType", "ADO.NET (XML)");
                connection.Attributes = attributes;
                crTable.ConnectionInfo = connection;

                var crFieldValueType = dataDef.GetType("CrystalDecisions.ReportAppServer.DataDefModel.CrFieldValueTypeEnum", true);
                var dbFieldType = dataDef.GetType("CrystalDecisions.ReportAppServer.DataDefModel.DbField", true);
                foreach (DataColumn column in table.Columns)
                {
                    dynamic field = Activator.CreateInstance(dbFieldType);
                    field.Name = column.ColumnName;
                    field.Description = column.ColumnName;
                    field.Type = MapFieldType(crFieldValueType, column.DataType);
                    crTable.DataFields.Add(field);
                }

                document.DatabaseController.AddTable(crTable, null);

                var textType = reportDef.GetType("CrystalDecisions.ReportAppServer.ReportDefModel.TextObject", true);
                dynamic titleObject = Activator.CreateInstance(textType);
                titleObject.Name = "ReportTitle";
                titleObject.Text = title ?? table.TableName;
                titleObject.Left = 200;
                titleObject.Top = 200;
                titleObject.Width = 10000;
                titleObject.Height = 500;
                document.ReportDefController.ReportObjectController.Add(titleObject, 0, -1);

                var fieldObjectType = reportDef.GetType("CrystalDecisions.ReportAppServer.ReportDefModel.FieldObject", true);
                var left = 200;
                foreach (DataColumn column in table.Columns)
                {
                    dynamic fieldObject = Activator.CreateInstance(fieldObjectType);
                    fieldObject.Name = "fld_" + column.ColumnName;
                    fieldObject.DataSourceName = table.TableName + "." + column.ColumnName;
                    fieldObject.Left = left;
                    fieldObject.Top = 900;
                    fieldObject.Width = 1800;
                    fieldObject.Height = 280;
                    document.ReportDefController.ReportObjectController.Add(fieldObject, 0, -1);
                    left += 1900;
                }

                document.SaveAs(rptPath, 0);
            }
            finally
            {
                try { document.Close(); } catch { }
            }
        }

        private static object MapFieldType(Type crFieldValueType, Type dataType)
        {
            var name = "crStringField";
            if (dataType == typeof(int) || dataType == typeof(long) || dataType == typeof(short))
            {
                name = "crInt64Field";
            }
            else if (dataType == typeof(decimal) || dataType == typeof(double) || dataType == typeof(float))
            {
                name = "crCurrencyField";
            }
            else if (dataType == typeof(DateTime))
            {
                name = "crDateTimeField";
            }

            return Enum.Parse(crFieldValueType, name);
        }
    }
}
