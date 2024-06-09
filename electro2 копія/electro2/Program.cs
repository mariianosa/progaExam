using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using electro2;


namespace Program
{
    public class Program
    {
        public static void Main()
        {
            List<Brygada> brygadas = new List<Brygada>
            {
                new Brygada(1, "Brigade A"),
                new Brygada(2, "Brigade B")

            };
            List<Worker> workers = new List<Worker>
            {
                new Worker(11,"Nosa",01,1),
                new Worker(33,"Chichak",02,1),
                new Worker(44,"Berkela",01,2)
            };

            List<Tariff> tariffs = new List<Tariff>
            {
                new Tariff(01, 100),
                new Tariff(02, 50)

            };

            List<Device> devices = new List<Device>
            {
                new Device(111, "Lala",10),
                new Device(222, "Voda", 2)
            };



                XDocument Naryad1 = XDocument.Load(@"/Users/marianosa/Projects/electro2/electro2/naryad.xml");
                var naryad1Data = from n in Naryad1.Descendants("Naryad")
                                  select new
                                  {
                                      workerId = int.Parse(n.Element("workerId").Value),
                                      hours = int.Parse(n.Element("hours").Value),
                                      deviceId = int.Parse(n.Element("deviceId").Value),
                                      countEkzemp = int.Parse(n.Element("countEkzemp").Value)

                                  };
            

            XDocument Naryad2 = XDocument.Load(@"/Users/marianosa/Projects/electro2/electro2/naryad2.xml");
            var naryad2Data = from n in Naryad2.Descendants("Naryad")
                              select new
                              {
                                  workerId = int.Parse(n.Element("workerId").Value),
                                  hours = int.Parse(n.Element("hours").Value),
                                  deviceId = int.Parse(n.Element("deviceId").Value),
                                  countEkzemp = int.Parse(n.Element("countEkzemp").Value)


                              };

            //хті - файл, де для кожної бригади пораховано сумарну вартість пристроїв, встановлених її пра-цівниками;
            //переліки впорядкувати за назвою бригад у лексико - графічному порядку:
            var brigadeDevicesCosts = from b in brygadas
                                      join w in workers on b.brygadaId equals w.brygadaId
                                      join n1 in naryad1Data on w.workerId equals n1.workerId into n1Group
                                      join n2 in naryad2Data on w.workerId equals n2.workerId into n2Group
                                      let devicesCost1 = n1Group.Sum(n => devices.First(d => d.deviceId == n.deviceId).devicePrice * n.countEkzemp)
                                      let devicesCost2 = n2Group.Sum(n => devices.First(d => d.deviceId == n.deviceId).devicePrice * n.countEkzemp)
                                      orderby b.brygadaName // Впорядкування за назвою бригади
                                      select new
                                      {
                                          Brigade = b,
                                          TotalCost = devicesCost1 + devicesCost2
                                      }
                                      into result
                                      group result by result.Brigade into g
                                      select new
                                      {
                                          Brigade = g.Key,
                                          TotalCost = g.Sum(x => x.TotalCost)
                                      };




            XElement resultXml = new XElement("Brigades",
                from brigadeCost in brigadeDevicesCosts.OrderBy(bc => bc.Brigade.brygadaName)
                select new XElement("Brigade",
                    new XElement("Name", brigadeCost.Brigade.brygadaName),
                    new XElement("TotalCost", brigadeCost.TotalCost)
                )
            );


            resultXml.Save("/Users/marianosa/Projects/electro2/electro2/BrigadeDevicesCosts.xml");



            //(б)хті - файл, де для кожного працівника подано зароблені ним кошти і перелік встановлених пристроїв із вказанням їхньої кількості;
            //впорядкувати за прізвищем у лексико - графічному порядку та за спаданням кількості.
            var workerEarningsAndDevices = from worker in workers
                                           let earnings = (from naryad in naryad1Data.Concat(naryad2Data)
                                                           where naryad.workerId == worker.workerId
                                                           select naryad.hours).Sum() * tariffs.First(t => t.tariffId == worker.tafiffId).hourlyRate
                                           let workerDevices = from naryad in naryad1Data.Concat(naryad2Data)
                                                               where naryad.workerId == worker.workerId
                                                               group naryad by naryad.deviceId into deviceGroup
                                                               select new
                                                               {
                                                                   DeviceId = deviceGroup.Key,
                                                                   Quantity = deviceGroup.Sum(n => n.countEkzemp)
                                                               }
                                           orderby worker.workerSurname ascending, workerDevices.Sum(d => d.Quantity) descending
                                           select new
                                           {
                                               Worker = worker,
                                               Earnings = earnings,
                                               Devices = workerDevices
                                           };


            XElement resultXml2 = new XElement("Workers",
                from workerData in workerEarningsAndDevices
                select new XElement("Worker",
                    new XElement("Surname", workerData.Worker.workerSurname),
                    new XElement("Earnings", workerData.Earnings),
                    new XElement("Devices",
                        from device in workerData.Devices
                        select new XElement("Device",
                            new XAttribute("Id", device.DeviceId),
                            new XAttribute("Quantity", device.Quantity)
                        )
                    )
                )
            );

            resultXml2.Save("/Users/marianosa/Projects/electro2/electro2/WorkerEarnigsAndDevices.xml");

        }
    }
}