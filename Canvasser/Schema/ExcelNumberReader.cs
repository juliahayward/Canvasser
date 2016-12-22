using Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Canvasser.Schema
{
    public class ExcelNumberReader
    {
        public class PollingNumber
        {
            public string PD { get; set; }
            public int PN { get; set; }
            public int PNs { get; set; }

            public override string ToString()
            {
                return PD + PN + ((PNs > 0) ? "/" + PNs : "");
            }
        }

        public IList<PollingNumber> Read(string filePath)
        {
            var result = new List<PollingNumber>();

            FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);

            //Choose one of either 1 or 2
            //1. Reading from a binary Excel file ('97-2003 format; *.xls)
            // IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(stream);

            //2. Reading from a OpenXml Excel file (2007 format; *.xlsx)
            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

            //Choose one of either 3, 4, or 5
            //3. DataSet - The result of each spreadsheet will be created in the result.Tables
            // DataSet result = excelReader.AsDataSet();

            //4. DataSet - Create column names from first row
            //excelReader.IsFirstRowAsColumnNames = false;
            // DataSet result = excelReader.AsDataSet();

            //5. Data Reader methods
            while (excelReader.Read())
            {
                var pd = excelReader.GetString(0);
                var pnAndPs = excelReader.GetString(1);
                if (string.IsNullOrEmpty(pd)) break;
                if (pd.Length != 2) continue;
                if (pnAndPs == null) continue;
                if (pnAndPs.Contains("/"))
                {
                    var parts = pnAndPs.Split('/');
                    var pn = int.Parse(parts[0]);
                    var pns = int.Parse(parts[1]);
                    result.Add(new PollingNumber { PD = pd, PN = pn, PNs = pns });
                }
                else
                {
                    var pnOnly = int.Parse(pnAndPs);
                    result.Add(new PollingNumber { PD = pd, PN = pnOnly, PNs = 0 });
                }
            }

            //6. Free resources (IExcelDataReader is IDisposable)
            excelReader.Close();

            return result;
        }
    }
}
