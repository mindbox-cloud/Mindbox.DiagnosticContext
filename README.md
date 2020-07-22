# Mindbox.DiagnosticContext

## Setting up DiagnosticContext factory

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

## Adding DiagnosticContext to a Asp.Net Core app

Add a reference to `Mindbox.DiagnosticContext.AspNetCore` package. This will allow you to instrument your action methods with diagnostic context:

```csharp
[UseDiagnosticContext("order")]
public ActionResult CreateOrder()
{ 
  var diagnosticContext = HttpContext.GetDiagnosticContext();
  // ...   
}
```

