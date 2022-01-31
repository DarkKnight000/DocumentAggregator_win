using DocAggregator.API.Infrastructure;
using System;
using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class ClaimInteractorTest
    {
        [Fact]
        public void GenerationInteractor_SuccessScenario()
        {
            // arrange

            // 1. ��� ������ ������ ����� ������-�� �����
            var claimRepository = new ClaimRepository();

            // 2. ����� ���������� ����������
            var claimInteractor = new ClaimInteractor(claimRepository);

            // 3. �������� ������
            var request = new ClaimRequest(18012);

            // act

            // 4. ������������� ��������
            var response = claimInteractor.Handle(request);

            // assert
            Assert.True(response.Success);
        }
    }
}
