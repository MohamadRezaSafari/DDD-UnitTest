using HR.RequestContext.Domain.EmployeeRequests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HR.RequestContext.Domain.Tests
{
    [TestClass]
    public class EmployeeRequestRoleInRequestTest
    {
        [TestMethod]
        public void Constructor()
        {
           var employeeRequestRoleInRequest=  new EmployeeRequestRoleInRequest(931001, 1, 16, 2, 2, 1);

           Assert.IsNotNull(employeeRequestRoleInRequest.EmployeeId);
           Assert.IsNotNull(employeeRequestRoleInRequest.PartId);
           Assert.IsNotNull(employeeRequestRoleInRequest.Priority);
           Assert.IsNotNull(employeeRequestRoleInRequest.RequestCheckId);
           Assert.IsNotNull(employeeRequestRoleInRequest.RequestTypeId);
           Assert.IsNotNull(employeeRequestRoleInRequest.RoleId);
        }
    }
}
