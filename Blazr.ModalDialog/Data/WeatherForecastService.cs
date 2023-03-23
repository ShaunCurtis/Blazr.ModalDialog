namespace Blazr.ModalDialog.Data
{
    public class WeatherForecastService
    {
        public readonly List<string> Summaries = new() {"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        
        private List<WeatherForecast>? _forecasts;

        public async ValueTask<IEnumerable<WeatherForecast>> GetForecastAsync()
        {
            // Mock an async data pipeline call
            await Task.Yield();

            _forecasts = _forecasts ?? GetForecasts().ToList();

            return _forecasts.OrderBy(item => item.Date);
        }

        public async ValueTask<WeatherForecast?> GetForecastAsync(Guid uid)
        {
            // Mock an async data pipeline call
            await Task.Yield();

            return _forecasts?.SingleOrDefault(item => item.Uid == uid);
        }

        public async ValueTask<bool> SaveForecastAsync(WeatherForecast record)
        {
            // Mock an async data pipeline call
            await Task.Yield();
            
            ArgumentNullException.ThrowIfNull(_forecasts);
            
            var forecast = _forecasts.SingleOrDefault(item => item.Uid == record.Uid);
            
            if (forecast is not null)
                _forecasts.Remove(forecast);

            if(record.Uid  == Guid.Empty)
                record.Uid = Guid.NewGuid();
            
            _forecasts.Add(record);

            return true;
        }

        private IEnumerable<WeatherForecast> GetForecasts()
        {
            var startDate = DateOnly.FromDateTime(DateTime.Now);
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Uid = Guid.NewGuid(),
                Date = startDate.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Count)]
            });
        }
    }
}