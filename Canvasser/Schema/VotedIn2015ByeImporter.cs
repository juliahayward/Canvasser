using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Canvasser.Schema
{
    /// <summary>
    /// Imports a CSV file of the form ER,123,1 containing people who votes in 2015 by-election.
    /// </summary>
    public class VotedIn2015ByeImporter
    {
        private readonly CanvasserDataContext _context;
        private readonly TextBlock _status;

        public VotedIn2015ByeImporter(CanvasserDataContext context, TextBlock status)
        {
            _context = context;
            _status = status;
        }

        public void Import(string path)
        {

            var reader = new ExcelNumberReader();
            var numbers = reader.Read(path);
            foreach (var number in numbers)
            {
                var elector = _context.Electors.FirstOrDefault(x =>
                     (x.PD2015 == number.PD
                     && x.PN2015 == number.PN
                     && x.PNs2015 == number.PNs));

                if (elector == null)
                {
                    _status.Text = "Could not find " + number;
                    continue;
                }

                elector.Voted2015Bye = true;
                _status.Text = number.ToString();
            }
            _context.SubmitChanges();
            _status.Text = "Done";
        }
    }
}
