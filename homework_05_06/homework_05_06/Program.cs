using System.Linq;
using System.Xml.Linq;
namespace Program
{
    public class Program
    {
        public static void Main()
        {
            // Завантаження XML документа LicenseRecord.xml
            XDocument RecordDoc = XDocument.Load(@"/Users/marianosa/Desktop/homework_05_06/homework_05_06/LicenseRecord.xml");
            var records = from d in RecordDoc.Descendants("LicenceRecord")
                          select new
                          {
                              licenseId = int.Parse(d.Element("licenseId").Value),
                              examDate = DateOnly.Parse(d.Element("examDate").Value),
                              theoryId = int.Parse(d.Element("theoryId").Value),
                              practicId = int.Parse(d.Element("practicId").Value)
                          };
            // Завантаження XML документа License.xml
            XDocument LicenseDoc = XDocument.Load(@"/Users/marianosa/Desktop/homework_05_06/homework_05_06/License.xml");
            var licenses = from d in LicenseDoc.Descendants("License")
                           select new
                           {
                               id = int.Parse(d.Element("id").Value),
                               driverSurname = d.Element("driverSurname").Value,
                               category = d.Element("category").Value,
                               expirationDate = DateOnly.Parse(d.Element("expirationDate").Value)
                           };
            // Завантаження XML документа Theory.xml
            XDocument theoryDoc = XDocument.Load(@"/Users/marianosa/Desktop/homework_05_06/homework_05_06/Theory.xml");
            var theories = from d in theoryDoc.Descendants("Theory")
                           select new
                           {
                               id = int.Parse(d.Element("id").Value),
                               question = d.Element("question").Value,
                               grade = int.Parse(d.Element("grade").Value),
                           };

            // Завантаження XML документа Practic.xml
            XDocument practicDoc = XDocument.Load(@"/Users/marianosa/Desktop/homework_05_06/homework_05_06/Practic.xml");
            var practics = from d in practicDoc.Descendants("Practic")
                           select new
                           {
                               id = int.Parse(d.Element("id").Value),
                               autoBrand = d.Element("autoBrand").Value,
                               grade = int.Parse(d.Element("grade").Value),
                           };


            // Запит A: отримання списку ліцензій та відповідних даних про водія, упорядкованих за прізвищем
            // Початок запиту LINQ: джерело даних - колекція licenses
            var resultA = from license in licenses
                              // Об'єднання з колекцією records за допомогою license.id і record.licenseId
                          join record in records on license.id equals record.licenseId
                          // Сортування результатів за прізвищем водія (license.driverSurname)
                          orderby license.driverSurname
                          // Вибірка нових анонімних об'єктів, що містять наступні поля:
                          select new
                          {
                              driverSurname = license.driverSurname,
                              category = license.category,
                              termin = license.expirationDate.Year - record.examDate.Year
                          };
            // Створення кореневого елементу XML з назвою "Results"
            XElement resultAXml = new XElement("Results",
                from r in resultA
                    // Створення нового елементу "Result" для кожного елементу resultA
                select new XElement("Result",
                    // Додавання піделементу "driverSurname" з прізвищем водія
                    new XElement("driverSurname", r.driverSurname),
                    new XElement("category", r.category),
                    new XElement("termin", r.termin)
                )
            );
            // Збереження результату A в XML файл
            // resultAXml.Save(@"/Users/danyloyakymets/Projects/dotnet/homework_05_06/homework_05_06/ResultA.xml");

            // Запит B: отримання оцінок теоретичного та практичного іспитів для кожної ліцензії, упорядкованих за прізвищем
            var resultB = from license in licenses // Вибірка ліцензій з колекції licenses
                                                   // Об'єднання з колекцією records, де ідентифікатор ліцензії співпадає з ідентифікатором у записі
                          join record in records on license.id equals record.licenseId
                          // Об'єднання з колекцією theories, де ідентифікатор теоретичного іспиту співпадає з ідентифікатором у записі 
                          join theory in theories on record.theoryId equals theory.id
                          // Об'єднання з колекцією practics, де ідентифікатор практичного іспиту співпадає з ідентифікатором у записі
                          join practic in practics on record.practicId equals practic.id
                          // Сортування результатів за прізвищем водія
                          orderby license.driverSurname
                          select new // Створення нового анонімного об'єкту для кожного результату запиту
                          {
                              driverSurname = license.driverSurname, // Прізвище водія з ліцензії
                              category = license.category, // Категорія ліцензії
                                                           // Різниця між роком закінчення терміну дії ліцензії та роком складання іспиту
                              termin = license.expirationDate.Year - record.examDate.Year,
                              theoryGrade = theory.grade, // Оцінка з теоретичного іспиту
                              practicGrade = practic.grade // Оцінка з практичного іспиту
                          };

            // Створення XML елементів з результатами запиту B
            XElement resultBXml = new XElement("Results", // Створення кореневого елементу XML з назвою "Results"
                from r in resultB // Проходження по кожному елементу результату запиту B
                select new XElement("Result", // Створення нового елементу "Result" для кожного результату
                    new XElement("driverSurname", r.driverSurname), // Додавання елементу з прізвищем водія
                    new XElement("category", r.category), // Додавання елементу з категорією ліцензії
                    new XElement("termin", r.termin), // Додавання елементу з терміном
                    new XElement("theoryGrade", r.theoryGrade), // Додавання елементу з оцінкою з теоретичного іспиту
                    new XElement("practicGrade", r.practicGrade) // Додавання елементу з оцінкою з практичного іспиту
                )
            );

            // resultBXml.Save(@"/Users/danyloyakymets/Projects/dotnet/homework_05_06/homework_05_06/ResultB.xml");


            // Запит C: отримання ліцензій, термін дії яких не перевищує 2 роки, згрупованих за категорією
            var resultC = from record in records
                          join licence in licenses on record.licenseId equals licence.id
                          group new { record, licence } by licence.category
                into g
                          select new
                          {
                              category = g.Key,
                              lics = g.Where(item => item.licence.expirationDate.Year - item.record.examDate.Year <= 2)
                          };

            XElement resultCXml = new XElement("Results",
                from r in resultC
                select new XElement("Result",
                    new XElement("category", r.category),
                    new XElement("licenses",
                        from i in r.lics
                        select new XElement("license",
                            new XElement("driverSurname", i.licence.driverSurname)
                            )
                        )
                    )
                );
            // resultCXml.Save(@"/Users/danyloyakymets/Projects/dotnet/homework_05_06/homework_05_06/ResultC.xml");


            // Запит D: отримання мінімальних оцінок з теорії для кожної категорії
            var resultD = from record in records
                          join license in licenses on record.licenseId equals license.id
                          join theory in theories on record.theoryId equals theory.id
                          orderby theory.grade
                          group new { record, license, theory } by license.category
                into g

                          select new
                          {
                              category = g.Key,
                              minimumGrade = g.First(item => item.theory.grade > 0)
                          };
            // Виведення результату D на консоль
            foreach (var item in resultD)
            {
                Console.WriteLine($"{item.category}, {item.minimumGrade.theory.question}");
            }

            XElement resultDXml = new XElement("Results",
                from r in resultD
                select new XElement("Result",
                    new XElement("category", r.category),
                    new XElement("minimumTheoryQuestion", r.minimumGrade.theory.question),
                    new XElement("minimumGrade", r.minimumGrade.theory.grade)
                )
            );
            resultDXml.Save(@"/Users/marianosa/Desktop/homework_05_06/homework_05_06/ResultD.xml");
        }


    }
}