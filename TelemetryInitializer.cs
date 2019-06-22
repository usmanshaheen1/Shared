using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Api
{

    public class HttpContextItemsTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        public HttpContextItemsTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            var context = httpContextAccessor.HttpContext;
            if (context != null && telemetry.GetType() == typeof(RequestTelemetry)) // log message Body for Request telemetry only
            {
                foreach (var item in context.Items)
                {
                    var itemKey = item.Key.ToString();
                    
                    if (!telemetry.Context.GlobalProperties.ContainsKey(itemKey))
                    {
                        telemetry.Context.GlobalProperties.Add(itemKey, item.Value.ToString());
                    }
                }
            }
        }
    }
      
    }

