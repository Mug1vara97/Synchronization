using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace СП_ПРактика_4
{
    public partial class Form1 : Form
    {
        Semaphore s;
        List<Thread> threads1;
        List<Thread> threads2;
        List<Thread> threads3;
        int counter;

        public Form1()
        {
            InitializeComponent();
            s = new Semaphore(2, 2);
            threads1 = new List<Thread>();
            threads2 = new List<Thread>();
            threads3 = new List<Thread>();
            counter = 0;
            numericUpDown1.Value = 2;
        }

        private void UpdateListBox(object listBox, object tmp, bool isAdded, int? index = null)
        {
            ListBox list = listBox as ListBox;
            if (list.InvokeRequired)
            {
                list.Invoke(new Action<object, object, bool, int?>(UpdateListBox), listBox, tmp, isAdded, index);
            }
            else
            {
                if (isAdded)
                {
                    list.Items.Add(tmp);
                }
                else if (index.HasValue && index.Value >= 0 && index.Value < list.Items.Count)
                {
                    list.Items[index.Value] = tmp;
                }
                else if (tmp is int idx && idx >= 0 && idx < list.Items.Count)
                {
                    list.Items.RemoveAt(idx);
                }
            }
        }

        private void GoToWork()
        {
            while (counter < numericUpDown1.Value && threads2.Count > 0)
            {
                Thread t = threads2[0];
                threads2.RemoveAt(0);
                UpdateListBox(listBox2, 0, false);
                threads3.Add(t);
                UpdateListBox(listBox3, $"{t.ManagedThreadId} --> выполняется: 0", true);
                counter++;
                t.Start();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(ThreadMethod);
            threads1.Add(t);
            UpdateListBox(listBox1, $"{t.ManagedThreadId} --> создан", true);
        }

        private void ThreadMethod()
        {
            if (!s.WaitOne())
            {
                return;
            }

            int localCounter = 0;
            int indexInListBox = threads3.Count - 1; 

            try
            {
                while (true)
                {
                    localCounter++;
                    UpdateListBox(listBox3, $"{Thread.CurrentThread.ManagedThreadId} --> выполняется: {localCounter}", false, indexInListBox);
                    Thread.Sleep(1000);
                }
            }
            finally
            {
                s.Release();
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            GoToWork();
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;

            Thread t = threads1[listBox1.SelectedIndex];
            threads2.Add(t);
            UpdateListBox(listBox2, $"{t.ManagedThreadId} --> ожидает", true);

            threads1.RemoveAt(listBox1.SelectedIndex);
            UpdateListBox(listBox1, listBox1.SelectedIndex, false);
            GoToWork();
        }

        private void listBox3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox3.SelectedIndex == -1)
                return;

            s.Release();
            Thread t = threads3[listBox3.SelectedIndex];
            threads3.RemoveAt(listBox3.SelectedIndex);
            UpdateListBox(listBox3, listBox3.SelectedIndex, false);
            counter--;

            GoToWork();
        }
    }
}