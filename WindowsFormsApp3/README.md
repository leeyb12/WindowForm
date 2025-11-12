# To-Do List (C# WinForms)

이 프로젝트는 **C# Windows Forms**로 제작한 간단한 **할 일(To-Do List)** 관리 프로그램입니다. 
사용자는 할 일을 추가, 수정, 삭제하고, 파일에 저장 및 불러올 수 있습니다. 

---

## 1 주요 기능 요약
| 기능 | 설명 |
| ------ | ------ |
| 추가(Add) | 새 할 일을 목록에 추가 |
| 수정(Edit) | 선택한 항목의 내용을 수정 |
| 삭제(Delete) | 체크된 항목 삭제 |
| 저장(Save) | todos.txt 파일로 저장 |
| 불러오기(Load) | 저장된 목록 불러오기 |

---

## 2 코드 구조 개요

### 1. 폼 로드 시 설정 (`Form1_Load`)

```csharp
private void Form1_Load(object sender, EventArgs e)
{
    lstTodos.View = View.Details;
    lstTodos.CheckBoxes = true;
    lstTodos.FullRowSelect = true;
    lstTodos.GridLines = true;

    lstTodos.Columns.Clear();
    lstTodos.Columns.Add("할 일", 400);

    lstTodos.ItemChecked += LstTodos_ItemChecked;
    lstTodos.SelectedIndexChanged += LstTodos_SelectedIndexChanged;
}
```
설명:
폼이 실행될 때 `ListView`의 모양과 동작을 설정합니다. 
- `CheckBoxes = true` : 체크박스 표시
- `FullRowSelect = true` : 행 전체 선택 가능
- `GridLines = true` : 표의 선 표시
- `ItemChecked`, `SelectedIndexChanged` : 이벤트 연결

### 2. 항목 추가 (`btnAdd_Click`)

```csharp
private void btnAdd_Click(object sender, EventArgs e)
{
    string text = txtTodo.Text.ToString();

    if (string.IsNullOrWhiteSpace(txtTodo.Text))
    {
        MessageBox.Show("할 일을 입력해주세요!");
        return;
    }

    ListViewItem item = new ListViewItem(text);
    item.Checked = false;
    lstTodos.Items.Add(item);
    txtTodo.Clear();
}
```
설명:
- 입력창(`txtTodo`)의 내용을 가져와 항목 추가
- 공백이면 오류 메시지 출력
- 입력 후 `txtTodo.Clear()`로 입력창 초기화

### 3. 항목 수정 (`btnEdit_Click`)

```csharp
private void btnEdit_Click(object sender, EventArgs e)
{
    if (lstTodos.SelectedItems.Count == 0)
    {
        MessageBox.Show("수정할 항목을 선택하세요!");
        return;
    }

    string newText = txtTodo.Text.Trim();
    if (string.IsNullOrWhiteSpace(newText))
    {
        MessageBox.Show("수정할 내용을 입력하세요!");
        return;
    }

    lstTodos.SelectedItems[0].Text = newText;
    txtTodo.Clear();
}
```
설명:
- 선택된 항목이 없으면 알림 표시
- 텍스트 박스에 새 내용을 입력 후, 선택 항목의 텍스트를 수정

### 4. 항목 삭제 (`btnDelete_Click`)

```csharp
private void btnDelete_Click(object sender, EventArgs e)
{
    if (lstTodos.CheckedItems.Count > 0)
    {
        for (int i = lstTodos.CheckedItems.Count - 1; i >= 0; i--)
        {
            lstTodos.Items.Remove(lstTodos.CheckedItems[i]);
        }
    }
    else
    {
        MessageBox.Show("삭제할 항목을 선택하거나 체크하세요.",
                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
```
설명:
- 체크된 항목이 있으면 뒤에서부터 삭제
- 없을 경우 안내 메시지 표시

### 5. 체크 이벤트 (`LstTodos_ItemChecked`)

```csharp
private void LstTodos_ItemChecked(object sender, ItemCheckedEventArgs e)
{
    var item = e.Item;

    if (item.Checked)
    {
        item.ForeColor = Color.Gray;
        item.Font = new Font(item.Font, FontStyle.Strikeout);
    }
    else
    {
        item.ForeColor = Color.Black;
        item.Font = new Font(item.Font, FontStyle.Regular);
    }
}
```
설명:
- 체크된 항목은 <strong>회색 + 취소선(Strikeout)</strong> 표시
- 해제되면 원래대로 복구

### 6. 저장 기능 (`btnSave_Click`)

```csharp
private void btnSave_Click(object sender, EventArgs e)
{
    List<string> lines = new List<string>();

    foreach (ListViewItem item in lstTodos.Items)
    {
        lines.Add($"{item.Checked} | {item.Text}");
    }

    File.WriteAllLines(filePath, lines, Encoding.UTF8);
    MessageBox.Show("저장 완료!");
}
```
설명:
- `ListView`의 모든 항목을 문자열로 변환
- `"체크여부 | 내용"` 형식으로 `todos.txt`에 저장

### 7. 불러오기 기능 (`LoadTasks`)

```csharp
private void LoadTasks()
{
    if(!File.Exists(filePath))
    {
        MessageBox.Show("저장된 파일이 없습니다.");
        return;
    }

    try
    {
        lstTodos.Items.Clear();
        var lines = File.ReadAllLines(filePath, Encoding.UTF8);

        foreach (var line in lines)
        {
            var parts = line.Split('|');
            if (parts.Length == 2)
            {
                bool isChecked = bool.TryParse(parts[0].Trim(), out bool result) && result;
                string text = parts[1].Trim();

                ListViewItem item = new ListViewItem(text);
                item.Checked = isChecked;

                if (isChecked)
                {
                    item.ForeColor = Color.Gray;
                    item.Font = new Font(lstTodos.Font, FontStyle.Strikeout);
                }

                lstTodos.Items.Add(item);
            }
        }

        MessageBox.Show("불러오기 완료!");
    }
    catch (Exception ex)
    {
        MessageBox.Show("불러오기 중 오류:" + {ex.Message});
    }
}
```
