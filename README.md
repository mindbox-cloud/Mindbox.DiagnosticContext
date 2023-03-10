# Mindbox.DiagnosticContext

![](https://github.com/mindbox-cloud/Mindbox.DiagnosticContext/workflows/publish/badge.svg)

## Adding diagnostic context to a Asp.Net Core app

Add a reference to `Mindbox.DiagnosticContext.AspNetCore` package. This will allow you to instrument your action methods with diagnostic context:

```csharp
[UseDiagnosticContext("order")]
public ActionResult CreateOrder()
{ 
  var diagnosticContext = HttpContext.GetDiagnosticContext();
  // ...   
}
```

## Setting up diagnostic context factory

You must setup dependency injection in order to use diagnostic context. `IDiagnosticContextFactory` implementation controls what type of diagnostic context gets created and how the collected metrics are exposed. 

It is recommended to use one of the provided implementations (see below), but any `IDiagnosticContextFactory` service registration will suffice.

### Using prometheus to expose the metrics

Add a reference to `Mindbox.DiagnosticContext.Prometheus` package. Then, add this code to your service configuration:

```csharp
services
  .AddPrometheusDiagnosticContext(orders);
```
It is strongly recommended to use a unique prefix that includes the name of the application - this can guarantee that there is no intersection of metrics.

If your application doesn't yet expose prometheus metrics, add the following code to your `Startup` class or use the [prometheus-net documentation](https://github.com/prometheus-net/prometheus-net) to instrument your code: 

```csharp
app.UseMetricServer();
```

### Using diagnostic context + prometheus in .Net Framework

1. [Register a factory with the desired settings](https://github.com/mindbox-cloud/DirectCRM/blob/51c6a6e418afd4a696b0f68998aaf9fa46056f62/Product/DirectCrm/DirectCrm.Core/DirectCrmCoreModule.cs#L177-L204).
1. [Create a DiagnosticContext instance](https://github.com/mindbox-cloud/DirectCRM/blob/b16aca860a6c5c6d16c806c915f24af7a2703106/Product/DirectCrm/Mailings/Mailings.Model/BulkOperation/MailingBulkSendingOperation.cs#L44-L49)
1. Use the generated DiagnosticContext

### Using the diagnostic context in DirectCRM

IDiagnosticContextFactory [already registered in DirectCrmCoreModule](https://github.com/mindbox-cloud/DirectCRM/blob/51c6a6e418afd4a696b0f68998aaf9fa46056f62/Product/DirectCrm/DirectCrm.Core/DirectCrmCoreModule.cs#L177-L204), just use it.

Metrics are sent to [special prometheus](https://kube-infra.mindbox.ru/common-dc/prometheus/), here you can see the values of metrics and build queries.

[Eexample of creating a DiagnosticContext for a ModelContext](https://github.com/mindbox-cloud/DirectCRM/blob/b16aca860a6c5c6d16c806c915f24af7a2703106/Product/DirectCrm/Mailings/Mailings.Model/BulkOperation/MailingBulkSendingOperation.cs#L44-L49).

To use an external DiagnosticContext, you need to use `IDiagnosticContextFactory` and create an instance of IDiagnosticContext. Remember to dispose it.

### Things to know when switching from Relic to Prometheus in DirectCRM

* Prometheus uses metrics in conjunction with labels. By labels, using promql, you can conveniently group (`sum(some_metric) by (lable)`).

* When creating a `DiagnosticContext`, you must specify the name of the metric. If it contains characters invalid for the metric, they will be removed. You can read about which characters are invalid [here](https://prometheus.io/docs/concepts/data_model/).

* To split the dashboard by projects, the name of the project must be selected from the name of the metric using the query and regex in Grafana. An example of a dashboard using such metrics, broken down by project, [here](https://grafana.mindbox.ru/d/uWOO6yjGk/mailings-dc?editview=templating&orgId=1&from=now-15m&to=now&refresh=5s).

* When creating a `DiagnosticContext` using `IDiagnosticContextFactory`, the name of the metric being written is `diagnosticcontext_{metricName}_metric_projectSystemName`.
In other words, several metrics are collected at once and `metric` can be: `processingtime`, `cpuprocessingtime`, `counters`, etc.

Example:
```csharp
...
using var diagnosticContext = diagnosticContextFactory.CreateDiagnosticContext("metric_name");
using diagnosticContext.Measure("some_step");
...
```
The final metric name will look like: `diagnosticcontext_metric_name_[metric]_projectSystemName`.
For example, if we want to build a pie based on the time spent, then we need to use the metric: `diagnosticcontext_metric_name_processingtime_projectSystemName`.
The names of the steps will be recorded on the labels. The example shows one step: `some_step` - it will go to the label `step`.

* Prometheus, unlike Relic, has a set of counters that differ in the name label.
This metric is named: `diagnosticcontext_metricName_counters_projectSystemName`. In other words, `counters` is appended to the metric name specified when the `DiagnosticContext` was created.
If you need to find out the value of a specific counter, you need to make the following request: `diagnosticcontext_metricName_counters_projectSystemName{name=~"counter_name"}`.

Example:
```csharp
using var diagnosticContext = diagnosticContextFactory.CreateDiagnosticContext("metric_name");
using diagnosticContext.Increment("some_counter");
```
The final query for this counter will be as follows: `diagnosticcontext_metric_name_counters_projectSystemName{name=~"some_counter"}`.
