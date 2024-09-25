using SimpleCopyOfCNPJ.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SimpleCopyOfCNPJ
{
    public partial class SimpleCopyOfCNPJ : Form
    {
        private string filePath;

        public SimpleCopyOfCNPJ()
        {
            InitializeComponent();
            this.TopMost = true;

            Button closeButton = new Button
            {
                Text = "❌",
                Location = new Point(this.ClientSize.Width - 35, 5),
                Size = new Size(28, 28),
                ForeColor = Color.White,
                Font = new Font("Arial", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            closeButton.Click += (s, e) => this.Close();

            Label infoLabel = new Label()
            {
                Text = "Importe um arquivo csv para carregar os CNPJs:",
                ForeColor = Color.White,
                Size = new Size(this.ClientSize.Width, 40),
                Location = new Point(0, 40),
                Font = new Font("Arial", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Button importButton = new Button
            {
                Text = "Importar CNPJs (CSV)",
                Location = new Point(25, 80),
                Size = new Size(160, 30),
                ForeColor = Color.White,
                Font = new Font("Arial", 8, FontStyle.Bold),
            };
            importButton.Click += ImportButton_Click;

            this.Controls.Add(closeButton);
            this.Controls.Add(infoLabel);
            this.Controls.Add(importButton);
        }

        private void SimpleCopyOfCNPJ_Load(object sender, EventArgs e)
        {
            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            int margin = 4;

            this.Size = new Size(210, screenHeight - 10);
            this.Location = new Point(screenWidth - this.Width - margin, screenHeight - this.Height - margin);

            // Tenta ler o arquivo salvo na pasta Documentos
            filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CNPJList.csv");
            if (File.Exists(filePath))
            {
                List<CompanyModel> companyList = ReadCsv(filePath);
                CreateCopyFields(companyList);
            }
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV files (*.csv)|*.csv";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string sourceFilePath = openFileDialog.FileName;
                    List<CompanyModel> companyList = ReadCsv(sourceFilePath);
                    CreateCopyFields(companyList);

                    // Salva o arquivo na pasta Documentos
                    File.Copy(sourceFilePath, filePath, true); // O terceiro parâmetro sobrescreve o arquivo existente
                }
            }
        }

        private List<CompanyModel> ReadCsv(string filePath)
        {
            var companyList = new List<CompanyModel>();
            var cnpjPattern = @"^\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}$";
            var delimiters = new[] { ',', ';' };

            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                    if (values.Length == 2)
                    {
                        var company = new CompanyModel
                        {
                            Name = values[0].Trim(),
                            CNPJ = values[1].Trim()
                        };

                        if (Regex.IsMatch(company.CNPJ, cnpjPattern))
                        {
                            companyList.Add(company);
                        }
                    }
                }
            }

            return companyList;
        }

        private void CreateCopyFields(List<CompanyModel> companyList)
        {
            int panelHeight = 34;
            int marginBetweenPanels = 4;
            int startY = 160;

            Label infoLabel = new Label()
            {
                Text = "Clique na empresa para copiar:",
                ForeColor = Color.White,
                Size = new Size(this.ClientSize.Width, 40),
                Location = new Point(0, 120),
                Font = new Font("Arial", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            this.Controls.Add(infoLabel);

            foreach (var company in companyList)
            {
                Panel panel = CreatePanel(startY, panelHeight, company.CNPJ);
                Label companyNameLabel = CreateLabel($"{company.Name}:", Color.White, 8, company.CNPJ);
                Label companyCNPJLabel = CreateLabel($"{company.CNPJ}", Color.White, 10, company.CNPJ, new Point(0, 14));

                panel.Controls.Add(companyNameLabel);
                panel.Controls.Add(companyCNPJLabel);

                this.Controls.Add(panel);

                startY += panelHeight + marginBetweenPanels;
            }
        }

        private Panel CreatePanel(int startY, int panelHeight, string textToCopy)
        {
            Panel panel = new Panel
            {
                Size = new Size(this.ClientSize.Width - 20, panelHeight),
                Location = new Point(10, startY),
                Cursor = Cursors.Hand,
            };

            panel.Paint += (sender, e) => DrawSeparator(panel, e);
            panel.Click += (sender, e) => CopyToClipboard(textToCopy);

            return panel;
        }

        private void DrawSeparator(Panel panel, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.Gray, 1))
            {
                e.Graphics.DrawLine(pen, 0, panel.Height - 1, panel.Width, panel.Height - 1);
            }
        }

        private Label CreateLabel(string text, Color foreColor, int fontSize, string textToCopy = null, Point? location = null)
        {
            Label label = new Label
            {
                Text = text,
                ForeColor = foreColor,
                Size = new Size(this.ClientSize.Width, fontSize + 4),
                Location = location ?? new Point(0, 0),
                Font = new Font("Arial", fontSize, FontStyle.Regular),
            };

            label.Click += (sender, e) => CopyToClipboard(textToCopy);

            return label;
        }

        private void CopyToClipboard(string text)
        {
            Clipboard.SetText(text);

            Label alertLabel = new Label
            {
                Text = $"CNPJ {text} copiado!",
                ForeColor = Color.White,
                BackColor = Color.Black,
                AutoSize = true,
                Padding = new Padding(6),
                Location = new Point((this.ClientSize.Width - 190) / 2, this.ClientSize.Height - 50)
            };

            this.Controls.Add(alertLabel);
            this.Controls.SetChildIndex(alertLabel, 10);

            alertLabel.Visible = true;

            Timer timer = new Timer
            {
                Interval = 1500
            };
            timer.Tick += (s, e) =>
            {
                alertLabel.Visible = false;
                this.Controls.Remove(alertLabel);
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }
    }
}
