using Moq;
using System;
using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class ClaimInteractorTest
    {
        [Fact]
        public void ClaimInteractor_SuccessScenario()
        {
            // arrange

            // 1. ��� ������ ������ ����� ������-�� �����
            var mockClaimRepository = new Mock<IClaimRepository>();
            mockClaimRepository.Setup(r => r.GetClaim(It.IsAny<int>())).Returns(new Claim());

            // 2. ����� ������� ��� ���������� ����������
            var editorService = new Infrastructure.OfficeInterop.WordService();

            // 3. ����� ���������� ����������
            var claimInteractor = new ClaimInteractor(editorService, mockClaimRepository.Object);

            // 4. �������� ������
            var request = new ClaimRequest() { ClaimID = 18012 };

            // act

            // 5. ������������� ��������
            var response = claimInteractor.Handle(request);

            // assert
            Assert.True(response.Success);
        }

        [Fact]
        public void ClaimInteractor_NotClaimFound()
        {
            // arrange

            // 1. �������� ����������� ������
            var mockClaimRepository = new Mock<IClaimRepository>();
            mockClaimRepository.Setup(r => r.GetClaim(It.IsAny<int>())).Returns((Claim)null);

            // 2. ����� ������� ��� ���������� ����������
            var editorService = new Infrastructure.OfficeInterop.WordService();

            // 3. ����� ���������� ����������
            var claimInteractor = new ClaimInteractor(editorService, mockClaimRepository.Object);

            // 4. �������� ������
            var request = new ClaimRequest() { ClaimID = 18011 };

            // act

            // 5. ������������� ��������
            var response = claimInteractor.Handle(request);

            // assert
            Assert.False(response.Success);
        }
    }
}
