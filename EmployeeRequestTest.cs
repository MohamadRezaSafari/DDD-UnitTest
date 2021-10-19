using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Framework.Core.Persistence;
using Framework.Globalization;
using HR.Constants;
using HR.RequestContext.Domain.ACL;
using HR.RequestContext.Domain.ACL.Dto;
using HR.RequestContext.Domain.EmployeeRequests;
using HR.RequestContext.Domain.EmployeeRequests.Exceptions;
using HR.RequestContext.Domain.EmployeeRequests.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HR.RequestContext.Domain.Tests
{
   [TestClass]
   public class EmployeeRequestTest
   {
       private Mock<IEntityIdGenerator<EmployeeRequest>> idGenerator;
       private Mock<IEntityIdGenerator<EmployeeRequestDetail>> idGeneratorDetail;
       private Mock<IDefinitionAclService> definitionAclService;
       private Mock<IEmployeeAclService> employeeAclService;
       private Mock<IEmployeeRequestRepository> employeeRequestRepository;

       [TestInitialize]
       public void Initial()
       {
           idGenerator = new Mock<IEntityIdGenerator<EmployeeRequest>>();
           idGeneratorDetail = new Mock<IEntityIdGenerator<EmployeeRequestDetail>>();
           definitionAclService = new Mock<IDefinitionAclService>();
           employeeAclService = new Mock<IEmployeeAclService>();
           employeeRequestRepository = new Mock<IEmployeeRequestRepository>();
       }

       [TestMethod]
       public void Constructor()
       {
           var request = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object,
               employeeAclService.Object, 1, 1, 990041, DateTime.Now);

           Assert.IsNotNull(request.Id);
           Assert.IsNotNull(request.WorkplaceId);
           Assert.IsNotNull(request.OperationDateTime);
           Assert.IsNotNull(request.EmployeeRequestDetails);
           Assert.IsNotNull(request.EmployeeRequestProcesses);
           //Assert.AreEqual(1, request.EmployeeRequestDetails.Count);
          // Assert.AreEqual(2, request.EmployeeRequestProcesses.Count);
       }


       [TestMethod]
       public void CheckForInternetConnection()
       {
           dynamic x;
           try
           {
               using (var client = new WebClient())
               using (client.OpenRead("http://google.com/generate_204"))
                   x= true;
           }
           catch
           {
               x= false;
           }
       }

       [TestMethod]
       public void SignDecline_SupervisorRole_InternalMission()
       {
           EmployeeDto employeeDto = new EmployeeDto() { EmployeeId = 990041, PartId = 16 };
           EmployeeRequestDetail requestDetail = new EmployeeRequestDetail()
           {
               EmployeeRequestId = 1010,
               EmployeeId = 990041,
               PartId = 16
           };
           IList<EmployeeRequestRoleInRequest> inRequestDto = new List<EmployeeRequestRoleInRequest>()
           {
               new EmployeeRequestRoleInRequest()
               {
                   EmployeeId = 83000, PartId = 16, RequestTypeId = 2, RequestCheckId = 1, RoleId = 2, Priority = 1
               },
               new EmployeeRequestRoleInRequest()
               {
                   EmployeeId = 931001, PartId = 16, RequestTypeId = 2, RequestCheckId = 2, RoleId = 2, Priority = 2
               },
               new EmployeeRequestRoleInRequest()
               {
                   EmployeeId = 950010, PartId = 288, RequestTypeId = 2, RequestCheckId = 3, RoleId = 2, Priority = 3
               }
           };

           var employeeRequest = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object, employeeAclService.Object, 2, 1, 990041, DateTime.Now);

           employeeRequest.EmployeeRequestDetails.Add(requestDetail);
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 2,
               StatusId = 1
           });
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 1,
               StatusId = 1
           });
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 3,
               StatusId = 1
           });

           IList<RequestCheckSignDto> requestCheckSignDto = new List<RequestCheckSignDto>();
           requestCheckSignDto.Add(new RequestCheckSignDto() { RequestCheckId = 1, RequestTypeId = 2 });
           requestCheckSignDto.Add(new RequestCheckSignDto() { RequestCheckId = 2, RequestTypeId = 2 });
           requestCheckSignDto.Add(new RequestCheckSignDto() { RequestCheckId = 3, RequestTypeId = 2 });
           definitionAclService.Setup(i => i.GetRequestCheckSigns(2, 1)).Returns(requestCheckSignDto);

           employeeAclService.Setup(i => i.GetEmployeeById(990041)).Returns(employeeDto);
           employeeRequestRepository.Setup(i => i.EmployeeRequestRoleInRequestByRequestTypeIdAndPart(2, 16)).Returns(inRequestDto);

           employeeRequest.Sign(requestDetail, false, 931001, null, 2, employeeRequestRepository.Object, employeeAclService.Object, definitionAclService.Object);

           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).Visible);
           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 1).Visible);
           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).Visible);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Rejected, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).StatusId);
           //Assert.AreEqual((int)EmployeeRequestProcessStatus.Pending, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 1).StatusId);
           Assert.IsNotNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).RequestCheckEmployeeId);
           Assert.IsNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).RequestCheckEmployeeId);
           Assert.IsNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 1).RequestCheckEmployeeId);
       }
        
       [TestMethod]
       public void SignApprove_SupervisorRole_InternalMission()
       {
           EmployeeDto employeeDto = new EmployeeDto() {EmployeeId = 990041, PartId = 16};
           EmployeeRequestDetail requestDetail = new EmployeeRequestDetail()
           {
               EmployeeRequestId = 1010,
               EmployeeId = 990041,
               PartId = 16
           };
           IList<EmployeeRequestRoleInRequest> inRequestDto = new List<EmployeeRequestRoleInRequest>()
           {
               new EmployeeRequestRoleInRequest()
               {
                   EmployeeId = 83000, PartId = 16, RequestTypeId = 2, RequestCheckId = 1, RoleId = 2, Priority = 1
               },
               new EmployeeRequestRoleInRequest()
               {
                   EmployeeId = 931001, PartId = 16, RequestTypeId = 2, RequestCheckId = 2, RoleId = 2, Priority = 2
               },
               new EmployeeRequestRoleInRequest()
               {
                   EmployeeId = 950010, PartId = 288, RequestTypeId = 2, RequestCheckId = 3, RoleId = 2, Priority = 3
               }
           };

           var employeeRequest = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object, employeeAclService.Object, 2, 1, 990041, DateTime.Now);

           employeeRequest.EmployeeRequestDetails.Add(requestDetail);
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 2,
               StatusId = 1
           });
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 1,
               StatusId = 1
           });
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 3,
               StatusId = 1
           });

           IList<RequestCheckSignDto> requestCheckSignDto = new List<RequestCheckSignDto>();
           requestCheckSignDto.Add(new RequestCheckSignDto() { RequestCheckId = 1, RequestTypeId = 2 });
           requestCheckSignDto.Add(new RequestCheckSignDto() { RequestCheckId = 2, RequestTypeId = 2 });
           requestCheckSignDto.Add(new RequestCheckSignDto() { RequestCheckId = 3, RequestTypeId = 2 });
           definitionAclService.Setup(i => i.GetRequestCheckSigns(2, 1)).Returns(requestCheckSignDto);

           employeeAclService.Setup(i => i.GetEmployeeById(990041)).Returns(employeeDto);
           employeeRequestRepository.Setup(i => i.EmployeeRequestRoleInRequestByRequestTypeIdAndPart(2, 16)).Returns(inRequestDto);

           employeeRequest.Sign(requestDetail, true, 931001, null, 2, employeeRequestRepository.Object, employeeAclService.Object, definitionAclService.Object);

           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).Visible);
           Assert.AreEqual(true, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 1).Visible);
           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).Visible);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Approved, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).StatusId);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Pending, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 1).StatusId);
           Assert.IsNotNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).RequestCheckEmployeeId);
           Assert.IsNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).RequestCheckEmployeeId);
           Assert.IsNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 1).RequestCheckEmployeeId);
       }

       [TestMethod]
       public void AddProcessInternalMission_CommuteTypeOfficial()
       {
           IList<RequestCheckSignDto> requestCheckSignDto = new List<RequestCheckSignDto>();
           requestCheckSignDto.Add(new RequestCheckSignDto() { RequestCheckId = 1, RequestTypeId = 2 });
           requestCheckSignDto.Add(new RequestCheckSignDto() { RequestCheckId = 2, RequestTypeId = 2 });
           requestCheckSignDto.Add(new RequestCheckSignDto() { RequestCheckId = 3, RequestTypeId = 2 });
           definitionAclService.Setup(i => i.GetRequestCheckSigns(2, 1)).Returns(requestCheckSignDto);

           var request = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object,
               employeeAclService.Object, 2, 1, 990041, DateTime.Now);

           var stringjsonData = "{'CommuteTypeId': '2'}";
           JObject s = JObject.Parse(stringjsonData);

           request.AddProcess(2, 1, s.ToString());

           Assert.IsNotNull(request.EmployeeRequestProcesses);
           Assert.AreEqual(3, request.EmployeeRequestProcesses.Count);
       }
        
       [TestMethod]
       public void AddProcessInternalMission_CommuteTypePersonal()
       {
           IList<RequestCheckSignDto> requestCheckSignDto = new List<RequestCheckSignDto>();
           requestCheckSignDto.Add(new RequestCheckSignDto() { RequestCheckId = 1, RequestTypeId = 2 });
           requestCheckSignDto.Add(new RequestCheckSignDto() { RequestCheckId = 2, RequestTypeId = 2 });
           requestCheckSignDto.Add(new RequestCheckSignDto() { RequestCheckId = 3, RequestTypeId = 2 });
           definitionAclService.Setup(i => i.GetRequestCheckSigns(2, 1)).Returns(requestCheckSignDto);

           var request = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object,
               employeeAclService.Object, 2, 1, 990041, DateTime.Now);
            
           var stringjsonData = "{'CommuteTypeId': '1'}";
           JObject s = JObject.Parse(stringjsonData);

           request.AddProcess(2, 1, s.ToString());

           Assert.IsNotNull(request.EmployeeRequestProcesses);
           Assert.AreEqual(2, request.EmployeeRequestProcesses.Count);
       }
        
       [TestMethod]
       [ExpectedException(typeof(NoCheckSignForThisWorkPlaceException))]
       public void AddProcessInternalMission_ThrowException()
       {
           IList<RequestCheckSignDto> requestCheckSignDto = new List<RequestCheckSignDto>();
           definitionAclService.Setup(i => i.GetRequestCheckSigns(2, 1)).Returns(requestCheckSignDto);

           var request = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object,
               employeeAclService.Object, 3, 1, 990041, DateTime.Now);

           var stringjsonData = "{'CommuteTypeId': '2'}";
           JObject s = JObject.Parse(stringjsonData);
           request.AddProcess(2, 1, s.ToString());
       }
        
       [TestMethod]
       public void DeleteSignWithValue()
       {
           var request = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object,
               employeeAclService.Object, 1, 1, 990041, DateTime.Now);

           request.EmployeeRequestProcesses.Add(new EmployeeRequestProcess() { RequestCheckEmployeeId = 931001 });
           request.DeleteSign();

           Assert.IsNull(request.EmployeeRequestProcesses.First().RequestCheckEmployeeId);
           Assert.IsNull(request.EmployeeRequestProcesses.First().OperationDateTime);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Pending, request.EmployeeRequestProcesses.First().StatusId);
       }
        
       [TestMethod]
       public void DeleteSignWithoutCheckId()
       {
           EmployeeDto employeeDto = new EmployeeDto() { EmployeeId = 990041, PartId = 16 };
           var request = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object,
               employeeAclService.Object, 1, 1, 990041, DateTime.Now);

           var requestDetail = new EmployeeRequestDetail(idGeneratorDetail.Object, 1, 1010, 990041, "{}", 1, "10:00", "11:00", DateTime.Now.GetPersianDate(), 16);

           request.EmployeeRequestDetails.Add(new EmployeeRequestDetail()
           {
               EmployeeId = 990041,
               Data = "{}",
               EmployeeRequestId = 1010,
               FromDateTime = DateTime.Now,
               ToDateTime = DateTime.Now.AddHours(2),
               PartId = 16,
               StatusId = 1
           });
           request.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               //RequestCheckEmployeeId = 950010,
               EmployeeRequestId = 1010,
               RequestCheckId = 3
           });
           request.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               //RequestCheckEmployeeId = 931001,
               EmployeeRequestId = 1010,
               RequestCheckId = 2
           });

           IList<EmployeeRequestRoleInRequest> inRequestDto = new List<EmployeeRequestRoleInRequest>()
           {
               new EmployeeRequestRoleInRequest() {EmployeeId = 931001, PartId = 16, RequestTypeId = 1, RequestCheckId = 2, RoleId = 2, Priority = 1
               },
               new EmployeeRequestRoleInRequest() {EmployeeId = 950010, PartId = 288, RequestTypeId = 1, RequestCheckId = 3, RoleId = 2, Priority = 1
               }
           };

           employeeAclService.Setup(i => i.GetEmployeeById(931001)).Returns(employeeDto);
           employeeRequestRepository.Setup(i => i.EmployeeRequestRoleInRequestByRequestTypeIdAndPartEmployee(931001, 1, 16)).Returns(inRequestDto);

           request.DeleteSign(1010, 931001, employeeRequestRepository.Object, employeeAclService.Object);
       }

       [TestMethod]
       public void DeleteSign()
       {
           EmployeeDto employeeDto = new EmployeeDto() { EmployeeId = 990041, PartId = 16 };
           var request = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object,
               employeeAclService.Object, 1, 1, 990041, DateTime.Now);

           var requestDetail = new EmployeeRequestDetail(idGeneratorDetail.Object, 1, 1010, 990041, "{}", 1, "10:00", "11:00", DateTime.Now.GetPersianDate(), 16);

           request.EmployeeRequestDetails.Add(new EmployeeRequestDetail()
           {
               EmployeeId = 990041,
               Data = "{}", 
               EmployeeRequestId = 1010,
               FromDateTime = DateTime.Now,
               ToDateTime = DateTime.Now.AddHours(2),
               PartId = 16,
               StatusId = 1
           });
           request.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               RequestCheckEmployeeId = 950010,
               EmployeeRequestId = 1010,
               RequestCheckId = 3
           });
           request.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               RequestCheckEmployeeId = 931001,
               EmployeeRequestId = 1010,
               RequestCheckId = 2
           });

           IList<EmployeeRequestRoleInRequest> inRequestDto = new List<EmployeeRequestRoleInRequest>()
           {
               new EmployeeRequestRoleInRequest() {EmployeeId = 931001, PartId = 16, RequestTypeId = 1, RequestCheckId = 2, RoleId = 2, Priority = 1
               },
               new EmployeeRequestRoleInRequest() {EmployeeId = 950010, PartId = 288, RequestTypeId = 1, RequestCheckId = 3, RoleId = 2, Priority = 1
               }
           };

           employeeAclService.Setup(i => i.GetEmployeeById(931001)).Returns(employeeDto);
           employeeRequestRepository.Setup(i => i.EmployeeRequestRoleInRequestByRequestTypeIdAndPartEmployee(931001, 1, 16, 2)).Returns(inRequestDto);

           request.DeleteSign(1010, 931001, employeeRequestRepository.Object, employeeAclService.Object, 2);
       }

       [TestMethod]
       public void SignDecline_GuardRoleWithGuardOutTime()
       {
           EmployeeDto employeeDto = new EmployeeDto() { EmployeeId = 990041, PartId = 16 };
           EmployeeRequestDetail requestDetail = new EmployeeRequestDetail()
           {
               EmployeeRequestId = 1010, EmployeeId = 990041, PartId = 16, Data = "{}"
           };
           IList<EmployeeRequestRoleInRequest> inRequestDto = new List<EmployeeRequestRoleInRequest>()
           {
               new EmployeeRequestRoleInRequest() {EmployeeId = 931001, PartId = 16, RequestTypeId = 1, RequestCheckId = 2, RoleId = 2, Priority = 1
               },
               new EmployeeRequestRoleInRequest() {EmployeeId = 950010, PartId = 288, RequestTypeId = 1, RequestCheckId = 3, RoleId = 2, Priority = 1
               }
           };

           var employeeRequest = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object, employeeAclService.Object, 1, 1, 990041, DateTime.Now);

           employeeRequest.EmployeeRequestDetails.Add(requestDetail);
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 2,
               StatusId = 1
           });
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 3,
               StatusId = 1
           });

           employeeAclService.Setup(i => i.GetEmployeeById(990041)).Returns(employeeDto);
           employeeRequestRepository.Setup(i => i.EmployeeRequestRoleInRequestByRequestTypeIdAndPart(1, 16)).Returns(inRequestDto);

           employeeRequest.Sign(requestDetail, true, 931001, null, 2, employeeRequestRepository.Object, employeeAclService.Object);
           employeeRequest.Sign(requestDetail, false, 950010, null, 3, employeeRequestRepository.Object, employeeAclService.Object, "11:00", String.Empty);

           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).Visible);
           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).Visible);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Approved, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).StatusId);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Rejected, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).StatusId);
           Assert.IsNotNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).RequestCheckEmployeeId);
           Assert.IsNotNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).RequestCheckEmployeeId);
       }

       [TestMethod]
       public void SignApprove_GuardRoleWithGuardOutTime()
       {
           EmployeeDto employeeDto = new EmployeeDto()
           {
               EmployeeId = 990041,
               PartId = 16
           };
           EmployeeRequestDetail requestDetail = new EmployeeRequestDetail()
           {
               EmployeeRequestId = 1010, EmployeeId = 990041, PartId = 16, Data = "{}"
           };
           IList<EmployeeRequestRoleInRequest> inRequestDto = new List<EmployeeRequestRoleInRequest>()
           {
               new EmployeeRequestRoleInRequest()
               {
                   EmployeeId = 931001, PartId = 16, RequestTypeId = 1, RequestCheckId = 2, RoleId = 2, Priority = 1
               },
               new EmployeeRequestRoleInRequest()
               {
                   EmployeeId = 950010, PartId = 288, RequestTypeId = 1, RequestCheckId = 3, RoleId = 2, Priority = 1
               }
           };

           var employeeRequest = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object, employeeAclService.Object, 1, 1, 990041, DateTime.Now);

           employeeRequest.EmployeeRequestDetails.Add(requestDetail);
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 2,
               StatusId = 1
           });
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 3,
               StatusId = 1
           });

           employeeAclService.Setup(i => i.GetEmployeeById(990041)).Returns(employeeDto);
           employeeRequestRepository.Setup(i => i.EmployeeRequestRoleInRequestByRequestTypeIdAndPart(1, 16)).Returns(inRequestDto);

           employeeRequest.Sign(requestDetail, true, 931001, null, 2, employeeRequestRepository.Object, employeeAclService.Object);
           employeeRequest.Sign(requestDetail, true, 950010, null, 3, employeeRequestRepository.Object, employeeAclService.Object, "11:00", String.Empty);

           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).Visible);
           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).Visible);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Approved, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).StatusId);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Approved, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).StatusId);
           Assert.IsNotNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).RequestCheckEmployeeId);
           Assert.IsNotNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).RequestCheckEmployeeId);
       }

       [TestMethod]
       public void SignDecline_GuardRole()
       {
           EmployeeDto employeeDto = new EmployeeDto() { EmployeeId = 990041, PartId = 16 };
           EmployeeRequestDetail requestDetail = new EmployeeRequestDetail()
           {
               EmployeeRequestId = 1010, EmployeeId = 990041, PartId = 16
           };
           IList<EmployeeRequestRoleInRequest> inRequestDto = new List<EmployeeRequestRoleInRequest>()
           {
               new EmployeeRequestRoleInRequest() {EmployeeId = 931001, PartId = 16, RequestTypeId = 1, RequestCheckId = 2, RoleId = 2, Priority = 1
               },
               new EmployeeRequestRoleInRequest() {EmployeeId = 950010, PartId = 288, RequestTypeId = 1, RequestCheckId = 3, RoleId = 2, Priority = 1
               }
           };

           var employeeRequest = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object, employeeAclService.Object, 1, 1, 990041, DateTime.Now);

           employeeRequest.EmployeeRequestDetails.Add(requestDetail);
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010, RequestCheckId = 2, StatusId = 1
           });
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010, RequestCheckId = 3, StatusId = 1
           });

           employeeAclService.Setup(i => i.GetEmployeeById(990041)).Returns(employeeDto);
           employeeRequestRepository.Setup(i => i.EmployeeRequestRoleInRequestByRequestTypeIdAndPart(1, 16)).Returns(inRequestDto);

           employeeRequest.Sign(requestDetail, true, 931001, null, 2, employeeRequestRepository.Object, employeeAclService.Object);
           employeeRequest.Sign(requestDetail, false, 950010, null, 3, employeeRequestRepository.Object, employeeAclService.Object);

           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).Visible);
           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).Visible);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Approved, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).StatusId);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Rejected, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).StatusId);
           Assert.IsNotNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).RequestCheckEmployeeId);
           Assert.IsNotNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).RequestCheckEmployeeId);
       }

       [TestMethod]
       public void SignApprove_GuardRole()
       {
           EmployeeDto employeeDto = new EmployeeDto()
           {
               EmployeeId = 990041,
               PartId = 16
           };
           EmployeeRequestDetail requestDetail = new EmployeeRequestDetail()
           {
               EmployeeRequestId = 1010,
               EmployeeId = 990041,
               PartId = 16
           };
           IList<EmployeeRequestRoleInRequest> inRequestDto = new List<EmployeeRequestRoleInRequest>()
           {
               new EmployeeRequestRoleInRequest()
               {
                   EmployeeId = 931001, PartId = 16, RequestTypeId = 1, RequestCheckId = 2, RoleId = 2, Priority = 1
               },
               new EmployeeRequestRoleInRequest()
               {
                   EmployeeId = 950010, PartId = 288, RequestTypeId = 1, RequestCheckId = 3, RoleId = 2, Priority = 1
               }
           };

           var employeeRequest = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object, employeeAclService.Object, 1, 1, 990041, DateTime.Now);

           employeeRequest.EmployeeRequestDetails.Add(requestDetail);
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 2,
               StatusId = 1
           });
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 3,
               StatusId = 1
           });

           employeeAclService.Setup(i => i.GetEmployeeById(990041)).Returns(employeeDto);
           employeeRequestRepository.Setup(i => i.EmployeeRequestRoleInRequestByRequestTypeIdAndPart(1, 16)).Returns(inRequestDto);

           employeeRequest.Sign(requestDetail, true, 931001, null, 2, employeeRequestRepository.Object, employeeAclService.Object);
           employeeRequest.Sign(requestDetail, true, 950010, null, 3, employeeRequestRepository.Object, employeeAclService.Object);

           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).Visible);
           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).Visible);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Approved, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).StatusId);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Approved, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).StatusId);
           Assert.IsNotNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).RequestCheckEmployeeId);
           Assert.IsNotNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).RequestCheckEmployeeId);
       }


       [TestMethod]
       public void SignDecline_ConfirmationRoleWithGuardOutTime()
       {
           EmployeeDto employeeDto = new EmployeeDto() { EmployeeId = 990041, PartId = 16 };
           EmployeeRequestDetail requestDetail = new EmployeeRequestDetail()
           {
               EmployeeRequestId = 1010, EmployeeId = 990041, PartId = 16, Data = "{}"
           };
           IList<EmployeeRequestRoleInRequest> inRequestDto = new List<EmployeeRequestRoleInRequest>()
           {
               new EmployeeRequestRoleInRequest() {EmployeeId = 931001, PartId = 16, RequestTypeId = 1, RequestCheckId = 2, RoleId = 2, Priority = 1
               },
               new EmployeeRequestRoleInRequest() {EmployeeId = 950010, PartId = 288, RequestTypeId = 1, RequestCheckId = 3, RoleId = 2, Priority = 1
               }
           };

           var employeeRequest = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object, employeeAclService.Object, 1, 1, 990041, DateTime.Now);

           employeeRequest.EmployeeRequestDetails.Add(requestDetail);
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010, RequestCheckId = 2, StatusId = 1, Visible = true
           });
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010, RequestCheckId = 3, StatusId = 1, Visible = false
           });

           employeeAclService.Setup(i => i.GetEmployeeById(990041)).Returns(employeeDto);
           employeeRequestRepository.Setup(i => i.EmployeeRequestRoleInRequestByRequestTypeIdAndPart(1, 16)).Returns(inRequestDto);

           employeeRequest.Sign(requestDetail, false, 931001, null, 2, employeeRequestRepository.Object, employeeAclService.Object);

           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).Visible);
           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).Visible);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Rejected, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).StatusId);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Pending, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).StatusId);
           Assert.IsNotNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).RequestCheckEmployeeId);
           Assert.IsNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).RequestCheckEmployeeId);
       }

       [TestMethod]
       public void SignDecline_ConfirmationRole()
       {
           EmployeeDto employeeDto = new EmployeeDto() { EmployeeId = 990041, PartId = 16 };
           EmployeeRequestDetail requestDetail = new EmployeeRequestDetail()
           {
               EmployeeRequestId = 1010,
               EmployeeId = 990041,
               PartId = 16
           };
           IList<EmployeeRequestRoleInRequest> inRequestDto = new List<EmployeeRequestRoleInRequest>()
           {
               new EmployeeRequestRoleInRequest() {EmployeeId = 931001, PartId = 16, RequestTypeId = 1, RequestCheckId = 2, RoleId = 2, Priority = 1
               },
               new EmployeeRequestRoleInRequest() {EmployeeId = 950010, PartId = 288, RequestTypeId = 1, RequestCheckId = 3, RoleId = 2, Priority = 1
               }
           };

           var employeeRequest = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object, employeeAclService.Object, 1, 1, 990041, DateTime.Now);

           employeeRequest.EmployeeRequestDetails.Add(requestDetail);
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 2,
               StatusId = 1
           });
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 3,
               StatusId = 1
           });

           employeeAclService.Setup(i => i.GetEmployeeById(990041)).Returns(employeeDto);
           employeeRequestRepository.Setup(i => i.EmployeeRequestRoleInRequestByRequestTypeIdAndPart(1, 16)).Returns(inRequestDto);

           employeeRequest.Sign(requestDetail, false, 931001, null, 2, employeeRequestRepository.Object, employeeAclService.Object);

           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).Visible);
           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).Visible);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Rejected, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).StatusId);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Pending, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).StatusId);
           Assert.IsNotNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).RequestCheckEmployeeId);
           Assert.IsNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).RequestCheckEmployeeId);
       }

       [TestMethod]
       public void SignApprove_SupervisorRole_PersonalForm()
       {
           EmployeeDto employeeDto = new EmployeeDto() { EmployeeId = 990041, PartId = 16 };
           EmployeeRequestDetail requestDetail = new EmployeeRequestDetail()
           {
               EmployeeRequestId = 1010,
               EmployeeId = 990041,
               PartId = 16
           };
           IList<EmployeeRequestRoleInRequest> inRequestDto = new List<EmployeeRequestRoleInRequest>()
           {
               new EmployeeRequestRoleInRequest()
               {
                   EmployeeId = 83000, PartId = 16, RequestTypeId = 1, RequestCheckId = 1, RoleId = 2, Priority = 1
               },
               new EmployeeRequestRoleInRequest()
               {
                   EmployeeId = 931001, PartId = 16, RequestTypeId = 1, RequestCheckId = 2, RoleId = 2, Priority = 2
               },
               new EmployeeRequestRoleInRequest()
               {
                   EmployeeId = 950010, PartId = 288, RequestTypeId = 1, RequestCheckId = 3, RoleId = 2, Priority = 3
               }
           };

           var employeeRequest = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object, employeeAclService.Object, 1, 1, 990041, DateTime.Now);

           employeeRequest.EmployeeRequestDetails.Add(requestDetail);
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 2,
               StatusId = 1
           });
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 3,
               StatusId = 1
           });

           IList<RequestCheckSignDto> requestCheckSignDto = new List<RequestCheckSignDto>();
           requestCheckSignDto.Add(new RequestCheckSignDto() { RequestCheckId = 2, RequestTypeId = 2 });
           requestCheckSignDto.Add(new RequestCheckSignDto() { RequestCheckId = 3, RequestTypeId = 2 });
           definitionAclService.Setup(i => i.GetRequestCheckSigns(1, 1)).Returns(requestCheckSignDto);

           employeeAclService.Setup(i => i.GetEmployeeById(990041)).Returns(employeeDto);
           employeeRequestRepository.Setup(i => i.EmployeeRequestRoleInRequestByRequestTypeIdAndPart(1, 16)).Returns(inRequestDto);

           employeeRequest.Sign(requestDetail, true, 931001, null, 2, employeeRequestRepository.Object, employeeAclService.Object, definitionAclService.Object);

           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).Visible);
           Assert.AreEqual(true, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).Visible);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Approved, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).StatusId);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Pending, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).StatusId);
           Assert.IsNotNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).RequestCheckEmployeeId);
           Assert.IsNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).RequestCheckEmployeeId);
           Assert.IsNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).RequestCheckEmployeeId);
       }

       [TestMethod]
       public void SignApprove_ConfirmationRole()
       {
           EmployeeDto employeeDto = new EmployeeDto()
           {
               EmployeeId = 990041,
               PartId = 16
           };
           EmployeeRequestDetail requestDetail = new EmployeeRequestDetail()
           {
               EmployeeRequestId = 1010,
               EmployeeId = 990041,
               PartId = 16
           };
           IList<EmployeeRequestRoleInRequest> inRequestDto = new List<EmployeeRequestRoleInRequest>()
           {
               new EmployeeRequestRoleInRequest()
               {
                   EmployeeId = 931001, PartId = 16, RequestTypeId = 1, RequestCheckId = 2, RoleId = 2, Priority = 1
               },
               new EmployeeRequestRoleInRequest()
               {
                   EmployeeId = 950010, PartId = 288, RequestTypeId = 1, RequestCheckId = 3, RoleId = 2, Priority = 1
               }
           };

           var employeeRequest = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object, employeeAclService.Object, 1, 1, 990041, DateTime.Now);

           employeeRequest.EmployeeRequestDetails.Add(requestDetail);
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 2,
               StatusId = 1
           });
           employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           {
               EmployeeRequestId = 1010,
               RequestCheckId = 3,
               StatusId = 1
           });


           employeeAclService.Setup(i => i.GetEmployeeById(990041)).Returns(employeeDto);
           employeeRequestRepository.Setup(i => i.EmployeeRequestRoleInRequestByRequestTypeIdAndPart(1, 16)).Returns(inRequestDto);

           employeeRequest.Sign(requestDetail, true, 931001, null, 2, employeeRequestRepository.Object, employeeAclService.Object);

           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).Visible);
           Assert.AreEqual(true, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).Visible);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Approved, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).StatusId);
           Assert.AreEqual((int)EmployeeRequestProcessStatus.Pending, employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).StatusId);
           Assert.IsNotNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 2).RequestCheckEmployeeId);
           Assert.IsNull(employeeRequest.EmployeeRequestProcesses.First(i => i.RequestCheckId == 3).RequestCheckEmployeeId);
       }


       [TestMethod]
       public void AddProcess_WorkPlaceIsBlur()
       {
           IList<RequestCheckSignDto> requestCheckSignDto = new List<RequestCheckSignDto>();
           requestCheckSignDto.Add(new RequestCheckSignDto() { RequestCheckId = 2, RequestTypeId = 1 });
           definitionAclService.Setup(i => i.GetRequestCheckSigns(1, 6)).Returns(requestCheckSignDto);

           var employeeRequest = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object, employeeAclService.Object, 1, 1, 990041, DateTime.Now);

           employeeRequest.AddProcess(1, 6);

           Assert.IsNotNull(requestCheckSignDto);
           Assert.AreEqual(1, employeeRequest.EmployeeRequestProcesses.Count);
           Assert.AreEqual(true, employeeRequest.EmployeeRequestProcesses.First().Visible);
       }

       [TestMethod]
       public void AddProcess_WorkPlaceIsCompany()
       {
           IList<RequestCheckSignDto> requestCheckSignDto = new List<RequestCheckSignDto>();
           requestCheckSignDto.Add(new RequestCheckSignDto() { RequestCheckId = 2, RequestTypeId = 1 });
           requestCheckSignDto.Add(new RequestCheckSignDto() { RequestCheckId = 3, RequestTypeId = 1 });
           definitionAclService.Setup(i => i.GetRequestCheckSigns(1, 1)).Returns(requestCheckSignDto);

           var employeeRequest = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object, employeeAclService.Object, 1, 1, 990041, DateTime.Now);

           employeeRequest.AddProcess(1, 1);
           //foreach (var checkSignDto in requestCheckSignDto)
           //{
           //    employeeRequest.EmployeeRequestProcesses.Add(new EmployeeRequestProcess()
           //    {
           //        RequestCheckId = checkSignDto.RequestCheckId,
           //        StatusId = (int)EmployeeRequestDetailStatus.Pending,
           //        Description = null,
           //        EmployeeRequestId = 1010,
           //        OperationDateTime = null,
           //        RequestCheckEmployeeId = null
           //    });
           //}

           Assert.IsNotNull(requestCheckSignDto);
           Assert.AreEqual(2, employeeRequest.EmployeeRequestProcesses.Count);
           Assert.AreEqual(true, employeeRequest.EmployeeRequestProcesses.First().Visible);
           Assert.AreEqual(false, employeeRequest.EmployeeRequestProcesses.Last().Visible);
       }


       [TestMethod]
       public void AddData()
       {
           EmployeeDto employeeDto = new EmployeeDto() { EmployeeId = 990041 };
           employeeAclService.Setup(i => i.GetEmployeeById(990041)).Returns(employeeDto);

           var requestDetail = new EmployeeRequestDetail(idGeneratorDetail.Object, 1, 1010, 990041, "{}", (int)EmployeeRequestDetailStatus.Pending, "10:00", "11:00", DateTime.Now.GetPersianDate(), 16);

           var employeeRequest = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object, employeeAclService.Object, 1, 1, 990041, DateTime.Now);
            
           employeeRequest.AddData("{}", 990041, employeeRequestRepository.Object, "10:00", "11:11", DateTime.Now.GetPersianDate(), 16);
           employeeRequest.EmployeeRequestDetails.Add(requestDetail);

           Assert.IsNotNull(employeeDto);
           //Assert.AreEqual(1, employeeRequest.EmployeeRequestDetails.Count);
           Assert.IsNotNull(employeeRequest.EmployeeRequestDetails);
       }


       [TestMethod]
       [ExpectedException(typeof(EmployeeSignAlreadySignedException))]
       public void DeleteSignWithCheckId_ThrowException()
       {
           EmployeeDto employeeDto = new EmployeeDto() { EmployeeId = 990041, PartId = 16 };
           IList<EmployeeRequestRoleInRequest> employeeRequestRoleInRequest = new List<EmployeeRequestRoleInRequest>()
           {
               new EmployeeRequestRoleInRequest() { RequestCheckId = 2, PartId = 16, RequestTypeId = 1, EmployeeId = 931001 }
           };
           var request = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object,
               employeeAclService.Object, 1, 1, 931001, DateTime.Now);

           request.EmployeeRequestProcesses.Add(new EmployeeRequestProcess() { RequestCheckEmployeeId = 931001, EmployeeRequestId = 1010, RequestCheckId = 2, OperationDateTime = DateTime.Now.AddDays(-1) });
           request.EmployeeRequestProcesses.Add(new EmployeeRequestProcess() { RequestCheckEmployeeId = 950010, EmployeeRequestId = 1010, RequestCheckId = 3, OperationDateTime = DateTime.Now });

           employeeAclService.Setup(i => i.GetEmployeeById(931001)).Returns(employeeDto);
           employeeRequestRepository.Setup(i =>
               i.EmployeeRequestRoleInRequestByRequestTypeIdAndPartEmployee(931001, 1, 16, 2)).Returns(employeeRequestRoleInRequest);

           request.DeleteSign(1010, 931001, employeeRequestRepository.Object, employeeAclService.Object, 2);
       }

       [TestMethod]
       [ExpectedException(typeof(EmployeeSignAlreadySignedException))]
       public void DeleteSignWithOutCheckId_ThrowException()
       {
           EmployeeDto employeeDto = new EmployeeDto() { EmployeeId = 990041, PartId = 16 };
           IList<EmployeeRequestRoleInRequest> employeeRequestRoleInRequest = new List<EmployeeRequestRoleInRequest>()
           {
              new EmployeeRequestRoleInRequest() { RequestCheckId = 2, PartId = 16, RequestTypeId = 1, EmployeeId = 990041 }
           };
           var request = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object,
               employeeAclService.Object, 1, 1, 990041, DateTime.Now);

           request.EmployeeRequestProcesses.Add(new EmployeeRequestProcess() { RequestCheckEmployeeId = 990041, EmployeeRequestId = 1010, RequestCheckId = 2, OperationDateTime = DateTime.Now.AddDays(-1) });
           request.EmployeeRequestProcesses.Add(new EmployeeRequestProcess() { RequestCheckEmployeeId = 931001, EmployeeRequestId = 1010, RequestCheckId = 3, OperationDateTime = DateTime.Now });

           employeeAclService.Setup(i => i.GetEmployeeById(990041)).Returns(employeeDto);
           employeeRequestRepository.Setup(i =>
               i.EmployeeRequestRoleInRequestByRequestTypeIdAndPartEmployee(990041, 1, 16)).Returns(employeeRequestRoleInRequest);

           request.DeleteSign(1010, 990041, employeeRequestRepository.Object, employeeAclService.Object);
       }

       [TestMethod]
       [ExpectedException(typeof(EmployeeSignAlreadySignedException))]
       public void DeleteSign_ThrowException()
       {
           var request = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object,
               employeeAclService.Object, 1, 1, 990041, DateTime.Now);

           request.EmployeeRequestProcesses.Add(new EmployeeRequestProcess() { RequestCheckEmployeeId = 990041 });
           request.EmployeeRequestProcesses.Add(new EmployeeRequestProcess() { RequestCheckEmployeeId = 931001 });

           request.DeleteSign();
       }


       [TestMethod]
       [ExpectedException(typeof(NoCheckSignForThisWorkPlaceException))]
       public void AddProcess_ThrowException()
       {
           IList<RequestCheckSignDto> requestCheckSignDto = new List<RequestCheckSignDto>();
           definitionAclService.Setup(i => i.GetRequestCheckSigns(1, 1)).Returns(requestCheckSignDto);
           var request = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object,
               employeeAclService.Object, 1, 1, 990041, DateTime.Now);
           request.AddProcess(1, 1);
       }

       [TestMethod]
       [ExpectedException(typeof(EmployeeRequestHasNotSufficientPermissionException))]
       public void AddData_ThrowException()
       {
           var request = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object,
               employeeAclService.Object, 1, 1, 990041, DateTime.Now);
           request.AddData("test", 990041, employeeRequestRepository.Object, "10:00", "11:00",
               "1399/06/22", 16);
       }

       //[TestMethod]
       //public void CheckNullCreateEmployeeRequest()
       //{
       //    EmployeeDto employeeDto=new EmployeeDto();
       //    employeeDto.EmployeeId = 990041;
       //    employeeDto.PartId = 1;
       //    employeeDto.CenterId = 100;

       //    employeeAclService.Setup(o => o.GetEmployeeById(990041)).Returns(employeeDto);
       //    var request = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object,
       //        employeeAclService.Object, 1, 1, 990041, DateTime.Now);
       //   request.AddData(null, 990041, employeeRequestRepository.Object, DateTime.Now.ToShortTimeString(), DateTime.Now.AddHours(1).ToShortTimeString(),
       //        DateTime.Now.ToString(), 1);
       //    //Assert.IsNull(request.);
       //}

       [TestMethod]
       [ExpectedException(typeof(RequestSignedAndCannotBeDeletedException))]
       public void DeleteRequest_ThrowException()
       {
           EmployeeRequestProcess testdata = new EmployeeRequestProcess();
           testdata.StatusId = (int)EmployeeRequestProcessStatus.Approved;

           var request = new EmployeeRequest(idGenerator.Object, idGeneratorDetail.Object, definitionAclService.Object,
               employeeAclService.Object, 1, 1, 990041, DateTime.Now);
           request.EmployeeRequestProcesses.Add(testdata);
           request.DeleteRequest();
       }
   }
}
