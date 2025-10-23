using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {
        // randomizer라는 Random 객체를 만들어 난수를 생성합니다.
        Random randomizer = new Random();

        // 이러한 정수 변수는 덧셈 문제에 대한 숫자를 저장합니다.
        int addend1;
        int addend2;

        // 이러한 정수 변수는 뺄셈 문제에 대한 숫자를 저장합니다.
        int minuend;
        int subtrahend;

        // 이러한 정수 변수는 곱셈 문제에 대한 숫자를 저장합니다.
        int multiplicand;
        int multiplier;

        // 이러한 정수 변수는 나눗셈 문제에 대한 숫자를 저장합니다.
        int dividend;
        int divisor;

        // 이 정수 변수는 남은 시간을 추적합니다.
        int timeLeft;

        public Form1()
        {
            InitializeComponent();
        }

        ///<summary>
        /// 답변을 확인하여 사용자가 모든 것을 올바르게 이해했는지 확인합니다.
        ///</summary>
        ///<returns>답이 맞으면 True, 그렇지 않으면 false입니다.</returns>
        private bool CheckTheAnswer()
        {
            if ((addend1 + addend2 == sum.Value)
                && (minuend - subtrahend == difference.Value)
                && (multiplicand * multiplier == product.Value)
                && (dividend / divisor == quotient.Value))
                return true;
            else
                return false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (CheckTheAnswer())
            {
                timer1.Stop();
                MessageBox.Show("모든 답을 맞히셨어요!", "축하합니다!");
                startButton.Enabled = true;
            }
            else if (timeLeft > 0)
            {
                timeLeft = timeLeft - 1;
                timeLabel.Text = timeLeft + " 초";
            }
            else
            {
                timer1.Stop();
                timeLabel.Text = "시간 초과!";
                MessageBox.Show("시간 내에 끝내지 못했어요.", "미안해요!");
                sum.Value = addend1 + addend2;
                difference.Value = minuend - subtrahend;
                product.Value = multiplicand * multiplier;
                quotient.Value = dividend / divisor;
                startButton.Enabled = true;
            }
        }
        /// <summary>
        ///   모든 문제를 채우고 타이머를 시작하여 퀴즈를 시작합니다.  
        /// </summary>
        public void StartTheQuiz()
        {
            // 덧셈 문제를 채웁니다.
            // 더할 두 개의 난수를 생성합니다. 변수 'addend1' 및 'addend2'에 값을 저장합니다.
            addend1 = randomizer.Next(51);
            addend2 = randomizer.Next(51);


            // 임의로 생성된 두 숫자를 문자열로 변환하여 레이블 컨트롤에 표시할 수 있도록 합니다.
            plusLeftLabel.Text = addend1.ToString();
            plusRightLabel.Text = addend2.ToString();

            // 'sum'은 NumericUpDown 컨트롤의 이름입니다.
            // 이 단계에서는 값을 추가하기 전에 값이 0인지 확인합니다.
            sum.Value = 0;

            // 뺄셈 문제를 채웁니다.
            minuend = randomizer.Next(1, 101);
            subtrahend = randomizer.Next(1, minuend);
            minusLeftLabel.Text = minuend.ToString();
            minusRightLabel.Text = subtrahend.ToString();
            difference.Value = 0;

            // 곱셈 문제를 채웁니다.
            multiplicand = randomizer.Next(2, 11);
            multiplier = randomizer.Next(2, 11);
            timesLeftLabel.Text = multiplicand.ToString();
            timesRightLabel.Text = multiplier.ToString();
            product.Value = 0; ;

            // 나눗셈 문제를 채웁니다.
            divisor = randomizer.Next(2, 11);
            int temporaryQuotient = randomizer.Next(2, 11);
            dividend = divisor * temporaryQuotient;
            dividedLeftLabel.Text = dividend.ToString();
            dividedRightLabel.Text = divisor.ToString();
            quotient.Value = 0;

            timeLeft = 100;
            timeLabel.Text = "100초";
            timer1.Start();
        }
        private void startButton_Click(object sender, EventArgs e)
        {
            StartTheQuiz();
            startButton.Enabled = false;
            
        }

        private void answer_Enter(object sender, EventArgs e)
        {
            NumericUpDown answerBox = sender as NumericUpDown;

            if (answerBox != null)
            {
                // NumericUpDown 컨트롤에서 전체 답을 합니다.
                int lengthOfAnswer = answerBox.Value.ToString().Length;
                answerBox.Select(0, lengthOfAnswer);
            }
        }
    }
}
