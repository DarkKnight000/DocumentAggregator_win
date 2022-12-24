//using DocAggregator.API.Core.Models;
//using Moq;
//using System.Collections.Generic;
//using Xunit;

//namespace DocAggregator.API.Core.Tests
//{
//    public class ClaimInteractorTest : TestBase
//    {
//        [Fact]
//        public void ClaimInteractor_SuccessScenario()
//        {
//            // 1. ��� ������ ������ ����� ������-�� �����
//            var mockClaimRepository = new Mock<IClaimRepository>();
//            mockClaimRepository.Setup(r => r.GetClaim(It.IsAny<int>())).Returns(new Claim());
//            // 2. ����� ������� ��� ���������� ����������
//            //var mockEditorService = new Mock<IEditorService>();
//            //mockEditorService.Setup(s => s.OpenTemplate(It.IsAny<string>())).Returns(new Document());
//            //mockEditorService.Setup(s => s.GetInserts(It.IsAny<Document>())).Returns(new[] { new Insert("id") });
//            //mockEditorService.Setup(s => s.SetInserts(It.IsAny<Document>(), It.IsAny<IEnumerable<Insert>>()));
//            // 3. ��� ���������� ����� ����������� �����
//            //var mockFieldRepository = new Mock<IMixedFieldRepository>();
//            //mockFieldRepository.Setup(r => r.GetFieldByNameOrId(It.IsAny<string>())).Returns("text");
//            // 4. ����� ���������� ����������
//            var claimInteractor = new ClaimInteractor(new Mock<FormInteractor>(null, null, LoggerFactory).Object, mockClaimRepository.Object, LoggerFactory);
//            // 5. �������� ������
//            var request = new ClaimRequest() { ClaimID = 18012 };

//            // 6. ������������� ��������
//            var response = claimInteractor.Handle(request);

//            //mockFieldRepository.Verify(r => r.GetFieldByNameOrId("id"));
//            //mockEditorService.Verify(s => s.SetInserts(new Document(), new[] { new Insert("id", InsertKind.PlainText) { ReplacedText = "text" } }));
//            Assert.True(response.Success, ResponseDebugPresenter.Handle(response));
//        }

//        [Fact]
//        public void ClaimInteractor_NotClaimFound()
//        {
//            // 1. �������� ����������� ������
//            var mockClaimRepository = new Mock<IClaimRepository>();
//            mockClaimRepository.Setup(r => r.GetClaim(It.IsAny<int>())).Returns((Claim)null);
//            // 2. ����� ������� ��� ���������� ����������
//            //var mockEditorService = new Mock<IEditorService>();
//            //mockEditorService.Setup(s => s.GetInserts(It.IsAny<Document>())).Returns(System.Array.Empty<Insert>());
//            // 3. ��� ���������� ����� ����������� �����
//            //var mockFieldRepository = new Mock<IMixedFieldRepository>();
//            // 4. ����� ���������� ����������
//            var claimInteractor = new ClaimInteractor(null, mockClaimRepository.Object, LoggerFactory);
//            // 5. �������� ������
//            var request = new ClaimRequest() { ClaimID = 18011 };

//            // 6. ������������� ��������
//            var response = claimInteractor.Handle(request);

//            Assert.False(response.Success);
//        }
//    }
//}
