using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using UFRI.FrameWork;
namespace Service.JSlogger
{
    /// <summary>
    /// 로그 레벨 열거형
    /// </summary>
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Critical = 5
    }

    /// <summary>
    /// 통합 로그 관리 클래스
    /// </summary>
    public class LogManager
    {
        #region 싱글톤 패턴 구현
        private static LogManager _instance;
        private static readonly object _lock = new object();

        public static LogManager GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new LogManager();
                    }
                }
            }
            return _instance;
        }

        private LogManager()
        {
            // 기본 설정 초기화
            _logToFile = true;
            _logToConsole = true;
            _logToUI = true;
            _minFileLogLevel = LogLevel.Info;
            _minConsoleLogLevel = LogLevel.Debug;
            _minUILogLevel = LogLevel.Info;
            _maxLogEntries = 1000;
            _maxLogFileSize = 10 * 1024 * 1024; // 10MB
            _maxLogFiles = 10;
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            _logFileName = "ServiceDataCollect_{0}.log";
            _logFileNameFormat = "yyyyMMdd";

            // 로그 디렉토리 생성
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }
        #endregion

        #region 필드 및 속성
        private ListBox _listBox;
        private bool _logToFile = true;
        private bool _logToConsole = true;
        private bool _logToUI = true;
        private LogLevel _minFileLogLevel = LogLevel.Info;
        private LogLevel _minConsoleLogLevel = LogLevel.Debug;
        private LogLevel _minUILogLevel = LogLevel.Info;
        private int _maxLogEntries = 1000;
        private long _maxLogFileSize = 10 * 1024 * 1024; // 10MB
        private int _maxLogFiles = 10;
        private string _logDirectory;
        private string _logFileName;
        private string _logFileNameFormat;
        private string _currentLogFile;
        private string _moduleName;

        // 속성
        public LogLevel MinFileLogLevel
        {
            get { return _minFileLogLevel; }
            set { _minFileLogLevel = value; }
        }

        public LogLevel MinUILogLevel
        {
            get { return _minUILogLevel; }
            set { _minUILogLevel = value; }
        }

        public bool LogToFile
        {
            get { return _logToFile; }
            set { _logToFile = value; }
        }

        public bool LogToUI
        {
            get { return _logToUI; }
            set { _logToUI = value; }
        }

        public string ModuleName
        {
            get { return _moduleName; }
            set { _moduleName = value; }
        }
        #endregion

        #region 초기화 메서드
        /// <summary>
        /// 로그 관리자 초기화
        /// </summary>
        /// <param name="listBox">로그를 표시할 ListBox 컨트롤</param>
        /// <param name="configPath">로그 설정 파일 경로</param>
        /// <param name="moduleName">모듈 이름</param>
        public void Initialize(ListBox listBox, string configPath, string moduleName)
        {
            _listBox = listBox;
            _moduleName = moduleName;

            // GMLogManager 설정 (기존 로그 시스템과의 호환성 유지)
            GMLogManager.ConfigureLogger(configPath);

            // 설정 파일에서 로그 설정 로드
            LoadLogConfiguration(configPath);

            // 현재 로그 파일 설정
            UpdateCurrentLogFile();

            // 초기화 완료 로그
            Info($"{moduleName} 로그 시스템이 초기화되었습니다.");
        }

        /// <summary>
        /// 설정 파일에서 로그 설정 로드
        /// </summary>
        /// <param name="configPath">설정 파일 경로</param>
        private void LoadLogConfiguration(string configPath)
        {
            try
            {
                if (File.Exists(configPath))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(configPath);

                    // 로그 레벨 설정
                    XmlNode levelNode = doc.SelectSingleNode("//log4net/root/level");
                    if (levelNode != null && levelNode.Attributes["value"] != null)
                    {
                        string levelValue = levelNode.Attributes["value"].Value;
                        switch (levelValue.ToUpper())
                        {
                            case "DEBUG": _minFileLogLevel = LogLevel.Debug; break;
                            case "INFO": _minFileLogLevel = LogLevel.Info; break;
                            case "WARN": _minFileLogLevel = LogLevel.Warning; break;
                            case "ERROR": _minFileLogLevel = LogLevel.Error; break;
                            case "FATAL": _minFileLogLevel = LogLevel.Critical; break;
                        }
                    }

                    // 로그 파일 설정
                    XmlNode fileNode = doc.SelectSingleNode("//log4net/appender[@name='RollingFileAppender']/file");
                    if (fileNode != null && fileNode.Attributes["value"] != null)
                    {
                        string filePath = fileNode.Attributes["value"].Value;
                        _logDirectory = Path.GetDirectoryName(filePath);
                        _logFileName = Path.GetFileName(filePath);
                    }

                    // 최대 파일 크기 설정
                    XmlNode maxSizeNode = doc.SelectSingleNode("//log4net/appender[@name='RollingFileAppender']/maximumFileSize");
                    if (maxSizeNode != null && maxSizeNode.Attributes["value"] != null)
                    {
                        string maxSize = maxSizeNode.Attributes["value"].Value;
                        if (maxSize.EndsWith("KB"))
                        {
                            _maxLogFileSize = long.Parse(maxSize.Replace("KB", "")) * 1024;
                        }
                        else if (maxSize.EndsWith("MB"))
                        {
                            _maxLogFileSize = long.Parse(maxSize.Replace("MB", "")) * 1024 * 1024;
                        }
                    }

                    // 최대 파일 수 설정
                    XmlNode maxFilesNode = doc.SelectSingleNode("//log4net/appender[@name='RollingFileAppender']/maxSizeRollBackups");
                    if (maxFilesNode != null && maxFilesNode.Attributes["value"] != null)
                    {
                        _maxLogFiles = int.Parse(maxFilesNode.Attributes["value"].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"로그 설정 로드 중 오류 발생: {ex.Message}");
                // 기본 설정 사용
            }
        }

        /// <summary>
        /// 현재 로그 파일 경로 업데이트
        /// </summary>
        private void UpdateCurrentLogFile()
        {
            string formattedDate = DateTime.Now.ToString(_logFileNameFormat);
            string fileName = string.Format(_logFileName, formattedDate);
            _currentLogFile = Path.Combine(_logDirectory, fileName);
        }
        #endregion

        #region 로그 메서드
        /// <summary>
        /// 로그 메시지 기록
        /// </summary>
        /// <param name="message">로그 메시지</param>
        /// <param name="level">로그 레벨</param>
        /// <param name="category">로그 카테고리</param>
        public void Log(string message, LogLevel level = LogLevel.Info, string category = "")
        {
            // 현재 날짜가 변경되었는지 확인하고 로그 파일 업데이트
            UpdateCurrentLogFile();

            // 로그 메시지 포맷팅
            string formattedMessage = FormatLogMessage(message, level, category);

            // 파일에 로그 기록
            if (_logToFile && level >= _minFileLogLevel)
            {
                WriteToFile(formattedMessage, level);
            }

            // 콘솔에 로그 출력
            if (_logToConsole && level >= _minConsoleLogLevel)
            {
                WriteToConsole(formattedMessage, level);
            }

            // UI에 로그 표시
            if (_logToUI && level >= _minUILogLevel && _listBox != null)
            {
                WriteToUI(formattedMessage);
            }
        }

        /// <summary>
        /// 로그 메시지 포맷팅
        /// </summary>
        private string FormatLogMessage(string message, LogLevel level, string category)
        {
            StringBuilder sb = new StringBuilder();

            // 날짜 및 시간
            sb.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] ");

            // 로그 레벨
            sb.Append($"[{level.ToString().ToUpper()}] ");

            // 모듈 이름
            if (!string.IsNullOrEmpty(_moduleName))
            {
                sb.Append($"[{_moduleName}] ");
            }

            // 카테고리
            if (!string.IsNullOrEmpty(category))
            {
                sb.Append($"[{category}] ");
            }

            // 메시지
            sb.Append(message);

            return sb.ToString();
        }

        /// <summary>
        /// 파일에 로그 기록
        /// </summary>
        private void WriteToFile(string message, LogLevel level)
        {
            try
            {
                // 로그 파일 크기 확인 및 로그 회전
                CheckLogFileSize();

                // 파일에 로그 추가
                using (StreamWriter writer = new StreamWriter(_currentLogFile, true, Encoding.UTF8))
                {
                    writer.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                // 로그 파일 쓰기 실패 시 콘솔에 오류 출력
                Console.WriteLine($"로그 파일 쓰기 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 콘솔에 로그 출력
        /// </summary>
        private void WriteToConsole(string message, LogLevel level)
        {
            // 로그 레벨에 따라 콘솔 색상 설정
            ConsoleColor originalColor = Console.ForegroundColor;
            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
            }

            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }

        /// <summary>
        /// UI에 로그 표시
        /// </summary>
        private void WriteToUI(string message)
        {
            if (_listBox.InvokeRequired)
            {
                _listBox.Invoke(new Action(() => UpdateListBox(message)));
            }
            else
            {
                UpdateListBox(message);
            }
        }

        /// <summary>
        /// ListBox 업데이트
        /// </summary>
        private void UpdateListBox(string message)
        {
            // 리스트박스가 비어있고 "표시할 데이터가 없습니다" 메시지가 있으면 제거
            if (_listBox.Items.Count == 1 && _listBox.Items[0].ToString().Contains("표시할 데이터가 없습니다"))
            {
                _listBox.Items.Clear();
            }

            // 새 메시지 추가
            _listBox.Items.Add(message);

            // 자동 스크롤을 위해 마지막 항목 선택
            _listBox.SelectedIndex = _listBox.Items.Count - 1;
            _listBox.ClearSelected();

            // 최대 로그 항목 수 제한
            while (_listBox.Items.Count > _maxLogEntries)
            {
                _listBox.Items.RemoveAt(0);
            }
        }

        /// <summary>
        /// 로그 파일 크기 확인 및 로그 회전
        /// </summary>
        private void CheckLogFileSize()
        {
            try
            {
                if (File.Exists(_currentLogFile))
                {
                    FileInfo fileInfo = new FileInfo(_currentLogFile);
                    if (fileInfo.Length >= _maxLogFileSize)
                    {
                        RotateLogFiles();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"로그 파일 크기 확인 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 로그 파일 회전 중 예외 발생 시 처리
        /// </summary>
        private void RotateLogFiles()
        {
            try
            {
                // 기존 백업 파일 이동
                for (int i = _maxLogFiles - 1; i >= 1; i--)
                {
                    string sourceFile = $"{_currentLogFile}.{i}";
                    string destFile = $"{_currentLogFile}.{i + 1}";

                    if (File.Exists(sourceFile))
                    {
                        if (File.Exists(destFile))
                        {
                            File.Delete(destFile);
                        }
                        File.Move(sourceFile, destFile);
                    }
                }

                // 현재 로그 파일을 백업
                string backupFile = $"{_currentLogFile}.1";
                if (File.Exists(backupFile))
                {
                    File.Delete(backupFile);
                }
                File.Move(_currentLogFile, backupFile);

                // 오래된 로그 파일 삭제
                string oldestFile = $"{_currentLogFile}.{_maxLogFiles}";
                if (File.Exists(oldestFile))
                {
                    File.Delete(oldestFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"로그 파일 회전 중 오류 발생: {ex.Message}");
            }
        }

        /// <summary>
        /// 로그 디렉토리 정리 (오래된 로그 파일 삭제)
        /// </summary>
        public void CleanupLogDirectory(int daysToKeep = 30)
        {
            try
            {
                if (!Directory.Exists(_logDirectory))
                    return;

                // 로그 디렉토리의 모든 파일 가져오기
                DirectoryInfo di = new DirectoryInfo(_logDirectory);
                FileInfo[] logFiles = di.GetFiles("*.log*");

                // 현재 날짜 기준으로 daysToKeep일 이전의 파일 삭제
                DateTime cutoffDate = DateTime.Now.AddDays(-daysToKeep);

                foreach (FileInfo file in logFiles)
                {
                    if (file.CreationTime < cutoffDate)
                    {
                        try
                        {
                            file.Delete();
                            Console.WriteLine($"오래된 로그 파일 삭제: {file.Name}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"파일 삭제 중 오류 발생: {file.Name}, {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"로그 디렉토리 정리 중 오류 발생: {ex.Message}");
            }
        }

        /// <summary>
        /// 로그 필터 설정
        /// </summary>
        public void SetLogLevels(LogLevel fileLevel, LogLevel uiLevel)
        {
            _minFileLogLevel = fileLevel;
            _minUILogLevel = uiLevel;
        }

        /// <summary>
        /// 로그 출력 대상 설정
        /// </summary>
        public void SetLogTargets(bool logToFile, bool logToUI, bool logToConsole)
        {
            _logToFile = logToFile;
            _logToUI = logToUI;
            _logToConsole = logToConsole;
        }

        /// <summary>
        /// 로그 파일 설정 변경
        /// </summary>
        public void SetLogFileSettings(string directory, string fileNameFormat, string dateFormat, long maxSize, int maxFiles)
        {
            _logDirectory = directory;
            _logFileName = fileNameFormat;
            _logFileNameFormat = dateFormat;
            _maxLogFileSize = maxSize;
            _maxLogFiles = maxFiles;

            // 디렉토리가 없으면 생성
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            // 현재 로그 파일 경로 업데이트
            UpdateCurrentLogFile();
        }

        /// <summary>
        /// 로그 통계 정보 가져오기
        /// </summary>
        public Dictionary<string, object> GetLogStatistics()
        {
            Dictionary<string, object> stats = new Dictionary<string, object>();

            try
            {
                if (File.Exists(_currentLogFile))
                {
                    FileInfo fi = new FileInfo(_currentLogFile);
                    stats["CurrentLogFile"] = _currentLogFile;
                    stats["CurrentLogFileSize"] = fi.Length;
                    stats["CurrentLogFileCreationTime"] = fi.CreationTime;
                    stats["CurrentLogFileLastWriteTime"] = fi.LastWriteTime;
                }

                DirectoryInfo di = new DirectoryInfo(_logDirectory);
                if (di.Exists)
                {
                    FileInfo[] logFiles = di.GetFiles("*.log*");
                    stats["TotalLogFiles"] = logFiles.Length;
                    stats["TotalLogSize"] = logFiles.Sum(f => f.Length);
                    stats["OldestLogFile"] = logFiles.OrderBy(f => f.CreationTime).FirstOrDefault()?.Name ?? "None";
                    stats["NewestLogFile"] = logFiles.OrderByDescending(f => f.CreationTime).FirstOrDefault()?.Name ?? "None";
                }

                stats["LogToFile"] = _logToFile;
                stats["LogToUI"] = _logToUI;
                stats["LogToConsole"] = _logToConsole;
                stats["MinFileLogLevel"] = _minFileLogLevel.ToString();
                stats["MinUILogLevel"] = _minUILogLevel.ToString();
                stats["MaxLogEntries"] = _maxLogEntries;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"로그 통계 정보 수집 중 오류 발생: {ex.Message}");
            }

            return stats;
        }

        #region 편의 메서드
        /// <summary>
        /// 추적 레벨 로그 기록
        /// </summary>
        public void Trace(string message, string category = "")
        {
            Log(message, LogLevel.Trace, category);
        }

        /// <summary>
        /// 디버그 레벨 로그 기록
        /// </summary>
        public void Debug(string message, string category = "")
        {
            Log(message, LogLevel.Debug, category);
        }

        /// <summary>
        /// 정보 레벨 로그 기록
        /// </summary>
        public void Info(string message, string category = "")
        {
            Log(message, LogLevel.Info, category);
        }

        /// <summary>
        /// 경고 레벨 로그 기록
        /// </summary>
        public void Warning(string message, string category = "")
        {
            Log(message, LogLevel.Warning, category);
        }

        /// <summary>
        /// 오류 레벨 로그 기록
        /// </summary>
        public void Error(string message, string category = "")
        {
            Log(message, LogLevel.Error, category);
        }

        /// <summary>
        /// 심각한 오류 레벨 로그 기록
        /// </summary>
        public void Critical(string message, string category = "")
        {
            Log(message, LogLevel.Critical, category);
        }

        /// <summary>
        /// 예외 정보를 로그에 기록
        /// </summary>
        public void LogException(Exception ex, string additionalInfo = "", LogLevel level = LogLevel.Error, string category = "")
        {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(additionalInfo))
            {
                sb.AppendLine(additionalInfo);
            }

            sb.AppendLine($"예외 유형: {ex.GetType().Name}");
            sb.AppendLine($"메시지: {ex.Message}");
            sb.AppendLine($"스택 트레이스: {ex.StackTrace}");

            // 내부 예외가 있는 경우 함께 기록
            if (ex.InnerException != null)
            {
                sb.AppendLine("내부 예외:");
                sb.AppendLine($"  유형: {ex.InnerException.GetType().Name}");
                sb.AppendLine($"  메시지: {ex.InnerException.Message}");
                sb.AppendLine($"  스택 트레이스: {ex.InnerException.StackTrace}");
            }

            Log(sb.ToString(), level, category);
        }

        /// <summary>
        /// 성능 측정 로그 기록
        /// </summary>
        public void LogPerformance(string operation, long elapsedMilliseconds, string category = "Performance")
        {
            Log($"성능 측정: {operation} - {elapsedMilliseconds}ms", LogLevel.Debug, category);
        }

        /// <summary>
        /// 데이터베이스 작업 로그 기록
        /// </summary>
        public void LogDatabaseOperation(string operation, int affectedRows, long elapsedMilliseconds, string category = "Database")
        {
            Log($"DB 작업: {operation} - {affectedRows}행 영향, 소요시간: {elapsedMilliseconds}ms", LogLevel.Debug, category);
        }

        /// <summary>
        /// API 호출 로그 기록
        /// </summary>
        public void LogApiCall(string apiName, string endpoint, int statusCode, long elapsedMilliseconds, string category = "API")
        {
            Log($"API 호출: {apiName} - {endpoint}, 상태 코드: {statusCode}, 소요시간: {elapsedMilliseconds}ms", LogLevel.Debug, category);
        }
        #endregion
    }
}
#endregion