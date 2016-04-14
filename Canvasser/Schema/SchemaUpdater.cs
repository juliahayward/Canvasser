using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Canvasser.Schema
{
    public class SchemaUpdater
    {
        CanvasserDataContext _context;

        public SchemaUpdater(CanvasserDataContext context)
        {
            _context = context;
        }

        public void Update()
        {
            int currentVersion = GetVersion();
            if (currentVersion < 1) UpgradeTo1();
            if (currentVersion < 2) UpgradeTo2();
            if (currentVersion < 3) UpgradeTo3();
            if (currentVersion < 4) UpgradeTo4();
            if (currentVersion < 5) UpgradeTo5();
            if (currentVersion < 6) UpgradeTo6();
            SetVersion(6);
        }

        public int GetVersion()
        {
            var sql = @"SELECT Version FROM SchemaVersion";
            try
            {
                return (int)(_context.ExecuteQuery<int>(sql).First());
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        private void SetVersion(int version)
        {
            var sql = @"UPDATE SchemaVersion SET Version = " + version;
            _context.ExecuteCommand(sql);
        }

        private void UpgradeTo1()
        {
            AddPhoneFieldIfNotPresent();
            var sql = @"CREATE TABLE SchemaVersion (Version INT)";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO SchemaVersion VALUES (1)";
            _context.ExecuteCommand(sql);
        }

        private void UpgradeTo2()
        {
            var sql = @"ALTER TABLE Elector ADD PD NVARCHAR(2)";
            _context.ExecuteCommand(sql);
            sql = @"ALTER TABLE Elector ADD PN SMALLINT";
            _context.ExecuteCommand(sql);
            sql = @"ALTER TABLE Elector ADD PNs SMALLINT";
            _context.ExecuteCommand(sql);
        }

        private void UpgradeTo3()
        {
            var sql = @"ALTER TABLE Elector ADD Postal2016 BIT";
            _context.ExecuteCommand(sql);
            sql = @"ALTER TABLE Elector ADD Voted2015 BIT";
            _context.ExecuteCommand(sql);
            _context.ExecuteCommand(@"UPDATE Elector SET Voted2015 = 1");
            sql = @"ALTER TABLE Elector ADD Voted2015Bye BIT";
            _context.ExecuteCommand(sql);
            _context.ExecuteCommand(@"UPDATE Elector SET Voted2015Bye = 0");
            sql = @"UPDATE Elector SET Postal2016 = Postal2015";
            _context.ExecuteCommand(sql);
            sql = @"ALTER TABLE Elector ADD Intention2016 NVarChar(16)";
            _context.ExecuteCommand(sql); 
        }

        private void UpgradeTo4()
        {
            var sql = @"CREATE TABLE TargetVoter (
PD NVARCHAR(2) NOT NULL,
PN SMALLINT NOT NULL,
PNs SMALLINT NOT NULL,
FIRSTNAME NVARCHAR(100) NOT NULL,
Surname NVARCHAR(100) NOT NULL,
Address NVARCHAR(100) NOT NULL,
Address2 NVARCHAR(100) NOT NULL,
Telephone NVARCHAR(12), 
Voted BIT NOT NULL,
Notes NVARCHAR(100)
)";
            _context.ExecuteCommand(sql);
        }

        private void UpgradeTo5()
        {
            var sql = @"UPDATE Elector SET Voted2015 = 0 WHERE PD2015 NOT IN ('EN','EP','ER','ES')";
            _context.ExecuteCommand(sql);
        }

        private void UpgradeTo6()
        {
            var sql = @"CREATE TABLE PollingDistrict (
PD NVARCHAR(2) NOT NULL,
ShortName NVARCHAR(16) NOT NULL,
ImprintName NVARCHAR(100) NOT NULL,
ImprintAddress NVARCHAR(100) NOT NULL
)";
            _context.ExecuteCommand(sql);

            sql = @"INSERT INTO PollingDistrict VALUES ('ER', 'Jubilee', 'Derek Giles', '6 Stratford Place, Eaton Socon PE19 8HY')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('ES', 'Bushmead', 'Derek Giles', '6 Stratford Place, Eaton Socon PE19 8HY')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('EN', 'Scout Hut', 'Derek Giles', '6 Stratford Place, Eaton Socon PE19 8HY')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('EP', 'Eatons Ctr', 'Derek Giles', '6 Stratford Place, Eaton Socon PE19 8HY')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('EF', 'Methodists', 'Derek Giles', '6 Stratford Place, Eaton Socon PE19 8HY')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('EG', 'Wintringham', 'Derek Giles', '6 Stratford Place, Eaton Socon PE19 8HY')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('EH', 'Ernulf', 'Derek Giles', '6 Stratford Place, Eaton Socon PE19 8HY')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('EJ', 'Almond Road', 'Derek Giles', '6 Stratford Place, Eaton Socon PE19 8HY')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('EL', 'Almond Road', 'Derek Giles', '6 Stratford Place, Eaton Socon PE19 8HY')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('ET', 'Loves Farm', 'Derek Giles', '6 Stratford Place, Eaton Socon PE19 8HY')";
            _context.ExecuteCommand(sql);

        }

        private void AddPhoneFieldIfNotPresent()
        {
            var sql = @"ALTER TABLE Elector ADD Telephone NVARCHAR(12)";
            try
            {
                _context.ExecuteCommand(sql);

                foreach (var e in _context.Electors.Where(x => x.Notes != null).ToList())
                {
                    int phone;
                    if (Int32.TryParse(e.Notes, out phone))
                    {
                        e.Telephone = e.Notes;
                        e.Notes = "";
                    }
                }
                _context.SubmitChanges();
            }
            catch (Exception) { /* Discard once done on Derek's box*/ }
        }
    }
}
