using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Canvasser.Schema
{
    /// <summary>
    /// Imports a CSV register file as supplied by HDC
    /// </summary>
    public class HDC2016RegisterImporter
    {
        private readonly CanvasserDataContext _context;
        private readonly TextBlock _status;

        public HDC2016RegisterImporter(CanvasserDataContext context, TextBlock status)
        {
            _context = context;
            _status = status;
        }

        public void Import(string path)
        {
            int newEntry = 0, existingEntry = 0;
            var reader = new ExcelHDCRegisterReader();
            var entries = reader.Read(path);
            foreach (var entry in entries)
            {
                var elector = _context.Electors.FirstOrDefault(x =>
                     (x.FirstName.Equals(entry.Forename)
                     && x.Surname.Equals(entry.Surname)
                     && x.Address.Equals(entry.Address1)));

                if (elector != null)
                {
                    elector.PD = entry.PD;
                    elector.PN = entry.PN;
                    elector.PNs = entry.PNs;
                    elector.Date = "";
                    elector.Markers = entry.Markers;
                    elector.FirstName = entry.Forename;
                    elector.Address2 = entry.Address2;
                    elector.Postcode = entry.Postcode;
                    elector.Surname = entry.Surname; // Hack for 2016 - take trimmed camel case from register as it was uppercase last time
                    existingEntry++;
                }
                else
                {
                    elector = new Elector()
                    {
                        PD = entry.PD,
                        PN = entry.PN,
                        PNs = entry.PNs,
                        FirstName = entry.Forename,
                        Surname = entry.Surname,
                        Address = entry.Address1,
                        Address2 = entry.Address2,
                        Postcode = entry.Postcode,
                        Markers = entry.Markers,
                        Date = entry.DoB,
                        Intention2012 = "",
                        Intention2013 = "",
                        Intention2014 = "",
                        Intention2015 = "", 
                        Intention2016 = "",
                        Intention2017 = "",
                        Intention2018 = "",
                        Intention2019 = "",
                        Postal2012 = false,
                        Postal2013 = false,
                        Postal2014 = false,
                        Postal2015 = false,
                        Postal2016 = false,
                        Postal2017 = false,
                        Postal2018 = false,
                        Postal2019 = false,
                        Voted2012 = false,
                        Voted2013 = false,
                        Voted2014 = false,
                        Voted2015 = false,
                        Voted2015Bye = false,
                        Voted2016 = false,
                        Voted2017 = false,
                        Voted2018 = false
                    };
                    _context.Electors.InsertOnSubmit(elector);
                    newEntry++;
                }

                _status.Dispatcher.Invoke(new Action(() => _status.Text = entry.ToString()));
                // Try to do all in one go and you'll get Out of Memory
                _context.SubmitChanges();
            }
            _status.Text = string.Format("Done: {0} existing voters updated, {1} new added", existingEntry, newEntry);
        }
    }
}

                       
