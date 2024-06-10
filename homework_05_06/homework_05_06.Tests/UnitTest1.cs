namespace homework_05_06.Tests;
using System.Xml.Linq;
public class Theory
{
    public int id { get; set; }
    public string question { get; set; }
    public int grade { get; set; }

    public Theory(int id, string question, int grade)
    {
        this.id = id;
        this.question = question;
        this.grade = grade;
    }
}

public class Record
{
    public int licenseId { get; set; }
    public DateOnly examDate { get; set; }
    public int theoryId { get; set; }
    public int practicId { get; set; }

    public Record(int licenseId, DateOnly date, int theoryId, int practicId)
    {
        this.licenseId = licenseId;
        this.examDate = date;
        this.theoryId = theoryId;
        this.practicId = practicId;
    }
}

public class Practic
{
    public int id { get; set; }
    public string autoBrand { get; set; }
    public int grade { get; set; }

    public Practic(int id, string brand, int grade)
    {
        this.id = id;
        this.autoBrand = brand;
        this.grade = grade;
    }
}

public class License
{
    public int id { get; set; }
    public string driverSurname { get; set; }
    public string category { get; set; }
    public DateOnly expirationDate { get; set; }

    public License(int id, string surname, string category, DateOnly date)
    {
        this.id = id;
        this.driverSurname = surname;
        this.category = category;
        this.expirationDate = date;
    }
}
public class MyTestData : IDisposable
{
    public List<Theory> theories { get; set; }
    public List<Record> records { get; set; }
    public List<Practic> practics { get; set; }
    public List<License> licenses { get; set; }


    public MyTestData()
    {
        XDocument RecordDoc = XDocument.Load(@"/Users/marianosa/Desktop/homework_05_06/homework_05_06/LicenseRecord.xml");
        var recordsData = from d in RecordDoc.Descendants("LicenceRecord")
                          select new Record(
                              int.Parse(d.Element("licenseId").Value),
                              DateOnly.Parse(d.Element("examDate").Value),
                              int.Parse(d.Element("theoryId").Value),
                              int.Parse(d.Element("practicId").Value)
                          );
        records = recordsData.ToList();

        XDocument LicenseDoc = XDocument.Load(@"/Users/marianosa/Desktop/homework_05_06/homework_05_06/License.xml");
        var licensesData = from d in LicenseDoc.Descendants("License")
                           select new License(int.Parse(d.Element("id").Value),
                               d.Element("driverSurname").Value,
                               d.Element("category").Value,
                               DateOnly.Parse(d.Element("expirationDate").Value));

        licenses = licensesData.ToList();


        XDocument theoryDoc = XDocument.Load(@"/Users/marianosa/Desktop/homework_05_06/homework_05_06/Theory.xml");
        var theoriesData = from d in theoryDoc.Descendants("Theory")
                           select new Theory(int.Parse(d.Element("id").Value),
                               d.Element("question").Value,
                               int.Parse(d.Element("grade").Value));

        theories = theoriesData.ToList();

        XDocument practicDoc = XDocument.Load(@"/Users/marianosa/Desktop/homework_05_06/homework_05_06/Practic.xml");
        var practicsData = from d in practicDoc.Descendants("Practic")
                           select new Practic(int.Parse(d.Element("id").Value),
                               d.Element("autoBrand").Value,
                               int.Parse(d.Element("grade").Value));

        practics = practicsData.ToList();
    }
    public void Dispose()
    {
        theories.Clear();
        records.Clear();
        practics.Clear();
        licenses.Clear();
    }
}
public class MyTests : IClassFixture<MyTestData>
{
    private MyTestData fixture;

    public MyTests(MyTestData fixture)
    {
        this.fixture = fixture;
    }
    [Fact]
    public void Test1()
    {
        var resultA = from license in fixture.licenses
                      join record in fixture.records on license.id equals record.licenseId
                      orderby license.driverSurname
                      select new
                      {
                          driverSurname = license.driverSurname,
                          category = license.category,
                          termin = license.expirationDate.Year - record.examDate.Year
                      };

        Assert.Equal(resultA.ToList()[0].driverSurname, "Gosling");
        Assert.Equal(resultA.ToList()[0].category, "B");
        Assert.Equal(resultA.ToList()[0].termin, 4);
    }

    [Fact]
    public void Test2()
    {
        var resultB = from license in fixture.licenses
                      join record in fixture.records on license.id equals record.licenseId
                      join theory in fixture.theories on record.theoryId equals theory.id
                      join practic in fixture.practics on record.practicId equals practic.id
                      orderby license.driverSurname
                      select new
                      {
                          driverSurname = license.driverSurname,
                          category = license.category,
                          termin = license.expirationDate.Year - record.examDate.Year,
                          theoryGrade = theory.grade,
                          practicGrade = practic.grade
                      };

        Assert.Equal(resultB.ToList()[0].practicGrade, 65);
        Assert.Equal(resultB.ToList()[0].theoryGrade, 98);

        Assert.Equal(resultB.ToList()[1].practicGrade, 76);
        Assert.Equal(resultB.ToList()[1].theoryGrade, 70);

        Assert.Equal(resultB.ToList()[2].practicGrade, 76);
        Assert.Equal(resultB.ToList()[2].theoryGrade, 70);
    }

    [Fact]
    public void Test3()
    {
        var resultC = from record in fixture.records
                      join licence in fixture.licenses on record.licenseId equals licence.id
                      group new { record, licence } by licence.category
            into g
                      select new
                      {
                          category = g.Key,
                          lics = g.Where(item => item.licence.expirationDate.Year - item.record.examDate.Year <= 2)
                      };

        Assert.Equal(resultC.ToList()[2].lics.ToList()[0].licence.driverSurname, "Urdeichuk");
    }
}