using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Canvasser
{
    public class ImprintProvider
    {
        CanvasserDataContext _context;

        public ImprintProvider(CanvasserDataContext context)
        {
            _context = context;
        }

        public string Provide(string targetPD)
        {
            var pd = _context.PollingDistricts.First(x => x.PD == targetPD);

            return string.Format("Published and promoted by {0} at {1}", pd.ImprintName, pd.ImprintAddress);
        }
    }
}
