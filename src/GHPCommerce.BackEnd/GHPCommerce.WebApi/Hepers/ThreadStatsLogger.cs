using System;
using System.Threading;
using System.Timers;
using Serilog;
using Timer = System.Timers.Timer;

namespace GHPCommerce.WebApi.Hepers
{
    public class ThreadStatsLogger : IDisposable
    {
        private const int DEPLETION_WARN_LEVEL = 10;
        private const int HISTERESIS_LEVEL = 10;

        private const double SAMPLE_RATE_MILLISECONDS = 500;
        private bool _workerThreadWarned = false;
        private bool _ioThreadWarned = false;
        private bool _minWorkerThreadLevelWarned = false;
        private bool _minIoThreadLevelWarned = false;

        private readonly int _maxWorkerThreadLevel;
        private readonly int _maxIoThreadLevel;
        private readonly int _minWorkerThreadLevel;
        private readonly int _minWorkerThreadLevelRecovery;
        private readonly int _minIoThreadLevel;
        private readonly int _minIoThreadLevelRecovery;
        private Timer _timer;

        Serilog.Core.Logger _logger = new LoggerConfiguration()
            .WriteTo.File("httprequest_log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();


        public ThreadStatsLogger()
        {

            _timer = new Timer
            {
                AutoReset = true,
                Interval = SAMPLE_RATE_MILLISECONDS,
            };

            _timer.Elapsed += TimerElapsed;
            _timer.Start();
            ThreadPool.GetMinThreads(out _minWorkerThreadLevel, out _minIoThreadLevel);
            ThreadPool.GetMaxThreads(out _maxWorkerThreadLevel, out _maxIoThreadLevel);
            ThreadPool.GetAvailableThreads(out int workerAvailable, out int ioAvailable);

            _logger.Information("Thread statistics at startup: minimum worker:{0} io:{1}", _minWorkerThreadLevel,
                _minIoThreadLevel);
            _logger.Information("Thread statistics at startup: maximum worker:{0} io:{1}", _maxWorkerThreadLevel,
                _maxIoThreadLevel);
            _logger.Information("Thread statistics at startup: available worker:{0} io:{1}", workerAvailable,
                ioAvailable);

            _minWorkerThreadLevelRecovery = (_minWorkerThreadLevel * 3) / 4;
            _minIoThreadLevelRecovery = (_minIoThreadLevel * 3) / 4;
            if (_minWorkerThreadLevelRecovery == _minWorkerThreadLevel)
                _minWorkerThreadLevelRecovery = _minWorkerThreadLevel - 1;
            if (_minIoThreadLevelRecovery == _minIoThreadLevel) _minIoThreadLevelRecovery = _minIoThreadLevel - 1;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {

            ThreadPool.GetAvailableThreads(out int availableWorkerThreads, out int availableIoThreads);

            var activeWorkerThreads = _maxWorkerThreadLevel - availableWorkerThreads;
            var activeIoThreads = _maxIoThreadLevel - availableIoThreads;

            _logger.Information("Thread statistics: active worker:{0} io:{1}", activeWorkerThreads, activeIoThreads);

            if (activeWorkerThreads > _minWorkerThreadLevel && !_minWorkerThreadLevelWarned)
            {
                _logger.Information("Thread statistics WARN active worker threads above minimum {0}:{1}",
                    activeWorkerThreads, _minWorkerThreadLevel);
                _minWorkerThreadLevelWarned = !_minWorkerThreadLevelWarned;
            }

            if (activeWorkerThreads < _minWorkerThreadLevelRecovery && _minWorkerThreadLevelWarned)
            {
                _logger.Information("Thread statistics RECOVERY active worker threads below minimum {0}:{1}",
                    activeWorkerThreads, _minWorkerThreadLevel);
                _minWorkerThreadLevelWarned = !_minWorkerThreadLevelWarned;
            }

            if (activeIoThreads > _minIoThreadLevel && !_minIoThreadLevelWarned)
            {
                _logger.Information("Thread statistics WARN active io threads above minimum {0}:{1}", activeIoThreads,
                    _minIoThreadLevel);
                _minIoThreadLevelWarned = !_minIoThreadLevelWarned;
            }

            if (activeIoThreads < _minIoThreadLevelRecovery && _minIoThreadLevelWarned)
            {
                _logger.Information("Thread statistics RECOVERY active io threads below minimum {0}:{1}",
                    activeIoThreads, _minIoThreadLevel);
                _minIoThreadLevelWarned = !_minIoThreadLevelWarned;
            }

            if (availableWorkerThreads < DEPLETION_WARN_LEVEL && !_workerThreadWarned)
            {
                _logger.Information("Thread statistics WARN available worker threads below warning level {0}:{1}",
                    availableWorkerThreads, DEPLETION_WARN_LEVEL);
                _workerThreadWarned = !_workerThreadWarned;
            }

            if (availableWorkerThreads > (DEPLETION_WARN_LEVEL + HISTERESIS_LEVEL) && _workerThreadWarned)
            {
                _logger.Information("Thread statistics RECOVERY available worker thread recovery {0}:{1}",
                    availableWorkerThreads, DEPLETION_WARN_LEVEL);
                _workerThreadWarned = !_workerThreadWarned;
            }

            if (availableIoThreads < DEPLETION_WARN_LEVEL && !_ioThreadWarned)
            {
                _logger.Information("Thread statistics WARN available io threads below warning level {0}:{1}",
                    availableIoThreads, DEPLETION_WARN_LEVEL);
                _ioThreadWarned = !_ioThreadWarned;
            }

            if (availableIoThreads > (DEPLETION_WARN_LEVEL + HISTERESIS_LEVEL) && _ioThreadWarned)
            {
                _logger.Information("Thread statistics RECOVERY available io thread recovery {0}:{1}",
                    availableIoThreads, DEPLETION_WARN_LEVEL);
                _ioThreadWarned = !_ioThreadWarned;
            }
        }

        public void Dispose()
        {
            if (_timer == null) return;
            _timer.Close();
            _timer.Dispose();
            _timer = null;
        }
    }
}