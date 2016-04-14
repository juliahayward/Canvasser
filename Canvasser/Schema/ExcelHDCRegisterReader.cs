using Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Canvasser.Schema
{
    public class ExcelHDCRegisterReader
    {
        public class RegisterEntry
        {
            public string PD { get; set; }
            public short PN { get; set; }
            public short PNs { get; set; }
            public string Markers { get; set; }
            public string DoB { get; set; }
            public string Surname { get; set; }
            public string Forename { get; set; }
            public string Postcode { get; set; }
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string Address3 { get; set; }
            public string Address4 { get; set; }
            public string Address5 { get; set; }
            public string Address6 { get; set; }

            public override string ToString()
            {
                return PD + PN + ((PNs > 0) ? "/" + PNs : "");
            }
        }

        public IList<RegisterEntry> Read(string filePath)
        {
            var result = new List<RegisterEntry>();

            FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);

            //Choose one of either 1 or 2
            //1. Reading from a binary Excel file ('97-2003 format; *.xls)
            //IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(stream);

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
                if (string.IsNullOrEmpty(pd)) break;
                if (pd.Length != 2) continue;
                if (string.IsNullOrEmpty(excelReader.GetString(8))) continue; // Address-less people

                var dob = excelReader.GetString(4);
                if (dob != null && dob.Length > 10) dob = dob.Substring(0, 10);    // Excel adds times implicitly

                //if (excelReader.GetString(3) != null && excelReader.GetString(3).Length > 4)
                //    throw new Exception("Markers too long");
                //if (excelReader.GetString(4) != null && excelReader.GetString(4).Length > 16)
                //    throw new Exception("DoB too long");
                //if (excelReader.GetString(5) != null && excelReader.GetString(5).Length > 100)
                //    throw new Exception("Surname too long");
                //if (excelReader.GetString(6) != null && excelReader.GetString(6).Length > 100)
                //    throw new Exception("Forename too long");
                //if (excelReader.GetString(8) != null && excelReader.GetString(8).Length > 100)
                //    throw new Exception("Address1 too long");
                //if (excelReader.GetString(9) != null && excelReader.GetString(9).Length > 100)
                //    throw new Exception("Address2 too long");

                result.Add(new RegisterEntry
                {
                    PD = pd,
                    PN = excelReader.GetInt16(1),
                    PNs = excelReader.GetInt16(2),
                    Markers = excelReader.GetString(3),
                    DoB = dob,
                    Surname = excelReader.GetString(5),
                    Forename = excelReader.GetString(6),
                    Postcode = excelReader.GetString(7),
                    Address1 = excelReader.GetString(8),
                    Address2 = excelReader.GetString(9),
                    Address3 = excelReader.GetString(10),
                    Address4 = excelReader.GetString(11),
                    Address5 = excelReader.GetString(12),
                    Address6 = excelReader.GetString(13)
                });
            }

            //6. Free resources (IExcelDataReader is IDisposable)
            excelReader.Close();

            return result;
        }
    }
}
