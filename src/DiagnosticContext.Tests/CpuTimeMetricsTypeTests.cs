#nullable disable

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Itc.Commons.Tests
{
	[TestClass]
	public sealed class CpuTimeMetricsTypeTests
	{
		[TestMethod]
		public void CreateMeasurerCore_ICreateMeasurerHandlerCalled()
		{
			var handlerMock = new Mock<IMetricsMeasurerCreationHandler>(MockBehavior.Strict);

			MetricsMeasurer handledMeasurer = null;
			handlerMock
				.Setup(m => m.HandleMeasurerCreation(It.IsAny<MetricsMeasurer>()))
				.Callback<MetricsMeasurer>(m => handledMeasurer = m);

			var metricsType = CpuTimeMetricsType.Create("Test", handlerMock.Object);
			var measurer = metricsType.CreateMeasurer();

			Assert.AreEqual(handledMeasurer, measurer);
		}
	}
}
