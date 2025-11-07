using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Globalization; // CultureInfo를 사용하기 위해 추가

namespace FileAnalyzerApp
{
    public partial class Form1 : Form
    {
        // 데이터베이스 연결 문자열
        private const string ConnectionString =
            "Server=127.0.0.1;Port=3306;Database=file_db;Uid=file_data;Pwd=1234;SslMode=Disabled;";

        public Form1()
        {
            InitializeComponent();

            // cmbFilePath라는 이름의 ComboBox 컨트롤의 드롭다운 스타일을 ComboBoxStyle.DropDown으로 설정
            cmbFilePath.DropDownStyle = ComboBoxStyle.DropDown;
        }

        private async void btnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "모든 파일 (*.*)|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string path = dialog.FileName;

                // UI 업데이트 및 초기화
                // 동일한 파일 경로가 드롭다운 목록에 두 번 이상 추가되지 않도록 합니다.
                if (!cmbFilePath.Items.Contains(path))
                    cmbFilePath.Items.Add(path);

                cmbFilePath.Text = path;
                progressBar1.Value = 0;
                richTextBox1.Clear();

                long fileSize = new FileInfo(path).Length;
                long totalBytesRead = 0;
                bool isDbSaveSuccessful = true;

                try
                {
                    // using문을 쓰는 이유: IDisposal 인터페이스를 구현하는 객체를 사용 후 메모리에서 비우기 위해서.
                    // 즉, 자원 반납을 확실히 해서 메모리 누수를 막기 위함이다. 

                    // 특정 경로의 파일을 읽기 전용으로 열고, 열려 있는 동안 다른 프로세스가 읽을 수 있도록 허용
                    // using문은 스트림을 사용한 후 자동으로 Dispose()를 호출하여 파일을 안전하게 닫아 자원을 해제해 줍니다.
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))

                    // System.IO 네임스페이스의 StreamReader 클래스를 사용하여 FileStream에서 데이터를 읽음
                    // using문은 StreamReader 객체를 안전하고 효율적으로 사용하고 자동으로 닫는 역할을 합니다.
                    using (StreamReader reader = new StreamReader(fs, Encoding.Default))
                    {
                        string line;
                        // 비동기적으로 텍스트 파일이나 스트림을 한 줄씩 읽어오는 일반적인 패턴
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            // 빈 줄 또는 공백만 있는 줄은 건너뜁니다.
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            // UI에 라인 출력
                            richTextBox1.AppendText(line + Environment.NewLine);

                            // 데이터 파싱 및 DB 저장 시도
                            try
                            {
                                // await는 프로그래밍 언에서 비동기 작업이 완료될 때까지 기다리는 데 사용되는 키워드
                                await SaveParsedLineToMariaDBAsync(path, line);
                            }
                            catch (Exception dbEx)
                            {
                                // DB 저장 중 오류 발생 시 상세 메시지 표시
                                isDbSaveSuccessful = false;
                                MessageBox.Show($"DB 처리 중 최종 오류 발생: {dbEx.Message}", "치명적 저장 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                // 오류 발생 시 파일 읽기 중단 
                                break;
                            }

                            // 진행률 업데이트

                            // 파일 읽기가 완료된 시점 또는 특정 시점까지 읽은 총 바이트 수를 추적할 때 사용
                            totalBytesRead = fs.Position;
                            int progress = (int)((double)totalBytesRead / fileSize * 100);
                            progressBar1.Value = Math.Min(progress, 100);

                            // 비동기 프로그래밍에서 현재 실행 중인 비동기 작업을 일시 중단(잠시 멈춤)하는 역할을 합니다.
                            await Task.Delay(1);
                        }

                        progressBar1.Value = 100;
                    }

                    if (isDbSaveSuccessful)
                    {
                        MessageBox.Show("파일 내용 출력 및 DB 저장이 완료되었습니다.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일 처리 중 오류 발생: {ex.Message}", "파일 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// CSV 라인을 파싱하여 MariaDB에 저장하고, 상세 오류를 보고하는 메서드
        /// </summary>
        private async Task SaveParsedLineToMariaDBAsync(string sourceFile, string dataLine)
        {
            // 1. 큰따옴표 제거 및 쉼표로 분할 (파싱 1단계)
            string cleanedLine = dataLine.Replace("\"", "").Trim();
            // 쉼표 주변의 공백을 제거하여 분할 안정화 (e.g., "A , B" -> "A,B")
            cleanedLine = System.Text.RegularExpressions.Regex.Replace(cleanedLine, @"\s*,\s*", ",");

            // 쉼표로 분할. 필드는 3개 또는 4개가 나옵니다.
            // 예시: {"AL-Cr 24805B ", "25-10-27 16:13 ", "46.7"}
            string[] chunks = cleanedLine.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            // 만약 3개 미만이면 유효하지 않은 데이터로 판단
            if (chunks.Length < 3) return;

            // 2첫 번째 덩어리(chunk[0])를 공백으로 다시 분리하여 col1과 col2 추출 (파싱 2단계)
            string firstChunk = chunks[0].Trim(); // 예시: "AL-Cr 24805B"

            // 문자열을 특정 구분자를 기준으로 나누어 문자열 배열로 만듬
            string[] parts = firstChunk.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // col1과 col2의 추출 성공 여부 확인
            if (parts.Length < 2) return;

            // 최종 4개 필드 추출 및 정리
            string col1 = parts[0].Trim();        // "AL-Cr"
            string col2 = parts[1].Trim();        // "24805B" 또는 "B"
            string timestamp = chunks[1].Trim();  // "25-10-27 16:13"

            double value = 0.0;
            if (chunks.Length > 2)
            {
                // 측정값 파싱
                double.TryParse(chunks[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            }

            // 이 시점에서 col1, col2, timestamp, value 4개의 유효한 데이터가 확보되었습니다.

            string query = "INSERT INTO file_data_chunks (col1, col2, timestamp_data, measurement_value) VALUES (@Col1, @Col2, @Timestamp, @Value)";

            // MariaDB 또는 MySQL 데이터베이스 트랜잭션을 처리하기 위한 변수를 선언하는 코드입니다.
            MySqlTransaction transaction = null;

            try
            {
                // 시스템 리소스를 효율적으로 관리하고 잠재적인 오류를 방지하고, 
                // using 블록 내에서 데이터베이스 작업 중 에러가 발생하더라도, using 문은 무조건 리소스를 정리하고 연결을 닫아줍니다.
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    // 데이터베이스 연결 객체를 비동기적으로 열림.
                    await connection.OpenAsync();
                    transaction = await connection.BeginTransactionAsync();

                    // 데이터베이스에서 실행할 명령(Command)을 생성하고 초기화
                    // using 문을 사용하여 리소스 관리를 자동화 
                    using (var command = new MySqlCommand(query, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@Col1", col1);
                        command.Parameters.AddWithValue("@Col2", col2);
                        command.Parameters.AddWithValue("@Timestamp", timestamp);
                        command.Parameters.AddWithValue("@Value", value);

                        // 데이터베이스 명령을 비동기적으로 실행하고 그 결과를 처리
                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected == 0)
                        {
                            await transaction.RollbackAsync();
                            throw new InvalidOperationException("DB 명령 실행 결과 0개의 행에 영향이 있었습니다. 저장 실패.");
                        }
                    }
                    // 비동기 프로그래밍에서 데이터베이스 트랜잭션을 취소(되돌리기)할 때 사용
                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                // 트랜잭션 처리 중 오류가 발생했을 때 주로 사용
                if (transaction != null)
                {
                    // 데이터베이스 트랜잭션에서 오류가 발생했을 때 비동기적으로 롤백을 시도하고,
                    // 만약 롤백 시도 자체에서 또 다른 오류가 발생해도 무시하고 넘어가도록 처리하는 예외 처리 패턴
                    try { await transaction.RollbackAsync(); } catch {  }
                }
                // MySqlException과 일반 Exception 모두 상위로 다시 던져 상세 오류를 표시합니다.
                throw new Exception($"[파싱 또는 SQL 실행 오류] 메시지: {ex.Message}", ex);
            }
        }
    }
}