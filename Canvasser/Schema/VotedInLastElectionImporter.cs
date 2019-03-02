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
    /// Imports a CSV file of the form ER,123,1 containing people who voted in 2015.
    /// </summary>
    public class VotedInLastElectionImporter
    {
        private readonly CanvasserDataContext _context;
        private readonly TextBlock _status;

        public VotedInLastElectionImporter(CanvasserDataContext context, TextBlock status)
        {
            _context = context;
            _status = status;
        }

        public void Import(string path)
        {
            var reader = new ExcelNumberReader();
            var numbers = reader.Read(path);
            int numberNoted = 0;

            foreach (var number in numbers)
            {
                var elector = _context.Electors.FirstOrDefault(x =>
                     (x.PD == number.PD
                     && x.PN == number.PN
                     && x.PNs == number.PNs));

                if (elector == null)
                {
                    _status.Text = "Could not find " + number;
                    continue;
                }

                elector.VotedInLastElection = true;
                _status.Text = number.ToString();
                numberNoted++;
            }

            _context.SubmitChanges();
            _status.Text = string.Format("Done: {0} votes noted", numberNoted);
        }
    }

    /// <summary>
    /// Imports a CSV file of the form ER,123,1 containing people who voted in 2015.
    /// </summary>
    public class PostalInLastElectionImporter
    {
        private readonly CanvasserDataContext _context;
        private readonly TextBlock _status;

        public PostalInLastElectionImporter(CanvasserDataContext context, TextBlock status)
        {
            _context = context;
            _status = status;
        }

        public void Import(string path)
        {
            var reader = new ExcelNumberReader();
            var numbers = reader.Read(path);
            int numberNoted = 0;

            foreach (var number in numbers)
            {
                var elector = _context.Electors.FirstOrDefault(x =>
                     (x.PD == number.PD
                     && x.PN == number.PN
                     && x.PNs == number.PNs));

                if (elector == null)
                {
                    _status.Text = "Could not find " + number;
                    continue;
                }

                elector.PostalInNextElection = true;
                _status.Text = number.ToString();
                numberNoted++;
            }

            _context.SubmitChanges();
            _status.Text = string.Format("Done: {0} votes noted", numberNoted);
        }
    }
}
