#nullable disable

using Itc.Commons.Model;
using Itc.Commons.Tests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itc.Commons.Tests
{
	[TestClass]
	public class DiagnosticContextTests : CommonsTests
	{
		private Mock<INewRelicPlugin> pluginMock;

		private INewRelicPlugin Plugin => pluginMock.Object;

		private NewRelicDiagnosticContextMetricsCollection metrics;

		protected override void TestInitializeCore()
		{
			base.TestInitializeCore();

			pluginMock = new Mock<INewRelicPlugin> { CallBase = false };

			metrics = new NewRelicDiagnosticContextMetricsCollection { Plugin = Plugin };

			pluginMock
				.Setup(p => p.Enabled)
				.Returns(true);

			pluginMock
				.Setup(p => p.GetMetrics<NewRelicDiagnosticContextMetricsCollection>())
				.Returns(metrics);
		}

		[TestMethod]
		public void Счетчики()
		{
			using (var firstContext = CreateDiagnosticContext("SendEmail"))
			{
				firstContext.Increment("BuildCount");
				using (firstContext.Measure("Build"))
				{
					firstContext.Increment("BuildCount");

					using (var secondContext = CreateDiagnosticContext("SendEmail2"))
					{
						secondContext.Increment("BuildCount");
					}
				}
				firstContext.Increment("SendCount");
			}

			using (var thirdContext = CreateDiagnosticContext("SendEmail"))
			{
				thirdContext.Increment("Build");
				thirdContext.Increment("SendCount");
			}

			var result = metrics.GetData(0);

			Assert.AreEqual(2, ((NewRelicInt64MetricData)result["Component/SendEmail/Counters/BuildCount"]).Total);
			Assert.AreEqual(1, ((NewRelicInt64MetricData)result["Component/SendEmail2/Counters/BuildCount"]).Total);
			Assert.AreEqual(1, ((NewRelicInt64MetricData)result["Component/SendEmail/Counters/Build"]).Total);
			Assert.AreEqual(2, ((NewRelicInt64MetricData)result["Component/SendEmail/Counters/SendCount"]).Total);
		}

		[TestMethod]
		public void ВложенныеМетрики()
		{
			Controller.CurrentDateTimeUtc = new DateTime(2016, 03, 08, 13, 00, 00, DateTimeKind.Utc);

			using (var context = CreateDiagnosticContext("SendEmail"))
			{
				Controller.CurrentDateTimeUtc = new DateTime(2016, 03, 08, 13, 00, 10, DateTimeKind.Utc);

				using (context.Measure("Prepare"))
				{
					Controller.CurrentDateTimeUtc = new DateTime(2016, 03, 08, 13, 00, 11, DateTimeKind.Utc);
				}

				Controller.CurrentDateTimeUtc = new DateTime(2016, 03, 08, 13, 00, 16, DateTimeKind.Utc);

				using (context.Measure("Sending"))
				{
					Controller.CurrentDateTimeUtc = new DateTime(2016, 03, 08, 13, 00, 17, DateTimeKind.Utc);

					using (context.Measure("ConnectingToMta"))
					{
						Controller.CurrentDateTimeUtc = new DateTime(2016, 03, 08, 13, 00, 18, DateTimeKind.Utc);
					}

					Controller.CurrentDateTimeUtc = new DateTime(2016, 03, 08, 13, 00, 19, DateTimeKind.Utc);
				}

				using (context.Measure("Saving"))
				{
					Controller.CurrentDateTimeUtc = new DateTime(2016, 03, 08, 13, 00, 20, DateTimeKind.Utc);

					using (context.Measure("Submit"))
						Controller.CurrentDateTimeUtc = new DateTime(2016, 03, 08, 13, 00, 21, DateTimeKind.Utc);
				}

				using (context.Measure("Saving"))
				{
					Controller.CurrentDateTimeUtc = new DateTime(2016, 03, 08, 13, 00, 22, DateTimeKind.Utc);

					using (context.Measure("Submit"))
						Controller.CurrentDateTimeUtc = new DateTime(2016, 03, 08, 13, 00, 23, DateTimeKind.Utc);
				}
			}

			var result = metrics.GetData(0);

			Assert.AreEqual(2000,
				((NewRelicInt64MetricData)result["Component/SendEmail/ProcessingTime/Saving[ms]"]).Total);
			Assert.AreEqual(2000,
				((NewRelicInt64MetricData)result["Component/SendEmail/ProcessingTime/Saving/Submit[ms]"]).Total);
			Assert.AreEqual(15000, 
				((NewRelicInt64MetricData)result["Component/SendEmail/ProcessingTime/Other[ms]"]).Total);
			Assert.AreEqual(1000, 
				((NewRelicInt64MetricData)result["Component/SendEmail/ProcessingTime/Prepare[ms]"]).Total);
			Assert.AreEqual(2000,
				((NewRelicInt64MetricData)result["Component/SendEmail/ProcessingTime/Sending[ms]"]).Total);
			Assert.AreEqual(1000,
				((NewRelicInt64MetricData)result["Component/SendEmail/ProcessingTime/Sending/ConnectingToMta[ms]"]).Total);
			Assert.AreEqual(23000,
				((NewRelicInt64MetricData)result["Component/SendEmail/TotalProcessingTime[ms]"]).Total);
		}

		[TestMethod]
		public void ДваКонтекста_TotalАгрегируется()
		{
			var baseTime = new DateTime(2016, 03, 08, 13, 00, 00, DateTimeKind.Utc);
			Controller.CurrentDateTimeUtc = baseTime;

			using (var toplevel = CreateDiagnosticContext("level0"))
			{
				Controller.CurrentDateTimeUtc = baseTime + TimeSpan.FromSeconds(2);
				using (toplevel.Measure("level1"))
				{
					Controller.CurrentDateTimeUtc = baseTime + TimeSpan.FromSeconds(5);
					using (toplevel.Measure("level2"))
					{
						Controller.CurrentDateTimeUtc = baseTime + TimeSpan.FromSeconds(10);
					}
				}
			}

			Controller.CurrentDateTimeUtc = baseTime;
			using (var toplevel1 = CreateDiagnosticContext("level0"))
			{
				Controller.CurrentDateTimeUtc = baseTime + TimeSpan.FromSeconds(2);
				using (toplevel1.Measure("level1"))
				{
					Controller.CurrentDateTimeUtc = baseTime + TimeSpan.FromSeconds(5);
					using (toplevel1.Measure("level2"))
					{
						Controller.CurrentDateTimeUtc = baseTime + TimeSpan.FromSeconds(10);
					}
				}
			}

			var result = metrics.GetData(0);

			Assert.AreEqual(20000,
				((NewRelicInt64MetricData)result["Component/level0/TotalProcessingTime[ms]"]).Total);
			Assert.AreEqual(2,
				((NewRelicInt64MetricData)result["Component/level0/TotalProcessingTime[ms]"]).Count);
			Assert.AreEqual(6000,
				((NewRelicInt64MetricData)result["Component/level0/ProcessingTime/level1[ms]"]).Total);
			Assert.AreEqual(2,
				((NewRelicInt64MetricData)result["Component/level0/ProcessingTime/level1[ms]"]).Count);
			Assert.AreEqual(10000,
				((NewRelicInt64MetricData)result["Component/level0/ProcessingTime/level1/level2[ms]"]).Total);
			Assert.AreEqual(2,
				((NewRelicInt64MetricData)result["Component/level0/ProcessingTime/level1/level2[ms]"]).Count);
		}

		[TestMethod]
		public void СвязанныеКонтекстыВремя()
		{
			var baseTime = new DateTime(2016, 03, 08, 13, 00, 00, DateTimeKind.Utc);
			Controller.CurrentDateTimeUtc = baseTime;

			using (var toplevel = CreateDiagnosticContext("level0"))
			{
				Controller.CurrentDateTimeUtc = baseTime + TimeSpan.FromSeconds(2);
				using (toplevel.Measure("level1"))
				{
					Controller.CurrentDateTimeUtc = baseTime + TimeSpan.FromSeconds(5);

					using (toplevel.MeasureForAdditionalMetric(CreateDiagnosticContext("anotherLevel0")))
					using (toplevel.Measure("level2"))
					{
						Controller.CurrentDateTimeUtc = baseTime + TimeSpan.FromSeconds(10);
					}
				}
			}

			var result = metrics.GetData(0);

			Assert.AreEqual(10000,
				((NewRelicInt64MetricData)result["Component/level0/TotalProcessingTime[ms]"]).Total);
			Assert.AreEqual(3000,
				((NewRelicInt64MetricData)result["Component/level0/ProcessingTime/level1[ms]"]).Total);
			Assert.AreEqual(5000,
				((NewRelicInt64MetricData)result["Component/level0/ProcessingTime/level1/level2[ms]"]).Total);
			Assert.AreEqual(5000,
				((NewRelicInt64MetricData)result["Component/anotherLevel0/ProcessingTime/level2[ms]"]).Total);
		}

		[TestMethod]
		public void СвязанныеКонтекстыСчётчики()
		{
			using (var context = CreateDiagnosticContext("level0"))
			{
				context.Increment("FirstCounter");
				using (context.Measure("level1"))
				{
					context.Increment("FirstCounter");

					using (context.MeasureForAdditionalMetric(CreateDiagnosticContext("anotherLevel0")))
					using (context.Measure("level2"))
					{
						context.Increment("SecondCounter");
					}
				}
				context.Increment("ThirdCounter");
			}

			var result = metrics.GetData(0);

			Assert.AreEqual(2,
				((NewRelicInt64MetricData)result["Component/level0/Counters/FirstCounter"]).Total);
			Assert.AreEqual(1,
				((NewRelicInt64MetricData)result["Component/level0/Counters/SecondCounter"]).Total);
			Assert.AreEqual(1,
				((NewRelicInt64MetricData)result["Component/level0/Counters/ThirdCounter"]).Total);

			Assert.AreEqual(1,
				((NewRelicInt64MetricData)result["Component/anotherLevel0/Counters/SecondCounter"]).Total);
		}

		private DiagnosticContext CreateDiagnosticContext(string metricPath)
		{
			return new DiagnosticContext(
				metricPath, 
				Plugin.GetMetrics<NewRelicDiagnosticContextMetricsCollection>(), 
				DefaultMetricTypesConfiguration.Instance.GetStandardMetricsTypes());
		}
	}
}
