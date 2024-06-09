using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
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

            var naryad = naryad1Data.ToList().Concat(naryad2Data.ToList());
            //хml - файл, де для кожної бригади пораховано сумарну вартість пристроїв, встановлених її працівниками;
            //переліки впорядкувати за назвою бригад у лексико - графічному порядку:
            var brigadeDevicesCosts = from brygada in brygadas
                                      join worker in workers on brygada.brygadaId equals worker.brygadaId
                                      join n in naryad on worker.workerId equals n.workerId
                                      join device in devices on n.deviceId equals device.deviceId
                                      group device.devicePrice * n.countEkzemp by brygada into grouped
                                      orderby grouped.Key.brygadaName
                                      select new { totalPrice = grouped.Sum(), brigadeName = grouped.Key.brygadaName };



            XElement resultXml = new XElement("Brigades",
                from brigadeCost in brigadeDevicesCosts
                select new XElement("Brigade",
                    new XElement("Name", brigadeCost.brigadeName),
                    new XElement("TotalCost", brigadeCost.totalPrice)
                )
            );


            resultXml.Save("/Users/marianosa/Projects/electro2/electro2/BrigadeDevicesCosts.xml");



            //(б)хml - файл, де для кожного працівника подано зароблені ним кошти і перелік встановлених пристроїв із вказанням їхньої кількості;
            //впорядкувати за прізвищем у лексико - графічному порядку та за спаданням кількості.
            var workerEarningsAndDevices = from worker in workers
                                           join n in naryad on worker.workerId equals n.workerId
                                           join device in devices on n.deviceId equals device.deviceId
                                           group new { n, device } by worker into workerGroup
                                           let tariff = tariffs.FirstOrDefault(t => t.tariffId == workerGroup.Key.tariffId)
                                           let earnings = workerGroup.Sum(g => g.n.hours * tariff.hourlyRate)
                                           let workerDevices = workerGroup.GroupBy(g => g.device)
                                                                          .Select(g => new
                                                                          {
                                                                              DeviceId = g.Key.deviceId,
                                                                              DeviceName = g.Key.deviceName,
                                                                              Quantity = g.Sum(x => x.n.countEkzemp)
                                                                          })
                                           orderby workerGroup.Key.workerSurname ascending, workerDevices.Sum(d => d.Quantity) descending
                                           select new
                                           {
                                               Worker = workerGroup.Key,
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
                            new XAttribute("Name", device.DeviceName),
                            new XAttribute("Quantity", device.Quantity)
                        )
                    )
                )
            );




            resultXml2.Save("/Users/marianosa/Projects/electro2/electro2/WorkerEarnigsAndDevices.xml");

        }
    }
}