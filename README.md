# Mindbox.DiagnosticContext

![](https://github.com/mindbox-moscow/Mindbox.DiagnosticContext/workflows/master/badge.svg)

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
  .AddPrometheusDiagnosticContext();
```

If your application doesn't yet expose prometheus metrics, add the following code to your `Startup` class or use the [prometheus-net documentation](https://github.com/prometheus-net/prometheus-net) to instrument your code: 

```csharp
app.UseMetricServer();
```

### Using diagnostic context + prometheus in .Net Framework

1. [Register a factory with the desired settings](https://github.com/mindbox-moscow/DirectCRM/blob/51c6a6e418afd4a696b0f68998aaf9fa46056f62/Product/DirectCrm/DirectCrm.Core/DirectCrmCoreModule.cs#L177-L204).
1. [Create a DiagnosticContext instance](https://github.com/mindbox-moscow/DirectCRM/blob/b16aca860a6c5c6d16c806c915f24af7a2703106/Product/DirectCrm/Mailings/Mailings.Model/BulkOperation/MailingBulkSendingOperation.cs#L44-L49)
1. Use the generated DiagnosticContext

### Using the diagnostic context in DirectCRM

IDiagnosticContextFactory [already registered in DirectCrmCoreModule](https://github.com/mindbox-moscow/DirectCRM/blob/51c6a6e418afd4a696b0f68998aaf9fa46056f62/Product/DirectCrm/DirectCrm.Core/DirectCrmCoreModule.cs#L177-L204).

Metrics are sent to [special prometheus](https://kube-infra.mindbox.ru/common-dc/prometheus/).

[Eexample of creating a DiagnosticContext for a ModelContext](https://github.com/mindbox-moscow/DirectCRM/blob/b16aca860a6c5c6d16c806c915f24af7a2703106/Product/DirectCrm/Mailings/Mailings.Model/BulkOperation/MailingBulkSendingOperation.cs#L44-L49).

To use an external DiagnosticContext, you need to use `IDiagnosticContextFactory` and create an instance of IDiagnosticContext. Remember to dispose it.

#### The nuances that arise when transferring dashboards from NewRelic to Grafana

Metrics have a prefix: `diagnosticcontext` and a postfix:` projectSystemName` (with the removal of all invalid for the metric name characters).
The name of the project, when creating a dashboard, must be taken from the postfix. An example is in the [mailing dashboard](https://grafana.mindbox.ru/d/uWOO6yjGk/mailings-dc?editview=templating&orgId=1&from=now-15m&to=now&refresh=5s) variables.

The increment of the counter for a NewRelic looks like `diagnosticContext.Increment("counter_name[message]")`. Do not do this, do `diagnosticContext.Increment("counter_name")`. The name of the counter will end up in the `name` label.
