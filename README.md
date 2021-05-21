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

### Использование diagnostic context + prometheus в .Net Framework

1. [Зарегистрировать фабрику с нужными настройками](https://github.com/mindbox-moscow/DirectCRM/blob/51c6a6e418afd4a696b0f68998aaf9fa46056f62/Product/DirectCrm/DirectCrm.Core/DirectCrmCoreModule.cs#L177-L204).
1. [Создать инстанс DiagnosticContext](https://github.com/mindbox-moscow/DirectCRM/blob/b16aca860a6c5c6d16c806c915f24af7a2703106/Product/DirectCrm/Mailings/Mailings.Model/BulkOperation/MailingBulkSendingOperation.cs#L44-L49)
1. Использовать созданный DiagnosticContext как обычно - он имеет тот же интерфейс.

### Использование diagnostic context в DirectCRM

IDiagnosticContextFactory [уже зарегистрирована]((https://github.com/mindbox-moscow/DirectCRM/blob/51c6a6e418afd4a696b0f68998aaf9fa46056f62/Product/DirectCrm/DirectCrm.Core/DirectCrmCoreModule.cs#L177-L204).

Метрики отдаются в [специальный прометей](https://kube-infra.mindbox.ru/common-dc/prometheus/).

[Пример с созданием DiagnosticContext для ModelContext](https://github.com/mindbox-moscow/DirectCRM/blob/b16aca860a6c5c6d16c806c915f24af7a2703106/Product/DirectCrm/Mailings/Mailings.Model/BulkOperation/MailingBulkSendingOperation.cs#L44-L49).

Для использования внешнего DiagnosticContext нужно воспользоваться `IDiagnosticContextFactory` и создать инстанс IDiagnosticContext, не забыв его задиспоузить.

#### Нюансы, которые возникнут при переносе дашбордов из ньюрелика в графану

Метрики имеют префикс: `diagnosticcontext` и постфикс: `projectSystemName` (с удалением всех невалидных для имени метрики символов).
Имя проекта, при создании дашборда, нужно брать из постфикса. Пример есть в переменных [дашборда рассылок](https://grafana.mindbox.ru/d/uWOO6yjGk/mailings-dc?editview=templating&orgId=1&from=now-15m&to=now&refresh=5s).

Увеличение каунтера для ньюрелика имеет вид `diagnosticContext.Increment("counter_name[message]")`. Так делать не надо, надо делать `diagnosticContext.Increment("counter_name")`. Имя каунтера попадет в лейбл `name`.
