﻿using System;
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
//            xml - файл, де для кожної бригади пораховано зароблені кожним працівником кошти;
//            переліки впорядкувати за назвою бригад і прізвищем працівників у лексико-графічному порядку; при
//цьому назви бригад подавати без повторень;
            var brigadeEarningsByWorker = from brygada in brygadas
                                          from worker in workers
                                          where worker.brygadaId == brygada.brygadaId
                                          let tariff = tariffs.FirstOrDefault(t => t.tariffId == worker.tariffId)
                                          let workerEarnings = (from n in naryad
                                                                where n.workerId == worker.workerId
                                                                select n.hours * (tariff?.hourlyRate ?? 0)).Sum()
                                          group new { Worker = worker, Earnings = workerEarnings } by brygada into grouped
                                          orderby grouped.Key.brygadaName
                                          select new XElement("Brigade",
                                                      new XElement("Name", grouped.Key.brygadaName),
                                                      from workerData in grouped.OrderBy(w => w.Worker.workerSurname)
                                                      select new XElement("Worker",
                                                          new XElement("Surname", workerData.Worker.workerSurname),
                                                          new XElement("Earnings", workerData.Earnings)
                                                      )
                                          );

            // Створення XML-структури та збереження у файл
            XElement resultXml3 = new XElement("Brigades", brigadeEarningsByWorker);
            resultXml3.Save("/Users/marianosa/Projects/electro2/electro2/BrigadeEarningsByWorker.xml");




            //для кожної бригади працівник у якого найбільший заробіток
            var brigadeTopEarners = from brygada in brygadas
                                    join worker in workers on brygada.brygadaId equals worker.brygadaId
                                    let workerNaryad = naryad.Where(n => n.workerId == worker.workerId)
                                    let workerDevices = workerNaryad.Join(devices, n => n.deviceId, d => d.deviceId, (n, d) => new { n.workerId, d.devicePrice, n.countEkzemp })
                                    let workerEarnings = workerNaryad.Sum(n => n.hours * tariffs.FirstOrDefault(t => t.tariffId == worker.tariffId)?.hourlyRate ?? 0)
                                    orderby brygada.brygadaName
                                    group new { Worker = worker, Earnings = workerEarnings } by brygada into grouped
                                    select new
                                    {
                                        BrigadeName = grouped.Key.brygadaName,
                                        TopEarner = grouped.OrderByDescending(g => g.Earnings).FirstOrDefault()?.Worker
                                    };

            XElement resultXml4 = new XElement("Brigades",
                from brigade in brigadeTopEarners
                select new XElement("Brigade",
                    new XElement("Name", brigade.BrigadeName),
                    new XElement("TopEarner",
                        new XElement("Surname", brigade.TopEarner.workerSurname),
                        new XElement("Earnings", brigade.TopEarner == null ? 0 : brigade.TopEarner.Earnings)
                    )
                )
            )

               //для кожної бригади працівник у якого найбільший заробіток(точно правильно)
                //var brigadeTopEarners = from brygada in brygadas
                //                        let brigadeWorkers = workers.Where(w => w.brygadaId == brygada.brygadaId)
                //                        let brigadeNaryad = naryad.Where(n => brigadeWorkers.Any(w => w.workerId == n.workerId))
                //                        let brigadeDevices = brigadeNaryad.Join(devices, n => n.deviceId, d => d.deviceId, (n, d) => new { n.workerId, d.devicePrice, n.countEkzemp })
                //                        let brigadeEarnings = brigadeNaryad.GroupBy(b => b.workerId)
                //                                .Select(g => new { WorkerId = g.Key, Earnings = g.Sum(x => x.hours * tariffs.FirstOrDefault(t => t.tariffId == workers.First(w => w.workerId == g.Key).tariffId)?.hourlyRate ?? 0) })


            //                        let brigadeWorkerEarnings = from worker in brigadeWorkers
            //                                                    join n in brigadeNaryad on worker.workerId equals n.workerId
            //                                                    join tariff in tariffs on worker.tariffId equals tariff.tariffId
            //                                                    let workerEarnings = brigadeEarnings.FirstOrDefault(e => e.WorkerId == worker.workerId)?.Earnings ?? 0
            //                                                    select new { WorkerId = worker.workerId, Surname = worker.workerSurname, Earnings = workerEarnings, HourlyRate = tariff.hourlyRate }
            //                        let topWorker = brigadeWorkerEarnings.OrderByDescending(w => w.Earnings).FirstOrDefault()
            //                        select new
            //                        {
            //                            BrigadeName = brygada.brygadaName,
            //                            TopEarner = new
            //                            {
            //                                Surname = topWorker?.Surname,
            //                                Earnings = topWorker != null ? topWorker.Earnings : 0
            //                            }
            //                        };




            resultXml4.Save("/Users/marianosa/Projects/electro2/electro2/BrigadeTopEarners.xml");

        }


    }
    }
