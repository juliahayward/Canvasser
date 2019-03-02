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
            if (currentVersion < 7) UpgradeTo7();
            if (currentVersion < 8) UpgradeTo8();
            if (currentVersion < 9) UpgradeTo9();
            if (currentVersion < 10) UpgradeTo10();
            if (currentVersion < 11) UpgradeTo11();
            if (currentVersion < 12) UpgradeTo12();
            if (currentVersion < 12) UpgradeTo13();
            if (currentVersion < 14) UpgradeTo14();
            if (currentVersion < 15) UpgradeTo15();
            if (currentVersion < 16) UpgradeTo16();
            if (currentVersion < 17) UpgradeTo17();
            if (currentVersion < 18) UpgradeTo18();
            if (currentVersion < 19) UpgradeTo19();
            if (currentVersion < 20) UpgradeTo20();
            if (currentVersion < 21) UpgradeTo21();
            SetVersion(21);
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

            sql = @"INSERT INTO PollingDistrict VALUES ('ER', 'Jubilee', '(redacted name)', '(redacted address)')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('ES', 'Bushmead', '(redacted name)', '(redacted address)')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('EN', 'Scout Hut', '(redacted name)', '(redacted address)')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('EP', 'Eatons Ctr', '(redacted name)', '(redacted address)')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('EF', 'Methodists', '(redacted name)', '(redacted address)')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('EG', 'Wintringham', '(redacted name)', '(redacted address)')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('EH', 'Ernulf', '(redacted name)', '(redacted address)')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('EJ', 'Almond Road', '(redacted name)', '(redacted address)')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('EL', 'Almond Road', '(redacted name)', '(redacted address)')";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO PollingDistrict VALUES ('ET', 'Loves Farm', '(redacted name)', '(redacted address)')";
            _context.ExecuteCommand(sql);

        }

        private void UpgradeTo7()
        {
            var sql = @"ALTER TABLE TargetVoter ADD Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY";
            _context.ExecuteCommand(sql);
        }

        private void UpgradeTo8()
        {
            foreach (var sql in new [] {
                // Add postcode column - will be filled by 2017 loader
                @"ALTER TABLE Elector ADD Postcode NVarChar(8) NULL",
                 // Remove unwanted old columns
                @"ALTER TABLE Elector DROP COLUMN PD2012",
                @"ALTER TABLE Elector DROP COLUMN PN2012",
                @"ALTER TABLE Elector DROP COLUMN PNs2012",
                @"ALTER TABLE Elector DROP COLUMN PD2013",
                @"ALTER TABLE Elector DROP COLUMN PN2013",
                @"ALTER TABLE Elector DROP COLUMN PNs2013",
                @"ALTER TABLE Elector DROP COLUMN PD2014",
                @"ALTER TABLE Elector DROP COLUMN PN2014",
                @"ALTER TABLE Elector DROP COLUMN PNs2014",
                @"ALTER TABLE Elector ADD PDPrevious NVARCHAR(2)",
                @"UPDATE Elector SET PDPrevious = PD2015",
                @"ALTER TABLE Elector DROP COLUMN PD2015",
                @"ALTER TABLE Elector ADD PNPrevious SMALLINT",
                @"UPDATE Elector SET PNPrevious = PN2015",
                @"ALTER TABLE Elector DROP COLUMN PN2015",
                @"ALTER TABLE Elector ADD PNsPrevious SMALLINT",
                @"UPDATE Elector SET PNsPrevious = PNs2015",
                @"ALTER TABLE Elector DROP COLUMN PNs2015",
                })
            { 
            _context.ExecuteCommand(sql);
            }
        }

        private void UpgradeTo9()
        {
            foreach (var sql in new[] {
                 @"ALTER TABLE PollingDistrict ADD DisplayOrder SMALLINT NULL",

                 @"UPDATE PollingDistrict SET DisplayOrder = 1 WHERE PD = 'ER'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 2 WHERE PD = 'ES'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 3 WHERE PD = 'EN'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 4 WHERE PD = 'EP'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 5 WHERE PD = 'EF'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 6 WHERE PD = 'EG'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 7 WHERE PD = 'EH'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 8 WHERE PD = 'EJ'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 9 WHERE PD = 'EL'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 10 WHERE PD = 'ET'",
            })
            {
                _context.ExecuteCommand(sql);
            }
        }

        // Add the 2017 columns
        private void UpgradeTo10()
        {
            var sql = @"ALTER TABLE Elector ADD Postal2017 BIT";
            _context.ExecuteCommand(sql);
            sql = @"ALTER TABLE Elector ADD Voted2016 BIT";
            _context.ExecuteCommand(sql);
            sql = @"ALTER TABLE Elector ADD Intention2017 NVarChar(16)";
            _context.ExecuteCommand(sql);
            sql = @"UPDATE Elector SET Postal2017 = Postal2016 WHERE Postal2016 IS NOT NULL";
            _context.ExecuteCommand(sql);
            sql = @"UPDATE Elector SET Postal2017 = 0 WHERE Postal2016 IS NULL";
            _context.ExecuteCommand(sql);
            sql = @"UPDATE Elector SET Voted2016 = 0";
            _context.ExecuteCommand(sql);
            sql = @"UPDATE Elector SET Intention2017 = ''";
            _context.ExecuteCommand(sql);
        }

        // Copy across the PNs
        private void UpgradeTo11()
        {
            var sql = @"UPDATE Elector SET PNPrevious = PN, PDPrevious = PD, PNsPrevious = PNs";
            _context.ExecuteCommand(sql);
            sql = @"UPDATE Elector SET PD = '', PN = 0, PNs = 0";
            _context.ExecuteCommand(sql);
        }

        // do 11 right this time
        private void UpgradeTo12()
        {
            var sql = @"UPDATE Elector SET PD = NULL, PN = NULL, PNs = NULL";
            _context.ExecuteCommand(sql);
        }

        // New polling districts for 2018
        private void UpgradeTo13()
        {
            foreach (var sql in new[] {
                 
                @"INSERT INTO PollingDistrict VALUES ('EQ', 'Scout Hut', '(redacted name)', '(redacted address)', 5)",
                 @"UPDATE PollingDistrict SET DisplayOrder = 6 WHERE PD = 'EF'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 7 WHERE PD = 'EG'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 8 WHERE PD = 'EH'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 9 WHERE PD = 'EJ'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 10 WHERE PD = 'EL'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 11 WHERE PD = 'ET'",
            })
            {
                _context.ExecuteCommand(sql);
            }
        }

        private void UpgradeTo14()
        {
            var sql = @"ALTER TABLE Elector ADD Postal2018 BIT";
            _context.ExecuteCommand(sql);
            sql = @"ALTER TABLE Elector ADD Voted2017 BIT";
            _context.ExecuteCommand(sql);
            sql = @"ALTER TABLE Elector ADD Intention2018 NVarChar(16)";
            _context.ExecuteCommand(sql);
            sql = @"UPDATE Elector SET Postal2018 = Postal2017";
            _context.ExecuteCommand(sql);
            sql = @"UPDATE Elector SET Voted2017 = 0";
            _context.ExecuteCommand(sql);
            sql = @"UPDATE Elector SET Intention2018 = ''";
            _context.ExecuteCommand(sql);
        }

        // Copy across the PNs
        private void UpgradeTo15()
        {
            var sql = @"UPDATE Elector SET PNPrevious = PN, PDPrevious = PD, PNsPrevious = PNs";
            _context.ExecuteCommand(sql);
            sql = @"UPDATE Elector SET PD = NULL, PN = NULL, PNs = NULL";
            _context.ExecuteCommand(sql);
        }

        private void UpgradeTo16()
        {
            foreach (var sql in new[] {               
                @"UPDATE Elector SET Voted2016 = 0 WHERE Voted2016 IS NULL",
                @"UPDATE Elector SET Voted2017 = 0 WHERE Voted2017 IS NULL",
                @"UPDATE Elector SET Postal2017 = 0 WHERE Postal2017 IS NULL",
                @"UPDATE Elector SET Postal2018 = 0 WHERE Postal2018 IS NULL",
            })
            {
                _context.ExecuteCommand(sql);
            }
        }

        private void UpgradeTo17()
        {
            foreach (var sql in new[] { 
                 @"INSERT INTO PollingDistrict VALUES ('EQ', 'Scout Hut', '(redacted name)', '(redacted address)', 5)",
                 @"UPDATE PollingDistrict SET DisplayOrder = 6 WHERE PD = 'EF'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 7 WHERE PD = 'EG'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 8 WHERE PD = 'EH'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 9 WHERE PD = 'EJ'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 10 WHERE PD = 'EL'",
                 @"UPDATE PollingDistrict SET DisplayOrder = 11 WHERE PD = 'ET'",
                 @"INSERT INTO PollingDistrict VALUES ('EW', 'Town', '(redacted name)', '(redacted address)', 12)"
            })
            {
                _context.ExecuteCommand(sql);
            }
        }

        private void UpgradeTo18()
        {
            foreach (var sql in new[] {
                 @"INSERT INTO PollingDistrict VALUES ('DF', 'L/Paxton', '(redacted name)', '(redacted address)', 13)",
            })
            {
                _context.ExecuteCommand(sql);
            }
        }

        private void UpgradeTo19()
        {
            foreach (var sql in new[] { 
                 @"UPDATE Elector SET Postal2018 = 0 where Postal2018 IS NULL",
            })
            {
                _context.ExecuteCommand(sql);
            }
        }


        // Once you've done this, import the 2017 data...

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

        private void UpgradeTo20()
        {
            // Add the 2019 columns

            var sql = @"ALTER TABLE Elector ADD Postal2019 BIT";
            _context.ExecuteCommand(sql);
            sql = @"ALTER TABLE Elector ADD Voted2018 BIT";
            _context.ExecuteCommand(sql);
            sql = @"ALTER TABLE Elector ADD Intention2019 NVarChar(16)";
            _context.ExecuteCommand(sql);
            sql = @"UPDATE Elector SET Postal2019 = Postal2018";
            _context.ExecuteCommand(sql);
            // If they had a postal in 2018, assume they used it
            sql = @"UPDATE Elector SET Voted2018 = Postal2018";
            _context.ExecuteCommand(sql);
            sql = @"UPDATE Elector SET Intention2019 = ''";
            _context.ExecuteCommand(sql);
        }

        private void UpgradeTo21()
        {
            // Copy the PNs backwards

            var sql = @"UPDATE Elector SET PNPrevious = PN, PDPrevious = PD, PNsPrevious = PNs";
            _context.ExecuteCommand(sql);
            sql = @"UPDATE Elector SET PD = NULL, PN = NULL, PNs = NULL";
            _context.ExecuteCommand(sql);
        }
    }
}
