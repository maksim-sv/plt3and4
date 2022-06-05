using System;
using System.Windows.Forms;
namespace plt3
{
    public partial class Form1 : Form
    {

        public void Error(string text = "", string caption = "Error", int start = 0, int end = 0)
        {
            TextBox.Focus();
            TextBox.Select(start, end);
            if (text == "") { text = "Стэк вызовов:\n" + Status.PrintErr(); }
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            PrintLexemesGrid();
        }
        public void PrintLexemesGrid()
        {
            System.Collections.Generic.Dictionary<string, string> buff = new System.Collections.Generic.Dictionary<string, string>();
            foreach (var a in Status.lexemes)
            {
                lexemesGrid.Rows.Add(a.value, a.type);
                buff[a.value] = a.type;
            }
            foreach (var a in buff)
            {
                if (a.Value == "id") idGrid.Rows.Add(a.Key);
                else if (a.Value == "const") ConstGrid.Rows.Add(a.Key);
            }
        }
        public void Reset()
        {
            Status.Reset();
            lexemesGrid.Rows.Clear(); lexemesGrid.Refresh();
            idGrid.Rows.Clear(); idGrid.Refresh();
            ConstGrid.Rows.Clear(); ConstGrid.Refresh();
        }
        public void Ok(string text = "Конструкция принята", string caption = "Success")
        {
            PrintLexemesGrid();
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Reset();
            string str = TextBox.Text;
            Check3 plt3 = new Check3(str);
            int result=0;
            state prevState = state.S;
            int counterState = 0;
            while (result==0)
            {
                result = plt3.Step();
                if (prevState == plt3.currentState) counterState++;
                else { prevState = plt3.currentState; }
                if (counterState > 1000) { result = -1; Status.AddErrorRecord("бесконечный цикл)"); }
                if (plt3.index>str.Length) { result = -1; Status.AddErrorRecord("конструкция не закончена"); }
            }
            if (result==-1)
            {
                Error();
            }
            else
            {
                Ok();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Reset();
            string str = TextBox.Text;
            int index = 0;
            string left, right;
            //TextBox.Select(50, 80);
            try
            {

                //from start to if
                if (step("if ", "конструкция должна начинаться с if", String.IsNullOrWhiteSpace, "конструкция должна начинаться с if") == false) return;
                //from if to then
                if (step(" then ", "после if должен следовать then", Checker.IsLogicExpression, "неправильное логическое выражение") == false) return;


                while (Checker.GetLeftRigtParts(str, out left, out right, " elseif ") == true)
                {
                    //from then to elseif
                    if (Checker.IsOperators(left) == false) { Error(caption: "неправильные операторы", start: index, end: left.Length + 1); return; }
                    index += left.Length + " elseif ".Length;
                    str = right;
                    Status.AddLexeme("elseif", "elseif");
                    //from elseif to then
                    if (step(" then ", "после elseif должен следовать then", Checker.IsLogicExpression, "неправильное логическое выражение") == false) return;
                }


                //from then to else
                if (step(" else ", "после then должен следовать else или elseif", Checker.IsOperators, "неправильные операторы") == false) return;

                //from if to then
                if (Checker.GetLeftRigtParts(str, out left, out right, " end") == false)
                { Error("конструкция должна заканчиваться end"); return; }
                if (Checker.IsOperators(left) == false) { Error(caption: "неправильные операторы", start: index, end: left.Length + 1); return; }
                index += left.Length + " end".Length;
                if (String.IsNullOrWhiteSpace(right) == false) { Error(caption: "конструкция должна заканчиваться end", start: index + 1, end: right.Length); return; }
                Status.AddLexeme("end", "end");

                bool step(string separator, string ErrSepNotFound, Func<string, bool> checkFunc, string checkErr)
                {
                    if (Checker.GetLeftRigtParts(str, out left, out right, separator) == false)
                    { Error(ErrSepNotFound); return false; }
                    if (checkFunc(left) == false) { Error(caption: checkErr, start: index, end: left.Length + 1); return false; }
                    index += left.Length + separator.Length;
                    str = right;
                    Status.AddLexeme(separator, separator);
                    return true;
                }
            }
            catch (Exception ex) { Error(ex.Message); }
            Ok();
        }
    }


}
