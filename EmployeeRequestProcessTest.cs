using System;
using System.Text.Json;
using Framework.Core.Persistence;
using HR.RequestContext.Domain.EmployeeRequests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Text.Json.Serialization;

namespace HR.RequestContext.Domain.Tests
{
   [TestClass]
   public class EmployeeRequestProcessTest
   {
       private Mock<IEntityIdGenerator<EmployeeRequestDetail>> idGenerator;


       [TestInitialize]
       public void Initial()
       {
           idGenerator = new Mock<IEntityIdGenerator<EmployeeRequestDetail>>();
       }


       //[TestMethod]
       //public void Constructor()
       //{
       //    var process = new EmployeeRequestProcess(1010, 2, 931001, String.Empty, DateTime.Now, 1);
       //}
        
       [TestMethod]
       [DataRow("{CPU: 'Intel'}", "{CPU: 'AMD'}")]
       public void UpdateData(string description, string newValue)
       {
           var process = new EmployeeRequestProcess(1010, 2, 931001, description, DateTime.Now, 1);
           process.UpdateData(newValue);
            
           Assert.IsNotNull(process.Description);
           Assert.AreEqual(Newtonsoft.Json.JsonConvert.DeserializeObject(newValue).ToString(), Newtonsoft.Json.JsonConvert.DeserializeObject(process.Description).ToString());
       }
   }
}
