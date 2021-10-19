using System;
using Framework.Core.Persistence;
using Framework.Globalization;
using HR.RequestContext.Domain.EmployeeRequests;
using HR.RequestContext.Domain.EmployeeRequests.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

namespace HR.RequestContext.Domain.Tests
{
   [TestClass]
   public class EmployeeRequestDetailTest
   {
       private Mock<IEntityIdGenerator<EmployeeRequestDetail>> idGenerator;


       [TestInitialize]
       public void Initial()
       {
           idGenerator = new Mock<IEntityIdGenerator<EmployeeRequestDetail>>();
       }


       [TestMethod]
       public void Constructor()
       {
           var requestDetail = new EmployeeRequestDetail(idGenerator.Object, 1, 1010, 990041, "{}", 1, "10:00", "11:00", DateTime.Now.GetPersianDate(), 16);

           Assert.IsNotNull(requestDetail.Id);
           Assert.IsNotNull(requestDetail.PartId);
           Assert.IsNotNull(requestDetail.FromDateTime);
           Assert.IsNotNull(requestDetail.ToDateTime);
           Assert.IsNotNull(requestDetail.StatusId);
           Assert.IsNotNull(requestDetail.Confirm);
       }


        



       [TestMethod]
       [DataRow("10:00", "11:11", 10, 22 , 980060)]
       public void UpdateData_InternalMission_WithDriverEmployeeId(string inTimeByGuard, string outTimeByGuard, long VehicleId, long VehicleTagId, long DriverEmployeeId)
       {
           var data = JObject.Parse("{Purpose: 'test' }");
           var requestDetail = new EmployeeRequestDetail(idGenerator.Object, 2, 1010, 990041, data.ToString(), 1, "10:00", "11:00", DateTime.Now.GetPersianDate(), 16);
           requestDetail.UpdateData(inTimeByGuard, outTimeByGuard, VehicleId, VehicleTagId, DriverEmployeeId);

           var deserialize = JObject.Parse(requestDetail.Data);
           var inTime = deserialize["InTimeByGuard"];
           var outTime = deserialize["OutTimeByGuard"];
           var vehicleId = deserialize["VehicleId"];
           var vehicleTagId = deserialize["VehicleTagId"];
           var driverEmployeeId = deserialize["DriverEmployeeId"];

           Assert.IsNotNull(inTime);
           Assert.IsNotNull(outTime);
           Assert.IsNotNull(vehicleId);
           Assert.IsNotNull(vehicleTagId);
           Assert.IsNotNull(driverEmployeeId);
       }

       [TestMethod]
       [DataRow(null, null, "1000", null)]
       public void UpdateDataInKilometerByGuard_InternalMission_WithOutDetail(string inTime, string outTime, string inKilometerByGuard, string outKilometerByGuard)
       {
           var data = JObject.Parse("{Purpose: 'test' }");
           var requestDetail = new EmployeeRequestDetail(idGenerator.Object, 2, 1010, 990041, data.ToString(), 1, "10:00", "11:00", DateTime.Now.GetPersianDate(), 16);
           requestDetail.UpdateData(inTime, outTime, inKilometerByGuard, outKilometerByGuard);

           var deserialize = JObject.Parse(requestDetail.Data);
           var inTimeByGuard = JsonHelper.IsNullOrEmpty(deserialize["InTimeByGuard"]);
           var outTimeByGuard = JsonHelper.IsNullOrEmpty(deserialize["OutTimeByGuard"]);
           var inKilometer = deserialize["InKilometerByGuard"];
           var outKilometer = JsonHelper.IsNullOrEmpty(deserialize["OutKilometerByGuard"]);

           Assert.AreEqual(true, outTimeByGuard);
           Assert.AreEqual(true, inTimeByGuard);
           Assert.IsNotNull(inKilometer);
           Assert.AreEqual(true, outKilometer);
       }

       [TestMethod]
       [DataRow(null, "10:00", null, null)]
       public void UpdateDataOutTime_InternalMission_WithOutDetail(string inTime, string outTime, string inKilometerByGuard, string outKilometerByGuard)
       {
           var data = JObject.Parse("{Purpose: 'test' }");
           var requestDetail = new EmployeeRequestDetail(idGenerator.Object, 2, 1010, 990041, data.ToString(), 1, "10:00", "11:00", DateTime.Now.GetPersianDate(), 16);
           requestDetail.UpdateData(inTime, outTime, inKilometerByGuard, outKilometerByGuard);

           var deserialize = JObject.Parse(requestDetail.Data);
           var inTimeByGuard = JsonHelper.IsNullOrEmpty(deserialize["InTimeByGuard"]);
           var outTimeByGuard = deserialize["OutTimeByGuard"];
           var inKilometer = JsonHelper.IsNullOrEmpty(deserialize["InKilometerByGuard"]);
           var outKilometer = JsonHelper.IsNullOrEmpty(deserialize["OutKilometerByGuard"]);

           Assert.IsNotNull(outTimeByGuard);
           Assert.AreEqual(true, inTimeByGuard);
           Assert.AreEqual(true, inKilometer);
           Assert.AreEqual(true, outKilometer);
       }

       [TestMethod]
       [DataRow("10:00", null, null, null)]
       public void UpdateDataInTime_InternalMission_WithOutDetail(string inTime, string outTime, string inKilometerByGuard, string outKilometerByGuard)
       {
           var data = JObject.Parse("{Purpose: 'test' }");
           var requestDetail = new EmployeeRequestDetail(idGenerator.Object, 2, 1010, 990041, data.ToString(), 1, "10:00", "11:00", DateTime.Now.GetPersianDate(), 16);
           requestDetail.UpdateData(inTime, outTime, inKilometerByGuard, outKilometerByGuard);

           var deserialize = JObject.Parse(requestDetail.Data);
           var inTimeByGuard = deserialize["InTimeByGuard"];
           var outTimeByGuard = JsonHelper.IsNullOrEmpty(deserialize["OutTimeByGuard"]);
           var inKilometer = JsonHelper.IsNullOrEmpty(deserialize["InKilometerByGuard"]);
           var outKilometer = JsonHelper.IsNullOrEmpty(deserialize["OutKilometerByGuard"]);

           Assert.IsNotNull(inTimeByGuard);
           Assert.AreEqual(true, outTimeByGuard);
           Assert.AreEqual(true, inKilometer);
           Assert.AreEqual(true, outKilometer);
       }


       [TestMethod]
       [DataRow("10:00", "11:11", "100", "150", true)]
       public void UpdateData_InternalMission_WithOutDetail(string inTime, string outTime, string inKilometerByGuard, string outKilometerByGuard, bool isSignWithOutDetail)
       {
           var data = JObject.Parse("{Purpose: 'test' }");
           var requestDetail = new EmployeeRequestDetail(idGenerator.Object, 2, 1010, 990041, data.ToString(), 1, "10:00", "11:00", DateTime.Now.GetPersianDate(), 16);
           requestDetail.UpdateData(inTime, outTime, inKilometerByGuard, outKilometerByGuard, isSignWithOutDetail);

           var deserialize = JObject.Parse(requestDetail.Data);
           var inTimeByGuard = deserialize["InTimeByGuard"];
           var outTimeByGuard = deserialize["OutTimeByGuard"];
           var inKilometer = deserialize["InKilometerByGuard"];
           var outKilometer = deserialize["OutKilometerByGuard"];

           Assert.IsNotNull(inTimeByGuard);
           Assert.IsNotNull(outTimeByGuard);
           Assert.IsNotNull(inKilometer);
           Assert.IsNotNull(outKilometer);
       }



       [TestMethod]
       [DataRow("22:00", "01:25")]
       [DataRow("20:00", "04:30")]
       [DataRow("23:00", "01:00")]
       [DataRow("23:00", "00:25")]
       [DataRow("18:00", "06:25")]
       public void SetDateTimeWhenToDateTowardTomorrow(string fromTime, string toTime)
       {
           var requestDetail = new EmployeeRequestDetail(idGenerator.Object, 1, 1010, 990041, "{}", 1, fromTime, toTime, DateTime.Now.GetPersianDate(), 16);

           Assert.AreEqual(requestDetail.FromDateTime.Value.AddDays(1).Date, requestDetail.ToDateTime.Value.Date);
       }



       [TestMethod]
       [DataRow("10:00", "11:00")]
       public void SetDateTime(string fromTime, string toTime)
       {
           var requestDetail = new EmployeeRequestDetail(idGenerator.Object, 1, 1010, 990041, "{}", 1, fromTime, toTime, DateTime.Now.GetPersianDate(), 16);

           Assert.AreEqual(requestDetail.FromDateTime.Value.Date, requestDetail.ToDateTime.Value.Date);
       }


       [TestMethod]
       public void UpdateDataMultipleWithEmptyTimes()
       {
           var data = JObject.Parse("{}");
           var requestDetail = new EmployeeRequestDetail(idGenerator.Object, 1, 1010, 990041, data.ToString(), 1, "10:00", "11:00", DateTime.Now.GetPersianDate(), 16);

           requestDetail.UpdateData(String.Empty, String.Empty, true);

           var deserialize = JObject.Parse(requestDetail.Data);
           var inTimeByGuard = deserialize["InTimeByGuard"];
           var outTimeByGuard = deserialize["OutTimeByGuard"];

           var inTime1 = JsonHelper.IsNullOrEmpty(inTimeByGuard.ToString());
           var inTime2 = JsonHelper.IsNullOrEmpty2(inTimeByGuard.ToString());
           var outTime1 = JsonHelper.IsNullOrEmpty(outTimeByGuard.ToString());
           var outTime2 = JsonHelper.IsNullOrEmpty2(outTimeByGuard.ToString());

           Assert.AreEqual(true, inTime1);
           Assert.AreEqual(true, inTime2);
           Assert.AreEqual(true, outTime1);
           Assert.AreEqual(true, outTime2);
       }




       [TestMethod]
       [DataRow("10:00", "11:00")]
       public void UpdateDataMultiple_OutTimeByGuard(string inTime, string outTime)
       {
           var data = JObject.Parse("{}");
           var requestDetail = new EmployeeRequestDetail(idGenerator.Object, 1, 1010, 990041, data.ToString(), 1, "10:00", "11:00", DateTime.Now.GetPersianDate(), 16);

           requestDetail.UpdateData(inTime, outTime, true);

           var deserialize = JObject.Parse(requestDetail.Data);
           var inTimeByGuard = deserialize["InTimeByGuard"];
           var outTimeByGuard = deserialize["OutTimeByGuard"];

           Assert.IsNotNull(inTimeByGuard);
           Assert.IsNotNull(outTimeByGuard);
       }

       [TestMethod]
       [DataRow("10:00")]
       public void UpdateDataMultiple_OutTimeByGuard(string outTime)
       {
           var data = JObject.Parse("{}");
           var requestDetail = new EmployeeRequestDetail(idGenerator.Object, 1, 1010, 990041, data.ToString(), 1, "10:00", "11:00", DateTime.Now.GetPersianDate(), 16);

           requestDetail.UpdateData(String.Empty, outTime, true);

           var deserialize = JObject.Parse(requestDetail.Data);
           var inTimeByGuard = deserialize["InTimeByGuard"];
           var outTimeByGuard = deserialize["OutTimeByGuard"];

           Assert.IsNull(inTimeByGuard);
           Assert.IsNotNull(outTimeByGuard);
       }


       [TestMethod]
       [DataRow("10:00")]
       public void UpdateData_OutTimeByGuard(string outTime)
       {
           var data = JObject.Parse("{}");
           var requestDetail = new EmployeeRequestDetail(idGenerator.Object, 1, 1010, 990041, data.ToString(), 1, "10:00", "11:00", DateTime.Now.GetPersianDate(), 16);
           requestDetail.UpdateData(String.Empty, outTime);

           var deserialize = JObject.Parse(requestDetail.Data);
           var inTimeByGuard = deserialize["InTimeByGuard"];
           var outTimeByGuard = deserialize["OutTimeByGuard"];

           Assert.IsNull(inTimeByGuard);
           Assert.IsNotNull(outTimeByGuard);
       }


       [TestMethod]
       [DataRow("10:00", "11:00")]
       public void UpdateDataMultiple_InTimeByGuard(string inTime, string outTime)
       {
           var data = JObject.Parse("{}");
           var requestDetail = new EmployeeRequestDetail(idGenerator.Object, 1, 1010, 990041, data.ToString(), 1, "10:00", "11:00", DateTime.Now.GetPersianDate(), 16);

           requestDetail.UpdateData(inTime, outTime, true);

           var deserialize = JObject.Parse(requestDetail.Data);
           var inTimeByGuard = deserialize["InTimeByGuard"];
           var outTimeByGuard = deserialize["OutTimeByGuard"];

           Assert.IsNotNull(inTimeByGuard);
           Assert.IsNotNull(outTimeByGuard);
       }

       [TestMethod]
       [DataRow("10:00")]
       public void UpdateDataMultiple_InTimeByGuard(string inTime)
       {
           var data = JObject.Parse("{}");
           var requestDetail = new EmployeeRequestDetail(idGenerator.Object, 1, 1010, 990041, data.ToString(), 1, "10:00", "11:00", DateTime.Now.GetPersianDate(), 16);

           requestDetail.UpdateData(inTime, String.Empty, true);

           var deserialize = JObject.Parse(requestDetail.Data);
           var inTimeByGuard = deserialize["InTimeByGuard"];
           var outTimeByGuard = deserialize["OutTimeByGuard"];

           Assert.IsNotNull(inTimeByGuard);
           Assert.IsNull(outTimeByGuard);
       }

       [TestMethod]
       [DataRow("10:00")]
       public void UpdateData_InTimeByGuard(string inTime)
       {
           var data = JObject.Parse("{}");
           var requestDetail = new EmployeeRequestDetail(idGenerator.Object, 1, 1010, 990041, data.ToString(), 1, "10:00", "11:00", DateTime.Now.GetPersianDate(), 16);
           requestDetail.UpdateData(inTime, String.Empty);

           var deserialize = JObject.Parse(requestDetail.Data);
           var inTimeByGuard = deserialize["InTimeByGuard"];
           var outTimeByGuard = deserialize["OutTimeByGuard"];

           Assert.IsNotNull(inTimeByGuard);
           Assert.IsNull(outTimeByGuard);
       }


       //[TestMethod]
       //[ExpectedException(typeof(ToTimeIsLessThanFromTimeException))]
       //[DataRow("22:00", "02:00")]
       //public void SetDateTimeWhenToDateTowardTomorrow_ThrowException(string fromTime, string toTime)
       //{
       //    new EmployeeRequestDetail(idGenerator.Object, 1, 1010, 990041, "{}", 1, fromTime, toTime, DateTime.Now.GetPersianDate(), 16);
       //}


       [TestMethod]
       [ExpectedException(typeof(InvalidDateRangeForEmployeeRequestDetailException))]
       [DataRow("10:00", "11:00", 366)]
       [DataRow("10:00", "11:00", -46)]
       [DataRow("10:00", "11:00", 500)]
       public void CheckDateTimeRange_ThrowException(string fromTime, string toTime, int days)
       {
           new EmployeeRequestDetail(idGenerator.Object, 1, 1010, 990041, "{}", 1, fromTime, toTime, DateTime.Now.AddDays(days).GetPersianDate(), 16);
       }

       [TestMethod]
       [ExpectedException(typeof(EmployeeRequestDetailDataRequiredException))]
       [DataRow(null, null)]
       [DataRow(null, "1")]
       [DataRow(null, "10:05")]
       [DataRow("null", null)]
       [DataRow("25", null)]
       //[DataRow("-10", "-2")]
       public void CheckDateTimeIsNull_ThrowException(string fromTime, string toTime)
       {
           new EmployeeRequestDetail(idGenerator.Object, 1, 1010, 990041, "{}", 1, fromTime, toTime, DateTime.Now.GetPersianDate(), 16);
       }


       [TestMethod]
       [ExpectedException(typeof(EmployeeRequestDetailDataRequiredException))]
       public void CheckPartId_ThrowException()
       {
           new EmployeeRequestDetail(idGenerator.Object, 1, 1010, 990041, "{}", 1, "10:00", "11:00", "1399/06/23", null);
           new EmployeeRequestDetail(idGenerator.Object, 1, 1010, 990041, "{}", 1, "10:00", "11:00", "1399/06/23", 0);
       }
   }
}
