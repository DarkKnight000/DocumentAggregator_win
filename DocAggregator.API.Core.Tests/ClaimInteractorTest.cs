using Moq;
using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class ClaimInteractorTest
    {
        [Fact]
        public void ClaimInteractor_SuccessScenario()
        {
            // 1. ��� ������ ������ ����� ������-�� �����
            var mockClaimRepository = new Mock<IClaimRepository>();
            mockClaimRepository.Setup(r => r.GetClaim(It.IsAny<int>())).Returns(new Claim());
            // 2. ����� ������� ��� ���������� ����������
            var mockEditorService = new Mock<IEditorService>();
            // 3. ����� ���������� ����������
            var claimInteractor = new ClaimInteractor(mockEditorService.Object, mockClaimRepository.Object);
            // 4. �������� ������
            var request = new ClaimRequest() { ClaimID = 18012 };

            // 5. ������������� ��������
            var response = claimInteractor.Handle(request);

            Assert.True(response.Success);
        }

        [Fact]
        public void ClaimInteractor_NotClaimFound()
        {
            // 1. �������� ����������� ������
            var mockClaimRepository = new Mock<IClaimRepository>();
            mockClaimRepository.Setup(r => r.GetClaim(It.IsAny<int>())).Returns((Claim)null);
            // 2. ����� ������� ��� ���������� ����������
            var mockEditorService = new Mock<IEditorService>();
            // 3. ����� ���������� ����������
            var claimInteractor = new ClaimInteractor(mockEditorService.Object, mockClaimRepository.Object);
            // 4. �������� ������
            var request = new ClaimRequest() { ClaimID = 18011 };

            // 5. ������������� ��������
            var response = claimInteractor.Handle(request);

            Assert.False(response.Success);
        }
    }
}
