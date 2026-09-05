using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace business_managment_system.Services
{
    /// <summary>
    /// Builds a simple landscape PDF table without Crystal Reports or extra packages.
    /// </summary>
    public static class TablePdfWriter
    {
        private const float PageWidth = 842f;
        private const float PageHeight = 595f;
        private const float Margin = 32f;
        private const float TitleSize = 16f;
        private const float HeaderSize = 8f;
        private const float CellSize = 8f;
        private const float LineHeight = 11f;
        private const float HeaderBar = 16f;

        public static byte[] FromTable(DataTable table, string title)
        {
            if (table == null || table.Rows.Count == 0)
            {
                throw new InvalidOperationException("There is no data for this report.");
            }

            var usable = PageWidth - (Margin * 2);
            var widths = ColumnWidths(table, usable);
            var headers = new string[table.Columns.Count];
            for (var i = 0; i < table.Columns.Count; i++)
            {
                headers[i] = Label(table.Columns[i].ColumnName);
            }

            var pages = new List<StringBuilder>();
            var page = NewPage(pages, title, headers, widths);
            var y = PageHeight - Margin - 62f;

            foreach (DataRow row in table.Rows)
            {
                var cells = new string[table.Columns.Count];
                var wraps = new List<string>[table.Columns.Count];
                var rowLines = 1;
                for (var i = 0; i < table.Columns.Count; i++)
                {
                    cells[i] = Format(row[i], table.Columns[i].DataType);
                    wraps[i] = Wrap(cells[i], widths[i] - 6f, CellSize);
                    if (wraps[i].Count > rowLines)
                    {
                        rowLines = wraps[i].Count;
                    }
                }

                var rowHeight = Math.Max(LineHeight, rowLines * LineHeight) + 4f;
                if (y - rowHeight < Margin + 22f)
                {
                    page = NewPage(pages, title, headers, widths);
                    y = PageHeight - Margin - 62f;
                }

                var x = Margin;
                DrawRule(page, Margin, y, usable, 0.85f, 0.85f, 0.85f);
                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var alignRight = IsNumeric(table.Columns[i].DataType);
                    var textY = y - 10f;
                    foreach (var line in wraps[i])
                    {
                        DrawText(page, line, x + 3f, textY, CellSize, false, widths[i] - 6f, alignRight);
                        textY -= LineHeight;
                    }

                    x += widths[i];
                }

                y -= rowHeight;
            }

            for (var i = 0; i < pages.Count; i++)
            {
                DrawText(
                    pages[i],
                    "Page " + (i + 1) + " of " + pages.Count,
                    Margin,
                    Margin - 6f,
                    8f,
                    false,
                    usable,
                    true);
            }

            return Assemble(pages);
        }

        private static StringBuilder NewPage(List<StringBuilder> pages, string title, string[] headers, float[] widths)
        {
            var page = new StringBuilder();
            DrawText(page, "SkySoft Business Management", Margin, PageHeight - Margin, 9f, false, PageWidth - (Margin * 2), false);
            DrawText(page, DateTime.Now.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture), Margin, PageHeight - Margin, 9f, false, PageWidth - (Margin * 2), true);
            DrawText(page, title ?? "Report", Margin, PageHeight - Margin - 20f, TitleSize, true, PageWidth - (Margin * 2), false);

            var headerTop = PageHeight - Margin - 40f;
            FillRect(page, Margin, headerTop - HeaderBar, PageWidth - (Margin * 2), HeaderBar, 0.06f, 0.09f, 0.16f);
            var x = Margin;
            for (var i = 0; i < headers.Length; i++)
            {
                DrawText(page, headers[i], x + 3f, headerTop - 12f, HeaderSize, true, widths[i] - 6f, false, 1f, 1f, 1f);
                x += widths[i];
            }

            pages.Add(page);
            return page;
        }

        private static float[] ColumnWidths(DataTable table, float usable)
        {
            var weights = new float[table.Columns.Count];
            for (var i = 0; i < table.Columns.Count; i++)
            {
                var name = table.Columns[i].ColumnName;
                var type = table.Columns[i].DataType;
                if (name.IndexOf("Description", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    weights[i] = 2.4f;
                }
                else if (name.IndexOf("Name", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Recorded", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    weights[i] = 1.7f;
                }
                else if (IsNumeric(type) || type == typeof(DateTime))
                {
                    weights[i] = 0.9f;
                }
                else
                {
                    weights[i] = 1.2f;
                }
            }

            var total = 0f;
            foreach (var weight in weights)
            {
                total += weight;
            }

            var widths = new float[weights.Length];
            for (var i = 0; i < weights.Length; i++)
            {
                widths[i] = usable * (weights[i] / total);
            }

            return widths;
        }

        private static bool IsNumeric(Type type)
        {
            return type == typeof(decimal)
                || type == typeof(int)
                || type == typeof(long)
                || type == typeof(double)
                || type == typeof(float)
                || type == typeof(short);
        }

        private static string Format(object value, Type type)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            if (type == typeof(DateTime) || value is DateTime)
            {
                return ((DateTime)value).ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
            }

            if (type == typeof(decimal) || value is decimal)
            {
                return ((decimal)value).ToString("N2", CultureInfo.InvariantCulture);
            }

            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static string Label(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                return string.Empty;
            }

            return Regex.Replace(columnName, "([a-z])([A-Z0-9])", "$1 $2");
        }

        private static List<string> Wrap(string text, float maxWidth, float fontSize)
        {
            var lines = new List<string>();
            if (string.IsNullOrEmpty(text))
            {
                lines.Add(string.Empty);
                return lines;
            }

            var words = text.Replace("\r", " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var current = string.Empty;
            foreach (var word in words)
            {
                var candidate = current.Length == 0 ? word : current + " " + word;
                if (TextWidth(candidate, fontSize) <= maxWidth)
                {
                    current = candidate;
                    continue;
                }

                if (current.Length > 0)
                {
                    lines.Add(current);
                    current = word;
                }

                while (TextWidth(current, fontSize) > maxWidth && current.Length > 1)
                {
                    var fit = Fit(current, maxWidth, fontSize);
                    lines.Add(fit);
                    current = current.Substring(fit.Length);
                }
            }

            if (current.Length > 0)
            {
                lines.Add(current);
            }

            if (lines.Count == 0)
            {
                lines.Add(string.Empty);
            }

            return lines;
        }

        private static string Fit(string text, float maxWidth, float fontSize)
        {
            var length = text.Length;
            while (length > 1 && TextWidth(text.Substring(0, length), fontSize) > maxWidth)
            {
                length--;
            }

            return text.Substring(0, length);
        }

        private static float TextWidth(string text, float fontSize)
        {
            return (text ?? string.Empty).Length * fontSize * 0.5f;
        }

        private static void DrawText(
            StringBuilder page,
            string text,
            float x,
            float y,
            float size,
            bool bold,
            float boxWidth,
            bool alignRight,
            float r = 0.12f,
            float g = 0.16f,
            float b = 0.22f)
        {
            var safe = text ?? string.Empty;
            while (safe.Length > 0 && TextWidth(safe, size) > boxWidth)
            {
                safe = safe.Substring(0, safe.Length - 1);
            }

            var drawX = alignRight ? x + boxWidth - TextWidth(safe, size) : x;
            page.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0:0.###} {1:0.###} {2:0.###} rg\nBT /{3} {4:0.###} Tf {5:0.###} {6:0.###} Td ({7}) Tj ET\n",
                r,
                g,
                b,
                bold ? "F2" : "F1",
                size,
                drawX,
                y,
                Escape(safe));
        }

        private static void FillRect(StringBuilder page, float x, float y, float w, float h, float r, float g, float b)
        {
            page.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0:0.###} {1:0.###} {2:0.###} rg {3:0.###} {4:0.###} {5:0.###} {6:0.###} re f\n",
                r,
                g,
                b,
                x,
                y,
                w,
                h);
        }

        private static void DrawRule(StringBuilder page, float x, float y, float w, float r, float g, float b)
        {
            page.AppendFormat(
                CultureInfo.InvariantCulture,
                "q {0:0.###} {1:0.###} {2:0.###} RG 0.4 w {3:0.###} {4:0.###} m {5:0.###} {4:0.###} l S Q\n",
                r,
                g,
                b,
                x,
                y,
                x + w);
        }

        private static string Escape(string text)
        {
            return (text ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)");
        }

        private static byte[] Assemble(IList<StringBuilder> pages)
        {
            using (var output = new MemoryStream())
            {
                var offsets = new List<long> { 0 };
                Write(output, "%PDF-1.4\n");

                WriteObject(output, offsets, "<< /Type /Catalog /Pages 2 0 R >>");

                var kids = new StringBuilder("[");
                var pageObjectId = 5;
                for (var i = 0; i < pages.Count; i++)
                {
                    if (i > 0)
                    {
                        kids.Append(" ");
                    }

                    kids.Append(pageObjectId);
                    kids.Append(" 0 R");
                    pageObjectId += 2;
                }

                kids.Append("]");
                WriteObject(
                    output,
                    offsets,
                    "<< /Type /Pages /Kids " + kids + " /Count " + pages.Count + " >>");
                WriteObject(output, offsets, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");
                WriteObject(output, offsets, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>");

                var winAnsi = Encoding.GetEncoding(1252);
                for (var i = 0; i < pages.Count; i++)
                {
                    var content = winAnsi.GetBytes(pages[i].ToString());
                    var contentId = 6 + (i * 2);
                    WriteObject(
                        output,
                        offsets,
                        "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 "
                        + PageWidth.ToString("0", CultureInfo.InvariantCulture)
                        + " "
                        + PageHeight.ToString("0", CultureInfo.InvariantCulture)
                        + "] /Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents "
                        + contentId
                        + " 0 R >>");

                    offsets.Add(output.Position);
                    Write(output, contentId + " 0 obj\n<< /Length " + content.Length + " >>\nstream\n");
                    output.Write(content, 0, content.Length);
                    Write(output, "\nendstream\nendobj\n");
                }

                var xref = output.Position;
                Write(output, "xref\n0 " + offsets.Count + "\n");
                Write(output, "0000000000 65535 f \n");
                for (var i = 1; i < offsets.Count; i++)
                {
                    Write(output, offsets[i].ToString("0000000000", CultureInfo.InvariantCulture) + " 00000 n \n");
                }

                Write(
                    output,
                    "trailer\n<< /Size " + offsets.Count + " /Root 1 0 R >>\nstartxref\n" + xref + "\n%%EOF\n");
                return output.ToArray();
            }
        }

        private static void WriteObject(MemoryStream output, List<long> offsets, string body)
        {
            offsets.Add(output.Position);
            Write(output, (offsets.Count - 1) + " 0 obj\n" + body + "\nendobj\n");
        }

        private static void Write(MemoryStream output, string text)
        {
            var bytes = Encoding.ASCII.GetBytes(text);
            output.Write(bytes, 0, bytes.Length);
        }
    }
}
