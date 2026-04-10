using FluentAssertions.Equivalency;
using System.Diagnostics.Metrics;

namespace API_sprot_training_program.Metrics
{
    public class DataBaseRequestTime
    {
        private readonly Counter<double> _counter;
        public DataBaseRequestTime(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create("data_base_request_time", "1.0.0");
            _counter = meter.CreateCounter<double>(
                name: "request-time",
                unit: "seconds",
                description: "The number of seconds to access database");
        }

        public void add_to_counter(double value)
        {
            _counter.Add(value);
        }
    }
}
