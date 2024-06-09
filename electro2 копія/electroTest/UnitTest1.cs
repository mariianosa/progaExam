using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using electro2;
using Xunit;

namespace Program.Tests
{
    public class MyTestData : IDisposable
    {
        public List<Brygada> brygadas { get; private set; }
        public List<Worker> workers { get; private set; }
        public List<Tariff> tariffs { get; private set; }
        public List<Device> devices { get; private set; }
        public IEnumerable<object> naryad1Data { get; private set; }
        public IEnumerable<object> naryad2Data { get; private set; }

        public MyTestData()
        {
            brygadas = new List<Brygada>
            {
                new Brygada(1, "Brigade A"),
                new Brygada(2, "Brigade B")
            };

            workers = new List<Worker>
            {
                new Worker(11, "Nosa", 01, 1),
                new Worker(33, "Chichak", 02, 1),
                new Worker(44, "Berkela", 01, 2)
            };

            tariffs = new List<Tariff>
            {
                new Tariff(01, 100),
                new Tariff(02, 50)
            };

            devices = new List<Device>
            {
                new Device(111, "Lala", 10),
                new Device(222, "Voda", 2)
            };

            XDocument Naryad1 = XDocument.Load(@"/Users/marianosa/Projects/electro2/electro2/naryad.xml");
            naryad1Data = from n in Naryad1.Descendants("Naryad")
                          select new
                          {
                              workerId = int.Parse(n.Element("workerId").Value),
                              hours = int.Parse(n.Element("hours").Value),
                              deviceId = int.Parse(n.Element("deviceId").Value),
                              countEkzemp = int.Parse(n.Element("countEkzemp").Value)
                          };

            XDocument Naryad2 = XDocument.Load(@"/Users/marianosa/Projects/electro2/electro2/naryad2.xml");
            naryad2Data = from n in Naryad2.Descendants("Naryad")
                          select new
                          {
                              workerId = int.Parse(n.Element("workerId").Value),
                              hours = int.Parse(n.Element("hours").Value),
                              deviceId = int.Parse(n.Element("deviceId").Value),
                              countEkzemp = int.Parse(n.Element("countEkzemp").Value)
                          };
        }

        public void Dispose()
        {
            brygadas.Clear();
            workers.Clear();
            tariffs.Clear();
            devices.Clear();
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
        public void TestBrigadeTotalCost()
        {
            var brigadeDevicesCosts = from b in fixture.brygadas
                                      join w in fixture.workers on b.brygadaId equals w.brygadaId
                                      join n1 in fixture.naryad1Data on w.workerId equals ((dynamic)n1).workerId into n1Group
                                      join n2 in fixture.naryad2Data on w.workerId equals ((dynamic)n2).workerId into n2Group
                                      let devicesCost1 = n1Group.Sum(n => fixture.devices.First(d => d.deviceId == ((dynamic)n).deviceId).devicePrice * ((dynamic)n).countEkzemp)
                                      let devicesCost2 = n2Group.Sum(n => fixture.devices.First(d => d.deviceId == ((dynamic)n).deviceId).devicePrice * ((dynamic)n).countEkzemp)
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

            // Assert
            Assert.NotNull(brigadeDevicesCosts);
            Assert.Equal(130, brigadeDevicesCosts.First(b => b.Brigade.brygadaName == "Brigade A").TotalCost);
            Assert.Equal(20, brigadeDevicesCosts.First(b => b.Brigade.brygadaName == "Brigade B").TotalCost);




        }
    }
}
