using System;
using System.Collections.Generic;
namespace electro2
{
        public class Brygada
        {
            public int brygadaId { get; set; }
            public string brygadaName { get; set; }

            public Brygada(int brygadaId, string brygadaName)
            {
                this.brygadaId = brygadaId;
                this.brygadaName = brygadaName;
            }


        }
        public class Worker
        {
            public int workerId { get; set; }
            public string workerSurname { get; set; }
            public int tariffId { get; set; }
            public int brygadaId { get; set; }

            public Worker(int workerId, string workerSurname, int tariffId, int brygadaId)
            {
                this.workerId = workerId;
                this.workerSurname = workerSurname;
                this.tariffId = tariffId;
                this.brygadaId = brygadaId;
            }


        }
        public class Tariff
        {
            public int tariffId { get; set; }
            public int hourlyRate { get; set; }

            public Tariff(int tariffId, int hourlyRate)
            {
                this.tariffId = tariffId;
                this.hourlyRate = hourlyRate;
            }


        }
        public class Device
        {
            public int deviceId { get; set; }
            public string deviceName { get; set; }
            public int devicePrice { get; set; }

            public Device(int deviceId, string deviceName, int devicePrice)
            {
                this.deviceId = deviceId;
                this.deviceName = deviceName;
                this.devicePrice = devicePrice;

            }
        }
    }
