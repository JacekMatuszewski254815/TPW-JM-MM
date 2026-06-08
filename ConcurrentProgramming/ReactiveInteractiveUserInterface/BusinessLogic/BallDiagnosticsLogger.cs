using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    public interface IBallDiagnosticsLogger : IDisposable
    {
        void LogBallState(int ballId, double positionX, double positionY, double velocityX, double velocityY, double mass, double radius);
        Task FlushAsync();
        bool IsEnabled { get; set; }
    }

    internal class BallDiagnosticsLogger : IBallDiagnosticsLogger
    {
        private readonly string _filePath;
        private readonly int _bufferSize;
        private readonly Queue<string> _writeQueue = new Queue<string>();
        private readonly object _queueLock = new object();
        private readonly SemaphoreSlim _writeSemaphore;
        private Task _writeTask;
        private bool _isDisposed = false;
        private bool _isEnabled = true;
        private CancellationTokenSource _cancellationTokenSource;

        public bool IsEnabled
        {
            get { lock (_queueLock) return _isEnabled; }
            set { lock (_queueLock) _isEnabled = value; }
        }

        public BallDiagnosticsLogger(string filePath = null, int bufferSize = 100)
        {
            if (filePath == null)
            {
                string binDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string projectFolder = Path.GetFullPath(Path.Combine(binDirectory, "..", "..", ".."));
                filePath = Path.Combine(projectFolder, "ball_diagnostics.log");
            }

            _filePath = filePath;
            _bufferSize = bufferSize;
            _writeSemaphore = new SemaphoreSlim(1, 1);
            _cancellationTokenSource = new CancellationTokenSource();

            InitializeLogFile();
            _writeTask = ProcessWriteQueueAsync(_cancellationTokenSource.Token);
        }

        private void InitializeLogFile()
        {
            try
            {
                string directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string header = SerializeBallStateHeader();
                File.WriteAllText(_filePath, header + Environment.NewLine, Encoding.ASCII);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing log file: {ex.Message}");
            }
        }

        public void LogBallState(int ballId, double positionX, double positionY, 
            double velocityX, double velocityY, double mass, double radius)
        {
            if (!IsEnabled || _isDisposed)
                return;

            try
            {
                lock (_queueLock)
                {
                    if (!_isEnabled)
                        return;

                    string serializedData = SerializeBallState(ballId, positionX, positionY, 
                        velocityX, velocityY, mass, radius);
                    _writeQueue.Enqueue(serializedData);

                    // Obsługa backpressure - jeśli buffer się przepełnia, czekamy na zapis
                    if (_writeQueue.Count >= _bufferSize)
                    {
                        Task.Delay(10).Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging ball state: {ex.Message}");
            }
        }

        private async Task ProcessWriteQueueAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    List<string> itemsToWrite = new List<string>();

                    lock (_queueLock)
                    {
                        while (_writeQueue.Count > 0 && itemsToWrite.Count < _bufferSize / 2)
                        {
                            itemsToWrite.Add(_writeQueue.Dequeue());
                        }
                    }

                    if (itemsToWrite.Count > 0)
                    {
                        await WriteToFileAsync(itemsToWrite);
                    }
                    else
                    {
                        await Task.Delay(50, cancellationToken);
                    }
                }

                // Flush remaining items
                await FlushAsync();
            }
            catch (OperationCanceledException)
            {
                // Expected when shutting down
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in write queue processor: {ex.Message}");
            }
        }

        private async Task WriteToFileAsync(List<string> items)
        {
            try
            {
                await _writeSemaphore.WaitAsync();
                try
                {
                    using (FileStream fs = new FileStream(_filePath, FileMode.Append, 
                        FileAccess.Write, FileShare.Read, 4096, useAsync: true))
                    {
                        using (StreamWriter writer = new StreamWriter(fs, Encoding.ASCII))
                        {
                            foreach (var item in items)
                            {
                                await writer.WriteLineAsync(item);
                            }
                            await writer.FlushAsync();
                        }
                    }
                }
                finally
                {
                    _writeSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error writing to file: {ex.Message}");
            }
        }

        public async Task FlushAsync()
        {
            if (_isDisposed)
                return;

            List<string> itemsToWrite = new List<string>();

            lock (_queueLock)
            {
                while (_writeQueue.Count > 0)
                {
                    itemsToWrite.Add(_writeQueue.Dequeue());
                }
            }

            if (itemsToWrite.Count > 0)
            {
                await WriteToFileAsync(itemsToWrite);
            }
        }

        private string SerializeBallStateHeader()
        {
            return "Timestamp|BallId|PositionX|PositionY|VelocityX|VelocityY|Mass|Radius";
        }

        private string SerializeBallState(int ballId, double positionX, double positionY,
            double velocityX, double velocityY, double mass, double radius)
        {
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            return $"{timestamp}|{ballId}|{positionX:F4}|{positionY:F4}|{velocityX:F4}|{velocityY:F4}|{mass:F4}|{radius:F4}";
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            IsEnabled = false;

            try
            {
                _cancellationTokenSource.Cancel();
                _writeTask?.Wait(TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during disposal: {ex.Message}");
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _writeSemaphore?.Dispose();
            }
        }
    }
}
