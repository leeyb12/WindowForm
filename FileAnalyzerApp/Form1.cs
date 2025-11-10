using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace FileAnalyzerApp
{
    // 데이터 구조체를 정의하여 파싱된 데이터를 명확하게 관리합니다.
    public struct FileData
    {
        public string Col1 { get; set; }
        public string Col2 { get; set; }
        public string Timestamp { get; set; }
        public double Value { get; set; }
    }

    public partial class Form1 : Form
    {
        // 데이터베이스 연결 문자열
        private const string ConnectionString =
            "Server=127.0.0.1;Port=3306;Database=file_db;Uid=file_data;Pwd=1234;SslMode=Disabled;";

        // UI 업데이트 빈도 제어를 위한 변수
        private DateTime _lastUiUpdateTime = DateTime.MinValue;
        private const int UiUpdateIntervalMs = 500;

        public Form1()
        {
            InitializeComponent();

            // 파일 경로 콤보박수에 사용자가 직접 경로를 입력할 수 있도록 허용하고, 동시에 준비된 경로 목록도 제공한다
            cmbFilePath.DropDownStyle = ComboBoxStyle.DropDown;
        }

        // --- 파일 열기 및 전체 데이터 처리 메서드 ---
        private async void btnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "모든 파일 (*.*)|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string path = dialog.FileName;

                // UI 업데이트 및 초기화
                if (!cmbFilePath.Items.Contains(path))
                    cmbFilePath.Items.Add(path);

                cmbFilePath.Text = path;
                progressBar1.Value = 0;
                richTextBox1.Clear();

                // 파싱된 데이터를 저장할 리스트 (DB 배치 삽입을 위한 준비)
                List<FileData> parsedDataList = new List<FileData>();

                // UI에 출력할 텍스트를 모으는 빌더 (UI 부하 감소)
                StringBuilder displayContent = new StringBuilder();

                long fileSize = new FileInfo(path).Length;  // 지정된 위치에 있는 파일의 크기를 계산하고 path이를 .long이라는 이름의 정수로 저장
                long totalBytesRead = 0;  // long 호출된 정수 변수를 totalBytesRead()으로 초기화
                bool isParsingSuccessful = true;  // to 라는 bool 변수를 초기화

                try
                {
                    // 텍스트 파일 읽기 (비동기 I/O, 리소스 자동 해제)

                    // 지정된 경로의 파일을 읽기 전용으로 열고, 이 파일에 대한 스트림 기반의 접근을 설정합니다.
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))

                    // 이전 단계에서 생성된 바이트 스트림(fs)을 사용하여 파일을 텍스트로 읽기 위한 준비를 하며, 이때 시스템의 기본(ANSI) 인코딩을 사용하도록 지정합니다.
                    using (StreamReader reader = new StreamReader(fs, Encoding.Default))
                    {
                        string line;

                        // 비동기적으로 텍스트 파일을 읽거나 스트림에서 한 줄씩 데이터를 가져오는 데 사용
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            // UI 출력 텍스트에 추가
                            displayContent.AppendLine(line);

                            // 데이터 파싱 시도 (업데이트된 TryParseLine 사용)
                            if (TryParseLine(line, out FileData data))
                            {
                                parsedDataList.Add(data);
                            }
                            else
                            {
                                // 파싱 실패 시 플래그 설정
                                isParsingSuccessful = false;
                            }

                            // 진행률 업데이트 제어
                            totalBytesRead = fs.Position;
                            int progress = (int)((double)totalBytesRead / fileSize * 100);

                            // 500ms마다 또는 100% 근처에서만 UI 업데이트
                            if (progress >= 100 || (DateTime.Now - _lastUiUpdateTime).TotalMilliseconds > UiUpdateIntervalMs)
                            {
                                progressBar1.Value = Math.Min(progress, 100);
                                Application.DoEvents(); // UI 갱신 강제
                                _lastUiUpdateTime = DateTime.Now;
                            }
                        }

                        progressBar1.Value = 100;
                    }

                    // 파일 읽기가 완료된 후, RichTextBox에 한 번에 내용을 출력 (UI 성능 향상)
                    richTextBox1.Text = displayContent.ToString();

                    // DB 배치 저장 시도 (성능 최적화)
                    if (parsedDataList.Count > 0)
                    {
                        await SaveBatchToMariaDBAsync(parsedDataList);
                        MessageBox.Show($"파일 내용 출력 완료. 총 {parsedDataList.Count}개의 데이터를 DB에 저장 완료.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("파일에서 유효한 데이터를 찾을 수 없어 DB에 저장하지 않았습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    // !isParsingSuccessful:프로그래밍 맥락에서 사용되는 메서드 또는 속성으로, 파싱 작업이 성공했는지 여부를 true 또는 false로 나타나고,
                    // 데이터 파싱, 시간 파싱 또는 특정 명령의 실행 여부를 확인하는 데 사용
                    if (!isParsingSuccessful)
                    {
                        MessageBox.Show("일부 라인 파싱에 실패했지만, 유효한 데이터는 DB에 저장했습니다.", "파싱 경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일 처리 중 오류 발생: {ex.Message}", "치명적 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // --- 데이터 파싱 메서드 (col2 공백 포함 지원) ---
        /// <summary>
        /// CSV 라인을 파싱하여 FileData 구조체로 반환하는 메서드 (모든 복합적인 col2 형식 지원)
        /// </summary>
        private bool TryParseLine(string dataLine, out FileData result)
        {
            result = new FileData();

            // 큰따옴표 제거 및 쉼표로 분할
            string cleanedLine = dataLine.Replace("\"", "").Trim();
            // 정규식을 사용해 쉼표 주변의 공백을 제거하여 분할 안정화
            cleanedLine = Regex.Replace(cleanedLine, @"\s*,\s*", ",");

            // 쉼표로 분할. 필드는 3개가 나옵니다.
            string[] chunks = cleanedLine.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (chunks.Length < 3) return false;

            // 첫 번째 덩어리(chunk[0])를 공백으로 다시 분리하여 col1과 col2 추출
            string firstChunk = chunks[0].Trim();
            // 공백을 기준으로 분리하여 모든 서브 파트를 얻습니다.
            string[] parts = firstChunk.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // parts는 최소 2개의 요소(col1 + col2의 첫 번째 부분)를 가져야 합니다.
            if (parts.Length < 2) return false;

            // col1: parts의 첫 번째 요소
            result.Col1 = parts[0].Trim();

            // col2: parts의 두 번째 요소부터 끝까지를 다시 공백으로 연결합니다.
            // string.Join(" ", parts, 1, parts.Length - 1)는 parts[1]부터 끝까지의 요소를 공백으로 연결합니다.
            result.Col2 = string.Join(" ", parts, 1, parts.Length - 1).Trim();

            // Timestamp 추출
            result.Timestamp = chunks[1].Trim();

            // 측정값 파싱 (Invariant 문화권을 사용하여 소수점 처리를 통일)
            double value = 0.0;
            if (chunks.Length > 2)
            {
                // chunks[2]의 내용을 문화권에 관계없이 double 값으로 변환하려고 시도합니다.
                // 이 시도가 성공하면 value 변수에 결과가 저장되고 true가 반환되며, 실패하면 false가 반환됩니다. 
                double.TryParse(chunks[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            }

            // 특정 변수의 값을 다른 변수의 속성에 할당하는 작업을 수행
            result.Value = value;

            return true;
        }

        // --- DB 배치 삽입 메서드 (성능 최적화) ---
        /// <summary>
        /// 파싱된 데이터 리스트를 받아 MariaDB에 배치(Batch)로 저장하는 메서드
        /// </summary>
        private async Task SaveBatchToMariaDBAsync(List<FileData> dataList)
        {
            // 함수나 메서드의 시작 부분에서 입력 데이터의 유효성을 검사하는 데 사용되는 조건문
            if (dataList == null || dataList.Count == 0) return;

            // INSERT 쿼리를 생성 (VALUES (@p1), (@p2), ...)
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append("INSERT INTO file_data_chunks (col1, col2, timestamp_data, measurement_value) VALUES ");

            List<MySqlParameter> parameters = new List<MySqlParameter>();

            for (int i = 0; i < dataList.Count; i++)
            {
                var data = dataList[i];
                // 쿼리 문자열에 삽입할 파라미터 Placeholder 추가
                queryBuilder.Append($"(@Col1_{i}, @Col2_{i}, @Timestamp_{i}, @Value_{i})");
                if (i < dataList.Count - 1)
                {
                    queryBuilder.Append(", ");
                }

                // 파라미터 객체 생성 및 리스트에 추가
                parameters.Add(new MySqlParameter($"@Col1_{i}", data.Col1));
                // col2가 길어질 수 있으므로, DB 컬럼도 VARCHAR(100) 이상으로 설정하는 것이 좋습니다.
                parameters.Add(new MySqlParameter($"@Col2_{i}", data.Col2));
                parameters.Add(new MySqlParameter($"@Timestamp_{i}", data.Timestamp));
                parameters.Add(new MySqlParameter($"@Value_{i}", data.Value));
            }

            MySqlTransaction transaction = null;

            try
            {
                // MySQL 데이터베이스에 안전하고 신뢰할 수 있는 방식으로 연결하는 과정을 시작합니다.
                // using 블록 내부의 코드는 이 connection 객체를 사용하여 쿼리를 실행하거나 데이터베이스 작업을 수행할 수 있으며,
                // 블록이 끝나면 연결은 자동으로 종료됩니다.
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    // 데이터베이스 연결(connection)에서 비동기적으로 새 데이터베이스 트랜잭션을 시작합니다.
                    transaction = await connection.BeginTransactionAsync();

                    // 특정 SQL 쿼리를 하나의 트랜잭션 단위로 실행되도록 설정합니다. 
                    // 여러 데이터베이스 작업이 모두 성공해야만 영구적으로 커밋되고, 하나라도 실패하면 전체가 롤백되도록 보장하여 
                    // 데이터 무결성을 유지하는 데 필수적입니다. 
                    using (var command = new MySqlCommand(queryBuilder.ToString(), connection, transaction))
                    {

                        // 데이터베이스와 상호 작용할 때, .NET 애플리케이션에서 매개변수 컬렉션을 SqlCommand 데이터베이스 명령 객체나
                        // 유사한 객체에 추가하는 데 사용
                        command.Parameters.AddRange(parameters.ToArray());

                        // 비동기적으로 배치 명령 실행

                        // ExecuteNonQueryAsync() 메서드는 데이터베이스의 상태를 변경하지만 데이터를 반환하지 않는 명령을 실행하며, 실행 결과로
                        // 영향을 받은 행의 수를 반환합니다.
                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected != dataList.Count)
                        {
                            // 데이터베이스 트랜잭션을 비동기적으로 롤백
                            await transaction.RollbackAsync();
                            throw new InvalidOperationException($"DB에 저장된 행 수 불일치. 시도: {dataList.Count}, 실제: {rowsAffected}");
                        }
                    }
                    
                    await transaction.CommitAsync();
                    // 비동기 프로그래밍에서, 일반적으로 데이터베이스 컨텍스트 내에서 트랜잭션을 완료하고 해당 트랜잭션 내에서 변경된 모든 내용을 
                    // 데이터베이스에 유지하는 데 사용
                }
            }
            catch (Exception ex)
            {
                // 오류 발생 시 롤백 시도
                if (transaction != null)
                {
                    try { await transaction.RollbackAsync(); } catch { }
                }
                throw new Exception($"[DB 배치 저장 실패] 메시지: {ex.Message}", ex);
            }
        }
    }
}